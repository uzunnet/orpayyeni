using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Admin;

public partial class AdminAracCubugu : ComponentBase
{
    [Parameter] public bool AramaGosterilsinMi { get; set; } = true;
    [Parameter] public string AramaYerMetni { get; set; } = "Ara...";
    [Parameter] public string AramaMetni { get; set; } = "";
    [Parameter] public EventCallback<string> AramaMetniDegisti { get; set; }
    [Parameter] public EventCallback AramaYapildi { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private async Task AramaMetniDegistir(string deger)
    {
        AramaMetni = deger;
        await AramaMetniDegisti.InvokeAsync(deger);
        await AramaYapildi.InvokeAsync();
    }
}
