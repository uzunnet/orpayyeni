using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Pazarlama;

/// <summary>
/// Hero slider slaytlari icin API endpoint'i.
/// Herkese aciktir � anonim erisime izin verilir.
/// </summary>
[ApiController]
[Route("api")]
public class SlaytKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet("slaytlar")]
    public async Task<IActionResult> SlaytlariGetir([FromQuery] string dil = "tr", [FromQuery] string sayfaKodu = "anasayfa")
    {
        var slaytlar = await vt.Slaytlar
            .Where(s => s.AktifMi && !s.SilindiMi && s.Dil == dil && s.SayfaKodu == sayfaKodu)
            .Where(s => s.BaslangicTarihi == null || s.BaslangicTarihi <= DateTime.UtcNow)
            .Where(s => s.BitisTarihi == null || s.BitisTarihi >= DateTime.UtcNow)
            .OrderBy(s => s.SiraNo)
            .ToListAsync();

        return Ok(Cevap<List<Slayt>>.Basarili(slaytlar));
    }
}



