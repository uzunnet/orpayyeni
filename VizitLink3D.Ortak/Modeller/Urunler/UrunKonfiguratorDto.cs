using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Ürün 3D konfigüratörü (Ziyaretçi) — renk + malzeme + seçenekler
/// </summary>
public class UrunKonfiguratorDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal? Fiyat { get; set; }

    // 3D model
    public int? VarsayilanUcBoyutModeliId { get; set; }
    public string? GlbUrl { get; set; }
    public List<UcBoyutParcaKonfigDto> Parcalar { get; set; } = new();

    // Seçenekler
    public List<RenkSecenekKonfigDto> RenkSecenekleri { get; set; } = new();
    public List<MalzemeSecenekKonfigDto> MalzemeSecenekleri { get; set; } = new();
    public List<OlcuSecenekKonfigDto> OlcuSecenekleri { get; set; } = new();

    // Hotspot'lar
    public List<HotspotKonfigDto> Hotspotlar { get; set; } = new();
}

public class UcBoyutParcaKonfigDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? TekstureKodu { get; set; }
    public bool RenklendirilebilirMi { get; set; }
    public bool MalzemeSecenegiVarMi { get; set; }

    // Parçaya ait renkler
    public List<RenkSecenekKonfigDto> ParcaRenkleri { get; set; } = new();

    // Parçaya ait malzemeler
    public List<MalzemeSecenekKonfigDto> ParcaMalzemeleri { get; set; } = new();
}

public class RenkSecenekKonfigDto
{
    public int Id { get; set; }
    public int RalRengiId { get; set; }
    public string RalKodu { get; set; } = string.Empty;
    public string RalAdi { get; set; } = string.Empty;
    public string HexKodu { get; set; } = string.Empty;
    public string? EginliResimUrl { get; set; }
    public int? UcBoyutParcaId { get; set; }
}

public class MalzemeSecenekKonfigDto
{
    public int Id { get; set; }
    public int MalzemeId { get; set; }
    public string MalzemeAdi { get; set; } = string.Empty;
    public string? Teksur { get; set; }
    public string? TeksurResimUrl { get; set; }
    public decimal? FiyatFarki { get; set; }
    public int? UcBoyutParcaId { get; set; }
}

public class OlcuSecenekKonfigDto
{
    public int Id { get; set; }
    public string Boyut { get; set; } = string.Empty; // "600x800"
    public decimal GenislikMm { get; set; }
    public decimal YukseklikMm { get; set; }
    public decimal? DerinlikMm { get; set; }
    public decimal? FiyatFarki { get; set; }
}

public class HotspotKonfigDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }
    public string? Aciklama { get; set; }
    public string? IconTipi { get; set; }
}

/// <summary>
/// Müşteri tarafından gönderilen konfigürasyon JSON'ı
/// </summary>
public class KonfigurasyonIslegiDto
{
    public int UrunId { get; set; }
    public int? SeciliRenkId { get; set; }
    public int? SeciliMalzemeId { get; set; }
    public int? SeciliOlcuId { get; set; }
    public Dictionary<int, int> ParcaRenkleri { get; set; } = new(); // ParcaId -> RenkId
    public Dictionary<int, int> ParcaMalzemeleri { get; set; } = new(); // ParcaId -> MalzemeId
    public string? MusteriNotu { get; set; }
}

/// <summary>
/// PDF teklif için konfigürasyon JSON'ı
/// </summary>
public class TeklifKonfigurasyonDto
{
    public int UrunId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public int? SeciliRenkId { get; set; }
    public string? RenkAdi { get; set; }
    public int? SeciliMalzemeId { get; set; }
    public string? MalzemeAdi { get; set; }
    public int? SeciliOlcuId { get; set; }
    public string? OlcuBilgisi { get; set; }
    public decimal? TahminiToplam { get; set; }
    public List<TeklifSatiriDto> Satirlar { get; set; } = new();
}

public class TeklifSatiriDto
{
    public string Aciklama { get; set; } = string.Empty;
    public int Adet { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal Toplam { get; set; }
}
