using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class IcerikYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private record BolumOzet(string Bolum, int AnahtarSayisi, string Dil);
    private List<BolumOzet> _bolumler = new();
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _yukleniyor = true;
        var icerikler = await Api.GetAsync<List<IcerikDto>>("api/sayfa-icerigi");
        if (icerikler != null)
        {
            _bolumler = icerikler
                .GroupBy(i => new { i.Bolum, i.Dil })
                .Select(g => new BolumOzet(g.Key.Bolum, g.Count(), g.Key.Dil))
                .OrderBy(b => b.Bolum)
                .ToList();
        }
        _yukleniyor = false;
    }

    private void Duzenle(string bolum)
    {
        NavManager.NavigateTo($"/admin/sayfa-duzenle/{bolum}");
    }

    private async Task SilAsync(string bolum)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync("Silme Onayı",
            $"'{bolum}' bölümü ve tüm içeriği silinecek. Emin misiniz?",
            yesText: "Sil", cancelText: "İptal");

        if (onay == true)
        {
            var yanit = await Api.DeleteAsync($"api/sayfa-icerigi/{bolum}");
            if (yanit != null && yanit.BasariliMi)
            {
                Snackbar.Add("Bölüm silindi.", Severity.Success);
                await YukleAsync();
            }
            else
            {
                Snackbar.Add("Silme başarısız.", Severity.Error);
            }
        }
    }

    private class IcerikDto
    {
        public string Bolum { get; set; } = string.Empty;
        public string Anahtar { get; set; } = string.Empty;
        public string Deger { get; set; } = string.Empty;
        public string Dil { get; set; } = "tr";
    }
}

