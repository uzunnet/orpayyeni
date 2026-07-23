using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class AnaSayfa : IDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    protected override void OnInitialized()
    {
        DilServisi.DilDegisti += DilDegistiginde;
    }

    private void DilDegistiginde() => InvokeAsync(StateHasChanged);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await JS.InvokeVoidAsync("eval", @"
            document.querySelectorAll('.gb-reveal').forEach(el => el.classList.add('gorunur'));
        ");
    }

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegistiginde;
    }
}
