using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Icerik;

/// <summary>
/// Projeler icin API endpoint'i.
/// </summary>
[ApiController]
[Route("api/projeler")]
public class ProjeKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listele([FromQuery] int? kategoriId)
    {
        var sorgu = vt.Projeler
            .Include(p => p.Resimler)
            .Where(p => p.AktifMi)
            .AsQueryable();

        if (kategoriId.HasValue)
            sorgu = sorgu.Where(p => p.KategoriId == kategoriId.Value);

        var liste = await sorgu.OrderBy(p => p.SiraNo).ToListAsync();
        return Ok(Cevap<List<Proje>>.Basarili(liste));
    }

    [HttpGet("en-cok-incelenen")]
    public async Task<IActionResult> EnCokIncelenenler()
    {
        var liste = await vt.Projeler
            .Include(p => p.Resimler)
            .Where(p => p.AktifMi)
            .OrderByDescending(p => p.OneCikanMi)
            .ThenBy(p => p.SiraNo)
            .Take(5)
            .ToListAsync();

        return Ok(Cevap<List<Proje>>.Basarili(liste));
    }

    [HttpGet("one-cikan")]
    public async Task<IActionResult> OneCikanlar()
    {
        var liste = await vt.Projeler
            .Include(p => p.Resimler)
            .Where(p => p.AktifMi && p.OneCikanMi)
            .OrderBy(p => p.SiraNo)
            .Take(6)
            .ToListAsync();

        return Ok(Cevap<List<Proje>>.Basarili(liste));
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> SlugIleDetay(string slug)
    {
        var proje = await vt.Projeler
            .Include(p => p.Resimler)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.AktifMi);

        if (proje is null) return NotFound(Cevap<Proje>.Hata("Proje bulunamadi."));
        return Ok(Cevap<Proje>.Basarili(proje));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detay(int id)
    {
        var proje = await vt.Projeler
            .Include(p => p.Resimler)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proje is null) return NotFound(Cevap<Proje>.Hata("Proje bulunamadi."));
        return Ok(Cevap<Proje>.Basarili(proje));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Guncelle(int id, Proje g)
    {
        var p = await vt.Projeler.FindAsync(id);
        if (p is null) return NotFound(Cevap<Proje>.Hata("Bulunamadi."));
        p.Baslik = g.Baslik; p.KisaAciklama = g.KisaAciklama; p.MusteriAdi = g.MusteriAdi;
        p.MusteriSehir = g.MusteriSehir; p.SiraNo = g.SiraNo; p.OneCikanMi = g.OneCikanMi;
        p.AktifMi = g.AktifMi; p.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Ok(Cevap<Proje>.Basarili(p));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Sil(int id)
    {
        var p = await vt.Projeler.FindAsync(id);
        if (p is null) return NotFound(Cevap<Proje>.Hata("Bulunamadi."));
        p.AktifMi = false;
        p.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!));
    }
}

