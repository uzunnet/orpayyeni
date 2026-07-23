using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VizitLink3D.Api.Modeller;

/// <summary>
/// KapiModeliResim, her kapi/kapak modeline ait galeri gorsellerini tutar.
/// Bir modelin birden cok gorseli olabilir.
/// </summary>
public class KapiModeliResim
{
    public int Id { get; set; }
    public int KapakModeliId { get; set; }

    [JsonIgnore]
    public KapakModeli? KapakModeli { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? AltMetin { get; set; }
    public int Sira { get; set; }
}

/// <summary>
/// KapiModeliYerellestirme, her kapi/kapak modeli icin coklu dil destegi saglar.
/// Her dil icin ayri ad, aciklama ve SEO bilgisi tutulur.
/// </summary>
public class KapiModeliYerellestirme
{
    public int Id { get; set; }
    public int KapakModeliId { get; set; }

    [JsonIgnore]
    public KapakModeli? KapakModeli { get; set; }

    public string Dil { get; set; } = "tr";
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? OnYazi { get; set; }
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
}
