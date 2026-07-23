namespace VizitLink3D.Ortak.Modeller.Ayarlar;

public class MedyaTopluIslemSonucDto
{
    public int Islenen { get; set; }
    public int Atlanan { get; set; }
    public int Hata { get; set; }
    public long EskiToplamBoyut { get; set; }
    public long YeniToplamBoyut { get; set; }
    public List<string> Hatalar { get; set; } = [];
}
