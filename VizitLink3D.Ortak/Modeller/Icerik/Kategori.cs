namespace VizitLink3D.Ortak.Modeller;

public class Kategori
{
    public int Id { get; set; }
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    public int? FirmaId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public Firma? Firma { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ResimUrl { get; set; } = string.Empty;
    public int Sira { get; set; }
    public bool AktifMi { get; set; } = true;
    public string Aciklama { get; set; } = string.Empty;
    public int? UstKategoriId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public Kategori? UstKategori { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Kategori> AltKategoriler { get; set; } = new();
}
