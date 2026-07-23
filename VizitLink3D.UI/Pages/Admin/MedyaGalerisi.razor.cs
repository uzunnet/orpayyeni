using VizitLink3D.Ortak.Modeller;
using VizitLink3D.UI.Models;
using VizitLink3D.UI.Servisler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace VizitLink3D.UI.Pages.Admin;

public partial class MedyaGalerisi : ComponentBase
{
    private const long MAKSIMUM_GALERI_DOSYA_BOYUTU = 20 * 1024 * 1024;

    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private List<GaleriGorseliDto> _gorseller = [];
    private List<GaleriGorseliDto> _filtreliGorseller = [];
    private List<GaleriGorseliDto> _seciliGorseller = [];
    private IReadOnlyList<IBrowserFile> _yuklenecekDosyalar = [];
    private GaleriGorseliDto _duzenlenenGorsel = new();
    private string _aramaMetni = string.Empty;
    private bool _yukleniyor = true;
    private bool _kaydediliyor;
    private bool _yuklemeDialogAcik;
    private bool _duzenlemeDialogAcik;
    private int _yuklemeYuzdesi;

    protected override async Task OnInitializedAsync() => await Yukle();

    private async Task Yukle()
    {
        _yukleniyor = true;
        _gorseller = await Api.GetAsync<List<GaleriGorseliDto>>("api/galeri-gorselleri/detay") ?? [];
        _seciliGorseller = _seciliGorseller.Where(secili => _gorseller.Any(g => g.Id == secili.Id)).ToList();
        Filtrele();
        _yukleniyor = false;
    }

    private void AramaDegisti(string deger)
    {
        _aramaMetni = deger;
        Filtrele();
    }

    private void Filtrele()
    {
        var arama = _aramaMetni.Trim();
        var sorgu = _gorseller.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            sorgu = sorgu.Where(g =>
                GorselBasligi(g).Contains(arama, StringComparison.OrdinalIgnoreCase) ||
                GorselAltMetni(g).Contains(arama, StringComparison.OrdinalIgnoreCase) ||
                g.Url.Contains(arama, StringComparison.OrdinalIgnoreCase));
        }

        _filtreliGorseller = sorgu
            .OrderBy(g => g.Sira)
            .ThenByDescending(g => g.OlusturulmaTarihi)
            .ToList();
    }

    private void YeniYuklemeAc()
    {
        _yuklenecekDosyalar = [];
        _yuklemeYuzdesi = 0;
        _yuklemeDialogAcik = true;
    }

    private void DosyalarSecildi(InputFileChangeEventArgs e)
    {
        _yuklenecekDosyalar = e.GetMultipleFiles(50);
    }

    private async Task YuklemeyiBaslat()
    {
        if (_yuklenecekDosyalar.Count == 0)
            return;

        _kaydediliyor = true;
        var sira = 0;

        foreach (var dosya in _yuklenecekDosyalar)
        {
            sira++;
            using var icerik = new MultipartFormDataContent();
            await using var stream = dosya.OpenReadStream(MAKSIMUM_GALERI_DOSYA_BOYUTU);
            using var dosyaIcerigi = new StreamContent(stream);
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

            _yuklemeYuzdesi = Math.Max(10, sira * 100 / _yuklenecekDosyalar.Count);
            await Api.PostMultipartAsync<GaleriGorseliDto>("api/galeri-gorselleri/yukle", icerik);
        }

        _kaydediliyor = false;
        _yuklemeDialogAcik = false;
        _yuklemeYuzdesi = 0;
        Snackbar.Add(dil.T("admin.galeri.yuklemeTamam", "Galeri görselleri yüklendi."), Severity.Success);
        await Yukle();
    }

    private void YuklemeIptal()
    {
        _yuklenecekDosyalar = [];
        _yuklemeDialogAcik = false;
        _yuklemeYuzdesi = 0;
    }

    private void DuzenleAc(GaleriGorseliDto gorsel)
    {
        _duzenlenenGorsel = new GaleriGorseliDto
        {
            Id = gorsel.Id,
            Url = gorsel.Url,
            Baslik = gorsel.Baslik,
            AltMetin = gorsel.AltMetin,
            Sira = gorsel.Sira,
            AktifMi = gorsel.AktifMi,
            OlusturulmaTarihi = gorsel.OlusturulmaTarihi
        };
        _duzenlemeDialogAcik = true;
    }

    private async Task Kaydet()
    {
        if (_duzenlenenGorsel.Id <= 0)
            return;

        _kaydediliyor = true;
        var cevap = await Api.PutAsync<GaleriGorseliDto>(
            $"api/galeri-gorselleri/{_duzenlenenGorsel.Id}",
            _duzenlenenGorsel);
        _kaydediliyor = false;

        if (cevap?.BasariliMi == true)
        {
            _duzenlemeDialogAcik = false;
            Snackbar.Add(dil.T("ortak.kayitGuncellendi", "Kayıt güncellendi."), Severity.Success);
            await Yukle();
            return;
        }

        Snackbar.Add(cevap?.Mesaj ?? dil.T("ortak.kayitBasarisiz", "Kayıt işlemi tamamlanamadı."), Severity.Error);
    }

    private void DuzenlemeIptal()
    {
        _duzenlemeDialogAcik = false;
        _duzenlenenGorsel = new GaleriGorseliDto();
    }

    private async Task SilOnay(GaleriGorseliDto gorsel)
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            dil.T("ortak.silmeOnayi", "Silme Onayı"),
            dil.T("admin.galeri.silOnay", "Seçilen galeri görseli yayından kaldırılacak. Emin misiniz?"),
            yesText: dil.T("ortak.sil", "Sil"),
            cancelText: dil.T("ortak.iptal", "İptal"));

        if (onay == true)
            await Sil(gorsel);
    }

    private async Task SeciliSilOnay()
    {
        if (_seciliGorseller.Count == 0)
            return;

        var onay = await DialogServisi.ShowMessageBoxAsync(
            dil.T("ortak.silmeOnayi", "Silme Onayı"),
            dil.T("admin.galeri.seciliSilOnay", "Seçili galeri görselleri yayından kaldırılacak. Emin misiniz?"),
            yesText: dil.T("ortak.sil", "Sil"),
            cancelText: dil.T("ortak.iptal", "İptal"));

        if (onay != true)
            return;

        foreach (var gorsel in _seciliGorseller.ToList())
            await Sil(gorsel, bildirimGoster: false);

        _seciliGorseller.Clear();
        Snackbar.Add(dil.T("bildirim.silindi", "Silindi."), Severity.Success);
        await Yukle();
    }

    private async Task Sil(GaleriGorseliDto gorsel, bool bildirimGoster = true)
    {
        var cevap = await Api.DeleteAsync($"api/galeri-gorselleri/{gorsel.Id}");

        if (cevap?.BasariliMi == true)
        {
            _gorseller.RemoveAll(g => g.Id == gorsel.Id);
            _seciliGorseller.RemoveAll(g => g.Id == gorsel.Id);
            Filtrele();

            if (bildirimGoster)
                Snackbar.Add(dil.T("ortak.kayitSilindi", "Kayıt silindi."), Severity.Success);

            return;
        }

        if (bildirimGoster)
            Snackbar.Add(cevap?.Mesaj ?? dil.T("ortak.islemBasarisiz", "İşlem tamamlanamadı."), Severity.Error);
    }

    private void SecimDegistir(GaleriGorseliDto gorsel)
    {
        var mevcut = _seciliGorseller.FirstOrDefault(g => g.Id == gorsel.Id);
        if (mevcut is null)
            _seciliGorseller.Add(gorsel);
        else
            _seciliGorseller.Remove(mevcut);
    }

    private bool SeciliMi(GaleriGorseliDto gorsel) => _seciliGorseller.Any(g => g.Id == gorsel.Id);

    private string GorselKartSinifi(GaleriGorseliDto gorsel)
        => SeciliMi(gorsel) ? "galeri-admin-kart galeri-admin-kart-secili" : "galeri-admin-kart";

    private string GorselBasligi(GaleriGorseliDto gorsel)
        => string.IsNullOrWhiteSpace(gorsel.Baslik)
            ? dil.T("admin.galeri.isimsiz", "İsimsiz görsel")
            : gorsel.Baslik;

    private string GorselAltMetni(GaleriGorseliDto gorsel)
        => string.IsNullOrWhiteSpace(gorsel.AltMetin) ? GorselBasligi(gorsel) : gorsel.AltMetin;

    private string GorselYolu(GaleriGorseliDto gorsel)
    {
        if (string.IsNullOrWhiteSpace(gorsel.Url))
            return "/medya/placeholder.png";

        return gorsel.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? gorsel.Url
            : $"{Api.ApiBaseUrl}{gorsel.Url}";
    }
}

