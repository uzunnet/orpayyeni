namespace VizitLink3D.Api.Moduller.AI.Servisler;

public interface IAISaglayici
{
    Task<AIYanit> MetinUretAsync(AIIstek istek, CancellationToken iptal = default);
    IAsyncEnumerable<string> MetinStreamAsync(AIIstek istek, CancellationToken iptal = default);
    Task<bool> SaglikTestiAsync();
    decimal MaliyetHesapla(int istekToken, int cevapToken);
    string SaglayiciAdi { get; }
}

public class AIIstek
{
    public string SistemPrompt { get; set; } = "Sen VizitLink3D kapı ve mobilya sektöründe uzman bir asistansın. Türkçe yanıt ver.";
    public string KullaniciPrompt { get; set; } = "";
    public string Model { get; set; } = "gpt-4o-mini";
    public float Sicaklik { get; set; } = 0.7f;
    public int MaksimumToken { get; set; } = 2000;
    public string? Baglam { get; set; }
}

public class AIYanit
{
    public string Metin { get; set; } = "";
    public int IstekTokenSayisi { get; set; }
    public int CevapTokenSayisi { get; set; }
    public decimal MaliyetUsd { get; set; }
    public bool BasariliMi { get; set; }
    public string? HataMesaji { get; set; }
}
