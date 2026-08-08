using Microsoft.JSInterop;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Servisler;

/// <summary>
/// Coklu dil servisi — 8+ dil destekli.
/// SADECE statik JSON dosyalarindan yuklenir (hizli acilis, tarayici icin optimize).
/// API, DB, FusionCache veya AI ile senkronizasyon YAPMAZ.
/// Statik dosya yolu: wwwroot/i18n/{dil}.json veya firmalar/{slug}/i18n/{dil}.json
/// </summary>
public class DilServisi
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly HttpClient _yerelHttp;
    private Dictionary<string, string> _sozluk = [];
    private string _aktifDil = "tr";
    private List<DilBilgisi> _desteklenenDiller = [];

    public DilServisi(HttpClient http, IJSRuntime js, NavigationManager nav)
    {
        _http = http;
        _js = js;
        _yerelHttp = new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
    }

    public string AktifDil => _aktifDil;
    public IReadOnlyList<DilBilgisi> DesteklenenDiller => _desteklenenDiller.AsReadOnly();

    public event Action? DilDegisti;

    public async Task BaslatAsync(string varsayilanDil = "tr", bool kayitliTercihYoksaTarayiciDiliAlgila = true)
    {
        DilleriYukle();

        var kayitliDil = await _js.InvokeAsync<string?>("localStorage.getItem", "vizitlink3dil");
        var secilenDil = string.IsNullOrWhiteSpace(kayitliDil)
            ? varsayilanDil
            : kayitliDil;

        if (string.IsNullOrWhiteSpace(kayitliDil) && kayitliTercihYoksaTarayiciDiliAlgila)
        {
            var tarayiciDili = await TarayiciDiliniGetirAsync();
            secilenDil = TarayiciDiliEsle(tarayiciDili, varsayilanDil);
        }

        _aktifDil = DesteklenenDileIndirge(secilenDil, varsayilanDil);

        if (string.IsNullOrWhiteSpace(kayitliDil))
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "vizitlink3dil", _aktifDil);
        }

        await DilDosyasiniYukleAsync(_aktifDil);
        await HtmlDiliniAyarlaAsync();
        DilDegisti?.Invoke();
    }

    private async Task HtmlDiliniAyarlaAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", _aktifDil);
        }
        catch { }
    }

    private void DilleriYukle()
    {
        _desteklenenDiller = new List<DilBilgisi>
        {
            new() { Kod = "tr", Ad = "Turkce", Bayrak = "fi fi-tr" },
            new() { Kod = "en", Ad = "English", Bayrak = "fi fi-gb" },
            new() { Kod = "de", Ad = "Deutsch", Bayrak = "fi fi-de" },
            new() { Kod = "fr", Ad = "Français", Bayrak = "fi fi-fr" },
            new() { Kod = "ru", Ad = "Русский", Bayrak = "fi fi-ru" },
            new() { Kod = "ar", Ad = "العربية", Bayrak = "fi fi-sa" },
            new() { Kod = "es", Ad = "Español", Bayrak = "fi fi-es" },
            new() { Kod = "zh", Ad = "中文", Bayrak = "fi fi-cn" }
        };
    }

    public string T(string anahtar, string yedekMetin = "")
    {
        return _sozluk.TryGetValue(anahtar, out var deger) && !string.IsNullOrEmpty(deger) ? deger : yedekMetin;
    }

    public async Task DilDegistirAsync(string dil)
    {
        _aktifDil = DesteklenenDileIndirge(dil, "tr");
        await _js.InvokeVoidAsync("localStorage.setItem", "vizitlink3dil", _aktifDil);
        await DilDosyasiniYukleAsync(_aktifDil);
        await HtmlDiliniAyarlaAsync();
        DilDegisti?.Invoke();
    }

    private async Task<string?> TarayiciDiliniGetirAsync()
    {
        try
        {
            return await _js.InvokeAsync<string?>("vizitlink3dDil.tercihDiliniGetir");
        }
        catch
        {
            return null;
        }
    }

    private string TarayiciDiliEsle(string? tarayiciDili, string varsayilanDil)
    {
        if (string.IsNullOrWhiteSpace(tarayiciDili))
        {
            return varsayilanDil;
        }

        var normalizeDil = tarayiciDili.Trim().ToLowerInvariant();
        if (normalizeDil.StartsWith("tr", StringComparison.Ordinal))
        {
            return "tr";
        }

        if (normalizeDil.StartsWith("en", StringComparison.Ordinal))
        {
            return "en";
        }

        return _desteklenenDiller.Any(d => d.Kod.Equals("en", StringComparison.OrdinalIgnoreCase))
            ? "en"
            : varsayilanDil;
    }

    private string DesteklenenDileIndirge(string? dil, string varsayilanDil)
    {
        if (string.IsNullOrWhiteSpace(dil))
        {
            return varsayilanDil;
        }

        var normalizeDil = dil.Trim().ToLowerInvariant();
        var ikiHarfliKod = normalizeDil.Split('-', '_')[0];

        var birebir = _desteklenenDiller.FirstOrDefault(d => d.Kod.Equals(normalizeDil, StringComparison.OrdinalIgnoreCase));
        if (birebir is not null)
        {
            return birebir.Kod.ToLowerInvariant();
        }

        var kisaltilmis = _desteklenenDiller.FirstOrDefault(d => d.Kod.Equals(ikiHarfliKod, StringComparison.OrdinalIgnoreCase));
        if (kisaltilmis is not null)
        {
            return kisaltilmis.Kod.ToLowerInvariant();
        }

        return varsayilanDil.ToLowerInvariant();
    }

    /// <summary>
    /// SADECE statik JSON dosyasindan sozluk yukler.
    /// Once wwwroot/i18n/{dil}.json denenir, bulunamazsa bos sozluk ile devam edilir.
    /// API, DB veya AI ile senkronizasyon YAPILMAZ.
    /// </summary>
    private async Task DilDosyasiniYukleAsync(string dil)
    {
        try
        {
            var sozluk = await _yerelHttp.GetFromJsonAsync<Dictionary<string, string>>($"i18n/{dil}.json");
            if (sozluk is not null && sozluk.Count > 0)
            {
                _sozluk = sozluk;
            }
        }
        catch { _sozluk = []; }
    }

    /// <summary>
    /// Tek anahtar AI cevirisi — admin paneli tarafindan kullanilir.
    /// Sunucu tarafindaki api/ai/cevir endpoint'ine istek atar.
    /// </summary>
    public async Task<string?> AICeviriAlAsync(string anahtar, string varsayilanMetin, string hedefDil = "en", string? kaynakDil = null)
    {
        if (string.IsNullOrWhiteSpace(varsayilanMetin)) return null;
        try
        {
            var yanit = await _http.PostAsJsonAsync("api/ai/cevir", new
            {
                Metin = varsayilanMetin,
                HedefDil = hedefDil,
                KaynakDil = kaynakDil
            });
            yanit.EnsureSuccessStatusCode();
            var cevap = await yanit.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            if (cevap.TryGetProperty("basariliMi", out var bm) && bm.GetBoolean() &&
                cevap.TryGetProperty("veri", out var veri) && veri.ValueKind != System.Text.Json.JsonValueKind.Null)
            {
                return veri.GetString();
            }
        }
        catch { }
        return null;
    }

    public class DilBilgisi
    {
        public int Id { get; set; }
        public string Kod { get; set; } = "";
        public string Ad { get; set; } = "";
        public string? Bayrak { get; set; }
    }
}
