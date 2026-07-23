using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class IsTakip : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<IsTakipKaydi> _liste = new();
    private IsTakipIstatistik _istatistik = new();
    private string _aktifDurum = "Tumu";
    private bool _dialogAcik;
    private bool _silDialogAcik;
    private bool _duzenlemeModu;
    private bool _yukleniyor;
    private IsTakipKaydi _form = new();
    private int _duzenlenenId;
    private IsTakipKaydi? _silinecek;

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _yukleniyor = true;
        StateHasChanged();

        _liste = await Api.GetAsync<List<IsTakipKaydi>>("api/is-takip?durum=" + (_aktifDurum == "Kritik" ? "Tumu" : _aktifDurum)) ?? new();
        _istatistik = await Api.GetAsync<IsTakipIstatistik>("api/is-takip/istatistik") ?? new();
        if (_aktifDurum == "Kritik") _liste = _liste.Where(i => i.Oncelik == "Kritik").ToList();

        _yukleniyor = false;
        StateHasChanged();
    }

    private void YeniAc()
    {
        _duzenlemeModu = false;
        _duzenlenenId = 0;
        _form = new IsTakipKaydi { Durum = "Bekliyor", Oncelik = "Orta", Kategori = "Diger" };
        _dialogAcik = true;
    }

    private void DuzenleAc(IsTakipKaydi kayit)
    {
        _duzenlemeModu = true;
        _duzenlenenId = kayit.Id;
        _form = new IsTakipKaydi
        {
            Baslik = kayit.Baslik,
            Aciklama = kayit.Aciklama,
            Durum = kayit.Durum,
            Oncelik = kayit.Oncelik,
            Kategori = kayit.Kategori
        };
        _dialogAcik = true;
    }

    private async Task DurumDegistir(IsTakipKaydi kayit, string yeniDurum)
    {
        kayit.Durum = yeniDurum;
        await Api.PutAsync<IsTakipKaydi>($"api/is-takip/{kayit.Id}", kayit);
        Snackbar.Add("Durum güncellendi.", Severity.Success);
    }

    private async Task KaydetAsync()
    {
        if (_duzenlemeModu)
        {
            await Api.PutAsync<IsTakipKaydi>($"api/is-takip/{_duzenlenenId}", _form);
            Snackbar.Add("İş güncellendi.", Severity.Success);
        }
        else
        {
            await Api.PostAsync<IsTakipKaydi>("api/is-takip", _form);
            Snackbar.Add("Yeni iş eklendi.", Severity.Success);
        }
        _dialogAcik = false;
        await YukleAsync();
    }

    private void DialogKapat() => _dialogAcik = false;

    private void SilOnay(IsTakipKaydi kayit)
    {
        _silinecek = kayit;
        _silDialogAcik = true;
    }

    private async Task SilAsync()
    {
        if (_silinecek != null)
        {
            await Api.DeleteAsync($"api/is-takip/{_silinecek.Id}");
            Snackbar.Add("İş silindi.", Severity.Success);
        }
        _silDialogAcik = false;
        _silinecek = null;
        await YukleAsync();
    }
}
