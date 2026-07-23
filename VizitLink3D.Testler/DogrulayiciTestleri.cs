using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Api.Dogrulayicilar;

namespace VizitLink3D.Testler;

/// <summary>
/// FluentValidation dogrulayici birim testleri.
/// Anayasa §23.6: Her DTO icin dogrulayici zorunlu.
/// </summary>
public class DogrulayiciTestleri
{
    [Fact]
    public void KapakModeliDogrulayici_BosModelAdi_HataVermeli()
    {
        var d = new KapakModeliDogrulayici();
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "", Slug = "test", Kategori = "Kapak" };
        var sonuc = d.Validate(m);
        Assert.False(sonuc.IsValid);
        Assert.Contains(sonuc.Errors, e => e.PropertyName == "ModelAdi");
    }

    [Fact]
    public void KapakModeliDogrulayici_GecersizSlug_HataVermeli()
    {
        var d = new KapakModeliDogrulayici();
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "Test", Slug = "gecersiz slug!", Kategori = "Kapak" };
        Assert.False(d.Validate(m).IsValid);
    }

    [Fact]
    public void HaberYazisiDogrulayici_BosBaslik_HataVermeli()
    {
        var d = new HaberYazisiDogrulayici();
        var y = new HaberYazisi { Baslik = "", Slug = "test", Icerik = "icerik" };
        Assert.False(d.Validate(y).IsValid);
    }

    [Fact]
    public void HaberYazisiDogrulayici_GecerliVeri_BasariliOlmali()
    {
        var d = new HaberYazisiDogrulayici();
        var y = new HaberYazisi { Baslik = "Test Baslik", Slug = "test-baslik", Icerik = "Icerik", Ozet = "Ozet" };
        Assert.True(d.Validate(y).IsValid);
    }

    [Fact]
    public void BultenDogrulayici_GecersizEposta_HataVermeli()
    {
        var d = new BultenAbonesiDogrulayici();
        var b = new BultenAbonesi { Eposta = "gecersiz" };
        Assert.False(d.Validate(b).IsValid);
    }

    [Fact]
    public void BultenDogrulayici_GecerliEposta_BasariliOlmali()
    {
        var d = new BultenAbonesiDogrulayici();
        var b = new BultenAbonesi { Eposta = "test@3dvizitlink.com.tr" };
        Assert.True(d.Validate(b).IsValid);
    }

    [Fact]
    public void EkipUyesiDogrulayici_BosAdSoyad_HataVermeli()
    {
        var d = new EkipUyesiDogrulayici();
        var e = new EkipUyesi { AdSoyad = "", Unvan = "" };
        Assert.False(d.Validate(e).IsValid);
    }

    [Fact]
    public void SubeDogrulayici_BosTelefon_HataVermeli()
    {
        var d = new SubeDogrulayici();
        var s = new Sube { Ad = "Test Sube", Telefon = "" };
        Assert.False(d.Validate(s).IsValid);
    }

    [Fact]
    public void SertifikaDogrulayici_BosAd_HataVermeli()
    {
        var d = new SertifikaDogrulayici();
        var s = new Sertifika { Ad = "" };
        Assert.False(d.Validate(s).IsValid);
    }

    [Fact]
    public void KatalogDogrulayici_GecerliVeri_BasariliOlmali()
    {
        var d = new KatalogDogrulayici();
        var k = new Katalog { Baslik = "Test Katalog", SiraNo = 1 };
        Assert.True(d.Validate(k).IsValid);
    }

    [Fact]
    public void IletisimMesajiDogrulayici_GecerliVeri_BasariliOlmali()
    {
        var d = new IletisimMesajiDogrulayici();
        var m = new IletisimMesaji { AdSoyad = "Test", Eposta = "t@t.com", Mesaj = "Test mesaji" };
        Assert.True(d.Validate(m).IsValid);
    }

    [Fact]
    public void KullaniciDogrulayici_KisaKullaniciAdi_HataVermeli()
    {
        var d = new KullaniciDogrulayici();
        var k = new Kullanici { Eposta = "t@t.com", KullaniciAdi = "ab", AdSoyad = "Test" };
        Assert.False(d.Validate(k).IsValid);
    }
}
