using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class SlaytYonetimi : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private AdminCeviriServisi AdminCeviriServisi { get; set; } = default!;

    private List<Slayt> _liste = [];
    private List<Slayt> _filtreliListe = [];
    private bool _yukleniyor = true, _formAcik, _duzenlemeModu, _kaydediliyor;
    private bool _tumCeviriCalisiyor;
    private bool _gorselYukleniyor;
    private int _ceviriIslenen;
    private int _ceviriToplam;
    private Slayt _form = new();
    private int? _duzenlenenId;
    private string _arama = string.Empty;
    private AdminCeviriAnalizSonucu? _ceviriAnalizi;

    // Medya Galerisi
    private bool _galeriDialog;
    private bool _galeriYukleniyor;
    private string _galeriHedef = "arkaplan"; // "arkaplan" veya "mobil"
    private int _galeriSeciliId;
    private string _galeriArama = "";
    private List<Medya> _galeriListe = [];

    protected override async Task OnInitializedAsync() => await Yukle();

    async Task Yukle()
    {
        _yukleniyor = true; StateHasChanged();
        _liste = await Api.GetAsync<List<Slayt>>("api/admin/icerik/slaytlar") ?? [];
        _ceviriAnalizi = await AdminCeviriServisi.AnalizEtAsync("Slayt", _liste.Select(SlaytCeviriKaydiOlustur));
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
                              (x.AltBaslik?.ToLower().Contains(a) ?? false)).ToList();
    }

    void YeniAc()
    {
        _form = new Slayt
        {
            SayfaKodu = "anasayfa",
            AktifMi = true,
            Dil = "tr",
            AnimasyonTipi = "fade",
            GecisHizi = 800,
            GosterimSuresi = 5000,
            MetinHizalama = "sol"
        };
        _duzenlenenId = null;
        _duzenlemeModu = false;
        _formAcik = true;
    }

    void Duzenle(Slayt x)
    {
        _form = new Slayt
        {
            Id = x.Id,
            SayfaKodu = x.SayfaKodu ?? "anasayfa",
            Dil = x.Dil,
            Baslik = x.Baslik,
            AltBaslik = x.AltBaslik,
            Aciklama = x.Aciklama,
            ArkaplanResim = x.ArkaplanResim,
            ArkaplanResimMobil = x.ArkaplanResimMobil,
            ButonMetni1 = x.ButonMetni1,
            ButonLink1 = x.ButonLink1,
            ButonMetni2 = x.ButonMetni2,
            ButonLink2 = x.ButonLink2,
            AnimasyonTipi = x.AnimasyonTipi,
            GecisHizi = x.GecisHizi,
            GosterimSuresi = x.GosterimSuresi,
            MetinHizalama = x.MetinHizalama,
            MetinRengi = x.MetinRengi,
            SiraNo = x.SiraNo,
            AktifMi = x.AktifMi,
            BaslangicTarihi = x.BaslangicTarihi,
            BitisTarihi = x.BitisTarihi,
            OlusturulmaTarihi = x.OlusturulmaTarihi
        };
        _duzenlenenId = x.Id;
        _duzenlemeModu = true;
        _formAcik = true;
    }

    async Task SilOnay(Slayt x)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            "Silme Onayı",
            $"'{x.Baslik}' kaydı kalıcı olarak silinecektir. Emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");
        if (onay == true) await Sil(x);
    }

    async Task Sil(Slayt x)
    {
        var cevap = await Api.DeleteAsync($"api/admin/icerik/slaytlar/{x.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Slayt silinemedi.", Severity.Error);
        }
    }

    async Task Kaydet()
    {
        _kaydediliyor = true;
        Cevap<Slayt>? cevap;
        if (_duzenlenenId.HasValue)
            cevap = await Api.PutAsync<Slayt>($"api/admin/icerik/slaytlar/{_duzenlenenId.Value}", _form);
        else
            cevap = await Api.PostAsync<Slayt>("api/admin/icerik/slaytlar", _form);

        if (cevap?.BasariliMi == true)
        {
            _formAcik = false;
            Snackbar.Add(cevap.Mesaj, Severity.Success);
            await Yukle();
        }
        else
        {
            Snackbar.Add(cevap?.Mesaj ?? "Slayt kaydedilemedi.", Severity.Error);
        }

        _kaydediliyor = false;
    }

    void FormIptal() { _formAcik = false; }

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol)) return string.Empty;
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        return $"{Api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private async Task GorselYukle(string hedef, IBrowserFile? dosya)
    {
        if (dosya is null) return;
        _gorselYukleniyor = true;
        try
        {
            using var icerik = new MultipartFormDataContent();
            using var akis = dosya.OpenReadStream(20_000_000);
            using var dosyaIcerigi = new StreamContent(akis);
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);
            icerik.Add(new StringContent("slaytlar"), "klasor");

            var cevap = await Api.PostMultipartAsync<Medya>("api/medya/yukle", icerik);
            if (cevap?.BasariliMi == true && cevap.Veri != null)
            {
                var gorselUrl = $"/api/medya/dosya/{cevap.Veri.Id}";
                if (hedef == "mobil")
                    _form.ArkaplanResimMobil = gorselUrl;
                else
                    _form.ArkaplanResim = gorselUrl;
                Snackbar.Add("Görsel yüklendi.", Severity.Success);
            }
            else
                Snackbar.Add(cevap?.Mesaj ?? "Görsel yüklenemedi.", Severity.Error);
        }
        catch (Exception ex) { Snackbar.Add($"Yükleme hatası: {ex.Message}", Severity.Error); }
        finally { _gorselYukleniyor = false; }
    }

    private async Task GaleriAc(string hedef)
    {
        _galeriHedef = hedef;
        _galeriSeciliId = 0;
        _galeriArama = "";
        _galeriYukleniyor = true;
        _galeriDialog = true;
        var liste = await Api.GetAsync<List<Medya>>("api/medya");
        _galeriListe = liste?.Where(m => m.Tip == MedyaTipi.Resim).ToList() ?? [];
        _galeriYukleniyor = false;
    }

    private async Task GaleriAramaDegisti(string deger)
    {
        _galeriArama = deger;
        _galeriYukleniyor = true;
        var url = string.IsNullOrWhiteSpace(deger) ? "api/medya" : $"api/medya?q={Uri.EscapeDataString(deger)}";
        var liste = await Api.GetAsync<List<Medya>>(url);
        _galeriListe = liste?.Where(m => m.Tip == MedyaTipi.Resim).ToList() ?? [];
        _galeriYukleniyor = false;
    }

    private void GaleriSec(Medya medya) => _galeriSeciliId = medya.Id;
    private void GaleriKapat() => _galeriDialog = false;

    private void GaleriOnayla()
    {
        if (_galeriSeciliId <= 0) return;
        var gorselUrl = $"/api/medya/dosya/{_galeriSeciliId}";
        if (_galeriHedef == "mobil")
            _form.ArkaplanResimMobil = gorselUrl;
        else
            _form.ArkaplanResim = gorselUrl;
        _galeriDialog = false;
        Snackbar.Add("Görsel seçildi.", Severity.Success);
    }

    private string MedyaGorselYolu(Medya medya)
    {
        return $"{Api.ApiBaseUrl}/api/medya/dosya/{medya.Id}";
    }

    async Task AICeviriDialogAc(Slayt s)
    {
        var parameters = new DialogParameters
        {
            { "KayitId", s.Id },
            { "TabloOnEki", "Slayt" },
            { "CevrilecekAlanlar", new Dictionary<string, string>
                {
                    { "Baslik", s.Baslik ?? "" },
                    { "AltBaslik", s.AltBaslik ?? "" },
                    { "Aciklama", s.Aciklama ?? "" },
                    { "ButonMetni1", s.ButonMetni1 ?? "" },
                    { "ButonMetni2", s.ButonMetni2 ?? "" }
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
            var sonuc = await AdminCeviriServisi.TumunuCevirAsync("Slayt", _liste.Select(SlaytCeviriKaydiOlustur), async (islenen, toplam) =>
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

    async Task KaydiTumDillerdeCevir(Slayt s)
    {
        if (_tumCeviriCalisiyor)
            return;

        _tumCeviriCalisiyor = true;
        StateHasChanged();

        try
        {
            var sonuc = await AdminCeviriServisi.KaydiCevirAsync("Slayt", SlaytCeviriKaydiOlustur(s));
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

    private static AdminCeviriKaydi SlaytCeviriKaydiOlustur(Slayt s)
    {
        return new AdminCeviriKaydi(s.Id,
        [
            new("Baslik", s.Baslik ?? string.Empty),
            new("AltBaslik", s.AltBaslik ?? string.Empty),
            new("Aciklama", s.Aciklama ?? string.Empty),
            new("ButonMetni1", s.ButonMetni1 ?? string.Empty),
            new("ButonMetni2", s.ButonMetni2 ?? string.Empty)
        ]);
    }
}
