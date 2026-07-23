using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/firma")]
public class FirmaKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    /// <summary>Tüm aktif firmaları listele (admin yönetim için).</summary>
    [HttpGet]
    public async Task<IActionResult> ListAll()
    {
        var firmalar = await vt.Firmalar.Where(f => f.AktifMi).ToListAsync();
        return Ok(Cevap<List<Firma>>.Basarili(firmalar));
    }

    /// <summary>Firma bilgisini ID ile getir (SaaS - frontend dinamik tema).</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Id == id && f.AktifMi);
        if (firma == null)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));

        return Ok(Cevap<Firma>.Basarili(firma));
    }

    /// <summary>Slug ile firma bilgisini getir (domain tanıması).</summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Slug == slug && f.AktifMi);
        if (firma == null)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));

        return Ok(Cevap<Firma>.Basarili(firma));
    }

    /// <summary>Geçerli firma (middleware tarafından set edilen FirmaId; yoksa ilk aktif firma).</summary>
    [HttpGet("guncel")]
    public async Task<IActionResult> GetMevcut()
    {
        if (HttpContext.Items.TryGetValue("FirmaId", out var firmaId) && firmaId is int id)
        {
            var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Id == id && f.AktifMi);
            if (firma != null)
                return Ok(Cevap<Firma>.Basarili(firma));
        }

        // Middleware FirmaId set etmemisse ilk aktif firmayi don (tek-tenant varsayilani)
        var varsayilan = await vt.Firmalar.FirstOrDefaultAsync(f => f.AktifMi);
        if (varsayilan == null)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));

        return Ok(Cevap<Firma>.Basarili(varsayilan));
    }

    /// <summary>Firma bilgisini güncelle (admin yönetim - logo, tema, renk vb.).</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] FirmaGuncellemeDto dto)
    {
        if (dto == null)
            return BadRequest(Cevap<bool>.Hata("İstek verisi boş."));

        var firma = await vt.Firmalar.FirstOrDefaultAsync(f => f.Id == id);
        if (firma == null)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadı."));

        // Güncellenebilir alanlar
        if (!string.IsNullOrWhiteSpace(dto.Logo))
            firma.Logo = dto.Logo;
        if (!string.IsNullOrWhiteSpace(dto.Ad))
            firma.Ad = dto.Ad;
        if (!string.IsNullOrWhiteSpace(dto.Unvan))
            firma.Unvan = dto.Unvan;
        if (!string.IsNullOrWhiteSpace(dto.SiteTema))
            firma.SiteTema = dto.SiteTema;
        if (!string.IsNullOrWhiteSpace(dto.AdminTema))
            firma.AdminTema = dto.AdminTema;
        if (!string.IsNullOrWhiteSpace(dto.TasarimRengi1))
            firma.TasarimRengi1 = dto.TasarimRengi1;
        if (!string.IsNullOrWhiteSpace(dto.TasarimRengi2))
            firma.TasarimRengi2 = dto.TasarimRengi2;
        if (!string.IsNullOrWhiteSpace(dto.TasarimRengi3))
            firma.TasarimRengi3 = dto.TasarimRengi3;

        firma.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<Firma>.Basarili(firma, "Firma başarıyla güncellendi."));
    }
}

public class FirmaGuncellemeDto
{
    public string? Logo { get; set; }
    public string? Ad { get; set; }
    public string? Unvan { get; set; }
    public string? SiteTema { get; set; }
    public string? AdminTema { get; set; }
    public string? TasarimRengi1 { get; set; }
    public string? TasarimRengi2 { get; set; }
    public string? TasarimRengi3 { get; set; }
}
