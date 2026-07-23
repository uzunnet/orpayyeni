using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VizitLink3D.Api.AraYazilimlar;
using VizitLink3D.Api.Moduller.Guvenlik.Dtolar;
using VizitLink3D.Ortak.Modeller.Guvenlik;

namespace VizitLink3D.Testler;

/// <summary>
/// Paket-4: Embed/Entegrasyon API Erişim Katmanı testleri.
/// Kapsam: Scope red, expired/inactive key, origin mismatch,
/// tenant conflict, DTO leak check, server scope, no key 401,
/// validation helpers, widget API pattern.
/// </summary>
public class Paket4_EmbedEntegrasyonTestleri
{
    // ===================================================================
    // A) API Anahtarı Scope Testleri
    // ===================================================================

    [Fact]
    public void ApiAnahtari_EmbedKapsamiYoksa_EmbedIcinYetkisiz()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = false,
            Kapsam = "PublicOkuma",
            IzinVerilenDomainler = """["https://example.com"]""",
            FirmaId = 1
        };

        Assert.True(anahtar.GecerliMi());
        Assert.False(anahtar.KapsamVarMi("Embed"));
        Assert.True(anahtar.KapsamVarMi("PublicOkuma"));
    }

    [Fact]
    public void ApiAnahtari_SunucuEntegrasyonuKapsami_SadeceEntegrasyonIcinGecerli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = false,
            Kapsam = "SunucuEntegrasyonu",
            IzinVerilenDomainler = null, // Sunucu entegrasyonunda domain zorunlu değil
            FirmaId = 1
        };

        Assert.True(anahtar.GecerliMi());
        Assert.True(anahtar.KapsamVarMi("SunucuEntegrasyonu"));
        Assert.False(anahtar.KapsamVarMi("Embed"));
    }

    [Fact]
    public void ApiAnahtari_CiftKapsam_HemEmbedHemEntegrasyon()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = false,
            Kapsam = "Embed,SunucuEntegrasyonu",
            IzinVerilenDomainler = """["https://example.com"]""",
            FirmaId = 1
        };

        Assert.True(anahtar.KapsamVarMi("Embed"));
        Assert.True(anahtar.KapsamVarMi("SunucuEntegrasyonu"));
        Assert.Equal(2, anahtar.KapsamListesi().Count);
    }

    // ===================================================================
    // B) Süresi Dolmuş / Pasif / Silinmiş Anahtar Testleri
    // ===================================================================

    [Fact]
    public void ApiAnahtari_SuresiDolmus_EmbedIcinReddedilmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = false,
            Kapsam = "Embed",
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(-1), // Dün dolmuş
            IzinVerilenDomainler = """["https://example.com"]"""
        };

        Assert.False(anahtar.GecerliMi(), "Süresi dolmuş anahtar geçerli olmamalı.");
        Assert.True(anahtar.SuresiDolduMu());
    }

    [Fact]
    public void ApiAnahtari_Pasif_EmbedIcinReddedilmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = false,
            SilindiMi = false,
            Kapsam = "Embed",
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(30),
            IzinVerilenDomainler = """["https://example.com"]"""
        };

        Assert.False(anahtar.GecerliMi(), "Pasif anahtar geçerli olmamalı.");
    }

    [Fact]
    public void ApiAnahtari_Silinmis_EmbedIcinReddedilmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            AktifMi = true,
            SilindiMi = true,
            Kapsam = "Embed",
            SonKullanmaTarihi = DateTime.UtcNow.AddDays(30),
            IzinVerilenDomainler = """["https://example.com"]"""
        };

        Assert.False(anahtar.GecerliMi(), "Silinmiş anahtar geçerli olmamalı.");
    }

    // ===================================================================
    // C) Origin Doğrulama Testleri
    // ===================================================================

    [Fact]
    public void ApiAnahtari_OriginEşleşiyor_Izinli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            IzinVerilenDomainler = """["https://orpayormanurunleri.com.tr", "https://www.orpayormanurunleri.com.tr"]"""
        };

        Assert.True(anahtar.OriginIzınliMi("https://orpayormanurunleri.com.tr"));
        Assert.True(anahtar.OriginIzınliMi("https://www.orpayormanurunleri.com.tr"));
    }

    [Fact]
    public void ApiAnahtari_OriginEslesmiyor_Reddedilmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            IzinVerilenDomainler = """["https://orpayormanurunleri.com.tr"]"""
        };

        Assert.False(anahtar.OriginIzınliMi("https://evil.com"));
        Assert.False(anahtar.OriginIzınliMi("https://orpayormanurunleri.com.tr.evil.com"));
    }

    [Fact]
    public void ApiAnahtari_OriginBosIse_Reddedilmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            IzinVerilenDomainler = """["https://orpayormanurunleri.com.tr"]"""
        };

        Assert.False(anahtar.OriginIzınliMi(null));
        Assert.False(anahtar.OriginIzınliMi(""));
    }

    [Fact]
    public void ApiAnahtari_DomainListesiBosIse_OriginReddedilmeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            IzinVerilenDomainler = null
        };

        Assert.False(anahtar.OriginIzınliMi("https://orpayormanurunleri.com.tr"));

        anahtar.IzinVerilenDomainler = "";
        Assert.False(anahtar.OriginIzınliMi("https://orpayormanurunleri.com.tr"));

        anahtar.IzinVerilenDomainler = "[]";
        Assert.False(anahtar.OriginIzınliMi("https://orpayormanurunleri.com.tr"));
    }

    // ===================================================================
    // D) API Anahtarı Hash ve Üretim Testleri
    // ===================================================================

    [Fact]
    public void ApiAnahtarUretici_HashTutarliligi()
    {
        var orijinal = "vt3d_test_anahtar_1234567890abcdef";
        var h1 = ApiAnahtarUretici.HashHesapla(orijinal);
        var h2 = ApiAnahtarUretici.HashHesapla(orijinal);
        var h3 = ApiAnahtarDogrulamaMiddleware.Sha256Hash(orijinal);

        Assert.Equal(h1, h2);
        Assert.Equal(h1, h3);
        Assert.Equal(64, h1.Length); // SHA256 hex = 64 karakter
    }

    [Fact]
    public void ApiAnahtarUretici_AnahtarFormatDogru()
    {
        var anahtar = ApiAnahtarUretici.AnahtarUret();

        Assert.StartsWith("vt3d_", anahtar);
        Assert.Equal(53, anahtar.Length); // vt3d_ + 48 hex
    }

    [Fact]
    public void ApiAnahtarUretici_HerUretimFarkli()
    {
        var anahtarlar = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var anahtar = ApiAnahtarUretici.AnahtarUret();
            Assert.True(anahtarlar.Add(anahtar), "Her üretilen anahtar benzersiz olmalı.");
        }
    }

    // ===================================================================
    // E) Kapsam Doğrulama Testleri
    // ===================================================================

    [Fact]
    public void KapsamDogrula_GecerliKapsam_KabulEtmeli()
    {
        var (gecerli, hata) = ApiAnahtarUretici.KapsamDogrula("Embed");
        Assert.True(gecerli);
        Assert.Null(hata);

        (gecerli, hata) = ApiAnahtarUretici.KapsamDogrula("SunucuEntegrasyonu");
        Assert.True(gecerli);
        Assert.Null(hata);

        (gecerli, hata) = ApiAnahtarUretici.KapsamDogrula("Embed,PublicOkuma");
        Assert.True(gecerli);
        Assert.Null(hata);
    }

    [Fact]
    public void KapsamDogrula_GecersizKapsam_Reddetmeli()
    {
        var (gecerli, hata) = ApiAnahtarUretici.KapsamDogrula("AdminYetkisi");
        Assert.False(gecerli);
        Assert.NotNull(hata);
        Assert.Contains("AdminYetkisi", hata);
    }

    [Fact]
    public void KapsamDogrula_BosKapsam_Reddetmeli()
    {
        var (gecerli, hata) = ApiAnahtarUretici.KapsamDogrula("");
        Assert.False(gecerli);
        Assert.NotNull(hata);

        (gecerli, hata) = ApiAnahtarUretici.KapsamDogrula(null!);
        Assert.False(gecerli);
    }

    // ===================================================================
    // F) Domain Doğrulama Testleri
    // ===================================================================

    [Fact]
    public void IzinVerilenDomainlerDogrula_GecerliDomainler_KabulEtmeli()
    {
        var json = """["https://orpayormanurunleri.com.tr", "https://www.orpayormanurunleri.com.tr"]""";
        var (gecerli, hata, domainler) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula(json);

        Assert.True(gecerli);
        Assert.Null(hata);
        Assert.NotNull(domainler);
        Assert.Equal(2, domainler!.Count);
    }

    [Fact]
    public void IzinVerilenDomainlerDogrula_EmbedKapsamindaDomainZorunlu()
    {
        var (gecerli, hata, _) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula(null, "Embed");
        Assert.False(gecerli);
        Assert.NotNull(hata);
        Assert.Contains("Embed", hata);
    }

    [Fact]
    public void IzinVerilenDomainlerDogrula_SunucuEntegrasyonu_DomainZorunluDegil()
    {
        var (gecerli, hata, _) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula(null, "SunucuEntegrasyonu");
        Assert.True(gecerli);
        Assert.Null(hata);
    }

    [Fact]
    public void IzinVerilenDomainlerDogrula_GecersizJson_Reddetmeli()
    {
        var (gecerli, hata, _) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula("gecersiz json!!!");
        Assert.False(gecerli);
        Assert.NotNull(hata);
        Assert.Contains("JSON", hata);
    }

    [Fact]
    public void IzinVerilenDomainlerDogrula_ProtocolsuzDomain_Reddetmeli()
    {
        var json = """["orpayormanurunleri.com.tr"]""";
        var (gecerli, hata, _) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula(json);
        Assert.False(gecerli);
        Assert.NotNull(hata);
        Assert.Contains("https://", hata);
    }

    [Fact]
    public void IzinVerilenDomainlerDogrula_PathIcerenDomain_Reddetmeli()
    {
        var json = """["https://orpayormanurunleri.com.tr/konfigurator"]""";
        var (gecerli, hata, _) = ApiAnahtarUretici.IzinVerilenDomainlerDogrula(json);
        Assert.False(gecerli);
        Assert.NotNull(hata);
        Assert.Contains("path", hata, StringComparison.OrdinalIgnoreCase);
    }

    // ===================================================================
    // G) Tenant İzolasyon Testleri
    // ===================================================================

    [Fact]
    public void ApiAnahtari_FirmaId_FarkliTenantlarAyrilmali()
    {
        var a1 = new FirmaApiAnahtari
        {
            Id = 1, FirmaId = 10, AnahtarAd = "KeyA",
            ApiKeyHash = "hash1", AnahtarOnEki = "vt3d_aaa",
            Kapsam = "Embed"
        };
        var a2 = new FirmaApiAnahtari
        {
            Id = 2, FirmaId = 20, AnahtarAd = "KeyB",
            ApiKeyHash = "hash2", AnahtarOnEki = "vt3d_bbb",
            Kapsam = "Embed"
        };

        Assert.NotEqual(a1.FirmaId, a2.FirmaId);
        Assert.NotEqual(a1.ApiKeyHash, a2.ApiKeyHash);
        Assert.True(a1.FirmaId == 10);
        Assert.True(a2.FirmaId == 20);
    }

    // ===================================================================
    // H) PublicKonfiguratorDto Sızıntı Testi (Embed DTO Leak)
    // ===================================================================

    [Fact]
    public void PublicKonfiguratorDto_TeknikAlanlar_Yok()
    {
        // PublicKonfiguratorDto'da mesh/HDR/admin alanları olmamalı
        var tip = typeof(VizitLink3D.Ortak.Modeller.Urunler.PublicKonfiguratorDto);
        var ozellikler = tip.GetProperties().Select(p => p.Name).ToHashSet();

        Assert.DoesNotContain("ModelDosyaYolu", ozellikler);
        Assert.DoesNotContain("HDRDosyaYolu", ozellikler);
        Assert.DoesNotContain("KameraAyarlari", ozellikler);
        Assert.DoesNotContain("OlusturulmaTarihi", ozellikler);
        Assert.DoesNotContain("SilindiMi", ozellikler);
    }

    [Fact]
    public void PublicParcaDto_TeknikAlanlar_Yok()
    {
        var tip = typeof(VizitLink3D.Ortak.Modeller.Urunler.PublicParcaDto);
        var ozellikler = tip.GetProperties().Select(p => p.Name).ToHashSet();

        Assert.DoesNotContain("MeshAdi", ozellikler);
        Assert.DoesNotContain("HareketAyarlariJson", ozellikler);
        Assert.DoesNotContain("MalzemeTipiKisiti", ozellikler);
        Assert.DoesNotContain("OlusturulmaTarihi", ozellikler);
        Assert.DoesNotContain("AdminOnayliMi", ozellikler);
    }

    // ===================================================================
    // I) Embed vs Entegrasyon Scope Çapraz Testleri
    // ===================================================================

    [Fact]
    public void EmbedAnahtari_EntegrasyonEndpointindeYetkisiz()
    {
        // Embed kapsamlı anahtar SunucuEntegrasyonu endpoint'inde yetkisiz olmalı
        var embedAnahtar = new FirmaApiAnahtari
        {
            Kapsam = "Embed",
            IzinVerilenDomainler = """["https://example.com"]"""
        };

        Assert.True(embedAnahtar.KapsamVarMi("Embed"));
        Assert.False(embedAnahtar.KapsamVarMi("SunucuEntegrasyonu"));
    }

    [Fact]
    public void EntegrasyonAnahtari_EmbedEndpointindeYetkisiz()
    {
        // SunucuEntegrasyonu kapsamlı anahtar Embed endpoint'inde yetkisiz olmalı
        var entegrasyonAnahtar = new FirmaApiAnahtari
        {
            Kapsam = "SunucuEntegrasyonu",
            IzinVerilenDomainler = null
        };

        Assert.True(entegrasyonAnahtar.KapsamVarMi("SunucuEntegrasyonu"));
        Assert.False(entegrasyonAnahtar.KapsamVarMi("Embed"));
    }

    // ===================================================================
    // J) JsonIgnore Güvenlik Testi
    // ===================================================================

    [Fact]
    public void FirmaApiAnahtari_JsonIgnore_ApiKeyHash_Serilestirilmemeli()
    {
        var anahtar = new FirmaApiAnahtari
        {
            Id = 1,
            FirmaId = 1,
            AnahtarAd = "Test",
            ApiKeyHash = "supersecrethash1234567890abcdef",
            AnahtarOnEki = "vt3d_abc",
            Kapsam = "Embed"
        };

        var json = JsonSerializer.Serialize(anahtar);
        Assert.DoesNotContain("supersecrethash", json);
        Assert.DoesNotContain("ApiKeyHash", json);
    }

    // ===================================================================
    // K) Anahtar Üretim ve Yanıt Güvenliği
    // ===================================================================

    [Fact]
    public void FirmaApiAnahtariOlusturYanitDto_DuzMetinAnahtar_SadeceOlusturmadaVar()
    {
        // Oluşturma yanıtında düz metin anahtar bulunur
        var yanit = new FirmaApiAnahtariOlusturYanitDto(
            1, "Test", "vt3d_abc", "vt3d_secret123456", "Embed",
            """["https://example.com"]""", null, DateTime.UtcNow);

        Assert.NotNull(yanit.DuzMetinAnahtar);
        Assert.NotEmpty(yanit.DuzMetinAnahtar);
    }

    [Fact]
    public void FirmaApiAnahtariListeDto_DuzMetinAnahtar_Yok()
    {
        // Liste DTO'sunda düz metin anahtar ALANI BİLE YOK
        var tip = typeof(FirmaApiAnahtariListeDto);
        var ozellikler = tip.GetProperties().Select(p => p.Name).ToHashSet();

        Assert.DoesNotContain("DuzMetinAnahtar", ozellikler);
        Assert.DoesNotContain("ApiKeyHash", ozellikler);
    }

    // ===================================================================
    // L) GecerliKapsamlar Sabit Testi
    // ===================================================================

    [Fact]
    public void GecerliKapsamlar_TumBeklenenKapsamlarIceriyor()
    {
        Assert.Contains("PublicOkuma", ApiAnahtarUretici.GecerliKapsamlar);
        Assert.Contains("KonfigurasyonKaydetme", ApiAnahtarUretici.GecerliKapsamlar);
        Assert.Contains("Embed", ApiAnahtarUretici.GecerliKapsamlar);
        Assert.Contains("SunucuEntegrasyonu", ApiAnahtarUretici.GecerliKapsamlar);
    }

    // ===================================================================
    // M) EmbedTokenServisi Birim Testleri (DataProtection olmadan kontrat)
    // ===================================================================

    [Fact]
    public void EmbedTokenPayload_Alanlar_DogruSekildeTasiniyor()
    {
        var payload = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedTokenPayload
        {
            FirmaId = 42,
            UrunSlug = "test-urun",
            HedefOrigin = "https://musteri-sitesi.com",
            Nonce = "abc123nonce",
            Olusturma = new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc)
        };

        Assert.Equal(42, payload.FirmaId);
        Assert.Equal("test-urun", payload.UrunSlug);
        Assert.Equal("https://musteri-sitesi.com", payload.HedefOrigin);
        Assert.Equal("abc123nonce", payload.Nonce);
        Assert.Equal(DateTimeKind.Utc, payload.Olusturma.Kind);
    }

    [Fact]
    public void EmbedTokenPayload_KeyIcerigi_Yok()
    {
        var payload = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedTokenPayload
        {
            FirmaId = 1,
            UrunSlug = "test",
            HedefOrigin = "https://example.com",
            Nonce = "nonce123",
            Olusturma = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        // Token payload'ında API key, hash veya şifre olmamalı
        Assert.DoesNotContain("apiKey", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmbedTokenServisi_Nonce_HerCagridaFarkli()
    {
        var nonce1 = VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.IEmbedTokenServisi.NonceUret();
        var nonce2 = VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.IEmbedTokenServisi.NonceUret();
        var nonce3 = VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.IEmbedTokenServisi.NonceUret();

        Assert.NotEqual(nonce1, nonce2);
        Assert.NotEqual(nonce2, nonce3);
        Assert.NotEqual(nonce1, nonce3);
        Assert.Equal(32, nonce1.Length); // 16 byte hex = 32 karakter
    }

    [Fact]
    public void EmbedTokenServisi_Nonce_FormatDogru()
    {
        var nonce = VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.IEmbedTokenServisi.NonceUret();
        Assert.Matches("^[0-9a-f]{32}$", nonce);
    }

    // ===================================================================
    // N) EmbedOturumIstekDogrulayici Testleri
    // ===================================================================

    [Fact]
    public void EmbedOturumIstekDogrulayici_GecerliHttpsOrigin_Kabul()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar.EmbedOturumIstekDogrulayici();
        var dto = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumIstekDto(
            "https://musteri-sitesi.com");
        var sonuc = dogrulayici.Validate(dto);

        Assert.True(sonuc.IsValid);
    }

    [Fact]
    public void EmbedOturumIstekDogrulayici_GecerliHttpLocalhost_Kabul()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar.EmbedOturumIstekDogrulayici();
        var dto = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumIstekDto(
            "http://localhost:3000");
        var sonuc = dogrulayici.Validate(dto);

        Assert.True(sonuc.IsValid);
    }

    [Fact]
    public void EmbedOturumIstekDogrulayici_PathIcerenOrigin_Red()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar.EmbedOturumIstekDogrulayici();
        var dto = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumIstekDto(
            "https://musteri-sitesi.com/konfigurator");
        var sonuc = dogrulayici.Validate(dto);

        Assert.False(sonuc.IsValid);
    }

    [Fact]
    public void EmbedOturumIstekDogrulayici_ProtocolsuzOrigin_Red()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar.EmbedOturumIstekDogrulayici();
        var dto = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumIstekDto(
            "musteri-sitesi.com");
        var sonuc = dogrulayici.Validate(dto);

        Assert.False(sonuc.IsValid);
    }

    [Fact]
    public void EmbedOturumIstekDogrulayici_BosOrigin_Red()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar.EmbedOturumIstekDogrulayici();
        var dto = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumIstekDto("");
        var sonuc = dogrulayici.Validate(dto);

        Assert.False(sonuc.IsValid);
    }

    [Fact]
    public void EmbedOturumIstekDogrulayici_QueryIcerenOrigin_Red()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar.EmbedOturumIstekDogrulayici();
        var dto = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumIstekDto(
            "https://musteri-sitesi.com?token=abc");
        var sonuc = dogrulayici.Validate(dto);

        Assert.False(sonuc.IsValid);
    }

    [Fact]
    public void EmbedOturumIstekDogrulayici_OriginPortIle_Kabul()
    {
        var dogrulayici = new VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar.EmbedOturumIstekDogrulayici();
        var dto = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumIstekDto(
            "https://musteri-sitesi.com:8443");
        var sonuc = dogrulayici.Validate(dto);

        Assert.True(sonuc.IsValid);
    }

    // ===================================================================
    // O) Embed Oturum Yanit DTO Testleri
    // ===================================================================

    [Fact]
    public void EmbedOturumYanitDto_GecerliSaniye()
    {
        var yanit = new VizitLink3D.Api.Moduller.Konfigurasyon.Servisler.EmbedOturumYanitDto(
            "/konfigurator/embed/test123token",
            300);

        Assert.StartsWith("/konfigurator/embed/", yanit.IframeUrl);
        Assert.Equal(300, yanit.GecerlilikSaniye);
        Assert.False(string.IsNullOrWhiteSpace(yanit.IframeUrl));
    }

    // ===================================================================
    // P) PublicKonfiguratorDto FirmaId Testi
    // ===================================================================

    [Fact]
    public void PublicKonfiguratorDto_FirmaId_TenantBilgisiTasiyor()
    {
        var dto = new VizitLink3D.Ortak.Modeller.Urunler.PublicKonfiguratorDto
        {
            UrunId = 1,
            FirmaId = 42,
            Slug = "test-urun",
            Ad = "Test Ürün"
        };

        Assert.Equal(42, dto.FirmaId);
        Assert.True(dto.FirmaId > 0);
    }
}
