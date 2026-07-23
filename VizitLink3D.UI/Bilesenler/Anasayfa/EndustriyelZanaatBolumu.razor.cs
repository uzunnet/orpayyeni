using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class EndustriyelZanaatBolumu : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Aciklama { get; set; } = "";
    [Parameter] public string GorselUrl { get; set; } = "";
    [Parameter] public List<OzellikOgesi> Ozellikler { get; set; } = [];

    public class OzellikOgesi
    {
        public string Etiket { get; set; } = "";
        public string Deger { get; set; } = "";
    }
}
