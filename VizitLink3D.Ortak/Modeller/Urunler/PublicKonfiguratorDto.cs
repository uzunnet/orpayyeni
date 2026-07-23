using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Public konfigüratör sayfası için güvenli DTO.
/// Yalnız AdminOnayliMi=true, AktifMi=true, SilindiMi=false kayıtları döndürür.
/// Ham HareketAyarlariJson, mesh teknik adları, admin audit alanları dönmez.
/// </summary>
public class PublicKonfiguratorDto
{
    public int UrunId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal? Fiyat { get; set; }

    /// <summary>Tenant ID — embed token dogrulamasi icin</summary>
    public int FirmaId { get; set; }

    /// <summary>Model GLB/GLTF yolu (medya havuzu veya dosya yolu)</summary>
    public string? ModelYolu { get; set; }

    /// <summary>Model ID — SignalR ve JS tarafı için</summary>
    public int? ModelId { get; set; }

    /// <summary>Admin onaylı parçalar</summary>
    public List<PublicParcaDto> Parcalar { get; set; } = [];

    /// <summary>Admin onaylı sahne önayarları (kamera/ışık ayarları dönmez)</summary>
    public List<PublicSahneOnayariDto> SahneOnayarlari { get; set; } = [];
}

/// <summary>
/// Public gösterime uygun, teknik bilgilerden arındırılmış parça DTO'su.
/// MeshAdi, HareketAyarlariJson, MalzemeTipiKisiti (ham JSON), admin audit alanları dönmez.
/// Yalnız admin onaylı ve aktif parçalar listelenir.
/// </summary>
public class PublicParcaDto
{
    public int Id { get; set; }
    public int? ParcaGrubuId { get; set; }

    /// <summary>Kullanıcıya gösterilecek ad (örn. "Altın Kulp", "Ahşap Kapak Sol")</summary>
    public string GorunenAd { get; set; } = string.Empty;

    /// <summary>Parça tipi: Govde, Cam, Metal, Ahsap, Porselen, Ayna, Musluk, Kulp, Zemin, Diger</summary>
    public string? ParcaTipi { get; set; }

    /// <summary>Kullanıcı bu parçaya renk uygulayabilir mi?</summary>
    public bool RenklenebilirMi { get; set; }

    /// <summary>Kullanıcı bu parçaya malzeme değiştirebilir mi?</summary>
    public bool MalzemeDegisebilirMi { get; set; }

    /// <summary>Kullanıcı bu parçaya doku/tekstür uygulayabilir mi?</summary>
    public bool DokuUygulanabilirMi { get; set; }

    /// <summary>Kullanıcı bu parçayı gizleyebilir/gösterebilir mi?</summary>
    public bool GizlenebilirMi { get; set; }

    /// <summary>Parça seçilebilir mi?</summary>
    public bool SecilebilirMi { get; set; }

    /// <summary>Parça hareketli mi?</summary>
    public bool HareketliMi { get; set; }

    /// <summary>Hareket tipi (enum string): Sabit, Menteseli, Surgulu, Cekmece, YukariAcilir, Pivot, Recliner</summary>
    public string HareketTipi { get; set; } = string.Empty;

    /// <summary>Varsayılan renk ID (varsa)</summary>
    public int? VarsayilanRenkId { get; set; }

    /// <summary>Varsayılan malzeme ID (varsa)</summary>
    public int? VarsayilanMalzemeId { get; set; }

    /// <summary>Min hareket değeri (varsa)</summary>
    public double? MinDeger { get; set; }

    /// <summary>Max hareket değeri (varsa)</summary>
    public double? MaxDeger { get; set; }

    /// <summary>Varsayılan hareket değeri (varsa)</summary>
    public double? VarsayilanDeger { get; set; }

    /// <summary>Sıra numarası</summary>
    public int SiraNo { get; set; }

    /// <summary>Parçaya ait renk seçenekleri</summary>
    public List<PublicParcaRenkDto> Renkler { get; set; } = [];

    /// <summary>Parçaya ait malzeme seçenekleri</summary>
    public List<PublicParcaMalzemeDto> Malzemeler { get; set; } = [];

    /// <summary>Parçaya ait doku seçenekleri</summary>
    public List<PublicParcaDokuDto> Dokular { get; set; } = [];
}

/// <summary>
/// Parça için public renk seçeneği.
/// </summary>
public class PublicParcaRenkDto
{
    public int RenkId { get; set; }
    public int RalRengiId { get; set; }
    public string RalKodu { get; set; } = string.Empty;
    public string RalAdi { get; set; } = string.Empty;
    public string HexKodu { get; set; } = string.Empty;
    public string? EginliResimUrl { get; set; }
}

/// <summary>
/// Parça için public malzeme seçeneği.
/// </summary>
public class PublicParcaMalzemeDto
{
    public int MalzemeId { get; set; }
    public string MalzemeAdi { get; set; } = string.Empty;
    public string? Teksur { get; set; }
    public string? TeksurResimUrl { get; set; }
    public decimal? FiyatFarki { get; set; }
}

/// <summary>
/// Parça için public doku/kaplama seçeneği.
/// </summary>
public class PublicParcaDokuDto
{
    public int KaplamaId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? TeksurResimUrl { get; set; }
    public decimal? FiyatFarki { get; set; }
}

/// <summary>
/// Public sahne önayarı — yalnız ad ve kod döner, AyarlarJson içermez.
/// </summary>
public class PublicSahneOnayariDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public bool VarsayilanMi { get; set; }
    public int SiraNo { get; set; }
}

/// <summary>
/// Müşterinin public konfigüratörden gönderdiği seçim DTO'su.
/// Tenant FirmaId'si ve OturumAnahtari backend tarafından belirlenir.
/// </summary>
public class PublicSecimKaydetDto
{
    public int UrunId { get; set; }
    public string? MusteriNotu { get; set; }
    public List<PublicParcaSecimiDto> Secimler { get; set; } = [];
}

/// <summary>
/// Tek bir parça için müşteri seçimi.
/// </summary>
public class PublicParcaSecimiDto
{
    public int ParcaId { get; set; }
    public int? SeciliRenkId { get; set; }
    public int? SeciliMalzemeId { get; set; }
    public int? SeciliKaplamaId { get; set; }
    public string? SeciliDoku { get; set; }
    public double? HareketDegeri { get; set; }
    public double? Aci { get; set; }
    public bool GorunurMu { get; set; } = true;
}
