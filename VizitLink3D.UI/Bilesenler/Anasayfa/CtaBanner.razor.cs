using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class CtaBanner : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Aciklama { get; set; } = "";
    [Parameter] public string ButonMetin { get; set; } = "Şimdi Teklif Al";
    [Parameter] public string ButonUrl { get; set; } = "/iletisim";
}
