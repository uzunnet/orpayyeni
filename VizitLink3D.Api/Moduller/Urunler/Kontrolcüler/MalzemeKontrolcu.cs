using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/malzemeler")]
public class MalzemeKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<Malzeme>>> Liste()
    {
        var liste = await vt.Malzemeler
            .AsNoTracking()
            .OrderBy(m => m.SiraNo)
            .ToListAsync();

        return Cevap<List<Malzeme>>.Basarili(liste);
    }

    [HttpGet("{id:int}")]
    public async Task<Cevap<Malzeme>> Detay(int id)
    {
        var malzeme = await vt.Malzemeler
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (malzeme is null)
            return Cevap<Malzeme>.Hata("Malzeme bulunamadi.");

        return Cevap<Malzeme>.Basarili(malzeme);
    }

    [HttpGet("{id:int}/kaplamalar")]
    public async Task<Cevap<List<KaplamaSecenegi>>> Kaplamalar(int id)
    {
        var kaplamalar = await vt.KaplamaSecenekleri
            .AsNoTracking()
            .Where(k => k.MalzemeId == id)
            .OrderBy(k => k.SiraNo)
            .ToListAsync();

        return Cevap<List<KaplamaSecenegi>>.Basarili(kaplamalar);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<Malzeme>> Olustur([FromBody] Malzeme malzeme)
    {
        vt.Malzemeler.Add(malzeme);
        await vt.SaveChangesAsync();

        return Cevap<Malzeme>.Basarili(malzeme, "Malzeme olusturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<Malzeme>> Guncelle(int id, [FromBody] Malzeme malzeme)
    {
        var mevcut = await vt.Malzemeler.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<Malzeme>.Hata("Malzeme bulunamadi.");

        mevcut.Ad = malzeme.Ad;
        mevcut.Aciklama = malzeme.Aciklama;
        mevcut.Tip = malzeme.Tip;
        mevcut.ResimUrl = malzeme.ResimUrl;
        mevcut.SiraNo = malzeme.SiraNo;
        mevcut.AktifMi = malzeme.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<Malzeme>.Basarili(mevcut, "Malzeme guncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.Malzemeler.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Malzeme bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Malzeme silindi.");
    }
}
