using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Pazarlama;

/// <summary>
/// Admin paneli icin icerik yonetimi CRUD endpoint'leri.
/// Slayt, SSS, HizmetAdimi, Referans ve MusteriYorumu yonetimi.
/// Tum endpoint'ler Admin/SuperAdmin rolu ile korunmaktadir.
/// </summary>
[ApiController]
[Route("api/admin/icerik")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminIcerikKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    // ═══════════════════════════════════════════════════════════════
    // SLAYT CRUD
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("slaytlar")]
    public async Task<IActionResult> SlaytlariListele()
    {
        var liste = await vt.Slaytlar
            .Where(s => !s.SilindiMi)
            .OrderBy(s => s.SiraNo)
            .ToListAsync();
        return Ok(Cevap<List<Slayt>>.Basarili(liste));
    }

    [HttpPost("slaytlar")]
    public async Task<IActionResult> SlaytEkle([FromBody] Slayt slayt)
    {
        slayt.OlusturulmaTarihi = DateTime.UtcNow;
        vt.Slaytlar.Add(slayt);
        await vt.SaveChangesAsync();
        return Ok(Cevap<Slayt>.Basarili(slayt, "Slayt eklendi."));
    }

    [HttpPut("slaytlar/{id}")]
    public async Task<IActionResult> SlaytGuncelle(int id, [FromBody] Slayt guncel)
    {
        var slayt = await vt.Slaytlar.FirstOrDefaultAsync(s => s.Id == id && !s.SilindiMi);
        if (slayt is null) return NotFound(Cevap<Slayt>.Hata("Slayt bulunamadi."));

        slayt.Dil = guncel.Dil;
        slayt.Baslik = guncel.Baslik;
        slayt.AltBaslik = guncel.AltBaslik;
        slayt.Aciklama = guncel.Aciklama;
        slayt.ArkaplanResim = guncel.ArkaplanResim;
        slayt.ArkaplanResimMobil = guncel.ArkaplanResimMobil;
        slayt.ButonMetni1 = guncel.ButonMetni1;
        slayt.ButonLink1 = guncel.ButonLink1;
        slayt.ButonMetni2 = guncel.ButonMetni2;
        slayt.ButonLink2 = guncel.ButonLink2;
        slayt.AnimasyonTipi = guncel.AnimasyonTipi;
        slayt.GecisHizi = guncel.GecisHizi;
        slayt.GosterimSuresi = guncel.GosterimSuresi;
        slayt.MetinHizalama = guncel.MetinHizalama;
        slayt.MetinRengi = guncel.MetinRengi;
        slayt.SiraNo = guncel.SiraNo;
        slayt.AktifMi = guncel.AktifMi;
        slayt.BaslangicTarihi = guncel.BaslangicTarihi;
        slayt.BitisTarihi = guncel.BitisTarihi;

        await vt.SaveChangesAsync();
        return Ok(Cevap<Slayt>.Basarili(slayt, "Slayt guncellendi."));
    }

    [HttpDelete("slaytlar/{id}")]
    public async Task<IActionResult> SlaytSil(int id)
    {
        var slayt = await vt.Slaytlar.FindAsync(id);
        if (slayt is null) return NotFound(Cevap<Slayt>.Hata("Slayt bulunamadi."));
        slayt.SilindiMi = true; slayt.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!, "Slayt silindi."));
    }

    // ═══════════════════════════════════════════════════════════════
    // SSS CRUD
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("sss")]
    public async Task<IActionResult> SSSListele()
    {
        var liste = await vt.SikSorulanSorular
            .Where(s => !s.SilindiMi)
            .OrderBy(s => s.SiraNo)
            .ToListAsync();
        return Ok(Cevap<List<SikSorulanSoru>>.Basarili(liste));
    }

    [HttpPost("sss")]
    public async Task<IActionResult> SSSEkle([FromBody] SikSorulanSoru soru)
    {
        soru.OlusturulmaTarihi = DateTime.UtcNow;
        vt.SikSorulanSorular.Add(soru);
        await vt.SaveChangesAsync();
        return Ok(Cevap<SikSorulanSoru>.Basarili(soru, "SSS eklendi."));
    }

    [HttpPut("sss/{id}")]
    public async Task<IActionResult> SSSGuncelle(int id, [FromBody] SikSorulanSoru guncel)
    {
        var soru = await vt.SikSorulanSorular.FirstOrDefaultAsync(s => s.Id == id && !s.SilindiMi);
        if (soru is null) return NotFound(Cevap<SikSorulanSoru>.Hata("SSS bulunamadi."));

        soru.Soru = guncel.Soru;
        soru.Cevap = guncel.Cevap;
        soru.KategoriAdi = guncel.KategoriAdi;
        soru.SiraNo = guncel.SiraNo;
        soru.AktifMi = guncel.AktifMi;

        await vt.SaveChangesAsync();
        return Ok(Cevap<SikSorulanSoru>.Basarili(soru, "SSS guncellendi."));
    }

    [HttpDelete("sss/{id}")]
    public async Task<IActionResult> SSSSil(int id)
    {
        var soru = await vt.SikSorulanSorular.FindAsync(id);
        if (soru is null) return NotFound(Cevap<SikSorulanSoru>.Hata("SSS bulunamadi."));
        soru.SilindiMi = true; soru.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!, "SSS silindi."));
    }

    // ═══════════════════════════════════════════════════════════════
    // HIZMET ADIMI CRUD
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("hizmet-adimlari")]
    public async Task<IActionResult> HizmetAdimiListele()
    {
        var liste = await vt.HizmetAdimlari
            .Where(h => !h.SilindiMi)
            .OrderBy(h => h.AdimNo)
            .ToListAsync();
        return Ok(Cevap<List<HizmetAdimi>>.Basarili(liste));
    }

    [HttpPost("hizmet-adimlari")]
    public async Task<IActionResult> HizmetAdimiEkle([FromBody] HizmetAdimi adim)
    {
        adim.OlusturulmaTarihi = DateTime.UtcNow;
        vt.HizmetAdimlari.Add(adim);
        await vt.SaveChangesAsync();
        return Ok(Cevap<HizmetAdimi>.Basarili(adim, "Hizmet adimi eklendi."));
    }

    [HttpPut("hizmet-adimlari/{id}")]
    public async Task<IActionResult> HizmetAdimiGuncelle(int id, [FromBody] HizmetAdimi guncel)
    {
        var adim = await vt.HizmetAdimlari.FirstOrDefaultAsync(h => h.Id == id && !h.SilindiMi);
        if (adim is null) return NotFound(Cevap<HizmetAdimi>.Hata("Adim bulunamadi."));

        adim.Baslik = guncel.Baslik;
        adim.Aciklama = guncel.Aciklama;
        adim.Ikon = guncel.Ikon;
        adim.AdimNo = guncel.AdimNo;
        adim.SiraNo = guncel.SiraNo;
        adim.AktifMi = guncel.AktifMi;

        await vt.SaveChangesAsync();
        return Ok(Cevap<HizmetAdimi>.Basarili(adim, "Adim guncellendi."));
    }

    [HttpDelete("hizmet-adimlari/{id}")]
    public async Task<IActionResult> HizmetAdimiSil(int id)
    {
        var adim = await vt.HizmetAdimlari.FindAsync(id);
        if (adim is null) return NotFound(Cevap<HizmetAdimi>.Hata("Adim bulunamadi."));
        adim.SilindiMi = true; adim.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!, "Adim silindi."));
    }

    // ═══════════════════════════════════════════════════════════════
    // REFERANS CRUD
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("referanslar")]
    public async Task<IActionResult> ReferansListele()
    {
        var liste = await vt.Referanslar
            .Where(r => !r.SilindiMi)
            .OrderBy(r => r.SiraNo)
            .ToListAsync();
        return Ok(Cevap<List<Referans>>.Basarili(liste));
    }

    [HttpPost("referanslar")]
    public async Task<IActionResult> ReferansEkle([FromBody] Referans r)
    {
        r.OlusturulmaTarihi = DateTime.UtcNow;
        vt.Referanslar.Add(r);
        await vt.SaveChangesAsync();
        return Ok(Cevap<Referans>.Basarili(r, "Referans eklendi."));
    }

    [HttpPut("referanslar/{id}")]
    public async Task<IActionResult> ReferansGuncelle(int id, [FromBody] Referans g)
    {
        var r = await vt.Referanslar.FirstOrDefaultAsync(x => x.Id == id && !x.SilindiMi);
        if (r is null) return NotFound(Cevap<Referans>.Hata("Bulunamadi."));
        r.Ad = g.Ad; r.Tip = g.Tip; r.Logo = g.Logo; r.WebSite = g.WebSite; r.Aciklama = g.Aciklama; r.SiraNo = g.SiraNo; r.AktifMi = g.AktifMi;
        await vt.SaveChangesAsync();
        return Ok(Cevap<Referans>.Basarili(r, "Guncellendi."));
    }

    [HttpDelete("referanslar/{id}")]
    public async Task<IActionResult> ReferansSil(int id)
    {
        var r = await vt.Referanslar.FindAsync(id);
        if (r is null) return NotFound(Cevap<Referans>.Hata("Bulunamadi."));
        r.SilindiMi = true; r.SilinmeTarihi = DateTime.UtcNow; await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!, "Silindi."));
    }

    // ═══════════════════════════════════════════════════════════════
    // MUSTERI YORUMU CRUD
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("musteri-yorumlari")]
    public async Task<IActionResult> YorumListele()
    {
        var liste = await vt.MusteriYorumlari
            .Where(y => !y.SilindiMi)
            .OrderByDescending(y => y.YorumTarihi)
            .ToListAsync();
        return Ok(Cevap<List<MusteriYorumu>>.Basarili(liste));
    }

    [HttpPut("musteri-yorumlari/{id}/onay")]
    public async Task<IActionResult> YorumOnayla(int id, [FromQuery] bool onay)
    {
        var y = await vt.MusteriYorumlari.FirstOrDefaultAsync(x => x.Id == id && !x.SilindiMi);
        if (y is null) return NotFound(Cevap<MusteriYorumu>.Hata("Bulunamadi."));
        y.Onaylandi = onay; await vt.SaveChangesAsync();
        return Ok(Cevap<MusteriYorumu>.Basarili(y, onay ? "Onaylandi." : "Onay kaldirildi."));
    }

    [HttpDelete("musteri-yorumlari/{id}")]
    public async Task<IActionResult> YorumSil(int id)
    {
        var y = await vt.MusteriYorumlari.FindAsync(id);
        if (y is null) return NotFound(Cevap<MusteriYorumu>.Hata("Bulunamadi."));
        y.SilindiMi = true; y.SilinmeTarihi = DateTime.UtcNow; await vt.SaveChangesAsync();
        return Ok(Cevap<object>.Basarili(null!, "Silindi."));
    }
}


