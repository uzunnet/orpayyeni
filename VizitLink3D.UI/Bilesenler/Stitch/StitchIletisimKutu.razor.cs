using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Stitch;

public partial class StitchIletisimKutu
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string IkonAdi { get; set; } = "info";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public double Gecikme { get; set; }
    private bool Gecikmeli => Gecikme > 0;
}
