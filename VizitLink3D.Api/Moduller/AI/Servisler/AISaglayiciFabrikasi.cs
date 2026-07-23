using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.AI;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

public class AISaglayiciFabrikasi
{
    private readonly IServiceProvider _sp;
    private readonly IApiKeySifrelemeServisi _sifrelemeServisi;
    private readonly IHttpClientFactory _httpFabrikasi;

    public AISaglayiciFabrikasi(IServiceProvider sp, IApiKeySifrelemeServisi sifrelemeServisi, IHttpClientFactory httpFabrikasi)
    {
        _sp = sp;
        _sifrelemeServisi = sifrelemeServisi;
        _httpFabrikasi = httpFabrikasi;
    }

    public IAISaglayici SaglayiciOlustur(AISaglayicisi saglayici, HttpClient http)
    {
        var apiKey = _sifrelemeServisi.Coz(saglayici.ApiKeyEncrypted) ?? "";

        return saglayici.Tip switch
        {
            AISaglayiciTipi.OpenAI => new OpenAISaglayici(apiKey, http),
            AISaglayiciTipi.Anthropic => new AnthropicSaglayici(apiKey, http),
            AISaglayiciTipi.Gemini => new GeminiSaglayici(apiKey, http),
            AISaglayiciTipi.LlamaLocal => new LlamaLocalSaglayici(apiKey, http),
            AISaglayiciTipi.GoogleTranslate => new GoogleTranslateSaglayici(apiKey, http),
            AISaglayiciTipi.DeepSeek => new DeepSeekSaglayici(apiKey, http),
            AISaglayiciTipi.OpenCodeZen => new ZenSaglayici(apiKey, http),
            _ => new OpenAISaglayici(apiKey, http)
        };
    }

    /// <summary>Sağlayıcıyı Id ile getir (orkestra işçi seçimi için).</summary>
    public async Task<(IAISaglayici Saglayici, AISaglayicisi Entity)?> SaglayiciIdIleGetirAsync(VizitLink3DDbContext db, int id, HttpClient http)
    {
        var entity = await db.AISaglayicilari.FirstOrDefaultAsync(s => s.Id == id && s.AktifMi);
        if (entity == null) return null;
        return (SaglayiciOlustur(entity, http), entity);
    }

    public async Task<IAISaglayici?> SaglayiciGetirAsync(VizitLink3DDbContext db, AISaglayiciTipi? tip = null, HttpClient? http = null)
    {
        var entity = tip.HasValue
            ? await db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi && s.Tip == tip.Value)
            : await db.AISaglayicilari.FirstOrDefaultAsync(s => s.AktifMi);

        if (entity == null) return null;

        var client = http ?? _httpFabrikasi.CreateClient();
        return SaglayiciOlustur(entity, client);
    }
}
