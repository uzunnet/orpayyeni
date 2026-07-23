using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/urun-kategorileri")]
public class UrunKategoriKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<UrunKategori>>> Liste()
    {
        var liste = await vt.UrunKategorileri
            .AsNoTracking()
            .OrderBy(k => k.SiraNo)
            .ToListAsync();

        return Cevap<List<UrunKategori>>.Basarili(liste);
    }

    [HttpGet("{id:int}")]
    public async Task<Cevap<UrunKategori>> Detay(int id)
    {
        var kategori = await vt.UrunKategorileri
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kategori is null)
            return Cevap<UrunKategori>.Hata("Urun kategorisi bulunamadi.");

        return Cevap<UrunKategori>.Basarili(kategori);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunKategori>> Olustur([FromBody] UrunKategori kategori)
    {
        vt.UrunKategorileri.Add(kategori);
        await vt.SaveChangesAsync();

        return Cevap<UrunKategori>.Basarili(kategori, "Urun kategorisi olusturuldu.");
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunKategori>> Guncelle(int id, [FromBody] UrunKategori kategori)
    {
        var mevcut = await vt.UrunKategorileri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<UrunKategori>.Hata("Urun kategorisi bulunamadi.");

        mevcut.Ad = kategori.Ad;
        mevcut.Slug = kategori.Slug;
        mevcut.Aciklama = kategori.Aciklama;
        mevcut.UstKategoriId = kategori.UstKategoriId;
        mevcut.SiraNo = kategori.SiraNo;
        mevcut.AktifMi = kategori.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<UrunKategori>.Basarili(mevcut, "Urun kategorisi guncellendi.");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int id)
    {
        var mevcut = await vt.UrunKategorileri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi)
            return Cevap<bool>.Hata("Urun kategorisi bulunamadi.");

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true, "Urun kategorisi silindi.");
    }
}
