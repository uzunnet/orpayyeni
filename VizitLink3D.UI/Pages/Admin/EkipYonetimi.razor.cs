using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class EkipYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<EkipUyesi> _liste = [];
    private List<EkipUyesi> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenleme;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private EkipUyesi _form = new();
    private int? _id;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<EkipUyesi>>("api/ekip") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("Ekip", _liste.Select(EkipCeviriKaydiOlustur));
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
                (x.AdSoyad?.ToLower().Contains(a) ?? false) ||
                (x.Unvan?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new EkipUyesi { AktifMi = true };
        _id = null;
        _duzenleme = false;
        _formAcik = true;
    }

    void Duzenle(EkipUyesi e)
    {
        _form = new EkipUyesi
        {
            Id = e.Id,
            AdSoyad = e.AdSoyad,
            Unvan = e.Unvan,
            Bio = e.Bio,
            Resim = e.Resim,
            Linkedin = e.Linkedin,
            SiraNo = e.SiraNo,
            AktifMi = e.AktifMi
        };
        _id = e.Id;
        _duzenleme = true;
        _formAcik = true;
    }

    async Task Kaydet()
    {
        Cevap<EkipUyesi>? cevap;
        if (_id.HasValue)
            cevap = await Api.PutAsync<EkipUyesi>($"api/ekip/{_id.Value}", _form);
        else
            cevap = await Api.PostAsync<EkipUyesi>("api/ekip", _form);

        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            _formAcik = false;
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Ekip uyesi kaydedilemedi.", Severity.Error);
        }
    }

    void Iptal()
    {
        _formAcik = false;
    }

    async Task SilOnay(EkipUyesi e)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{e.AdSoyad}' kaydı silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(e);
    }

    async Task Sil(EkipUyesi e)
    {
        var cevap = await Api.DeleteAsync($"api/ekip/{e.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Ekip uyesi silinemedi.", Severity.Error);
        }
    }

    async Task AICeviriDialogAc(EkipUyesi e)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", e.Id },
            { "TabloOnEki", "Ekip" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "AdSoyad", e.AdSoyad ?? "" },
                    { "Unvan", e.Unvan ?? "" },
                    { "Bio", e.Bio ?? "" }
                }
            }
        };

        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Admin.AICeviriDialog>("🌍 Yapay Zeka Çevirisi", parameters);
        await dialog.Result;
        await Yukle();
    }

    async Task TumuCevir() => await TumunuCevirDahili("Ekip", _liste.Select(EkipCeviriKaydiOlustur));

    async Task KaydiTumDillerdeCevir(EkipUyesi e) => await KaydiCevirDahili("Ekip", EkipCeviriKaydiOlustur(e));

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

    private async Task KaydiCevirDahili(string modulAdi, AdminCeviriKaydi kayit)
    {
        if (_tumCeviriCalisiyor) return;
        _tumCeviriCalisiyor = true;
        StateHasChanged();
        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync(modulAdi, kayit);
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

    private static AdminCeviriKaydi EkipCeviriKaydiOlustur(EkipUyesi e)
    {
        return new AdminCeviriKaydi(e.Id,
        [
            new("AdSoyad", e.AdSoyad ?? string.Empty),
            new("Unvan", e.Unvan ?? string.Empty),
            new("Bio", e.Bio ?? string.Empty)
        ]);
    }
}

