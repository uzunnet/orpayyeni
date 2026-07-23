using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Models;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SayfaDuzenAyarlari : ComponentBase
{
    private bool _yukleniyor = true;
    private bool _kaydediliyor;
    private List<SayfaDuzenAyariDto> _ayarlar = [];

    protected override async Task OnInitializedAsync()
    {
        await Yukle();
    }

    private async Task Yukle()
    {
        _yukleniyor = true;
        try
        {
            var sonuc = await api.GetAsync<List<SayfaDuzenAyariDto>>("api/sayfa-duzen-ayarlari");
            _ayarlar = sonuc ?? [];
        }
        catch (Exception ex)
        {
            snackbar.Add($"Yükleme hatası: {ex.Message}", Severity.Error);
            _ayarlar = [];
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task Kaydet()
    {
        _kaydediliyor = true;
        try
        {
            var sonuc = await api.PutAsync<List<SayfaDuzenAyariDto>>("api/sayfa-duzen-ayarlari", _ayarlar);
            snackbar.Add("Sayfa düzen ayarları kaydedildi.", Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kaydetme hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }
}

