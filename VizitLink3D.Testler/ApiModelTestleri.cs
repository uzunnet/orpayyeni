using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// API modelleri ve son kapsam testleri.
/// </summary>
public class ApiModelTestleri
{
    [Fact]
    public void CanliSohbetMesaji_VarsayilanDegerler_DogruMu()
    {
        var m = new CanliSohbetMesaji
        {
            OturumId = "abc",
            GonderenAd = "Ziyaretci",
            MesajMetni = "Merhaba"
        };
        Assert.False(m.YoneticiMi);
        Assert.False(m.OkunduMu);
    }

    [Fact]
    public void GaleriGorseli_VarsayilanSira_SifirOlmali()
    {
        var g = new GaleriGorseli { Url = "/test.jpg" };
        Assert.Equal(0, g.Sira);
    }

    [Fact]
    public void SayfaIcerigi_CompositeKey_UcAlanli()
    {
        var s = new SayfaIcerigi
        {
            Bolum = "anasayfa",
            Anahtar = "hero.baslik",
            Dil = "tr",
            Deger = "Test"
        };
        Assert.Equal("anasayfa", s.Bolum);
        Assert.Equal("hero.baslik", s.Anahtar);
        Assert.Equal("tr", s.Dil);
    }

    [Fact]
    public void KapakModeliDto_RenkAlani_VarsayilanBosOlmali()
    {
        var d = new VizitLink3D.Ortak.Modeller.Sektorler.KapakModeliDto();
        Assert.Equal("", d.RenkAdi);
        Assert.Equal("", d.RenkHex);
    }

    [Fact]
    public void KapakModeliDto_UygulamaGorselleri_BosListeyleBaslamali()
    {
        var d = new VizitLink3D.Ortak.Modeller.Sektorler.KapakModeliDto();
        Assert.Empty(d.UygulamaGorselleri);
    }

    [Fact]
    public void Medya_GuncellenmeTarihi_BaslangictaNullOlmali()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Null(m.GuncellenmeTarihi);
    }

    [Fact]
    public void AISaglayicisi_GuncellenmeTarihi_BaslangictaNullOlmali()
    {
        var s = new AISaglayicisi { Ad = "OpenAI" };
        Assert.Null(s.GuncellenmeTarihi);
    }

    [Fact]
    public void Kullanici_ProfilResmiUrl_BaslangictaNullOlmali()
    {
        var k = new Kullanici { KullaniciAdi = "test", Eposta = "t@t.com" };
        Assert.Null(k.ProfilResmiUrl);
    }

    [Fact]
    public void Firma_YetkiliAdSoyad_VarsayilanBosOlmali()
    {
        var f = new Firma { Ad = "Test", Slug = "t" };
        Assert.Null(f.YetkiliAdSoyad);
    }

    [Fact]
    public void Sertifika_VerilmeTarihi_NullableDateTime()
    {
        var s = new Sertifika { Ad = "ISO 9001" };
        Assert.Null(s.VerilmeTarihi);
    }
}
