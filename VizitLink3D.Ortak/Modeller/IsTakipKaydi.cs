namespace VizitLink3D.Ortak.Modeller;

public class IsTakipKaydi
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string Durum { get; set; } = "Bekliyor"; // Bekliyor, Yapiliyor, Tamamlandi, Iptal
    public string Oncelik { get; set; } = "Orta"; // Dusuk, Orta, Yuksek, Kritik
    public string Kategori { get; set; } = "Diger"; // Backend, Frontend, Tasarim, Altyapi, Diger
    public int SiraNo { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? TamamlanmaTarihi { get; set; }
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

public enum IsDurumu { Bekliyor, Yapiliyor, Tamamlandi, Iptal }
public enum IsOnceligi { Dusuk, Orta, Yuksek, Kritik }
public enum IsKategorisi { Backend, Frontend, Tasarim, Altyapi, Diger }

public class IsTakipIstatistik
{
    public int Toplam { get; set; }
    public int Bekleyen { get; set; }
    public int Yapiliyor { get; set; }
    public int Tamamlanan { get; set; }
    public int Kritik { get; set; }
}
