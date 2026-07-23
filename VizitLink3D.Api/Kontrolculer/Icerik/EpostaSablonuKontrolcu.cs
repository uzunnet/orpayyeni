using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Icerik;

[ApiController]
[Route("api/eposta-sablonlari")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
public class EpostaSablonuKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> TumunuGetir()
    {
        var liste = await vt.EpostaSablonlari
            .Where(e => e.AktifMi)
            .OrderByDescending(e => e.OlusturulmaTarihi)
            .ToListAsync();
        return Ok(Cevap<List<EpostaSablonu>>.Basarili(liste));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Getir(int id)
    {
        var sablon = await vt.EpostaSablonlari.FindAsync(id);
        if (sablon == null || !sablon.AktifMi) return NotFound(Cevap<EpostaSablonu>.Hata("Şablon bulunamadı."));
        return Ok(Cevap<EpostaSablonu>.Basarili(sablon));
    }

    [HttpPost]
    public async Task<IActionResult> Ekle([FromBody] EpostaSablonu sablon)
    {
        sablon.OlusturulmaTarihi = DateTime.UtcNow;
        vt.EpostaSablonlari.Add(sablon);
        await vt.SaveChangesAsync();
        return Ok(Cevap<EpostaSablonu>.Basarili(sablon, "Şablon eklendi."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] EpostaSablonu sablon)
    {
        var mevcut = await vt.EpostaSablonlari.FindAsync(id);
        if (mevcut == null) return NotFound(Cevap<EpostaSablonu>.Hata("Şablon bulunamadı."));

        mevcut.Ad = sablon.Ad;
        mevcut.Konu = sablon.Konu;
        mevcut.IcerikHtml = sablon.IcerikHtml;
        mevcut.Tip = sablon.Tip;
        mevcut.AktifMi = sablon.AktifMi;

        await vt.SaveChangesAsync();
        return Ok(Cevap<EpostaSablonu>.Basarili(mevcut, "Şablon güncellendi."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var mevcut = await vt.EpostaSablonlari.FindAsync(id);
        if (mevcut == null || !mevcut.AktifMi) return NotFound(Cevap<bool>.Hata("Şablon bulunamadı."));

        mevcut.AktifMi = false;
        await vt.SaveChangesAsync();
        return Ok(Cevap<bool>.Basarili(true, "Şablon silindi."));
    }
}

