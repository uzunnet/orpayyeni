namespace VizitLink3D.Ortak.Modeller.Tema;

public class FirmaTemaAtama
{
    public int Id { get; set; }
    public int FirmaId { get; set; }
    public int TemaSablonuId { get; set; }
    public string Tur { get; set; } = "site";
    public bool AktifMi { get; set; } = true;
    public string? OzelDegiskenlerJson { get; set; }
    public DateTime AtamaTarihi { get; set; } = DateTime.UtcNow;
}
