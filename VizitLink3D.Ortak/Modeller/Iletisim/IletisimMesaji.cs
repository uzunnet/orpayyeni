using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Iletisim formu mesajlarini temsil eder.
/// Okunma, cevaplanma durumu ve oncelik seviyesi takip edilir.
/// </summary>
public class IletisimMesaji
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }

    [JsonIgnore]
    public Firma? Firma { get; set; }

    public string AdSoyad { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Konu { get; set; }
    public string Mesaj { get; set; } = string.Empty;

    // Durum takibi
    public bool OkunduMu { get; set; }
    public DateTime? OkunmaTarihi { get; set; }
    public bool CevaplandiMi { get; set; }
    public DateTime? CevapTarihi { get; set; }
    public string? CevapMetni { get; set; }

    // Oncelik
    public string? OncelikSeviyesi { get; set; } = "Normal"; // Dusuk, Normal, Yuksek, Acil
    public string? EtiketlerJson { get; set; } // JSON array

    // Analitik
    [JsonIgnore]
    public string? IPAdresi { get; set; }
    public string? Tarayici { get; set; }
    public string? Cihaz { get; set; }

    // Audit
    public DateTime Tarih { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
