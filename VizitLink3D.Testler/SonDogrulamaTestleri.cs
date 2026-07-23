using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Son model dogrulama testleri.
/// </summary>
public class SonDogrulamaTestleri
{
    [Fact]
    public void MedyaKullanim_SiraNo_VarsayilanSifir()
    {
        var k = new MedyaKullanim { MedyaId = 1, EntiteAdi = "Test", EntiteId = 1 };
        Assert.Equal(0, k.SiraNo);
    }

    [Fact]
    public void Medya_DosyaYolu_Ad_BagliDegil()
    {
        var m = new Medya { Ad = "kapak", DosyaYolu = "/medya/genel/abc.jpg" };
        Assert.NotEqual(m.Ad, m.DosyaYolu);
    }

    [Fact]
    public void AISaglayicisi_EkBaslik_JsonFormati()
    {
        var s = new AISaglayicisi
        {
            Ad = "OpenAI",
            EkBaslik = "{\"X-Custom\":\"deger\"}"
        };
        Assert.Contains("X-Custom", s.EkBaslik);
    }

    [Fact]
    public void AICagrisiKaydi_KullanimAmaci_MetinYazVarsayilan()
    {
        var k = new AICagrisiKaydi { SaglayiciId = 1 };
        Assert.Equal("MetinYaz", k.KullanimAmaci);
    }

    [Fact]
    public void AuditLog_EylemAlani_DogruFormatta()
    {
        var log = new AuditLog
        {
            Eylem = "KapiModeli.Guncellendi",
            EskiDeger = "{\"Ad\":\"Eski\"}",
            YeniDeger = "{\"Ad\":\"Yeni\"}"
        };
        Assert.Contains(".", log.Eylem);
        Assert.Contains("Ad", log.EskiDeger);
    }

    [Fact]
    public void AuditLog_ImzaHash_VarsayilanNull()
    {
        var log = new AuditLog { Eylem = "Test" };
        Assert.Null(log.ImzaHash);
    }

    [Fact]
    public void ZiyaretKaydi_Sehir_VarsayilanNull()
    {
        var z = new ZiyaretKaydi { IP = "127.0.0.1", Sayfa = "/" };
        Assert.Null(z.Sehir);
        Assert.Null(z.Ulke);
    }

    [Fact]
    public void Ceviri_GuncellenmeTarihi_VarsayilanNull()
    {
        var c = new Ceviri { Anahtar = "test", Dil = "tr", Deger = "test" };
        Assert.Null(c.GuncellenmeTarihi);
    }

    [Fact]
    public void Dil_Bayrak_VarsayilanNull()
    {
        var d = new Dil { Kod = "tr", Ad = "Türkçe" };
        Assert.Null(d.Bayrak);
    }

    [Fact]
    public void Lisans_SonDogrulamaTarihi_VarsayilanNull()
    {
        var l = new Lisans { BirincilDomain = "test.com", LisansAnahtari = "key" };
        Assert.Null(l.SonDogrulamaTarihi);
    }
}
