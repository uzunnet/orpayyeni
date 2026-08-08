using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace VizitLink3D.SuperAdmin.IServisler;

public class GirisServisi
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly SuperAdminAuthStateProvider _authState;

    public GirisServisi(IHttpClientFactory httpFactory, AuthenticationStateProvider authState)
    {
        _httpFactory = httpFactory;
        _authState = (SuperAdminAuthStateProvider)authState;
    }

    public async Task<bool> GirisYapAsync(string kullaniciAdi, string sifre)
    {
        var http = _httpFactory.CreateClient();
        http.BaseAddress = new Uri("http://localhost:5200");
        var yanit = await http.PostAsJsonAsync("/api/super-admin/kimlik/giris", new
        {
            KullaniciAdi = kullaniciAdi,
            Sifre = sifre
        });

        if (!yanit.IsSuccessStatusCode)
            return false;

        var sonuc = await yanit.Content.ReadFromJsonAsync<GirisYaniti>();
        if (sonuc is null || string.IsNullOrEmpty(sonuc.Token))
            return false;

        await _authState.TokenKaydetAsync(sonuc.Token);
        return true;
    }

    public async Task CikisYapAsync()
    {
        await _authState.CikisYapAsync();
    }

    private record GirisYaniti(string Token, string KullaniciAdi, string AdSoyad);
}

public class SuperAdminAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _mevcutKullanici = new(new ClaimsIdentity());
    private string? _token;

    public string? MevcutToken => _token;

    public async Task TokenKaydetAsync(string token)
    {
        _token = token;
        var kimlik = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        _mevcutKullanici = new ClaimsPrincipal(kimlik);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_mevcutKullanici)));
        await Task.CompletedTask;
    }

    public async Task CikisYapAsync()
    {
        _token = null;
        _mevcutKullanici = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_mevcutKullanici)));
        await Task.CompletedTask;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_mevcutKullanici));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var anahtarDegerler = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

        if (anahtarDegerler is null) yield break;

        foreach (var kvp in anahtarDegerler)
        {
            if (kvp.Value.ValueKind == JsonValueKind.String)
            {
                yield return new Claim(MapClaimType(kvp.Key), kvp.Value.GetString() ?? "");
            }
            else
            {
                yield return new Claim(MapClaimType(kvp.Key), kvp.Value.ToString());
            }
        }
    }

    private static string MapClaimType(string jsonKey) => jsonKey switch
    {
        "sub" => ClaimTypes.NameIdentifier,
        "unique_name" => ClaimTypes.Name,
        "role" => ClaimTypes.Role,
        _ => jsonKey
    };

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
    }
}
