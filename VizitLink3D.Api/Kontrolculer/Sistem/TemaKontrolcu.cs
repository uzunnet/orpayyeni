using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.Moduller.Tema.Servisler;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Tema;
using TemaKapsam = VizitLink3D.Ortak.Modeller.Tema.TemaKapsam;

namespace VizitLink3D.Api.Kontrolculer.Sistem;

[ApiController]
[Route("api/tema")]
public class TemaKontrolcu(
    VizitLink3DDbContext db,
    CokluTemaServisi cokluTemaServisi,
    StitchTemaServisi stitchTemaServisi) : ControllerBase
{
    private static readonly string[] TemaKokParcalari =
    [
        "..", "..", "..", "..", "VizitLink3D.UI", "wwwroot", "css", "temalar"
    ];

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var temaAyarlari = await db.SistemAyarlari
            .Where(a => a.Anahtar.StartsWith("tema.") || a.Anahtar.StartsWith("gorunum."))
            .ToListAsync();

        var sozluk = temaAyarlari.ToDictionary(a => a.Anahtar, a => a.Deger);

        return Ok(Cevap<object>.Basarili(new
        {
            BirincilRenk = sozluk.GetValueOrDefault("tema.birincilRenk", "#1A1A27"),
            IkincilRenk = sozluk.GetValueOrDefault("tema.ikincilRenk", "#C8952A"),
            VurguRengi = sozluk.GetValueOrDefault("tema.vurguRengi", "#8B4543"),
            ArkaPlanRengi = sozluk.GetValueOrDefault("tema.arkaPlanRengi", "#F5F2ED"),
            KoyuTemaMi = sozluk.GetValueOrDefault("gorunum.koyuTema", "false") == "true",
            YuvarlakKoseler = sozluk.GetValueOrDefault("gorunum.yuvarlakKoseler", "false") == "true",
            Glassmorphism = sozluk.GetValueOrDefault("gorunum.glassmorphism", "true") == "true"
        }));
    }

    /// <summary>Tüm temaları listeler (admin paneli için).</summary>
    [HttpGet("hepsi")]
    public async Task<IActionResult> Hepsi()
    {
        var liste = await cokluTemaServisi.KatalogGetirAsync();
        return Ok(Cevap<object>.Basarili(liste));
    }

    /// <summary>Kapsam filtresi ile tema listeler. ?kapsam=site → sadece frontend temaları.</summary>
    [HttpGet("kapsam")]
    [AllowAnonymous]
    public async Task<IActionResult> KapsamGetir([FromQuery] string? kapsam)
    {
        TemaKapsam? kapsamDegeri = kapsam?.ToLowerInvariant() switch
        {
            "site" => TemaKapsam.Sadece_Site,
            "admin" => TemaKapsam.Sadece_Admin,
            _ => null
        };

        var liste = await cokluTemaServisi.KatalogGetirAsync(kapsamDegeri);
        return Ok(Cevap<object>.Basarili(liste));
    }

    /// <summary>Slug ile tek tema detayı.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> Detay(string slug)
    {
        var tema = await cokluTemaServisi.TemaBulAsync(slug);
        if (tema == null)
            return NotFound(Cevap<bool>.Hata("Tema bulunamadı."));

        return Ok(Cevap<object>.Basarili(tema));
    }

    /// <summary>Tema CSS parçasını güvenli şekilde düz metin olarak döner.</summary>
    [HttpGet("css/{temaSlug}/{dosyaAdi}")]
    [AllowAnonymous]
    public IActionResult CssGetir(string temaSlug, string dosyaAdi)
    {
        var temizTema = (temaSlug ?? string.Empty).Trim().ToLowerInvariant();
        var temizDosya = (dosyaAdi ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(temizTema) || string.IsNullOrWhiteSpace(temizDosya))
        {
            return BadRequest(Cevap<bool>.Hata("Tema dosya bilgisi eksik."));
        }

        if (temizDosya is not ("tokens" or "bilesenler" or "animasyonlar"))
        {
            return NotFound(Cevap<bool>.Hata("Tema parçası bulunamadı."));
        }

        var kesinYol = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Path.Combine(TemaKokParcalari), temizTema, $"{temizDosya}.css"));
        if (!System.IO.File.Exists(kesinYol))
        {
            return NotFound(Cevap<bool>.Hata("Tema CSS dosyası bulunamadı."));
        }

        var icerik = System.IO.File.ReadAllText(kesinYol);
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";
        return Content(icerik, "text/css; charset=utf-8");
    }

    /// <summary>Tema kataloğu (eski uyumluluk).</summary>
    [HttpGet("katalog")]
    public async Task<IActionResult> Katalog()
    {
        var liste = await cokluTemaServisi.KatalogGetirAsync();
        return Ok(Cevap<object>.Basarili(liste));
    }

    /// <summary>Stitch DESIGN.md içeriğini tema manifest taslağına çevirir.</summary>
    [HttpPost("stitch/taslak")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<StitchTemaTaslakSonucu>> StitchTaslak(
        [FromBody] StitchTemaTaslakIstek istek,
        CancellationToken iptal)
        => await stitchTemaServisi.TaslakOlusturAsync(istek, iptal);

    /// <summary>Stitch tema taslağını onaylar, manifest ve CSS dosyalarını üretir.</summary>
    [HttpPost("stitch/onay")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<Cevap<StitchTemaOnaySonucu>> StitchOnay(
        [FromBody] StitchTemaOnayIstek istek,
        CancellationToken iptal)
        => await stitchTemaServisi.OnaylaAsync(istek, iptal);

    /// <summary>Aktif temayı seçer ve SignalR broadcast yapar.</summary>
    [HttpPost("aktif")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> AktifTemaSec([FromBody] AktifTemaIstek istek)
    {
        if (istek == null || string.IsNullOrWhiteSpace(istek.TemaAd))
            return Ok(Cevap<object>.Hata("Tema adı zorunludur."));

        var tema = await cokluTemaServisi.TemaBulAsync(istek.TemaAd);
        if (tema == null)
            return Ok(Cevap<object>.Hata($"Tema bulunamadı: {istek.TemaAd}"));

        var mevcut = await db.SistemAyarlari.FirstOrDefaultAsync(a => a.Anahtar == CokluTemaServisi.AKTIF_TEMA_AYAR);
        if (mevcut == null)
        {
            db.SistemAyarlari.Add(new SistemAyari
            {
                Anahtar = CokluTemaServisi.AKTIF_TEMA_AYAR,
                Deger = istek.TemaAd,
                Tip = "string",
                OlusturulmaTarihi = DateTime.UtcNow
            });
        }
        else
        {
            mevcut.Deger = istek.TemaAd;
            mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();

        await cokluTemaServisi.TemaYukleVeUygulaAsync(istek.TemaAd);

        return Ok(Cevap<object>.Basarili(new { temaAd = tema.Slug, temaAdi = tema.Ad },
            $"'{tema.Ad}' teması aktif edildi."));
    }

    /// <summary>Yeni tema ekler (super admin).</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Ekle([FromBody] TemaSablonu tema)
    {
        var sonuc = await cokluTemaServisi.TemaEkleAsync(tema);
        if (sonuc == null)
            return BadRequest(Cevap<bool>.Hata("Bu slug ile zaten bir tema var."));

        return Ok(Cevap<TemaSablonu>.Basarili(sonuc, "Tema eklendi."));
    }

    /// <summary>Temayı günceller (super admin).</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] TemaSablonu tema)
    {
        tema.Id = id;
        var basarili = await cokluTemaServisi.TemaGuncelleAsync(tema);
        if (!basarili)
            return NotFound(Cevap<bool>.Hata("Tema bulunamadı."));

        return Ok(Cevap<bool>.Basarili(true, "Tema güncellendi."));
    }

    /// <summary>Temayı siler (super admin, soft delete).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Sil(int id)
    {
        var basarili = await cokluTemaServisi.TemaSilAsync(id);
        if (!basarili)
            return NotFound(Cevap<bool>.Hata("Tema bulunamadı."));

        return Ok(Cevap<bool>.Basarili(true, "Tema silindi."));
    }

    /// <summary>Sistem tema ayarlarını kaydeder.</summary>
    [HttpPost("ayarlar")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Kaydet([FromBody] Dictionary<string, string> ayarlar)
    {
        foreach (var (anahtar, deger) in ayarlar)
        {
            var mevcut = await db.SistemAyarlari.FirstOrDefaultAsync(a => a.Anahtar == anahtar);
            if (mevcut != null)
            {
                mevcut.Deger = deger;
                mevcut.GuncellenmeTarihi = DateTime.UtcNow;
            }
            else
            {
                db.SistemAyarlari.Add(new SistemAyari
                {
                    Anahtar = anahtar,
                    Deger = deger,
                    Tip = "string",
                    OlusturulmaTarihi = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
        return Ok(Cevap<bool>.Basarili(true, "Tema ayarları kaydedildi."));
    }

    /// <summary>Tema modunu (acik/koyu) değiştirir ve SignalR broadcast yapar.</summary>
    [HttpPost("mod")]
    public async Task<IActionResult> ModDegistir([FromBody] TemaModIstek istek)
    {
        if (istek == null || string.IsNullOrWhiteSpace(istek.Tema) || string.IsNullOrWhiteSpace(istek.Mod))
            return Ok(Cevap<object>.Hata("Tema ve mod zorunludur."));

        var basarili = await cokluTemaServisi.ModDegistirAsync(istek.Tema, istek.Mod);
        if (!basarili)
            return Ok(Cevap<object>.Hata($"Mod değiştirilemedi: {istek.Tema}/{istek.Mod}"));

        return Ok(Cevap<object>.Basarili(new { tema = istek.Tema, mod = istek.Mod }, "Mod değiştirildi."));
    }
}

public sealed class AktifTemaIstek
{
    public string TemaAd { get; set; } = string.Empty;
}

public sealed class TemaModIstek
{
    public string Tema { get; set; } = string.Empty;
    public string Mod { get; set; } = string.Empty;
}
