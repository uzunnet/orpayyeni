using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class KaplamaYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private List<KaplamaSecenegi> _liste = [];
    private List<KaplamaSecenegi> _filtreliListe = [];
    private List<Malzeme> _malzemeListesi = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private KaplamaSecenegi _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private int? _secilenMalzemeId;

    protected override async Task OnInitializedAsync()
    {
        _malzemeListesi = await Api.GetAsync<List<Malzeme>>("api/malzemeler") ?? [];
        await Yukle();
    }

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();

        _liste = _secilenMalzemeId.HasValue
            ? await Api.GetAsync<List<KaplamaSecenegi>>($"api/malzemeler/{_secilenMalzemeId.Value}/kaplamalar") ?? []
            : await Api.GetAsync<List<KaplamaSecenegi>>("api/kaplamalar") ?? [];

        AramaUygula();
        _yukleniyor = false;
    }

    private async Task MalzemeFiltreDegisti(int? malzemeId)
    {
        _secilenMalzemeId = malzemeId;
        await Yukle();
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
                (x.HexKod?.ToLowerInvariant().Contains(arama) ?? false) ||
                MalzemeAdi(x.MalzemeId).ToLowerInvariant().Contains(arama)).ToList();
    }

    private void YeniAc()
    {
        _form = new KaplamaSecenegi { AktifMi = true, MalzemeId = _secilenMalzemeId ?? _malzemeListesi.FirstOrDefault()?.Id ?? 0 };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(KaplamaSecenegi k)
    {
        _form = new KaplamaSecenegi
        {
            Id = k.Id,
            MalzemeId = k.MalzemeId,
            Ad = k.Ad,
            HexKod = k.HexKod,
            SiraNo = k.SiraNo,
            AktifMi = k.AktifMi
        };
        _duzenlenenId = k.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(KaplamaSecenegi k)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.onay", "Onay"),
            string.Format(
                DilServisi.T("admin.kaplama.silOnay", "'{0}' kaplamasi yayindan kaldirilacak. Emin misiniz?"),
                k.Ad ?? string.Empty),
            yesText: DilServisi.T("ortak.yayindanKaldir", "Yayindan kaldir"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));

        if (onay == true)
        {
            await Sil(k);
        }
    }

    private async Task Sil(KaplamaSecenegi k)
    {
        var cevap = await Api.DeleteAsync($"api/kaplamalar/{k.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.kaplama.silindi", "Kaplama yayindan kaldirildi."), Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.kaplama.silinemedi", "Kaplama yayindan kaldirilamadi."), Severity.Error);
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.Ad) || !_form.MalzemeId.HasValue || _form.MalzemeId.Value <= 0)
        {
            Snackbar.Add(DilServisi.T("admin.kaplama.zorunluAlan", "Ad ve malzeme alanlari zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<KaplamaSecenegi>? cevap;
        if (_duzenlenenId.HasValue)
        {
            cevap = await Api.PutAsync<KaplamaSecenegi>($"api/kaplamalar/{_duzenlenenId.Value}", _form);
        }
        else
        {
            cevap = await Api.PostAsync<KaplamaSecenegi>("api/kaplamalar", _form);
        }

        _kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(
                _duzenlenenId.HasValue
                    ? DilServisi.T("admin.kaplama.guncellendi", "Kaplama guncellendi.")
                    : DilServisi.T("admin.kaplama.eklendi", "Yeni kaplama eklendi."),
                Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.kaplama.kaydedilemedi", "Kaplama kaydedilemedi."), Severity.Error);
    }

    private void FormIptal() => _formAcik = false;

    private string MalzemeAdi(int? malzemeId)
    {
        if (!malzemeId.HasValue)
        {
            return "-";
        }

        return _malzemeListesi.FirstOrDefault(m => m.Id == malzemeId.Value)?.Ad ?? malzemeId.Value.ToString();
    }

    private static string RenkStili(string? hexKod)
    {
        var renk = string.IsNullOrWhiteSpace(hexKod) ? "transparent" : hexKod;
        return $"background-color:{renk}";
    }
}
