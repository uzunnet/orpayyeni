using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

public class GeminiSaglayici : IAISaglayici
{
    private readonly string _apiKey;
    private readonly HttpClient _http;
    public string SaglayiciAdi => "Gemini";

    public GeminiSaglayici(string apiKey, HttpClient http)
    {
        _apiKey = apiKey;
        _http = http;
    }

    public async Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var modelId = string.IsNullOrEmpty(istek.Model) ? "gemini-1.5-flash" : istek.Model;
            if (modelId.Contains("pro")) url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:generateContent?key={_apiKey}";

            var payload = new
            {
                system_instruction = new { parts = new { text = istek.SistemPrompt } },
                contents = new[]
                {
                    new { parts = new[] { new { text = istek.KullaniciPrompt } } }
                }
            };

            var response = await _http.PostAsJsonAsync(url, payload, iptal);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(iptal);
                return new AIYanit { BasariliMi = false, HataMesaji = $"HTTP {response.StatusCode}: {errorText}" };
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: iptal);
            var text = json.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

            return new AIYanit
            {
                Metin = text ?? "",
                IstekTokenSayisi = istek.KullaniciPrompt.Length / 4, // Basit tahmin
                CevapTokenSayisi = (text?.Length ?? 0) / 4,
                MaliyetUsd = 0.0005m,
                BasariliMi = true
            };
        }
        catch (Exception ex)
        {
            return new AIYanit { BasariliMi = false, HataMesaji = ex.Message };
        }
    }

    public async IAsyncEnumerable<string> MetinStreamAsync(AIIstek istek, [EnumeratorCancellation] CancellationToken iptal = default)
    {
        // Şimdilik stream yerine toplu döndürüp stream gibi verelim
        var yanit = await MetinUretAsync(istek, iptal);
        if (!yanit.BasariliMi)
        {
            yield return $"[Hata: {yanit.HataMesaji}]";
            yield break;
        }

        var kelimeler = yanit.Metin.Split(' ');
        foreach (var kelime in kelimeler)
        {
            iptal.ThrowIfCancellationRequested();
            yield return kelime + " ";
            await Task.Delay(10, iptal);
        }
    }

    public async Task<bool> SaglikTestiAsync()
    {
        try
        {
            var yanit = await MetinUretAsync(new AIIstek { KullaniciPrompt = "Test", SistemPrompt = "Cevap olarak sadece 'OK' yaz." });
            return yanit.BasariliMi && yanit.Metin.Contains("OK");
        }
        catch
        {
            return false;
        }
    }

    public decimal MaliyetHesapla(int istekToken, int cevapToken)
        => (istekToken / 1000m * 0.075m) + (cevapToken / 1000m * 0.30m);
}
