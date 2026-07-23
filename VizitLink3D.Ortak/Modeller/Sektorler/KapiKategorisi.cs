using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Kapi ve kapak kategorilerini temsil eder.
/// Ornek: Membran, Lake, High Gloss, Laminant, Melamin, Kaplama, Klasik
/// </summary>
public class KapiKategorisi
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? KapakResim { get; set; }
    public string? Ikon { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;

    // SEO
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public string? SeoAnahtarKelimeler { get; set; }

    // Audit
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}

/// <summary>
/// Kapi kategorisi icin coklu dil destegi.
/// Her dil icin ayri ad, aciklama ve SEO bilgisi tutulur.
/// </summary>
public class KapiKategorisiYerellestirme
{
    public int Id { get; set; }
    public int KapiKategorisiId { get; set; }

    [JsonIgnore]
    public KapiKategorisi? KapiKategorisi { get; set; }

    public string Dil { get; set; } = "tr";
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
}
