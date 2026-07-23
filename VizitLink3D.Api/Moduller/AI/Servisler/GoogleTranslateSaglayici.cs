using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

/// <summary>
/// Google Cloud Translation API v2 sağlayıcısı.
/// REST API üzerinden çeviri yapar. API key ile kimlik doğrular.
/// Endpoint: https://translation.googleapis.com/language/translate/v2
/// </summary>
public class GoogleTranslateSaglayici : IAISaglayici
{
    private readonly string _apiKey;
    private readonly HttpClient _http;
    private static readonly string[] DesteklenenDiller = { "tr", "en", "de", "fr", "ru", "ar", "es", "zh" };
    public string SaglayiciAdi => "GoogleTranslate";

    public GoogleTranslateSaglayici(string apiKey, HttpClient http)
    {
        _apiKey = apiKey;
        _http = http;
    }

    public async Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return new AIYanit { BasariliMi = false, HataMesaji = "Google Translate API key tanimlanmamis. Admin panelden ekleyin." };

        try
        {
            // SistemPrompt'ten kaynak/hedef dil ayikla: "Turkce'den Ingilizce'ye cevir..."
            var (kaynakDil, hedefDil) = DilAyikla(istek.SistemPrompt);

            var requestBody = new
            {
                q = istek.KullaniciPrompt,
                target = hedefDil,
                source = kaynakDil,
                format = "text"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://translation.googleapis.com/language/translate/v2?key={_apiKey}";
            var response = await _http.PostAsync(url, content, iptal);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(iptal);
                return new AIYanit { BasariliMi = false, HataMesaji = $"Google Translate HTTP {response.StatusCode}: {errorBody[..Math.Min(errorBody.Length, 200)]}" };
            }

            var resultJson = await response.Content.ReadAsStringAsync(iptal);
            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;
            var translations = root.GetProperty("data").GetProperty("translations");
            var text = translations[0].GetProperty("translatedText").GetString() ?? "";

            return new AIYanit
            {
                Metin = text,
                IstekTokenSayisi = istek.KullaniciPrompt.Length,
                CevapTokenSayisi = text.Length,
                MaliyetUsd = 0.00002m * istek.KullaniciPrompt.Length, // ~$20/milyon karakter
                BasariliMi = true
            };
        }
        catch (TaskCanceledException)
        {
            return new AIYanit { BasariliMi = false, HataMesaji = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            return new AIYanit { BasariliMi = false, HataMesaji = $"Google Translate hatasi: {ex.Message}" };
        }
    }

    /// <summary>
    /// Sistem prompt'tan kaynak ve hedef dil kodlarini cikarir.
    /// Prompt: "X'den Y'ye cevir" -> (xKod, yKod)
    /// </summary>
    private static (string kaynak, string hedef) DilAyikla(string sistemPrompt)
    {
        var dilHaritasi = new Dictionary<string, string>
        {
            ["turkce"] = "tr", ["ingilizce"] = "en", ["almanca"] = "de",
            ["fransizca"] = "fr", ["rusca"] = "ru", ["arapca"] = "ar",
            ["ispanyolca"] = "es", ["cince"] = "zh"
        };

        var promptKucuk = sistemPrompt.ToLowerInvariant();
        string kaynak = "tr", hedef = "en";

        foreach (var (ad, kod) in dilHaritasi)
        {
            if (promptKucuk.Contains($"{ad}'den")) kaynak = kod;
            if (promptKucuk.Contains($"{ad}'ye")) hedef = kod;
            if (promptKucuk.Contains($"{ad}'den")) kaynak = kod;
            if (promptKucuk.Contains($"{ad}'ya")) hedef = kod;
        }

        // Google Translate desteklemedigi diller icin fallback
        if (!DesteklenenDiller.Contains(kaynak)) kaynak = "tr";
        if (!DesteklenenDiller.Contains(hedef)) hedef = "en";

        return (kaynak, hedef);
    }

    public async IAsyncEnumerable<string> MetinStreamAsync(AIIstek istek, [EnumeratorCancellation] CancellationToken iptal = default)
    {
        var yanit = await MetinUretAsync(istek, iptal);
        if (!yanit.BasariliMi || string.IsNullOrEmpty(yanit.Metin))
        {
            yield return yanit.HataMesaji ?? "Boş yanıt";
            yield break;
        }
        yield return yanit.Metin;
    }

    public async Task<bool> SaglikTestiAsync()
    {
        if (string.IsNullOrWhiteSpace(_apiKey)) return false;
        try
        {
            var testBody = JsonSerializer.Serialize(new { q = "test", target = "en", format = "text" });
            var content = new StringContent(testBody, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"https://translation.googleapis.com/language/translate/v2?key={_apiKey}", content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public decimal MaliyetHesapla(int istekToken, int cevapToken)
        => istekToken * 0.00002m; // Google Cloud Translation: $20/1M chars
}
