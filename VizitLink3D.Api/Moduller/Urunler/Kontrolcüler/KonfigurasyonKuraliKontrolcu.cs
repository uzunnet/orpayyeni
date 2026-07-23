using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Urunler.Kontrolcüler;

[ApiController]
[Route("api/urunler")]
public class KonfigurasyonKuraliKontrolcu(VizitLink3DDbContext vt) : ControllerBase
{
    [HttpGet("{urunId:int}/konfigurasyon-kurallari")]
    public async Task<Cevap<List<UrunKonfigurasyonKurali>>> UrunIcinListele(int urunId)
    {
        var kurallar = await vt.UrunKonfigurasyonKurallari
            .AsNoTracking()
            .Where(k => k.UrunId == urunId && k.AktifMi)
            .OrderBy(k => k.KuralTipi)
            .ThenBy(k => k.OlusturulmaTarihi)
            .ToListAsync();

        return Cevap<List<UrunKonfigurasyonKurali>>.Basarili(kurallar);
    }

    [HttpPost("{urunId:int}/konfigurasyon-kurallari")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunKonfigurasyonKurali>> Olustur(int urunId, [FromBody] UrunKonfigurasyonKurali kural)
    {
        kural.UrunId = urunId;
        kural.OlusturulmaTarihi = DateTime.UtcNow;
        vt.UrunKonfigurasyonKurallari.Add(kural);
        await vt.SaveChangesAsync();

        return Cevap<UrunKonfigurasyonKurali>.Basarili(kural, "Konfigurasyon kurali olusturuldu.");
    }

    [HttpPut("{urunId:int}/konfigurasyon-kurallari/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<UrunKonfigurasyonKurali>> Guncelle(int urunId, int id, [FromBody] UrunKonfigurasyonKurali kural)
    {
        var mevcut = await vt.UrunKonfigurasyonKurallari
            .FirstOrDefaultAsync(k => k.Id == id && k.UrunId == urunId);

        if (mevcut is null)
            return Cevap<UrunKonfigurasyonKurali>.Hata("Kural bulunamadi.");

        mevcut.Parca1Id = kural.Parca1Id;
        mevcut.Parca2Id = kural.Parca2Id;
        mevcut.Parca1RenkId = kural.Parca1RenkId;
        mevcut.Parca2RenkId = kural.Parca2RenkId;
        mevcut.Parca1MalzemeId = kural.Parca1MalzemeId;
        mevcut.Parca2MalzemeId = kural.Parca2MalzemeId;
        mevcut.KuralTipi = kural.KuralTipi;
        mevcut.AktifMi = kural.AktifMi;

        await vt.SaveChangesAsync();
        return Cevap<UrunKonfigurasyonKurali>.Basarili(mevcut, "Konfigurasyon kurali guncellendi.");
    }

    [HttpDelete("{urunId:int}/konfigurasyon-kurallari/{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<bool>> Sil(int urunId, int id)
    {
        var mevcut = await vt.UrunKonfigurasyonKurallari
            .FirstOrDefaultAsync(k => k.Id == id && k.UrunId == urunId);

        if (mevcut is null)
            return Cevap<bool>.Hata("Kural bulunamadi.");

        mevcut.AktifMi = false;
        await vt.SaveChangesAsync();

        return Cevap<bool>.Basarili(true, "Konfigurasyon kurali silindi.");
    }
}
