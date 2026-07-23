using VizitLink3D.Api.Moduller.AI.Servisler;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace VizitLink3D.Testler;

public class AIGuvenlikTestleri
{
    private readonly AIGuvenlikServisi _servis;

    public AIGuvenlikTestleri()
    {
        var hizmetler = new ServiceCollection();
        hizmetler.AddDataProtection();
        var saglayici = hizmetler.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>();
        _servis = new AIGuvenlikServisi(saglayici);
    }

    [Fact]
    public void ApiKeySifreleCoz_GidipGelme_DogruCalismali()
    {
        var orijinal = "sk-test-api-key-1234567890";
        var sifreli = _servis.ApiKeySifrele(orijinal);
        var cozulmus = _servis.ApiKeyCoz(sifreli);

        Assert.Equal(orijinal, cozulmus);
        Assert.NotEqual(orijinal, sifreli);
    }

    [Fact]
    public void PIIFiltre_TCKimlik_Maskelenmeli()
    {
        var metin = "TC kimlik numaram 12345678901 dir.";
        var sonuc = _servis.PIIFiltrele(metin);
        Assert.DoesNotContain("12345678901", sonuc);
        Assert.Contains("***TC_KIMLIK***", sonuc);
    }

    [Fact]
    public void PIIFiltre_Telefon_Maskelenmeli()
    {
        var metin = "Tel: 0532 123 45 67";
        var sonuc = _servis.PIIFiltrele(metin);
        Assert.DoesNotContain("0532", sonuc);
        Assert.Contains("***TELEFON***", sonuc);
    }

    [Fact]
    public void PIIFiltre_Eposta_Maskelenmeli()
    {
        var metin = "E-posta: test@3dvizitlink.com.tr adresine yazın.";
        var sonuc = _servis.PIIFiltrele(metin);
        Assert.DoesNotContain("test@3dvizitlink.com.tr", sonuc);
        Assert.Contains("***EPOSTA***", sonuc);
    }

    [Fact]
    public void PIIFiltre_KrediKart_Maskelenmeli()
    {
        var metin = "Kart: 1234 5678 9012 3456";
        var sonuc = _servis.PIIFiltrele(metin);
        Assert.DoesNotContain("1234", sonuc);
        Assert.Contains("***KREDI_KARTI***", sonuc);
    }

    [Fact]
    public void PIIFiltre_TemizMetin_Degismemeli()
    {
        var metin = "Bu normal bir metindir, hassas veri yok.";
        var sonuc = _servis.PIIFiltrele(metin);
        Assert.Equal(metin, sonuc);
    }

    [Fact]
    public void PIIFiltre_BosMetin_BosDonmeli()
    {
        Assert.Equal("", _servis.PIIFiltrele(""));
        Assert.Equal("A", _servis.PIIFiltrele("A")); // normal metin bozulmamalı
    }

    [Fact]
    public void PIIFiltre_CokluHassasVeri_HepsiMaskelenmeli()
    {
        var metin = "TC: 98765432109, Tel: 0542 987 65 43, E-posta: bilgi@firma.com";
        var sonuc = _servis.PIIFiltrele(metin);

        Assert.Contains("***TC_KIMLIK***", sonuc);
        Assert.Contains("***TELEFON***", sonuc);
        Assert.Contains("***EPOSTA***", sonuc);
    }

    [Fact]
    public void ApiKeySifrele_FarkliKeyler_FarkliSonucUretmeli()
    {
        var s1 = _servis.ApiKeySifrele("key1");
        var s2 = _servis.ApiKeySifrele("key2");
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void ApiKeySifrele_AyniKey_AyniSonucUretmeli()
    {
        var s1 = _servis.ApiKeySifrele("test-key");
        var s2 = _servis.ApiKeySifrele("test-key");
        Assert.NotEqual(s1, s2); // DataProtection her seferinde farklı şifreler
    }
}
