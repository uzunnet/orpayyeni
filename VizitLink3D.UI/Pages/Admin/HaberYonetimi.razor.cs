using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class HaberYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<HaberYazisi> _liste = [];
    private List<HaberYazisi> _filtreliListe = [];
    private bool _yukleniyor = true, _formAcik, _duzenlemeModu, _kaydediliyor;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private HaberYazisi _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true; StateHasChanged();
        _liste = await Api.GetAsync<List<HaberYazisi>>("api/Haber-yazilari") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("Haber", _liste.Select(HaberCeviriKaydiOlustur));
        AramaUygula();
        _yukleniyor = false;
    }

    void AramaYap(KeyboardEventArgs e) => AramaUygula();
    void AramaMetniDegisti(string deger) { _arama = deger; AramaUygula(); }

    void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a) ? _liste :
            _liste.Where(x => (x.Baslik?.ToLower().Contains(a) ?? false) ||
                              (x.Slug?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new HaberYazisi { AktifMi = true, YayinTarihi = DateTime.UtcNow };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    void Duzenle(HaberYazisi x)
    {
        _form = new HaberYazisi
        {
            Id = x.Id,
            Baslik = x.Baslik,
            Slug = x.Slug,
            Ozet = x.Ozet,
            Icerik = x.Icerik,
            AnaResimUrl = x.AnaResimUrl,
            SeoBaslik = x.SeoBaslik,
            SeoAciklama = x.SeoAciklama,
            Etiketler = x.Etiketler,
            AktifMi = x.AktifMi,
            OkunmaSayisi = x.OkunmaSayisi,
            OlusturmaTarihi = x.OlusturmaTarihi,
            YayinTarihi = x.YayinTarihi
        };
        _duzenlenenId = x.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    async Task SilOnay(HaberYazisi x)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onay�",
            $"'{x.Baslik}' kayd� kal�c� olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "�ptal");
        if (onay == true) await Sil(x);
    }

    async Task Sil(HaberYazisi x)
    {
        var cevap = await Api.DeleteAsync($"api/Haber-yazilari/{x.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Haber pasife alinamadi.", Severity.Error);
        }
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        Cevap<HaberYazisi>? cevap;
        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<HaberYazisi>($"api/Haber-yazilari/{_duzenlenenId.Value}", _form);
        else
            cevap = await Api.PostAsync<HaberYazisi>("api/Haber-yazilari", _form);

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Haber kaydedilemedi.", Severity.Error);
        }

        _kaydediliyor = false;
    }

    void FormIptal() { _formAcik = false; }

    async Task AICeviriDialogAc(HaberYazisi x)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", x.Id },
            { "TabloOnEki", "Haber" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Baslik", x.Baslik ?? "" },
                    { "Ozet", x.Ozet ?? "" },
                    { "Icerik", x.Icerik ?? "" }
                }
            }
        };

        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Admin.AICeviriDialog>("?? Yapay Zeka �evirisi", parameters);
        await dialog.Result;
        await Yukle();
    }

    async Task TumuCevir()
    {
        if (_tumCeviriCalisiyor || _liste.Count == 0)
            return;

        _tumCeviriCalisiyor = true;
        _ceviriIslenen = 0;
        _ceviriToplam = _liste.Count;
        StateHasChanged();

        try
        {
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync(
                "Haber",
                _liste.Select(HaberCeviriKaydiOlustur),
                async (islenen, toplam) =>
                {
                    _ceviriIslenen = islenen;
                    _ceviriToplam = toplam;
                    StateHasChanged();
                    await Task.CompletedTask;
                });

            await Yukle();
            Snackbar.Add(
                string.Format(
                    dil.T("admin.Haber.topluCeviriTamamlandi", "Toplu ceviri tamamlandi. {0} haberde toplam {1} alan cevrildi."),
                    sonuc.CevrilenKayitSayisi,
                    sonuc.CevrilenAlanSayisi),
                Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(
                string.Format(dil.T("admin.Haber.topluCeviriHata", "Toplu ceviri sirasinda hata olustu: {0}"), ex.Message),
                Severity.Error);
        }
        finally
        {
            _tumCeviriCalisiyor = false;
            _ceviriIslenen = 0;
            _ceviriToplam = 0;
            StateHasChanged();
        }
    }

    async Task HaberuTumDillerdeCevir(HaberYazisi Haber)
    {
        if (_tumCeviriCalisiyor)
            return;

        _tumCeviriCalisiyor = true;
        StateHasChanged();

        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("Haber", HaberCeviriKaydiOlustur(Haber));
            await Yukle();

            if (sonuc.CevrilenAlanSayisi == 0)
            {
                Snackbar.Add(
                    dil.T("admin.Haber.ceviriGuncel", "Bu haber icin tum diller zaten guncel."),
                    Severity.Info);
            }
            else
            {
                Snackbar.Add(
                    string.Format(dil.T("admin.Haber.HaberCeviriTamamlandi", "Haber cevirisi tamamlandi. {0} alan guncellendi."), sonuc.CevrilenAlanSayisi),
                    Severity.Success);
            }
        }
        finally
        {
            _tumCeviriCalisiyor = false;
            StateHasChanged();
        }
    }

    private static AdminCeviriKaydi HaberCeviriKaydiOlustur(HaberYazisi Haber)
    {
        return new AdminCeviriKaydi(Haber.Id,
        [
            new("Baslik", Haber.Baslik ?? string.Empty),
            new("Ozet", Haber.Ozet ?? string.Empty),
            new("Icerik", Haber.Icerik ?? string.Empty)
        ]);
    }
}

