namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UygulamaAlanSonucu
{
    public int MedyaId { get; set; }
    public int PdfKaynagiId { get; set; }
    public int SayfaNo { get; set; }
    public string Konum { get; set; } = "";
    public string GorselUrl { get; set; } = "";
    public int? EslenenUrunId { get; set; }
    public string? EslenenUrunAdi { get; set; }
    public int EslenenSiraNo { get; set; }
}

public class UrunEslemeIstegi
{
    public int MedyaId { get; set; }
    public int UrunId { get; set; }
    public int SiraNo { get; set; } = 1;
}
