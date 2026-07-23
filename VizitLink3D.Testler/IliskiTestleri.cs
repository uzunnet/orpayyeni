using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Entity iliski ve default degerler.
/// </summary>
public class IliskiTestleri
{
    [Fact]
    public void Lisans_FirmaIliskisi_DogruBaglanmali()
    {
        var f = new Firma { Ad = "VizitLink3D", Slug = "vizitlink3d" };
        var l = new Lisans { BirincilDomain = "3dvizitlink.com.tr", LisansAnahtari = "key", Firma = f, FirmaId = f.Id };
        Assert.Equal(f, l.Firma);
    }

    [Fact]
    public void HaberYazisi_FirmaIliskisi_DogruBaglanmali()
    {
        var f = new Firma { Ad = "Test", Slug = "t", Id = 1 };
        var b = new HaberYazisi { Baslik = "T", Slug = "t", Icerik = "i", FirmaId = f.Id, Firma = f };
        Assert.Equal(f.Id, b.FirmaId);
    }

    [Fact]
    public void IletisimMesaji_FirmaIliskisi_DogruBaglanmali()
    {
        var f = new Firma { Ad = "Test", Slug = "t", Id = 1 };
        var m = new IletisimMesaji { AdSoyad = "A", Eposta = "a@a.com", Mesaj = "M", FirmaId = f.Id, Firma = f };
        Assert.Equal(f.Id, m.FirmaId);
    }

    [Fact]
    public void Kullanici_FirmaIliskisi_DogruBaglanmali()
    {
        var f = new Firma { Ad = "Test", Slug = "t", Id = 1 };
        var k = new Kullanici { KullaniciAdi = "t", Eposta = "t@t.com", FirmaId = f.Id, Firma = f };
        Assert.Equal(f.Id, k.FirmaId);
    }

    [Fact]
    public void Kategori_FirmaIliskisi_DogruBaglanmali()
    {
        var f = new Firma { Ad = "Test", Slug = "t", Id = 1 };
        var k = new Kategori { Ad = "Test", FirmaId = f.Id, Firma = f };
        Assert.Equal(f.Id, k.FirmaId);
    }

    [Fact]
    public void MenuOgesi_FirmaIliskisi_DogruBaglanmali()
    {
        var f = new Firma { Ad = "Test", Slug = "t", Id = 1 };
        var m = new MenuOgesi { Baslik = "Test", FirmaId = f.Id, Firma = f };
        Assert.Equal(f.Id, m.FirmaId);
    }

    [Fact]
    public void Medya_FirmaId_VarsayilanNull()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Null(m.FirmaId);
    }

    [Fact]
    public void MedyaKlasoru_FirmaId_VarsayilanNull()
    {
        var k = new MedyaKlasoru { Ad = "Test" };
        Assert.Null(k.FirmaId);
    }

    [Fact]
    public void KapakModeli_KategoriId_VarsayilanNull()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "Test", Slug = "t" };
        Assert.Null(m.KategoriId);
    }

    [Fact]
    public void Proje_KategoriId_VarsayilanSifir()
    {
        var p = new Proje { Baslik = "T", Slug = "t" };
        Assert.Equal(0, p.KategoriId);
    }
}
