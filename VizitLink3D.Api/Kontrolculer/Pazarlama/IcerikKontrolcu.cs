using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Pazarlama;

/// <summary>
/// Referanslar, SSS ve hizmet adimlari icin API endpoint'leri.
/// Herkese acik okuma endpoint'leridir.
/// </summary>
[ApiController]
[Route("api")]
public class IcerikKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    // ��� REFERANSLAR ���������������������������������������������

    [HttpGet("referanslar")]
    public async Task<IActionResult> ReferanslariGetir([FromQuery] string? tip)
    {
        var sorgu = vt.Referanslar.Where(r => r.AktifMi && !r.SilindiMi).AsQueryable();
        if (!string.IsNullOrEmpty(tip))
            sorgu = sorgu.Where(r => r.Tip == tip);

        var liste = await sorgu.OrderBy(r => r.SiraNo).ToListAsync();
        return Ok(Cevap<List<Referans>>.Basarili(liste));
    }

    // ��� HIZMET ADIMLARI �����������������������������������������

    [HttpGet("hizmet-adimlari")]
    public async Task<IActionResult> HizmetAdimlariniGetir()
    {
        var liste = await vt.HizmetAdimlari
            .Where(h => h.AktifMi && !h.SilindiMi)
            .OrderBy(h => h.AdimNo)
            .ToListAsync();

        return Ok(Cevap<List<HizmetAdimi>>.Basarili(liste));
    }

    // ��� SSS ������������������������������������������������������

    [HttpGet("sss")]
    public async Task<IActionResult> SSSGetir([FromQuery] string? kategori)
    {
        var sorgu = vt.SikSorulanSorular.Where(s => s.AktifMi && !s.SilindiMi).AsQueryable();
        if (!string.IsNullOrEmpty(kategori))
            sorgu = sorgu.Where(s => s.KategoriAdi == kategori);

        var liste = await sorgu.OrderBy(s => s.SiraNo).ToListAsync();
        return Ok(Cevap<List<SikSorulanSoru>>.Basarili(liste));
    }

    // ��� MUSTERI YORUMLARI ���������������������������������������

    [HttpGet("musteri-yorumlari")]
    public async Task<IActionResult> MusteriYorumlariniGetir()
    {
        var liste = await vt.MusteriYorumlari
            .Where(y => y.AktifMi && y.Onaylandi && !y.SilindiMi)
            .OrderByDescending(y => y.YorumTarihi)
            .Take(10)
            .ToListAsync();

        return Ok(Cevap<List<MusteriYorumu>>.Basarili(liste));
    }
}



