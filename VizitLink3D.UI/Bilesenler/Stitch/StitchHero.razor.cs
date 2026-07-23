using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Stitch;

public partial class StitchHero
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string? UstBaslik { get; set; }
    [Parameter] public string? Aciklama { get; set; }
    [Parameter] public string? GorselUrl { get; set; }
    [Parameter] public string? CtaUrl { get; set; }
    [Parameter] public string? CtaMetin { get; set; }
    [Parameter] public string? Cta2Url { get; set; }
    [Parameter] public string? Cta2Metin { get; set; }
}
