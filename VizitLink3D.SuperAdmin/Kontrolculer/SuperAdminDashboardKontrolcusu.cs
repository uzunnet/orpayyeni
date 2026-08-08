using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Kontrolculer;

[ApiController]
[Route("api/super-admin/dashboard")]
public class SuperAdminDashboardKontrolcusu(SuperAdminDbContext vt) : ControllerBase
{
    /// <summary>Dashboard ozet verilerini getir.</summary>
    [HttpGet]
    public async Task<IActionResult> Getir()
    {
        var toplamFirma = await vt.Firmalar.CountAsync();
        var aktifFirma = await vt.Firmalar.CountAsync(f => f.AktifMi);
        var demoFirma = await vt.Firmalar.CountAsync(f => f.DemoMu);
        var toplamModul = await vt.Moduller.CountAsync();

        // Lisans ozeti
        var simdi = DateTimeOffset.UtcNow;
        var otuzGunSona = simdi.AddDays(30);
        var tumLisanslar = await vt.SuperAdminLisansKayitlari.AsNoTracking().ToListAsync();
        var suresiDolmakUzere = tumLisanslar.Count(l => l.AktifMi && l.BitisTarihi > simdi && l.BitisTarihi <= otuzGunSona);
        var suresiDolmus = tumLisanslar.Count(l => l.AktifMi && l.BitisTarihi <= simdi);

        // Son 5 firma
        var sonFirmalar = await vt.Firmalar
            .OrderByDescending(f => f.OlusturulmaTarihi)
            .Take(5)
            .Select(f => new SonFirmaDto
            {
                Id = f.Id,
                Ad = f.Ad,
                Slug = f.Slug,
                Domain = f.Domain,
                AktifMi = f.AktifMi,
                DemoMu = f.DemoMu,
                OlusturulmaTarihi = f.OlusturulmaTarihi
            })
            .ToListAsync();

        var dashboard = new DashboardDto
        {
            ToplamFirma = toplamFirma,
            AktifFirma = aktifFirma,
            DemoFirma = demoFirma,
            ToplamModul = toplamModul,
            LisansOzet = new LisansOzetDto
            {
                SuresiDolmakUzere = suresiDolmakUzere,
                SuresiDolmus = suresiDolmus
            },
            SonFirmalar = sonFirmalar
        };

        return Ok(Cevap<DashboardDto>.Basarili(dashboard));
    }
}

// ── DTO'lar ──

public class DashboardDto
{
    public int ToplamFirma { get; set; }
    public int AktifFirma { get; set; }
    public int DemoFirma { get; set; }
    public int ToplamModul { get; set; }
    public LisansOzetDto LisansOzet { get; set; } = new();
    public List<SonFirmaDto> SonFirmalar { get; set; } = [];
}

public class LisansOzetDto
{
    public int SuresiDolmakUzere { get; set; }
    public int SuresiDolmus { get; set; }
}

public class SonFirmaDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public bool AktifMi { get; set; }
    public bool DemoMu { get; set; }
    public DateTime OlusturulmaTarihi { get; set; }
}
