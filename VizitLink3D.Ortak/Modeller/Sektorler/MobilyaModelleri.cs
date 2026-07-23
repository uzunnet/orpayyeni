using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Mobilya kategorilerini temsil eder (Mutfak Dolaplari, Banyo Dolaplari, TV Unitesi, vb.)
/// </summary>
public class MobilyaKategorisi
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? KapakResim { get; set; }
    public string? Ikon { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}

public class MobilyaKategorisiYerellestirme
{
    public int Id { get; set; }
    public int MobilyaKategorisiId { get; set; }
    [JsonIgnore] public MobilyaKategorisi? MobilyaKategorisi { get; set; }
    public string Dil { get; set; } = "tr";
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
}

public class MobilyaUrunu
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int MobilyaKategorisiId { get; set; }
    [JsonIgnore] public MobilyaKategorisi? MobilyaKategorisi { get; set; }
    public string? AnaGorselUrl { get; set; }
    public string? GaleriResimleriJson { get; set; }
    public int SiraNo { get; set; }
    public bool OneCikanMi { get; set; }
    public bool AktifMi { get; set; } = true;
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}

public class MobilyaUrunuYerellestirme
{
    public int Id { get; set; }
    public int MobilyaUrunuId { get; set; }
    [JsonIgnore] public MobilyaUrunu? MobilyaUrunu { get; set; }
    public string Dil { get; set; } = "tr";
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
}
