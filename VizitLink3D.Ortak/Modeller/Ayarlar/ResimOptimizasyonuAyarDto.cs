namespace VizitLink3D.Ortak.Modeller.Ayarlar;

public class ResimOptimizasyonuAyarDto
{
    public int MaksimumKenar { get; set; }   // [256, 4096]
    public int Kalite { get; set; }          // [50, 100]
    public bool WebpZorunlu { get; set; }
}
