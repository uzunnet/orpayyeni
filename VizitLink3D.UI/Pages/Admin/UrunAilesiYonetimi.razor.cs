using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class UrunAilesiYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private readonly List<DetaySablonuSecenegi> DetaySablonlari =
    [
        new("Endustriyel3D", "Endustriyel 3D"),
        new("KatalogGorselAgirlikli", "Katalog gorsel agirlikli"),
        new("MinimalTeklif", "Minimal teklif"),
        new("TeknikOzellikAgirlikli", "Teknik ozellik agirlikli"),
        new("BanyoKonfigurator", "Banyo konfigurator"),
        new("DusakabinKonfigurator", "Dusakabin konfigurator"),
        new("KapiKonfigurator", "Kapi konfigurator"),
        new("KapakKonfigurator", "Kapak konfigurator")
    ];

    private List<UrunAilesi> _liste = [];
    private List<UrunAilesi> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private UrunAilesi _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<UrunAilesi>>("api/urun-ailesi") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("UrunAilesi", _liste.Select(UrunAilesiCeviriKaydiOlustur));
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
                (x.Aciklama?.ToLowerInvariant().Contains(arama) ?? false) ||
                (x.VarsayilanDetaySablonu?.ToLowerInvariant().Contains(arama) ?? false) ||
                SablonEtiketi(x.VarsayilanDetaySablonu).ToLowerInvariant().Contains(arama)).ToList();
    }

    private void YeniAc()
    {
        _form = new UrunAilesi { AktifMi = true, VarsayilanDetaySablonu = "Endustriyel3D" };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    private void Duzenle(UrunAilesi ua)
    {
        _form = new UrunAilesi
        {
            Id = ua.Id,
            Ad = ua.Ad,
            Slug = ua.Slug,
            Aciklama = ua.Aciklama,
            VarsayilanDetaySablonu = ua.VarsayilanDetaySablonu,
            SiraNo = ua.SiraNo,
            AktifMi = ua.AktifMi
        };
        _duzenlenenId = ua.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    private async Task SilOnay(UrunAilesi ua)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ortak.onay", "Onay"),
            string.Format(
                DilServisi.T("admin.urunAilesi.silOnay", "'{0}' urun ailesi yayindan kaldirilacak. Emin misiniz?"),
                ua.Ad ?? string.Empty),
            yesText: DilServisi.T("ortak.yayindanKaldir", "Yayindan kaldir"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));

        if (onay == true)
        {
            await Sil(ua);
        }
    }

    private async Task Sil(UrunAilesi ua)
    {
        var cevap = await Api.DeleteAsync($"api/urun-ailesi/{ua.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(DilServisi.T("admin.urunAilesi.silindi", "Ürün ailesi yayından kaldırıldı."), Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.urunAilesi.silinemedi", "Ürün ailesi yayından kaldırılamadı."), Severity.Error);
    }

    private async Task Kaydet()
    {
        if (string.IsNullOrWhiteSpace(_form.Ad) || string.IsNullOrWhiteSpace(_form.Slug))
        {
            Snackbar.Add(DilServisi.T("admin.urunAilesi.zorunluAlan", "Ad ve slug alanlari zorunludur."), Severity.Warning);
            return;
        }

        _kaydediliyor = true;
        Cevap<UrunAilesi>? cevap;
        if (_duzenlenenId.HasValue)
        {
            cevap = await Api.PutAsync<UrunAilesi>($"api/urun-ailesi/{_duzenlenenId.Value}", _form);
        }
        else
        {
            cevap = await Api.PostAsync<UrunAilesi>("api/urun-ailesi", _form);
        }

        _kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(
                _duzenlenenId.HasValue
                    ? DilServisi.T("admin.urunAilesi.guncellendi", "Ürün ailesi güncellendi.")
                    : DilServisi.T("admin.urunAilesi.eklendi", "Yeni urun ailesi eklendi."),
                Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.urunAilesi.kaydedilemedi", "Ürün ailesi kaydedilemedi."), Severity.Error);
    }

    private void FormIptal() => _formAcik = false;

    private async Task AICeviriDialogAc(UrunAilesi ua)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", ua.Id },
            { "TabloOnEki", "UrunAilesi" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Ad", ua.Ad ?? string.Empty },
                    { "Aciklama", ua.Aciklama ?? string.Empty }
                }
            }
        };

        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Admin.AICeviriDialog>(
            DilServisi.T("admin.ai.ceviri", "Yapay Zeka Cevirisi"),
            parameters);
        await dialog.Result;
        await Yukle();
    }

    private async Task TumuCevir() => await TumunuCevirDahili("UrunAilesi", _liste.Select(UrunAilesiCeviriKaydiOlustur));

    private async Task KaydiTumDillerdeCevir(UrunAilesi ua) => await KaydiCevirDahili("UrunAilesi", UrunAilesiCeviriKaydiOlustur(ua));

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

    private static AdminCeviriKaydi UrunAilesiCeviriKaydiOlustur(UrunAilesi ua)
    {
        return new AdminCeviriKaydi(ua.Id,
        [
            new("Ad", ua.Ad ?? string.Empty),
            new("Aciklama", ua.Aciklama ?? string.Empty)
        ]);
    }

    private string SablonEtiketi(string? deger)
    {
        if (string.IsNullOrWhiteSpace(deger))
        {
            return "-";
        }

        return DetaySablonlari.FirstOrDefault(x => x.Deger == deger)?.Etiket ?? deger;
    }

    private sealed record DetaySablonuSecenegi(string Deger, string Etiket);
}
