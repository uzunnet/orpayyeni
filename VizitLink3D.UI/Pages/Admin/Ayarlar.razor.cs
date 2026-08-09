using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class Ayarlar : ComponentBase
{
    [Inject] private ApiIstemcisi Api { get; set; } = default!;
    [Inject] private FirmaBilgisiServisi FirmaBilgisi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private DilServisi DilServisi { get; set; } = default!;
    [Inject] private IDialogService DialogServisi { get; set; } = default!;

    private bool _yukleniyor;
    private bool _yukleniyorSayfa = true;

    private string _firmaAdi = "ORPAY";
    private string _logoUrl = "/medya/brand/orpay-logo-kare.png";
    private string _faviconUrl = "/favicon.png";
    private string LogoOnizlemeYolu => MarkaVarligiNormalizeEt(_logoUrl, "/medya/brand/orpay-logo-kare.png");
    private string FaviconOnizlemeYolu => MarkaVarligiNormalizeEt(_faviconUrl, "/favicon.png");

    private string _siteBasligi = "Firma vitrini";
    private string _aciklama = "Firma vitrini, koleksiyonlar ve proje çözümleri.";
    private string _anahtarKelimeler = "firma, koleksiyon, ürün, vitrin";
    private string _varsayilanDil = "tr";
    private string _temaModu = "koyu";

    private string _telefon = "+90 312 847 55 22";
    private string _eposta = "info@orpaym.com.tr";
    private string _adres = "Çankırı Yolu 8. km Büğdüz Mah. 24. Sok. No: 4 Akyurt / Ankara";
    private string _whatsapp = "";
    private string _enlem = "";
    private string _boylam = "";

    private string _instagram = "";
    private string _facebook = "";
    private string _youtube = "";

    // Ürün Listeleme
    private int _urunSayfaBoyutu = 12;
    private string _urunVarsayilanSiralama = "varsayilan";

    // Görsel optimizasyonu
    private int _resimMaksimumKenar = 1000;
    private int _resimKalite = 85;
    private bool _resimWebpZorunlu = true;

    protected override async Task OnInitializedAsync()
    {
        await DilServisi.BaslatAsync();
        var firma = await FirmaBilgisi.GetFirmaAsync();
        if (firma != null)
        {
            _firmaAdi = string.IsNullOrWhiteSpace(firma.Ad) ? _firmaAdi : firma.Ad;
            _logoUrl = MarkaVarligiNormalizeEt(firma.Logo, "/medya/brand/orpay-logo-kare.png");
            _faviconUrl = MarkaVarligiNormalizeEt(firma.Favicon, "/favicon.png");
            _siteBasligi = string.IsNullOrWhiteSpace(firma.Ad) ? _siteBasligi : $"{firma.Ad} - Kurumsal Site";
        }

        await AyarlariYukle();
        _yukleniyorSayfa = false;
    }

    private async Task AyarlariYukle()
    {
        try
        {
            var sozluk = await Api.GetAsync<Dictionary<string, string>>("api/sayfa-icerigi/ayarlar");
            if (sozluk != null)
            {
                _logoUrl = MarkaVarligiNormalizeEt(sozluk.GetValueOrDefault("LogoUrl", _logoUrl), "/medya/brand/orpay-logo-kare.png");
                _faviconUrl = MarkaVarligiNormalizeEt(sozluk.GetValueOrDefault("FaviconUrl", _faviconUrl), "/favicon.png");
                _siteBasligi = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("SiteBasligi", _siteBasligi), _siteBasligi);
                _aciklama = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Aciklama", _aciklama), _aciklama);
                _anahtarKelimeler = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("AnahtarKelimeler", _anahtarKelimeler), _anahtarKelimeler);
                _varsayilanDil = sozluk.GetValueOrDefault("VarsayilanDil", _varsayilanDil).ToLowerInvariant();
                _temaModu = sozluk.GetValueOrDefault("TemaModu", _temaModu).ToLowerInvariant() == "acik" ? "acik" : "koyu";
                _telefon = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Telefon1", _telefon), _telefon);
                _eposta = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Eposta", _eposta), _eposta);
                _adres = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Adres", _adres), _adres);
                _whatsapp = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Whatsapp", _whatsapp), _whatsapp);
                _enlem = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Enlem", _enlem), _enlem);
                _boylam = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Boylam", _boylam), _boylam);
                _instagram = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Instagram", sozluk.GetValueOrDefault("InstagramUrl", _instagram)), _instagram);
                _facebook = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Facebook", sozluk.GetValueOrDefault("FacebookUrl", _facebook)), _facebook);
                _youtube = MarkaMetniNormalizeEt(sozluk.GetValueOrDefault("Youtube", sozluk.GetValueOrDefault("YoutubeUrl", _youtube)), _youtube);
                
                if (int.TryParse(sozluk.GetValueOrDefault("UrunSayfaBoyutu", "12"), out var urunSayfaBoyutu))
                    _urunSayfaBoyutu = urunSayfaBoyutu;
                
                _urunVarsayilanSiralama = sozluk.GetValueOrDefault("UrunVarsayilanSiralama", "varsayilan");
            }

            await ResimAyarlariYukle();
        }
        catch { /* ayarlar yüklenemezse varsayılan değerler kullanılır */ }
    }

    private async Task ResimAyarlariYukle()
    {
        try
        {
            var dto = await Api.GetAsync<VizitLink3D.Ortak.Modeller.Ayarlar.ResimOptimizasyonuAyarDto>("api/ayarlar/resim-optimizasyonu");
            if (dto != null)
            {
                _resimMaksimumKenar = dto.MaksimumKenar;
                _resimKalite = dto.Kalite;
                _resimWebpZorunlu = dto.WebpZorunlu;
            }
        }
        catch { /* varsayılan değerler kalır */ }
    }

    private async Task ResimAyarlariKaydet()
    {
        _yukleniyor = true;
        try
        {
            var dto = new VizitLink3D.Ortak.Modeller.Ayarlar.ResimOptimizasyonuAyarDto
            {
                MaksimumKenar = _resimMaksimumKenar,
                Kalite = _resimKalite,
                WebpZorunlu = _resimWebpZorunlu
            };

            var yanit = await Api.PostAsync<VizitLink3D.Ortak.Modeller.Ayarlar.ResimOptimizasyonuAyarDto>(
                "api/ayarlar/resim-optimizasyonu", dto);

            if (yanit?.BasariliMi == true)
                Snackbar.Add(DilServisi.T("ayar.resimOptimizasyonu.kaydedildi", "Görsel optimizasyonu ayarları kaydedildi."), Severity.Success);
            else
                Snackbar.Add(yanit?.Mesaj ?? DilServisi.T("admin.ayarlar.hataOlustu", "Ayarlar kaydedilirken hata oluştu."), Severity.Error);
        }
        catch
        {
            Snackbar.Add(DilServisi.T("admin.ayarlar.hataOlustu", "Ayarlar kaydedilirken hata oluştu."), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task TopluYenidenBoyutlandir()
    {
        var onay = await DialogServisi.ShowMessageBoxAsync(
            DilServisi.T("ayar.resimOptimizasyonu.topluOnayBaslik", "Toplu Yeniden Boyutlandırma"),
            DilServisi.T("ayar.resimOptimizasyonu.topluOnayMesaj", "Bu işlem wwwroot/medya altındaki tüm görselleri yeniden boyutlandıracak ve WebP formatına dönüştürecektir. Orijinal dosyalar yedeklenecektir.\n\nDevam etmek istiyor musunuz?"),
            yesText: DilServisi.T("ortak.evet", "Evet"),
            cancelText: DilServisi.T("ortak.iptal", "İptal"));

        if (onay != true) return;

        _yukleniyor = true;
        try
        {
            var yanit = await Api.PostAsync<VizitLink3D.Ortak.Modeller.Ayarlar.MedyaTopluIslemSonucDto>(
                "api/medya/toplu-yeniden-boyutlandir", new {});

            if (yanit?.BasariliMi == true && yanit.Veri != null)
            {
                var s = yanit.Veri;
                Snackbar.Add(string.Format(
                    DilServisi.T("ayar.resimOptimizasyonu.topluSonuc", "Tamamlandı: {0} işlendi, {1} atlandı, {2} hata. Boyut: {3} → {4}"),
                    s.Islenen, s.Atlanan, s.Hata,
                    DosyaBoyutuFormat(s.EskiToplamBoyut),
                    DosyaBoyutuFormat(s.YeniToplamBoyut)), Severity.Success);
            }
            else
            {
                Snackbar.Add(yanit?.Mesaj ?? DilServisi.T("ayar.resimOptimizasyonu.topluHata", "Toplu işlem sırasında hata oluştu."), Severity.Error);
            }
        }
        catch
        {
            Snackbar.Add(DilServisi.T("ayar.resimOptimizasyonu.topluHata", "Toplu işlem sırasında hata oluştu."), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private static string DosyaBoyutuFormat(long byteSayisi) => byteSayisi switch
    {
        < 1024 => $"{byteSayisi} B",
        < 1024 * 1024 => $"{byteSayisi / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{byteSayisi / (1024.0 * 1024):F1} MB",
        _ => $"{byteSayisi / (1024.0 * 1024 * 1024):F2} GB"
    };

    private async Task Kaydet(int bolum)
    {
        _yukleniyor = true;
        try
        {
            var ayarlar = new Dictionary<string, string>();

            if (bolum == 4)
            {
                ayarlar["LogoUrl"] = _logoUrl;
                ayarlar["FaviconUrl"] = _faviconUrl;
            }
            else if (bolum == 1)
            {
                ayarlar["SiteBasligi"] = _siteBasligi;
                ayarlar["Aciklama"] = _aciklama;
                ayarlar["AnahtarKelimeler"] = _anahtarKelimeler;
            }
            else if (bolum == 5)
            {
                ayarlar["VarsayilanDil"] = _varsayilanDil;
                ayarlar["TemaModu"] = _temaModu;
            }
            else if (bolum == 2)
            {
                ayarlar["Telefon1"] = _telefon;
                ayarlar["Eposta"] = _eposta;
                ayarlar["Adres"] = _adres;
                ayarlar["Whatsapp"] = _whatsapp;
                ayarlar["Enlem"] = _enlem;
                ayarlar["Boylam"] = _boylam;
            }
            else if (bolum == 3)
            {
                ayarlar["Instagram"] = _instagram;
                ayarlar["InstagramUrl"] = _instagram;
                ayarlar["Facebook"] = _facebook;
                ayarlar["FacebookUrl"] = _facebook;
                ayarlar["Youtube"] = _youtube;
                ayarlar["YoutubeUrl"] = _youtube;
            }
            else if (bolum == 6)
            {
                ayarlar["UrunSayfaBoyutu"] = _urunSayfaBoyutu.ToString();
                ayarlar["UrunVarsayilanSiralama"] = _urunVarsayilanSiralama;
            }

            foreach (var (anahtar, deger) in ayarlar)
            {
                await Api.PutAsync<object>("api/sayfa-icerigi", new
                {
                    Bolum = "ayarlar",
                    Anahtar = anahtar,
                    Deger = deger,
                    Dil = "tr"
                });
            }

            Snackbar.Add(DilServisi.T("admin.ayarlar.guncellendi", "Ayarlar başarıyla güncellendi."), Severity.Success);
        }
        catch
        {
            Snackbar.Add(DilServisi.T("admin.ayarlar.hataOlustu", "Ayarlar kaydedilirken hata oluştu."), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task LogoYukle(IBrowserFile dosya)
    {
        if (dosya == null) return;
        _yukleniyor = true;
        try
        {
            using var icerik = new MultipartFormDataContent();
            using var dosyaAkisi = dosya.OpenReadStream(10_000_000); // En fazla 10MB
            using var dosyaIcerigi = new StreamContent(dosyaAkisi);
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

            var yanit = await Api.PostMultipartAsync<VizitLink3D.Ortak.Modeller.Medya.Medya>("api/medya/yukle", icerik);
            if (yanit?.BasariliMi == true && yanit.Veri?.DosyaYolu != null)
            {
                _logoUrl = yanit.Veri.DosyaYolu.StartsWith('/') ? yanit.Veri.DosyaYolu : "/" + yanit.Veri.DosyaYolu;
                Snackbar.Add(DilServisi.T("admin.ayarlar.logoYuklendi", "Logo başarıyla yüklendi. Kaydet butonuna basarak güncelleyebilirsiniz."), Severity.Info);
            }
            else
            {
                Snackbar.Add(yanit?.Mesaj ?? DilServisi.T("admin.ayarlar.logoYuklemeHatasi", "Logo yüklenirken hata oluştu."), Severity.Error);
            }
        }
        catch (Exception hata)
        {
            Snackbar.Add(string.Format(DilServisi.T("admin.ayarlar.logoYuklemeHataDetay", "Logo yüklenirken hata oluştu: {0}"), hata.Message), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task FaviconYukle(IBrowserFile dosya)
    {
        if (dosya == null) return;
        _yukleniyor = true;
        try
        {
            using var icerik = new MultipartFormDataContent();
            using var dosyaAkisi = dosya.OpenReadStream(2_000_000); // En fazla 2MB
            using var dosyaIcerigi = new StreamContent(dosyaAkisi);
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

            var yanit = await Api.PostMultipartAsync<VizitLink3D.Ortak.Modeller.Medya.Medya>("api/medya/yukle", icerik);
            if (yanit?.BasariliMi == true && yanit.Veri?.DosyaYolu != null)
            {
                _faviconUrl = yanit.Veri.DosyaYolu.StartsWith('/') ? yanit.Veri.DosyaYolu : "/" + yanit.Veri.DosyaYolu;
                Snackbar.Add(DilServisi.T("admin.ayarlar.faviconYuklendi", "Favicon başarıyla yüklendi. Kaydet butonuna basarak güncelleyebilirsiniz."), Severity.Info);
            }
            else
            {
                Snackbar.Add(yanit?.Mesaj ?? DilServisi.T("admin.ayarlar.faviconYuklemeHatasi", "Favicon yüklenirken hata oluştu."), Severity.Error);
            }
        }
        catch (Exception hata)
        {
            Snackbar.Add(string.Format(DilServisi.T("admin.ayarlar.faviconYuklemeHataDetay", "Favicon yüklenirken hata oluştu: {0}"), hata.Message), Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private static string MarkaVarligiNormalizeEt(string? deger, string varsayilanDeger)
    {
        if (string.IsNullOrWhiteSpace(deger))
        {
            return varsayilanDeger;
        }

        var normalizeDeger = deger.Contains("vizitlink3d", StringComparison.OrdinalIgnoreCase)
            ? varsayilanDeger
            : deger;

        if (normalizeDeger.Equals("/medya/brand/orpay-logo.svg", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.Equals("medya/brand/orpay-logo.svg", StringComparison.OrdinalIgnoreCase))
        {
            normalizeDeger = "/medya/brand/orpay-logo-kare.png";
        }

        if (normalizeDeger.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || normalizeDeger.StartsWith("/", StringComparison.Ordinal))
        {
            return normalizeDeger;
        }

        return "/" + normalizeDeger.TrimStart('~').TrimStart('/');
    }

    private static string MarkaMetniNormalizeEt(string? deger, string varsayilanDeger)
    {
        if (string.IsNullOrWhiteSpace(deger))
        {
            return varsayilanDeger;
        }

        return deger.Contains("vizitlink3d", StringComparison.OrdinalIgnoreCase)
            || deger.Contains("3dvizitlink", StringComparison.OrdinalIgnoreCase)
            ? varsayilanDeger
            : deger;
    }
}

