using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class KoleksiyonlarBentoGrid : ComponentBase
{
    [Parameter] public string Baslik { get; set; } = "";
    [Parameter] public string Aciklama { get; set; } = "";
    [Parameter] public string KesfetMetin { get; set; } = "Keşfet";
    [Parameter] public List<BentoKartVerisi> Kartlar { get; set; } = [];

    public class BentoKartVerisi
    {
        public string Baslik { get; set; } = "";
        public string Etiket { get; set; } = "";
        public string Aciklama { get; set; } = "";
        public string GorselUrl { get; set; } = "";
        public string Href { get; set; } = "";
        public bool Buyuk { get; set; }
    }
}
