using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class IstatistikBolumu : ComponentBase
{
    [Parameter] public List<IstatistikOgesi> Istatistikler { get; set; } = [];

    public class IstatistikOgesi
    {
        public string Deger { get; set; } = "";
        public string Etiket { get; set; } = "";
    }
}
