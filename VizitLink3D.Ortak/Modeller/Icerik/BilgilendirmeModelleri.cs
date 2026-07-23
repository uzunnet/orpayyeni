using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Sikca sorulan sorular.
/// </summary>
public class SikSorulanSoru
{
    public int Id { get; set; }
    public string Soru { get; set; } = string.Empty;
    public string Cevap { get; set; } = string.Empty;
    public string? KategoriAdi { get; set; } // Genel, Urun, Hizmet, Garanti
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public int GoruntulemeSayisi { get; set; }
    public bool FaydaliMi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// Kalite belgeleri ve sertifikalar.
/// </summary>
public class Sertifika
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? Resim { get; set; }
    public string? PdfDosya { get; set; }
    public DateTime? VerilmeTarihi { get; set; }
    public DateTime? GecerlilikTarihi { get; set; }
    public string? VerenKurum { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// PDF katalog dosyalari.
/// </summary>
public class Katalog
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? KapakResim { get; set; }
    public string PdfDosyaYolu { get; set; } = string.Empty;
    public double? DosyaBoyutuMb { get; set; }
    public int? SayfaSayisi { get; set; }
    public int? Yil { get; set; }
    public int IndirilmeSayisi { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// E-bulten aboneleri.
/// </summary>
public class BultenAbonesi
{
    public int Id { get; set; }
    public string Eposta { get; set; } = string.Empty;
    public string? AdSoyad { get; set; }
    public DateTime AbonelikTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? IptalTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
    public string? DogrulamaToken { get; set; }
    public bool DogrulandiMi { get; set; }
    public string? KaynakSayfa { get; set; }
    [JsonIgnore] public string? IPAdresi { get; set; }
}

/// <summary>
/// E-posta sablonlari.
/// </summary>
public class EpostaSablonu
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Konu { get; set; } = string.Empty;
    public string IcerikHtml { get; set; } = string.Empty;
    public string? Tip { get; set; } // HosGeldin, Dogrulama, IletisimCevabi, Bulten
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sube / Showroom bilgileri.
/// </summary>
public class Sube
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Adres { get; set; }
    public string? Sehir { get; set; }
    public string? Ilce { get; set; }
    public string? Telefon { get; set; }
    public string? Eposta { get; set; }
    public double? Enlem { get; set; }
    public double? Boylam { get; set; }
    public string? CalismaSaatleri { get; set; }
    public string? Aciklama { get; set; }
    public string? SubeYetkilisi { get; set; }
    public string? SubeYetkilisiTelefon { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// Ekip uyeleri (Hakkimizda sayfasi icin).
/// </summary>
public class EkipUyesi
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string? Unvan { get; set; }
    public string? Bio { get; set; }
    public string? Resim { get; set; }
    public string? Linkedin { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// Site genel ayarlari (key-value tabanli).
/// </summary>
public class SistemAyari
{
    public int Id { get; set; }
    public string Anahtar { get; set; } = string.Empty;
    public string Deger { get; set; } = string.Empty;
    public string? Tip { get; set; } = "string"; // string, int, bool, json
    public string? Aciklama { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}

/// <summary>
/// Tanitim videolari.
/// </summary>
public class TanitimVideo
{
    public int Id { get; set; }
    public string? Baslik { get; set; }
    public string? Aciklama { get; set; }
    public string? VideoUrl { get; set; }
    public string? KapakResim { get; set; }
    public int? SureSaniye { get; set; }
    public int GoruntulemeSayisi { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
