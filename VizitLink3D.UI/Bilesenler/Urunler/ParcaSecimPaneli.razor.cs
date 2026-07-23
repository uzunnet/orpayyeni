using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Urunler;

public partial class ParcaSecimPaneli : ComponentBase
{
    [Parameter, EditorRequired] public List<UrunUcBoyutParcasi> Parcalar { get; set; } = [];
    [Parameter] public int? SeciliParcaId { get; set; }
    [Parameter] public EventCallback<UrunUcBoyutParcasi> ParcaSecildi { get; set; }

    private string ParcaSinifi(UrunUcBoyutParcasi parca)
    {
        var temel = "gb-parca-kalem";
        if (parca.Id == SeciliParcaId)
            temel += " gb-parca-secili";
        return temel;
    }

    private async Task ParcaTiklandi(UrunUcBoyutParcasi parca)
    {
        if (ParcaSecildi.HasDelegate)
            await ParcaSecildi.InvokeAsync(parca);
    }
}
