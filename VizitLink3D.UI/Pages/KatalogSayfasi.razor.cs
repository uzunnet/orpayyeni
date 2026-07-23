using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Yardimcilar;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages;

public partial class KatalogSayfasi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private NavigationManager Navigasyon { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private List<Katalog> _kataloglar = [];
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        _kataloglar = (await Api.GetAsync<List<Katalog>>("api/kataloglar") ?? [])
            .Where(katalog => katalog.AktifMi)
            .OrderBy(katalog => katalog.SiraNo)
            .ToList();
        _yukleniyor = false;
    }

    private string OnizlemeUrl(Katalog katalog)
    {
        if (!string.IsNullOrWhiteSpace(katalog.KapakResim))
        {
            return TamMedyaUrl(katalog.KapakResim);
        }

        if (KatalogYolu.GuvenliGenelKatalogYolu(katalog.PdfDosyaYolu) is not null)
        {
            return TamMedyaUrl("");
        }

        return $"{Api.ApiBaseUrl}/api/belge-onizleme?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}";
    }

    private static string PdfGostericiUrl(Katalog katalog)
    {
        return $"/pdf-gosterici?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}&baslik={Uri.EscapeDataString(katalog.Baslik)}&donus={Uri.EscapeDataString("/katalog")}";
    }

    private string IndirUrl(Katalog katalog)
    {
        var genelYol = KatalogYolu.GuvenliGenelKatalogYolu(katalog.PdfDosyaYolu);
        return genelYol is not null
            ? TamMedyaUrl(genelYol)
            : $"{Api.ApiBaseUrl}/api/belge-dosya?dosya={Uri.EscapeDataString(katalog.PdfDosyaYolu)}";
    }

    private string TamMedyaUrl(string yol)
    {
        if (Uri.TryCreate(yol, UriKind.Absolute, out var mutlakUrl))
        {
            return mutlakUrl.ToString();
        }

        return $"{Navigasyon.BaseUri.TrimEnd('/')}/{yol.TrimStart('/')}";
    }
}
