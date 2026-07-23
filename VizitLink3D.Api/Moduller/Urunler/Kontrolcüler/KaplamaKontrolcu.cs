using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/kaplamalar")]
public class KaplamaKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<KaplamaSecenegi>>> Liste()
    {
        var liste = await vt.KaplamaSecenekleri
            .AsNoTracking()
            .OrderBy(k => k.SiraNo)
            .ToListAsync();

        return Cevap<List<KaplamaSecenegi>>.Basarili(liste);
    }

    [HttpGet("{id:int}")]
    public async Task<Cevap<KaplamaSecenegi>> Detay(int id)
    {
        var kaplama = await vt.KaplamaSecenekleri
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kaplama is null)
            return Cevap<KaplamaSecenegi>.Hata("Kaplama secenegi bulunamadi.");

        return Cevap<KaplamaSecenegi>.Basarili(kaplama);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<KaplamaSecenegi>> Olustur([FromBody] KaplamaSecenegi kaplama)
    {
        vt.KaplamaSecenekleri.Add(kaplama);
        await vt.SaveChangesAsync();

        return Cevap<KaplamaSecenegi>.Basarili(kaplama, "Kaplama secenegi olusturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<KaplamaSecenegi>> Guncelle(int id, [FromBody] KaplamaSecenegi kaplama)
    {
        var mevcut = await vt.KaplamaSecenekleri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<KaplamaSecenegi>.Hata("Kaplama secenegi bulunamadi.");

        mevcut.Ad = kaplama.Ad;
        mevcut.Aciklama = kaplama.Aciklama;
        mevcut.HexKod = kaplama.HexKod;
        mevcut.ResimUrl = kaplama.ResimUrl;
        mevcut.MalzemeId = kaplama.MalzemeId;
        mevcut.SiraNo = kaplama.SiraNo;
        mevcut.AktifMi = kaplama.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<KaplamaSecenegi>.Basarili(mevcut, "Kaplama secenegi guncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.KaplamaSecenekleri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Kaplama secenegi bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Kaplama secenegi silindi.");
    }
}
