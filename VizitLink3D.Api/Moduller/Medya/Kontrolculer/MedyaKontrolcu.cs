using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Ayarlar;
using VizitLink3D.Ortak.Sabitler;
using VizitLink3D.Api.Moduller.Medya.Servisler;
using MedyaModel = VizitLink3D.Ortak.Modeller.Medya.Medya;

namespace VizitLink3D.Api.Moduller.Medya.Kontrolculer;

[ApiController]
[Route("api/medya")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class MedyaKontrolcu : ControllerBase
{
    private readonly IMedyaServisi _medyaServisi;
    private readonly IResimIslemcisi _resimIslemcisi;
    private readonly IWebHostEnvironment _cevre;
    private readonly VizitLink3DDbContext _db;
    private static readonly FileExtensionContentTypeProvider IcerikTipiSaglayici = new();

    public MedyaKontrolcu(IMedyaServisi medyaServisi, IResimIslemcisi resimIslemcisi, IWebHostEnvironment cevre, VizitLink3DDbContext db)
    {
        _medyaServisi = medyaServisi;
        _resimIslemcisi = resimIslemcisi;
        _cevre = cevre;
        _db = db;
    }

    [HttpGet]
    public async Task<Cevap<List<MedyaModel>>> Listele([FromQuery] int? klasorId, [FromQuery] string? q, [FromQuery] string? tip, [FromQuery] int sayfa = 1)
    {
        var liste = await _medyaServisi.ListeleAsync(klasorId, q, tip, sayfa);
        return Cevap<List<MedyaModel>>.Basarili(liste);
    }

    [HttpGet("{id:long}")]
    public async Task<Cevap<MedyaModel?>> Detay(long id)
    {
        var medya = await _db.Medyalar.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.SilindiMi);
        return Cevap<MedyaModel?>.Basarili(medya);
    }

    [HttpGet("dosya/{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> DosyaGetir(long id)
    {
        var medya = await _db.Medyalar.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && !m.SilindiMi);
        if (medya == null || string.IsNullOrEmpty(medya.DosyaYolu))
            return NotFound();

        var dosyaYolu = medya.DosyaYolu.Replace('\\', '/').TrimStart('/');
        if (dosyaYolu.Contains("..", StringComparison.Ordinal))
            return BadRequest();

        var apiWwwroot = _cevre.WebRootPath ?? Path.Combine(_cevre.ContentRootPath, "wwwroot");
        var tamYol = Path.Combine(apiWwwroot, dosyaYolu).Replace('/', Path.DirectorySeparatorChar);

        if (!System.IO.File.Exists(tamYol))
            return NotFound();

        var stream = new FileStream(tamYol, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        var saglayici = new FileExtensionContentTypeProvider();
        if (!saglayici.TryGetContentType(tamYol, out var icerikTipi))
        {
            // Uzantidan cikaramadiysa dosya icindeki magic byte'lara bak
            var header = new byte[8];
            var readBytes = await stream.ReadAsync(header.AsMemory(0, 8));
            stream.Position = 0;

            icerikTipi = (header, readBytes) switch
            {
                ([0xFF, 0xD8, 0xFF, ..], _) => "image/jpeg",
                ([0x89, 0x50, 0x4E, 0x47, ..], _) => "image/png",
                ([0x52, 0x49, 0x46, 0x46, ..], _) => "image/webp",
                ([0x47, 0x49, 0x46, 0x38, ..], _) => "image/gif",
                ([0x25, 0x50, 0x44, 0x46, ..], _) => "application/pdf",
                ([0x67, 0x6C, 0x54, 0x46, ..], _) => "model/gltf-binary",
                _ => "application/octet-stream"
            };
        }

        return File(stream, icerikTipi, enableRangeProcessing: true);
    }

    [HttpPost("yukle")]
    public async Task<Cevap<MedyaModel>> Yukle([FromForm] IFormFile dosya, [FromForm] int? klasorId)
    {
        await using var stream = dosya.OpenReadStream();
        int? kullaniciId = null; // TODO: JWT'den al
        var medya = await _medyaServisi.YukleAsync(stream, dosya.FileName, klasorId, kullaniciId);
        return Cevap<MedyaModel>.Basarili(medya, "Dosya basariyla yuklendi.");
    }

    [HttpPost("youtube")]
    public async Task<Cevap<MedyaModel?>> YoutubeEkle([FromBody] YoutubeEkleIstegi istek)
    {
        var medya = await _medyaServisi.YoutubeEkleAsync(istek.Url, istek.KlasorId, null);
        return medya != null
            ? Cevap<MedyaModel?>.Basarili(medya, "YouTube videosu eklendi.")
            : Cevap<MedyaModel?>.Hata("Gecersiz YouTube URL.");
    }

    [HttpDelete("{id:long}")]
    public async Task<Cevap<bool>> Sil(long id)
    {
        await _medyaServisi.SilAsync(id);
        return Cevap<bool>.Basarili(true, "Medya silindi.");
    }

    [HttpPut("{id:long}")]
    public async Task<Cevap<MedyaModel>> Guncelle(long id, [FromBody] MedyaGuncelleIstegi istek)
    {
        if (string.IsNullOrWhiteSpace(istek.Ad))
            return Cevap<MedyaModel>.Hata("Medya adi zorunludur.");

        var medya = await _medyaServisi.GuncelleAsync(
            id,
            istek.Ad,
            istek.AltMetin,
            istek.Aciklama,
            istek.EtiketlerJson,
            istek.KlasorId);

        return medya is null
            ? Cevap<MedyaModel>.Hata("Medya bulunamadi.")
            : Cevap<MedyaModel>.Basarili(medya, "Medya guncellendi.");
    }

    [HttpGet("klasorler")]
    public async Task<Cevap<List<MedyaKlasoru>>> Klasorler()
    {
        var liste = await _medyaServisi.KlasorleriGetirAsync();
        return Cevap<List<MedyaKlasoru>>.Basarili(liste);
    }

    [HttpPost("klasor")]
    public async Task<Cevap<MedyaKlasoru>> KlasorOlustur([FromBody] KlasorOlusturIstegi istek)
    {
        var klasor = await _medyaServisi.KlasorOlusturAsync(istek.Ad, istek.UstKlasorId);
        return Cevap<MedyaKlasoru>.Basarili(klasor, "Klasor olusturuldu.");
    }

    [HttpGet("{id:long}/kullanim")]
    public async Task<Cevap<List<MedyaKullanim>>> Kullanimlar(long id)
    {
        var liste = await _medyaServisi.KullanimlariGetirAsync(id);
        return Cevap<List<MedyaKullanim>>.Basarili(liste);
    }

    // ─── Toplu yeniden boyutlandırma ────────────────────────────────────────────

    [HttpPost("toplu-yeniden-boyutlandir")]
    public async Task<Cevap<MedyaTopluIslemSonucDto>> TopluYenidenBoyutlandirAsync(
        [FromQuery] string? klasorYolu, CancellationToken iptalToken)
    {
        var wwwroot = _cevre.WebRootPath ?? Path.Combine(_cevre.ContentRootPath, "wwwroot");
        var medyaRoot = Path.GetFullPath(Path.Combine(wwwroot, "medya"));

        // Ayar değerlerini oku
        var maksimumKenar = AyarOkuInt(SistemAyariSabitleri.Resim.MaksimumKenar, 1000);
        var kalite = AyarOkuInt(SistemAyariSabitleri.Resim.Kalite, 85);
        var webpZorunlu = AyarOkuBool(SistemAyariSabitleri.Resim.WebpZorunlu, true);

        // Dosya tarama
        var aramaKlasoru = string.IsNullOrWhiteSpace(klasorYolu)
            ? medyaRoot
            : Path.GetFullPath(Path.Combine(medyaRoot, klasorYolu.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (!aramaKlasoru.StartsWith(medyaRoot) || !Directory.Exists(aramaKlasoru))
            return Cevap<MedyaTopluIslemSonucDto>.Hata("Geçersiz klasör yolu.");

        var resimUzantilari = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif", ".tiff" };

        var dosyalar = Directory.GetFiles(aramaKlasoru, "*.*", SearchOption.AllDirectories)
            .Where(f => resimUzantilari.Contains(Path.GetExtension(f)))
            .Where(f =>
            {
                var goreceliYol = Path.GetRelativePath(medyaRoot, f);
                var parcalar = goreceliYol.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                // "_yedek" ile başlayan klasörleri (yedek/geri alma amaçlı) tarama dışı bırak
                return !parcalar.Take(parcalar.Length - 1).Any(p => p.StartsWith("_yedek", StringComparison.OrdinalIgnoreCase));
            })
            .Take(500) // Maksimum 500 dosya
            .ToList();

        // Yedek klasörü
        var yedekKlasor = Path.Combine(medyaRoot, $"_toplu_yedek_{DateTime.UtcNow:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(yedekKlasor);

        var sonuc = new MedyaTopluIslemSonucDto();
        var islenenDbGuncellemeleri = new List<(string eskiYol, string yeniYol)>();

        foreach (var dosyaYolu in dosyalar)
        {
            iptalToken.ThrowIfCancellationRequested();

            try
            {
                var uzanti = Path.GetExtension(dosyaYolu).ToLowerInvariant();
                var eskiBoyut = new FileInfo(dosyaYolu).Length;
                sonuc.EskiToplamBoyut += eskiBoyut;

                var veri = await System.IO.File.ReadAllBytesAsync(dosyaYolu, iptalToken);
                var yeniVeri = await _resimIslemcisi.OtomatikBoyutlandirAsync(veri, maksimumKenar, kalite, webpZorunlu, iptalToken);

                // Ayar webpZorunlu=false ise ve girdi zaten webp değilse format/uzantı DEĞİŞMEZ.
                var hedefUzanti = (uzanti == ".webp" || webpZorunlu) ? ".webp" : uzanti;
                var uzantiDegisecekMi = hedefUzanti != uzanti;

                if (yeniVeri.Length >= eskiBoyut && !uzantiDegisecekMi)
                {
                    // Ne format değişiyor ne de boyut küçülüyor: atla
                    sonuc.YeniToplamBoyut += eskiBoyut;
                    sonuc.Atlanan++;
                    continue;
                }

                // Eski dosyayı yedekle
                var goreceYol = Path.GetRelativePath(medyaRoot, dosyaYolu);
                var yedekYol = Path.Combine(yedekKlasor, goreceYol.Replace(Path.DirectorySeparatorChar, '_'));
                Directory.CreateDirectory(Path.GetDirectoryName(yedekYol)!);
                System.IO.File.Move(dosyaYolu, yedekYol);

                // Yeni dosyayı yaz (aynı uzantı korunur, sadece webpZorunlu=true ise .webp'ye döner)
                var yeniYol = uzantiDegisecekMi ? Path.ChangeExtension(dosyaYolu, hedefUzanti) : dosyaYolu;
                await System.IO.File.WriteAllBytesAsync(yeniYol, yeniVeri, iptalToken);

                sonuc.YeniToplamBoyut += yeniVeri.Length;
                sonuc.Islenen++;

                if (uzantiDegisecekMi)
                {
                    // DB güncellemesi için kaydet (eski uzantı → yeni .webp)
                    var dbdekiYol = "/" + Path.GetRelativePath(wwwroot, dosyaYolu).Replace('\\', '/');
                    var dbYeniYol = "/" + Path.GetRelativePath(wwwroot, yeniYol).Replace('\\', '/');
                    islenenDbGuncellemeleri.Add((dbdekiYol, dbYeniYol));
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                sonuc.Hata++;
                sonuc.Hatalar.Add($"{Path.GetFileName(dosyaYolu)}: {ex.Message}");
                sonuc.YeniToplamBoyut += new FileInfo(dosyaYolu).Length; // eski boyutu ekle
            }
        }

        // DB güncellemeleri
        foreach (var (eskiYol, yeniYol) in islenenDbGuncellemeleri)
        {
            var medyaKayitlari = await _db.Medyalar
                .Where(m => m.DosyaYolu == eskiYol && !m.SilindiMi)
                .ToListAsync(iptalToken);

            foreach (var m in medyaKayitlari)
            {
                m.DosyaYolu = yeniYol;
                m.MimeTipi = "image/webp";
                m.GuncellenmeTarihi = DateTime.UtcNow;
            }
        }

        if (islenenDbGuncellemeleri.Any())
            await _db.SaveChangesAsync(iptalToken);

        return Cevap<MedyaTopluIslemSonucDto>.Basarili(sonuc,
            $"Toplu işlem tamamlandı. İşlenen: {sonuc.Islenen}, Atlanan: {sonuc.Atlanan}, Hata: {sonuc.Hata}");
    }

    private int AyarOkuInt(string anahtar, int varsayilan)
    {
        var ayar = _db.SistemAyarlari.AsNoTracking().FirstOrDefault(a => a.Anahtar == anahtar);
        return ayar != null && int.TryParse(ayar.Deger, out var deger) ? deger : varsayilan;
    }

    private bool AyarOkuBool(string anahtar, bool varsayilan)
    {
        var ayar = _db.SistemAyarlari.AsNoTracking().FirstOrDefault(a => a.Anahtar == anahtar);
        return ayar != null && bool.TryParse(ayar.Deger, out var deger) ? deger : varsayilan;
    }

    // ─── wwwroot fiziksel dosya yönetimi ───────────────────────────────────────

    [HttpGet("wwwroot-tara")]
    public IActionResult WwwrootTara([FromQuery] string? dizin, [FromQuery] string? q, [FromQuery] string? tip)
    {
        var wwwroot = _cevre.WebRootPath ?? Path.Combine(_cevre.ContentRootPath, "wwwroot");
        var medyaRoot = Path.GetFullPath(Path.Combine(wwwroot, "medya"));

        var aramaKlasoru = string.IsNullOrWhiteSpace(dizin) || dizin == "."
            ? medyaRoot
            : Path.GetFullPath(Path.Combine(medyaRoot, dizin.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (!aramaKlasoru.StartsWith(medyaRoot) || !Directory.Exists(aramaKlasoru))
            return Ok(Cevap<List<DosyaTaramaItem>>.Basarili([]));

        var dosyalar = Directory.GetFiles(aramaKlasoru, "*.*", SearchOption.AllDirectories);
        var sonuclar = new List<DosyaTaramaItem>();

        foreach (var dosya in dosyalar)
        {
            var info = new FileInfo(dosya);
            var gorecekYol = "/" + Path.GetRelativePath(wwwroot, dosya).Replace('\\', '/');
            var uzanti = info.Extension.ToLower();
            var dosyaTipi = uzanti switch
            {
                ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".svg" or ".avif" or ".bmp" => "Resim",
                ".pdf" => "PDF",
                ".glb" or ".gltf" => "3D",
                ".mp4" or ".webm" or ".mov" or ".avi" => "Video",
                _ => "Diger"
            };

            if (!string.IsNullOrWhiteSpace(tip) && tip != "Tumu" && dosyaTipi != tip)
                continue;
            if (!string.IsNullOrWhiteSpace(q) && !info.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                && !gorecekYol.Contains(q, StringComparison.OrdinalIgnoreCase))
                continue;

            sonuclar.Add(new DosyaTaramaItem
            {
                Ad = info.Name,
                Yol = gorecekYol,
                BoyutByte = info.Length,
                Tip = dosyaTipi,
                OlusturmaTarihi = info.CreationTimeUtc,
                Klasor = Path.GetRelativePath(medyaRoot, info.DirectoryName!).Replace('\\', '/')
            });
        }

        var liste = sonuclar.OrderBy(s => s.Klasor).ThenBy(s => s.Ad).ToList();
        return Ok(Cevap<List<DosyaTaramaItem>>.Basarili(liste));
    }

    [HttpDelete("wwwroot-sil")]
    public IActionResult WwwrootSil([FromQuery] string yol)
    {
        if (string.IsNullOrWhiteSpace(yol) || yol.Contains(".."))
            return BadRequest("Gecersiz dosya yolu.");

        var wwwroot = _cevre.WebRootPath ?? Path.Combine(_cevre.ContentRootPath, "wwwroot");
        var medyaRoot = Path.GetFullPath(Path.Combine(wwwroot, "medya"));
        var tamYol = Path.GetFullPath(Path.Combine(wwwroot, yol.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (!tamYol.StartsWith(medyaRoot))
            return BadRequest("Yalnizca medya/ altindaki dosyalar silinebilir.");

        if (!System.IO.File.Exists(tamYol))
            return NotFound("Dosya bulunamadi.");

        System.IO.File.Delete(tamYol);
        return Ok(Cevap<bool>.Basarili(true, "Dosya silindi."));
    }

    [HttpGet("wwwroot-dizinler")]
    public IActionResult WwwrootDizinler()
    {
        var wwwroot = _cevre.WebRootPath ?? Path.Combine(_cevre.ContentRootPath, "wwwroot");
        var medyaRoot = Path.Combine(wwwroot, "medya");
        if (!Directory.Exists(medyaRoot)) return Ok(new List<string>());

        var dizinler = Directory.GetDirectories(medyaRoot, "*", SearchOption.AllDirectories)
            .Select(d => Path.GetRelativePath(medyaRoot, d).Replace('\\', '/'))
            .OrderBy(d => d)
            .ToList();

        return Ok(Cevap<List<string>>.Basarili(dizinler));
    }
}

public class DosyaTaramaItem
{
    public string Ad { get; set; } = "";
    public string Yol { get; set; } = "";
    public long BoyutByte { get; set; }
    public string Tip { get; set; } = "";
    public DateTime OlusturmaTarihi { get; set; }
    public string Klasor { get; set; } = "";
}

public class YoutubeEkleIstegi { public string Url { get; set; } = ""; public int? KlasorId { get; set; } }
public class KlasorOlusturIstegi { public string Ad { get; set; } = ""; public int? UstKlasorId { get; set; } }
public class MedyaGuncelleIstegi
{
    public string Ad { get; set; } = "";
    public string? AltMetin { get; set; }
    public string? Aciklama { get; set; }
    public string? EtiketlerJson { get; set; }
    public int? KlasorId { get; set; }
}
