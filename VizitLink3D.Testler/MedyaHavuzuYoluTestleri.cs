using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.Ortak.Yardimcilar;

namespace VizitLink3D.Testler;

public class MedyaHavuzuYoluTestleri
{
    private const string ApiTabanUrl = "https://orpay.uzunreklam.com";

    [Fact]
    public void UrunGalerisiOlustur_AnaGorseliIlkSirayaKoyar()
    {
        var urun = new Urun { AnaGorselMedyaId = 42 };

        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(urun, [], ApiTabanUrl);

        Assert.Equal($"{ApiTabanUrl}/api/medya/dosya/42", Assert.Single(sonuc));
    }

    [Fact]
    public void UrunGalerisiOlustur_HavuzGalerisiniSirasiylaEkler()
    {
        var medyalar = new[]
        {
            Medya("/api/medya/dosya/8", 2),
            Medya("https://eski.example.com/api/medya/dosya/7", 1)
        };

        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(new Urun(), medyalar, ApiTabanUrl);

        Assert.Equal([$"{ApiTabanUrl}/api/medya/dosya/7", $"{ApiTabanUrl}/api/medya/dosya/8"], sonuc);
    }

    [Fact]
    public void UrunGalerisiOlustur_TekrarlananAnaGorseliBirKezDondurur()
    {
        var urun = new Urun { AnaGorselMedyaId = 5 };

        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(urun, [Medya("/api/medya/dosya/5")], ApiTabanUrl);

        Assert.Single(sonuc);
    }

    [Fact]
    public void UrunGalerisiOlustur_UrunFotograflariVarkenKatalogAnaGorseliniDahilEtmez()
    {
        var urun = new Urun { AnaGorselMedyaId = 3 };

        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(
            urun,
            [Medya("/medya/orpay-katalog/bottega-120-real-1.webp", 1), Medya("/medya/orpay-katalog/bottega-120-real-2.webp", 2)],
            ApiTabanUrl);

        Assert.Equal(
            ["/medya/orpay-katalog/bottega-120-real-1.webp", "/medya/orpay-katalog/bottega-120-real-2.webp"],
            sonuc);
    }

    [Fact]
    public void UrunGalerisiOlustur_OrpayKatalogUrunGorselleriniDahilEder()
    {
        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(
            new Urun(),
            [Medya("/medya/orpay-katalog/hermes-120-real-2.png")],
            ApiTabanUrl);

        Assert.Equal(["/medya/orpay-katalog/hermes-120-real-2.png"], sonuc);
    }

    [Fact]
    public void UrunGalerisiOlustur_KatalogArsivSayfalariniGaleriyeDahilEtmez()
    {
        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(
            new Urun(),
            [Medya("/medya/orpay-katalog/sayfa-024-spread.png", medyaTuru: "ResimArsiv")],
            ApiTabanUrl);

        Assert.Empty(sonuc);
    }

    [Fact]
    public void UrunGalerisiOlustur_HariciWordPressYollariniDahilEtmez()
    {
        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(
            new Urun(),
            [Medya("https://www.orpayormanurunleri.com.tr/wp-content/uploads/orpay-40.jpg")],
            ApiTabanUrl);

        Assert.Empty(sonuc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/api/medya/dosya/0")]
    [InlineData("/api/medya/dosya/-1")]
    [InlineData("/api/medya/dosya/12/yanlis")]
    [InlineData("/api/medya/dosya/resim.png")]
    public void HavuzDosyaYoluMu_GecersizYollariReddeder(string? yol)
    {
        Assert.False(MedyaHavuzuYolu.HavuzDosyaYoluMu(yol));
    }

    [Fact]
    public void UrunGalerisiOlustur_SilinmisMedyayiDahilEtmez()
    {
        var medya = Medya("/api/medya/dosya/9");
        medya.SilindiMi = true;

        var sonuc = MedyaHavuzuYolu.UrunGalerisiOlustur(new Urun(), [medya], ApiTabanUrl);

        Assert.Empty(sonuc);
    }

    private static UrunMedya Medya(string yol, int siraNo = 1, string medyaTuru = "Resim")
    {
        return new UrunMedya
        {
            MedyaUrl = yol,
            MedyaTuru = medyaTuru,
            SiraNo = siraNo
        };
    }
}
