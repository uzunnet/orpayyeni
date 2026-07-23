using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Proje kategorisi: Mutfak, Banyo, Yatak Odasi, Ofis, Tac, Aksesuar
/// </summary>
public class ProjeKategorisi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tamamlanmis projeleri temsil eder. Musteriye ait mutfak/banyo/ofis projeleri.
/// </summary>
public class Proje
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Baslik { get; set; } = string.Empty;
    public string? KisaAciklama { get; set; }
    public string? Aciklama { get; set; }
    public int KategoriId { get; set; }
    [JsonIgnore] public ProjeKategorisi? Kategori { get; set; }
    public string? MusteriAdi { get; set; }
    public string? MusteriSehir { get; set; }
    public DateTime? ProjeTarihi { get; set; }
    public string? KapakResim { get; set; }
    public string? MusteriLogo { get; set; }
    public bool OneCikanMi { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    [JsonIgnore] public List<ProjeResim>? Resimler { get; set; }
}

public class ProjeResim
{
    public int Id { get; set; }
    public int ProjeId { get; set; }
    [JsonIgnore] public Proje? Proje { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltMetin { get; set; }
    public int Sira { get; set; }
}

/// <summary>
/// Ana sayfa hero slider icin slayt modeli.
/// </summary>
public class Slayt
{
    public int Id { get; set; }
    public string SayfaKodu { get; set; } = "anasayfa"; // anasayfa, ekibimiz, vizyon-misyon, vb.
    public string Dil { get; set; } = "tr";
    public string? Baslik { get; set; }
    public string? AltBaslik { get; set; }
    public string? Aciklama { get; set; }
    public string? ArkaplanResim { get; set; }
    public string? ArkaplanResimMobil { get; set; }
    public string? ButonMetni1 { get; set; }
    public string? ButonLink1 { get; set; }
    public string? ButonMetni2 { get; set; }
    public string? ButonLink2 { get; set; }
    public string? AnimasyonTipi { get; set; } = "fade"; // fade, slide, zoom
    public int GecisHizi { get; set; } = 800;
    public int GosterimSuresi { get; set; } = 5000;
    public string? MetinHizalama { get; set; } = "sol"; // sol, orta, sag
    public string? MetinRengi { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// Referanslar
/// </summary>
public class Referans
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string Tip { get; set; } = "Musteri";
    public string? WebSite { get; set; }
    public string? Aciklama { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// Musteri yorumlari
/// </summary>
public class MusteriYorumu
{
    public int Id { get; set; }
    public string MusteriAdi { get; set; } = string.Empty;
    public string? MusteriUnvan { get; set; }
    public string? MusteriSehir { get; set; }
    public string? Avatar { get; set; }
    public string Yorum { get; set; } = string.Empty;
    public int Puan { get; set; } = 5;
    public int? ProjeId { get; set; }
    public bool Onaylandi { get; set; }
    public bool OneCikan { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime YorumTarihi { get; set; } = DateTime.UtcNow;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}

/// <summary>
/// Hizmet adimlari
/// </summary>
public class HizmetAdimi
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? Ikon { get; set; }
    public int AdimNo { get; set; }
    public int SiraNo { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
