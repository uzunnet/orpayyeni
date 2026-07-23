using System.Security.Cryptography;
using System.Text.RegularExpressions;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Servisler;

/// <summary>
/// wwwroot/medya altindaki standart isimli (thumb_{N}.jpg, nrd_{N}*.glb) NRD kapak
/// dosyalarini, slug'i "nrd-{N}" olan urunlere ana gorsel ve 3D model olarak baglar.
/// Idempotent: var olan Medya/UcBoyut kayitlarini cogaltmaz, mevcut baglantilari korur.
/// </summary>
public partial class UrunMedyaBaglamaServisi(VizitLink3DDbContext vt, IWebHostEnvironment env)
{
    [GeneratedRegex(@"^nrd-(\d+)$")]
    private static partial Regex SlugNumaraDeseni();
    [GeneratedRegex(@"^nrd-(cam|boy-kpk)-(\d+)$")]
    private static partial Regex SlugCamBoyDeseni();

    public async Task<Cevap<UrunMedyaBaglamaSonucu>> BaglaAsync(CancellationToken iptal = default)
    {
        var sonuc = new UrunMedyaBaglamaSonucu();

        if (string.IsNullOrEmpty(env.WebRootPath))
            return Cevap<UrunMedyaBaglamaSonucu>.Basarili(sonuc, "WebRootPath tanimli degal, medya baglama atlandi.");

        var kapaklarKlasor = Path.Combine(env.WebRootPath, "medya", "kapaklar");
        var glbKlasor = Path.Combine(env.WebRootPath, "medya", "3d");

        var urunler = await vt.Urunler.IgnoreQueryFilters()
            .Where(u => !u.SilindiMi)
            .ToListAsync(iptal);

        foreach (var urun in urunler)
        {
            var eslesme = SlugNumaraDeseni().Match(urun.Slug);
            if (!eslesme.Success) continue; // sadece duz "nrd-{N}" serisi (cam/boy haric)

            var numara = eslesme.Groups[1].Value;
            bool degisti = false;

            // --- 1) Ana gorsel: thumb_{N}.jpg ---
            var thumbTam = Path.Combine(kapaklarKlasor, $"thumb_{numara}.jpg");
            if (File.Exists(thumbTam))
            {
                var medya = await MedyaGetirVeyaOlusturAsync(
                    thumbTam, $"medya/kapaklar/thumb_{numara}.jpg", "image/jpeg", iptal);
                if (urun.AnaGorselMedyaId != medya.Id)
                {
                    urun.AnaGorselMedyaId = medya.Id;
                    degisti = true;
                    sonuc.BaglananGorsel++;
                }
            }

            // --- 2) 3D modeller: nrd_{N}.glb (varsayilan) + nrd_{N}_*.glb (varyant) ---
            if (Directory.Exists(glbKlasor))
            {
                var sadeAd = $"nrd_{numara}.glb";
                var glbDosyalar = Directory.GetFiles(glbKlasor, $"nrd_{numara}*.glb")
                    .Where(f => Path.GetFileName(f) == sadeAd ||
                                Path.GetFileName(f).StartsWith($"nrd_{numara}_"))
                    .OrderBy(f => Path.GetFileName(f) == sadeAd ? 0 : 1) // sade once
                    .ToList();

                UrunUcBoyutModeli? varsayilan = null;
                foreach (var glb in glbDosyalar)
                {
                    var ad = Path.GetFileName(glb);
                    bool sadeMi = ad == sadeAd;
                    var ucBoyut = await UcBoyutGetirVeyaOlusturAsync(
                        urun.Id, glb, $"/medya/3d/{ad}", sadeMi, iptal);
                    sonuc.BaglananModel += ucBoyut.YeniMi ? 1 : 0;
                    if (sadeMi) varsayilan = ucBoyut.Model;
                }

                varsayilan ??= (await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
                    .Where(m => m.UrunId == urun.Id && !m.SilindiMi)
                    .OrderByDescending(m => m.VarsayilanMi)
                    .FirstOrDefaultAsync(iptal));

                if (varsayilan is not null && urun.VarsayilanUcBoyutModeliId != varsayilan.Id)
                {
                    urun.VarsayilanUcBoyutModeliId = varsayilan.Id;
                    degisti = true;
                }
            }

            if (degisti)
            {
                urun.GuncellenmeTarihi = DateTime.UtcNow;
                sonuc.GuncellenenUrun++;
            }
        }

        // 3D modeli olan ama VarsayilanUcBoyutModeliId atanmamis urunleri duzelt
        var eksikUrunler = await vt.Urunler.IgnoreQueryFilters()
            .Where(u => !u.SilindiMi && u.VarsayilanUcBoyutModeliId == null)
            .ToListAsync(iptal);
        foreach (var urun in eksikUrunler)
        {
            var varsayilanModel = await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
                .Where(m => m.UrunId == urun.Id && !m.SilindiMi)
                .OrderByDescending(m => m.VarsayilanMi)
                .FirstOrDefaultAsync(iptal);
            if (varsayilanModel is not null)
            {
                urun.VarsayilanUcBoyutModeliId = varsayilanModel.Id;
                sonuc.GuncellenenUrun++;
            }
        }

        await vt.SaveChangesAsync(iptal);
        return Cevap<UrunMedyaBaglamaSonucu>.Basarili(sonuc,
            $"Urun medya baglama tamamlandi. {sonuc.GuncellenenUrun} urun, {sonuc.BaglananGorsel} gorsel, {sonuc.BaglananModel} 3D model.");
    }

    private async Task<Medya> MedyaGetirVeyaOlusturAsync(
        string tamYol, string bagilYol, string mime, CancellationToken iptal)
    {
        var mevcut = await vt.Medyalar.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.DosyaYolu == bagilYol && !m.SilindiMi, iptal);
        if (mevcut is not null) return mevcut;

        string hash;
        await using (var stream = File.OpenRead(tamYol))
            hash = Convert.ToHexStringLower(SHA256.HashData(stream));

        var medya = new Medya
        {
            Ad = Path.GetFileNameWithoutExtension(bagilYol),
            OrijinalAd = Path.GetFileName(bagilYol),
            DosyaYolu = bagilYol,
            Tip = MedyaTipi.Resim,
            Kaynak = MedyaKaynagi.Yerel,
            BoyutByte = new FileInfo(tamYol).Length,
            MimeTipi = mime,
            Hash = hash,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.Medyalar.Add(medya);
        await vt.SaveChangesAsync(iptal); // Id almak icin
        return medya;
    }

    private async Task<(UrunUcBoyutModeli Model, bool YeniMi)> UcBoyutGetirVeyaOlusturAsync(
        int urunId, string tamYol, string bagilYol, bool varsayilan, CancellationToken iptal)
    {
        var mevcut = await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.UrunId == urunId && m.ModelDosyaYolu == bagilYol && !m.SilindiMi, iptal);
        if (mevcut is not null) return (mevcut, false);

        var model = new UrunUcBoyutModeli
        {
            UrunId = urunId,
            ModelAdi = Path.GetFileNameWithoutExtension(bagilYol),
            ModelDosyaYolu = bagilYol,
            ModelYolu = bagilYol,
            ModelTipi = "Glb",
            DosyaBoyutuByte = new FileInfo(tamYol).Length,
            VarsayilanMi = varsayilan,
            Versiyon = 1,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.UrunUcBoyutModelleri.Add(model);
        await vt.SaveChangesAsync(iptal); // Id almak icin
        return (model, true);
    }

    /// <summary>
    /// Static overload — disaridan (TohumVerisi vb.) dogrudan cagrilabilir.
    /// </summary>
    internal static async Task<(UrunUcBoyutModeli Model, bool YeniMi)> UcBoyutGetirVeyaOlusturAsync(
        VizitLink3DDbContext vt, int urunId, string tamYol, string bagilYol, bool varsayilan, CancellationToken iptal)
    {
        var mevcut = await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.UrunId == urunId && m.ModelDosyaYolu == bagilYol && !m.SilindiMi, iptal);
        if (mevcut is not null) return (mevcut, false);

        var model = new UrunUcBoyutModeli
        {
            UrunId = urunId,
            ModelAdi = Path.GetFileNameWithoutExtension(bagilYol),
            ModelDosyaYolu = bagilYol,
            ModelYolu = bagilYol,
            ModelTipi = "Glb",
            DosyaBoyutuByte = new FileInfo(tamYol).Length,
            VarsayilanMi = varsayilan,
            Versiyon = 1,
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        };
        vt.UrunUcBoyutModelleri.Add(model);
        await vt.SaveChangesAsync(iptal);
        return (model, true);
    }

    /// <summary>
    /// 3D model dosya adini temizler: uzanti kaldir, kucuk harf, bosluk->tire, altcizgi->tire.
    /// </summary>
    private static string TemizleDosyaAdi(string dosyaAdi)
    {
        var ad = Path.GetFileNameWithoutExtension(dosyaAdi);
        ad = ad.Replace(' ', '-').Replace('_', '-').ToLowerInvariant();
        while (ad.Contains("--", StringComparison.Ordinal)) ad = ad.Replace("--", "-");
        ad = ad.Trim('-');
        return ad;
    }

    /// <summary>
    /// wwwroot/medya/3d-modeller/ altindaki tum .glb dosyalarini katalog slug'lari ile
    /// eslestirip UrunUcBoyutModeli kaydi olusturur.
    /// Idempotent: mevcut kaydi tekrar eklemez.
    /// Mevcut BaglaAsync() mantigini bozmaz — ayri bir metottur.
    /// </summary>
    public static async Task<Cevap<KatalogModelBaglamaSonucu>> KatalogModelleriniBaglaAsync(
        VizitLink3DDbContext vt, string webRootPath, CancellationToken iptal = default)
    {
        var sonuc = new KatalogModelBaglamaSonucu();

        var ucBoyutKlasor = Path.Combine(webRootPath, "medya", "3d-modeller");
        if (!Directory.Exists(ucBoyutKlasor))
            return Cevap<KatalogModelBaglamaSonucu>.Basarili(sonuc,
                $"3D model klasoru bulunamadi: {ucBoyutKlasor}");

        // Tum .glb dosyalarini bul (alt klasorler dahil)
        var glbDosyalari = Directory.GetFiles(ucBoyutKlasor, "*.glb", SearchOption.AllDirectories);
        if (glbDosyalari.Length == 0)
            return Cevap<KatalogModelBaglamaSonucu>.Basarili(sonuc, "3D model dosyasi bulunamadi.");

        // Tum aktif urunleri yukle
        var urunler = await vt.Urunler
            .IgnoreQueryFilters()
            .Where(u => !u.SilindiMi)
            .ToListAsync(iptal);

        var slugUrunMap = urunler.ToDictionary(u => u.Slug, StringComparer.OrdinalIgnoreCase);
        var slugSet = new HashSet<string>(slugUrunMap.Keys, StringComparer.OrdinalIgnoreCase);

        foreach (var glbDosyasi in glbDosyalari)
        {
            var dosyaAdi = Path.GetFileName(glbDosyasi);
            var temizAd = TemizleDosyaAdi(dosyaAdi);

            Urun? eslesenUrun = null;
            bool varsayilanMi = false;

            // --- Strateji 1: Tam eslesme (dosya_adi == slug) ---
            if (slugSet.Contains(temizAd))
            {
                eslesenUrun = slugUrunMap[temizAd];
                varsayilanMi = true;
            }
            // --- Strateji 2: Onek eslesmesi (slug, temizAd + "-" ile basliyor) ---
            // Orn: piedra.glb -> "piedra", slug "piedra-55" ile eslesir
            else
            {
                var onekEslesen = urunler.FirstOrDefault(u =>
                    u.Slug.StartsWith(temizAd + "-", StringComparison.OrdinalIgnoreCase));
                if (onekEslesen is not null)
                {
                    eslesenUrun = onekEslesen;
                    varsayilanMi = true; // ilk eslesen varsayilan olsun
                }
                // --- Strateji 3: nrd-{N} regex (katalog alti dosyalar) ---
                else
                {
                    var nrdMatch = Regex.Match(temizAd, @"^nrd-(\d+)$", RegexOptions.IgnoreCase);
                    if (nrdMatch.Success)
                    {
                        var nrdSlug = $"nrd-{nrdMatch.Groups[1].Value}";
                        if (slugSet.Contains(nrdSlug))
                        {
                            eslesenUrun = slugUrunMap[nrdSlug];
                            varsayilanMi = true;
                        }
                    }
                }
            }

            if (eslesenUrun is null)
            {
                sonuc.EslesmeyenDosyalar.Add(dosyaAdi);
                continue;
            }

            // Bagil yolu hesapla (webRootPath'e gore)
            var goreliYol = Path.GetRelativePath(webRootPath, glbDosyasi)
                .Replace('\\', '/')
                .TrimStart('/');
            var bagilYol = "/" + goreliYol;

            // Idempotent: mevcut kaydi tekrar ekleme
            var (model, yeniMi) = await UcBoyutGetirVeyaOlusturAsync(
                vt, eslesenUrun.Id, glbDosyasi, bagilYol, varsayilanMi, iptal);

            if (yeniMi)
            {
                sonuc.BaglananModel++;
                sonuc.BaglananUrunSluglari.Add(eslesenUrun.Slug);
            }

            // Varsayilan model ata (ilk/tek model icin)
            if (varsayilanMi && eslesenUrun.VarsayilanUcBoyutModeliId != model.Id)
            {
                eslesenUrun.VarsayilanUcBoyutModeliId = model.Id;
                eslesenUrun.GuncellenmeTarihi = DateTime.UtcNow;
                sonuc.GuncellenenUrun++;
            }
        }

        // Varsayilan model atanmamis urunleri de düzelt
        var eksikUrunler = await vt.Urunler.IgnoreQueryFilters()
            .Where(u => !u.SilindiMi && u.VarsayilanUcBoyutModeliId == null)
            .ToListAsync(iptal);
        foreach (var urun in eksikUrunler)
        {
            if (slugSet.Contains(urun.Slug))
            {
                var varsayilanModel = await vt.UrunUcBoyutModelleri.IgnoreQueryFilters()
                    .Where(m => m.UrunId == urun.Id && !m.SilindiMi)
                    .OrderByDescending(m => m.VarsayilanMi)
                    .FirstOrDefaultAsync(iptal);
                if (varsayilanModel is not null)
                {
                    urun.VarsayilanUcBoyutModeliId = varsayilanModel.Id;
                    sonuc.GuncellenenUrun++;
                }
            }
        }

        // Dosya adi tekrarlarini raporla
        sonuc.TekrarlananDosyalar = glbDosyalari
            .GroupBy(f => TemizleDosyaAdi(Path.GetFileName(f)))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key} ({g.Count()} kez: {string.Join(", ", g.Select(Path.GetFileName))})")
            .ToList();

        await vt.SaveChangesAsync(iptal);
        return Cevap<KatalogModelBaglamaSonucu>.Basarili(sonuc,
            $"3D model baglama tamamlandi. {sonuc.BaglananModel} yeni model baglandi, " +
            $"{sonuc.GuncellenenUrun} urun guncellendi, {sonuc.EslesmeyenDosyalar.Count} dosya eslesmedi.");
    }
}

public class UrunMedyaBaglamaSonucu
{
    public int GuncellenenUrun { get; set; }
    public int BaglananGorsel { get; set; }
    public int BaglananModel { get; set; }
}

/// <summary>
/// KatalogModelleriniBaglaAsync sonucu — eslesmeyen dosyalari da icerir.
/// </summary>
public class KatalogModelBaglamaSonucu
{
    public int GuncellenenUrun { get; set; }
    public int BaglananModel { get; set; }
    public List<string> BaglananUrunSluglari { get; set; } = [];
    public List<string> EslesmeyenDosyalar { get; set; } = [];
    public List<string> TekrarlananDosyalar { get; set; } = [];
}
