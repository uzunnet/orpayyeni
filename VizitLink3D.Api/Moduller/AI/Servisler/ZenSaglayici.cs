using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

/// <summary>
/// OpenCode Zen sağlayıcısı — OpenAI-uyumlu gateway (opencode.ai/zen).
/// Tek anahtarla birden çok model sunar: deepseek-v4-flash(-free), minimax-m3,
/// minimax-m2.7, mimo-v2.5-free. Model, AISaglayicisi.Model alanından gelir.
/// (Bu sınıfın ilk taslağı deepseek-v4-flash-free tarafından üretildi, denetlenip uygulandı.)
/// </summary>
public class ZenSaglayici : IAISaglayici
{
    private const string Url = "https://opencode.ai/zen/v1/chat/completions";
    private const string VarsayilanModel = "deepseek-v4-flash-free";
    private const decimal IstekTokenFiyat = 0.0002m;
    private const decimal CevapTokenFiyat = 0.0008m;

    private readonly HttpClient _http;
    private readonly string _apiKey;

    public string SaglayiciAdi => "OpenCodeZen";

    public ZenSaglayici(string apiKey, HttpClient http)
    {
        _apiKey = apiKey;
        _http = http;
    }

    public async Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default)
    {
        try
        {
            var model = string.IsNullOrWhiteSpace(istek.Model) || istek.Model.StartsWith("gpt", StringComparison.OrdinalIgnoreCase)
                ? VarsayilanModel
                : istek.Model;

            var govde = new
            {
                model,
                messages = new object[]
                {
                    new { role = "system", content = istek.SistemPrompt },
                    new { role = "user", content = string.IsNullOrEmpty(istek.Baglam)
                        ? istek.KullaniciPrompt
                        : $"{istek.Baglam}\n\n{istek.KullaniciPrompt}" }
                },
                temperature = istek.Sicaklik,
                max_tokens = istek.MaksimumToken
            };

            using var mesaj = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(JsonSerializer.Serialize(govde), Encoding.UTF8, "application/json")
            };
            mesaj.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var yanit = await _http.SendAsync(mesaj, iptal);
            var json = await yanit.Content.ReadAsStringAsync(iptal);

            if (!yanit.IsSuccessStatusCode)
                return new AIYanit { BasariliMi = false, HataMesaji = $"Zen API {(int)yanit.StatusCode}: {json}" };

            using var dok = JsonDocument.Parse(json);
            var kok = dok.RootElement;

            var metin = string.Empty;
            if (kok.TryGetProperty("choices", out var secenekler) && secenekler.GetArrayLength() > 0
                && secenekler[0].TryGetProperty("message", out var m))
            {
                if (m.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String)
                    metin = c.GetString() ?? "";
                // Bazı free modeller içerik yerine reasoning_content doldurur
                if (string.IsNullOrEmpty(metin) && m.TryGetProperty("reasoning_content", out var r) && r.ValueKind == JsonValueKind.String)
                    metin = r.GetString() ?? "";
            }

            int istekToken = 0, cevapToken = 0;
            if (kok.TryGetProperty("usage", out var kullanim))
            {
                if (kullanim.TryGetProperty("prompt_tokens", out var pt)) istekToken = pt.GetInt32();
                if (kullanim.TryGetProperty("completion_tokens", out var ct)) cevapToken = ct.GetInt32();
            }

            return new AIYanit
            {
                Metin = metin,
                IstekTokenSayisi = istekToken,
                CevapTokenSayisi = cevapToken,
                MaliyetUsd = MaliyetHesapla(istekToken, cevapToken),
                BasariliMi = true
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new AIYanit { BasariliMi = false, HataMesaji = ex.Message };
        }
    }

    public async IAsyncEnumerable<string> MetinStreamAsync(AIIstek istek, [EnumeratorCancellation] CancellationToken iptal = default)
    {
        var yanit = await MetinUretAsync(istek, iptal);
        if (!yanit.BasariliMi)
        {
            yield return yanit.HataMesaji ?? "Zen hatası";
            yield break;
        }
        foreach (var kelime in yanit.Metin.Split(' '))
        {
            iptal.ThrowIfCancellationRequested();
            yield return kelime + " ";
        }
    }

    public async Task<bool> SaglikTestiAsync()
    {
        try
        {
            using var mesaj = new HttpRequestMessage(HttpMethod.Get, "https://opencode.ai/zen/v1/models");
            mesaj.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            using var yanit = await _http.SendAsync(mesaj);
            return yanit.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public decimal MaliyetHesapla(int istekToken, int cevapToken)
        => (istekToken / 1000m * IstekTokenFiyat) + (cevapToken / 1000m * CevapTokenFiyat);
}
