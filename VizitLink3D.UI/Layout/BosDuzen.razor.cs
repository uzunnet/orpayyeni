using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Layout;

public partial class BosDuzen : LayoutComponentBase
{
    [Inject] private DilServisi DilServisi { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private string _aktifTemaModu = "koyu";
    private bool KoyuTemaMi => _aktifTemaModu != "acik";

    protected override async Task OnInitializedAsync()
    {
        await DilServisi.BaslatAsync("tr");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await JS.InvokeVoidAsync("vizitlink3dDil.htmlDiliniAyarla", DilServisi.AktifDil);

        var kayitliTemaModu = await JS.InvokeAsync<string?>("localStorage.getItem", "temaMod");
        _aktifTemaModu = string.IsNullOrWhiteSpace(kayitliTemaModu)
            ? "koyu"
            : (kayitliTemaModu.Equals("acik", StringComparison.OrdinalIgnoreCase) ? "acik" : "koyu");

        await JS.InvokeVoidAsync("vizitlink3dTema.adminModUygula", _aktifTemaModu);
        StateHasChanged();
    }
}
