using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/denetim-log")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DenetimLogKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listele([FromQuery] int sayfa = 1, [FromQuery] int sayfaBoyutu = 20, [FromQuery] string? ara = null)
    {
        var sorgu = vt.AuditLoglar.AsQueryable();

        if (!string.IsNullOrWhiteSpace(ara))
            sorgu = sorgu.Where(a => a.Eylem.Contains(ara) || (a.KullaniciId != null && a.KullaniciId.Contains(ara)));

        var toplam = await sorgu.CountAsync();
        var liste = await sorgu
            .OrderByDescending(a => a.ZamanDamgasi)
            .Skip((sayfa - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .Select(a => new
            {
                a.Id,
                a.ZamanDamgasi,
                a.KullaniciId,
                a.Eylem,
                a.FirmaId
            })
            .ToListAsync();

        return Ok(Cevap<object>.Basarili(new { liste, toplam, sayfa, sayfaBoyutu })); 
    }
}
