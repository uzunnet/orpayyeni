using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class ProjelerBolumu
{
    [Parameter] public List<ProjeVerisi> Projeler { get; set; } = [];

    public class ProjeVerisi
    {
        public string GorselUrl { get; set; } = string.Empty;
        public string Sehir { get; set; } = string.Empty;
        public string ProjeAd { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
    }
}
