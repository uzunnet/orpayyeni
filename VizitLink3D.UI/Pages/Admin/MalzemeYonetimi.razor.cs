using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class MalzemeYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private readonly List<MalzemeTipiSecenegi> MalzemeTipleri =
    [
        new("malzeme.tip.cam", "Cam"),
        new("malzeme.tip.aluminyum", "Alüminyum"),
        new("malzeme.tip.metal", "Metal"),
        new("malzeme.tip.ayna", "Ayna"),
        new("malzeme.tip.porselen", "Porselen"),
        new("malzeme.tip.ahsap", "Ahşap"),
        new("malzeme.tip.mdf", "MDF"),
        new("malzeme.tip.lake", "Lake"),
        new("malzeme.tip.membran", "Membran"),
        new("malzeme.tip.akrilik", "Akrilik"),
        new("malzeme.tip.kompakt", "Kompakt"),
        new("malzeme.tip.diger", "Diğer")
    ];

    private List<Malzeme> _liste = [];
    private List<Malzeme> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private Malzeme _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Malzeme>>("api/malzemeler") ?? [];
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
                (x.Tip?.ToLowerInvariant().Contains(arama) ?? false) ||
                TipEtiketi(x.Tip).ToLowerInvariant().Contains(arama) ||
                (x.Aciklama?.ToLowerInvariant().Contains(arama) ?? false)).ToList();
    }

    private void YeniAc()
    {
        _form = new Malzeme { AktifMi = true, Tip = "Cam" };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(Malzeme m)
    {
        _form = new Malzeme
        {
            Id = m.Id,
            Ad = m.Ad,
            Aciklama = m.Aciklama,
            Tip = m.Tip,
            SiraNo = m.SiraNo,
            AktifMi = m.AktifMi
        };
        _duzenlenenId = m.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(Malzeme m)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.onay", "Onay"),
            string.Format(
                DilServisi.T("admin.malzeme.silOnay", "'{0}' malzemesi yayindan kaldirilacak. Emin misiniz?"),
                m.Ad ?? string.Empty),
            yesText: DilServisi.T("ortak.yayindanKaldir", "Yayindan kaldir"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));

        if (onay == true)
        {
            await Sil(m);
        }
    }

    private async Task Sil(Malzeme m)
    {
        var cevap = await Api.DeleteAsync($"api/malzemeler/{m.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.malzeme.silindi", "Malzeme yayindan kaldirildi."), Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.malzeme.silinemedi", "Malzeme yayindan kaldirilamadi."), Severity.Error);
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.Ad) || string.IsNullOrWhiteSpace(_form.Tip))
        {
            Snackbar.Add(DilServisi.T("admin.malzeme.zorunluAlan", "Ad ve tip alanlari zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<Malzeme>? cevap;
        if (_duzenlenenId.HasValue)
        {
            cevap = await Api.PutAsync<Malzeme>($"api/malzemeler/{_duzenlenenId.Value}", _form);
        }
        else
        {
            cevap = await Api.PostAsync<Malzeme>("api/malzemeler", _form);
        }

        _kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(
                _duzenlenenId.HasValue
                    ? DilServisi.T("admin.malzeme.guncellendi", "Malzeme guncellendi.")
                    : DilServisi.T("admin.malzeme.eklendi", "Yeni malzeme eklendi."),
                Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.malzeme.kaydedilemedi", "Malzeme kaydedilemedi."), Severity.Error);
    }

    private void FormIptal() => _formAcik = false;

    private string TipEtiketi(string? deger)
    {
        if (string.IsNullOrWhiteSpace(deger))
            return "-";

        var secenek = MalzemeTipleri.FirstOrDefault(x => x.Deger == deger);
        return secenek is not null ? DilServisi.T(secenek.DilAnahtari, secenek.Varsayilan) : deger;
    }

    private sealed record MalzemeTipiSecenegi(string DilAnahtari, string Varsayilan)
    {
        public string Deger => Varsayilan;
    }
}
