using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class ReferansYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<Referans> _liste = [];
    private List<Referans> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenleme;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private Referans _form = new();
    private int? _id;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<Referans>>("api/admin/icerik/referanslar") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("Referans", _liste.Select(ReferansCeviriKaydiOlustur));
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
                (x.Tip?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new Referans { AktifMi = true };
        _id = null;
        _duzenleme = false;
        _formAcik = true;
    }

    void Duzenle(Referans r)
    {
        _form = new Referans
        {
            Id = r.Id,
            Ad = r.Ad,
            Tip = r.Tip,
            Logo = r.Logo,
            WebSite = r.WebSite,
            Aciklama = r.Aciklama,
            SiraNo = r.SiraNo,
            AktifMi = r.AktifMi
        };
        _id = r.Id;
        _duzenleme = true;
        _formAcik = true;
    }

    async Task Kaydet()
    {
        Cevap<Referans>? cevap;
        if (_id.HasValue)
            cevap = await Api.PutAsync<Referans>($"api/admin/icerik/referanslar/{_id.Value}", _form);
        else
            cevap = await Api.PostAsync<Referans>("api/admin/icerik/referanslar", _form);

        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            _formAcik = false;
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Referans kaydedilemedi.", Severity.Error);
        }
    }

    void Iptal()
    {
        _formAcik = false;
    }

    async Task SilOnay(Referans r)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{r.Ad}' referansı silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(r);
    }

    async Task Sil(Referans r)
    {
        var cevap = await Api.DeleteAsync($"api/admin/icerik/referanslar/{r.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Referans silinemedi.", Severity.Error);
        }
    }

    async Task AICeviriDialogAc(Referans r)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", r.Id },
            { "TabloOnEki", "Referans" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Ad", r.Ad ?? "" },
                    { "Tip", r.Tip ?? "" },
                    { "Aciklama", r.Aciklama ?? "" }
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
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync("Referans", _liste.Select(ReferansCeviriKaydiOlustur), async (islenen, toplam) =>
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

    async Task KaydiTumDillerdeCevir(Referans r)
    {
        if (_tumCeviriCalisiyor)
            return;

        _tumCeviriCalisiyor = true;
        StateHasChanged();

        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("Referans", ReferansCeviriKaydiOlustur(r));
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

    private static AdminCeviriKaydi ReferansCeviriKaydiOlustur(Referans r)
    {
        return new AdminCeviriKaydi(r.Id,
        [
            new("Ad", r.Ad ?? string.Empty),
            new("Tip", r.Tip ?? string.Empty),
            new("Aciklama", r.Aciklama ?? string.Empty)
        ]);
    }
}
