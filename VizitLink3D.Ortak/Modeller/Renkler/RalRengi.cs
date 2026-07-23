using System;

namespace VizitLink3D.Ortak.Modeller.Renkler;

public class RalRengi
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? HexKod { get; set; }
    public string? Grup { get; set; }
    public int KatalogId { get; set; }
    public int SiraNo { get; set; }
    public string YuzeyTipi { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}