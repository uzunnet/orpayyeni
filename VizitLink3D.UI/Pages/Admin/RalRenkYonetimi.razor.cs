using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.RegularExpressions;

namespace VizitLink3D.UI.Pages.Admin;

public partial class RalRenkYonetimi : ComponentBase
{
    private const string VarsayilanYuzeyTipi = "Mat";
    private static readonly Regex HexKodDeseni = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private readonly (string Deger, string DilAnahtari, string Varsayilan)[] YuzeyTipleri =
    [
        ("Mat", "admin.ral.yuzey.mat", "Mat"),
        ("Parlak", "admin.ral.yuzey.parlak", "Parlak"),
        ("Saten", "admin.ral.yuzey.saten", "Saten"),
        ("Metal", "admin.ral.yuzey.metal", "Metal")
    ];

    private List<RalRengi> _liste = [];
    private List<RalRengi> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private RalRengi _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<RalRengi>>("api/renkler/ral") ?? [];
        AramaUygula();
        _yukleniyor = false;
    }

    private Task AramaDegisti(string arama)
    {
        _arama = arama;
        AramaUygula();
        return Task.CompletedTask;
    }

    private void AramaUygula()
    {
        var arama = _arama?.ToLowerInvariant() ?? string.Empty;
        _filtreliListe = string.IsNullOrWhiteSpace(arama)
            ? _liste
            : _liste.Where(x =>
                (x.Ad?.ToLowerInvariant().Contains(arama) ?? false) ||
                (x.Kod?.ToLowerInvariant().Contains(arama) ?? false) ||
                (x.Grup?.ToLowerInvariant().Contains(arama) ?? false) ||
                (x.YuzeyTipi?.ToLowerInvariant().Contains(arama) ?? false) ||
                (x.HexKod?.ToLowerInvariant().Contains(arama) ?? false)).ToList();
    }

    private void YeniAc()
    {
        _form = new RalRengi { AktifMi = true, YuzeyTipi = VarsayilanYuzeyTipi };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(RalRengi r)
    {
        _form = new RalRengi
        {
            Id = r.Id,
            Kod = r.Kod,
            Ad = r.Ad,
            HexKod = r.HexKod,
            Grup = r.Grup,
            YuzeyTipi = r.YuzeyTipi,
            KatalogId = r.KatalogId,
            SiraNo = r.SiraNo,
            AktifMi = r.AktifMi
        };
        _duzenlenenId = r.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(RalRengi r)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.onay", "Onay"),
            string.Format(
                DilServisi.T("admin.ral.silOnay", "'{0} - {1}' rengi yayindan kaldirilacak. Emin misiniz?"),
                r.Kod ?? string.Empty,
                r.Ad ?? string.Empty),
            yesText: DilServisi.T("ortak.yayindanKaldir", "Yayindan kaldir"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));

        if (onay == true)
        {
            await Sil(r);
        }
    }

    private async Task Sil(RalRengi r)
    {
        var cevap = await Api.DeleteAsync($"api/renkler/ral/{r.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.ral.silindi", "Renk yayindan kaldirildi."), Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.ral.silinemedi", "Renk yayindan kaldirilamadi."), Severity.Error);
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.Kod) || string.IsNullOrWhiteSpace(_form.Ad) || string.IsNullOrWhiteSpace(_form.HexKod))
        {
            Snackbar.Add(DilServisi.T("admin.ral.zorunluAlan", "Kod, ad ve hex kod alanlari zorunludur."), Severity.Warning);
            return;
        }

        _form.Kod = _form.Kod.Trim().ToUpperInvariant();
        _form.Ad = _form.Ad.Trim();
        _form.HexKod = _form.HexKod.Trim().ToUpperInvariant();
        _form.Grup = string.IsNullOrWhiteSpace(_form.Grup) ? null : _form.Grup.Trim();
        _form.YuzeyTipi = string.IsNullOrWhiteSpace(_form.YuzeyTipi) ? VarsayilanYuzeyTipi : _form.YuzeyTipi.Trim();

        if (!HexKodDeseni.IsMatch(_form.HexKod))
        {
            Snackbar.Add(DilServisi.T("admin.ral.hexGecersiz", "Hex kod #RRGGBB formatinda olmalidir."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<RalRengi>? cevap;
        if (_duzenlenenId.HasValue)
        {
            cevap = await Api.PutAsync<RalRengi>($"api/renkler/ral/{_duzenlenenId.Value}", _form);
        }
        else
        {
            cevap = await Api.PostAsync<RalRengi>("api/renkler/ral", _form);
        }

        _kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(
                _duzenlenenId.HasValue
                    ? DilServisi.T("admin.ral.guncellendi", "Renk guncellendi.")
                    : DilServisi.T("admin.ral.eklendi", "Yeni renk eklendi."),
                Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.ral.kaydedilemedi", "Renk kaydedilemedi."), Severity.Error);
    }

    private void FormIptal() => _formAcik = false;

    private static string RenkStili(string? hexKod)
    {
        var renk = string.IsNullOrWhiteSpace(hexKod) ? "transparent" : hexKod;
        return $"background-color:{renk}";
    }
}
