using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Stitch;

public partial class StitchOzellikSatir
{
    [Parameter] public string Etiket { get; set; } = "";
    [Parameter] public string Deger { get; set; } = "";
}
