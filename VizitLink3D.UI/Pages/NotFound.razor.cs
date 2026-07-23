using Microsoft.AspNetCore.Components;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class NotFound : IDisposable
{
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    protected override void OnInitialized()
    {
        DilServisi.DilDegisti += DilDegistiginde;
    }

    private void DilDegistiginde() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        DilServisi.DilDegisti -= DilDegistiginde;
    }
}
