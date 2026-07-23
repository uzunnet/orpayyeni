using System;
using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// 3D model ile parça eşleştirmesi.
/// Hangi modelde hangi mesh'in hangi mantıksal parçaya karşılık geldiğini tutar.
/// </summary>
public class UrunParcaEslemesi
{
    public int Id { get; set; }
    public int UrunUcBoyutModeliId { get; set; }
    public int UrunUcBoyutParcasiId { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }

    // Soft delete
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }

    [JsonIgnore]
    public UrunUcBoyutModeli? UrunUcBoyutModeli { get; set; }

    [JsonIgnore]
    public UrunUcBoyutParcasi? UrunUcBoyutParcasi { get; set; }
}
