using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Coklu dil ceviri sistemi. Tum UI metinleri veritabaninda tutulur
/// ve FusionCache ile onbelleklenir (anayasa §35).
/// Anahtar + Dil composite unique index olmalidir.
/// </summary>
public class Ceviri
{
    public int Id { get; set; }
    public string Anahtar { get; set; } = string.Empty;
    public string Dil { get; set; } = "tr";
    public string Deger { get; set; } = string.Empty;
    public string? Bolum { get; set; } // anasayfa, iletisim, ortak, admin
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}

/// <summary>
/// Desteklenen diller.
/// </summary>
public class Dil
{
    public int Id { get; set; }
    public string Kod { get; set; } = "tr"; // tr, en, ar, de
    public string Ad { get; set; } = string.Empty; // Turkce, English
    public string? Bayrak { get; set; } // 🇹🇷
    public int SiraNo { get; set; }
    public bool VarsayilanMi { get; set; }
    public bool AktifMi { get; set; } = true;
}

/// <summary>
/// Lisans modeli (anayasa §5.2).
/// Musteri yazilimlarinin yetkisiz kullanimini engellemek icin
/// cift katmanli: Domain kilidi + Sure kilidi + HMAC imza.
/// </summary>
public class Lisans
{
    public int Id { get; set; }
    public int FirmaId { get; set; }

    [JsonIgnore]
    public Firma? Firma { get; set; }

    // Domain kilitleri (max 2)
    public string BirincilDomain { get; set; } = string.Empty;
    public string? YedekDomain { get; set; }

    // Sure
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }

    // Tip
    public string LisansTipi { get; set; } = "Yillik"; // Demo, Yillik, IkiYillik, UcYillik, BesYillik, Suresiz
    public int? SureYil { get; set; }
    public bool SuresizMi { get; set; }
    public bool DemoMu { get; set; }

    // Dogrulama
    public string LisansAnahtari { get; set; } = string.Empty; // HMAC-SHA256 imzali
    public bool AktifMi { get; set; } = true;
    public DateTime? SonDogrulamaTarihi { get; set; }

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}

/// <summary>
/// Degismez denetim kaydi (anayasa §33.3).
/// Append-only — silinemez, degistirilemez. Sadece INSERT yapilir.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public DateTime ZamanDamgasi { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public string? KullaniciId { get; set; }
    public string? FirmaId { get; set; }
    public string Eylem { get; set; } = string.Empty; // "Kapi.Olusturuldu"
    public string? EskiDeger { get; set; } // JSON
    public string? YeniDeger { get; set; } // JSON
    [JsonIgnore]
    public string? IPAdresi { get; set; }
    public string? Tarayici { get; set; }
    public string? ImzaHash { get; set; } // Kayit butunlugu kontrolu
}

/// <summary>
/// Ziyaretci analitigi.
/// </summary>
public class ZiyaretKaydi
{
    public long Id { get; set; }
    public DateTime Tarih { get; set; } = DateTime.UtcNow;
    [JsonIgnore]
    public string? IP { get; set; }
    public string? Sayfa { get; set; }
    public string? Referer { get; set; }
    public string? Tarayici { get; set; }
    public string? Cihaz { get; set; }
    public string? Sehir { get; set; }
    public string? Ulke { get; set; }
    public int? OturumSuresi { get; set; }
}
