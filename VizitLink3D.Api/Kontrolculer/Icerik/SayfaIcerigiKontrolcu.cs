using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Servisler;

namespace VizitLink3D.Api.Kontrolculer.Icerik;

[ApiController]
[Route("api/sayfa-icerigi")]
public class SayfaIcerigiKontrolcu(VizitLink3DDbContext vt, IOtomatikCeviriServisi otomatikCeviri, KiraciServisi kiraci) : ControllerBase
{
    [HttpGet("{bolum}")]
    public async Task<IActionResult> BolumGetir(string bolum, [FromQuery] string dil = "tr")
    {
        var targetDil = dil.ToLowerInvariant();
        var firmaId = await FirmaIdGetirAsync();
        var trIcerikler = await FirmaIcerikleriGetirAsync(bolum, "tr", firmaId);

        if (targetDil == "tr")
        {
            var sozluk = trIcerikler.ToDictionary(s => s.Anahtar, s => s.Deger);
            return Ok(Cevap<Dictionary<string, string>>.Basarili(sozluk));
        }

        var icerikler = await FirmaIcerikleriGetirAsync(bolum, targetDil, firmaId);
        var sozlukTarget = icerikler.ToDictionary(s => s.Anahtar, s => s.Deger);

        var missingKeys = trIcerikler.Where(tr => !sozlukTarget.ContainsKey(tr.Anahtar)).ToList();
        if (missingKeys.Any())
        {
            var newIcerikler = new List<SayfaIcerigi>();
            foreach (var tr in missingKeys)
            {
                var response = await otomatikCeviri.CevirAsync(tr.Deger, "tr", targetDil);
                var translatedVal = response.BasariliMi && !string.IsNullOrEmpty(response.Veri) ? response.Veri : tr.Deger;

                var newIcerik = new SayfaIcerigi
                {
                    FirmaId = firmaId,
                    Bolum = bolum,
                    Anahtar = tr.Anahtar,
                    Dil = targetDil,
                    Deger = translatedVal,
                    GuncellemeTarihi = DateTime.UtcNow
                };
                newIcerikler.Add(newIcerik);
                sozlukTarget[tr.Anahtar] = translatedVal;
            }

            if (newIcerikler.Any())
            {
                vt.SayfaIcerikleri.AddRange(newIcerikler);
                await vt.SaveChangesAsync();
            }
        }

        return Ok(Cevap<Dictionary<string, string>>.Basarili(sozlukTarget));
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> TumunuGetir()
    {
        var firmaId = await FirmaIdGetirAsync();
        var liste = await vt.SayfaIcerikleri
            .Where(s => s.FirmaId == firmaId || s.FirmaId == null)
            .OrderBy(s => s.FirmaId == firmaId ? 0 : 1)
            .ThenBy(s => s.Bolum)
            .ThenBy(s => s.Anahtar)
            .ToListAsync();
        return Ok(Cevap<List<SayfaIcerigi>>.Basarili(liste));
    }

    [HttpPut]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> KaydetVeyaGuncelle([FromBody] SayfaIcerigi icerik)
    {
        var firmaId = await FirmaIdGetirAsync();
        var dil = string.IsNullOrWhiteSpace(icerik.Dil) ? "tr" : icerik.Dil.ToLowerInvariant();
        var mevcut = await vt.SayfaIcerikleri
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.FirmaId == firmaId && s.Bolum == icerik.Bolum && s.Anahtar == icerik.Anahtar && s.Dil == dil);
        if (mevcut is null)
        {
            icerik.FirmaId = firmaId;
            icerik.Dil = dil;
            icerik.GuncellemeTarihi = DateTime.UtcNow;
            icerik.SilindiMi = false;
            icerik.SilinmeTarihi = null;
            vt.SayfaIcerikleri.Add(icerik);
        }
        else
        {
            mevcut.Deger = icerik.Deger;
            mevcut.SilindiMi = false;
            mevcut.SilinmeTarihi = null;
            mevcut.GuncellemeTarihi = DateTime.UtcNow;
        }
        await vt.SaveChangesAsync();
        return Ok(Cevap<bool>.Basarili(true, "İçerik kaydedildi."));
    }

    [HttpDelete("{bolum}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> BolumSil(string bolum)
    {
        var firmaId = await FirmaIdGetirAsync();
        var silinecekler = await vt.SayfaIcerikleri
            .Where(s => s.FirmaId == firmaId && s.Bolum == bolum)
            .ToListAsync();
            
        if (!silinecekler.Any())
            return NotFound(Cevap<bool>.Hata("Bölüm bulunamadı."));

        foreach (var icerik in silinecekler)
        {
            icerik.SilindiMi = true;
            icerik.SilinmeTarihi = DateTime.UtcNow;
            icerik.GuncellemeTarihi = DateTime.UtcNow;
        }

        await vt.SaveChangesAsync();
        
        return Ok(Cevap<bool>.Basarili(true, "Bölüm silindi."));
    }

    private async Task<int?> FirmaIdGetirAsync()
        => kiraci.MevcutFirmaId ?? await vt.Firmalar
            .Where(f => f.AktifMi)
            .Select(f => (int?)f.Id)
            .FirstOrDefaultAsync();

    private async Task<List<SayfaIcerigi>> FirmaIcerikleriGetirAsync(string bolum, string dil, int? firmaId)
    {
        var liste = await vt.SayfaIcerikleri
            .Where(s => s.Bolum == bolum && s.Dil == dil && (s.FirmaId == firmaId || s.FirmaId == null))
            .OrderBy(s => s.FirmaId == firmaId ? 0 : 1)
            .ThenByDescending(s => s.GuncellemeTarihi)
            .ToListAsync();

        return liste
            .GroupBy(s => s.Anahtar)
            .Select(g => g.First())
            .ToList();
    }

    [HttpPost("yukle/katalog")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> KatalogYukle(IFormFile dosya, [FromServices] IWebHostEnvironment env)
    {
        if (dosya == null || dosya.Length == 0)
            return BadRequest(new { basariliMi = false, mesaj = "Dosya seçilmedi." });

        var dosyaAdi = $"{Guid.NewGuid()}{Path.GetExtension(dosya.FileName)}";
        var dizin = Path.Combine(env.WebRootPath, "medya", "kataloglar");

        if (!Directory.Exists(dizin))
            Directory.CreateDirectory(dizin);

        var dosyaYolu = Path.Combine(dizin, dosyaAdi);

        using (var stream = new FileStream(dosyaYolu, FileMode.Create))
        {
            await dosya.CopyToAsync(stream);
        }

        return Ok(new { basariliMi = true, veri = $"/medya/kataloglar/{dosyaAdi}", mesaj = "Katalog yüklendi." });
    }
}




