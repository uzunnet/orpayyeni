using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class PdfSayfaGorseli
{
    public int Id { get; set; }
    public int PdfKaynagiId { get; set; }
    public int SayfaNo { get; set; }
    public long MedyaId { get; set; }
    public string? Aciklama { get; set; }
    public int? UrunId { get; set; }
    public bool UruneBaglandiMi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
