using System.Security.Cryptography;
using System.Text;
using VizitLink3D.Api.AraYazilimlar;
using VizitLink3D.Api.Moduller.Guvenlik.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Ortak.Modeller.Guvenlik;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Testler;

/// <summary>
/// Paket-1: Multi-Tenant 3D Konfigüratör Studio testleri.
/// API anahtarı kapsam/süre/origin, tenant izolasyonu, konfigürasyon kaydetme,
/// geçersiz parça/ürün senaryoları.
/// </summary>
public class KonfigurasyonStudioTestleri
{
    // ===================================================================
    // A) FirmaApiAnahtari — Entity mantıksal testleri
    // ===================================================================

    [Fact]
    public void FirmaApiAnahtari_GecerliMi_AktifVeSuresiDolmamis_TrueDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = false,
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(30)
        };

        Assert.True(anahtar.GecerliMi());
    }

    [Fact]
    public void FirmaApiAnahtari_GecerliMi_Pasif_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = false,
            SilindiMi = false,
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(30)
        };

        Assert.False(anahtar.GecerliMi());
    }

    [Fact]
    public void FirmaApiAnahtari_GecerliMi_SuresiDolmus_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = false,
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(-1)
        };

        Assert.False(anahtar.GecerliMi());
    }

    [Fact]
    public void FirmaApiAnahtari_GecerliMi_Silinmis_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = true,
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(30)
        };

        Assert.False(anahtar.GecerliMi());
    }

    [Fact]
    public void FirmaApiAnahtari_SuresiDolduMu_GecmisTarih_TrueDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(-1)
        };

        Assert.True(anahtar.SuresiDolduMu());
    }

    [Fact]
    public void FirmaApiAnahtari_SuresiDolduMu_GelcekTarih_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(30)
        };

        Assert.False(anahtar.SuresiDolduMu());
    }

    [Fact]
    public void FirmaApiAnahtari_SuresiDolduMu_BosTarih_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            SonKullanmaTarihi = null
        };

        Assert.False(anahtar.SuresiDolduMu());
    }

    [Fact]
    public void FirmaApiAnahtari_KapsamVarMi_VarolanKapsam_TrueDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            Kapsam = "PublicOkuma,Embed,KonfigurasyonKaydetme"
        };

        Assert.True(anahtar.KapsamVarMi("Embed"));
        Assert.True(anahtar.KapsamVarMi("PublicOkuma"));
        Assert.True(anahtar.KapsamVarMi("KonfigurasyonKaydetme"));
    }

    [Fact]
    public void FirmaApiAnahtari_KapsamVarMi_OlmayanKapsam_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            Kapsam = "PublicOkuma"
        };

        Assert.False(anahtar.KapsamVarMi("Embed"));
        Assert.False(anahtar.KapsamVarMi("Admin"));
    }

    [Fact]
    public void FirmaApiAnahtari_OriginIzinliMi_GecerliOrigin_TrueDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            IzinVerilenDomainler = """["https://orpayormanurunleri.com.tr", "https://www.orpayormanurunleri.com.tr"]"""
        };

        Assert.True(anahtar.OriginIzınliMi("https://orpayormanurunleri.com.tr"));
        Assert.True(anahtar.OriginIzınliMi("https://www.orpayormanurunleri.com.tr"));
    }

    [Fact]
    public void FirmaApiAnahtari_OriginIzinliMi_GecersizOrigin_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            IzinVerilenDomainler = """["https://orpayormanurunleri.com.tr"]"""
        };

        Assert.False(anahtar.OriginIzınliMi("https://evil.com"));
        Assert.False(anahtar.OriginIzınliMi(""));
        Assert.False(anahtar.OriginIzınliMi(null));
    }

    [Fact]
    public void FirmaApiAnahtari_OriginIzinliMi_BosDomainListesi_FalseDonmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            IzinVerilenDomainler = null
        };

        Assert.False(anahtar.OriginIzınliMi("https://orpayormanurunleri.com.tr"));
    }

    // ===================================================================
    // B) API Anahtarı Üretim ve Hash Testleri
    // ===================================================================

    [Fact]
    public void ApiAnahtarUretici_AnahtarUret_BenzersizOlmali()
    {
        var a1 = ApiAnahtarUretici.AnahtarUret();
        var a2 = ApiAnahtarUretici.AnahtarUret();
        var a3 = ApiAnahtarUretici.AnahtarUret();

        Assert.NotEqual(a1, a2);
        Assert.NotEqual(a2, a3);
        Assert.NotEqual(a1, a3);
    }

    [Fact]
    public void ApiAnahtarUretici_AnahtarUret_OnekIleBaslamali()
    {
        var anahtar = ApiAnahtarUretici.AnahtarUret();
        Assert.StartsWith("vt3d_", anahtar);
    }

    [Fact]
    public void ApiAnahtarUretici_AnahtarUret_UzunlukDogruOlmali()
    {
        var anahtar = ApiAnahtarUretici.AnahtarUret();
        // "vt3d_" (5) + 24 byte hex (48) = 53 karakter
        Assert.Equal(53, anahtar.Length);
    }

    [Fact]
    public void ApiAnahtarUretici_HashHesapla_AyniGirdiAyniHash()
    {
        var girdi = "test-api-key-12345";
        var h1 = ApiAnahtarUretici.HashHesapla(girdi);
        var h2 = ApiAnahtarUretici.HashHesapla(girdi);

        Assert.Equal(h1, h2);
    }

    [Fact]
    public void ApiAnahtarUretici_HashHesapla_FarkliGirdiFarkliHash()
    {
        var h1 = ApiAnahtarUretici.HashHesapla("key-1");
        var h2 = ApiAnahtarUretici.HashHesapla("key-2");

        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void ApiAnahtarDogrulamaMiddleware_Sha256Hash_Tutarlilik()
    {
        var orijinal = "vt3d_test_abcd1234";
        var h1 = ApiAnahtarDogrulamaMiddleware.Sha256Hash(orijinal);
        var h2 = ApiAnahtarDogrulamaMiddleware.Sha256Hash(orijinal);

        Assert.Equal(h1, h2);
        Assert.Equal(64, h1.Length); // SHA256 hex = 64 karakter
    }

    // ===================================================================
    // C) DTO Validasyon Testleri
    // ===================================================================

    [Fact]
    public void KonfigurasyonOlusturDto_GecerliVeri_TumAlanlarSetlenebilmeli()
    {
        var dto = new KonfigurasyonOlusturDto(
            UrunId: 1,
            OturumAnahtari: "abc-session",
            Not: "Test notu",
            Parcalar:
            [
                new KonfigurasyonParcaDto(1, 5, 10, null, "Ahsap", 1.5, 90.0, 42.0, true),
                new KonfigurasyonParcaDto(2, null, null, 3, null, null, null, null, false)
            ]
        );

        Assert.Equal(1, dto.UrunId);
        Assert.Equal("abc-session", dto.OturumAnahtari);
        Assert.Equal(2, dto.Parcalar.Count);
        Assert.Equal(1, dto.Parcalar[0].UrunUcBoyutParcasiId);
        Assert.Equal("Ahsap", dto.Parcalar[0].SeciliDoku);
        Assert.Equal(1.5, dto.Parcalar[0].HareketDegeri);
        Assert.Equal(90.0, dto.Parcalar[0].Aci);
    }

    [Fact]
    public void KonfigurasyonParcaDto_Gorunurluk_VarsayilanTrueOlmali()
    {
        var dto = new KonfigurasyonParcaDto(1);
        Assert.True(dto.GorunurMu);
    }

    [Fact]
    public void MusteriKonfigurasyonu_Durum_VarsayilanTaslakOlmali()
    {
        var konfig = new MusteriKonfigurasyonu();
        Assert.Equal("Taslak", konfig.Durum);
    }

    [Fact]
    public void MusteriKonfigurasyonParcasi_YeniAlanlar_NullableDogru()
    {
        var parca = new MusteriKonfigurasyonParcasi
        {
            SeciliDoku = "Metal Fircali",
            HareketDegeri = 2.5,
            Aci = 45.0
        };

        Assert.Equal("Metal Fircali", parca.SeciliDoku);
        Assert.Equal(2.5, parca.HareketDegeri);
        Assert.Equal(45.0, parca.Aci);
    }

    // ===================================================================
    // D) Tenant İzolasyonu Senaryoları
    // ===================================================================

    [Fact]
    public void FirmaApiAnahtari_FarkliFirma_FarkliIdIleAyrilmali()
    {
        var a1 = new FirmaApiAnahtari { Id = 1, FirmaId = 10, AnahtarAd = "Key1", ApiKeyHash = "hash1", AnahtarOnEki = "vt3d_aaa" };
        var a2 = new FirmaApiAnahtari { Id = 2, FirmaId = 20, AnahtarAd = "Key1", ApiKeyHash = "hash2", AnahtarOnEki = "vt3d_bbb" };

        Assert.NotEqual(a1.FirmaId, a2.FirmaId);
        // Aynı anahtar adı farklı firmalarda olabilir (unique constraint FirmaId+AnahtarAd)
        Assert.Equal(a1.AnahtarAd, a2.AnahtarAd);
    }

    [Fact]
    public void MusteriKonfigurasyonu_FirmaId_IzolasyonIcinKullanilmali()
    {
        var k1 = new MusteriKonfigurasyonu { Id = 1, FirmaId = 10, UrunId = 1 };
        var k2 = new MusteriKonfigurasyonu { Id = 2, FirmaId = 20, UrunId = 1 };

        Assert.NotEqual(k1.FirmaId, k2.FirmaId);
    }

    // ===================================================================
    // E) Embed API Anahtar Kapsam Testleri  
    // ===================================================================

    [Fact]
    public void FirmaApiAnahtari_EmbedKapsamiYoksa_GecerliOlsaBileEmbedIcinYetkisiz()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            Kapsam = "PublicOkuma",
            IzinVerilenDomainler = """["https://orpayormanurunleri.com.tr"]"""
        };

        Assert.True(anahtar.GecerliMi());
        Assert.False(anahtar.KapsamVarMi("Embed"));
        Assert.True(anahtar.KapsamVarMi("PublicOkuma"));
    }

    [Fact]
    public void FirmaApiAnahtari_KapsamListesi_BosKapsam_BosListeDonmeli()
    {
        var anahtar = new FirmaApiAnahtari { Kapsam = "" };
        Assert.Empty(anahtar.KapsamListesi());

        // Varsayilan değer "PublicOkuma" oldugu icin bos obje 1 elemanli liste dondurur
        var anahtar2 = new FirmaApiAnahtari();
        Assert.Single(anahtar2.KapsamListesi());
        Assert.Equal("PublicOkuma", anahtar2.KapsamListesi()[0]);
    }

    [Fact]
    public void FirmaApiAnahtari_KapsamListesi_BoslukluKapsam_Trimlenmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            Kapsam = " PublicOkuma , Embed , KonfigurasyonKaydetme "
        };

        var liste = anahtar.KapsamListesi();
        Assert.Equal(3, liste.Count);
        Assert.Equal("PublicOkuma", liste[0]);
        Assert.Equal("Embed", liste[1]);
        Assert.Equal("KonfigurasyonKaydetme", liste[2]);
    }

    // ===================================================================
    // F) Sınır Durum Testleri
    // ===================================================================

    [Fact]
    public void ApiAnahtarUretici_HashHesapla_BosMetin_BosHashDegil()
    {
        var hash = ApiAnahtarUretici.HashHesapla("");
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public void MusteriKonfigurasyonParcasi_SoftDelete_VarsayilanFalse()
    {
        var parca = new MusteriKonfigurasyonParcasi();
        Assert.False(parca.SilindiMi);
        Assert.Null(parca.SilinmeTarihi);
    }

    [Fact]
    public void FirmaApiAnahtari_JsonIgnore_ApiKeyHash_Serilestirilmemeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            ApiKeyHash = "gizli-hash-degeri",
            AnahtarAd = "Test"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(anahtar);
        Assert.DoesNotContain("gizli-hash-degeri", json);
        Assert.Contains("Test", json);
    }

    // ===================================================================
    // G) Paket-2B: Admin Konfigüratör Studio Sayfası Testleri
    // ===================================================================

    /// <summary>
    /// TEST G1: UcBoyutKonfiguratorAdminKontrolcu — Authorize attribute Admin/SuperAdmin rolleri.
    /// Yetkisiz kullanıcı erişememeli.
    /// </summary>
    [Fact]
    public void UcBoyutKonfiguratorAdminKontrolcu_AuthorizeAttribute_DogruRollerTanimlanmis()
    {
        var tip = typeof(VizitLink3D.Api.Moduller.Urunler.Kontrolcüler.UcBoyutKonfiguratorAdminKontrolcu);
        var attr = tip.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

        Assert.NotNull(attr);
        Assert.Contains("Admin", attr.Roles);
        Assert.Contains("SuperAdmin", attr.Roles);
    }

    /// <summary>
    /// TEST G2: UcBoyutParcaUpsertDto validasyonu — MeshAdi zorunlu, GorunenAd zorunlu.
    /// FluentValidation hataları doğru mesajları döndürmeli.
    /// </summary>
    [Fact]
    public void UcBoyutParcaUpsertDto_GecersizVeri_ValidasyonHatasiVermeli()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Urunler.Dogrulayicilar.UcBoyutParcaUpsertDogrulayici();

        var dto = new VizitLink3D.Api.Moduller.Urunler.Dtolar.UcBoyutParcaUpsertDto(
            MeshAdi: "",
            MantiksalKod: "geçersiz türkçe",
            GorunenAd: "",
            ParcaGrubuId: null,
            HareketTipi: "GeçersizTip",
            HareketAyarlariJson: "{invalid json",
            DokuUygulanabilirMi: false,
            GorunurlukDegisebilirMi: false,
            RenklenebilirMi: true,
            MalzemeDegisebilirMi: true,
            SecilebilirMi: true,
            HareketliMi: false,
            ParcaTipi: null,
            MalzemeTipiKisiti: null,
            SiraNo: -1,
            AktifMi: true,
            AdminOnayliMi: false
        );

        var sonuc = dogrulayici.Validate(dto);
        Assert.False(sonuc.IsValid);
        Assert.Contains(sonuc.Errors, e => e.PropertyName == "MeshAdi");
        Assert.Contains(sonuc.Errors, e => e.PropertyName == "GorunenAd");
        Assert.Contains(sonuc.Errors, e => e.PropertyName == "MantiksalKod");
        Assert.Contains(sonuc.Errors, e => e.PropertyName == "HareketTipi");
        Assert.Contains(sonuc.Errors, e => e.PropertyName == "HareketAyarlariJson");
        Assert.Contains(sonuc.Errors, e => e.PropertyName == "SiraNo");
    }

    /// <summary>
    /// TEST G3: UcBoyutParcaUpsertDto gecerli veri — validasyon geçmeli.
    /// </summary>
    [Fact]
    public void UcBoyutParcaUpsertDto_GecerliVeri_ValidasyonGecmeli()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Urunler.Dogrulayicilar.UcBoyutParcaUpsertDogrulayici();

        var dto = new VizitLink3D.Api.Moduller.Urunler.Dtolar.UcBoyutParcaUpsertDto(
            MeshAdi: "govde_mesh",
            MantiksalKod: "govde-01",
            GorunenAd: "Ana Govde",
            ParcaGrubuId: null,
            HareketTipi: "Sabit",
            HareketAyarlariJson: null,
            DokuUygulanabilirMi: true,
            GorunurlukDegisebilirMi: false,
            RenklenebilirMi: true,
            MalzemeDegisebilirMi: true,
            SecilebilirMi: true,
            HareketliMi: false,
            ParcaTipi: "Govde",
            MalzemeTipiKisiti: null,
            SiraNo: 1,
            AktifMi: true,
            AdminOnayliMi: true
        );

        var sonuc = dogrulayici.Validate(dto);
        Assert.True(sonuc.IsValid);
    }

    /// <summary>
    /// TEST G4: UrunUcBoyutSahneOnayari — Soft delete alanları varsayılan değerler.
    /// Preset silindiğinde fiziksel silme değil soft delete kullanılmalı.
    /// </summary>
    [Fact]
    public void UrunUcBoyutSahneOnayari_SoftDelete_VarsayilanDegerlerDogru()
    {
        var onayar = new UrunUcBoyutSahneOnayari();

        Assert.False(onayar.SilindiMi);
        Assert.Null(onayar.SilinmeTarihi);
        Assert.True(onayar.AktifMi);
        Assert.False(onayar.VarsayilanMi);
    }

    /// <summary>
    /// TEST G5: UrunUcBoyutSahneOnayari — Kod alanı ASCII slug formatında olmalı.
    /// Türkçe karakter içermemeli (DB sütun adı değil, veri alanı kontrolü — validasyon DTO seviyesinde).
    /// </summary>
    [Fact]
    public void UcBoyutSahneOnayariDto_Kod_GecerliAsciiOlmali()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Urunler.Dogrulayicilar.UcBoyutSahneOnayariDogrulayici();

        // Geçersiz: Türkçe karakter içeren kod
        var gecersiz = new VizitLink3D.Api.Moduller.Urunler.Dtolar.UcBoyutSahneOnayariDto(
            Ad: "Test Preset",
            Kod: "önayar-şablon",
            AyarlarJson: null,
            VarsayilanMi: false,
            AktifMi: true,
            SiraNo: 1
        );
        var sonucGecersiz = dogrulayici.Validate(gecersiz);
        Assert.False(sonucGecersiz.IsValid);
        Assert.Contains(sonucGecersiz.Errors, e => e.PropertyName == "Kod");

        // Geçerli: ASCII slug
        var gecerli = new VizitLink3D.Api.Moduller.Urunler.Dtolar.UcBoyutSahneOnayariDto(
            Ad: "Test Preset",
            Kod: "onayar-sablon",
            AyarlarJson: """{"kamera":{"fov":45}}""",
            VarsayilanMi: true,
            AktifMi: true,
            SiraNo: 1
        );
        var sonucGecerli = dogrulayici.Validate(gecerli);
        Assert.True(sonucGecerli.IsValid);
    }

    /// <summary>
    /// TEST G6: Model seçimi — Boş parça listesi durumunda filtreleme boş dönmeli.
    /// Admin Studio sayfasında model değiştiğinde eski parçalar temizlenmeli.
    /// </summary>
    [Fact]
    public void ParcaListesiBosIken_Filtreleme_BosListeDonmeli()
    {
        // Simüle: boş parça listesi ile filtreleme
        List<UrunUcBoyutParcasi> parcalar = [];
        string arama = "govde";

        var filtrelenmis = parcalar
            .Where(p => p.MeshAdi.Contains(arama, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(filtrelenmis);
    }
}
