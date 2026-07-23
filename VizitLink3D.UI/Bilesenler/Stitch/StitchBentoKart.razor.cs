using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Stitch;

public partial class StitchBentoKart
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Etiket { get; set; } = "";
    [Parameter] public string GorselUrl { get; set; } = "";
    [Parameter] public string Href { get; set; } = "";
    [Parameter] public string KesfetMetin { get; set; } = "Keşfet";
    [Parameter] public bool Buyuk { get; set; }

    private string KartClass => Buyuk
        ? "stitch-bento-kart stitch-bento-buyuk urun-kart anim-reveal-up"
        : "stitch-bento-kart stitch-bento-orta urun-kart anim-reveal-up";
}
