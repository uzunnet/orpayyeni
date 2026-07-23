using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

public partial class ReferansSeridi : ComponentBase
{
    private List<Referans> _referanslar = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var l = await api.GetAsync<List<Referans>>("api/referanslar");
            if (l != null)
            {
                _referanslar = l
                    .Where(r => r.AktifMi && !r.SilindiMi && !string.IsNullOrWhiteSpace(r.Logo) && r.Logo != "/api/medya/dosya/253")
                    .OrderBy(r => r.SiraNo)
                    .ToList();
            }
        }
        catch { }
    }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return "";
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return yol;
        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }
}

