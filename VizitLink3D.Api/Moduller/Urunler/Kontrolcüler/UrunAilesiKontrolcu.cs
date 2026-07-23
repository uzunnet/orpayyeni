using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/urun-ailesi")]
public class UrunAilesiKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<UrunAilesi>>> Liste()
    {
        var liste = await vt.UrunAilesileri
            .AsNoTracking()
            .OrderBy(a => a.SiraNo)
            .ToListAsync();

        return Cevap<List<UrunAilesi>>.Basarili(liste);
    }

    [HttpGet("{id:int}")]
    public async Task<Cevap<UrunAilesi>> Detay(int id)
    {
        var aile = await vt.UrunAilesileri
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aile is null)
            return Cevap<UrunAilesi>.Hata("Urun ailesi bulunamadi.");

        return Cevap<UrunAilesi>.Basarili(aile);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunAilesi>> Olustur([FromBody] UrunAilesi aile)
    {
        vt.UrunAilesileri.Add(aile);
        await vt.SaveChangesAsync();

        return Cevap<UrunAilesi>.Basarili(aile, "Urun ailesi olusturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunAilesi>> Guncelle(int id, [FromBody] UrunAilesi aile)
    {
        var mevcut = await vt.UrunAilesileri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<UrunAilesi>.Hata("Urun ailesi bulunamadi.");

        mevcut.Ad = aile.Ad;
        mevcut.Slug = aile.Slug;
        mevcut.Aciklama = aile.Aciklama;
        mevcut.VarsayilanDetaySablonu = aile.VarsayilanDetaySablonu;
        mevcut.SiraNo = aile.SiraNo;
        mevcut.AktifMi = aile.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<UrunAilesi>.Basarili(mevcut, "Urun ailesi guncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.UrunAilesileri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Urun ailesi bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Urun ailesi silindi.");
    }
}
