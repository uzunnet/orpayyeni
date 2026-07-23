using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

/// <summary>
/// Coklu dil ve ceviri yonetimi API'si.
/// Public endpoint anonim ceviri cekmeye izin verir.
/// Admin endpoint'leri JWT korumalidir.
/// </summary>
[ApiController]
[Route("api/dil")]
public class DilKontrolcu(VizitLink3DDbContext vt, ICeviriServisi ceviriServisi) : ControllerBase
{
    /// <summary>
    /// Belirtilen dil icin tum cevirileri getirir.
    /// Herkese aciktir � anayasa �35: DB + Onbellek.
    /// </summary>
    [HttpGet("ceviriler/{dil}")]
    public async Task<IActionResult> CevirileriGetir(string dil)
    {
        var ceviriler = await ceviriServisi.TumCevirileriGetirAsync(dil);
        return Ok(Cevap<Dictionary<string, string>>.Basarili(ceviriler));
    }

    /// <summary>
    /// Desteklenen tum dilleri getirir.
    /// </summary>
    [HttpGet("desteklenen")]
    public async Task<IActionResult> DilleriGetir()
    {
        var diller = await vt.Diller
            .Where(d => d.AktifMi)
            .OrderBy(d => d.SiraNo)
            .ToListAsync();

        return Ok(Cevap<List<Dil>>.Basarili(diller));
    }

    /// <summary>
    /// Admin: Tum cevirileri listele (tum diller).
    /// </summary>
    [HttpGet("admin/tum-ceviriler")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> TumCevirileriGetir()
    {
        var ceviriler = await vt.Ceviriler
            .OrderBy(c => c.Bolum)
            .ThenBy(c => c.Anahtar)
            .ToListAsync();

        return Ok(Cevap<List<Ceviri>>.Basarili(ceviriler));
    }

    /// <summary>
    /// Admin: Ceviri ekle veya guncelle.
    /// </summary>
    [HttpPut("admin/ceviri")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CeviriKaydet([FromBody] Ceviri ceviri)
    {
        var mevcut = await vt.Ceviriler
            .FirstOrDefaultAsync(c => c.Anahtar == ceviri.Anahtar && c.Dil == ceviri.Dil);

        if (mevcut is not null)
        {
            mevcut.Deger = ceviri.Deger;
            mevcut.Bolum = ceviri.Bolum ?? mevcut.Bolum;
            mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        }
        else
        {
            ceviri.OlusturulmaTarihi = DateTime.UtcNow;
            vt.Ceviriler.Add(ceviri);
        }

        await vt.SaveChangesAsync();
        await ceviriServisi.OnbellekTemizleAsync();
        return Ok(Cevap<Ceviri>.Basarili(mevcut ?? ceviri, "�eviri kaydedildi."));
    }

    /// <summary>
    /// AI tarafindan uretilen ceviriyi kaydeder.
    /// </summary>
    [HttpPost("ceviri-ekle")]
    public async Task<IActionResult> AICeviriEkle([FromBody] AICeviriEkleIstegi istek)
    {
        if (string.IsNullOrWhiteSpace(istek.Anahtar) || string.IsNullOrWhiteSpace(istek.Deger))
            return Ok(Cevap<bool>.Hata("Anahtar veya de�er bo�."));

        var mevcut = await vt.Ceviriler
            .FirstOrDefaultAsync(c => c.Anahtar == istek.Anahtar && c.Dil == istek.Dil);

        if (mevcut is null)
        {
            vt.Ceviriler.Add(new Ceviri
            {
                Anahtar = istek.Anahtar,
                Dil = istek.Dil ?? "en",
                Deger = istek.Deger,
                Bolum = "ai",
                OlusturulmaTarihi = DateTime.UtcNow
            });
            await vt.SaveChangesAsync();
            await ceviriServisi.OnbellekTemizleAsync();
        }

        return Ok(Cevap<bool>.Basarili(true, "AI �eviri kaydedildi."));
    }

    /// <summary>
    /// Admin: Tum dilleri (pasifler dahil) listele.
    /// </summary>
    [HttpGet("admin/diller")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> AdminDilleriGetir()
    {
        var diller = await vt.Diller
            .OrderBy(d => d.SiraNo)
            .ToListAsync();
        return Ok(Cevap<List<Dil>>.Basarili(diller));
    }

    /// <summary>
    /// Admin: Dil aktif/pasif durumunu guncelle.
    /// </summary>
    [HttpPut("admin/dil/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DilGuncelle(int id, [FromBody] Dil dil)
    {
        var mevcut = await vt.Diller.FindAsync(id);
        if (mevcut is null) return Ok(Cevap<Dil>.Hata("Dil bulunamad�."));

        mevcut.AktifMi = dil.AktifMi;
        mevcut.SiraNo = dil.SiraNo;
        await vt.SaveChangesAsync();
        await ceviriServisi.OnbellekTemizleAsync();
        return Ok(Cevap<Dil>.Basarili(mevcut, "Dil g�ncellendi."));
    }

    public class AICeviriEkleIstegi
    {
        public string Anahtar { get; set; } = "";
        public string Dil { get; set; } = "en";
        public string Deger { get; set; } = "";
    }
}

