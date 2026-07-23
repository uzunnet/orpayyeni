using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.Ortak.Modeller.Istekler;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class UrunSihirbazi : ComponentBase
{
    private const string YasakTip = "Yasak";
    private const string ZorunluTip = "Zorunlu";
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    // ─── OZET URL'LERI ──────────────────────────────────────────────────
    private string UrunOnizlemeUrl => $"/urunler/{_urunForm.Slug}";
    private UrunUcBoyutModeli? SeciliUcBoyutModel =>
        _seciliModelId.HasValue
            ? _modeller.FirstOrDefault(m => m.Id == _seciliModelId.Value)
            : _modeller.OrderByDescending(m => m.VarsayilanMi).ThenBy(m => m.OlusturulmaTarihi).FirstOrDefault();

    private string? ModelDosyaUrl(UrunUcBoyutModeli? model)
    {
        if (model is null)
            return null;

        var yol = string.IsNullOrWhiteSpace(model.ModelYolu) ? model.ModelDosyaYolu : model.ModelYolu;
        if (string.IsNullOrWhiteSpace(yol))
            return null;

        return yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? yol
            : $"{Api.ApiBaseUrl}{(yol.StartsWith("/") ? yol : "/" + yol)}";
    }

    // ─── DURUM ──────────────────────────────────────────────────────────
    private bool _kaydedildi;
    private int? _urunId;

    // ─── BUTON METODLARI ────────────────────────────────────────────────
    private void YeniUrunBaslat()
    {
        _kaydedildi = false;
        _urunId = null;
        _urunForm = new Urun { AktifMi = true };
        _secilenMedyaId = null;
        _urunMedyalari = [];
    }
    private void Sekme1() => _aktifSekme = 1;
    private async Task Sekme2() { _aktifSekme = 2; await Tab2Yukle(); }
    private async Task Sekme3() { _aktifSekme = 3; await Tab3Yukle(); }
    private async Task Sekme4() { _aktifSekme = 4; await Tab4Yukle(); }
    private async Task Sekme5() { _aktifSekme = 5; await Tab5Yukle(); }
    private void Sekme6() => _aktifSekme = 6;
    private void Sekme7() => _aktifSekme = 7;

    private void ParcaSecildiEvent(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var id)) _seciliParcaId = id;
    }
    private int _aktifSekme = 1;

    // ─── TAB 1 ──────────────────────────────────────────────────────────
    private Urun _urunForm = new() { AktifMi = true };
    private List<UrunAilesi> _aileler = [];
    private List<UrunKategori> _kategoriler = [];
    private bool _tab1Kaydediliyor;

    // ─── TAB 2 ──────────────────────────────────────────────────────────
    private long? _secilenMedyaId;
    private List<UrunMedya> _urunMedyalari = [];
    private bool _tab2Yukleniyor;

    private static readonly Regex MedyaIdRegex = new(@"/api/medya/dosya/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // ─── TAB 3 ──────────────────────────────────────────────────────────
    private List<UrunUcBoyutModeli> _modeller = [];
    private List<UrunUcBoyutParcasi> _parcalar = [];
    private UrunUcBoyutParcasi _parcaForm = new() { AktifMi = true };
    private bool _tab3Yukleniyor, _tab3FormAcik, _tab3Kaydediliyor;
    private int? _seciliModelId;

    // ─── TAB 4 ──────────────────────────────────────────────────────────
    private List<RalRengi> _ralListesi = [];
    private List<Malzeme> _malzemeListesi = [];
    private List<UrunUcBoyutParcasi> _tab4Parcalar = [];
    private int? _seciliParcaId, _seciliRenkId, _seciliMalzemeId, _seciliKaplamaId;
    private List<UrunParcaRenkSecenegi> _parcaRenkleri = [];
    private List<UrunParcaMalzemeSecenegi> _parcaMalzemeleri = [];
    private List<KaplamaSecenegi> _kaplamalar = [];
    private bool _tab4Yukleniyor;

    // ─── TAB 5 ──────────────────────────────────────────────────────────
    private UrunKonfigurasyonSablonu? _sablon;
    private UrunKonfigurasyonSablonu _sablonForm = new() { DetaySablonu = "Endustriyel3D", HeroAktifMi = true, TeknikOzellikAktifMi = true, PdfKaynakAktifMi = true, BenzerUrunlerAktifMi = true, TeklifFormuAktifMi = true, AktifMi = true };
    private List<UrunKonfigurasyonKurali> _kurallar = [];
    private UrunKonfigurasyonKurali _kuralForm = new() { KuralTipi = "Yasak", AktifMi = true };
    private bool _tab5Yukleniyor, _tab5SablonKaydediliyor, _kuralFormAcik, _kuralKaydediliyor;
    private int? _duzenlenenKuralId;

    // ─── TAB 7 ──────────────────────────────────────────────────────────
    private string _secilenHedefDil = string.Empty;
    private bool _ceviriYapiliyor = false;
    private Dictionary<string, string> _sonCeviriler = new();

    // ─── LISTE ──────────────────────────────────────────────────────────
    private List<Urun> _urunListesi = [];
    private string _urunAramaMetni = string.Empty;
    private bool _listeYukleniyor = true;
    private string _urunSiralaOlcutu = "ad";
    private bool _urunSiralaArtan = true;
    private int _sayfaBoyutu = 10;
    private int _seciliSayfa = 1;
    private int _secilenListeAileId;
    private string _secilenListeAltKategori = "tum";

    private List<Urun> FiltreliUrunler => _urunListesi
        .Where(SeciliUrunGrubundaMi)
        .Where(urun => _secilenListeAltKategori == "tum" || UrunListeAltKategoriAdi(urun) == _secilenListeAltKategori)
        .Where(urun => string.IsNullOrWhiteSpace(_urunAramaMetni) ||
            urun.Kod.Contains(_urunAramaMetni, StringComparison.OrdinalIgnoreCase) ||
            urun.Ad.Contains(_urunAramaMetni, StringComparison.OrdinalIgnoreCase))
        .ToList();

    private IEnumerable<string> FiltrelenebilirAltKategoriler => _urunListesi
        .Where(SeciliUrunGrubundaMi)
        .Select(UrunListeAltKategoriAdi)
        .Where(ad => !string.IsNullOrWhiteSpace(ad))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(ad => ad);

    private bool SeciliUrunGrubundaMi(Urun urun)
    {
        return _secilenListeAileId == 0 || urun.UrunAilesiId == _secilenListeAileId;
    }

    private IEnumerable<Urun> SiralanmisUrunler => _urunSiralaOlcutu switch
    {
        "tarih" => _urunSiralaArtan
            ? FiltreliUrunler.OrderBy(urun => urun.OlusturulmaTarihi)
            : FiltreliUrunler.OrderByDescending(urun => urun.OlusturulmaTarihi),
        "kategori" => _urunSiralaArtan
            ? FiltreliUrunler.OrderBy(UrunKategoriAdi).ThenBy(urun => urun.Ad)
            : FiltreliUrunler.OrderByDescending(UrunKategoriAdi).ThenByDescending(urun => urun.Ad),
        _ => _urunSiralaArtan
            ? FiltreliUrunler.OrderBy(urun => urun.Ad)
            : FiltreliUrunler.OrderByDescending(urun => urun.Ad)
    };

    private int ToplamSayfa => Math.Max(1, (int)Math.Ceiling(FiltreliUrunler.Count / (double)_sayfaBoyutu));

    private IEnumerable<Urun> SayfadakiUrunler => SiralanmisUrunler
        .Skip((_seciliSayfa - 1) * _sayfaBoyutu)
        .Take(_sayfaBoyutu);

    private string UrunKategoriAdi(Urun urun) => urun.UrunKategoriId is int kategoriId
        ? _kategoriler.FirstOrDefault(kategori => kategori.Id == kategoriId)?.Ad ?? "-"
        : _aileler.FirstOrDefault(aile => aile.Id == urun.UrunAilesiId)?.Ad ?? "-";

    private string UrunAltKategoriAdi(Urun urun)
    {
        if (urun.UrunKategoriId is int kategoriId)
        {
            var kategoriAdi = _kategoriler.FirstOrDefault(kategori => kategori.Id == kategoriId)?.Ad;
            if (!string.IsNullOrWhiteSpace(kategoriAdi)) return kategoriAdi;
        }

        var ad = urun.Ad ?? string.Empty;
        var kod = urun.Kod ?? string.Empty;
        if (ad.Contains("Lake", StringComparison.OrdinalIgnoreCase) || kod.StartsWith("DSL", StringComparison.OrdinalIgnoreCase)) return "Lake";
        if (ad.Contains("Membran", StringComparison.OrdinalIgnoreCase) || kod.StartsWith("DSM", StringComparison.OrdinalIgnoreCase)) return "Membran";
        if (ad.Contains("Özel Seri", StringComparison.OrdinalIgnoreCase)) return "Özel Seri";
        if (kod.StartsWith("NRD", StringComparison.OrdinalIgnoreCase)) return "NRD";
        if (kod.StartsWith("LND", StringComparison.OrdinalIgnoreCase)) return "LND";
        if (kod.StartsWith("KNR", StringComparison.OrdinalIgnoreCase)) return "KNR";
        return string.Empty;
    }

    private string UrunListeAltKategoriAdi(Urun urun)
    {
        var kapakAilesiId = _aileler.FirstOrDefault(aile => aile.Slug.Equals("kapak", StringComparison.OrdinalIgnoreCase))?.Id;
        if (_secilenListeAileId != kapakAilesiId) return UrunAltKategoriAdi(urun);

        if (urun.UrunKategoriId is int kategoriId)
        {
            var kategori = _kategoriler.FirstOrDefault(k => k.Id == kategoriId);
            var ustKategori = kategori?.UstKategoriId is int ustId ? _kategoriler.FirstOrDefault(k => k.Id == ustId) : null;
            var kokKategori = ustKategori ?? kategori;

            if (kokKategori?.Slug is "lake") return "Lake";
            if (kokKategori?.Slug is "membran") return "Membran";
            if (kokKategori?.Slug is "ozel-kapaklar" or "kapak-ozel") return "Özel Kapaklar";
        }

        var altKategori = UrunAltKategoriAdi(urun);
        if (altKategori.Contains("Lake", StringComparison.OrdinalIgnoreCase)) return "Lake";
        if (altKategori.Contains("Membran", StringComparison.OrdinalIgnoreCase)) return "Membran";
        return "Özel Kapaklar";
    }

    private string ListeAileMetni(int aileId) => aileId == 0
        ? dil.T("ortak.tumu", "Tümü")
        : _aileler.FirstOrDefault(aile => aile.Id == aileId)?.Ad ?? string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            ReferansYukleAsync(), UrunListeYukleAsync()
        );
        _listeYukleniyor = false;
    }

    private async Task ReferansYukleAsync()
    {
        var a = await Api.GetAsync<List<UrunAilesi>>("api/urun-ailesi");
        if (a != null) _aileler = a;
        var k = await Api.GetAsync<List<UrunKategori>>("api/urun-kategorileri");
        if (k != null) _kategoriler = k;
    }

    private async Task UrunListeYukleAsync() => _urunListesi = await Api.GetAsync<List<Urun>>("api/urunler") ?? [];

    // ─── TAB 1 ──────────────────────────────────────────────────────────
    private async Task Tab1Kaydet()
    {
        _tab1Kaydediliyor = true; StateHasChanged();
        try
        {
            Cevap<Urun>? s;
            if (_urunId.HasValue) s = await Api.PutAsync<Urun>($"api/urunler/{_urunId.Value}", _urunForm);
            else s = await Api.PostAsync<Urun>("api/urunler", _urunForm);
            if (s?.BasariliMi == true && s.Veri != null) { _urunId = s.Veri.Id; _urunForm = s.Veri; }
            else { Snackbar.Add(s?.Mesaj ?? dil.T("ortak.kayitBasarisiz", "Kayıt işlemi tamamlanamadı."), Severity.Error); return; }
            _kaydedildi = true; _aktifSekme = 1;
            Snackbar.Add(dil.T("ortak.kaydedildi", "Kaydedildi."), Severity.Success);
            await UrunListeYukleAsync();
        }
        catch (Exception ex) { Snackbar.Add($"{dil.T("ortak.hata", "Hata")}: {ex.Message}", Severity.Error); }
        finally { _tab1Kaydediliyor = false; }
    }

    // ─── TAB 2 ──────────────────────────────────────────────────────────
    private async Task Tab2Yukle()
    {
        _tab2Yukleniyor = true;
        StateHasChanged();
        if (_urunId.HasValue)
        {
            var u = await Api.GetAsync<Urun>($"api/urunler/{_urunId.Value}");
            if (u != null)
            {
                _urunForm = u;
                _secilenMedyaId = u.AnaGorselMedyaId;
                _urunMedyalari = await Api.GetAsync<List<UrunMedya>>($"api/urunler/{_urunId.Value}/medyalar") ?? [];
            }
        }
        _tab2Yukleniyor = false;
    }

    private async Task MedyaSec()
    {
        var p = new DialogParameters { { "CokluSecim", false }, { "BaslangicTipi", MedyaTipi.Resim } };
        var opts = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge, FullWidth = true, CloseButton = true };
        var d = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Medya.MedyaSecici>(dil.T("admin.medya.sec", "Medya seç"), p, opts);
        var r = await d.Result;
        if (r is { Canceled: false, Data: List<long> ids } && ids.Any())
        {
            var oncekiMedyaId = _urunForm.AnaGorselMedyaId;
            if (_urunId.HasValue)
            {
                _urunForm.AnaGorselMedyaId = ids[0];
                var cevap = await Api.PutAsync<Urun>($"api/urunler/{_urunId.Value}", _urunForm);
                if (cevap?.BasariliMi != true)
                {
                    _urunForm.AnaGorselMedyaId = oncekiMedyaId;
                    _secilenMedyaId = oncekiMedyaId;
                    Snackbar.Add(
                        cevap?.Mesaj ?? dil.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."),
                        Severity.Error);
                    return;
                }
            }

            _secilenMedyaId = ids[0];
            Snackbar.Add(dil.T("admin.urun.anaGorselSecildi", "Ana görsel seçildi."), Severity.Success);
            await Tab2Yukle();
        }
    }

    private async Task GaleriMedyaSec()
    {
        if (!_urunId.HasValue)
            return;

        var mevcutIdler = _urunMedyalari
            .Select(m => UrunMedyaIdCikar(m.MedyaUrl))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        var parametreler = new DialogParameters
        {
            { "CokluSecim", true },
            { "MevcutSeciliIdler", mevcutIdler },
            { "BaslangicTipi", MedyaTipi.Resim }
        };
        var secenekler = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge, FullWidth = true, CloseButton = true };
        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Medya.MedyaSecici>(dil.T("admin.medya.gorselSec", "Görsel seç"), parametreler, secenekler);
        var sonuc = await dialog.Result;
        if (sonuc is not { Canceled: false, Data: List<long> medyaIdleri } || medyaIdleri.Count == 0)
            return;

        var sira = _urunMedyalari.Count + 1;
        foreach (var medyaId in medyaIdleri.Where(medyaId => !_urunMedyalari.Any(m => UrunMedyaIdCikar(m.MedyaUrl) == medyaId)))
            await GaleriMedyaEkleAsync($"/api/medya/dosya/{medyaId}", "Resim", sira++);

        await Tab2Yukle();
    }

    private Task GaleriEkle()
    {
        // Havuz dışı URL ekleme kapatildi.
        Snackbar.Add(dil.T("admin.urun.sadeceHavuz", "Yalnızca medya havuzundan seçim yapabilirsiniz."), Severity.Info);
        return Task.CompletedTask;
    }

    private async Task TeknikCizimMedyaSec()
    {
        if (!_urunId.HasValue)
            return;

        var mevcutIdler = _urunMedyalari
            .Select(m => UrunMedyaIdCikar(m.MedyaUrl))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        var parametreler = new DialogParameters
        {
            { "CokluSecim", false },
            { "MevcutSeciliIdler", mevcutIdler },
            { "BaslangicTipi", MedyaTipi.Resim }
        };
        var secenekler = new DialogOptions { MaxWidth = MaxWidth.ExtraLarge, FullWidth = true, CloseButton = true };
        var dialog = await DialogServisi.ShowAsync<VizitLink3D.UI.Bilesenler.Medya.MedyaSecici>(dil.T("admin.medya.gorselSec", "Görsel seç"), parametreler, secenekler);
        var sonuc = await dialog.Result;
        if (sonuc is not { Canceled: false, Data: List<long> medyaIdleri } || medyaIdleri.Count == 0)
            return;

        var medyaId = medyaIdleri[0];
        await GaleriMedyaEkleAsync($"/api/medya/dosya/{medyaId}", "TeknikCizim", _urunMedyalari.Count + 1);
        await Tab2Yukle();
    }

    private async Task GaleriMedyaEkleAsync(string medyaUrl, string medyaTuru, int siraNo)
    {
        if (!_urunId.HasValue)
            return;

        var cevap = await Api.PostAsync<UrunMedya>($"api/urunler/{_urunId.Value}/medya", new
        {
            MedyaUrl = medyaUrl,
            MedyaTuru = medyaTuru,
            SiraNo = siraNo
        });

        if (cevap?.BasariliMi == true)
            Snackbar.Add(dil.T("admin.urun.galeriyeEklendi", "Galeriye eklendi."), Severity.Success);
        else
            Snackbar.Add(cevap?.Mesaj ?? dil.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."), Severity.Error);
    }

    private async Task MedyaSirasiniDegistir(UrunMedya medya, int fark)
    {
        if (!_urunId.HasValue)
            return;

        var cevap = await Api.PutAsync<UrunMedya>($"api/urunler/{_urunId.Value}/medyalar/{medya.Id}/sira", new UrunMedya
        {
            SiraNo = Math.Max(1, medya.SiraNo + fark)
        });

        Snackbar.Add(
            cevap?.BasariliMi == true
                ? dil.T("admin.urun.gorselSirasiGuncellendi", "Görsel sırası güncellendi.")
                : cevap?.Mesaj ?? dil.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        await Tab2Yukle();
    }

    private async Task AnaGorselYap(UrunMedya medya)
    {
        if (!_urunId.HasValue)
            return;

        var medyaId = UrunMedyaIdCikar(medya.MedyaUrl);
        if (!medyaId.HasValue)
        {
            Snackbar.Add(dil.T("admin.urun.anaGorselHavuzGerekli", "Ana görsel için medya havuzundaki bir görsel seçin."), Severity.Warning);
            return;
        }

        _urunForm.AnaGorselMedyaId = medyaId.Value;
        var cevap = await Api.PutAsync<Urun>($"api/urunler/{_urunId.Value}", _urunForm);
        Snackbar.Add(
            cevap?.BasariliMi == true
                ? dil.T("admin.urun.anaGorselSecildi", "Ana görsel seçildi.")
                : cevap?.Mesaj ?? dil.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        await Tab2Yukle();
    }

    private async Task MedyaBaglantisiniKaldirOnay(UrunMedya medya)
    {
        if (!_urunId.HasValue)
            return;

        var onay = await DialogServisi.ShowMessageBoxAsync(
            dil.T("admin.urun.gorselKaldirma", "Görseli galeriden kaldır"),
            dil.T("admin.urun.gorselKaldirmaAciklama", "Görsel bu ürüne bağlı olmaktan çıkarılacak. Devam edilsin mi?"),
            yesText: dil.T("ortak.kaldir", "Kaldır"),
            cancelText: dil.T("ortak.iptal", "İptal"));

        if (onay != true)
            return;

        var cevap = await Api.DeleteAsync($"api/urunler/{_urunId.Value}/medyalar/{medya.Id}");
        Snackbar.Add(
            cevap?.BasariliMi == true
                ? dil.T("admin.urun.galeriGorselKaldirildi", "Görsel galeriden kaldırıldı.")
                : cevap?.Mesaj ?? dil.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."),
            cevap?.BasariliMi == true ? Severity.Success : Severity.Error);
        await Tab2Yukle();
    }

    private static long? UrunMedyaIdCikar(string medyaUrl)
    {
        if (string.IsNullOrWhiteSpace(medyaUrl))
            return null;

        var eslesme = MedyaIdRegex.Match(medyaUrl);
        return eslesme.Success && long.TryParse(eslesme.Groups[1].Value, out var id) ? id : null;
    }

    // ─── TAB 3 ──────────────────────────────────────────────────────────
    private string UrunMedyaGorselUrl(UrunMedya medya)
    {
        if (string.IsNullOrWhiteSpace(medya.MedyaUrl))
            return "/medya/vizitlink3d_default.png";

        return medya.MedyaUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? medya.MedyaUrl
            : $"{Api.ApiBaseUrl}{(medya.MedyaUrl.StartsWith("/") ? medya.MedyaUrl : "/" + medya.MedyaUrl)}";
    }

    private async Task Tab3Yukle()
    {
        _tab3Yukleniyor = true; StateHasChanged();
        if (_urunId.HasValue)
        {
            _modeller = await Api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{_urunId.Value}/uc-boyut-modelleri") ?? [];
            ModelSeciminiDengele();
            await SeciliModelParcalariniYukle();
        }
        _tab3Yukleniyor = false;
    }

    private void ModelSeciminiDengele()
    {
        if (_seciliModelId.HasValue && _modeller.Any(m => m.Id == _seciliModelId.Value))
        {
            return;
        }

        _seciliModelId = _modeller
            .OrderByDescending(m => m.VarsayilanMi)
            .ThenBy(m => m.OlusturulmaTarihi)
            .Select(m => (int?)m.Id)
            .FirstOrDefault();
    }

    private async Task SeciliModelParcalariniYukle()
    {
        _parcalar = _seciliModelId.HasValue
            ? await Api.GetAsync<List<UrunUcBoyutParcasi>>($"api/uc-boyut/modeller/{_seciliModelId.Value}/parcalar") ?? []
            : [];
    }

    private async Task ModelSecildi(int? modelId)
    {
        _seciliModelId = modelId;
        _tab3FormAcik = false;
        await SeciliModelParcalariniYukle();
    }

    private async Task GlbYukle(IBrowserFile f)
    {
        if (f == null || !_urunId.HasValue) return;
        using var c = new MultipartFormDataContent();
        c.Add(new StreamContent(f.OpenReadStream(50 * 1024 * 1024)), "dosya", f.Name);
        c.Add(new StringContent(_urunId.Value.ToString()), "urunId");
        c.Add(new StringContent(f.Name), "modelAdi");
        c.Add(new StringContent("Glb"), "modelTipi");
        var s = await Api.PostMultipartAsync<UrunUcBoyutModeli>("api/uc-boyut/modeller/yukle", c);
        if (s?.BasariliMi == true && s.Veri != null)
        {
            _seciliModelId = s.Veri.Id;
            Snackbar.Add(dil.T("admin.urun.ucBoyutModelYuklendi", "3D model yüklendi."), Severity.Success);
            await Tab3Yukle();
        }
        else Snackbar.Add(s?.Mesaj ?? dil.T("ortak.yuklemeBasarisiz", "Yükleme başarısız."), Severity.Error);
    }
    private void GlbDosyaSecildi(IBrowserFile f) => _ = GlbYukle(f);

    private void YeniParca()
    {
        if (!_seciliModelId.HasValue)
        {
            Snackbar.Add(dil.T("admin.urun.once3dModelSecin", "Önce bir 3D model seçin veya yükleyin."), Severity.Warning);
            return;
        }

        _parcaForm = new UrunUcBoyutParcasi
        {
            UrunUcBoyutModeliId = _seciliModelId.Value,
            AktifMi = true,
            AdminOnayliMi = true,
            SecilebilirMi = true,
            RenklenebilirMi = true,
            MalzemeDegisebilirMi = true
        };
        _tab3FormAcik = true;
    }

    private void ParcaDuzenle(UrunUcBoyutParcasi p)
    {
        _parcaForm = new UrunUcBoyutParcasi
        {
            Id = p.Id,
            UrunUcBoyutModeliId = p.UrunUcBoyutModeliId,
            GorunenAd = p.GorunenAd,
            MeshAdi = p.MeshAdi,
            ParcaTipi = p.ParcaTipi,
            MalzemeTipiKisiti = p.MalzemeTipiKisiti,
            AdminOnayliMi = p.AdminOnayliMi,
            SecilebilirMi = p.SecilebilirMi,
            RenklenebilirMi = p.RenklenebilirMi,
            MalzemeDegisebilirMi = p.MalzemeDegisebilirMi,
            HareketliMi = p.HareketliMi,
            GizlenebilirMi = p.GizlenebilirMi,
            SiraNo = p.SiraNo,
            AktifMi = p.AktifMi
        };
        _tab3FormAcik = true;
    }

    private async Task ParcaKaydet()
    {
        if (!_seciliModelId.HasValue)
        {
            Snackbar.Add(dil.T("admin.urun.parcaKaydet3dZorunlu", "Parça kaydetmek için önce 3D model seçin."), Severity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_parcaForm.GorunenAd) || string.IsNullOrWhiteSpace(_parcaForm.MeshAdi))
        {
            Snackbar.Add(dil.T("admin.urun.parcaAdMeshZorunlu", "Görünen ad ve mesh adı zorunludur."), Severity.Warning);
            return;
        }

        _tab3Kaydediliyor = true;
        _parcaForm.UrunUcBoyutModeliId = _seciliModelId.Value;

        Cevap<UrunUcBoyutParcasi>? cevap;
        if (_parcaForm.Id > 0)
        {
            cevap = await Api.PutAsync<UrunUcBoyutParcasi>($"api/uc-boyut/modeller/parcalar/{_parcaForm.Id}", _parcaForm);
        }
        else
        {
            cevap = await Api.PostAsync<UrunUcBoyutParcasi>($"api/uc-boyut/modeller/{_seciliModelId.Value}/parcalar", _parcaForm);
        }

        _tab3Kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            _tab3FormAcik = false;
            Snackbar.Add(dil.T("admin.urun.parcaKaydedildi", "Parça kaydedildi."), Severity.Success);
            await SeciliModelParcalariniYukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.parcaKaydedilemedi", "Parça kaydedilemedi."), Severity.Error);
    }

    private async Task ParcaSil(UrunUcBoyutParcasi p)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            dil.T("ortak.onay", "Onay"),
            string.Format(dil.T("admin.urun.parcaSilOnay", "'{0}' parçası yayından kaldırılacak. Emin misiniz?"), p.GorunenAd),
            yesText: dil.T("ortak.yayindanKaldir", "Yayından kaldır"),
            cancelText: dil.T("ortak.iptal", "İptal"));

        if (onay != true)
        {
            return;
        }

        var cevap = await Api.DeleteAsync($"api/uc-boyut/modeller/parcalar/{p.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(dil.T("admin.urun.parcaYayindanKaldirildi", "Parça yayından kaldırıldı."), Severity.Success);
            await SeciliModelParcalariniYukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.parcaYayindanKaldirilamadi", "Parça yayından kaldırılamadı."), Severity.Error);
    }

    // ─── TAB 4 ──────────────────────────────────────────────────────────
    private async Task Tab4Yukle()
    {
        _tab4Yukleniyor = true; StateHasChanged();
        _ralListesi = await Api.GetAsync<List<RalRengi>>("api/renkler/ral") ?? [];
        _malzemeListesi = await Api.GetAsync<List<Malzeme>>("api/malzemeler") ?? [];
        if (_urunId.HasValue)
        {
            _modeller = await Api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{_urunId.Value}/uc-boyut-modelleri") ?? [];
            ModelSeciminiDengele();
            _tab4Parcalar = _seciliModelId.HasValue
                ? await Api.GetAsync<List<UrunUcBoyutParcasi>>($"api/uc-boyut/modeller/{_seciliModelId.Value}/parcalar") ?? []
                : [];
        }
        _tab4Yukleniyor = false;
    }

    private async Task ParcaSecildi(int? pid)
    {
        _seciliParcaId = pid; _seciliRenkId = null; _seciliMalzemeId = null; _seciliKaplamaId = null;
        _parcaRenkleri = []; _parcaMalzemeleri = []; _kaplamalar = [];
        if (pid.HasValue)
        {
            _parcaRenkleri = await Api.GetAsync<List<UrunParcaRenkSecenegi>>($"api/uc-boyut/parcalar/{pid}/renk-secenekleri") ?? [];
            _parcaMalzemeleri = await Api.GetAsync<List<UrunParcaMalzemeSecenegi>>($"api/uc-boyut/parcalar/{pid}/malzeme-secenekleri") ?? [];
        }
    }

    private async Task MalzemeSecildi(int? mid) { _seciliMalzemeId = mid; _seciliKaplamaId = null; _kaplamalar = mid.HasValue ? await Api.GetAsync<List<KaplamaSecenegi>>($"api/malzemeler/{mid}/kaplamalar") ?? [] : []; }

    private async Task RenkEkle()
    {
        if (!_seciliParcaId.HasValue || !_seciliRenkId.HasValue) return;
        var cevap = await Api.PostAsync<UrunParcaRenkSecenegi>($"api/uc-boyut/parcalar/{_seciliParcaId}/renk-secenekleri", new { RalRengiId = _seciliRenkId.Value, AktifMi = true });
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(dil.T("admin.urun.renkEklendi", "Renk eklendi."), Severity.Success);
            await ParcaSecildi(_seciliParcaId);
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.renkEklenemedi", "Renk eklenemedi."), Severity.Error);
    }

    private async Task MalzemeEkle()
    {
        if (!_seciliParcaId.HasValue || !_seciliMalzemeId.HasValue) return;
        var cevap = await Api.PostAsync<UrunParcaMalzemeSecenegi>($"api/uc-boyut/parcalar/{_seciliParcaId}/malzeme-secenekleri", new { MalzemeId = _seciliMalzemeId.Value, KaplamaSecenegiId = _seciliKaplamaId, AktifMi = true });
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(dil.T("admin.urun.malzemeEklendi", "Malzeme eklendi."), Severity.Success);
            await ParcaSecildi(_seciliParcaId);
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.malzemeEklenemedi", "Malzeme eklenemedi."), Severity.Error);
    }

    private async Task RenkSil(int id)
    {
        if (!_seciliParcaId.HasValue) return;
        var cevap = await Api.DeleteAsync($"api/uc-boyut/parcalar/{_seciliParcaId}/renk-secenekleri/{id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(dil.T("admin.urun.renkKaldirildi", "Renk kaldırıldı."), Severity.Success);
            await ParcaSecildi(_seciliParcaId);
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.renkKaldirilamadi", "Renk kaldırılamadı."), Severity.Error);
    }

    private async Task MalzemeSil(int id)
    {
        if (!_seciliParcaId.HasValue) return;
        var cevap = await Api.DeleteAsync($"api/uc-boyut/parcalar/{_seciliParcaId}/malzeme-secenekleri/{id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(dil.T("admin.urun.malzemeKaldirildi", "Malzeme kaldırıldı."), Severity.Success);
            await ParcaSecildi(_seciliParcaId);
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.malzemeKaldirilamadi", "Malzeme kaldırılamadı."), Severity.Error);
    }

    private async Task Tamamlandi()
    {
        Snackbar.Add(dil.T("admin.urun.kaydiTamamlandi", "Ürün kaydı tamamlandı."), Severity.Success);
        await UrunListeYukleAsync();
        Navigation.NavigateTo("/admin/urun-yonetimi");
    }

    // ─── TAB 5 ──────────────────────────────────────────────────────────
    private async Task Tab5Yukle()
    {
        _tab5Yukleniyor = true; StateHasChanged();
        if (_urunId.HasValue)
        {
            _sablon = await Api.GetAsync<UrunKonfigurasyonSablonu>($"api/urunler/{_urunId.Value}/konfigurasyon-sablonu");
            if (_sablon != null) _sablonForm = new UrunKonfigurasyonSablonu { Id = _sablon.Id, UrunId = _sablon.UrunId, DetaySablonu = _sablon.DetaySablonu, HeroAktifMi = _sablon.HeroAktifMi, TeknikOzellikAktifMi = _sablon.TeknikOzellikAktifMi, PdfKaynakAktifMi = _sablon.PdfKaynakAktifMi, BenzerUrunlerAktifMi = _sablon.BenzerUrunlerAktifMi, TeklifFormuAktifMi = _sablon.TeklifFormuAktifMi, UcBoyutIlkAcilacakMi = _sablon.UcBoyutIlkAcilacakMi, AnimasyonTipi = _sablon.AnimasyonTipi, MobilPanelDavranisi = _sablon.MobilPanelDavranisi, RenkPaneliKonumu = _sablon.RenkPaneliKonumu, AktifMi = _sablon.AktifMi };
            _kurallar = await Api.GetAsync<List<UrunKonfigurasyonKurali>>($"api/urunler/{_urunId.Value}/konfigurasyon-kurallari") ?? [];
            _modeller = await Api.GetAsync<List<UrunUcBoyutModeli>>($"api/urunler/{_urunId.Value}/uc-boyut-modelleri") ?? [];
            ModelSeciminiDengele();
            await SeciliModelParcalariniYukle();
        }
        _tab5Yukleniyor = false;
    }

    private async Task SablonKaydet()
    {
        _tab5SablonKaydediliyor = true;
        try
        {
            _sablonForm.UrunId = _urunId ?? 0;
            Cevap<UrunKonfigurasyonSablonu>? cevap;
            if (_sablonForm.Id > 0) cevap = await Api.PutAsync<UrunKonfigurasyonSablonu>($"api/urunler/konfigurasyon-sablonu/{_sablonForm.Id}", _sablonForm);
            else cevap = await Api.PostAsync<UrunKonfigurasyonSablonu>("api/urunler/konfigurasyon-sablonu", _sablonForm);

            if (cevap?.BasariliMi == true)
            {
                Snackbar.Add(dil.T("admin.urun.sablonKaydedildi", "Şablon kaydedildi."), Severity.Success);
                await Tab5Yukle();
            }
            else
            {
                Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.sablonKaydedilemedi", "Şablon kaydedilemedi."), Severity.Error);
            }
        }
        catch (Exception ex) { Snackbar.Add(ex.Message, Severity.Error); }
        finally { _tab5SablonKaydediliyor = false; }
    }

    private void YeniKural() { _kuralForm = new UrunKonfigurasyonKurali { UrunId = _urunId ?? 0, KuralTipi = "Yasak", AktifMi = true }; _duzenlenenKuralId = null; _kuralFormAcik = true; }
    private void KuralDuzenle(UrunKonfigurasyonKurali k) { _kuralForm = new UrunKonfigurasyonKurali { Id = k.Id, UrunId = k.UrunId, Parca1Id = k.Parca1Id, Parca2Id = k.Parca2Id, Parca1RenkId = k.Parca1RenkId, Parca2RenkId = k.Parca2RenkId, Parca1MalzemeId = k.Parca1MalzemeId, Parca2MalzemeId = k.Parca2MalzemeId, KuralTipi = k.KuralTipi, AktifMi = k.AktifMi }; _duzenlenenKuralId = k.Id; _kuralFormAcik = true; }

    private async Task KuralKaydet()
    {
        _kuralKaydediliyor = true;
        try
        {
            _kuralForm.UrunId = _urunId ?? 0;
            Cevap<UrunKonfigurasyonKurali>? cevap;
            if (_duzenlenenKuralId.HasValue) cevap = await Api.PutAsync<UrunKonfigurasyonKurali>($"api/urunler/{_urunId}/konfigurasyon-kurallari/{_duzenlenenKuralId.Value}", _kuralForm);
            else cevap = await Api.PostAsync<UrunKonfigurasyonKurali>($"api/urunler/{_urunId}/konfigurasyon-kurallari", _kuralForm);

            if (cevap?.BasariliMi == true)
            {
                _kuralFormAcik = false;
                Snackbar.Add(dil.T("admin.urun.kuralKaydedildi", "Kural kaydedildi."), Severity.Success);
                await Tab5Yukle();
            }
            else
            {
                Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.kuralKaydedilemedi", "Kural kaydedilemedi."), Severity.Error);
            }
        }
        catch (Exception ex) { Snackbar.Add(ex.Message, Severity.Error); }
        finally { _kuralKaydediliyor = false; }
    }

    private async Task KuralSil(UrunKonfigurasyonKurali k)
    {
        var cevap = await Api.DeleteAsync($"api/urunler/{_urunId}/konfigurasyon-kurallari/{k.Id}");
        if (cevap?.BasariliMi == true)
        {
            Snackbar.Add(dil.T("admin.urun.kuralYayindanKaldirildi", "Kural yayından kaldırıldı."), Severity.Success);
            await Tab5Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("admin.urun.kuralYayindanKaldirilamadi", "Kural yayından kaldırılamadı."), Severity.Error);
    }
    private void KuralTipiSec(string tip) { _kuralForm.KuralTipi = tip; }

    // ─── TAB 6 ──────────────────────────────────────────────────────────
    private async Task Tab6Yukle()
    {
        if (_urunId.HasValue) { var u = await Api.GetAsync<Urun>($"api/urunler/{_urunId.Value}"); if (u != null) _urunForm = u; }
    }

    // ─── TAB 7 ──────────────────────────────────────────────────────────
    private async Task OtomatikCevirVeKaydet()
    {
        if (!_urunId.HasValue || string.IsNullOrEmpty(_secilenHedefDil)) return;

        _ceviriYapiliyor = true;
        _sonCeviriler.Clear();
        StateHasChanged();

        try
        {
            var cevrilecekMetinler = new Dictionary<string, string?>
            {
                { $"Urun_{_urunId.Value}_Ad", _urunForm.Ad },
                { $"Urun_{_urunId.Value}_KisaAciklama", _urunForm.KisaAciklama },
                { $"Urun_{_urunId.Value}_Aciklama", _urunForm.Aciklama }
            };

            foreach (var kvp in cevrilecekMetinler)
            {
                if (string.IsNullOrWhiteSpace(kvp.Value)) continue;

                var istek = new OtomatikCeviriIstegi 
                { 
                    Metin = kvp.Value, 
                    KaynakDil = "tr", 
                    HedefDil = _secilenHedefDil 
                };

                var ceviriYanit = await Api.PostAsync<string>("api/yonetim/ceviri/cevir", istek);
                if (ceviriYanit?.BasariliMi == true && !string.IsNullOrEmpty(ceviriYanit.Veri))
                {
                    _sonCeviriler[kvp.Key] = ceviriYanit.Veri;
                    
                    // Ceviri tablosuna kaydetmek icin CeviriKontrolcu icindeki endpoint cagrilabilir
                    await Api.PostAsync<object>("api/yonetim/ceviri/kaydet", new VizitLink3D.Ortak.Modeller.Ceviriler.CeviriKayitIstegi 
                    { 
                        Anahtar = kvp.Key, 
                        Dil = _secilenHedefDil, 
                        Deger = ceviriYanit.Veri 
                    });
                }
            }

            Snackbar.Add(string.Format(dil.T("admin.ceviri.dilTamamlandi", "{0} dili için çeviriler tamamlandı."), _secilenHedefDil.ToUpper()), Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{dil.T("ortak.hata", "Hata")}: {ex.Message}", Severity.Error);
        }
        finally
        {
            _ceviriYapiliyor = false;
        }
    }

    // ─── LISTEDEN DUZENLE ───────────────────────────────────────────────
    private async Task Duzenle(int id)
    {
        _kaydedildi = true; _aktifSekme = 1;
        _urunId = id; var u = await Api.GetAsync<Urun>($"api/urunler/{id}"); if (u != null) _urunForm = u;
    }

    private void UrunAramaYap(KeyboardEventArgs _)
    {
        _seciliSayfa = 1;
        StateHasChanged();
    }

    private Task SiralamayiDegistir(string olcut)
    {
        _urunSiralaOlcutu = olcut;
        _seciliSayfa = 1;
        return Task.CompletedTask;
    }

    private Task AltKategoriFiltresiniDegistir(string kategori)
    {
        _secilenListeAltKategori = kategori;
        _seciliSayfa = 1;
        return Task.CompletedTask;
    }

    private Task AileFiltresiniDegistir(int aileId)
    {
        _secilenListeAileId = aileId;
        _secilenListeAltKategori = "tum";
        _seciliSayfa = 1;
        return Task.CompletedTask;
    }

    private void SiralamaYonunuDegistir()
    {
        _urunSiralaArtan = !_urunSiralaArtan;
        _seciliSayfa = 1;
    }

    private Task SayfaBoyutunuDegistir(int sayfaBoyutu)
    {
        _sayfaBoyutu = sayfaBoyutu;
        _seciliSayfa = 1;
        return Task.CompletedTask;
    }

    private Task SayfaDegistir(int sayfa)
    {
        _seciliSayfa = sayfa;
        return Task.CompletedTask;
    }

    private string UrunGorselUrl(Urun urun)
    {
        if (urun.AnaGorselMedyaId is long medyaId && medyaId > 0)
            return $"{Api.ApiBaseUrl}/api/medya/dosya/{medyaId}";
        return "/medya/vizitlink3d_default.png";
    }

    private async Task DuzenleVeGorselleriAc(int id)
    {
        await Duzenle(id);
        await Sekme2();
        await InvokeAsync(StateHasChanged);
        await js.InvokeVoidAsync("window.scrollTo", new { top = 0, behavior = "smooth" });
    }
}
