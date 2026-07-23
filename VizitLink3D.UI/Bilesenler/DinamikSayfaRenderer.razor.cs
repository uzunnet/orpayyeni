using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler;

public partial class DinamikSayfaRenderer : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = "anasayfa";
    [Parameter] public string Dil { get; set; } = "tr";

    private SayfaGorunumDto? _sayfa;
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _yukleniyor = true;
        StateHasChanged();
        try
        {
            _sayfa = await Api.GetAsync<SayfaGorunumDto>($"api/sayfa-gorunumu/{Slug}?dil={Dil}");
        }
        catch { _sayfa = null; }
        _yukleniyor = false;
    }

    private string GorselAdresi(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return string.Empty;

        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return yol;

        if (yol.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            return $"{Api.ApiBaseUrl.TrimEnd('/')}{yol}";

        return yol;
    }
}

