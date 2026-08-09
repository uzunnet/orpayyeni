using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class Giris : ComponentBase
{
    [Inject] private KimlikServisi Kimlik { get; set; } = default!;
    [Inject] private FirmaBilgisiServisi FirmaBilgisi { get; set; } = default!;

    private MudForm _form = default!;
    private string _kullaniciAdi = string.Empty;
    private string _sifre = string.Empty;
    private string _firmaAdi = "";
    private bool _yukleniyor;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var firma = await FirmaBilgisi.GetFirmaAsync();
            if (firma != null)
                _firmaAdi = firma.Ad ?? "";
        }
        catch { }
    }

    private async Task GirisYap()
    {
        await _form.ValidateAsync();
        if (!_form.IsValid) return;

        _yukleniyor = true;
        try
        {
            var basarili = await Kimlik.GirisYapAsync(_kullaniciAdi, _sifre);
            if (basarili)
            {
                snackbar.Add(dil.T("admin.giris.basarili", "Giris basarili!"), Severity.Success);
                nav.NavigateTo("admin/dashboard");
            }
            else
            {
                snackbar.Add(dil.T("admin.giris.hataliBilgi", "Kullanici adi veya sifre hatali."), Severity.Error);
            }
        }
        catch
        {
            snackbar.Add(dil.T("admin.giris.sunucuBaglanamadi", "Sunucuya baglanilamadi."), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }
}