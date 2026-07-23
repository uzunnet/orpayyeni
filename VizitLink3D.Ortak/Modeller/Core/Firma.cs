using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Firma (kiracı) modelidir. SaaS-ready mimaride her firma kendi izole
/// verisine sahiptir. VizitLink3D baslangicta tek firma olarak calisir.
/// Anayasa §4 geregi tum sorgularda FirmaId filtresi zorunludur.
/// </summary>
public class Firma
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Unvan { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? AciklamaKisa { get; set; }
    public string? Aciklama { get; set; }

    // Iletisim
    public string? Domain { get; set; }
    public string? YedekDomain { get; set; }
    public string? Logo { get; set; }
    public string? Favicon { get; set; }
    public string? Eposta { get; set; }
    public string? Telefon1 { get; set; }
    public string? Telefon2 { get; set; }
    public string? Whatsapp { get; set; }
    public string? Adres { get; set; }
    public string? Sehir { get; set; }
    public string? Ilce { get; set; }
    public string? PostaKodu { get; set; }
    public string? Ulke { get; set; }

    // Harita
    public double? Enlem { get; set; }
    public double? Boylam { get; set; }
    public string? CalismaSaatleri { get; set; }
    public int? KurulusYili { get; set; }

    // Sosyal Medya
    public string? Twitter { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? YoutubeKanal { get; set; }
    public string? Pinterest { get; set; }
    public string? LinkedIn { get; set; }
    public string? TiktokKanal { get; set; }

    // Tasarim (tema renkleri)
    public string? TasarimRengi1 { get; set; }
    public string? TasarimRengi2 { get; set; }
    public string? TasarimRengi3 { get; set; }
    public string? AdminTema { get; set; }
    public string? SiteTema { get; set; }

    // Menu ayarlari
    public int MenuYatayAralik { get; set; } = 30;
    public int MenuDikeyPadding { get; set; } = 20;
    public int LogoMaxYukseklik { get; set; } = 60;

    // Yetkili
    public string? YetkiliAdSoyad { get; set; }
    public string? VergiNo { get; set; }
    public string? VergiDairesi { get; set; }

    // Durum
    public bool AktifMi { get; set; } = true;
    public bool DemoMu { get; set; }
    public int? AktifSablonId { get; set; }

    // Modul (opsiyonel)
    [NotMapped]
    public List<string> AktifModulKodlari { get; set; } = new();

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
