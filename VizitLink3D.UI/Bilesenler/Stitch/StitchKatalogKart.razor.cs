using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Stitch;

public partial class StitchKatalogKart
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string? Aciklama { get; set; }
    [Parameter] public string GorselUrl { get; set; } = "";
    [Parameter] public string Yil { get; set; } = "";
    [Parameter] public string InceleUrl { get; set; } = "";
    [Parameter] public string? IndirUrl { get; set; }
    [Parameter] public string InceleMetin { get; set; } = "İncele";
}
