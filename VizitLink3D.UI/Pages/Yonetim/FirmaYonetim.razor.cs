using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Yonetim;

public partial class FirmaYonetim : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<Firma>? _firmalar;
    private Firma? _seciliFirma;
    private bool _duzenleDialoguAcik;

    protected override async Task OnInitializedAsync()
    {
        await FirmalariYukleAsync();
    }

    private async Task FirmalariYukleAsync()
    {
        _firmalar = await Api.GetAsync<List<Firma>>("api/firma");
    }

    private async Task DuzenleAcAsync(Firma firma)
    {
        _seciliFirma = new Firma
        {
            Id = firma.Id,
            Ad = firma.Ad,
            Unvan = firma.Unvan,
            Logo = firma.Logo,
            SiteTema = firma.SiteTema,
            AdminTema = firma.AdminTema,
            TasarimRengi1 = firma.TasarimRengi1,
            TasarimRengi2 = firma.TasarimRengi2,
            TasarimRengi3 = firma.TasarimRengi3
        };
        _duzenleDialoguAcik = true;
        await Task.CompletedTask;
    }

    private void DuzenleKapat()
    {
        _duzenleDialoguAcik = false;
        _seciliFirma = null;
    }

    private async Task KaydetAsync()
    {
        if (_seciliFirma == null) return;

        var dto = new
        {
            _seciliFirma.Ad,
            _seciliFirma.Unvan,
            _seciliFirma.Logo,
            _seciliFirma.SiteTema,
            _seciliFirma.AdminTema,
            _seciliFirma.TasarimRengi1,
            _seciliFirma.TasarimRengi2,
            _seciliFirma.TasarimRengi3
        };

        await Api.PutAsync<Firma>($"api/firma/{_seciliFirma.Id}", dto);
        Snackbar.Add($"{_seciliFirma.Ad} başarıyla güncellendi.", Severity.Success);
        DuzenleKapat();
        await FirmalariYukleAsync();
    }
}
