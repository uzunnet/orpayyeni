namespace VizitLink3D.Ortak.Modeller;

public class Sektor
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public string Ikon { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
    public int Sira { get; set; }
}
