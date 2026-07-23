using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace VizitLink3D.Api.Kontrolculer.Icerik;

[ApiController]
[Route("api/pdf-katalog")]
[Authorize]
public class PdfKatalogKontrolcu(VizitLink3DDbContext vt, IServiceProvider serviceProvider) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Liste()
    {
        var liste = await vt.UrunPdfKaynaklari
            .OrderByDescending(k => k.OlusturulmaTarihi)
            .ToListAsync();
        return Ok(Cevap<List<UrunPdfKaynagi>>.Basarili(liste));
    }

    [HttpGet("{id:int}/sayfalar")]
    [AllowAnonymous]
    public async Task<IActionResult> Sayfalar(int id)
    {
        var sayfalar = await vt.PdfSayfaGorselleri
            .Where(s => s.PdfKaynagiId == id)
            .OrderBy(s => s.SayfaNo)
            .ToListAsync();
        return Ok(Cevap<List<PdfSayfaGorseli>>.Basarili(sayfalar));
    }

    [HttpPost("yukle")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Yukle([FromBody] UrunPdfKaynagiDto veriDto)
    {
        var yeni = new UrunPdfKaynagi
        {
            Ad = veriDto.Ad,
            MedyaId = veriDto.MedyaId,
            CozumlemeDurumu = "Bekliyor",
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.UrunPdfKaynaklari.Add(yeni);
        await vt.SaveChangesAsync();
        return Ok(Cevap<UrunPdfKaynagi>.Basarili(yeni));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] UrunPdfKaynagiGuncelleDto veriDto)
    {
        var mevcut = await vt.UrunPdfKaynaklari.FindAsync(id);
        if (mevcut == null) return NotFound();

        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        mevcut.Ad = veriDto.Ad;
        mevcut.MedyaId = veriDto.MedyaId;
        if (veriDto.SayfaSayisi.HasValue) mevcut.SayfaSayisi = veriDto.SayfaSayisi.Value;
        if (!string.IsNullOrEmpty(veriDto.CozumlemeDurumu)) mevcut.CozumlemeDurumu = veriDto.CozumlemeDurumu;

        vt.UrunPdfKaynaklari.Update(mevcut);
        await vt.SaveChangesAsync();
        return Ok(Cevap<UrunPdfKaynagi>.Basarili(mevcut));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Sil(int id)
    {
        var mevcut = await vt.UrunPdfKaynaklari.FindAsync(id);
        if (mevcut == null) return NotFound();

        var sayfalar = await vt.PdfSayfaGorselleri
            .Where(s => s.PdfKaynagiId == id)
            .ToListAsync();

        foreach (var sayfa in sayfalar)
        {
            var medya = await vt.Medyalar
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == sayfa.MedyaId);
            if (medya != null)
            {
                medya.SilindiMi = true;
                medya.SilinmeTarihi = DateTime.UtcNow;
            }

            sayfa.SilindiMi = true;
            sayfa.SilinmeTarihi = DateTime.UtcNow;
            sayfa.GuncellenmeTarihi = DateTime.UtcNow;
        }

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!));
    }

    [HttpPost("{id:int}/cozumle")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Cozumle(int id)
    {
        var mevcut = await vt.UrunPdfKaynaklari.FindAsync(id);
        if (mevcut == null)
            return NotFound(Cevap<object>.Hata("Kayıt bulunamadı."));

        _ = Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            var servis = scope.ServiceProvider.GetRequiredService<PdfCozumlemeServisi>();
            await servis.CozumleAsync(id);
        });

        return Ok(Cevap<bool>.Basarili(true, "Çözümleme işlemi arka planda başlatıldı."));
    }
}

public class UrunPdfKaynagiDto
{
    public string Ad { get; set; } = string.Empty;
    public long MedyaId { get; set; }
    public string? Aciklama { get; set; }
}

public class UrunPdfKaynagiGuncelleDto
{
    public string Ad { get; set; } = string.Empty;
    public long MedyaId { get; set; }
    public string? Aciklama { get; set; }
    public int? SayfaSayisi { get; set; }
    public string? CozumlemeDurumu { get; set; }
}

