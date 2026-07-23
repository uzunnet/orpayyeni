using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Müşterinin 3D konfigüratörde oluşturduğu özelleştirilmiş ürün yapılandırması.
/// Tenant izolasyonu: FirmaId üzerinden.
/// </summary>
public class MusteriKonfigurasyonu
{
    public int Id { get; set; }

    /// <summary>Hangi ürün üzerinde yapılandırma</summary>
    public int UrunId { get; set; }

    /// <summary>SaaS tenant izolasyonu — hangi firmaya ait</summary>
    public int? FirmaId { get; set; }

    /// <summary>Yapılandırmayı oluşturan kullanıcı (nullable — misafir kullanıcı olabilir)</summary>
    public int? KullaniciId { get; set; }

    /// <summary>Oturum bazlı anonim erişim için anahtar</summary>
    public string? OturumAnahtari { get; set; }

    /// <summary>Müşteri notu / özel istek</summary>
    public string? Not { get; set; }

    /// <summary>Toplam fiyat (opsiyonel, parça toplamından hesaplanabilir)</summary>
    public decimal? ToplamFiyat { get; set; }

    /// <summary>Konfigürasyon durumu: Taslak, Tamamlandi, TeklifeDonustu</summary>
    public string Durum { get; set; } = "Taslak";

    // === AUDIT ===
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public int? GuncelleyenKullaniciId { get; set; }

    // === SOFT DELETE ===
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    // === NAVIGATION ===
    [JsonIgnore]
    public Urun? Urun { get; set; }

    [JsonIgnore]
    public Firma? Firma { get; set; }

    [JsonIgnore]
    public ICollection<MusteriKonfigurasyonParcasi> Parcalar { get; set; } = new List<MusteriKonfigurasyonParcasi>();
}
