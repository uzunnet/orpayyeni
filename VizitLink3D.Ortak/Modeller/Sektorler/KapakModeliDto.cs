using System.ComponentModel.DataAnnotations;

namespace VizitLink3D.Ortak.Modeller.Sektorler;

public class KapakModeliDto
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string ModelKodu { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string ModelAdi { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Kategori { get; set; } = "Ozel";

    [MaxLength(100)]
    public string RenkAdi { get; set; } = string.Empty;

    [MaxLength(10)]
    public string RenkHex { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DokuTipi { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AnaGorselUrl { get; set; } = string.Empty;

    public List<string> UygulamaGorselleri { get; set; } = new();

    [MaxLength(1000)]
    public string Aciklama { get; set; } = string.Empty;

    public int SiraNo { get; set; } = 100;
    public bool AktifMi { get; set; } = true;
    public bool YeniMi { get; set; } = false;
    public bool OneCikanMi { get; set; } = false;
    public DateTime OlusturulmaTarihi { get; set; }
}
