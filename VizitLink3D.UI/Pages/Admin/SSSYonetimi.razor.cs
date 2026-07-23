using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SSSYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<SikSorulanSoru> _liste = [];
    private List<SikSorulanSoru> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenleme;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private SikSorulanSoru _form = new();
    private int? _id;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<SikSorulanSoru>>("api/admin/icerik/sss") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("SSS", _liste.Select(SSSCeviriKaydiOlustur));
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
                (x.Soru?.ToLower().Contains(a) ?? false) ||
                (x.KategoriAdi?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new SikSorulanSoru { AktifMi = true };
        _id = null;
        _duzenleme = false;
        _formAcik = true;
    }

    void Duzenle(SikSorulanSoru s)
    {
        _form = new SikSorulanSoru
        {
            Id = s.Id,
            Soru = s.Soru,
            Cevap = s.Cevap,
            KategoriAdi = s.KategoriAdi,
            SiraNo = s.SiraNo,
            AktifMi = s.AktifMi
        };
        _id = s.Id;
        _duzenleme = true;
        _formAcik = true;
    }

    async Task Kaydet()
    {
        Cevap<SikSorulanSoru>? cevap;
        if (_id.HasValue)
            cevap = await Api.PutAsync<SikSorulanSoru>($"api/admin/icerik/sss/{_id.Value}", _form);
        else
            cevap = await Api.PostAsync<SikSorulanSoru>("api/admin/icerik/sss", _form);

        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            _formAcik = false;
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "SSS kaydedilemedi.", Severity.Error);
        }
    }

    void Iptal()
    {
        _formAcik = false;
    }

    async Task SilOnay(SikSorulanSoru s)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{s.Soru}' sorusu silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(s);
    }

    async Task Sil(SikSorulanSoru s)
    {
        var cevap = await Api.DeleteAsync($"api/admin/icerik/sss/{s.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "SSS silinemedi.", Severity.Error);
        }
    }

    async Task AICeviriDialogAc(SikSorulanSoru s)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", s.Id },
            { "TabloOnEki", "SSS" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Soru", s.Soru ?? "" },
                    { "Cevap", s.Cevap ?? "" },
                    { "KategoriAdi", s.KategoriAdi ?? "" }
                }
            }
        };

        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Admin.AICeviriDialog>("🌍 Yapay Zeka Çevirisi", parameters);
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
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync("SSS", _liste.Select(SSSCeviriKaydiOlustur), async (islenen, toplam) =>
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

    async Task KaydiTumDillerdeCevir(SikSorulanSoru s)
    {
        if (_tumCeviriCalisiyor)
            return;

        _tumCeviriCalisiyor = true;
        StateHasChanged();

        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("SSS", SSSCeviriKaydiOlustur(s));
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

    private static AdminCeviriKaydi SSSCeviriKaydiOlustur(SikSorulanSoru s)
    {
        return new AdminCeviriKaydi(s.Id,
        [
            new("Soru", s.Soru ?? string.Empty),
            new("Cevap", s.Cevap ?? string.Empty),
            new("KategoriAdi", s.KategoriAdi ?? string.Empty)
        ]);
    }
}
