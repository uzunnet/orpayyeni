using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Sabitler;
using Microsoft.EntityFrameworkCore;
using MedyaModel = VizitLink3D.Ortak.Modeller.Medya.Medya;

namespace VizitLink3D.Api.Moduller.Medya.Servisler;

public interface IMedyaServisi
{
    Task<MedyaModel> YukleAsync(Stream dosya, string orijinalAd, int? klasorId, int? kullaniciId, CancellationToken iptal = default);
    Task<MedyaModel?> YoutubeEkleAsync(string url, int? klasorId, int? kullaniciId, CancellationToken iptal = default);
    Task SilAsync(long id, CancellationToken iptal = default);
    Task<MedyaModel?> GuncelleAsync(long id, string ad, string? altMetin, string? aciklama, string? etiketlerJson, int? klasorId, CancellationToken iptal = default);
    Task<List<MedyaModel>> ListeleAsync(int? klasorId = null, string? q = null, string? tip = null, int sayfa = 1, int sayfaBoyutu = 50, CancellationToken iptal = default);
    Task<List<MedyaKlasoru>> KlasorleriGetirAsync();
    Task<MedyaKlasoru> KlasorOlusturAsync(string ad, int? ustKlasorId);
    Task<List<MedyaKullanim>> KullanimlariGetirAsync(long medyaId);
}

public class MedyaServisi : IMedyaServisi
{
    private readonly IDepolamaAdaptoru _depolama;
    private readonly IResimIslemcisi _resimIslemcisi;
    private readonly IYoutubeMetadataServisi _youtubeServisi;
    private readonly VizitLink3DDbContext _db;

    private static readonly HashSet<string> ResimMimeTipleri = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/bmp", "image/gif", "image/tiff"
    };

    public MedyaServisi(IDepolamaAdaptoru depolama, IResimIslemcisi resimIslemcisi, IYoutubeMetadataServisi youtubeServisi, VizitLink3DDbContext db)
    {
        _depolama = depolama;
        _resimIslemcisi = resimIslemcisi;
        _youtubeServisi = youtubeServisi;
        _db = db;
    }

    public async Task<MedyaModel> YukleAsync(Stream dosya, string orijinalAd, int? klasorId, int? kullaniciId, CancellationToken iptal = default)
    {
        var uzanti = Path.GetExtension(orijinalAd).ToLowerInvariant();
        var mimeTipi = $"image/{uzanti.TrimStart('.')}";
        var resimMi = ResimMimeTipleri.Contains(mimeTipi);

        // Resim ise önce buffer'a al, optimize et
        byte[] veri;
        if (resimMi)
        {
            using var ms = new MemoryStream();
            await dosya.CopyToAsync(ms, iptal);
            veri = ms.ToArray();

            var maksimumKenar = AyarOkuInt(SistemAyariSabitleri.Resim.MaksimumKenar, 1000);
            var kalite = AyarOkuInt(SistemAyariSabitleri.Resim.Kalite, 85);
            var webpZorunlu = AyarOkuBool(SistemAyariSabitleri.Resim.WebpZorunlu, true);

            // Boyut sınırı her zaman uygulanır.
            // Çıktı WebP olur eğer: ayar webpZorunlu=true İSE, ya da girdi zaten WebP İSE.
            // webpZorunlu=false ve girdi PNG/JPEG ise orijinal format korunur (uzantı değişmez).
            var hedefWebpMi = uzanti == ".webp" || webpZorunlu;
            veri = await _resimIslemcisi.OtomatikBoyutlandirAsync(veri, maksimumKenar, kalite, webpZorunlu, iptal);
            if (hedefWebpMi)
            {
                uzanti = ".webp";
                mimeTipi = "image/webp";
            }
        }
        else
        {
            using var ms = new MemoryStream();
            await dosya.CopyToAsync(ms, iptal);
            veri = ms.ToArray();
        }

        // Hash hesapla (yeni optimize edilmiş veriden)
        using var hashAkisi = new MemoryStream(veri);
        var hash = _resimIslemcisi.HashHesapla(hashAkisi);

        var mevcut = await _db.Medyalar.FirstOrDefaultAsync(m => m.Hash == hash && !m.SilindiMi, iptal);
        if (mevcut != null) return mevcut; // duplicate

        var guvenliAd = $"{Guid.NewGuid()}{uzanti}";
        var klasor = klasorId?.ToString() ?? "genel";

        // Optimize edilmiş veriyi depolamaya yaz
        using var yuklenecekAkis = new MemoryStream(veri);
        var depoYolu = await _depolama.YukleAsync(yuklenecekAkis, guvenliAd, klasor, iptal);

        var medya = new MedyaModel
        {
            Ad = Path.GetFileNameWithoutExtension(orijinalAd),
            OrijinalAd = orijinalAd,
            DosyaYolu = depoYolu,
            BoyutByte = veri.Length,
            Hash = hash,
            MimeTipi = mimeTipi,
            Tip = MedyaTipi.Resim,
            Kaynak = MedyaKaynagi.Yerel,
            KlasorId = klasorId,
            YukleyenKullaniciId = kullaniciId?.ToString(),
            OlusturulmaTarihi = DateTime.UtcNow
        };

        _db.Medyalar.Add(medya);
        await _db.SaveChangesAsync(iptal);
        return medya;
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

    public async Task<MedyaModel?> YoutubeEkleAsync(string url, int? klasorId, int? kullaniciId, CancellationToken iptal = default)
    {
        var bilgi = await _youtubeServisi.BilgiGetirAsync(url);
        if (bilgi == null) return null;

        var medya = new MedyaModel
        {
            Ad = bilgi.Baslik,
            KaynakUrl = bilgi.EmbedUrl,
            MiniaturYolu = bilgi.KapakResmiUrl,
            SureSaniye = bilgi.SureSaniye,
            Tip = MedyaTipi.Video,
            Kaynak = MedyaKaynagi.Youtube,
            KlasorId = klasorId,
            MimeTipi = "video/youtube",
            YukleyenKullaniciId = kullaniciId?.ToString(),
            OlusturulmaTarihi = DateTime.UtcNow
        };

        _db.Medyalar.Add(medya);
        await _db.SaveChangesAsync(iptal);
        return medya;
    }

    public async Task SilAsync(long id, CancellationToken iptal = default)
    {
        var medya = await _db.Medyalar.FindAsync([id], iptal);
        if (medya != null)
        {
            medya.SilindiMi = true;
            medya.SilinmeTarihi = DateTime.UtcNow;
            await _db.SaveChangesAsync(iptal);
        }
    }

    public async Task<MedyaModel?> GuncelleAsync(long id, string ad, string? altMetin, string? aciklama, string? etiketlerJson, int? klasorId, CancellationToken iptal = default)
    {
        var medya = await _db.Medyalar.FindAsync([id], iptal);
        if (medya is null || medya.SilindiMi)
            return null;

        medya.Ad = ad.Trim();
        medya.AltMetin = altMetin;
        medya.Aciklama = aciklama;
        medya.EtiketlerJson = etiketlerJson;
        medya.KlasorId = klasorId;
        medya.GuncellenmeTarihi = DateTime.UtcNow;

        await _db.SaveChangesAsync(iptal);
        return medya;
    }

    public async Task<List<MedyaModel>> ListeleAsync(int? klasorId = null, string? q = null, string? tip = null, int sayfa = 1, int sayfaBoyutu = 50, CancellationToken iptal = default)
    {
        var sorgu = _db.Medyalar.AsNoTracking().Where(m => !m.SilindiMi);
        if (klasorId.HasValue) sorgu = sorgu.Where(m => m.KlasorId == klasorId);
        if (!string.IsNullOrWhiteSpace(q)) sorgu = sorgu.Where(m => m.Ad.Contains(q) || (m.Aciklama != null && m.Aciklama.Contains(q)) || (m.AltMetin != null && m.AltMetin.Contains(q)));
        if (!string.IsNullOrWhiteSpace(tip)) sorgu = sorgu.Where(m => m.Tip.ToString() == tip);
        return await sorgu.OrderByDescending(m => m.OlusturulmaTarihi).Skip((sayfa - 1) * sayfaBoyutu).Take(sayfaBoyutu).ToListAsync(iptal);
    }

    public async Task<List<MedyaKlasoru>> KlasorleriGetirAsync()
        => await _db.MedyaKlasorleri.AsNoTracking().Where(k => k.AktifMi).OrderBy(k => k.SiraNo).ToListAsync();

    public async Task<MedyaKlasoru> KlasorOlusturAsync(string ad, int? ustKlasorId)
    {
        var klasor = new MedyaKlasoru { Ad = ad, UstKlasorId = ustKlasorId, OlusturulmaTarihi = DateTime.UtcNow };
        _db.MedyaKlasorleri.Add(klasor);
        await _db.SaveChangesAsync();
        return klasor;
    }

    public async Task<List<MedyaKullanim>> KullanimlariGetirAsync(long medyaId)
        => await _db.MedyaKullanimlari.AsNoTracking().Where(k => k.MedyaId == medyaId).ToListAsync();
}
