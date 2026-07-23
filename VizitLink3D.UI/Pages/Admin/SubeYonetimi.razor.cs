using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SubeYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<Sube> _liste = [];
    private List<Sube> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private Sube _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Sube>>("api/subeler") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("Sube", _liste.Select(SubeCeviriKaydiOlustur));
        AramaUygula();
        _yukleniyor = false;
    }

    void AramaYap(KeyboardEventArgs e) => AramaUygula();
    void AramaMetniDegisti(string deger) { _arama = deger; AramaUygula(); }

    void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a)
            ? _liste
            : _liste.Where(x =>
                (x.Ad?.ToLower().Contains(a) ?? false) ||
                (x.Sehir?.ToLower().Contains(a) ?? false) ||
                (x.Telefon?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new Sube { AktifMi = true };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    void Duzenle(Sube s)
    {
        _form = new Sube
        {
            Id = s.Id,
            Ad = s.Ad,
            Adres = s.Adres,
            Sehir = s.Sehir,
            Ilce = s.Ilce,
            Telefon = s.Telefon,
            Eposta = s.Eposta,
            Enlem = s.Enlem,
            Boylam = s.Boylam,
            CalismaSaatleri = s.CalismaSaatleri,
            Aciklama = s.Aciklama,
            SubeYetkilisi = s.SubeYetkilisi,
            SubeYetkilisiTelefon = s.SubeYetkilisiTelefon,
            SiraNo = s.SiraNo,
            AktifMi = s.AktifMi
        };
        _duzenlenenId = s.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    async Task SilOnay(Sube s)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{s.Ad}' şubesi kalıcı olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(s);
    }

    async Task Sil(Sube s)
    {
        var cevap = await Api.DeleteAsync($"api/subeler/{s.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Sube silinemedi.", Severity.Error);
        }
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        Cevap<Sube>? cevap;
        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<Sube>($"api/subeler/{_duzenlenenId.Value}", _form);
        else
            cevap = await Api.PostAsync<Sube>("api/subeler", _form);

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Sube kaydedilemedi.", Severity.Error);
        }

        _kaydediliyor = false;
    }

    void FormIptal() { _formAcik = false; }

    async Task AICeviriDialogAc(Sube s)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", s.Id },
            { "TabloOnEki", "Sube" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Ad", s.Ad ?? "" },
                    { "Adres", s.Adres ?? "" },
                    { "Sehir", s.Sehir ?? "" },
                    { "Ilce", s.Ilce ?? "" }
                }
            }
        };

        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Admin.AICeviriDialog>("🌍 Yapay Zeka Çevirisi", parameters);
        await dialog.Result;
        await Yukle();
    }

    async Task TumuCevir()
    {
        if (_tumCeviriCalisiyor || _liste.Count == 0) return;
        _tumCeviriCalisiyor = true;
        _ceviriIslenen = 0;
        _ceviriToplam = _liste.Count;
        StateHasChanged();
        try
        {
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync("Sube", _liste.Select(SubeCeviriKaydiOlustur), async (islenen, toplam) =>
            {
                _ceviriIslenen = islenen;
                _ceviriToplam = toplam;
                StateHasChanged();
                await Task.CompletedTask;
            });
            await Yukle();
            Snackbar.Add(string.Format(dil.T("admin.ceviri.topluTamamlandi", "Toplu ceviri tamamlandi. {0} kayitta toplam {1} alan cevrildi."), sonuc.CevrilenKayitSayisi, sonuc.CevrilenAlanSayisi), Severity.Success);
        }
        finally
        {
            _tumCeviriCalisiyor = false;
            _ceviriIslenen = 0;
            _ceviriToplam = 0;
            StateHasChanged();
        }
    }

    async Task KaydiTumDillerdeCevir(Sube s)
    {
        if (_tumCeviriCalisiyor) return;
        _tumCeviriCalisiyor = true;
        StateHasChanged();
        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("Sube", SubeCeviriKaydiOlustur(s));
            await Yukle();
            Snackbar.Add(sonuc.CevrilenAlanSayisi == 0
                ? dil.T("admin.ceviri.kayitGuncel", "Bu kayit icin tum diller zaten guncel.")
                : string.Format(dil.T("admin.ceviri.kayitTamamlandi", "Kayit cevirisi tamamlandi. {0} alan guncellendi."), sonuc.CevrilenAlanSayisi),
                sonuc.CevrilenAlanSayisi == 0 ? Severity.Info : Severity.Success);
        }
        finally
        {
            _tumCeviriCalisiyor = false;
            StateHasChanged();
        }
    }

    private static AdminCeviriKaydi SubeCeviriKaydiOlustur(Sube s)
    {
        return new AdminCeviriKaydi(s.Id,
        [
            new("Ad", s.Ad ?? string.Empty),
            new("Adres", s.Adres ?? string.Empty),
            new("Sehir", s.Sehir ?? string.Empty),
            new("Ilce", s.Ilce ?? string.Empty)
        ]);
    }
}

