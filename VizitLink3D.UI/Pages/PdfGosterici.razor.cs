using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Yardimcilar;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class PdfGosterici : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private NavigationManager Navigasyon { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "dosya")]
    public string? Dosya { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "baslik")]
    public string? Baslik { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "donus")]
    public string? Donus { get; set; }

    private string _belgeUrl = string.Empty;
    private string _baslik = string.Empty;
    private bool _gorselMi;

    protected override void OnParametersSet()
    {
        _belgeUrl = BelgeUrl(Dosya);
        _baslik = string.IsNullOrWhiteSpace(Baslik)
            ? DilServisi.T("pdfGosterici.baslik", "PDF Belgesi")
            : Baslik;
        _gorselMi = GorselMi(Dosya);
    }

    private string BelgeUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(yol, UriKind.Absolute, out var mutlakUrl))
        {
            return mutlakUrl.ToString();
        }

        var genelYol = KatalogYolu.GuvenliGenelKatalogYolu(yol);
        return genelYol is not null
            ? $"{Navigasyon.BaseUri.TrimEnd('/')}/{genelYol}"
            : $"{Api.ApiBaseUrl}/api/belge-dosya?dosya={Uri.EscapeDataString(yol)}";
    }

    private void GeriDon()
    {
        Navigasyon.NavigateTo(string.IsNullOrWhiteSpace(Donus) ? "/katalog" : Donus);
    }

    private static bool GorselMi(string? yol)
    {
        var uzanti = Path.GetExtension(yol) ?? string.Empty;
        return uzanti.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || uzanti.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || uzanti.Equals(".png", StringComparison.OrdinalIgnoreCase);
    }
}
