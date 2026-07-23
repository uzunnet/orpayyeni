using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Icerik;

/// <summary>
/// Kapi kategorileri icin API endpoint'i.
/// Herkese acik okuma, admin yetkili yazma.
/// </summary>
[ApiController]
[Route("api/kapi-kategorileri")]
public class KapiKategorisiKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listele()
    {
        var liste = await vt.KapiKategorileri
            .Where(k => k.AktifMi)
            .OrderBy(k => k.SiraNo)
            .ToListAsync();

        return Ok(Cevap<List<KapiKategorisi>>.Basarili(liste));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detay(int id)
    {
        var kategori = await vt.KapiKategorileri.FindAsync(id);
        if (kategori is null) return NotFound(Cevap<KapiKategorisi>.Hata("Kategori bulunamadi."));
        return Ok(Cevap<KapiKategorisi>.Basarili(kategori));
    }
}

