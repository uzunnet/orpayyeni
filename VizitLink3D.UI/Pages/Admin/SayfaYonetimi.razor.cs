using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VizitLink3D.UI.Servisler;
using System.Linq;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SayfaYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    public class SayfaTanim
    {
        public string Baslik { get; set; } = string.Empty;
        public string Bolum { get; set; } = string.Empty;
        public int Goruntulenme { get; set; }
        public bool AktifMi { get; set; } = true;
        public DateTime GuncellemeTarihi { get; set; }
    }

    public class SayfaIcerigiDto
    {
        public string Bolum { get; set; } = string.Empty;
        public string Anahtar { get; set; } = string.Empty;
        public string Deger { get; set; } = string.Empty;
    }

    private List<SayfaTanim> _sayfalar = new();
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        await SayfalarYukleAsync();
    }

    private async Task SayfalarYukleAsync()
    {
        _yukleniyor = true;
        var icerikler = await Api.GetAsync<List<SayfaIcerigiDto>>("api/sayfa-icerigi");
        if (icerikler != null)
        {
            _sayfalar = icerikler.GroupBy(i => i.Bolum).Select(g => new SayfaTanim
            {
                Bolum = g.Key,
                Baslik = g.FirstOrDefault(x => x.Anahtar == "SayfaBasligi")?.Deger ?? g.Key,
                Goruntulenme = 0, // Analitik eklendiğinde gerçek veri gelecek
                AktifMi = true,
                GuncellemeTarihi = DateTime.UtcNow
            }).ToList();
        }
        _yukleniyor = false;
    }

    private void SayfaEkle()
    {
        NavManager.NavigateTo("admin/sayfa-duzenle");
    }

    private void SayfaDuzenle(SayfaTanim sayfa)
    {
        NavManager.NavigateTo($"/admin/sayfa-duzenle/{sayfa.Bolum}");
    }

    private async Task SayfaSilAsync(SayfaTanim sayfa)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync("Silme Onayı", $"'{sayfa.Baslik}' sayfasının tüm içeriği kalıcı olarak silinecektir. Emin misiniz?", yesText: "Sil", cancelText: "İptal");

        if (onay == true)
        {
            var yanit = await Api.DeleteAsync($"api/sayfa-icerigi/{sayfa.Bolum}");
            if (yanit != null && yanit.BasariliMi)
            {
                Snackbar.Add("Sayfa başarıyla silindi.", Severity.Success);
                await SayfalarYukleAsync();
            }
            else
            {
                Snackbar.Add("Sayfa silinirken hata oluştu.", Severity.Error);
            }
        }
    }
}

