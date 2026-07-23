using System.ComponentModel.DataAnnotations;

namespace VizitLink3D.Ortak.Modeller;

public class GirisIstegi
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre zorunludur.")]
    public string Sifre { get; set; } = string.Empty;

    public string? IkiAsamaliKod { get; set; }
}
