using VizitLink3D.Api.Servisler;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Istekler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Yonetim;

[ApiController]
[Route("api/yonetim/ceviri")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class OtomatikCeviriKontrolcu : ControllerBase
{
    private readonly IOtomatikCeviriServisi _ceviriServisi;

    public OtomatikCeviriKontrolcu(IOtomatikCeviriServisi ceviriServisi)
    {
        _ceviriServisi = ceviriServisi;
    }

    [HttpPost("cevir")]
    public async Task<ActionResult<Cevap<string>>> Cevir([FromBody] OtomatikCeviriIstegi istek)
    {
        if (istek == null || string.IsNullOrWhiteSpace(istek.Metin) || string.IsNullOrWhiteSpace(istek.HedefDil))
        {
            return BadRequest(Cevap<string>.Hata("Metin ve Hedef Dil alanları zorunludur."));
        }

        var sonuc = await _ceviriServisi.CevirAsync(istek.Metin, istek.KaynakDil, istek.HedefDil);
        return Ok(sonuc);
    }

    [HttpPost("kaydet")]
    public async Task<ActionResult<Cevap<string>>> Kaydet([FromBody] VizitLink3D.Ortak.Modeller.Ceviriler.CeviriKayitIstegi istek, [FromServices] VizitLink3D.Api.VeriTabani.VizitLink3DDbContext db, [FromServices] VizitLink3D.Api.Servisler.ICeviriServisi ceviriCache)
    {
        if (string.IsNullOrWhiteSpace(istek.Anahtar) || string.IsNullOrWhiteSpace(istek.Dil) || string.IsNullOrWhiteSpace(istek.Deger))
            return BadRequest(Cevap<string>.Hata("Anahtar, Dil ve Değer zorunludur."));

        var mevcut = await db.Ceviriler.FirstOrDefaultAsync(c => c.Anahtar == istek.Anahtar && c.Dil == istek.Dil);
        if (mevcut != null)
        {
            mevcut.Deger = istek.Deger;
            mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        }
        else
        {
            db.Ceviriler.Add(new VizitLink3D.Ortak.Modeller.Ceviri
            {
                Anahtar = istek.Anahtar,
                Dil = istek.Dil,
                Deger = istek.Deger
            });
        }
        await db.SaveChangesAsync();
        await ceviriCache.OnbellekTemizleAsync();

        return Ok(Cevap<string>.Basarili("Kaydedildi"));
    }
}

