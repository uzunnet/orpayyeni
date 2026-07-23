using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Nihai birim testleri.
/// </summary>
public class NihaiTestler
{
    [Fact]
    public void KapiKategorisi_Ikon_VarsayilanNull()
    {
        var k = new KapiKategorisi { Ad = "Membran", Slug = "membran" };
        Assert.Null(k.Ikon);
    }

    [Fact]
    public void KapiKategorisi_KapakResim_VarsayilanNull()
    {
        var k = new KapiKategorisi { Ad = "Lake", Slug = "lake" };
        Assert.Null(k.KapakResim);
    }

    [Fact]
    public void MobilyaKategorisi_Ikon_VarsayilanNull()
    {
        var k = new MobilyaKategorisi { Ad = "Mutfak", Slug = "mutfak" };
        Assert.Null(k.Ikon);
        Assert.Null(k.KapakResim);
    }

    [Fact]
    public void ProjeKategorisi_Aciklama_VarsayilanNull()
    {
        var k = new ProjeKategorisi { Ad = "Mutfak", Slug = "mutfak" };
        Assert.Null(k.Aciklama);
    }

    [Fact]
    public void Slayt_MetinRengi_VarsayilanNull()
    {
        var s = new Slayt { Baslik = "T" };
        Assert.Null(s.MetinRengi);
    }

    [Fact]
    public void Slayt_Aciklama_VarsayilanNull()
    {
        var s = new Slayt { Baslik = "T" };
        Assert.Null(s.Aciklama);
    }

    [Fact]
    public void MusteriYorumu_Avatar_VarsayilanNull()
    {
        var y = new MusteriYorumu { MusteriAdi = "A", Yorum = "Test" };
        Assert.Null(y.Avatar);
        Assert.Null(y.MusteriUnvan);
        Assert.Null(y.MusteriSehir);
    }

    [Fact]
    public void Referans_Aciklama_VarsayilanNull()
    {
        var r = new Referans { Ad = "Show TV" };
        Assert.Null(r.Aciklama);
        Assert.Null(r.Logo);
    }

    [Fact]
    public void HizmetAdimi_Ikon_VarsayilanNull()
    {
        var h = new HizmetAdimi { Baslik = "Ölçüm", AdimNo = 1 };
        Assert.Null(h.Ikon);
    }

    [Fact]
    public void SikSorulanSoru_KategoriAdi_VarsayilanNull()
    {
        var s = new SikSorulanSoru { Soru = "Test?", Cevap = "Test." };
        Assert.Null(s.KategoriAdi);
    }
}
