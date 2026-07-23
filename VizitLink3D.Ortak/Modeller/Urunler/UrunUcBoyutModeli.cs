using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunUcBoyutModeli
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string ModelAdi { get; set; } = string.Empty;
    public string ModelDosyaYolu { get; set; } = string.Empty;
    public string? AnalizJson { get; set; }
    public string? ModelTipi { get; set; }
    public string? ModelYolu { get; set; }
    public long MedyaId { get; set; }
    public long? OnizlemeMedyaId { get; set; }
    public long DosyaBoyutuByte { get; set; }
    public string? ModelAnalizJson { get; set; }
    public string? KameraAyarJson { get; set; }
    public string? IsikAyarJson { get; set; }
    public string? CevreAyarJson { get; set; }
    public bool VarsayilanMi { get; set; }
    public int Versiyon { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}