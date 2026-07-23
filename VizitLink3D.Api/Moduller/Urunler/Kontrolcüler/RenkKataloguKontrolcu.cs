using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Renkler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/renk-kataloglari")]
public class RenkKataloguKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<RenkKatalogu>>> Liste()
    {
        var liste = await vt.RenkKataloglari
            .AsNoTracking()
            .OrderBy(k => k.Ad)
            .ToListAsync();

        return Cevap<List<RenkKatalogu>>.Basarili(liste);
    }

    [HttpGet("{id:int}")]
    public async Task<Cevap<RenkKatalogu>> Detay(int id)
    {
        var katalog = await vt.RenkKataloglari
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);

        if (katalog is null)
            return Cevap<RenkKatalogu>.Hata("Renk katalogu bulunamadi.");

        return Cevap<RenkKatalogu>.Basarili(katalog);
    }

    [HttpGet("{id:int}/renkler")]
    public async Task<Cevap<List<RalRengi>>> Renkler(int id)
    {
        var renkler = await vt.RalRenkleri
            .AsNoTracking()
            .Where(r => r.KatalogId == id)
            .OrderBy(r => r.SiraNo)
            .ToListAsync();

        return Cevap<List<RalRengi>>.Basarili(renkler);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<RenkKatalogu>> Olustur([FromBody] RenkKatalogu katalog)
    {
        vt.RenkKataloglari.Add(katalog);
        await vt.SaveChangesAsync();

        return Cevap<RenkKatalogu>.Basarili(katalog, "Renk katalogu olusturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<RenkKatalogu>> Guncelle(int id, [FromBody] RenkKatalogu katalog)
    {
        var mevcut = await vt.RenkKataloglari.FindAsync(id);
        if (mevcut is null)
            return Cevap<RenkKatalogu>.Hata("Renk katalogu bulunamadi.");

        mevcut.Ad = katalog.Ad;
        mevcut.Aciklama = katalog.Aciklama;
        mevcut.AktifMi = katalog.AktifMi;

        await vt.SaveChangesAsync();
        return Cevap<RenkKatalogu>.Basarili(mevcut, "Renk katalogu guncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.RenkKataloglari.FindAsync(id);
        if (mevcut is null)
            return Cevap<bool>.Hata("Renk katalogu bulunamadi.");

        mevcut.AktifMi = false;
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Renk katalogu pasif yapildi.");
    }
}
