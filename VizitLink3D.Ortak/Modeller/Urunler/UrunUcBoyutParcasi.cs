using System;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// 3D modelin bir mesh parçası — admin tarafından yapılandırılır.
/// AdminOnayliMi = false iken kullanıcı ara yüzde görünmez.
/// ParcaTipi: Govde | Cam | Metal | Ahsap | Porselen | Ayna | Musluk | Kulp | Zemin | Diger
/// MalzemeTipiKisiti: JSON array e.g. ["Cam","Metal"] — sadece bu tiplerdeki malzemeler gösterilir
/// </summary>
public class UrunUcBoyutParcasi
{
    public int Id { get; set; }
    public int UrunUcBoyutModeliId { get; set; }
    public int? ParcaGrubuId { get; set; }
    public string GorunenAd { get; set; } = string.Empty;
    public string MeshAdi { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
    public bool SecilebilirMi { get; set; } = true;
    public bool RenklenebilirMi { get; set; }
    public bool MalzemeDegisebilirMi { get; set; }
    public bool HareketliMi { get; set; }
    public bool GizlenebilirMi { get; set; }
    public string HareketTipi { get; set; } = string.Empty;
    public double? MinDeger { get; set; }
    public double? MaxDeger { get; set; }
    public double? VarsayilanDeger { get; set; }
    public int? VarsayilanRenkId { get; set; }
    public int? VarsayilanMalzemeId { get; set; }
    public int SiraNo { get; set; }

    /// <summary>ASCII slug/kod — model içinde unique. Admin tarafından atanır.</summary>
    public string? MantiksalKod { get; set; }

    /// <summary>Bu parçaya doku/tekstür uygulanabilir mi?</summary>
    public bool DokuUygulanabilirMi { get; set; }

    /// <summary>
    /// Hareket ayarları JSON: yön, eksen, pivot noktası, kaydırma limiti vb.
    /// Admin tarafından yapılandırılır. Schema: { "eksen":"x", "pivot":[0,0,0], "minAci":0, "maxAci":90, "kaydirmaSinir":0.5 }
    /// </summary>
    public string? HareketAyarlariJson { get; set; }

    // Admin onay sistemi
    /// <summary>Admin onaylamadan kullanıcı arayüzde görünmez.</summary>
    public bool AdminOnayliMi { get; set; } = false;

    /// <summary>Parçanın tipi: Govde, Cam, Metal, Ahsap, Porselen, Ayna, Musluk, Kulp, Zemin, Diger</summary>
    public string? ParcaTipi { get; set; }

    /// <summary>JSON array — sadece bu malzeme tipleri gösterilir. Null = tümü görünür.</summary>
    public string? MalzemeTipiKisiti { get; set; }

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
