using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Ürün detay sayfası (Ziyaretçi) — tam bilgi
/// </summary>
public class UrunDetayDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? KisaAciklama { get; set; }
    public string? Aciklama { get; set; }
    public bool OneCikanMi { get; set; }
    public bool YeniMi { get; set; }
    public decimal? Fiyat { get; set; }
    public string? Birim { get; set; }
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }

    // Kategori
    public int? UrunKategoriId { get; set; }
    public string? UrunKategoriAdi { get; set; }

    // Aile
    public int UrunAilesiId { get; set; }
    public string? UrunAilesiAdi { get; set; }

    // Ana görsel
    public string? AnaGorselUrl { get; set; }

    // Medya galeri
    public List<UrunMedyaDetayDto> Medyalar { get; set; } = new();

    // 3D model
    public int? VarsayilanUcBoyutModeliId { get; set; }
    public UrunUcBoyutDetayDto? UcBoyutModeli { get; set; }

    // Seçenek listeleri
    public List<RenkSecenekDetayDto> RenkSecenekleri { get; set; } = new();
    public List<MalzemeSecenekDetayDto> MalzemeSecenekleri { get; set; } = new();

    // Tarihler
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime? GuncellenmeTarihi { get; set; }
}

public class UrunMedyaDetayDto
{
    public int Id { get; set; }
    public long MedyaId { get; set; }
    public string? Url { get; set; }
    public string? AltMetni { get; set; }
    public int SiraNo { get; set; }
}

public class UrunUcBoyutDetayDto
{
    public int Id { get; set; }
    public long MedyaId { get; set; }
    public string? GlbUrl { get; set; }
    public string? Aciklama { get; set; }
    public List<UcBoyutParcaDetayDto> Parcalar { get; set; } = new();
}

public class UcBoyutParcaDetayDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? TekstureKodu { get; set; }
    public bool RenklendirilebilirMi { get; set; }
    public bool MalzemeSecenegiVarMi { get; set; }
}

public class RenkSecenekDetayDto
{
    public int Id { get; set; }
    public int RalRengiId { get; set; }
    public string? RalKodu { get; set; }
    public string? RalAdi { get; set; }
    public string? HexKodu { get; set; }
    public int? UcBoyutParcaId { get; set; }
}

public class MalzemeSecenekDetayDto
{
    public int Id { get; set; }
    public int MalzemeId { get; set; }
    public string? MalzemeAdi { get; set; }
    public string? Teksur { get; set; }
    public decimal? UretimMaliyet { get; set; }
    public int? UcBoyutParcaId { get; set; }
}
