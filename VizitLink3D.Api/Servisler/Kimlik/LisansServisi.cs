using VizitLink3D.Ortak.Modeller;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.VeriTabani;

namespace VizitLink3D.Api.Servisler.Kimlik;

public class LisansServisi(VizitLink3DDbContext vt, ILogger<LisansServisi> log, IConfiguration konfigurasyon)
{
    public const string Demo = "Demo";
    public const string Yillik = "Yillik";
    public const string IkiYillik = "IkiYillik";
    public const string UcYillik = "UcYillik";
    public const string BesYillik = "BesYillik";
    public const string Suresiz = "Suresiz";
    private const int DemoGunSayisi = 14;

    /// <summary>
    /// Lisans bitişine kalan gün. Eksi değer = süresi geçmiş.
    /// </summary>
    public static int KalanGun(DateTime bitisTarihi) =>
        (bitisTarihi.Date - DateTime.UtcNow.Date).Days;

    /// <summary>
    /// Lisans durumu kontrolü. Domain için aktif lisans var mı?
    /// </summary>
    public async Task<LisansDurumu> DomainKontrolAsync(string domain)
    {
        var lisans = await vt.Lisanslar
            .AsNoTracking()
            .Include(l => l.Firma)
            .FirstOrDefaultAsync(l =>
                l.AktifMi &&
                (l.BirincilDomain == domain || l.YedekDomain == domain));

        if (lisans is null)
            return new LisansDurumu { GecerliMi = false, Sebep = "Lisans bulunamadi." };

        if (!LisansAnahtariGecerliMi(lisans))
        {
            log.LogWarning("Lisans imzasi gecersiz: {Domain}", domain);
            return new LisansDurumu { GecerliMi = false, Sebep = "Lisans anahtari gecersiz." };
        }

        if (SuresizLisansMi(lisans))
        {
            return new LisansDurumu
            {
                GecerliMi = true,
                KalanGun = int.MaxValue,
                LisansTipi = Suresiz,
                SuresizMi = true,
                DemoMu = lisans.DemoMu,
                BitisTarihi = null
            };
        }

        var kalan = KalanGun(lisans.BitisTarihi);
        if (lisans.DemoMu && kalan < 0)
        {
            return new LisansDurumu
            {
                GecerliMi = false,
                Sebep = "Demo lisans suresi doldu.",
                KalanGun = kalan,
                DemoMu = true,
                LisansTipi = Demo,
                BitisTarihi = lisans.BitisTarihi
            };
        }

        // Ek süre: Bitiş + 7 gün grace period
        var hardLockGun = int.TryParse(konfigurasyon["LisansAyarlari:HardLockGunSayisi"], out var gun) ? gun : 7;
        var ekSureBitis = lisans.BitisTarihi.AddDays(hardLockGun);
        var ekSuredeMi = DateTime.UtcNow >= lisans.BitisTarihi && DateTime.UtcNow <= ekSureBitis;

        if (kalan <= -hardLockGun)
        {
            log.LogWarning("Lisans suresi doldu ve ek sure bitti: {Domain} {Bitis}", domain, lisans.BitisTarihi);
            return new LisansDurumu
            {
                GecerliMi = false,
                Sebep = "Lisans süresi doldu. Lütfen yenileyin.",
                KalanGun = kalan,
                EkSuredeMi = false,
                DemoMu = lisans.DemoMu,
                LisansTipi = lisans.LisansTipi,
                BitisTarihi = lisans.BitisTarihi
            };
        }

        if (ekSuredeMi)
        {
            var ekGun = (ekSureBitis.Date - DateTime.UtcNow.Date).Days;
            log.LogWarning("Lisans ek suresinde: {Domain} {EkGun} gun kaldi", domain, ekGun);
            return new LisansDurumu
            {
                GecerliMi = true,
                Sebep = $"Ek süre — {ekGun} gün kaldı.",
                KalanGun = kalan,
                EkSuredeMi = true,
                DemoMu = lisans.DemoMu,
                LisansTipi = lisans.LisansTipi,
                BitisTarihi = lisans.BitisTarihi,
                EkSureBitis = ekSureBitis
            };
        }

        // Yaklasan uyari seviyeleri
        string? uyari = kalan switch
        {
            <= 1 => "Lisans yarin doluyor!",
            <= 3 => $"Lisans {kalan} gun icinde dolacak!",
            <= 7 => $"Lisans {kalan} gun kaldi.",
            <= 15 => $"Lisans suresi yaklasiyor — {kalan} gun.",
            <= 20 => null, // bilgi seviyesi
            <= 30 => null, // bilgi seviyesi
            _ => null
        };

        if (uyari != null)
            log.LogInformation("Lisans uyarisi: {Domain} — {Uyari}", domain, uyari);

        return new LisansDurumu
        {
            GecerliMi = true,
            KalanGun = kalan,
            Uyari = uyari,
            EkSuredeMi = false,
            DemoMu = lisans.DemoMu,
            LisansTipi = lisans.LisansTipi,
            BitisTarihi = lisans.BitisTarihi
        };
    }

    public static (DateTime bitisTarihi, int? sureYil, bool demoMu, bool suresizMi) PlanCoz(string lisansTipi, DateTime baslangicTarihi, DateTime? ozelBitisTarihi = null)
    {
        var tip = TipNormalizeEt(lisansTipi);
        return tip switch
        {
            Demo => (ozelBitisTarihi ?? baslangicTarihi.AddDays(DemoGunSayisi), null, true, false),
            Yillik => (ozelBitisTarihi ?? baslangicTarihi.AddYears(1), 1, false, false),
            IkiYillik => (ozelBitisTarihi ?? baslangicTarihi.AddYears(2), 2, false, false),
            UcYillik => (ozelBitisTarihi ?? baslangicTarihi.AddYears(3), 3, false, false),
            BesYillik => (ozelBitisTarihi ?? baslangicTarihi.AddYears(5), 5, false, false),
            Suresiz => (new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc), null, false, true),
            _ => (ozelBitisTarihi ?? baslangicTarihi.AddYears(1), 1, false, false)
        };
    }

    public static string TipNormalizeEt(string? lisansTipi)
    {
        var tip = (lisansTipi ?? Yillik).Trim();
        return tip.ToLowerInvariant() switch
        {
            "demo" => Demo,
            "1" or "1yil" or "1-yil" or "yillik" or "yıllık" => Yillik,
            "2" or "2yil" or "2-yil" or "ikiyillik" or "iki-yillik" => IkiYillik,
            "3" or "3yil" or "3-yil" or "ucyillik" or "uc-yillik" or "üçyıllık" => UcYillik,
            "5" or "besyillik" or "bes-yillik" => BesYillik,
            "omurboyu" or "ömürboyu" or "suresiz" or "süresiz" => Suresiz,
            _ => Yillik
        };
    }

    /// <summary>
    /// Lisans anahtari olusturur.
    /// </summary>
    public static string AnahtarUret(string firmaSlug, string domain, DateTime bitisTarihi, string gizliAnahtar)
    {
        var icerik = $"{firmaSlug}_{domain}_{bitisTarihi:yyyyMMdd}";
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(gizliAnahtar));
        var hash = Convert.ToBase64String(hmac.ComputeHash(
            System.Text.Encoding.UTF8.GetBytes(icerik)));
        return $"{icerik}_{hash}";
    }

    /// <summary>
    /// Lisans anahtari dogrular.
    /// </summary>
    public static bool AnahtarDogrula(string anahtar, string gizliAnahtar)
    {
        try
        {
            var parcalar = anahtar.Split('_');
            if (parcalar.Length < 4) return false;
            var beklenen = AnahtarUret(parcalar[0], parcalar[1],
                DateTime.ParseExact(parcalar[2], "yyyyMMdd", null), gizliAnahtar);
            return anahtar == beklenen;
        }
        catch { return false; }
    }

    private bool LisansAnahtariGecerliMi(Lisans lisans)
    {
        if (string.IsNullOrWhiteSpace(lisans.LisansAnahtari))
        {
            return true;
        }

        var gizliAnahtar = Environment.GetEnvironmentVariable("VIZITLINK3D_LISANS_KEY")
            ?? konfigurasyon["LisansAyarlari:GizliAnahtar"];

        return !string.IsNullOrWhiteSpace(gizliAnahtar)
            && AnahtarDogrula(lisans.LisansAnahtari, gizliAnahtar);
    }

    private static bool SuresizLisansMi(Lisans lisans)
        => lisans.SuresizMi || TipNormalizeEt(lisans.LisansTipi) == Suresiz;
}

public class LisansDurumu
{
    public bool GecerliMi { get; set; }
    public string Sebep { get; set; } = string.Empty;
    public int KalanGun { get; set; }
    public bool EkSuredeMi { get; set; }
    public bool SuresizMi { get; set; }
    public bool DemoMu { get; set; }
    public string LisansTipi { get; set; } = string.Empty;
    public string? Uyari { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public DateTime? EkSureBitis { get; set; }
}
