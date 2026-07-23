using System;
using System.Text.Json.Serialization;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class Urun
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? KisaAciklama { get; set; }
    public string? Aciklama { get; set; }
    public int UrunAilesiId { get; set; }
    public int? UrunKategoriId { get; set; }

    /// <summary>SaaS tenant sahiplik. Null eski veriler için; seed ile doldurulur.</summary>
    public int? FirmaId { get; set; }

    public bool AktifMi { get; set; } = true;
    public bool OneCikanMi { get; set; }
    public bool YeniMi { get; set; }
    public decimal? Fiyat { get; set; }
    public string? Birim { get; set; }
    public int SiraNo { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public long? AnaGorselMedyaId { get; set; }
    public int? VarsayilanUcBoyutModeliId { get; set; }
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    [JsonIgnore]
    public UrunAilesi? UrunAilesi { get; set; }

    [JsonIgnore]
    public Firma? Firma { get; set; }
}