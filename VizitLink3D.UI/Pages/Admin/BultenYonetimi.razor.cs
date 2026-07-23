using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class BultenYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    private List<BultenAbonesi> _liste = [];
    private List<BultenAbonesi> _filtreliListe = [];
    private bool _yukleniyor = true;
    private string _arama = string.Empty;

    private int AktifSayisi => _liste.Count(x => x.AktifMi);
    private int DogrulananSayisi => _liste.Count(x => x.DogrulandiMi);

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true; StateHasChanged();
        _liste = await Api.GetAsync<List<BultenAbonesi>>("api/bulten") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    void AramaYap(KeyboardEventArgs e) => AramaUygula();

    void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a) ? _liste :
            _liste.Where(x => (x.Eposta?.ToLower().Contains(a) ?? false) || (x.AdSoyad?.ToLower().Contains(a) ?? false)).ToList();
    }
}

