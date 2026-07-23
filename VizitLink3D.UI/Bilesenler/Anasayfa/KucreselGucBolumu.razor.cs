using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class KucreselGucBolumu : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Aciklama { get; set; } = "";
    [Parameter] public string? OneCikanAlinti { get; set; }
    [Parameter] public string? AlintiSahibi { get; set; }
    [Parameter] public List<GucOgesi> Ogeler { get; set; } = [];

    public class GucOgesi
    {
        public string Ikon { get; set; } = "public";
        public string Baslik { get; set; } = "";
        public string Aciklama { get; set; } = "";
    }
}
