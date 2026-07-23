using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class AkilliYasamBolumu : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Aciklama { get; set; } = "";
    [Parameter] public string GorselUrl { get; set; } = "";
    [Parameter] public string UrunKodu { get; set; } = "SMART-X1";
    [Parameter] public List<OzellikOgesi> Ozellikler { get; set; } = [];

    public class OzellikOgesi
    {
        public string Ikon { get; set; } = "check_circle";
        public string Baslik { get; set; } = "";
        public string Aciklama { get; set; } = "";
    }
}
