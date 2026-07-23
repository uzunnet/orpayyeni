using System.Text.Json.Serialization;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using VizitLink3D.Ortak.Modeller.Renkler;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Müşteri konfigürasyonuna ait tek bir parça seçimi.
/// Renk, malzeme, kaplama, doku, hareket değeri/açı bilgilerini taşır.
/// </summary>
public class MusteriKonfigurasyonParcasi
{
    public int Id { get; set; }

    /// <summary>Hangi konfigürasyona ait</summary>
    public int MusteriKonfigurasyonuId { get; set; }

    /// <summary>3D modeldeki hangi parça</summary>
    public int UrunUcBoyutParcasiId { get; set; }

    /// <summary>Seçilen RAL rengi (nullable)</summary>
    public int? SeciliRenkId { get; set; }

    /// <summary>Seçilen malzeme (nullable)</summary>
    public int? SeciliMalzemeId { get; set; }

    /// <summary>Seçilen kaplama (nullable)</summary>
    public int? SeciliKaplamaId { get; set; }

    /// <summary>Seçilen doku adı / kodu (nullable)</summary>
    public string? SeciliDoku { get; set; }

    /// <summary>Hareketli parçalar için pozisyon değeri (kaydırma mesafesi vb.)</summary>
    public double? HareketDegeri { get; set; }

    /// <summary>Hareketli parçalar için açı değeri (derece)</summary>
    public double? Aci { get; set; }

    /// <summary>Genel değer (geriye dönük uyumluluk için)</summary>
    public double? Deger { get; set; }

    /// <summary>Parça görünür mü?</summary>
    public bool GorunurMu { get; set; } = true;

    // === AUDIT ===
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // === SOFT DELETE ===
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    // === NAVIGATION ===
    [JsonIgnore]
    public MusteriKonfigurasyonu? MusteriKonfigurasyonu { get; set; }

    [JsonIgnore]
    public UrunUcBoyutParcasi? UrunUcBoyutParcasi { get; set; }

    [JsonIgnore]
    public RalRengi? SeciliRenk { get; set; }

    [JsonIgnore]
    public Malzeme? SeciliMalzeme { get; set; }

    [JsonIgnore]
    public KaplamaSecenegi? SeciliKaplama { get; set; }
}
