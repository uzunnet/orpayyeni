using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class CeviriYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private DilServisi Dil { get; set; } = default!;
    [Inject] private IDialogService Dialog { get; set; } = default!;

    private List<Ceviri> _liste = [];
    private List<Ceviri> _filtreliListe = [];
    private List<Dil> _tumDiller = [];
    private List<string> _diller = [];
    private List<string> _bolumler = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _kaydediliyor;
    private bool _aiCeviriliyor;
    private int _aiCeviriSayac;
    private int _aiCeviriToplam;
    private Ceviri _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private string? _filtreDil;
    private string? _filtreBolum;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Ceviri>>("api/dil/admin/tum-ceviriler") ?? [];
        _diller = _liste.Select(x => x.Dil).Distinct().OrderBy(x => x).ToList();
        _bolumler = _liste.Select(x => x.Bolum)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct().OrderBy(x => x).OfType<string>().ToList();
        _tumDiller = await Api.GetAsync<List<Dil>>("api/dil/admin/diller") ?? [];
        FiltreUygula(null);
        _yukleniyor = false;
    }

    async Task DilAktifDegistir(Dil d, bool yeniDurum)
    {
        d.AktifMi = yeniDurum;
        await Api.PutAsync<object>($"api/dil/admin/dil/{d.Id}", new { d.AktifMi, d.SiraNo });
        Snackbar.Add(
            string.Format(Dil.T("admin.ceviri.dilGuncellendi", "{0} basariyla guncellendi."), d.Ad),
            Severity.Success);
    }

    async Task DilSiraGuncelle(Dil d, int yeniSira)
    {
        d.SiraNo = yeniSira;
        await Api.PutAsync<object>($"api/dil/admin/dil/{d.Id}", new { d.AktifMi, SiraNo = yeniSira });
        Snackbar.Add(
            string.Format(Dil.T("admin.ceviri.dilSiraGuncellendi", "{0} sirasi {1} olarak guncellendi."), d.Ad, yeniSira),
            Severity.Success);
    }

    void AramaYap(KeyboardEventArgs e) => FiltreUygula(null);

    void FiltreDilDegisti(string? yeniDeger)
    {
        _filtreDil = yeniDeger;
        FiltreUygula(null);
    }

    void FiltreBolumDegisti(string? yeniDeger)
    {
        _filtreBolum = yeniDeger;
        FiltreUygula(null);
    }

    void FiltreUygula(string? _)
    {
        var a = _arama?.ToLower() ?? "";
        var sorgu = _liste.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(a))
            sorgu = sorgu.Where(x =>
                (x.Anahtar?.ToLower().Contains(a) ?? false) ||
                (x.Deger?.ToLower().Contains(a) ?? false) ||
                (x.Bolum?.ToLower().Contains(a) ?? false));

        if (!string.IsNullOrWhiteSpace(_filtreDil))
            sorgu = sorgu.Where(x => x.Dil == _filtreDil);

        if (!string.IsNullOrWhiteSpace(_filtreBolum))
            sorgu = sorgu.Where(x => x.Bolum == _filtreBolum);

        _filtreliListe = sorgu.ToList();
    }

    void YeniCeviri()
    {
        _form = new Ceviri
        {
            Dil = _filtreDil ?? "tr",
            Bolum = _filtreBolum ?? ""
        };
        _duzenlenenId = null;
        _formAcik = true;
    }

    void Duzenle(Ceviri c)
    {
        _form = new Ceviri
        {
            Id = c.Id,
            Anahtar = c.Anahtar,
            Dil = c.Dil,
            Deger = c.Deger,
            Bolum = c.Bolum
        };
        _duzenlenenId = c.Id;
        _formAcik = true;
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        try
        {
            if (_duzenlenenId.HasValue)
                await Api.PutAsync<Ceviri>("api/dil/admin/ceviri", _form);
            else
                await Api.PostAsync<object>("api/dil/ceviri-ekle", new { _form.Anahtar, _form.Dil, _form.Deger, _form.Bolum });

            _formAcik = false;
            Snackbar.Add(Dil.T("admin.ceviri.kaydedildi", "Ceviri basariyla kaydedildi."), Severity.Success);
            await Yukle();
        }
        catch (Exception ex) { Snackbar.Add($"Hata: {ex.Message}", Severity.Error); }
        finally { _kaydediliyor = false; }
    }

    void FormIptal() { _formAcik = false; }

    async Task Sil(Ceviri c)
    {
        bool? onay = await Dialog.ShowMessageBoxAsync(
            Dil.T("admin.ceviri.silOnayBaslik", "Ceviri Sil"),
            string.Format(Dil.T("admin.ceviri.silOnayMesaj", "{0} anahtarli ceviriyi silmek istediginize emin misiniz?"), c.Anahtar),
            yesText: Dil.T("admin.ceviri.evet", "Evet"),
            cancelText: Dil.T("admin.ceviri.iptal", "Iptal"));

        if (onay != true) return;

        await Api.DeleteAsync($"api/dil/admin/ceviri/{c.Id}");
        Snackbar.Add(Dil.T("admin.ceviri.silindi", "Ceviri basariyla silindi."), Severity.Success);
        await Yukle();
    }

    /// <summary>
    /// Filtredeki kaynak dilden hedef dile eksik cevirileri AI ile tamamlar.
    /// Filtre yoksa TR -> EN varsayilan mantigi kullanir.
    /// </summary>
    async Task AIileTopluCevir()
    {
        if (_aiCeviriliyor) return;
        _aiCeviriliyor = true;
        _aiCeviriSayac = 0;

        var kaynakDil = _filtreDil ?? "tr";
        var hedefDil = "en";

        var kaynakAnahtarlar = _liste.Where(c => c.Dil == kaynakDil).Select(c => c.Anahtar).ToHashSet();
        var hedefAnahtarlar = _liste.Where(c => c.Dil == hedefDil).Select(c => c.Anahtar).ToHashSet();
        var eksikler = _liste
            .Where(c => c.Dil == kaynakDil && !string.IsNullOrWhiteSpace(c.Deger) && !hedefAnahtarlar.Contains(c.Anahtar))
            .Take(20)
            .ToList();

        _aiCeviriToplam = eksikler.Count;

        if (_aiCeviriToplam == 0)
        {
            Snackbar.Add(Dil.T("admin.ceviri.eksikBulunamadi", "Eksik ceviri bulunamadi."), Severity.Info);
            _aiCeviriliyor = false;
            return;
        }

        foreach (var c in eksikler)
        {
            _aiCeviriSayac++;
            StateHasChanged();

            var ceviri = await Dil.AICeviriAlAsync(c.Anahtar, c.Deger, hedefDil);
            if (ceviri != null)
            {
                await Api.PostAsync<object>("api/dil/ceviri-ekle", new
                {
                    Anahtar = c.Anahtar,
                    Dil = hedefDil,
                    Deger = ceviri,
                    Bolum = c.Bolum
                });
            }
            await Task.Delay(200);
        }

        await Yukle();
        _aiCeviriliyor = false;
        Snackbar.Add(
            string.Format(Dil.T("admin.ceviri.aiTamamlandi", "{0} ceviri AI ile tamamlandi."), _aiCeviriToplam),
            Severity.Success);
    }
}
