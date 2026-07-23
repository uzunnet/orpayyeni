using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;

namespace VizitLink3D.Testler;

/// <summary>
/// Son entity testleri — 300 hedefi.
/// </summary>
public class SonEntityTestleri
{
    [Fact]
    public void AuditLog_CorrelationId_VarsayilanNull()
    {
        var log = new AuditLog { Eylem = "Test" };
        Assert.Null(log.CorrelationId);
    }

    [Fact]
    public void AuditLog_IPAdresi_VarsayilanNull()
    {
        var log = new AuditLog { Eylem = "Test" };
        Assert.Null(log.IPAdresi);
        Assert.Null(log.Tarayici);
    }

    [Fact]
    public void Lisans_YedekDomain_VarsayilanNull()
    {
        var l = new Lisans { BirincilDomain = "test.com", LisansAnahtari = "key" };
        Assert.Null(l.YedekDomain);
    }

    [Fact]
    public void Lisans_GuncellenmeTarihi_VarsayilanNull()
    {
        var l = new Lisans { BirincilDomain = "test.com", LisansAnahtari = "key" };
        Assert.Null(l.GuncellenmeTarihi);
    }

    [Fact]
    public void AICagrisiKaydi_Saglayici_BaslangictaNull()
    {
        var k = new AICagrisiKaydi { SaglayiciId = 1 };
        Assert.Null(k.Saglayici);
    }

    [Fact]
    public void KapiModeliResim_KapakModeli_BaslangictaNull()
    {
        var r = new VizitLink3D.Api.Modeller.KapiModeliResim { Url = "/t.jpg", KapakModeliId = 1 };
        Assert.Null(r.KapakModeli);
    }

    [Fact]
    public void ProjeResim_Proje_BaslangictaNull()
    {
        var r = new ProjeResim { Url = "/t.jpg", ProjeId = 1 };
        Assert.Null(r.Proje);
    }

    [Fact]
    public void HaberResim_HaberYazisi_BaslangictaNull()
    {
        var r = new HaberResim { HaberYazisiId = 1, ResimUrl = "/t.jpg" };
        Assert.Null(r.HaberYazisi);
    }

    [Fact]
    public void Katalog_KapakResim_VarsayilanNull()
    {
        var k = new Katalog { Baslik = "T" };
        Assert.Null(k.KapakResim);
        Assert.Null(k.Aciklama);
    }

    [Fact]
    public void Sube_Aciklama_VarsayilanNull()
    {
        var s = new Sube { Ad = "Bursa" };
        Assert.Null(s.Aciklama);
        Assert.Null(s.Ilce);
        Assert.Null(s.Enlem);
    }
}
