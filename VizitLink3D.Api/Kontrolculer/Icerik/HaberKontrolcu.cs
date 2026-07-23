using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer;

[ApiController]
[Route("api/Haber-yazilari")]
public class HaberKontrolcu(VizitLink3DDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> TumunuGetir()
    {
        var yazilar = await db.Haberler
            .Include(b => b.Resimler)
            .Where(b => b.AktifMi)
            .OrderByDescending(b => b.OlusturmaTarihi)
            .Select(b => new
            {
                b.Id,
                b.Baslik,
                b.Slug,
                b.Ozet,
                b.Icerik,
                b.AnaResimUrl,
                b.SeoBaslik,
                b.SeoAciklama,
                Tarih = b.OlusturmaTarihi,
                b.Etiketler,
                b.AktifMi,
                b.OkunmaSayisi,
                b.OlusturmaTarihi,
                b.YayinTarihi
            })
            .ToListAsync();

        return Ok(Cevap<List<object>>.Basarili(yazilar.Cast<object>().ToList()));
    }

    [HttpGet("en-cok-okunan")]
    public async Task<IActionResult> EnCokOkunan()
    {
        var yazilar = await db.Haberler
            .Where(b => b.AktifMi)
            .OrderByDescending(b => b.OkunmaSayisi)
            .ThenByDescending(b => b.YayinTarihi ?? b.OlusturmaTarihi)
            .Take(5)
            .Select(b => new
            {
                b.Id,
                b.Baslik,
                b.Slug,
                b.Ozet,
                b.AnaResimUrl,
                Tarih = b.YayinTarihi ?? b.OlusturmaTarihi,
                b.Etiketler,
                b.OkunmaSayisi
            })
            .ToListAsync();

        return Ok(Cevap<List<object>>.Basarili(yazilar.Cast<object>().ToList()));
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> SlugIleDetayGetir(string slug)
    {
        var yazi = await db.Haberler
            .Include(b => b.Resimler)
            .FirstOrDefaultAsync(b => b.Slug == slug && b.AktifMi);

        if (yazi is null)
            return NotFound(Cevap<object>.Hata("Haber bulunamadi."));

        yazi.OkunmaSayisi++;
        await db.SaveChangesAsync();

        return Ok(Cevap<HaberYazisi>.Basarili(yazi));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> DetayGetir(int id)
    {
        var yazi = await db.Haberler
            .Include(b => b.Resimler)
            .FirstOrDefaultAsync(b => b.Id == id && b.AktifMi);

        if (yazi is null)
            return NotFound(Cevap<object>.Hata("Haber bulunamadi."));

        return Ok(Cevap<HaberYazisi>.Basarili(yazi));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Ekle([FromBody] HaberYazisi yazi)
    {
        yazi.Id = 0;
        yazi.OlusturmaTarihi = DateTime.UtcNow;
        yazi.AktifMi = true;
        db.Haberler.Add(yazi);
        await db.SaveChangesAsync();
        return Ok(Cevap<HaberYazisi>.Basarili(yazi, "Haber eklendi."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] HaberYazisi guncel)
    {
        var yazi = await db.Haberler.FindAsync(id);
        if (yazi is null)
            return NotFound(Cevap<HaberYazisi>.Hata("Haber bulunamadi."));

        yazi.Baslik = guncel.Baslik;
        yazi.Slug = guncel.Slug;
        yazi.Ozet = guncel.Ozet;
        yazi.Icerik = guncel.Icerik;
        yazi.AnaResimUrl = guncel.AnaResimUrl;
        yazi.SeoBaslik = guncel.SeoBaslik;
        yazi.SeoAciklama = guncel.SeoAciklama;
        yazi.Etiketler = guncel.Etiketler;
        yazi.AktifMi = guncel.AktifMi;
        yazi.YayinTarihi = guncel.YayinTarihi;

        await db.SaveChangesAsync();
        return Ok(Cevap<HaberYazisi>.Basarili(yazi, "Haber guncellendi."));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var yazi = await db.Haberler.FindAsync(id);
        if (yazi is null)
            return NotFound(Cevap<object>.Hata("Haber bulunamadi."));

        yazi.AktifMi = false;
        await db.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!, "Haber pasife alindi."));
    }
}



