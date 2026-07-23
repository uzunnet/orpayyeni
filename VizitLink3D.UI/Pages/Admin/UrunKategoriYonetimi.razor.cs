using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class UrunKategoriYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<UrunKategori> _liste = [];
    private List<UrunKategori> _filtreliListe = [];
    private List<UrunKategori> _kategoriListesi = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private UrunKategori _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<UrunKategori>>("api/urun-kategorileri") ?? [];
        _kategoriListesi = _liste;
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("UrunKategori", _liste.Select(UrunKategoriCeviriKaydiOlustur));
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
                (x.Slug?.ToLowerInvariant().Contains(arama) ?? false) ||
                (x.Aciklama?.ToLowerInvariant().Contains(arama) ?? false)).ToList();
    }

    private void YeniAc()
    {
        _form = new UrunKategori { AktifMi = true };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(UrunKategori uk)
    {
        _form = new UrunKategori
        {
            Id = uk.Id,
            Ad = uk.Ad,
            Slug = uk.Slug,
            Aciklama = uk.Aciklama,
            UstKategoriId = uk.UstKategoriId,
            SiraNo = uk.SiraNo,
            AktifMi = uk.AktifMi
        };
        _duzenlenenId = uk.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(UrunKategori uk)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.onay", "Onay"),
            string.Format(
                DilServisi.T("admin.kategori.silOnay", "'{0}' kategorisi yayindan kaldirilacak. Emin misiniz?"),
                uk.Ad ?? string.Empty),
            yesText: DilServisi.T("ortak.yayindanKaldir", "Yayindan kaldir"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));

        if (onay == true)
        {
            await Sil(uk);
        }
    }

    private async Task Sil(UrunKategori uk)
    {
        var cevap = await Api.DeleteAsync($"api/urun-kategorileri/{uk.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.kategori.silindi", "Kategori yayindan kaldirildi."), Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.kategori.silinemedi", "Kategori yayindan kaldirilamadi."), Severity.Error);
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.Ad) || string.IsNullOrWhiteSpace(_form.Slug))
        {
            Snackbar.Add(DilServisi.T("admin.kategori.zorunluAlan", "Ad ve slug alanlari zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<UrunKategori>? cevap;
        if (_duzenlenenId.HasValue)
        {
            cevap = await Api.PutAsync<UrunKategori>($"api/urun-kategorileri/{_duzenlenenId.Value}", _form);
        }
        else
        {
            cevap = await Api.PostAsync<UrunKategori>("api/urun-kategorileri", _form);
        }

        _kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(
                _duzenlenenId.HasValue
                    ? DilServisi.T("admin.kategori.guncellendi", "Kategori guncellendi.")
                    : DilServisi.T("admin.kategori.eklendi", "Yeni kategori eklendi."),
                Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.kategori.kaydedilemedi", "Kategori kaydedilemedi."), Severity.Error);
    }

    private void FormIptal() => _formAcik = false;

    private async Task AICeviriDialogAc(UrunKategori uk)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", uk.Id },
            { "TabloOnEki", "UrunKategori" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Ad", uk.Ad ?? string.Empty },
                    { "Aciklama", uk.Aciklama ?? string.Empty }
                }
            }
        };

        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Admin.AICeviriDialog>(
            DilServisi.T("admin.ai.ceviri", "Yapay Zeka Cevirisi"),
            parameters);
        await dialog.Result;
        await Yukle();
    }

    private async Task TumuCevir() => await TumunuCevirDahili("UrunKategori", _liste.Select(UrunKategoriCeviriKaydiOlustur));

    private async Task KaydiTumDillerdeCevir(UrunKategori uk) => await KaydiCevirDahili("UrunKategori", UrunKategoriCeviriKaydiOlustur(uk));

    private async Task TumunuCevirDahili(string modulAdi, IEnumerable<AdminCeviriKaydi> kayitlar)
    {
        if (_tumCeviriCalisiyor || _liste.Count == 0) return;

        _tumCeviriCalisiyor = true;
        _ceviriIslenen = 0;
        _ceviriToplam = _liste.Count;
        StateHasChanged();

        try
        {
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync(modulAdi, kayitlar, async (islenen, toplam) =>
            {
                _ceviriIslenen = islenen;
                _ceviriToplam = toplam;
                StateHasChanged();
                await Task.CompletedTask;
            });

            await Yukle();
            Snackbar.Add(
                string.Format(DilServisi.T("admin.ceviri.topluTamamlandi", "Toplu ceviri tamamlandi. {0} kayitta toplam {1} alan cevrildi."), sonuc.CevrilenKayitSayisi, sonuc.CevrilenAlanSayisi),
                Severity.Success);
        }
        finally
        {
            _tumCeviriCalisiyor = false;
            _ceviriIslenen = 0;
            _ceviriToplam = 0;
            StateHasChanged();
        }
    }

    private async Task KaydiCevirDahili(string modulAdi, AdminCeviriKaydi kayit)
    {
        if (_tumCeviriCalisiyor) return;

        _tumCeviriCalisiyor = true;
        StateHasChanged();

        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync(modulAdi, kayit);
            await Yukle();
            Snackbar.Add(
                sonuc.CevrilenAlanSayisi == 0
                    ? DilServisi.T("admin.ceviri.kayitGuncel", "Bu kayit icin tum diller zaten guncel.")
                    : string.Format(DilServisi.T("admin.ceviri.kayitTamamlandi", "Kayit cevirisi tamamlandi. {0} alan guncellendi."), sonuc.CevrilenAlanSayisi),
                sonuc.CevrilenAlanSayisi == 0 ? Severity.Info : Severity.Success);
        }
        finally
        {
            _tumCeviriCalisiyor = false;
            StateHasChanged();
        }
    }

    private static AdminCeviriKaydi UrunKategoriCeviriKaydiOlustur(UrunKategori uk)
    {
        return new AdminCeviriKaydi(uk.Id,
        [
            new("Ad", uk.Ad ?? string.Empty),
            new("Aciklama", uk.Aciklama ?? string.Empty)
        ]);
    }
}
