using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class HizmetAdimiYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<HizmetAdimi> _liste = [];
    private List<HizmetAdimi> _filtreliListe = [];
    private bool _yukleniyor = true;
    private bool _formAcik;
    private bool _duzenlemeModu;
    private bool _kaydediliyor;
    private bool _tumCeviriCalisiyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private HizmetAdimi _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true;
        StateHasChanged();
        _liste = await Api.GetAsync<List<HizmetAdimi>>("api/admin/icerik/hizmet-adimlari") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("HizmetAdimi", _liste.Select(HizmetAdimiCeviriKaydiOlustur));
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
        _form = new HizmetAdimi { AktifMi = true };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    void Duzenle(HizmetAdimi h)
    {
        _form = new HizmetAdimi
        {
            Id = h.Id,
            Baslik = h.Baslik,
            Aciklama = h.Aciklama,
            Ikon = h.Ikon,
            AdimNo = h.AdimNo,
            SiraNo = h.SiraNo,
            AktifMi = h.AktifMi
        };
        _duzenlenenId = h.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    async Task SilOnay(HizmetAdimi h)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{h.Baslik}' kaydı kalıcı olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(h);
    }

    async Task Sil(HizmetAdimi h)
    {
        var cevap = await Api.DeleteAsync($"api/admin/icerik/hizmet-adimlari/{h.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Hizmet adimi silinemedi.", Severity.Error);
        }
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        Cevap<HizmetAdimi>? cevap;
        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<HizmetAdimi>($"api/admin/icerik/hizmet-adimlari/{_duzenlenenId.Value}", _form);
        else
            cevap = await Api.PostAsync<HizmetAdimi>("api/admin/icerik/hizmet-adimlari", _form);

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Hizmet adimi kaydedilemedi.", Severity.Error);
        }

        _kaydediliyor = false;
    }

    void FormIptal() { _formAcik = false; }

    async Task AICeviriDialogAc(HizmetAdimi h)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", h.Id },
            { "TabloOnEki", "HizmetAdimi" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Baslik", h.Baslik ?? "" },
                    { "Aciklama", h.Aciklama ?? "" }
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
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync("HizmetAdimi", _liste.Select(HizmetAdimiCeviriKaydiOlustur), async (islenen, toplam) =>
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

    async Task KaydiTumDillerdeCevir(HizmetAdimi h)
    {
        if (_tumCeviriCalisiyor) return;
        _tumCeviriCalisiyor = true;
        StateHasChanged();
        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("HizmetAdimi", HizmetAdimiCeviriKaydiOlustur(h));
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

    private static AdminCeviriKaydi HizmetAdimiCeviriKaydiOlustur(HizmetAdimi h)
    {
        return new AdminCeviriKaydi(h.Id,
        [
            new("Baslik", h.Baslik ?? string.Empty),
            new("Aciklama", h.Aciklama ?? string.Empty)
        ]);
    }
}
