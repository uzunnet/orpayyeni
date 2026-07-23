using VizitLink3D.Ortak.Modeller.Urunler;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Bilesenler.Urunler;

public partial class UrunListeKart : ComponentBase
{
    [Parameter, EditorRequired] public Urun Urun { get; set; } = null!;
    [Parameter] public EventCallback<Urun> OnClick { get; set; }
    /// <summary>
    /// Aktif site temasi — bos birakilirsa DOM'dan okunur
    /// </summary>
    [Parameter] public string? AktifTema { get; set; }
    [Parameter] public string? KoleksiyonAdi { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ApiIstemcisi Api { get; set; } = default!;

    private string _temaClass = "";

    private string _gorselUrl
    {
        get
        {
            if (Urun.AnaGorselMedyaId is long medyaId and > 0)
                return $"{Api.ApiBaseUrl.TrimEnd('/')}/api/medya/dosya/{medyaId}";

            return "/medya/vizitlink3d_default.png";
        }
    }

    private string? _koleksiyonGrubu => string.IsNullOrWhiteSpace(KoleksiyonAdi) ? null : KoleksiyonAdi;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var tema = AktifTema;
            if (string.IsNullOrEmpty(tema))
            {
                try
                {
                    tema = await JS.InvokeAsync<string>("eval", "document.documentElement.getAttribute('data-site-tema') || ''");
                }
                catch { tema = ""; }
            }

            _temaClass = tema switch
            {
                "aurelian-onyx" => "gb-urun-kart--glass",
                _ => "gb-urun-kart--solid"
            };

            StateHasChanged();
        }
    }

    private async Task Tiklandi()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(Urun);
    }

    private async Task HakkindaIste()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync(Urun);
    }
}
