using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Kontrolculer;

[ApiController]
[Route("api/super-admin")]
public class SuperAdminModulKontrolcusu(SuperAdminDbContext vt) : ControllerBase
{
    /// <summary>Tum modulleri listeler.</summary>
    [HttpGet("modul")]
    public async Task<IActionResult> ModulleriListele()
    {
        var moduller = await vt.Moduller
            .OrderBy(m => m.Kategori)
            .ThenBy(m => m.Ad)
            .ToListAsync();

        return Ok(Cevap<List<Modul>>.Basarili(moduller));
    }

    /// <summary>Belirli bir firmanin atanan modullerini getir.</summary>
    [HttpGet("firma/{firmaId:int}/modul")]
    public async Task<IActionResult> FirmaModulleriGetir(int firmaId)
    {
        var firmaVarMi = await vt.Firmalar.AnyAsync(f => f.Id == firmaId);
        if (!firmaVarMi)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadi."));

        var atananModuller = await vt.FirmaModulAtamalari
            .Where(a => a.FirmaId == firmaId)
            .Include(a => a.Modul)
            .Select(a => new FirmaModulDto
            {
                ModulId = a.ModulId,
                ModulKodu = a.Modul!.Kod,
                ModulAdi = a.Modul.Ad,
                Kategori = a.Modul.Kategori,
                AtanmaTarihi = a.AtanmaTarihi
            })
            .ToListAsync();

        return Ok(Cevap<List<FirmaModulDto>>.Basarili(atananModuller));
    }

    /// <summary>Firmanin modullerini toplu olarak guncelle (eski atamalari siler, yenisini ekler).</summary>
    [HttpPost("firma/{firmaId:int}/modul")]
    public async Task<IActionResult> FirmaModulleriniGuncelle(int firmaId, [FromBody] FirmaModulGuncelleDto dto)
    {
        var firmaVarMi = await vt.Firmalar.AnyAsync(f => f.Id == firmaId);
        if (!firmaVarMi)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadi."));

        // Gecerli modul ID'leri kontrolu
        var gecerliModulIdleri = await vt.Moduller.Select(m => m.Id).ToListAsync();
        var gecersizIdler = dto.ModulIdleri.Where(id => !gecerliModulIdleri.Contains(id)).ToList();
        if (gecersizIdler.Count > 0)
            return BadRequest(Cevap<bool>.Hata($"Gecersiz modul ID'leri: {string.Join(", ", gecersizIdler)}"));

        // Mevcut atamalari sil
        var mevcutAtamalar = await vt.FirmaModulAtamalari
            .Where(a => a.FirmaId == firmaId)
            .ToListAsync();

        vt.FirmaModulAtamalari.RemoveRange(mevcutAtamalar);

        // Yeni atamalari ekle
        var yeniAtamalar = dto.ModulIdleri.Select(modulId => new FirmaModulAtama
        {
            FirmaId = firmaId,
            ModulId = modulId,
            AtanmaTarihi = DateTimeOffset.UtcNow
        }).ToList();

        vt.FirmaModulAtamalari.AddRange(yeniAtamalar);
        await vt.SaveChangesAsync();

        return Ok(Cevap<int>.Basarili(yeniAtamalar.Count, "Firma modulleri basariyla guncellendi."));
    }
}

// ── DTO'lar ──

public class FirmaModulDto
{
    public int ModulId { get; set; }
    public string ModulKodu { get; set; } = string.Empty;
    public string ModulAdi { get; set; } = string.Empty;
    public string? Kategori { get; set; }
    public DateTimeOffset AtanmaTarihi { get; set; }
}

public class FirmaModulGuncelleDto
{
    public List<int> ModulIdleri { get; set; } = [];
}
