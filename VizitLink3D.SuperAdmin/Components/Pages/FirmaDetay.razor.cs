using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.SuperAdmin.IServisler;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Components.Pages;

[Authorize]
public partial class FirmaDetay : ComponentBase
{
    [Parameter] public int Id { get; set; }
    [Inject] private SuperAdminDbContext Vt { get; set; } = null!;
    [Inject] private IHttpClientFactory HttpFabrika { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager Gezinme { get; set; } = null!;
    [Inject] private IDialogService DialogServisi { get; set; } = null!;
    [Inject] private SuperAdminAuthStateProvider AuthState { get; set; } = null!;

    private bool _yukleniyor = true;
    private bool _kaydediliyor;
    private int _aktifSekme;
    private Firma? _firma;
    private List<Modul> _tumModuller = new();
    private Dictionary<int, bool> _firmaModulIdleri = new();

    // Lisans formu
    private List<SuperAdminLisansKaydi> _mevcutLisanslar = new();
    private string _yeniLisansTip = "Yillik";
    private DateTime? _yeniLisansBaslangic = DateTime.UtcNow;
    private DateTime? _yeniLisansBitis = DateTime.UtcNow.AddYears(1);
    private string? _yeniLisansAciklama;

    // Form alanlari icin kopya
    private string _ad = string.Empty;
    private string _unvan = string.Empty;
    private string _slug = string.Empty;
    private string? _domain;
    private string? _sektor;
    private string? _adres;
    private string? _telefon1;
    private string? _telefon2;
    private string? _eposta;
    private string? _sehir;
    private string? _ilce;
    private string? _logo;
    private string? _favicon;
    private string? _adminTema;
    private string? _siteTema;
    private string? _twitter;
    private string? _facebook;
    private string? _instagram;
    private string? _youtubeKanal;
    private string? _pinterest;
    private string? _linkedIn;
    private string? _tiktokKanal;
    private string? _paketTipi;
    private int _maxKullaniciSayisi;
    private bool _aktifMi;
    private bool _demoMu;

    protected override async Task OnInitializedAsync()
    {
        await Yukle();
        _yukleniyor = false;
    }

    private async Task Yukle()
    {
        _firma = await Vt.Firmalar.FindAsync(Id);
        if (_firma == null)
        {
            Snackbar.Add("Firma bulunamadı.", Severity.Error);
            Gezinme.NavigateTo("/firmalar");
            return;
        }

        _ad = _firma.Ad;
        _unvan = _firma.Unvan;
        _slug = _firma.Slug;
        _domain = _firma.Domain;
        _sektor = _firma.Sektor;
        _adres = _firma.Adres;
        _telefon1 = _firma.Telefon1;
        _telefon2 = _firma.Telefon2;
        _eposta = _firma.Eposta;
        _sehir = _firma.Sehir;
        _ilce = _firma.Ilce;
        _logo = _firma.Logo;
        _favicon = _firma.Favicon;
        _adminTema = _firma.AdminTema;
        _siteTema = _firma.SiteTema;
        _twitter = _firma.Twitter;
        _facebook = _firma.Facebook;
        _instagram = _firma.Instagram;
        _youtubeKanal = _firma.YoutubeKanal;
        _pinterest = _firma.Pinterest;
        _linkedIn = _firma.LinkedIn;
        _tiktokKanal = _firma.TiktokKanal;
        _paketTipi = _firma.PaketTipi;
        _maxKullaniciSayisi = _firma.MaxKullaniciSayisi;
        _aktifMi = _firma.AktifMi;
        _demoMu = _firma.DemoMu;

        _tumModuller = await Vt.Moduller.OrderBy(m => m.Kategori).ThenBy(m => m.Ad).ToListAsync();
        var firmaModulIdleri = await Vt.FirmaModulAtamalari
            .Where(fm => fm.FirmaId == Id)
            .Select(fm => fm.ModulId)
            .ToListAsync();

        _firmaModulIdleri = new Dictionary<int, bool>();
        foreach (var modul in _tumModuller)
        {
            _firmaModulIdleri[modul.Id] = firmaModulIdleri.Contains(modul.Id);
        }

        _mevcutLisanslar = await Vt.SuperAdminLisansKayitlari
            .Where(l => l.FirmaId == Id)
            .OrderByDescending(l => l.OlusturulmaTarihi)
            .ToListAsync();

        _yeniLisansTip = "Yillik";
        _yeniLisansBaslangic = DateTime.UtcNow;
        _yeniLisansBitis = DateTime.UtcNow.AddYears(1);
        _yeniLisansAciklama = null;
    }

    private async Task Kaydet()
    {
        if (_firma == null) return;
        _kaydediliyor = true;

        try
        {
            var http = HttpFabrika.CreateClient();
            http.BaseAddress = new Uri("http://localhost:5200");

            var token = AuthState.MevcutToken;
            if (!string.IsNullOrEmpty(token))
            {
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var dto = new
            {
                Ad = _ad,
                Unvan = _unvan,
                Domain = _domain,
                Eposta = _eposta,
                Telefon1 = _telefon1,
                Sektor = _sektor,
                PaketTipi = _paketTipi,
                Adres = _adres,
                Sehir = _sehir,
                Ilce = _ilce,
                MaxKullaniciSayisi = _maxKullaniciSayisi,
                AktifMi = (bool?)_aktifMi,
                DemoMu = (bool?)_demoMu
            };

            var yanit = await http.PutAsJsonAsync($"/api/super-admin/firma/{Id}", dto);

            if (yanit.IsSuccessStatusCode)
            {
                _firma.Ad = _ad;
                _firma.Unvan = _unvan;
                _firma.Domain = _domain;
                _firma.Sektor = _sektor;
                _firma.Adres = _adres;
                _firma.Telefon1 = _telefon1;
                _firma.Telefon2 = _telefon2;
                _firma.Eposta = _eposta;
                _firma.Sehir = _sehir;
                _firma.Ilce = _ilce;
                _firma.Logo = _logo;
                _firma.Favicon = _favicon;
                _firma.AdminTema = _adminTema;
                _firma.SiteTema = _siteTema;
                _firma.Twitter = _twitter;
                _firma.Facebook = _facebook;
                _firma.Instagram = _instagram;
                _firma.YoutubeKanal = _youtubeKanal;
                _firma.Pinterest = _pinterest;
                _firma.LinkedIn = _linkedIn;
                _firma.TiktokKanal = _tiktokKanal;
                _firma.PaketTipi = _paketTipi;
                _firma.MaxKullaniciSayisi = _maxKullaniciSayisi;
                _firma.AktifMi = _aktifMi;
                _firma.DemoMu = _demoMu;
                _firma.GuncellenmeTarihi = DateTime.UtcNow;

                await Vt.SaveChangesAsync();
                Snackbar.Add("Firma bilgileri kaydedildi.", Severity.Success);
            }
            else
            {
                var hata = await yanit.Content.ReadAsStringAsync();
                Snackbar.Add($"API hatası: {hata}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Kaydetme hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task LisansOlustur()
    {
        if (_firma == null) return;

        var baslangic = _yeniLisansBaslangic ?? DateTime.UtcNow;
        var bitis = _yeniLisansBitis ?? DateTime.UtcNow.AddYears(1);

        if (bitis <= baslangic)
        {
            Snackbar.Add("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.", Severity.Warning);
            return;
        }

        var lisans = new SuperAdminLisansKaydi
        {
            FirmaId = Id,
            Domain = _firma.Domain,
            Tip = _yeniLisansTip,
            BaslangicTarihi = new DateTimeOffset(baslangic, TimeSpan.Zero),
            BitisTarihi = new DateTimeOffset(bitis, TimeSpan.Zero),
            AktifMi = true,
            Aciklama = _yeniLisansAciklama,
            OlusturulmaTarihi = DateTimeOffset.UtcNow
        };

        Vt.SuperAdminLisansKayitlari.Add(lisans);
        await Vt.SaveChangesAsync();

        _mevcutLisanslar = await Vt.SuperAdminLisansKayitlari
            .Where(l => l.FirmaId == Id)
            .OrderByDescending(l => l.OlusturulmaTarihi)
            .ToListAsync();

        _yeniLisansTip = "Yillik";
        _yeniLisansBaslangic = DateTime.UtcNow;
        _yeniLisansBitis = DateTime.UtcNow.AddYears(1);
        _yeniLisansAciklama = null;

        Snackbar.Add("Yeni lisans oluşturuldu.", Severity.Success);
    }

    private async Task LisansSil(SuperAdminLisansKaydi lisans)
    {
        var sonuc = await DialogServisi.ShowMessageBoxAsync(
            "Onay",
            "Bu lisansı silmek istediğinize emin misiniz?",
            yesText: "Evet, Sil",
            cancelText: "İptal");

        if (sonuc == true)
        {
            lisans.AktifMi = false;
            lisans.GuncellenmeTarihi = DateTimeOffset.UtcNow;
            await Vt.SaveChangesAsync();

            _mevcutLisanslar = await Vt.SuperAdminLisansKayitlari
                .Where(l => l.FirmaId == Id)
                .OrderByDescending(l => l.OlusturulmaTarihi)
                .ToListAsync();

            Snackbar.Add("Lisans pasife alındı.", Severity.Success);
        }
    }
}
