using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

/// <summary>
/// DeepSeek sağlayıcısı — OpenAI-uyumlu chat/completions API kullanır.
/// API key admin panelden AISaglayicilari tablosuna şifreli kaydedilir.
/// </summary>
public class DeepSeekSaglayici : IAISaglayici
{
    private const string ApiUrl = "https://api.deepseek.com/chat/completions";
    private const string VarsayilanModel = "deepseek-chat";

    private readonly string _apiKey;
    private readonly HttpClient _http;
    public string SaglayiciAdi => "DeepSeek";

    public DeepSeekSaglayici(string apiKey, HttpClient http)
    {
        _apiKey = apiKey;
        _http = http;
    }

    public async Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default)
    {
        try
        {
            // AIIstek.Model varsayılanı "gpt-4o-mini" — DeepSeek'e ait olmayan modelleri düzelt
            var model = istek.Model.StartsWith("deepseek", StringComparison.OrdinalIgnoreCase)
                ? istek.Model
                : VarsayilanModel;

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
                max_tokens = istek.MaksimumToken,
                temperature = istek.Sicaklik
            };

            using var mesaj = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(govde), Encoding.UTF8, "application/json")
            };
            mesaj.Headers.Add("Authorization", $"Bearer {_apiKey}");

            using var yanit = await _http.SendAsync(mesaj, iptal);
            var json = await yanit.Content.ReadAsStringAsync(iptal);

            if (!yanit.IsSuccessStatusCode)
                return new AIYanit { BasariliMi = false, HataMesaji = $"DeepSeek API {(int)yanit.StatusCode}: {json}" };

            using var dok = JsonDocument.Parse(json);
            var kok = dok.RootElement;
            var metin = kok.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
            var istekToken = kok.GetProperty("usage").GetProperty("prompt_tokens").GetInt32();
            var cevapToken = kok.GetProperty("usage").GetProperty("completion_tokens").GetInt32();

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
        // Basit akış: tam yanıtı alıp kelime kelime ilet (mevcut sağlayıcı desenine uygun)
        var yanit = await MetinUretAsync(istek, iptal);
        if (!yanit.BasariliMi)
        {
            yield return yanit.HataMesaji ?? "DeepSeek hatası";
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
            using var mesaj = new HttpRequestMessage(HttpMethod.Get, "https://api.deepseek.com/models");
            mesaj.Headers.Add("Authorization", $"Bearer {_apiKey}");
            using var yanit = await _http.SendAsync(mesaj);
            return yanit.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    /// <summary>deepseek-chat: ~$0.27/M girdi, ~$1.10/M çıktı.</summary>
    public decimal MaliyetHesapla(int istekToken, int cevapToken)
        => (istekToken / 1000m * 0.00027m) + (cevapToken / 1000m * 0.0011m);
}
