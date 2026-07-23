using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunPdfKaynagi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public long MedyaId { get; set; }
    public int? SayfaSayisi { get; set; }
    public string? CozumlemeDurumu { get; set; }
    public string? HataMesaji { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public DateTime? CozumlemeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
