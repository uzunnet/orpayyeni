using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Kontrolculer;

[ApiController]
[Route("api/super-admin/lisans")]
public class SuperAdminLisansKontrolcusu(SuperAdminDbContext vt) : ControllerBase
{
    /// <summary>Tum lisanslari sayfali olarak listeler.</summary>
    [HttpGet]
    public async Task<IActionResult> Listele([FromQuery] int sayfa = 1, [FromQuery] int sayfaBoyutu = 20)
    {
        var toplam = await vt.SuperAdminLisansKayitlari.CountAsync();

        var lisanslar = await vt.SuperAdminLisansKayitlari
            .AsNoTracking()
            .ToListAsync();
        lisanslar = lisanslar.OrderByDescending(l => l.OlusturulmaTarihi)
            .Skip((sayfa - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .ToList();

        return Ok(Cevap<object>.Basarili(new
        {
            Veri = lisanslar,
            Sayfa = sayfa,
            SayfaBoyutu = sayfaBoyutu,
            ToplamKayit = toplam,
            ToplamSayfa = (int)Math.Ceiling((double)toplam / sayfaBoyutu)
        }));
    }

    /// <summary>ID ile tek lisans getir.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Getir(int id)
    {
        var lisans = await vt.SuperAdminLisansKayitlari.FindAsync(id);
        if (lisans is null)
            return NotFound(Cevap<bool>.Hata("Lisans bulunamadi."));

        return Ok(Cevap<SuperAdminLisansKaydi>.Basarili(lisans));
    }

    /// <summary>Yeni lisans olustur.</summary>
    [HttpPost]
    public async Task<IActionResult> Olustur([FromBody] LisansOlusturDto dto)
    {
        var firmaVarMi = await vt.Firmalar.AnyAsync(f => f.Id == dto.FirmaId);
        if (!firmaVarMi)
            return BadRequest(Cevap<bool>.Hata("Firma bulunamadi."));

        var lisans = new SuperAdminLisansKaydi
        {
            FirmaId = dto.FirmaId,
            Domain = dto.Domain,
            Tip = dto.Tip,
            BaslangicTarihi = dto.BaslangicTarihi,
            BitisTarihi = dto.BitisTarihi,
            AktifMi = true,
            Aciklama = dto.Aciklama,
            OlusturulmaTarihi = DateTimeOffset.UtcNow
        };

        vt.SuperAdminLisansKayitlari.Add(lisans);
        await vt.SaveChangesAsync();

        return Ok(Cevap<SuperAdminLisansKaydi>.Basarili(lisans, "Lisans basariyla olusturuldu."));
    }

    /// <summary>Lisans bilgilerini guncelle.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] LisansGuncelleDto dto)
    {
        var lisans = await vt.SuperAdminLisansKayitlari.FindAsync(id);
        if (lisans is null)
            return NotFound(Cevap<bool>.Hata("Lisans bulunamadi."));

        if (!string.IsNullOrWhiteSpace(dto.Domain)) lisans.Domain = dto.Domain;
        if (!string.IsNullOrWhiteSpace(dto.Tip)) lisans.Tip = dto.Tip;
        if (dto.BaslangicTarihi.HasValue) lisans.BaslangicTarihi = dto.BaslangicTarihi.Value;
        if (dto.BitisTarihi.HasValue) lisans.BitisTarihi = dto.BitisTarihi.Value;
        if (dto.AktifMi.HasValue) lisans.AktifMi = dto.AktifMi.Value;
        if (dto.Aciklama is not null) lisans.Aciklama = dto.Aciklama;

        lisans.GuncellenmeTarihi = DateTimeOffset.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<SuperAdminLisansKaydi>.Basarili(lisans, "Lisans basariyla guncellendi."));
    }

    /// <summary>Lisansi sil (soft delete: AktifMi=false).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var lisans = await vt.SuperAdminLisansKayitlari.FindAsync(id);
        if (lisans is null)
            return NotFound(Cevap<bool>.Hata("Lisans bulunamadi."));

        lisans.AktifMi = false;
        lisans.GuncellenmeTarihi = DateTimeOffset.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<SuperAdminLisansKaydi>.Basarili(lisans, "Lisans pasife alindi (soft delete)."));
    }
}

// ── DTO'lar ──

public class LisansOlusturDto
{
    public int FirmaId { get; set; }
    public string? Domain { get; set; }
    public string Tip { get; set; } = string.Empty;
    public DateTimeOffset BaslangicTarihi { get; set; }
    public DateTimeOffset BitisTarihi { get; set; }
    public string? Aciklama { get; set; }
}

public class LisansGuncelleDto
{
    public string? Domain { get; set; }
    public string? Tip { get; set; }
    public DateTimeOffset? BaslangicTarihi { get; set; }
    public DateTimeOffset? BitisTarihi { get; set; }
    public bool? AktifMi { get; set; }
    public string? Aciklama { get; set; }
}
