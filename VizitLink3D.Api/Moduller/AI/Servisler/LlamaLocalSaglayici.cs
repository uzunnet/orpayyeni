using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

/// <summary>
/// LlamaLocal sağlayıcısı — LM Studio / llama.cpp sunucusuna
/// OpenAI uyumlu /v1/chat/completions endpoint'i üzerinden bağlanır.
/// Yerel GPU (CUDA) ile çalışır, API key gerekmez.
/// </summary>
public class LlamaLocalSaglayici : IAISaglayici
{
    private readonly string _baseUrl;
    private readonly HttpClient _http;
    public string SaglayiciAdi => "LlamaLocal";

    public LlamaLocalSaglayici(string apiKey, HttpClient http)
    {
        _http = http;
        _baseUrl = Environment.GetEnvironmentVariable("LLAMA_API_URL")
                   ?? apiKey
                   ?? "http://127.0.0.1:11434";
    }

    public async Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default)
    {
        try
        {
            var requestBody = new
            {
                model = istek.Model,
                messages = new[]
                {
                    new { role = "system", content = istek.SistemPrompt },
                    new { role = "user", content = istek.KullaniciPrompt }
                },
                temperature = istek.Sicaklik,
                max_tokens = istek.MaksimumToken,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // OpenAI uyumlu endpoint: POST /v1/chat/completions
            var response = await _http.PostAsync($"{_baseUrl.TrimEnd('/')}/v1/chat/completions", content, iptal);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(iptal);
                return new AIYanit
                {
                    Metin = "",
                    BasariliMi = false,
                    HataMesaji = $"LlamaLocal HTTP {response.StatusCode}: {errorBody[..Math.Min(errorBody.Length, 200)]}"
                };
            }

            var resultJson = await response.Content.ReadAsStringAsync(iptal);
            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;
            var choices = root.GetProperty("choices");
            var message = choices[0].GetProperty("message");
            var text = message.GetProperty("content").GetString() ?? "";
            var usage = root.TryGetProperty("usage", out var u) ? u : default;

            var istekToken = usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : istek.KullaniciPrompt.Length / 4;
            var cevapToken = usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : text.Length / 4;

            return new AIYanit
            {
                Metin = text,
                IstekTokenSayisi = istekToken,
                CevapTokenSayisi = cevapToken,
                MaliyetUsd = 0,
                BasariliMi = true
            };
        }
        catch (TaskCanceledException)
        {
            return new AIYanit { BasariliMi = false, HataMesaji = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            return new AIYanit { BasariliMi = false, HataMesaji = $"LlamaLocal hatası: {ex.Message}" };
        }
    }

    public async IAsyncEnumerable<string> MetinStreamAsync(AIIstek istek, [EnumeratorCancellation] CancellationToken iptal = default)
    {
        var yanit = await MetinUretAsync(istek, iptal);
        if (!yanit.BasariliMi || string.IsNullOrEmpty(yanit.Metin))
        {
            yield return yanit.HataMesaji ?? "Boş yanıt";
            yield break;
        }
        var kelimeler = yanit.Metin.Split(' ');
        foreach (var kelime in kelimeler)
        {
            iptal.ThrowIfCancellationRequested();
            yield return kelime + " ";
            await Task.Delay(20, iptal);
        }
    }

    public async Task<bool> SaglikTestiAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{_baseUrl.TrimEnd('/')}/v1/models");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public decimal MaliyetHesapla(int istekToken, int cevapToken) => 0; // Yerel LLM — maliyet yok
}
