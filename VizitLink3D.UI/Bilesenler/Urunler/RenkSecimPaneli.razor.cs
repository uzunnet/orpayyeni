using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Urunler;

public partial class RenkSecimPaneli : ComponentBase
{
    [Parameter, EditorRequired] public List<RalRengi> Renkler { get; set; } = [];
    [Parameter] public string SeciliRenkKodu { get; set; } = "#E8E4DF";
    [Parameter] public EventCallback<RalRengi> RenkSecildi { get; set; }

    internal RalRengi? _seciliRenk;

    protected override void OnParametersSet()
    {
        if (_seciliRenk == null && Renkler.Any())
        {
            _seciliRenk = Renkler.FirstOrDefault(r => r.HexKod == SeciliRenkKodu) ?? Renkler.FirstOrDefault();
        }
    }

    private string RenkSwatchSinifi(RalRengi renk)
    {
        var temel = "gb-renk-swatch";
        if (renk.HexKod == _seciliRenk?.HexKod)
            temel += " gb-renk-secili";
        return temel;
    }

    private async Task RenkTiklandi(RalRengi renk)
    {
        _seciliRenk = renk;
        if (RenkSecildi.HasDelegate)
            await RenkSecildi.InvokeAsync(renk);
    }

    public static bool RenkAcikMi(string hexKod)
    {
        if (string.IsNullOrEmpty(hexKod) || hexKod.Length < 7) return true;
        try
        {
            int r = Convert.ToInt32(hexKod.Substring(1, 2), 16);
            int g = Convert.ToInt32(hexKod.Substring(3, 2), 16);
            int b = Convert.ToInt32(hexKod.Substring(5, 2), 16);
            var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;
            return yiq >= 128;
        }
        catch { /* hex kodu parse edilemezse açık renk varsayılır */ return true; }
    }
}
