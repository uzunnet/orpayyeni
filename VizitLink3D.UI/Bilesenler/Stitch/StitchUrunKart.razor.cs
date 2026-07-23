using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.UI.Bilesenler.Stitch;

public partial class StitchUrunKart
{
    [Parameter] public required UrunOzetDto Urun { get; set; }
    [Parameter] public EventCallback<UrunOzetDto> OnClick { get; set; }

    private async Task UrunTiklandi()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(Urun);
    }
}
