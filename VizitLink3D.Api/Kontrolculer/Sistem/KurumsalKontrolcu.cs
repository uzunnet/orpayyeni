using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Ortak.Yardimcilar;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

/// <summary>
/// Katalog, sube, ekip, bulten ve kullanici kurumsal endpoint'leri.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class KurumsalKontrolcu(VizitLink3DDbContext vt, PdfOnizlemeServisi pdfOnizlemeServisi) : ControllerBase
{
    private static readonly HashSet<string> KatalogDosyaUzantilari = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    private static readonly HashSet<string> SertifikaDosyaUzantilari = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf"
    };

    [HttpGet("kataloglar")]
    [AllowAnonymous]
    public async Task<IActionResult> KatalogListe()
    {
        var liste = await vt.Kataloglar
            .AsNoTracking()
            .Where(katalog => katalog.AktifMi)
            .OrderBy(katalog => katalog.SiraNo)
            .ThenBy(katalog => katalog.Baslik)
            .ToListAsync();

        liste = liste
            .Where(katalog => KatalogDosyasiVarMi(katalog))
            .GroupBy(katalog => pdfOnizlemeServisi.FizikselBelgeYolu(katalog.PdfDosyaYolu), StringComparer.OrdinalIgnoreCase)
            .Select(grup => grup
                .OrderByDescending(katalog => katalog.PdfDosyaYolu.Contains("medya/kataloglar", StringComparison.OrdinalIgnoreCase))
                .ThenBy(katalog => katalog.SiraNo)
                .First())
            .OrderBy(katalog => katalog.SiraNo)
            .ThenBy(katalog => katalog.Baslik)
            .ToList();

        return Ok(Cevap<List<Katalog>>.Basarili(liste));
    }

    [HttpGet("kataloglar/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> KatalogDetay(int id)
    {
        var katalog = await vt.Kataloglar
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id && k.AktifMi);

        return katalog is null || !KatalogDosyasiVarMi(katalog)
            ? NotFound()
            : Ok(Cevap<Katalog>.Basarili(katalog));
    }

    [HttpPost("kataloglar")]
    public async Task<IActionResult> KatalogEkle(Katalog istek)
    {
        istek.OlusturulmaTarihi = DateTime.UtcNow;
        istek.AktifMi = true;

        vt.Kataloglar.Add(istek);
        await vt.SaveChangesAsync();

        return Ok(Cevap<Katalog>.Basarili(istek));
    }

    [HttpPut("kataloglar/{id:int}")]
    public async Task<IActionResult> KatalogGuncelle(int id, Katalog istek)
    {
        var katalog = await vt.Kataloglar.FindAsync(id);
        if (katalog is null)
        {
            return NotFound();
        }

        katalog.Baslik = istek.Baslik;
        katalog.Aciklama = istek.Aciklama;
        katalog.KapakResim = istek.KapakResim;
        katalog.PdfDosyaYolu = istek.PdfDosyaYolu;
        katalog.DosyaBoyutuMb = istek.DosyaBoyutuMb;
        katalog.SayfaSayisi = istek.SayfaSayisi;
        katalog.Yil = istek.Yil;
        katalog.SiraNo = istek.SiraNo;
        katalog.AktifMi = istek.AktifMi;

        await vt.SaveChangesAsync();

        return Ok(Cevap<Katalog>.Basarili(katalog));
    }

    [HttpDelete("kataloglar/{id:int}")]
    public async Task<IActionResult> KatalogSil(int id)
    {
        var katalog = await vt.Kataloglar.FindAsync(id);
        if (katalog is null)
        {
            return NotFound();
        }

        katalog.SilindiMi = true;
        katalog.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<bool>.Basarili(true));
    }

    [HttpPost("kataloglar/dosya-yukle")]
    [RequestSizeLimit(80_000_000)]
    public async Task<IActionResult> KatalogDosyaYukle([FromForm] IFormFile dosya, IWebHostEnvironment ortam)
    {
        if (dosya is null || dosya.Length == 0)
        {
            return BadRequest(Cevap<string>.Hata("Dosya secilmedi."));
        }

        var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
        if (!KatalogDosyaUzantilari.Contains(uzanti))
        {
            return BadRequest(Cevap<string>.Hata("Sadece PDF katalog dosyasi yuklenebilir."));
        }

        var klasor = Path.Combine(ortam.WebRootPath, "medya", "kataloglar");
        Directory.CreateDirectory(klasor);

        var dosyaAdi = $"{Guid.NewGuid():N}{uzanti}";
        var dosyaYolu = Path.Combine(klasor, dosyaAdi);

        await using var akis = System.IO.File.Create(dosyaYolu);
        await dosya.CopyToAsync(akis);

        var belgeYolu = $"/medya/kataloglar/{dosyaAdi}";
        var onizlemeYolu = await pdfOnizlemeServisi.OnizlemeOlusturAsync(belgeYolu);
        var sonuc = new KatalogDosyaYuklemeSonucu(
            belgeYolu,
            onizlemeYolu,
            Math.Round(dosya.Length / 1024d / 1024d, 2),
            dosya.Length);

        return Ok(Cevap<KatalogDosyaYuklemeSonucu>.Basarili(sonuc));
    }

    [HttpGet("sertifikalar")]
    [AllowAnonymous]
    public async Task<IActionResult> SertifikaListe()
    {
        var liste = await vt.Sertifikalar
            .AsNoTracking()
            .Where(sertifika => sertifika.AktifMi)
            .OrderBy(sertifika => sertifika.SiraNo)
            .ThenBy(sertifika => sertifika.Ad)
            .ToListAsync();

        liste = liste
            .Where(sertifika => SertifikaDosyasiVarMi(sertifika))
            .ToList();

        return Ok(Cevap<List<Sertifika>>.Basarili(liste));
    }

    [HttpGet("sertifikalar/yonetim")]
    public async Task<IActionResult> SertifikaYonetimListe()
    {
        var liste = await vt.Sertifikalar
            .AsNoTracking()
            .OrderBy(sertifika => sertifika.SiraNo)
            .ThenBy(sertifika => sertifika.Ad)
            .ToListAsync();

        return Ok(Cevap<List<Sertifika>>.Basarili(liste));
    }

    [HttpGet("sertifikalar/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> SertifikaDetay(int id)
    {
        var sertifika = await vt.Sertifikalar
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.AktifMi);

        return sertifika is null || !SertifikaDosyasiVarMi(sertifika)
            ? NotFound()
            : Ok(Cevap<Sertifika>.Basarili(sertifika));
    }

    [HttpPost("sertifikalar")]
    public async Task<IActionResult> SertifikaEkle(Sertifika istek)
    {
        istek.OlusturulmaTarihi = DateTime.UtcNow;
        istek.AktifMi = true;

        vt.Sertifikalar.Add(istek);
        await vt.SaveChangesAsync();

        return Ok(Cevap<Sertifika>.Basarili(istek));
    }

    [HttpPut("sertifikalar/{id:int}")]
    public async Task<IActionResult> SertifikaGuncelle(int id, Sertifika istek)
    {
        var sertifika = await vt.Sertifikalar.FindAsync(id);
        if (sertifika is null)
        {
            return NotFound();
        }

        sertifika.Ad = istek.Ad;
        sertifika.Aciklama = istek.Aciklama;
        sertifika.Resim = istek.Resim;
        sertifika.PdfDosya = istek.PdfDosya;
        sertifika.VerilmeTarihi = istek.VerilmeTarihi;
        sertifika.GecerlilikTarihi = istek.GecerlilikTarihi;
        sertifika.VerenKurum = istek.VerenKurum;
        sertifika.SiraNo = istek.SiraNo;
        sertifika.AktifMi = istek.AktifMi;

        await vt.SaveChangesAsync();

        return Ok(Cevap<Sertifika>.Basarili(sertifika));
    }

    [HttpDelete("sertifikalar/{id:int}")]
    public async Task<IActionResult> SertifikaSil(int id)
    {
        var sertifika = await vt.Sertifikalar.FindAsync(id);
        if (sertifika is null)
        {
            return NotFound();
        }

        sertifika.SilindiMi = true;
        sertifika.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<bool>.Basarili(true));
    }

    [HttpPost("sertifikalar/dosya-yukle")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> SertifikaDosyaYukle([FromForm] IFormFile dosya, IWebHostEnvironment ortam)
    {
        if (dosya is null || dosya.Length == 0)
        {
            return BadRequest(Cevap<string>.Hata("Dosya secilmedi."));
        }

        var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
        if (!SertifikaDosyaUzantilari.Contains(uzanti))
        {
            return BadRequest(Cevap<string>.Hata("Sadece JPG, PNG ve PDF dosyalari yuklenebilir."));
        }

        var klasor = Path.Combine(ortam.WebRootPath, "medya", "sertifikalar");
        Directory.CreateDirectory(klasor);

        var dosyaAdi = $"{Guid.NewGuid():N}{uzanti}";
        var dosyaYolu = Path.Combine(klasor, dosyaAdi);

        await using var akis = System.IO.File.Create(dosyaYolu);
        await dosya.CopyToAsync(akis);

        var belgeYolu = $"/medya/sertifikalar/{dosyaAdi}";
        var onizlemeYolu = uzanti == ".pdf"
            ? await pdfOnizlemeServisi.OnizlemeOlusturAsync(belgeYolu)
            : belgeYolu;

        var sonuc = new SertifikaDosyaYuklemeSonucu(
            belgeYolu,
            uzanti == ".pdf" ? "Pdf" : "Resim",
            onizlemeYolu,
            dosya.Length);

        return Ok(Cevap<SertifikaDosyaYuklemeSonucu>.Basarili(sonuc));
    }

    [HttpGet("fix-3d")]
    [AllowAnonymous]
    public async Task<IActionResult> Fix3D([FromServices] VizitLink3DDbContext vt)
    {
        var modeller = await vt.KapakModelleri.ToListAsync();
        foreach (var m in modeller)
        {
            m.ModelDosyaYolu = null;
        }
        await vt.SaveChangesAsync();
        return Ok("3D models cleared from DB.");
    }

    [HttpGet("belge-onizleme")]
    [AllowAnonymous]
    public async Task<IActionResult> BelgeOnizleme([FromQuery] string dosya)
    {
        if (PdfOnizlemeServisi.GorselMi(dosya))
        {
            var gorselYolu = pdfOnizlemeServisi.FizikselGorselYolu(dosya);
            return gorselYolu is null
                ? NotFound()
                : PhysicalFile(gorselYolu, MimeTipi(dosya));
        }

        if (!PdfOnizlemeServisi.PdfMi(dosya))
        {
            return NotFound();
        }

        var onizlemeYolu = await pdfOnizlemeServisi.OnizlemeOlusturAsync(dosya);
        var fizikselYol = pdfOnizlemeServisi.FizikselGorselYolu(onizlemeYolu);

        if (fizikselYol is not null)
        {
            return PhysicalFile(fizikselYol, "image/png");
        }

        var varsayilan = pdfOnizlemeServisi.FizikselGorselYolu("/medya/kataloglar/placeholder.webp")
            ?? pdfOnizlemeServisi.FizikselGorselYolu("/medya/sertifikalar/placeholder.webp");

        return varsayilan is null
            ? NotFound()
            : PhysicalFile(varsayilan, MimeTipi(varsayilan));
    }

    [HttpGet("belge-dosya")]
    [AllowAnonymous]
    public IActionResult BelgeDosya([FromQuery] string dosya)
    {
        var fizikselYol = pdfOnizlemeServisi.FizikselBelgeYolu(dosya);
        if (fizikselYol is null)
        {
            return NotFound();
        }

        return PhysicalFile(fizikselYol, MimeTipi(fizikselYol), Path.GetFileName(fizikselYol));
    }

    [HttpGet("subeler")]
    [AllowAnonymous]
    public async Task<IActionResult> SubeListe()
    {
        var liste = await vt.Subeler
            .AsNoTracking()
            .Where(sube => sube.AktifMi)
            .OrderBy(sube => sube.SiraNo)
            .ThenBy(sube => sube.Ad)
            .ToListAsync();

        return Ok(Cevap<List<Sube>>.Basarili(liste));
    }

    [HttpPost("subeler")]
    public async Task<IActionResult> SubeEkle(Sube istek)
    {
        istek.OlusturulmaTarihi = DateTime.UtcNow;
        istek.AktifMi = true;

        vt.Subeler.Add(istek);
        await vt.SaveChangesAsync();

        return Ok(Cevap<Sube>.Basarili(istek));
    }

    [HttpPut("subeler/{id:int}")]
    public async Task<IActionResult> SubeGuncelle(int id, Sube istek)
    {
        var sube = await vt.Subeler.FindAsync(id);
        if (sube is null)
        {
            return NotFound();
        }

        sube.Ad = istek.Ad;
        sube.Adres = istek.Adres;
        sube.Sehir = istek.Sehir;
        sube.Ilce = istek.Ilce;
        sube.Telefon = istek.Telefon;
        sube.Eposta = istek.Eposta;
        sube.Enlem = istek.Enlem;
        sube.Boylam = istek.Boylam;
        sube.CalismaSaatleri = istek.CalismaSaatleri;
        sube.Aciklama = istek.Aciklama;
        sube.SubeYetkilisi = istek.SubeYetkilisi;
        sube.SubeYetkilisiTelefon = istek.SubeYetkilisiTelefon;
        sube.SiraNo = istek.SiraNo;
        sube.AktifMi = istek.AktifMi;

        await vt.SaveChangesAsync();

        return Ok(Cevap<Sube>.Basarili(sube));
    }

    [HttpDelete("subeler/{id:int}")]
    public async Task<IActionResult> SubeSil(int id)
    {
        var sube = await vt.Subeler.FindAsync(id);
        if (sube is null)
        {
            return NotFound();
        }

        sube.SilindiMi = true;
        sube.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<bool>.Basarili(true));
    }

    [HttpGet("ekip")]
    [AllowAnonymous]
    public async Task<IActionResult> EkipListe()
    {
        var liste = await vt.EkipUyeleri
            .AsNoTracking()
            .Where(uye => uye.AktifMi)
            .OrderBy(uye => uye.SiraNo)
            .ThenBy(uye => uye.AdSoyad)
            .ToListAsync();

        return Ok(Cevap<List<EkipUyesi>>.Basarili(liste));
    }

    [HttpPost("ekip")]
    public async Task<IActionResult> EkipEkle(EkipUyesi istek)
    {
        istek.OlusturulmaTarihi = DateTime.UtcNow;
        istek.AktifMi = true;

        vt.EkipUyeleri.Add(istek);
        await vt.SaveChangesAsync();

        return Ok(Cevap<EkipUyesi>.Basarili(istek));
    }

    [HttpPut("ekip/{id:int}")]
    public async Task<IActionResult> EkipGuncelle(int id, EkipUyesi istek)
    {
        var uye = await vt.EkipUyeleri.FindAsync(id);
        if (uye is null)
        {
            return NotFound();
        }

        uye.AdSoyad = istek.AdSoyad;
        uye.Unvan = istek.Unvan;
        uye.Bio = istek.Bio;
        uye.Resim = istek.Resim;
        uye.Linkedin = istek.Linkedin;
        uye.SiraNo = istek.SiraNo;
        uye.AktifMi = istek.AktifMi;

        await vt.SaveChangesAsync();

        return Ok(Cevap<EkipUyesi>.Basarili(uye));
    }

    [HttpDelete("ekip/{id:int}")]
    public async Task<IActionResult> EkipSil(int id)
    {
        var uye = await vt.EkipUyeleri.FindAsync(id);
        if (uye is null)
        {
            return NotFound();
        }

        uye.SilindiMi = true;
        uye.SilinmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        return Ok(Cevap<bool>.Basarili(true));
    }

    [HttpGet("bulten")]
    public async Task<IActionResult> BultenListe()
    {
        var liste = await vt.BultenAboneleri
            .AsNoTracking()
            .OrderByDescending(abone => abone.AbonelikTarihi)
            .ToListAsync();

        return Ok(Cevap<List<BultenAbonesi>>.Basarili(liste));
    }

    [HttpGet("kullanicilar")]
    public async Task<IActionResult> KullaniciListe()
    {
        var liste = await vt.Kullanicilar
            .AsNoTracking()
            .OrderBy(kullanici => kullanici.OlusturulmaTarihi)
            .ToListAsync();

        return Ok(Cevap<List<Kullanici>>.Basarili(liste));
    }

    [HttpPut("kullanicilar/{id:int}")]
    public async Task<IActionResult> KullaniciGuncelle(int id, Kullanici istek)
    {
        var kullanici = await vt.Kullanicilar.FindAsync(id);
        if (kullanici is null)
        {
            return NotFound();
        }

        kullanici.AdSoyad = istek.AdSoyad;
        kullanici.Eposta = istek.Eposta;
        kullanici.Rol = istek.Rol;
        kullanici.AktifMi = istek.AktifMi;

        await vt.SaveChangesAsync();

        return Ok(Cevap<Kullanici>.Basarili(kullanici));
    }

    private static string MimeTipi(string yol)
    {
        var uzanti = Path.GetExtension(yol);
        if (uzanti.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return "application/pdf";
        }

        if (uzanti.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            return "image/png";
        }

        return uzanti.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            ? "image/webp"
            : "image/jpeg";
    }

    private bool KatalogDosyasiVarMi(Katalog katalog)
    {
        return PdfOnizlemeServisi.PdfMi(katalog.PdfDosyaYolu)
            && (KatalogYolu.GuvenliGenelKatalogYolu(katalog.PdfDosyaYolu) is not null
                || pdfOnizlemeServisi.FizikselBelgeYolu(katalog.PdfDosyaYolu) is not null);
    }

    private bool SertifikaDosyasiVarMi(Sertifika sertifika)
    {
        if (!string.IsNullOrWhiteSpace(sertifika.PdfDosya))
        {
            return pdfOnizlemeServisi.FizikselBelgeYolu(sertifika.PdfDosya) is not null;
        }

        return !string.IsNullOrWhiteSpace(sertifika.Resim)
            && pdfOnizlemeServisi.FizikselGorselYolu(sertifika.Resim) is not null;
    }

    public sealed record KatalogDosyaYuklemeSonucu(string Yol, string? OnizlemeYolu, double BoyutMb, long BoyutByte);

    public sealed record SertifikaDosyaYuklemeSonucu(string Yol, string DosyaTuru, string? OnizlemeYolu, long BoyutByte);
}



