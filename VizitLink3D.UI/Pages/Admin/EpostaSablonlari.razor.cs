using Microsoft.AspNetCore.Components;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class EpostaSablonlari : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<SablonDto> _sablonlar = new();
    private bool _yukleniyor = true;
    private bool _dialogAcik;
    private bool _duzenlemeModu;
    private int _duzenlenenId;

    private string _formAd = string.Empty;
    private string _formKonu = string.Empty;
    private string _formIcerik = string.Empty;
    private string _formTip = "Genel";
    private bool _formAktif = true;

    protected override async Task OnInitializedAsync()
    {
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        _yukleniyor = true;
        var liste = await Api.GetAsync<List<SablonDto>>("api/eposta-sablonlari");
        _sablonlar = liste ?? new();
        _yukleniyor = false;
    }

    private void YeniEkle()
    {
        _duzenlemeModu = false;
        _duzenlenenId = 0;
        _formAd = string.Empty;
        _formKonu = string.Empty;
        _formIcerik = string.Empty;
        _formTip = "Genel";
        _formAktif = true;
        _dialogAcik = true;
    }

    private void Duzenle(SablonDto sablon)
    {
        _duzenlemeModu = true;
        _duzenlenenId = sablon.Id;
        _formAd = sablon.Ad;
        _formKonu = sablon.Konu;
        _formIcerik = sablon.IcerikHtml;
        _formTip = sablon.Tip ?? "Genel";
        _formAktif = sablon.AktifMi;
        _dialogAcik = true;
    }

    private void DialogKapat() => _dialogAcik = false;

    private async Task KaydetAsync()
    {
        var sablon = new { Ad = _formAd, Konu = _formKonu, IcerikHtml = _formIcerik, Tip = _formTip, AktifMi = _formAktif };

        if (_duzenlemeModu)
        {
            var yanit = await Api.PutAsync<object>($"api/eposta-sablonlari/{_duzenlenenId}", sablon);
            if (yanit != null && yanit.BasariliMi)
                Snackbar.Add("Şablon güncellendi.", Severity.Success);
            else
                Snackbar.Add("Güncelleme başarısız.", Severity.Error);
        }
        else
        {
            var yanit = await Api.PostAsync<object>("api/eposta-sablonlari", sablon);
            if (yanit != null && yanit.BasariliMi)
                Snackbar.Add("Şablon eklendi.", Severity.Success);
            else
                Snackbar.Add("Ekleme başarısız.", Severity.Error);
        }

        _dialogAcik = false;
        await YukleAsync();
    }

    private async Task SilAsync(SablonDto sablon)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync("Silme Onayı",
            $"'{sablon.Ad}' silinecek. Emin misiniz?", yesText: "Sil", cancelText: "İptal");
        if (onay == true)
        {
            var yanit = await Api.DeleteAsync($"api/eposta-sablonlari/{sablon.Id}");
            if (yanit != null && yanit.BasariliMi)
            {
                Snackbar.Add("Silindi.", Severity.Success);
                await YukleAsync();
            }
            else Snackbar.Add("Silme başarısız.", Severity.Error);
        }
    }

    private class SablonDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Konu { get; set; } = string.Empty;
        public string IcerikHtml { get; set; } = string.Empty;
        public string? Tip { get; set; }
        public bool AktifMi { get; set; }
    }
}
