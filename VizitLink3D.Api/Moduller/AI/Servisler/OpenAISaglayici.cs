using System.Runtime.CompilerServices;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

public class OpenAISaglayici : IAISaglayici
{
    private readonly string _apiKey;
    private readonly HttpClient _http;
    public string SaglayiciAdi => "OpenAI";

    public OpenAISaglayici(string apiKey, HttpClient http)
    {
        _apiKey = apiKey;
        _http = http;
    }

    public async Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default)
    {
        await Task.Delay(100, iptal);
        return new AIYanit
        {
            Metin = "[AI yanıtı — OpenAI entegrasyonu henüz yapılandırılmadı]",
            IstekTokenSayisi = istek.KullaniciPrompt.Length / 4,
            CevapTokenSayisi = 50,
            MaliyetUsd = 0.001m,
            BasariliMi = true
        };
    }

    public async IAsyncEnumerable<string> MetinStreamAsync(AIIstek istek, [EnumeratorCancellation] CancellationToken iptal = default)
    {
        var yanit = await MetinUretAsync(istek, iptal);
        var kelimeler = yanit.Metin.Split(' ');
        foreach (var kelime in kelimeler)
        {
            iptal.ThrowIfCancellationRequested();
            yield return kelime + " ";
            await Task.Delay(30, iptal);
        }
    }

    public async Task<bool> SaglikTestiAsync()
    {
        await Task.Delay(10);
        return true;
    }

    public decimal MaliyetHesapla(int istekToken, int cevapToken)
        => (istekToken / 1000m * 0.15m) + (cevapToken / 1000m * 0.60m);
}
