using System;

namespace VizitLink3D.Ortak.Modeller.Malzemeler;

public class KaplamaSecenegi
{
    public int Id { get; set; }
    public int? MalzemeId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? HexKod { get; set; }
    public string? ResimUrl { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}