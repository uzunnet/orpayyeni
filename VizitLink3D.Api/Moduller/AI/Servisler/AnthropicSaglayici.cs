using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

/// <summary>
/// Anthropic Claude sağlayıcısı — Messages API (v1/messages).
/// Orkestrasyon senaryosunda "emir veren" (planlayıcı) roldedir;
/// kod yazma işi DeepSeekSaglayici'ya delege edilir (bkz. AIOrkestraServisi).
/// </summary>
public class AnthropicSaglayici : IAISaglayici
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string VarsayilanModel = "claude-opus-4-8";

    private readonly string _apiKey;
    private readonly HttpClient _http;
    public string SaglayiciAdi => "Anthropic";

    public AnthropicSaglayici(string apiKey, HttpClient http)
    {
        _apiKey = apiKey;
        _http = http;
    }

    public async Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default)
    {
        try
        {
            // AIIstek.Model varsayılanı "gpt-4o-mini" — Claude'a ait olmayan modelleri düzelt
            var model = istek.Model.StartsWith("claude", StringComparison.OrdinalIgnoreCase)
                ? istek.Model
                : VarsayilanModel;

            // Opus 4.7+ modellerinde temperature parametresi kaldırıldı — gönderme.
            // Adaptif düşünme (thinking) derinliği output_config.effort ile kontrol edilir.
            var govde = new
            {
                model,
                max_tokens = Math.Max(istek.MaksimumToken, 1024),
                thinking = new { type = "adaptive" },
                system = istek.SistemPrompt,
                messages = new object[]
                {
                    new { role = "user", content = string.IsNullOrEmpty(istek.Baglam)
                        ? istek.KullaniciPrompt
                        : $"{istek.Baglam}\n\n{istek.KullaniciPrompt}" }
                }
            };

            using var mesaj = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(govde), Encoding.UTF8, "application/json")
            };
            mesaj.Headers.Add("x-api-key", _apiKey);
            mesaj.Headers.Add("anthropic-version", "2023-06-01");

            using var yanit = await _http.SendAsync(mesaj, iptal);
            var json = await yanit.Content.ReadAsStringAsync(iptal);

            if (!yanit.IsSuccessStatusCode)
                return new AIYanit { BasariliMi = false, HataMesaji = $"Anthropic API {(int)yanit.StatusCode}: {json}" };

            using var dok = JsonDocument.Parse(json);
            var kok = dok.RootElement;

            // stop_reason=refusal olabilir — content'e dokunmadan önce kontrol et
            var durma = kok.GetProperty("stop_reason").GetString();
            if (durma == "refusal")
                return new AIYanit { BasariliMi = false, HataMesaji = "Claude isteği güvenlik nedeniyle reddetti (refusal)." };

            // content: thinking + text blokları — sadece text bloklarını birleştir
            var sb = new StringBuilder();
            foreach (var blok in kok.GetProperty("content").EnumerateArray())
            {
                if (blok.GetProperty("type").GetString() == "text")
                    sb.Append(blok.GetProperty("text").GetString());
            }

            var istekToken = kok.GetProperty("usage").GetProperty("input_tokens").GetInt32();
            var cevapToken = kok.GetProperty("usage").GetProperty("output_tokens").GetInt32();

            return new AIYanit
            {
                Metin = sb.ToString(),
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
            yield return yanit.HataMesaji ?? "Anthropic hatası";
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
            using var mesaj = new HttpRequestMessage(HttpMethod.Get, "https://api.anthropic.com/v1/models");
            mesaj.Headers.Add("x-api-key", _apiKey);
            mesaj.Headers.Add("anthropic-version", "2023-06-01");
            using var yanit = await _http.SendAsync(mesaj);
            return yanit.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    /// <summary>claude-opus-4-8: $5/M girdi, $25/M çıktı.</summary>
    public decimal MaliyetHesapla(int istekToken, int cevapToken)
        => (istekToken / 1000m * 0.005m) + (cevapToken / 1000m * 0.025m);
}
