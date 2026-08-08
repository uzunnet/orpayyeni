using Microsoft.AspNetCore.Components;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Bilesenler.Anasayfa;

/// <summary>
/// Nasıl Çalışıyoruz? — 4 adımlı süreç bölümü (İletişim → Projelendirme → Üretim → Teslimat).
/// Veriler API'den dinamik olarak çekilir.
/// </summary>
public partial class HizmetSureciBolumu : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = null!;

    private List<HizmetAdimi> _adimlar = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var liste = await Api.GetAsync<List<HizmetAdimi>>("api/hizmet-adimlari");
            if (liste != null && liste.Count > 0)
                _adimlar = liste;
        }
        catch { /* API erişilemezse DilServisi fallback kullanılır */ }
    }
}

