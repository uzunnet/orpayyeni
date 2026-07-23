using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.AI.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;

namespace VizitLink3D.Api.Servisler;

public interface IOtomatikCeviriServisi
{
    Task<Cevap<string>> CevirAsync(string metin, string kaynakDil, string hedefDil);
}

public class OtomatikCeviriServisi : IOtomatikCeviriServisi
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OtomatikCeviriServisi> _logger;
    private readonly AISaglayiciFabrikasi _aiSaglayiciFabrikasi;
    private readonly VizitLink3DDbContext _veriTabani;

    public OtomatikCeviriServisi(
        IHttpClientFactory httpClientFactory,
        ILogger<OtomatikCeviriServisi> logger,
        AISaglayiciFabrikasi aiSaglayiciFabrikasi,
        VizitLink3DDbContext veriTabani)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _aiSaglayiciFabrikasi = aiSaglayiciFabrikasi;
        _veriTabani = veriTabani;
    }

    public async Task<Cevap<string>> CevirAsync(string metin, string kaynakDil, string hedefDil)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return Cevap<string>.Basarili(string.Empty);

        if (kaynakDil.Equals(hedefDil, StringComparison.OrdinalIgnoreCase))
            return Cevap<string>.Basarili(metin);

        if (YerelCeviriEtkinMi())
            return await YerelModelleCevirAsync(metin, kaynakDil, hedefDil);

        try
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={kaynakDil}&tl={hedefDil}&dt=t&q={Uri.EscapeDataString(metin)}";
            
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Çeviri servisi hata döndürdü. Hedef: {HedefDil}, Durum: {DurumKodu}", hedefDil, response.StatusCode);
                return Cevap<string>.Hata($"Çeviri API hatası: {response.StatusCode}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            // Google Translate GTX endpoint returns an array like: [[["translated text", "original text", null, null, 1]], null, "tr", ...]
            
            using var doc = JsonDocument.Parse(responseText);
            var rootArray = doc.RootElement.EnumerateArray().FirstOrDefault();
            
            if (rootArray.ValueKind == JsonValueKind.Array)
            {
                var translatedText = string.Empty;
                foreach (var segment in rootArray.EnumerateArray())
                {
                    if (segment.ValueKind == JsonValueKind.Array && segment.GetArrayLength() > 0)
                    {
                        translatedText += segment[0].GetString();
                    }
                }
                
                return Cevap<string>.Basarili(translatedText.Trim());
            }

            return Cevap<string>.Hata("Çeviri yanıtı yorumlanamadı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yapay zeka çevirisi sırasında beklenmeyen bir hata oluştu.");
            return Cevap<string>.Hata("Sistem hatası nedeniyle çeviri yapılamadı.");
        }
    }

    private async Task<Cevap<string>> YerelModelleCevirAsync(string metin, string kaynakDil, string hedefDil)
    {
        var saglayiciKaydi = await _veriTabani.AISaglayicilari
            .FirstOrDefaultAsync(x => x.AktifMi && x.Tip == AISaglayiciTipi.LlamaLocal);

        if (saglayiciKaydi is null)
            return Cevap<string>.Hata("Yerel çeviri sağlayıcısı etkin değil.");

        var saglayici = _aiSaglayiciFabrikasi.SaglayiciOlustur(
            saglayiciKaydi,
            _httpClientFactory.CreateClient());

        var yanit = await saglayici.MetinUretAsync(new AIIstek
        {
            KullaniciPrompt = metin,
            SistemPrompt = $"Sen profesyonel bir web sitesi çevirmenisin. Metni {DilAdiGetir(kaynakDil)} dilinden {DilAdiGetir(hedefDil)} diline çevir. Yalnız çeviriyi döndür; açıklama, başlık, tırnak işareti veya not ekleme.",
            Model = Environment.GetEnvironmentVariable("LLAMA_TRANSLATION_MODEL") ?? saglayiciKaydi.Model,
            Sicaklik = 0.1f,
            MaksimumToken = Math.Clamp(metin.Length * 2, 96, 1400)
        });

        var ceviri = yanit.Metin.Trim();
        if (!yanit.BasariliMi || !CeviriGecerliMi(metin, ceviri))
        {
            _logger.LogWarning("Yerel çeviri kalite kontrolünden geçemedi. Hedef dil: {HedefDil}", hedefDil);
            return Cevap<string>.Hata("Yerel model geçerli bir çeviri üretemedi; kayıt yapılmadı.");
        }

        return Cevap<string>.Basarili(ceviri);
    }

    private static bool YerelCeviriEtkinMi()
        => string.Equals(Environment.GetEnvironmentVariable("YEREL_CEVIRI_AKTIF"), "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Environment.GetEnvironmentVariable("YEREL_CEVIRI_AKTIF"), "true", StringComparison.OrdinalIgnoreCase);

    private static bool CeviriGecerliMi(string kaynakMetin, string ceviriMetni)
        => !string.IsNullOrWhiteSpace(ceviriMetni)
            && !string.Equals(kaynakMetin.Trim(), ceviriMetni.Trim(), StringComparison.OrdinalIgnoreCase)
            && (kaynakMetin.Length < 20 || ceviriMetni.Length >= Math.Max(8, kaynakMetin.Length / 6));

    private static string DilAdiGetir(string dilKodu) => dilKodu.ToLowerInvariant() switch
    {
        "tr" => "Türkçe",
        "en" => "İngilizce",
        "de" => "Almanca",
        "fr" => "Fransızca",
        "ru" => "Rusça",
        "ar" => "Arapça",
        "es" => "İspanyolca",
        "zh" => "Çince",
        _ => dilKodu
    };
}
