using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;
using System.Text.Json;

namespace VizitLink3D.Testler;

/// <summary>
/// VizitLink3D API entegrasyon testleri.
/// Anayasa §6.2: Her ozellik icin en az 5 test yazilmalidir.
/// </summary>
public class ApiTemelTestler
{
    /// <summary>
    /// Test 1: Cevap<T> zarf yapisi dogru calisiyor mu?
    /// Basarili senaryo.
    /// </summary>
    [Fact]
    public void Cevap_Basarili_DogruYapiyiOlusturmali()
    {
        var cevap = Cevap<string>.Basarili("test-verisi", "Islem tamam.");
        Assert.True(cevap.BasariliMi);
        Assert.Equal("test-verisi", cevap.Veri);
        Assert.Equal("Islem tamam.", cevap.Mesaj);
        Assert.Empty(cevap.Hatalar);
    }

    /// <summary>
    /// Test 2: Cevap<T> hata durumu dogru calisiyor mu?
    /// </summary>
    [Fact]
    public void Cevap_Hata_DogruYapiyiOlusturmali()
    {
        var cevap = Cevap<string>.Hata("Bir hata olustu.", new List<string> { "Alan1 bos.", "Alan2 gecersiz." });
        Assert.False(cevap.BasariliMi);
        Assert.Equal("Bir hata olustu.", cevap.Mesaj);
        Assert.Equal(2, cevap.Hatalar.Count);
        Assert.Null(cevap.Veri);
    }

    /// <summary>
    /// Test 3: Cevap<T> bos veri ile calisiyor mu?
    /// </summary>
    [Fact]
    public void Cevap_Basarili_NullVeriKabulEtmeli()
    {
        var cevap = Cevap<object>.Basarili(null!);
        Assert.True(cevap.BasariliMi);
        Assert.Null(cevap.Veri);
    }

    /// <summary>
    /// Test 4: Kullanici modelinde JsonIgnore dogru calisiyor mu?
    /// Anayasa §3.4: Hassas alanlar API yanitinda gorunmemeli.
    /// </summary>
    [Fact]
    public void Kullanici_JsonIgnore_HassasAlanlariGizlemeli()
    {
        var kullanici = new Kullanici
        {
            Id = 1,
            KullaniciAdi = "test",
            Eposta = "test@test.com",
            SifreHash = "gizli-hash-degeri",
            PinHash = "gizli-pin",
            DesenHash = "gizli-desen",
            Rol = Rol.SuperAdmin
        };

        var json = JsonSerializer.Serialize(kullanici);
        Assert.Contains("test@test.com", json);
        Assert.Contains("SuperAdmin", json);
        Assert.DoesNotContain("gizli-hash-degeri", json);
        Assert.DoesNotContain("gizli-pin", json);
        Assert.DoesNotContain("gizli-desen", json);
    }

    /// <summary>
    /// Test 5: Firma modeli zorunlu alanlari iceriyor mu?
    /// </summary>
    [Fact]
    public void Firma_ZorunluAlanlar_DoluOlmali()
    {
        var firma = new Firma
        {
            Ad = "VizitLink3D",
            Unvan = "VizitLink3D Mobilya",
            Slug = "vizitlink3d",
            Eposta = "info@3dvizitlink.com.tr",
            Domain = "3dvizitlink.com.tr"
        };

        Assert.NotEmpty(firma.Ad);
        Assert.NotEmpty(firma.Slug);
        Assert.NotEmpty(firma.Eposta);
        Assert.True(firma.AktifMi); // Varsayilan true
    }

    /// <summary>
    /// Test 6: Slayt modeli varsayilan degerleri dogru mu?
    /// </summary>
    [Fact]
    public void Slayt_VarsayilanDegerler_DogruOlmali()
    {
        var slayt = new Slayt
        {
            Baslik = "Test Slayt",
            SiraNo = 1
        };

        Assert.Equal("fade", slayt.AnimasyonTipi);
        Assert.Equal(800, slayt.GecisHizi);
        Assert.Equal(5000, slayt.GosterimSuresi);
        Assert.True(slayt.AktifMi);
    }

    /// <summary>
    /// Test 7: MenuOgesi alt menuleri dogru yapilandiriliyor mu?
    /// </summary>
    [Fact]
    public void MenuOgesi_AltMenuler_DogruEklenmeli()
    {
        var anaMenu = new MenuOgesi
        {
            Baslik = "Urunler",
            Url = "urunler",
            AltMenuler = new List<MenuOgesi>
            {
                new() { Baslik = "Kapak", Url = "kapak" },
                new() { Baslik = "Kapi", Url = "kapi" }
            }
        };

        Assert.Equal(2, anaMenu.AltMenuler.Count);
        Assert.Equal("Kapak", anaMenu.AltMenuler[0].Baslik);
    }

    /// <summary>
    /// Test 8: Ceviri modeli anahtar+dil unique yapisi dogru mu?
    /// </summary>
    [Fact]
    public void Ceviri_AnahtarVeDil_BenzersizOlmali()
    {
        var ceviri1 = new Ceviri { Anahtar = "menu.anasayfa", Dil = "tr", Deger = "Ana Sayfa" };
        var ceviri2 = new Ceviri { Anahtar = "menu.anasayfa", Dil = "en", Deger = "Home" };

        // Ayni anahtar farkli dil = farkli kayit
        Assert.NotEqual(ceviri1.Dil, ceviri2.Dil);
        Assert.Equal(ceviri1.Anahtar, ceviri2.Anahtar);

        // Ayni anahtar ayni dil = ayni kayit (unique index garantiler)
        var ceviri3 = new Ceviri { Anahtar = "menu.anasayfa", Dil = "tr", Deger = "Ana Sayfa" };
        Assert.Equal(ceviri1.Dil, ceviri3.Dil);
        Assert.Equal(ceviri1.Anahtar, ceviri3.Anahtar);
    }

    /// <summary>
    /// Test 9: AuditLog modeli dogru varsayilan degerleri iceriyor mu?
    /// </summary>
    [Fact]
    public void AuditLog_VarsayilanDegerler_DogruOlmali()
    {
        var log = new AuditLog
        {
            Eylem = "KapiModeli.Eklendi",
            YeniDeger = "{\"Ad\":\"Test\"}"
        };

        Assert.NotEqual(default(DateTime), log.ZamanDamgasi);
        Assert.Equal("KapiModeli.Eklendi", log.Eylem);
        Assert.NotNull(log.YeniDeger);
    }

    /// <summary>
    /// Test 10: SistemAyari key-value yapisi dogru calisiyor mu?
    /// </summary>
    [Fact]
    public void SistemAyari_AnahtarDeger_DogruAtanmali()
    {
        var ayar = new SistemAyari
        {
            Anahtar = "site.baslik",
            Deger = "VizitLink3D",
            Tip = "string",
            Aciklama = "Site basligi"
        };

        Assert.Equal("site.baslik", ayar.Anahtar);
        Assert.Equal("VizitLink3D", ayar.Deger);
        Assert.Equal("string", ayar.Tip);
    }

    /// <summary>
    /// Test 11: HaberYazisi zorunlu alanlari dogru ataniyor mu?
    /// </summary>
    [Fact]
    public void HaberYazisi_ZorunluAlanlar_DoluOlmali()
    {
        var yazi = new HaberYazisi
        {
            Baslik = "Test Haber",
            Slug = "test-haber",
            Icerik = "<p>Icerik</p>",
            Ozet = "Ozet metin"
        };

        Assert.NotEmpty(yazi.Baslik);
        Assert.NotEmpty(yazi.Slug);
        Assert.NotEmpty(yazi.Icerik);
        Assert.True(yazi.AktifMi);
    }

    /// <summary>
    /// Test 12: Proje modeli FK iliskileri dogru mu?
    /// </summary>
    [Fact]
    public void Proje_KategoriVeResimler_DogruBaglanmali()
    {
        var proje = new Proje
        {
            Baslik = "Villa Projesi",
            Slug = "villa",
            KategoriId = 1,
            Resimler = new List<ProjeResim>
            {
                new() { Url = "/resim1.jpg", Sira = 1 },
                new() { Url = "/resim2.jpg", Sira = 2 }
            }
        };

        Assert.Equal(2, proje.Resimler.Count);
        Assert.Equal(1, proje.Resimler[0].Sira);
    }

    /// <summary>
    /// Test 13: IletisimMesaji oncelik seviyesi dogru varsayilan mi?
    /// </summary>
    [Fact]
    public void IletisimMesaji_VarsayilanOncelik_NormalOlmali()
    {
        var mesaj = new IletisimMesaji
        {
            AdSoyad = "Test Kullanici",
            Eposta = "test@test.com",
            Mesaj = "Deneme mesaji"
        };

        Assert.Equal("Normal", mesaj.OncelikSeviyesi);
        Assert.False(mesaj.OkunduMu);
        Assert.False(mesaj.CevaplandiMi);
    }

    /// <summary>
    /// Test 14: KapiKategorisi slug validasyonu dogru calisiyor mu?
    /// Anayasa §23.6: FluentValidation zorunlu.
    /// </summary>
    [Fact]
    public void KapiKategorisiDogrulayici_GecersizSlug_HataVermeli()
    {
        var dogrulayici = new VizitLink3D.Api.Dogrulayicilar.KapiKategorisiDogrulayici();
        var kategori = new KapiKategorisi { Ad = "", Slug = "gecersiz slug!" };

        var sonuc = dogrulayici.Validate(kategori);
        Assert.False(sonuc.IsValid);
    }

    /// <summary>
    /// Test 15: KapakModeli modeli JSON alanlari dogru serilestiriliyor mu?
    /// </summary>
    [Fact]
    public void KapakModeli_TeknikOzellikler_JsonFormatindaOlmali()
    {
        var model = new KapakModeli
        {
            ModelAdi = "Test Kapak",
            ModelKodu = "TK-001",
            Slug = "test-kapak",
            TeknikOzelliklerJson = "{\"kalınlık\":\"18mm\",\"agırlık\":\"25kg\"}"
        };

        Assert.NotEmpty(model.TeknikOzelliklerJson);
        Assert.Contains("kalınlık", model.TeknikOzelliklerJson);
    }

    /// <summary>
    /// Test 16: Cevap<T> JSON serialization dogru calisiyor mu?
    /// </summary>
    [Fact]
    public void Cevap_JsonSerialize_DogruFormattaOlmali()
    {
        var cevap = Cevap<List<string>>.Basarili(new List<string> { "a", "b" });
        var json = JsonSerializer.Serialize(cevap);

        Assert.Contains("\"BasariliMi\":true", json);
        Assert.Contains("\"Veri\":", json);
        Assert.Contains("\"a\"", json);
    }

    /// <summary>
    /// Test 17: Lisans modeli varsayilan tip dogru mu?
    /// </summary>
    [Fact]
    public void Lisans_VarsayilanTip_YillikOlmali()
    {
        var lisans = new Lisans
        {
            BirincilDomain = "test.com",
            BaslangicTarihi = DateTime.UtcNow,
            BitisTarihi = DateTime.UtcNow.AddYears(1),
            LisansAnahtari = "TEST-HMAC-KEY"
        };

        Assert.Equal("Yillik", lisans.LisansTipi);
        Assert.True(lisans.AktifMi);
    }

    /// <summary>
    /// Test 18: BultenAbonesi dogru varsayilan degerlere sahip mi?
    /// </summary>
    [Fact]
    public void BultenAbonesi_VarsayilanDegerler_DogruOlmali()
    {
        var abone = new BultenAbonesi
        {
            Eposta = "test@test.com",
            AdSoyad = "Test"
        };

        Assert.True(abone.AktifMi);
        Assert.False(abone.DogrulandiMi);
    }
}
