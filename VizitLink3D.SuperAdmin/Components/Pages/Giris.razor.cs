using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[AllowAnonymous]
public partial class Giris : ComponentBase
{
    [Inject] private IServisler.GirisServisi GirisServisi { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    private MudForm _form = null!;
    private bool _yukleniyor;
    private string _hata = string.Empty;

    private class GirisModel
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    private GirisModel _girisModel = new();

    private async Task GirisYap()
    {
        _hata = string.Empty;
        await _form.Validate();
        if (!_form.IsValid) return;

        _yukleniyor = true;
        try
        {
            var basarili = await GirisServisi.GirisYapAsync(_girisModel.KullaniciAdi, _girisModel.Sifre);
            if (basarili)
            {
                Nav.NavigateTo("/");
            }
            else
            {
                _hata = "Kullanıcı adı veya şifre hatalı.";
            }
        }
        catch (Exception ex)
        {
            _hata = $"Bağlantı hatası: {ex.Message}";
        }
        finally
        {
            _yukleniyor = false;
        }
    }
}
