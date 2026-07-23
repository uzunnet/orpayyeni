using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class Giris : ComponentBase
{
    [Inject] private KimlikServisi Kimlik { get; set; } = default!;

    private MudForm _form = default!;
    private string _kullaniciAdi = "admin";
    private string _sifre = "";
    private bool _yukleniyor;

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
                snackbar.Add(dil.T("admin.giris.basarili", "Giriş başarılı!"), Severity.Success);
                nav.NavigateTo("admin/dashboard");
            }
            else
            {
                snackbar.Add(dil.T("admin.giris.hataliBilgi", "Kullanıcı adı veya şifre hatalı."), Severity.Error);
            }
        }
        catch
        {
            snackbar.Add(dil.T("admin.giris.sunucuBaglanamadi", "Sunucuya bağlanılamadı."), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }
}
