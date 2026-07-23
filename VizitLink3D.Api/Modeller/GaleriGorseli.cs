namespace VizitLink3D.Api.Modeller;

public class GaleriGorseli
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Baslik { get; set; }
    public string? AltMetin { get; set; }
    public int Sira { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
