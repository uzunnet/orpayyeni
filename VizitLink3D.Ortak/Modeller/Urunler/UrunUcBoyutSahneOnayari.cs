using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Model bazlı çoklu sahne preset'i.
/// Her model için birden fazla kamera/ışık/çevre ayarı tanımlanabilir.
/// Tenant güvenli: model üzerinden dolaylı erişim kontrolü yapılır.
/// </summary>
public class UrunUcBoyutSahneOnayari
{
    public int Id { get; set; }
    public int UrunUcBoyutModeliId { get; set; }

    /// <summary>Bağlı olduğu 3D model. Restrict FK — model silinmeden önayarlar silinmeli.</summary>
    [JsonIgnore]
    public UrunUcBoyutModeli? UrunUcBoyutModeli { get; set; }

    /// <summary>Görünen ad (UI'da gösterilir).</summary>
    public string Ad { get; set; } = string.Empty;

    /// <summary>ASCII slug/kod — model içinde unique.</summary>
    public string Kod { get; set; } = string.Empty;

    /// <summary>
    /// JSON: kamera, ışık, çevre, post-processing ayarları.
    /// Schema: { "kamera":{...}, "isik":{...}, "cevre":{...}, "postProcess":{...} }
    /// </summary>
    public string? AyarlarJson { get; set; }

    public bool VarsayilanMi { get; set; }

    /// <summary>Admin onayı olmadan public endpoint'te gösterilmez. Varsayılan false.</summary>
    public bool AdminOnayliMi { get; set; }

    public bool AktifMi { get; set; } = true;
    public int SiraNo { get; set; }

    // Audit + soft delete
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
