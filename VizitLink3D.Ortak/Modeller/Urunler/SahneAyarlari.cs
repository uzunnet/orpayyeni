namespace VizitLink3D.Ortak.Modeller.Urunler;

public class KameraAyar
{
    public double BaslangicAciX { get; set; }
    public double BaslangicAciY { get; set; }
    public double BaslangicAciZ { get; set; }
    public double ZoomMin { get; set; } = 0.5;
    public double ZoomMax { get; set; } = 3.0;
    public bool OtomatikDonme { get; set; } = true;
    public double DonmeHizi { get; set; } = 0.5;
    public double KameraYuksekligi { get; set; } = 1.6;
    public double HedefYukseklik { get; set; } = 0.8;
}

public class IsikAyar
{
    public double Siddet { get; set; } = 1.0;
    public double AciX { get; set; } = 45;
    public double AciY { get; set; } = 45;
    public int Sayi { get; set; } = 2;
    public string Tip { get; set; } = "ortam";
    public double GolgeYumusakligi { get; set; } = 0.5;
    public double GolgeYogunlugu { get; set; } = 0.3;
    public double Ton { get; set; }
    public double Pozlama { get; set; } = 1.0;
    public double ParlamaEsigi { get; set; } = 0.8;
}

public class CevreAyar
{
    public string ZeminRengi { get; set; } = "#CCCCCC";
    public string ZeminTipi { get; set; } = "mat";
    public string ArkaPlanRengi { get; set; } = "#F5F5F0";
    public string ArkaPlanTipi { get; set; } = "duz";
    public string? HdrYolu { get; set; }
    public bool OtomatikArkaPlan { get; set; } = true;
    public double KontrastEsik { get; set; } = 0.5;
}
