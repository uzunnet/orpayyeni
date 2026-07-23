using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Rol
{
    Kullanici = 0,
    Editor = 1,
    Admin = 2,
    SuperAdmin = 3
}

/// <summary>
/// Sistem kullanicilarini temsil eder. Yonetici, editor ve musteri rollerini kapsar.
/// Hassas alanlar (SifreHash, PinHash, DesenHash, vb.) API yanitinda
/// kesinlikle gorunmez — JsonIgnore ile korunmaktadir (anayasa §3.4).
/// </summary>
public class Kullanici
{
    public int Id { get; set; }

    // Coklu Kiraci
    public int? FirmaId { get; set; }
    [JsonIgnore]
    public Firma? Firma { get; set; }

    // Kimlik Bilgileri
    public string AdSoyad { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;

    // Guvenlik (JsonIgnore ZORUNLU)
    [JsonIgnore]
    public string SifreHash { get; set; } = string.Empty;

    [JsonIgnore]
    public string? PinHash { get; set; }

    [JsonIgnore]
    public string? DesenHash { get; set; }

    [JsonIgnore]
    public string? WebAuthnPublicKey { get; set; }

    [JsonIgnore]
    public string? SifreSifirlamaToken { get; set; }

    [JsonIgnore]
    public DateTime? TokenGecerlilikTarihi { get; set; }

    [JsonIgnore]
    public string? EmailDogrulamaToken { get; set; }

    [JsonIgnore]
    public string? TotpAnahtari { get; set; }

    [JsonIgnore]
    public string? RefreshToken { get; set; }

    [JsonIgnore]
    public DateTime? RefreshTokenBitisTarihi { get; set; }

    // Rol ve Yetki
    public Rol Rol { get; set; } = Rol.Kullanici;

    // Dogrulama
    public bool EmailDogrulandiMi { get; set; }
    public bool IkiAdimDogrulamaAktif { get; set; }
    public bool TelefonDogrulandiMi { get; set; }

    // Hesap Durumu
    public bool AktifMi { get; set; } = true;
    public bool KilitlendiMi { get; set; }
    public int BasarisizGirisDenemesi { get; set; }
    public DateTime? KilitAcmaTarihi { get; set; }

    // Oturum
    public DateTime? SonGirisTarihi { get; set; }
    public string? SonGirisIP { get; set; }

    // Profil
    public string? ProfilResmiUrl { get; set; }
    public string? TercihEdilenDil { get; set; } = "tr";

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
