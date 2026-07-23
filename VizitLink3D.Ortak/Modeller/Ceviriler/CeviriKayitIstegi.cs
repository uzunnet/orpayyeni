using System.ComponentModel.DataAnnotations;

namespace VizitLink3D.Ortak.Modeller.Ceviriler;

public class CeviriKayitIstegi
{
    [Required]
    public string Anahtar { get; set; } = string.Empty;

    [Required]
    public string Dil { get; set; } = string.Empty;

    [Required]
    public string Deger { get; set; } = string.Empty;
}
