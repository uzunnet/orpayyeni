using System;

namespace VizitLink3D.Ortak.Modeller.Renkler;

public class RenkKatalogu
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}