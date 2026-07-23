using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class KatalogYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;

    private const long MaksimumKatalogDosyaBoyutu = 80_000_000;

    private List<Katalog> _liste = [];
    private List<Katalog> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private bool _dosyaYukleniyor;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private Katalog _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Katalog>>("api/kataloglar") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("Katalog", _liste.Select(KatalogCeviriKaydiOlustur));
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
                (x.Baslik?.ToLower().Contains(a) ?? false) ||
                (x.Aciklama?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new Katalog { AktifMi = true };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    void Duzenle(Katalog k)
    {
        _form = new Katalog
        {
            Id = k.Id,
            Baslik = k.Baslik,
            Aciklama = k.Aciklama,
            KapakResim = k.KapakResim,
            PdfDosyaYolu = k.PdfDosyaYolu,
            Yil = k.Yil,
            SiraNo = k.SiraNo,
            SayfaSayisi = k.SayfaSayisi,
            AktifMi = k.AktifMi
        };
        _duzenlenenId = k.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    async Task DosyaYukle(IBrowserFile dosya)
    {
        if (dosya is null)
        {
            return;
        }

        _dosyaYukleniyor = true;
        using var icerik = new MultipartFormDataContent();
        using var dosyaAkisi = dosya.OpenReadStream(MaksimumKatalogDosyaBoyutu);
        using var dosyaIcerigi = new StreamContent(dosyaAkisi);
        icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

        var cevap = await Api.PostMultipartAsync<KatalogDosyaYuklemeSonucu>("api/kataloglar/dosya-yukle", icerik);
        if (cevap?.BasariliMi == true && cevap.Veri is not null)
        {
            _form.PdfDosyaYolu = cevap.Veri.Yol;
            _form.KapakResim = cevap.Veri.OnizlemeYolu;
            _form.DosyaBoyutuMb = cevap.Veri.BoyutMb;
            Snackbar.Add(DilServisi.T("admin.katalog.dosyaYuklendi", "PDF yuklendi. Kaydet butonuna basarak katalogu yayinlayabilirsiniz."), Severity.Success);
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? DilServisi.T("admin.katalog.dosyaYuklemeHatasi", "PDF yuklenemedi."), Severity.Error);
        }

        _dosyaYukleniyor = false;
    }

    async Task SilOnay(Katalog k)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{k.Baslik}' kaydı kalıcı olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(k);
    }

    async Task Sil(Katalog k)
    {
        var cevap = await Api.DeleteAsync($"api/kataloglar/{k.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Katalog silinemedi.", Severity.Error);
        }
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        Cevap<Katalog>? cevap;
        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<Katalog>($"api/kataloglar/{_duzenlenenId.Value}", _form);
        else
            cevap = await Api.PostAsync<Katalog>("api/kataloglar", _form);

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Katalog kaydedilemedi.", Severity.Error);
        }

        _kaydediliyor = false;
    }

    void FormIptal() { _formAcik = false; }

    async Task AICeviriDialogAc(Katalog k)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", k.Id },
            { "TabloOnEki", "Katalog" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Baslik", k.Baslik ?? "" },
                    { "Aciklama", k.Aciklama ?? "" }
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
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync("Katalog", _liste.Select(KatalogCeviriKaydiOlustur), async (islenen, toplam) =>
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

    async Task KaydiTumDillerdeCevir(Katalog k)
    {
        if (_tumCeviriCalisiyor) return;
        _tumCeviriCalisiyor = true;
        StateHasChanged();
        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("Katalog", KatalogCeviriKaydiOlustur(k));
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

    private static AdminCeviriKaydi KatalogCeviriKaydiOlustur(Katalog k)
    {
        return new AdminCeviriKaydi(k.Id,
        [
            new("Baslik", k.Baslik ?? string.Empty),
            new("Aciklama", k.Aciklama ?? string.Empty)
        ]);
    }

    private sealed record KatalogDosyaYuklemeSonucu(string Yol, string? OnizlemeYolu, double BoyutMb, long BoyutByte);
}

