using VizitLink3D.Api.Moduller.Tema.Servisler;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/firma-tema")]
public class FirmaTemaKontrolcu(
    VizitLink3DDbContext vt,
    KiraciServisi kiraci,
    CokluTemaServisi cokluTemaServisi) : ControllerBase
{
    private static readonly HashSet<string> GecerliAdminTemalari = new(StringComparer.OrdinalIgnoreCase)
    {
        "endustri-karanlik",
        "klasik-aydinlik",
        "altin-siyah",
        "modern-gri",
        "komuta-mavi",
        "windows-11"
    };

    [HttpGet]
    public async Task<IActionResult> AktifFirmaTemasiniGetir([FromQuery] int? firmaId)
    {
        if (!FirmaSecmeYetkisiVar(firmaId))
        {
            return Forbid();
        }

        var firma = await FirmaGetirAsync(firmaId);
        if (firma is null)
        {
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));
        }

        return Ok(Cevap<FirmaTemaDto>.Basarili(FirmaTemaOlustur(firma)));
    }

    [HttpGet("firmalar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> FirmaSecimleriGetir()
    {
        if (!User.IsInRole("SuperAdmin"))
        {
            var aktifFirma = await AktifFirmaGetirAsync();
            List<FirmaTemaSecimDto> tekFirma = aktifFirma is null
                ? []
                : new List<FirmaTemaSecimDto> { FirmaTemaSecimOlustur(aktifFirma) };

            return Ok(Cevap<List<FirmaTemaSecimDto>>.Basarili(tekFirma));
        }

        var firmaKayitlari = await vt.Firmalar
            .AsNoTracking()
            .Where(f => f.AktifMi)
            .OrderBy(f => f.Ad)
            .ToListAsync();
        var firmalar = firmaKayitlari.Select(FirmaTemaSecimOlustur).ToList();

        return Ok(Cevap<List<FirmaTemaSecimDto>>.Basarili(firmalar));
    }

    [HttpPut]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> AktifFirmaTemasiniGuncelle([FromBody] FirmaTemaGuncelleDto istek)
    {
        if (!FirmaSecmeYetkisiVar(istek.FirmaId))
        {
            return Forbid();
        }

        var adminTema = AdminTemaDogrula(istek.AdminTema);
        var siteTema = await TemaDogrulaAsync(istek.SiteTema);

        if (siteTema is null)
        {
            return BadRequest(Cevap<bool>.Hata("Site teması geçersiz veya tema bulunamadı."));
        }

        var firma = await FirmaGetirAsync(istek.FirmaId);
        if (firma is null)
        {
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));
        }

        if (adminTema is not null)
            firma.AdminTema = adminTema;
        firma.SiteTema = siteTema;
        firma.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();

        return Ok(Cevap<FirmaTemaDto>.Basarili(FirmaTemaOlustur(firma), "Firma teması güncellendi."));
    }

    private async Task<Firma?> AktifFirmaGetirAsync()
    {
        if (kiraci.MevcutFirmaId is int firmaId)
        {
            var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Id == firmaId && f.AktifMi);
            if (firma is not null)
            {
                return firma;
            }
        }

        return await vt.Firmalar
            .OrderByDescending(f => f.Slug == "orpay")
            .ThenBy(f => f.Id)
            .FirstOrDefaultAsync(f => f.AktifMi);
    }

    private async Task<Firma?> FirmaGetirAsync(int? firmaId)
    {
        if (firmaId is int id)
        {
            return await vt.Firmalar.FirstOrDefaultAsync(f => f.Id == id && f.AktifMi);
        }

        return await AktifFirmaGetirAsync();
    }

    private bool FirmaSecmeYetkisiVar(int? firmaId)
    {
        if (firmaId is null || User.IsInRole("SuperAdmin"))
        {
            return true;
        }

        return kiraci.MevcutFirmaId == firmaId;
    }

    private static FirmaTemaDto FirmaTemaOlustur(Firma firma)
    {
        return new FirmaTemaDto(
            firma.Id,
            firma.Slug,
            firma.Ad,
            firma.AdminTema ?? CokluTemaServisi.VARSAYILAN_ADMIN_TEMA,
            firma.SiteTema ?? CokluTemaServisi.VARSAYILAN_TEMA,
            firma.TasarimRengi1,
            firma.TasarimRengi2,
            firma.TasarimRengi3);
    }

    private static FirmaTemaSecimDto FirmaTemaSecimOlustur(Firma firma)
    {
        return new FirmaTemaSecimDto(
            firma.Id,
            firma.Slug,
            firma.Ad,
            firma.AdminTema ?? CokluTemaServisi.VARSAYILAN_ADMIN_TEMA,
            firma.SiteTema ?? CokluTemaServisi.VARSAYILAN_TEMA);
    }

    private async Task<string?> TemaDogrulaAsync(string? temaSlug)
    {
        if (string.IsNullOrWhiteSpace(temaSlug))
            return null;

        var temiz = temaSlug.Trim().ToLowerInvariant();
        var varMi = await cokluTemaServisi.TemaMevcutMuAsync(temiz, VizitLink3D.Ortak.Modeller.Tema.TemaKapsam.Sadece_Site);
        return varMi ? temiz : null;
    }

    private static string? AdminTemaDogrula(string? temaSlug)
    {
        if (string.IsNullOrWhiteSpace(temaSlug))
        {
            return null;
        }

        var temiz = temaSlug.Trim().ToLowerInvariant();
        return GecerliAdminTemalari.Contains(temiz) ? temiz : null;
    }
}

public sealed record FirmaTemaDto(
    int FirmaId,
    string Slug,
    string Ad,
    string AdminTema,
    string SiteTema,
    string? TasarimRengi1,
    string? TasarimRengi2,
    string? TasarimRengi3);

public sealed record FirmaTemaSecimDto(
    int FirmaId,
    string Slug,
    string Ad,
    string AdminTema,
    string SiteTema);

public sealed class FirmaTemaGuncelleDto
{
    public int? FirmaId { get; set; }
    public string? AdminTema { get; set; }
    public string SiteTema { get; set; } = string.Empty;
}
