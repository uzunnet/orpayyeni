using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.Servisler;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Kontrolculer;

[ApiController]
[Route("api/super-admin/firma")]
public class SuperAdminFirmaKontrolcu(SuperAdminDbContext vt, FirmaOlusturmaServisi firmaServisi) : ControllerBase
{
    /// <summary>Tüm firmaları listele.</summary>
    [HttpGet]
    public async Task<IActionResult> Listele()
    {
        var firmalar = await vt.Firmalar
            .OrderByDescending(f => f.OlusturulmaTarihi)
            .ToListAsync();
        return Ok(Cevap<List<Firma>>.Basarili(firmalar));
    }

    /// <summary>ID ile tek firma getir.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Getir(int id)
    {
        var firma = await vt.Firmalar.FindAsync(id);
        if (firma == null)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadi."));
        return Ok(Cevap<Firma>.Basarili(firma));
    }

    /// <summary>Yeni firma olustur — altyapi (DB + klasor + seed) dahil.</summary>
    [HttpPost]
    public async Task<IActionResult> Olustur([FromBody] FirmaOlusturDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug))
            return BadRequest(Cevap<bool>.Hata("Slug zorunludur."));

        // Slug benzersizlik kontrolu
        var mevcut = await vt.Firmalar.AnyAsync(f => f.Slug == dto.Slug);
        if (mevcut)
            return BadRequest(Cevap<bool>.Hata($"'{dto.Slug}' slug'i zaten kullaniliyor."));

        var firma = new Firma
        {
            Ad = dto.Ad ?? dto.Slug,
            Unvan = dto.Unvan ?? dto.Ad ?? dto.Slug,
            Slug = dto.Slug,
            Domain = dto.Domain,
            Eposta = dto.Eposta,
            Telefon1 = dto.Telefon1,
            Sektor = dto.Sektor,
            PaketTipi = dto.PaketTipi ?? "Standart",
            Adres = dto.Adres,
            Sehir = dto.Sehir,
            Ilce = dto.Ilce,
            MaxKullaniciSayisi = dto.MaxKullaniciSayisi > 0 ? dto.MaxKullaniciSayisi : 5,
            DemoMu = dto.DemoMu,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };

        vt.Firmalar.Add(firma);
        await vt.SaveChangesAsync();

        // Varsayilan modulleri ata
        var varsayilanModulIdleri = await vt.Moduller
            .Where(m => m.VarsayilanMi || m.SistemModuluMu)
            .Select(m => m.Id)
            .ToListAsync();

        foreach (var modulId in varsayilanModulIdleri)
        {
            vt.FirmaModulAtamalari.Add(new FirmaModulAtama
            {
                FirmaId = firma.Id,
                ModulId = modulId,
                AtanmaTarihi = DateTimeOffset.UtcNow
            });
        }
        await vt.SaveChangesAsync();

        // Klasor + VT + seed olustur
        var altyapiBasarili = await firmaServisi.FirmaAltyapisiniOlustur(firma.Slug, firma.Id, firma.Ad, firma.Domain ?? $"{firma.Slug}.com", dto.PaketTipi ?? "Yillik");

        if (!altyapiBasarili)
            return StatusCode(500, Cevap<bool>.Hata("Firma kaydedildi ancak altyapi olusturulamadi."));

        return Ok(Cevap<Firma>.Basarili(firma, "Firma basariyla olusturuldu."));
    }

    /// <summary>Firma bilgilerini guncelle.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] FirmaGuncelleDto dto)
    {
        var firma = await vt.Firmalar.FindAsync(id);
        if (firma == null)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadi."));

        if (!string.IsNullOrWhiteSpace(dto.Ad)) firma.Ad = dto.Ad;
        if (!string.IsNullOrWhiteSpace(dto.Unvan)) firma.Unvan = dto.Unvan;
        if (!string.IsNullOrWhiteSpace(dto.Domain)) firma.Domain = dto.Domain;
        if (!string.IsNullOrWhiteSpace(dto.Eposta)) firma.Eposta = dto.Eposta;
        if (!string.IsNullOrWhiteSpace(dto.Telefon1)) firma.Telefon1 = dto.Telefon1;
        if (!string.IsNullOrWhiteSpace(dto.Sektor)) firma.Sektor = dto.Sektor;
        if (!string.IsNullOrWhiteSpace(dto.PaketTipi)) firma.PaketTipi = dto.PaketTipi;
        if (!string.IsNullOrWhiteSpace(dto.Adres)) firma.Adres = dto.Adres;
        if (!string.IsNullOrWhiteSpace(dto.Sehir)) firma.Sehir = dto.Sehir;
        if (!string.IsNullOrWhiteSpace(dto.Ilce)) firma.Ilce = dto.Ilce;
        if (dto.MaxKullaniciSayisi > 0) firma.MaxKullaniciSayisi = dto.MaxKullaniciSayisi;
        if (dto.AktifMi.HasValue) firma.AktifMi = dto.AktifMi.Value;
        if (dto.DemoMu.HasValue) firma.DemoMu = dto.DemoMu.Value;

        firma.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<Firma>.Basarili(firma, "Firma basariyla guncellendi."));
    }

    /// <summary>Firmayi sil (soft delete: AktifMi=false).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var firma = await vt.Firmalar.FindAsync(id);
        if (firma == null)
            return NotFound(Cevap<bool>.Hata("Firma bulunamadi."));

        // Soft delete
        firma.AktifMi = false;
        firma.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<Firma>.Basarili(firma, "Firma pasife alindi (soft delete)."));
    }
}

// ── DTO'lar ──

public class FirmaOlusturDto
{
    public string? Ad { get; set; }
    public string? Unvan { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? Eposta { get; set; }
    public string? Telefon1 { get; set; }
    public string? Sektor { get; set; }
    public string? PaketTipi { get; set; }
    public string? Adres { get; set; }
    public string? Sehir { get; set; }
    public string? Ilce { get; set; }
    public int MaxKullaniciSayisi { get; set; } = 5;
    public bool DemoMu { get; set; }
}

public class FirmaGuncelleDto
{
    public string? Ad { get; set; }
    public string? Unvan { get; set; }
    public string? Domain { get; set; }
    public string? Eposta { get; set; }
    public string? Telefon1 { get; set; }
    public string? Sektor { get; set; }
    public string? PaketTipi { get; set; }
    public string? Adres { get; set; }
    public string? Sehir { get; set; }
    public string? Ilce { get; set; }
    public int MaxKullaniciSayisi { get; set; }
    public bool? AktifMi { get; set; }
    public bool? DemoMu { get; set; }
}

