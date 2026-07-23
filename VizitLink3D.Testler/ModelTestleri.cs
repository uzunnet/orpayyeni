using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;
using System.Text.Json;

namespace VizitLink3D.Testler;

/// <summary>
/// Model yapisi ve serilestirme testleri.
/// </summary>
public class ModelTestleri
{
    [Fact]
    public void GaleriGorseli_VarsayilanDegerler_DogruOlmali()
    {
        var g = new GaleriGorseli { Url = "/test.jpg" };
        Assert.True(g.AktifMi);
        Assert.Equal(0, g.Sira);
    }

    [Fact]
    public void SayfaIcerigi_CompositeKey_YapisiDogruMu()
    {
        var s1 = new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "hero", Dil = "tr", Deger = "test" };
        var s2 = new SayfaIcerigi { Bolum = "anasayfa", Anahtar = "hero", Dil = "en", Deger = "test" };

        Assert.Equal(s1.Bolum, s2.Bolum);
        Assert.Equal(s1.Anahtar, s2.Anahtar);
        Assert.NotEqual(s1.Dil, s2.Dil);
    }

    [Fact]
    public void CanliSohbetMesaji_YoneticiMi_VarsayilanFalseOlmali()
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
    public void Dil_VarsayilanSecenekler_DogruOlmali()
    {
        var tr = new Dil { Kod = "tr", Ad = "Turkce", VarsayilanMi = true };
        var en = new Dil { Kod = "en", Ad = "English" };

        Assert.True(tr.VarsayilanMi);
        Assert.False(en.VarsayilanMi);
        Assert.True(tr.AktifMi);
    }

    [Fact]
    public void TanitimVideo_SureSaniye_VarsayilanDegerlerDogruMu()
    {
        var v = new TanitimVideo
        {
            Baslik = "Fabrika Turu",
            VideoUrl = "https://youtube.com/embed/xyz"
        };

        Assert.Equal(0, v.GoruntulemeSayisi);
        Assert.Equal(0, v.SiraNo);
        Assert.True(v.AktifMi);
    }

    [Fact]
    public void EpostaSablonu_VarsayilanDegerler_DogruOlmali()
    {
        var s = new EpostaSablonu
        {
            Ad = "Hos Geldiniz",
            Konu = "VizitLink3D'a Hos Geldiniz"
        };

        Assert.True(s.AktifMi);
    }

    [Fact]
    public void KapiModeliResim_SiraNo_VarsayilanSifirOlmali()
    {
        var r = new KapiModeliResim { Url = "/test.jpg", KapakModeliId = 1 };
        Assert.Equal(0, r.Sira);
    }

    [Fact]
    public void MobilyaKategorisi_VarsayilanDegerler_DogruOlmali()
    {
        var k = new MobilyaKategorisi { Ad = "Mutfak", Slug = "mutfak" };
        Assert.True(k.AktifMi);
        Assert.Equal(0, k.SiraNo);
    }

    [Fact]
    public void Referans_TipVarsayilan_BosDegilMi()
    {
        var r = new Referans { Ad = "Show TV" };
        Assert.True(r.AktifMi);
        Assert.Equal(0, r.SiraNo);
    }

    [Fact]
    public void HizmetAdimi_AdimNoVarsayilan_SifirOlmali()
    {
        var h = new HizmetAdimi { Baslik = "Olcum", AdimNo = 1 };
        Assert.True(h.AktifMi);
        Assert.Equal(0, h.SiraNo);
    }

    [Fact]
    public void Sertifika_GecerlilikTarihiOpsiyonel_NullOlmali()
    {
        var s = new Sertifika { Ad = "ISO 9001" };
        Assert.Null(s.GecerlilikTarihi);
        Assert.True(s.AktifMi);
    }

    [Fact]
    public void SikSorulanSoru_VarsayilanSayac_SifirOlmali()
    {
        var s = new SikSorulanSoru { Soru = "Test?", Cevap = "Test." };
        Assert.Equal(0, s.GoruntulemeSayisi);
        Assert.False(s.FaydaliMi);
    }
}
