using VizitLink3D.Api.Servisler;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/pdf-uygulama")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PdfUygulamaKontrolcu(PdfUygulamaAlanServisi servis) : ControllerBase
{
    [HttpPost("isle")]
    public async Task<Cevap<List<UygulamaAlanSonucu>>> IsleAsync(CancellationToken iptal)
    {
        var sonuclar = await servis.TumSayfalariIsleAsync(iptal);
        return Cevap<List<UygulamaAlanSonucu>>.Basarili(sonuclar,
            $"{sonuclar.Count} uygulama alanı görseli oluşturuldu.");
    }

    [HttpGet("liste")]
    public async Task<Cevap<List<UygulamaAlanSonucu>>> ListeAsync(CancellationToken iptal)
    {
        var liste = await servis.TumListeAsync(iptal);
        return Cevap<List<UygulamaAlanSonucu>>.Basarili(liste);
    }

    [HttpPost("urun-esle")]
    public async Task<Cevap<string>> UrunEsleAsync([FromBody] UrunEslemeIstegi istek, CancellationToken iptal)
    {
        await servis.UrunEsleAsync(istek, iptal);
        return Cevap<string>.Basarili("Eşleme kaydedildi.");
    }

    [HttpDelete("{medyaId:int}/urun/{urunId:int}")]
    public async Task<Cevap<string>> EslemeKaldir(int medyaId, int urunId, CancellationToken iptal)
    {
        await servis.EslemeKaldir(medyaId, urunId, iptal);
        return Cevap<string>.Basarili("Eşleme kaldırıldı.");
    }
}
