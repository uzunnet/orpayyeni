using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.UI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using VizitLink3D.UI.Servisler;

namespace VizitLink3D.UI.Pages.Admin;

public partial class AnaSayfaYonetimi
{
    [Inject] private AnimasyonMotoruServisi _animasyonMotoru { get; set; } = default!;
    
    private bool _yukleniyor = true;
    private bool _kaydediliyor = false;
    private bool _katGorselYukleniyor = false;

    // Medya Galerisi
    private bool _galeriDialog;
    private bool _galeriYukleniyor;
    private int _galeriHedefKategori; // 1 veya 2
    private int _galeriSeciliMedyaId;
    private string _galeriArama = "";
    private List<GaleriGorseliDto> _galeriMedyaListesi = [];
    private List<GaleriGorseliDto> _galeriTumListe = [];

    // Hero Model
    private MudForm _heroForm = default!;
    private bool _heroGecerliMi;
    private string _heroGorselUrl = "";
    private string _heroEtiket = "";
    private string _heroBaslik1 = "";
    private string _heroBaslik2 = "";
    private string _heroAciklama = "";

    // Video ve Medya
    private MudForm _medyaForm = default!;
    private string _videoUrl = "";
    private string _youtubeUrl = "";
    private string _videoSure = "";
    private string _videoHiz = "1.0";
    private bool _videoMute = true;
    private string _pdfKatalogUrl = "";

    // Istatistik Model
    private MudForm _istatistikForm = default!;
    private string _ist1Deger = "";
    private string _ist1Etiket = "";
    private string _ist2Deger = "";
    private string _ist2Etiket = "";
    private string _ist3Deger = "";
    private string _ist3Etiket = "";
    private string _ist4Deger = "";
    private string _ist4Etiket = "";

    // Kategori 1 (Kapak)
    private MudForm _kat1Form = default!;
    private string _kat1Gorsel = "";
    private string _kat1Etiket = "";
    private string _kat1Baslik = "";
    private string _kat1Aciklama = "";

    // Kategori 2 (Kapı)
    private MudForm _kat2Form = default!;
    private string _kat2Gorsel = "";
    private string _kat2Etiket = "";
    private string _kat2Baslik = "";
    private string _kat2Aciklama = "";

    // Mimari Seçimler Ürün Adet
    private string _oneCikanAdet = "4";

    // Genel Ayarlar
    private string _logoUrl = "";
    private string _footerAciklama = "";
    private string _facebookUrl = "";
    private string _instagramUrl = "";
    private string _youtubeSocialUrl = "";
    private string _pinterestUrl = "";
    private string _adres = "";
    private string _telefon1 = "";
    private string _telefon2 = "";
    private string _mesaiSaatleri = "";
    private SayfaDuzenAyariDto? _anasayfaDuzenAyari;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _animasyonMotoru.ScrollAnimasyonlariniBaslatAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _yukleniyor = true;
        try
        {
            // Ana sayfa içeriklerini yükle
            var anasayfaDict = await api.GetAsync<Dictionary<string, string>>("api/sayfa-icerigi/anasayfa");
            if (anasayfaDict != null)
            {
                _heroGorselUrl = anasayfaDict.GetValueOrDefault("HeroGorselUrl", "");
                _heroEtiket = anasayfaDict.GetValueOrDefault("HeroEtiket", "");
                _heroBaslik1 = anasayfaDict.GetValueOrDefault("HeroBaslik1", "");
                _heroBaslik2 = anasayfaDict.GetValueOrDefault("HeroBaslik2", "");
                _heroAciklama = anasayfaDict.GetValueOrDefault("HeroAciklama", "");

                _videoUrl = anasayfaDict.GetValueOrDefault("VideoUrl", "");
                _youtubeUrl = anasayfaDict.GetValueOrDefault("YoutubeUrl", "");
                _videoSure = anasayfaDict.GetValueOrDefault("VideoSure", "");
                _videoHiz = anasayfaDict.GetValueOrDefault("VideoHiz", "1.0");
                _videoMute = bool.TryParse(anasayfaDict.GetValueOrDefault("VideoMute", "true"), out var vm) ? vm : true;
                _pdfKatalogUrl = anasayfaDict.GetValueOrDefault("PdfKatalogUrl", "");

                _ist1Deger = anasayfaDict.GetValueOrDefault("Ist1Deger", "");
                _ist1Etiket = anasayfaDict.GetValueOrDefault("Ist1Etiket", "");
                _ist2Deger = anasayfaDict.GetValueOrDefault("Ist2Deger", "");
                _ist2Etiket = anasayfaDict.GetValueOrDefault("Ist2Etiket", "");
                _ist3Deger = anasayfaDict.GetValueOrDefault("Ist3Deger", "");
                _ist3Etiket = anasayfaDict.GetValueOrDefault("Ist3Etiket", "");
                _ist4Deger = anasayfaDict.GetValueOrDefault("Ist4Deger", "");
                _ist4Etiket = anasayfaDict.GetValueOrDefault("Ist4Etiket", "");

                _kat1Gorsel = anasayfaDict.GetValueOrDefault("Kat1Gorsel", "");
                _kat1Etiket = anasayfaDict.GetValueOrDefault("Kat1Etiket", "");
                _kat1Baslik = anasayfaDict.GetValueOrDefault("Kat1Baslik", "");
                _kat1Aciklama = anasayfaDict.GetValueOrDefault("Kat1Aciklama", "");

                _kat2Gorsel = anasayfaDict.GetValueOrDefault("Kat2Gorsel", "");
                _kat2Etiket = anasayfaDict.GetValueOrDefault("Kat2Etiket", "");
                _kat2Baslik = anasayfaDict.GetValueOrDefault("Kat2Baslik", "");
                _kat2Aciklama = anasayfaDict.GetValueOrDefault("Kat2Aciklama", "");

                _oneCikanAdet = anasayfaDict.GetValueOrDefault("OneCikanAdet", "4");
            }

            _anasayfaDuzenAyari = await api.GetAsync<SayfaDuzenAyariDto>("api/sayfa-duzen-ayarlari/anasayfa");
            if (_anasayfaDuzenAyari is not null && _anasayfaDuzenAyari.SayfaBasinaAdet > 0)
            {
                _oneCikanAdet = _anasayfaDuzenAyari.SayfaBasinaAdet.ToString();
            }

            // Genel ayarları yükle
            var ayarlarDict = await api.GetAsync<Dictionary<string, string>>("api/sayfa-icerigi/ayarlar");
            if (ayarlarDict != null)
            {
                _logoUrl = ayarlarDict.GetValueOrDefault("LogoUrl", "");
                _footerAciklama = ayarlarDict.GetValueOrDefault("FooterAciklama", "");
                _facebookUrl = ayarlarDict.GetValueOrDefault("FacebookUrl", "");
                _instagramUrl = ayarlarDict.GetValueOrDefault("InstagramUrl", "");
                _youtubeSocialUrl = ayarlarDict.GetValueOrDefault("YoutubeUrl", "");
                _pinterestUrl = ayarlarDict.GetValueOrDefault("PinterestUrl", "");
                _adres = ayarlarDict.GetValueOrDefault("Adres", "");
                _telefon1 = ayarlarDict.GetValueOrDefault("Telefon1", "");
                _telefon2 = ayarlarDict.GetValueOrDefault("Telefon2", "");
                _mesaiSaatleri = ayarlarDict.GetValueOrDefault("MesaiSaatleri", "");
            }
        }
        catch (Exception ex)
        {
            snackbar.Add(dil.T("admin.anaSayfa.yuklemeHata", "Veriler yüklenirken hata oluştu:") + " " + ex.Message, Severity.Error);
        }
        finally
        {
            _yukleniyor = false;
        }
    }

    private async Task HeroKaydet()
    {
        await _heroForm.ValidateAsync();
        if (!_heroGecerliMi) return;

        _kaydediliyor = true;
        try
        {
            await DegerKaydet("anasayfa", "HeroGorselUrl", _heroGorselUrl);
            await DegerKaydet("anasayfa", "HeroEtiket", _heroEtiket);
            await DegerKaydet("anasayfa", "HeroBaslik1", _heroBaslik1);
            await DegerKaydet("anasayfa", "HeroBaslik2", _heroBaslik2);
            await DegerKaydet("anasayfa", "HeroAciklama", _heroAciklama);
            snackbar.Add(dil.T("admin.anaSayfa.heroGuncellendi", "Hero bilgileri başarıyla güncellendi."), Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task MedyaKaydet()
    {
        _kaydediliyor = true;
        try
        {
            await DegerKaydet("anasayfa", "VideoUrl", _videoUrl);
            await DegerKaydet("anasayfa", "YoutubeUrl", _youtubeUrl);
            await DegerKaydet("anasayfa", "VideoSure", _videoSure);
            await DegerKaydet("anasayfa", "VideoHiz", _videoHiz);
            await DegerKaydet("anasayfa", "VideoMute", _videoMute.ToString());
            await DegerKaydet("anasayfa", "PdfKatalogUrl", _pdfKatalogUrl);
            snackbar.Add(dil.T("admin.anaSayfa.medyaGuncellendi", "Medya ve Katalog bilgileri başarıyla güncellendi."), Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task IstatistikKaydet()
    {
        _kaydediliyor = true;
        try
        {
            await DegerKaydet("anasayfa", "Ist1Deger", _ist1Deger);
            await DegerKaydet("anasayfa", "Ist1Etiket", _ist1Etiket);
            await DegerKaydet("anasayfa", "Ist2Deger", _ist2Deger);
            await DegerKaydet("anasayfa", "Ist2Etiket", _ist2Etiket);
            await DegerKaydet("anasayfa", "Ist3Deger", _ist3Deger);
            await DegerKaydet("anasayfa", "Ist3Etiket", _ist3Etiket);
            await DegerKaydet("anasayfa", "Ist4Deger", _ist4Deger);
            await DegerKaydet("anasayfa", "Ist4Etiket", _ist4Etiket);
            snackbar.Add(dil.T("admin.anaSayfa.istatistikGuncellendi", "İstatistikler başarıyla güncellendi."), Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task Kat1Kaydet()
    {
        await _kat1Form.ValidateAsync();
        if (!_kat1Form.IsValid) return;

        _kaydediliyor = true;
        try
        {
            await DegerKaydet("anasayfa", "Kat1Gorsel", _kat1Gorsel);
            await DegerKaydet("anasayfa", "Kat1Etiket", _kat1Etiket);
            await DegerKaydet("anasayfa", "Kat1Baslik", _kat1Baslik);
            await DegerKaydet("anasayfa", "Kat1Aciklama", _kat1Aciklama);
            snackbar.Add(dil.T("admin.anaSayfa.kat1Guncellendi", "1. Kategori başarıyla güncellendi."), Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task Kat2Kaydet()
    {
        await _kat2Form.ValidateAsync();
        if (!_kat2Form.IsValid) return;

        _kaydediliyor = true;
        try
        {
            await DegerKaydet("anasayfa", "Kat2Gorsel", _kat2Gorsel);
            await DegerKaydet("anasayfa", "Kat2Etiket", _kat2Etiket);
            await DegerKaydet("anasayfa", "Kat2Baslik", _kat2Baslik);
            await DegerKaydet("anasayfa", "Kat2Aciklama", _kat2Aciklama);
            snackbar.Add(dil.T("admin.anaSayfa.kat2Guncellendi", "2. Kategori başarıyla güncellendi."), Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task OneCikanAdetKaydet()
    {
        _kaydediliyor = true;
        try
        {
            var adet = int.TryParse(_oneCikanAdet, out var parsedAdet) && parsedAdet > 0 ? parsedAdet : 4;
            await DegerKaydet("anasayfa", "OneCikanAdet", adet.ToString());
            var duzen = new SayfaDuzenAyariDto
            {
                Id = _anasayfaDuzenAyari?.Id ?? 0,
                SayfaKodu = "anasayfa",
                SayfaAdi = "Ana Sayfa",
                SutunAdet = 4,
                SatirAdet = Math.Max(1, (int)Math.Ceiling(adet / 4.0)),
                SayfaBasinaAdet = adet,
                SayfalamaAktif = false,
                AktifMi = true
            };

            if (_anasayfaDuzenAyari?.Id > 0)
            {
                await api.PutAsync<SayfaDuzenAyariDto>("api/sayfa-duzen-ayarlari/anasayfa", duzen);
            }
            else
            {
                await api.PostAsync<SayfaDuzenAyariDto>("api/sayfa-duzen-ayarlari", duzen);
            }

            _oneCikanAdet = adet.ToString();
            _anasayfaDuzenAyari = duzen;
            snackbar.Add(dil.T("admin.anaSayfa.mimariGuncellendi", "Mimari Seçimler ürün sayısı güncellendi."), Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task AyarlarKaydet()
    {
        _kaydediliyor = true;
        try
        {
            await DegerKaydet("ayarlar", "LogoUrl", _logoUrl);
            await DegerKaydet("ayarlar", "FooterAciklama", _footerAciklama);
            await DegerKaydet("ayarlar", "FacebookUrl", _facebookUrl);
            await DegerKaydet("ayarlar", "InstagramUrl", _instagramUrl);
            await DegerKaydet("ayarlar", "YoutubeUrl", _youtubeSocialUrl);
            await DegerKaydet("ayarlar", "PinterestUrl", _pinterestUrl);
            await DegerKaydet("ayarlar", "Adres", _adres);
            await DegerKaydet("ayarlar", "Telefon1", _telefon1);
            await DegerKaydet("ayarlar", "Telefon2", _telefon2);
            await DegerKaydet("ayarlar", "MesaiSaatleri", _mesaiSaatleri);
            snackbar.Add(dil.T("admin.anaSayfa.genelAyarlarGuncellendi", "Genel ayarlar başarıyla güncellendi."), Severity.Success);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Kayıt hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _kaydediliyor = false;
        }
    }

    private async Task Kat1GorselYukle(IBrowserFile? dosya) => await KatGorselYukle(1, dosya);
    private async Task Kat2GorselYukle(IBrowserFile? dosya) => await KatGorselYukle(2, dosya);

    private string TamUrl(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol)) return string.Empty;
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        return $"{api.ApiBaseUrl}{(yol.StartsWith('/') ? yol : "/" + yol)}";
    }

    private async Task KatGorselYukle(int kategoriNo, IBrowserFile? dosya)
    {
        if (dosya is null) return;

        _katGorselYukleniyor = true;
        StateHasChanged();
        try
        {
            using var icerik = new MultipartFormDataContent();
            using var akis = dosya.OpenReadStream(20_000_000);
            using var dosyaIcerigi = new StreamContent(akis);
            icerik.Add(dosyaIcerigi, "dosya", dosya.Name);

            var cevap = await api.PostMultipartAsync<GaleriGorseliDto>("api/galeri-gorselleri/yukle", icerik);
            if (cevap?.BasariliMi == true && cevap.Veri != null)
            {
                var gorselUrl = cevap.Veri.Url;
                if (!gorselUrl.StartsWith("/")) gorselUrl = "/" + gorselUrl;

                if (kategoriNo == 1)
                    _kat1Gorsel = gorselUrl;
                else
                    _kat2Gorsel = gorselUrl;

                snackbar.Add(dil.T("admin.anaSayfa.gorselYuklendi", "Görsel başarıyla yüklendi."), Severity.Success);
            }
            else
            {
                snackbar.Add(cevap?.Mesaj ?? dil.T("admin.anaSayfa.gorselYuklenemedi", "Görsel yüklenemedi."), Severity.Error);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Yükleme hatası: {ex.Message}", Severity.Error);
        }
        finally
        {
            _katGorselYukleniyor = false;
            StateHasChanged();
        }
    }

    private async Task GaleriAc(int kategoriNo)
    {
        _galeriHedefKategori = kategoriNo;
        _galeriSeciliMedyaId = 0;
        _galeriArama = "";
        _galeriYukleniyor = true;
        _galeriDialog = true;

        var liste = await api.GetAsync<List<GaleriGorseliDto>>("api/galeri-gorselleri/detay");
        _galeriTumListe = liste ?? [];
        _galeriMedyaListesi = _galeriTumListe;
        _galeriYukleniyor = false;
    }

    private async Task GaleriAramaDegisti(string deger)
    {
        _galeriArama = deger;
        if (string.IsNullOrWhiteSpace(deger))
            _galeriMedyaListesi = _galeriTumListe;
        else
            _galeriMedyaListesi = _galeriTumListe.Where(g =>
                (g.Baslik ?? "").Contains(deger, StringComparison.OrdinalIgnoreCase) ||
                (g.AltMetin ?? "").Contains(deger, StringComparison.OrdinalIgnoreCase) ||
                g.Url.Contains(deger, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void GaleriSec(GaleriGorseliDto medya)
    {
        _galeriSeciliMedyaId = medya.Id;
    }

    private void GaleriKapat()
    {
        _galeriDialog = false;
    }

    private void GaleriOnayla()
    {
        if (_galeriSeciliMedyaId <= 0) return;

        var seciliGorsel = _galeriTumListe.FirstOrDefault(g => g.Id == _galeriSeciliMedyaId);
        if (seciliGorsel == null) return;

        var gorselUrl = seciliGorsel.Url;
        if (!gorselUrl.StartsWith("/")) gorselUrl = "/" + gorselUrl;
        if (_galeriHedefKategori == 1)
            _kat1Gorsel = gorselUrl;
        else
            _kat2Gorsel = gorselUrl;

        _galeriDialog = false;
        snackbar.Add(dil.T("admin.anaSayfa.gorselSecildi", "Görsel seçildi."), Severity.Success);
    }
    private string MedyaGorselYolu(GaleriGorseliDto medya)
    {
        var yol = medya.Url;
            
        if (!string.IsNullOrWhiteSpace(yol) && !yol.StartsWith("/")) yol = "/" + yol;
        
        if (string.IsNullOrWhiteSpace(yol)) return "/medya/vizitlink3d_default.png";
        
        if (yol.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return yol;
        return $"{api.ApiBaseUrl}{yol}";
    }
    private async Task DegerKaydet(string bolum, string anahtar, string deger)
    {
        var model = new { Bolum = bolum, Anahtar = anahtar, Deger = deger ?? "", Dil = "tr" };
        var yanit = await api.PutAsync<object>("api/sayfa-icerigi", model);
        if (yanit?.BasariliMi != true)
        {
            throw new Exception(yanit?.Mesaj ?? dil.T("admin.anaSayfa.apiHata", "Beklenmeyen API hatası"));
        }
    }
}

