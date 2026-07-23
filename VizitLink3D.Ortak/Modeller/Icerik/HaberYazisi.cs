using System;

namespace VizitLink3D.Ortak.Modeller;

public class HaberYazisi
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public Firma? Firma { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Ozet { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;
    public string AnaResimUrl { get; set; } = string.Empty;
    public List<HaberResim> Resimler { get; set; } = new();
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public string? Etiketler { get; set; }
    public bool AktifMi { get; set; } = true;
    public int OkunmaSayisi { get; set; } = 0;
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? YayinTarihi { get; set; }
}

public class HaberResim
{
    public int Id { get; set; }
    public int HaberYazisiId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public HaberYazisi? HaberYazisi { get; set; }
    public string ResimUrl { get; set; } = string.Empty;
    public int Sira { get; set; }
}
