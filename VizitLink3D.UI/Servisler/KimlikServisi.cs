using VizitLink3D.UI.Models;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace VizitLink3D.UI.Servisler;

public class KimlikServisi(ApiIstemcisi api, IJSRuntime js)
{
    private const string TokenAnahtari = "vizitlink3d_token";

    public async Task<bool> GirisYapAsync(string kullaniciAdi, string sifre)
    {
        try
        {
            var yanit = await api.PostAsync<GirisYaniti>("api/kimlik/giris", new { KullaniciAdi = kullaniciAdi, Sifre = sifre });
            if (yanit?.BasariliMi == true && !string.IsNullOrEmpty(yanit.Veri?.Token))
            {
                await js.InvokeVoidAsync("localStorage.setItem", TokenAnahtari, yanit.Veri.Token);
                return true;
            }
        }
        catch { }
        return false;
    }

    public async Task CikisYapAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenAnahtari);
    }

    public async Task<string?> TokenGetirAsync()
    {
        return await js.InvokeAsync<string?>("localStorage.getItem", TokenAnahtari);
    }

    public async Task<bool> GirisliMiAsync()
    {
        var token = await TokenGetirAsync();
        return !string.IsNullOrEmpty(token) && !TokenSuresiGecmisMi(token);
    }

    private static bool TokenSuresiGecmisMi(string token)
    {
        try
        {
            var parcalar = token.Split('.');
            if (parcalar.Length != 3) return true;
            var govde = parcalar[1];
            var doldurmaSayisi = (4 - govde.Length % 4) % 4;
            govde += new string('=', doldurmaSayisi);
            var baytlar = Convert.FromBase64String(govde);
            var json = JsonSerializer.Deserialize<JsonElement>(baytlar);
            var exp = json.GetProperty("exp").GetInt64();
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp;
        }
        catch
        {
            return true;
        }
    }

    public async Task<KullaniciBilgi?> KullaniciBilgiGetirAsync()
    {
        try
        {
            var token = await TokenGetirAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var parcalar = token.Split('.');
            if (parcalar.Length != 3) return null;
            var govde = parcalar[1];
            var doldurmaSayisi = (4 - govde.Length % 4) % 4;
            govde += new string('=', doldurmaSayisi);
            var baytlar = Convert.FromBase64String(govde);
            var json = JsonSerializer.Deserialize<JsonElement>(baytlar);

            return new KullaniciBilgi
            {
                KullaniciAdi = json.TryGetProperty("KullaniciAdi", out var ka) ? ka.GetString() ?? string.Empty : string.Empty,
                AdSoyad = json.TryGetProperty("AdSoyad", out var ad) ? ad.GetString() ?? string.Empty : string.Empty,
                Rol = json.TryGetProperty("Rol", out var r) ? r.GetString() ?? string.Empty : string.Empty,
                Eposta = json.TryGetProperty("Eposta", out var e) ? e.GetString() ?? string.Empty : string.Empty,
                KullaniciId = json.TryGetProperty("sub", out var s) ? s.GetString() ?? string.Empty : string.Empty
            };
        }
        catch
        {
            return null;
        }
    }
}

public class KullaniciBilgi
{
    public string KullaniciAdi { get; set; } = "";
    public string AdSoyad { get; set; } = "";
    public string Rol { get; set; } = "";
    public string Eposta { get; set; } = "";
    public string KullaniciId { get; set; } = "";
}

public record GirisYaniti(string Token, string KullaniciAdi, string AdSoyad, string Rol, string Eposta);
