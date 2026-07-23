using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Ürün listesi (Ziyaretçi) — minimal bilgi
/// </summary>
public class UrunOzetDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? KisaAciklama { get; set; }
    public string? AnaGorselUrl { get; set; }
    public bool OneCikanMi { get; set; }
    public bool YeniMi { get; set; }
    public decimal? Fiyat { get; set; }
    public int UrunAilesiId { get; set; }
    public string? UrunAilesiAdi { get; set; }
    public int? UrunKategoriId { get; set; }
    public string? UrunKategoriAdi { get; set; }
}
