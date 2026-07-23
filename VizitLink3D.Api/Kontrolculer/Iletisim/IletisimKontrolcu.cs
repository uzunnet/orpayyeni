using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Kontrolculer.Iletisim;

/// <summary>
/// IletisimKontrolcu - iletişim formu mesajlarını yöneten kontrolcü.
/// POST /api/iletisim - yeni mesajı veritabanına kaydeder.
/// GET /api/iletisim/mesajlar - admin mesaj listesini alır.
/// PATCH /api/iletisim/mesajlar/{id}/okundu - admin okundu işaretler.
/// DELETE /api/iletisim/mesajlar/{id} - admin arşivler.
/// </summary>
[ApiController]
[Route("api/iletisim")]
public class IletisimKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("Genel")]
    public async Task<IActionResult> MesajKaydet([FromBody] IletisimMesajiGiris giris)
    {
        if (string.IsNullOrWhiteSpace(giris.AdSoyad) || string.IsNullOrWhiteSpace(giris.Email) || string.IsNullOrWhiteSpace(giris.Mesaj))
            return BadRequest(new { BasariliMi = false, Mesaj = "Zorunlu alanlar eksik." });

        if (giris.AdSoyad.Length > 200 || giris.Email.Length > 254 || (giris.Telefon?.Length ?? 0) > 50 || (giris.Konu?.Length ?? 0) > 2000 || giris.Mesaj.Length > 8000)
            return BadRequest(new { BasariliMi = false, Mesaj = "Gonderim cok buyuk." });

        var yeniMesaj = new IletisimMesaji
        {
            AdSoyad = giris.AdSoyad.Trim(),
            Eposta = giris.Email.Trim(),
            Telefon = giris.Telefon?.Trim() ?? string.Empty,
            Konu = giris.Konu?.Trim() ?? string.Empty,
            Mesaj = giris.Mesaj.Trim(),
            Tarih = DateTime.UtcNow,
            OkunduMu = false
        };

        vt.IletisimMesajlari.Add(yeniMesaj);
        await vt.SaveChangesAsync();

        return Ok(new { BasariliMi = true, Mesaj = "Mesajiniz alindi. En kisa surede donus yapacagiz." });
    }

    [HttpGet("mesajlar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> MesajlariniGetir([FromQuery] bool? okundu = null, [FromQuery] int sayfa = 1, [FromQuery] int sayfaBoyutu = 20)
    {
        var sorgu = vt.IletisimMesajlari
            .Where(m => !m.CevaplandiMi)
            .AsQueryable();

        if (okundu.HasValue)
            sorgu = sorgu.Where(m => m.OkunduMu == okundu.Value);

        var toplam = await sorgu.CountAsync();
        var mesajlar = await sorgu
            .OrderByDescending(m => m.Tarih)
            .Skip((sayfa - 1) * sayfaBoyutu)
            .Take(sayfaBoyutu)
            .Select(m => new
            {
                m.Id, m.AdSoyad, Eposta = m.Eposta, m.Telefon, m.Konu,
                m.Mesaj, Tarih = m.Tarih, m.OkunduMu
            })
            .ToListAsync();

        return Ok(new { BasariliMi = true, Veri = new { Toplam = toplam, Mesajlar = mesajlar } });
    }

    [HttpPatch("mesajlar/{id:int}/okundu")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> OkunduIsaretle(int id)
    {
        var mesaj = await vt.IletisimMesajlari.FindAsync(id);
        if (mesaj is null) return NotFound();
        mesaj.OkunduMu = true;
        await vt.SaveChangesAsync();
        return Ok(new { BasariliMi = true });
    }

    [HttpDelete("mesajlar/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> MesajiSil(int id)
    {
        var mesaj = await vt.IletisimMesajlari.FindAsync(id);
        if (mesaj is null) return NotFound();
        mesaj.CevaplandiMi = true;
        await vt.SaveChangesAsync();
        return Ok(new { BasariliMi = true });
    }
}

public record IletisimMesajiGiris(
    string AdSoyad,
    string Email,
    string? Telefon,
    string? Konu,
    string Mesaj
);
