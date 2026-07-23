using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Bilesenler.Tema;

public partial class TemaSecici : ComponentBase
{
    private bool _acik;
    private string _seciliTema = "gold";
    private string _aktifMod = "koyu";
    private List<TemaOzetDto>? _temalar;
    private bool _jsYuklendi;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _seciliTema = await JS.InvokeAsync<string>("localStorage.getItem", "vizitlink3d_site_tema") ?? "gold";
        }
        catch { }

        try
        {
            _aktifMod = await JS.InvokeAsync<string>("localStorage.getItem", "temaMod") ?? "koyu";
        }
        catch { }

        try
        {
            _temalar = await Api.GetAsync<List<TemaOzetDto>>("api/tema/kapsam?kapsam=site");
        }
        catch { }

        try
        {
            var firmaTema = await Api.GetAsync<FirmaTemaDto>("api/firma-tema");
            if (!string.IsNullOrWhiteSpace(firmaTema?.SiteTema))
                _seciliTema = firmaTema.SiteTema!;
        }
        catch { }
    }

    protected override async Task OnAfterRenderAsync(bool ilkMi)
    {
        if (ilkMi)
        {
            _jsYuklendi = true;
        }
    }

    private void Toggle()
    {
        _acik = !_acik;
    }

    private async Task TemaSec(TemaOzetDto tema)
    {
        _seciliTema = tema.Slug;
        _acik = false;

        await JS.InvokeVoidAsync("localStorage.setItem", "vizitlink3d_site_tema", tema.Slug);

        if (_jsYuklendi)
        {
            await JS.InvokeVoidAsync("vizitlink3dTema.siteUygula", tema.Slug);
        }

        try
        {
            await Api.PutAsync<object>("api/firma-tema", new { AdminTema = (string?)null, SiteTema = tema.Slug });
        }
        catch { }

        StateHasChanged();
    }

    private async Task ModDegistir(string mod)
    {
        if (_aktifMod == mod) return;

        _aktifMod = mod;
        await JS.InvokeVoidAsync("vizitlink3dTema.modUygula", mod);

        try
        {
            await Api.PostAsync<object>("api/tema/mod", new { tema = _seciliTema, mod });
        }
        catch { }

        StateHasChanged();
    }

    private string ModDugmeClass(string mod) =>
        _aktifMod == mod ? "tema-secici-mod-btn aktif" : "tema-secici-mod-btn";

    private static string TemaRengiGetir(string slug) => slug switch
    {
        "gold" => "#FFD700",
        "aurelian-onyx" => "#d4af37",
        "aurelian-daylight" => "#C8952A",
        "midnight-noir" => "#C0C0C0",
        "marble-rose" => "#B76E79",
        "copper-bronze" => "#B87333",
        "sage-stone" => "#8B9D77",
        "ocean-azure" => "#2196F3",
        "ember-red" => "#D32F2F",
        "royal-purple" => "#9C27B0",
        "ivory-champagne" => "#C9A96E",
        "noir-graphite" => "#B87333",
        _ => "#888"
    };

    public sealed record TemaOzetDto(string Slug, string Ad, string Aciklama, bool GlassmorphismAktif, bool Premium, string Etiketler, string? ThumbnailUrl);

    private sealed class FirmaTemaDto
    {
        public string? SiteTema { get; set; }
    }
}
