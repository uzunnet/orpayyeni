using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Modeller;

public class SayfaIcerigi
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }
    public Firma? Firma { get; set; }
    public string Anahtar { get; set; } = string.Empty;
    public string Bolum { get; set; } = string.Empty;
    public string Deger { get; set; } = string.Empty;
    public string Dil { get; set; } = "tr";
    public DateTime GuncellemeTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
