using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Servisler;

/// <summary>
/// Coklu dil servisi — 8+ dil destekli.
/// ONCE statik JSON dosyalarindan yuklenir (hizli acilis, tarayici icin optimize).
/// Sonra arka planda API (DB + FusionCache) ile senkronize olur (dinamik icerikler).
/// Eksik ceviriler AI ile tamamlanir.
/// Desteklenen diller API'den dinamik olarak alinir.
/// </summary>
public class DilServisi
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly HttpClient _yerelHttp;
    private Dictionary<string, string> _sozluk = [];
    private string _aktifDil = "tr";
    private List<DilBilgisi> _desteklenenDiller = [];
    private HashSet<string> _aiCeviriBekleyen = [];

    public DilServisi(HttpClient http, IJSRuntime js, NavigationManager nav)
    {
        _http = http;
        _js = js;
        _yerelHttp = new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
    }

    public string AktifDil => _aktifDil;
    public IReadOnlyList<DilBilgisi> DesteklenenDiller => _desteklenenDiller.AsReadOnly();

    public event Action? DilDegisti;
    public event Action? SozlukGuncellendi;

    public async Task BaslatAsync(string varsayilanDil = "tr", bool kayitliTercihYoksaTarayiciDiliAlgila = true)
    {
        await DilleriYukleAsync();

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
        _ = EksikCevirileriAIileTamamlaAsync();
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

    private async Task DilleriYukleAsync()
    {
        try
        {
            var cevap = await _http.GetFromJsonAsync<CevapListeDto<DilBilgisi>>("api/dil/desteklenen");
            if (cevap?.BasariliMi == true && cevap.Veri?.Count > 0)
                _desteklenenDiller = cevap.Veri;
        }
        catch { }
        if (_desteklenenDiller.Count == 0)
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
        _ = EksikCevirileriAIileTamamlaAsync();
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
    /// AI ile tek anahtar cevirisi. Herhangi iki dil arasinda ceviri yapabilir.
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
            var cevap = await yanit.Content.ReadFromJsonAsync<JsonElement>();
            if (cevap.TryGetProperty("basariliMi", out var bm) && bm.GetBoolean() &&
                cevap.TryGetProperty("veri", out var veri) && veri.ValueKind != JsonValueKind.Null)
            {
                var ceviriMetin = veri.GetString();
                if (!string.IsNullOrEmpty(ceviriMetin))
                {
                    _sozluk[anahtar] = ceviriMetin;
                    _ = AIAnahtariKaydetAsync(anahtar, ceviriMetin, hedefDil);
                    SozlukGuncellendi?.Invoke();
                    return ceviriMetin;
                }
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Eksik anahtarlari AI ile otomatik cevir. Tum dil ciftleri icin calisir.
    /// </summary>
    public async Task EksikCevirileriAIileTamamlaAsync(string kaynakDil = "tr")
    {
        if (_aktifDil == kaynakDil) return;

        Dictionary<string, string> kaynakSozluk;
        try
        {
            var cevap = await _http.GetFromJsonAsync<CevapDto>($"api/dil/ceviriler/{kaynakDil}");
            kaynakSozluk = cevap?.BasariliMi == true && cevap.Veri != null ? cevap.Veri : [];
        }
        catch { return; }

        var eksikAnahtarlar = kaynakSozluk
            .Where(kv => !_sozluk.ContainsKey(kv.Key) || string.IsNullOrEmpty(_sozluk[kv.Key]))
            .Select(kv => kv.Key)
            .ToList();

        foreach (var anahtar in eksikAnahtarlar)
        {
            if (_aiCeviriBekleyen.Contains(anahtar + _aktifDil)) continue;
            _aiCeviriBekleyen.Add(anahtar + _aktifDil);
            await AICeviriAlAsync(anahtar, kaynakSozluk[anahtar], _aktifDil, kaynakDil);
        }
    }

    private async Task AIAnahtariKaydetAsync(string anahtar, string deger, string dil)
    {
        try
        {
            await _http.PostAsJsonAsync("api/dil/ceviri-ekle", new { Anahtar = anahtar, Dil = dil, Deger = deger });
        }
        catch { }
    }

    private async Task DilDosyasiniYukleAsync(string dil)
    {
        // 1. ONCELIKLE: Statik JSON dosyasindan yukle (tarayicida aninda calisir)
        try
        {
            var sozluk = await _yerelHttp.GetFromJsonAsync<Dictionary<string, string>>($"i18n/{dil}.json");
            if (sozluk is not null && sozluk.Count > 0)
            {
                _sozluk = sozluk;
            }
        }
        catch { _sozluk = []; }

        // 2. ARKA PLANDA: API ile senkronize et (dinamik icerikler DB'den gelir)
        _ = APIIleSenkronizeEtAsync(dil);
    }

    private async Task APIIleSenkronizeEtAsync(string dil)
    {
        try
        {
            var cevap = await _http.GetFromJsonAsync<CevapDto>($"api/dil/ceviriler/{dil}");
            if (cevap?.BasariliMi == true && cevap.Veri?.Count > 0)
            {
                // API'den gelen dinamik ceviriler statik olanlarin uzerine yazar
                foreach (var kv in cevap.Veri)
                {
                    _sozluk[kv.Key] = kv.Value;
                }
                SozlukGuncellendi?.Invoke();
            }
        }
        catch { }
    }

    private class CevapDto
    {
        public bool BasariliMi { get; set; }
        public Dictionary<string, string>? Veri { get; set; }
    }

    private class CevapListeDto<T>
    {
        public bool BasariliMi { get; set; }
        public List<T>? Veri { get; set; }
    }

    public class DilBilgisi
    {
        public int Id { get; set; }
        public string Kod { get; set; } = "";
        public string Ad { get; set; } = "";
        public string? Bayrak { get; set; }
    }
}
