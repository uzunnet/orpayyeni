using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Api.Dogrulayicilar;

namespace VizitLink3D.Testler;

/// <summary>
/// DTO ve validasyon kapsamli testleri.
/// </summary>
public class DtoValidasyonTestleri
{
    [Fact]
    public void ProjeDogrulayici_GecerliSlug_KabulEtmeli()
    {
        var d = new ProjeDogrulayici();
        var p = new Proje { Baslik = "Villa Projesi", Slug = "villa-projesi" };
        Assert.True(d.Validate(p).IsValid);
    }

    [Fact]
    public void MusteriYorumuDogrulayici_GecersizPuan_Reddetmeli()
    {
        var d = new MusteriYorumuDogrulayici();
        var y = new MusteriYorumu { MusteriAdi = "A", Yorum = "Test", Puan = 6 };
        Assert.False(d.Validate(y).IsValid);
    }

    [Fact]
    public void SlaytDogrulayici_SifirSiraNo_KabulEtmeli()
    {
        var d = new SlaytDogrulayici();
        var s = new Slayt { Baslik = "Test", SiraNo = 0 };
        Assert.True(d.Validate(s).IsValid);
    }

    [Fact]
    public void HizmetAdimiDogrulayici_SifirAdimNo_Reddetmeli()
    {
        var d = new HizmetAdimiDogrulayici();
        var h = new HizmetAdimi { Baslik = "Test", AdimNo = 0 };
        Assert.False(d.Validate(h).IsValid);
    }

    [Fact]
    public void SSSDogrulayici_BosCevap_Reddetmeli()
    {
        var d = new SSSDogrulayici();
        var s = new SikSorulanSoru { Soru = "Test?", Cevap = "" };
        Assert.False(d.Validate(s).IsValid);
    }

    [Fact]
    public void ReferansDogrulayici_BosTip_Reddetmeli()
    {
        var d = new ReferansDogrulayici();
        var r = new Referans { Ad = "Test", Tip = "" };
        Assert.False(d.Validate(r).IsValid);
    }

    [Fact]
    public void KapiKategorisiDogrulayici_GecerliSlug_KabulEtmeli()
    {
        var d = new KapiKategorisiDogrulayici();
        var k = new KapiKategorisi { Ad = "Membran", Slug = "membran" };
        Assert.True(d.Validate(k).IsValid);
    }

    [Fact]
    public void KapakModeliDogrulayici_GecerliVeri_KabulEtmeli()
    {
        var d = new KapakModeliDogrulayici();
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "Test", Slug = "test-kapak", Kategori = "Membran" };
        Assert.True(d.Validate(m).IsValid);
    }

    [Fact]
    public void HaberYazisiDogrulayici_GecersizSlug_Reddetmeli()
    {
        var d = new HaberYazisiDogrulayici();
        var y = new HaberYazisi { Baslik = "Test", Slug = "gecersiz slug!", Icerik = "x", Ozet = "x" };
        Assert.False(d.Validate(y).IsValid);
    }

    [Fact]
    public void EkipUyesiDogrulayici_GecerliVeri_KabulEtmeli()
    {
        var d = new EkipUyesiDogrulayici();
        var e = new EkipUyesi { AdSoyad = "Ali Demir", Unvan = "Mimar" };
        Assert.True(d.Validate(e).IsValid);
    }
}
