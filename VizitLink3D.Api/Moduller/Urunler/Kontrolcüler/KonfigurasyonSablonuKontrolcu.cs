using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/urunler")]
public class KonfigurasyonSablonuKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet("{urunId:int}/konfigurasyon-sablonu")]
    public async Task<Cevap<UrunKonfigurasyonSablonu>> UrunIcinGetir(int urunId)
    {
        var sablon = await vt.UrunKonfigurasyonSablonlari
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UrunId == urunId && s.AktifMi);

        if (sablon is null)
            return Cevap<UrunKonfigurasyonSablonu>.Hata("Sablon bulunamadi.");

        return Cevap<UrunKonfigurasyonSablonu>.Basarili(sablon);
    }

    [HttpPost("konfigurasyon-sablonu")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunKonfigurasyonSablonu>> Olustur([FromBody] UrunKonfigurasyonSablonu sablon)
    {
        sablon.OlusturulmaTarihi = DateTime.UtcNow;
        vt.UrunKonfigurasyonSablonlari.Add(sablon);
        await vt.SaveChangesAsync();

        return Cevap<UrunKonfigurasyonSablonu>.Basarili(sablon, "Konfigurasyon sablonu olusturuldu.");
    }

    [HttpPut("konfigurasyon-sablonu/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunKonfigurasyonSablonu>> Guncelle(int id, [FromBody] UrunKonfigurasyonSablonu sablon)
    {
        var mevcut = await vt.UrunKonfigurasyonSablonlari.FindAsync(id);
        if (mevcut is null)
            return Cevap<UrunKonfigurasyonSablonu>.Hata("Sablon bulunamadi.");

        mevcut.DetaySablonu = sablon.DetaySablonu;
        mevcut.HeroAktifMi = sablon.HeroAktifMi;
        mevcut.TeknikOzellikAktifMi = sablon.TeknikOzellikAktifMi;
        mevcut.PdfKaynakAktifMi = sablon.PdfKaynakAktifMi;
        mevcut.BenzerUrunlerAktifMi = sablon.BenzerUrunlerAktifMi;
        mevcut.TeklifFormuAktifMi = sablon.TeklifFormuAktifMi;
        mevcut.UcBoyutIlkAcilacakMi = sablon.UcBoyutIlkAcilacakMi;
        mevcut.AnimasyonTipi = sablon.AnimasyonTipi;
        mevcut.MobilPanelDavranisi = sablon.MobilPanelDavranisi;
        mevcut.RenkPaneliKonumu = sablon.RenkPaneliKonumu;
        mevcut.AktifMi = sablon.AktifMi;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;

        await vt.SaveChangesAsync();
        return Cevap<UrunKonfigurasyonSablonu>.Basarili(mevcut, "Konfigurasyon sablonu guncellendi.");
    }
}
