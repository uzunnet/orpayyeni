using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

public class UrunKonfigurasyonSablonu
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string DetaySablonu { get; set; } = string.Empty;
    public bool HeroAktifMi { get; set; }
    public bool TeknikOzellikAktifMi { get; set; }
    public bool PdfKaynakAktifMi { get; set; }
    public bool BenzerUrunlerAktifMi { get; set; }
    public bool TeklifFormuAktifMi { get; set; }
    public bool UcBoyutIlkAcilacakMi { get; set; }
    public string? AnimasyonTipi { get; set; }
    public string? MobilPanelDavranisi { get; set; }
    public string? RenkPaneliKonumu { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
}
