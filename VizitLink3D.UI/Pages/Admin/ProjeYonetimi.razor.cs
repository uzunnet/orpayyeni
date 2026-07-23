using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class ProjeYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<Proje> _liste = [];
    private List<Proje> _filtreliListe = [];
    private bool _yukleniyor = true, _formAcik, _duzenlemeModu, _kaydediliyor;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private Proje _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true; StateHasChanged();
        _liste = await Api.GetAsync<List<Proje>>("api/projeler") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("Proje", _liste.Select(ProjeCeviriKaydiOlustur));
        AramaUygula();
        _yukleniyor = false;
    }

    void AramaYap(KeyboardEventArgs e) => AramaUygula();

    void AramaUygula()
    {
        var a = _arama?.ToLower() ?? "";
        _filtreliListe = string.IsNullOrWhiteSpace(a) ? _liste :
            _liste.Where(x => (x.Baslik?.ToLower().Contains(a) ?? false) ||
                              (x.MusteriAdi?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new();
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    void Duzenle(Proje x)
    {
        _form = new Proje
        {
            Id = x.Id,
            Slug = x.Slug,
            Baslik = x.Baslik,
            KisaAciklama = x.KisaAciklama,
            Aciklama = x.Aciklama,
            KategoriId = x.KategoriId,
            MusteriAdi = x.MusteriAdi,
            MusteriSehir = x.MusteriSehir,
            ProjeTarihi = x.ProjeTarihi,
            KapakResim = x.KapakResim,
            OneCikanMi = x.OneCikanMi,
            SiraNo = x.SiraNo,
            AktifMi = x.AktifMi,
            SeoBaslik = x.SeoBaslik,
            SeoAciklama = x.SeoAciklama,
            OlusturulmaTarihi = x.OlusturulmaTarihi
        };
        _duzenlenenId = x.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    async Task SilOnay(Proje x)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{x.Baslik}' kaydı kalıcı olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(x);
    }

    async Task Sil(Proje x)
    {
        await Api.DeleteAsync($"api/projeler/{x.Id}");
        Snackbar.Add("Kayıt başarıyla silindi.", Severity.Success);
        await Yukle();
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        try
        {
            if (_duzenlenenId.HasValue)
                await Api.PutAsync<Proje>($"api/projeler/{_duzenlenenId.Value}", _form);
            else
                await Api.PostAsync<Proje>("api/projeler", _form);
            _formAcik = false;
            Snackbar.Add(_duzenlenenId.HasValue ? "Kayıt güncellendi." : "Yeni kayıt eklendi.", Severity.Success);
            await Yukle();
        }
        catch (Exception ex) { Snackbar.Add($"Hata: {ex.Message}", Severity.Error); }
        finally { _kaydediliyor = false; }
    }

    void FormIptal() { _formAcik = false; }

    async Task AICeviriDialogAc(Proje x)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", x.Id },
            { "TabloOnEki", "Proje" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Baslik", x.Baslik ?? "" },
                    { "KisaAciklama", x.KisaAciklama ?? "" },
                    { "Aciklama", x.Aciklama ?? "" }
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
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync("Proje", _liste.Select(ProjeCeviriKaydiOlustur), async (islenen, toplam) =>
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

    async Task KaydiTumDillerdeCevir(Proje x)
    {
        if (_tumCeviriCalisiyor) return;
        _tumCeviriCalisiyor = true;
        StateHasChanged();
        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("Proje", ProjeCeviriKaydiOlustur(x));
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

    private static AdminCeviriKaydi ProjeCeviriKaydiOlustur(Proje x)
    {
        return new AdminCeviriKaydi(x.Id,
        [
            new("Baslik", x.Baslik ?? string.Empty),
            new("KisaAciklama", x.KisaAciklama ?? string.Empty),
            new("Aciklama", x.Aciklama ?? string.Empty)
        ]);
    }
}
