using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/galeri-gorselleri")]
public class GaleriKontrolcu(VizitLink3DDbContext vt, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listele()
    {
        var urlListesi = await vt.GaleriGorselleri
            .Where(g => g.AktifMi)
            .OrderBy(g => g.Sira)
            .Select(g => g.Url)
            .ToListAsync();

        return Ok(Cevap<List<string>>.Basarili(urlListesi));
    }

    [HttpGet("detay")]
    public async Task<IActionResult> DetayListele()
    {
        var liste = await vt.GaleriGorselleri
            .Where(g => g.AktifMi)
            .OrderBy(g => g.Sira)
            .ToListAsync();

        return Ok(Cevap<List<GaleriGorseli>>.Basarili(liste));
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Ekle([FromBody] GaleriGorseli gorsel)
    {
        gorsel.OlusturulmaTarihi = DateTime.UtcNow;
        gorsel.AktifMi = true;
        vt.GaleriGorselleri.Add(gorsel);
        await vt.SaveChangesAsync();
        return Ok(Cevap<GaleriGorseli>.Basarili(gorsel, "Galeri gorseli eklendi."));
    }

    [HttpPost("yukle")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DosyaYukle(IFormFile dosya)
    {
        if (dosya is null || dosya.Length == 0)
            return BadRequest(Cevap<GaleriGorseli>.Hata("Dosya secilmedi."));

        var dosyaAdi = $"{Guid.NewGuid()}{Path.GetExtension(dosya.FileName)}";
        var dizin = Path.Combine(env.WebRootPath, "medya", "galeri");

        if (!Directory.Exists(dizin))
            Directory.CreateDirectory(dizin);

        var dosyaYolu = Path.Combine(dizin, dosyaAdi);

        using (var stream = new FileStream(dosyaYolu, FileMode.Create))
        {
            await dosya.CopyToAsync(stream);
        }

        var baslik = Path.GetFileNameWithoutExtension(dosya.FileName);
        var yeniGorsel = new GaleriGorseli
        {
            Url = $"/medya/galeri/{dosyaAdi}",
            Baslik = baslik,
            AltMetin = baslik,
            OlusturulmaTarihi = DateTime.UtcNow,
            AktifMi = true
        };

        vt.GaleriGorselleri.Add(yeniGorsel);
        await vt.SaveChangesAsync();

        return Ok(Cevap<GaleriGorseli>.Basarili(yeniGorsel, "Dosya yuklendi."));
    }

    [HttpPut("{id:int}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] GaleriGorseli guncel)
    {
        var mevcut = await vt.GaleriGorselleri.FirstOrDefaultAsync(g => g.Id == id && g.AktifMi);
        if (mevcut is null)
            return NotFound(Cevap<GaleriGorseli>.Hata("Gorsel bulunamadi."));

        mevcut.Baslik = guncel.Baslik;
        mevcut.AltMetin = guncel.AltMetin;
        mevcut.Sira = guncel.Sira;
        mevcut.AktifMi = guncel.AktifMi;

        await vt.SaveChangesAsync();
        return Ok(Cevap<GaleriGorseli>.Basarili(mevcut, "Gorsel bilgileri guncellendi."));
    }

    [HttpDelete("{id:int}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Sil(int id)
    {
        var mevcut = await vt.GaleriGorselleri.FindAsync(id);
        if (mevcut is null)
            return NotFound(Cevap<object>.Hata("Gorsel bulunamadi."));

        mevcut.AktifMi = false;
        await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!, "Gorsel pasife alindi."));
    }
}


