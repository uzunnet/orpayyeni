using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class KatalogBanner : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Aciklama { get; set; } = "";
    [Parameter] public string ButonMetin { get; set; } = "İncele";
    [Parameter] public string ButonUrl { get; set; } = "";
    [Parameter] public string? IndirmeUrl { get; set; }
    [Parameter] public string GorselUrl { get; set; } = "";
}
