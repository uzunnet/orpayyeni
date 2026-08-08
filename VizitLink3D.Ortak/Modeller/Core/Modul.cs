namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// SuperAdmin tarafindan yonetilen platform modullerini temsil eder.
/// Her modul bir islev grubunu (Blog, Galeri, Urun Yonetimi, vb.) kapsar.
/// </summary>
public class Modul
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? Kategori { get; set; }
    public bool VarsayilanMi { get; set; }
    public bool SistemModuluMu { get; set; }
}
