using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Mobilya modelleri ve kalan kapsam testleri.
/// </summary>
public class MobilyaTestleri
{
    [Fact]
    public void MobilyaKategorisi_VarsayilanAktif_TrueOlmali()
    {
        var k = new MobilyaKategorisi { Ad = "Mutfak", Slug = "mutfak" };
        Assert.True(k.AktifMi);
        Assert.Equal(0, k.SiraNo);
    }

    [Fact]
    public void MobilyaKategorisi_SeoAlanlari_BaslangictaNullOlmali()
    {
        var k = new MobilyaKategorisi { Ad = "Mutfak", Slug = "mutfak" };
        Assert.Null(k.SeoBaslik);
        Assert.Null(k.SeoAciklama);
    }

    [Fact]
    public void MobilyaKategorisiYerellestirme_Dil_DogruAtanmali()
    {
        var y = new MobilyaKategorisiYerellestirme
        {
            MobilyaKategorisiId = 1,
            Dil = "en",
            Ad = "Kitchen"
        };
        Assert.Equal("en", y.Dil);
        Assert.Equal("Kitchen", y.Ad);
    }

    [Fact]
    public void MobilyaUrunu_VarsayilanOneCikan_FalseOlmali()
    {
        var u = new MobilyaUrunu { Ad = "Dolap", Slug = "dolap" };
        Assert.False(u.OneCikanMi);
        Assert.True(u.AktifMi);
    }

    [Fact]
    public void MobilyaUrunu_GaleriResimleriJson_BaslangictaNullOlmali()
    {
        var u = new MobilyaUrunu { Ad = "Dolap", Slug = "dolap" };
        Assert.Null(u.GaleriResimleriJson);
        Assert.Null(u.AnaGorselUrl);
    }

    [Fact]
    public void MobilyaUrunuYerellestirme_DilVeId_DogruAtanmali()
    {
        var y = new MobilyaUrunuYerellestirme
        {
            MobilyaUrunuId = 5,
            Dil = "tr",
            Ad = "Mutfak Dolabı"
        };
        Assert.Equal(5, y.MobilyaUrunuId);
        Assert.Equal("tr", y.Dil);
    }

    [Fact]
    public void KapiKategorisiYerellestirme_DilVeId_DogruAtanmali()
    {
        var y = new KapiKategorisiYerellestirme
        {
            KapiKategorisiId = 2,
            Dil = "en",
            Ad = "Membrane"
        };
        Assert.Equal(2, y.KapiKategorisiId);
        Assert.Equal("en", y.Dil);
    }

    [Fact]
    public void KapiModeliYerellestirme_BosDil_DogruAtanmali()
    {
        var y = new VizitLink3D.Api.Modeller.KapiModeliYerellestirme
        {
            KapakModeliId = 1,
            Dil = "ar",
            Ad = "باب"
        };
        Assert.Equal("ar", y.Dil);
        Assert.Equal(1, y.KapakModeliId);
    }

    [Fact]
    public void Kategori_UstKategoriId_VarsayilanNullOlmali()
    {
        var k = new Kategori { Ad = "Test" };
        Assert.Null(k.UstKategoriId);
        Assert.Equal("", k.Slug);
    }

    [Fact]
    public void SayfaIcerigi_Dil_DuzgunSaklanmali()
    {
        var s = new SayfaIcerigi
        {
            Bolum = "hakkimizda",
            Anahtar = "vizyon",
            Dil = "tr",
            Deger = "Vizyonumuz"
        };
        Assert.Equal("Vizyonumuz", s.Deger);
    }
}
