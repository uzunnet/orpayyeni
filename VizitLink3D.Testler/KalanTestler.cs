using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Kalan model ve validasyon testleri.
/// </summary>
public class KalanTestler
{
    [Fact]
    public void Sektor_Modeli_DogruVarsayilanDegerler()
    {
        var s = new Sektor { Ad = "Kapı", Kod = "KAPI" };
        Assert.True(s.AktifMi);
        Assert.Equal(0, s.Sira);
    }

    [Fact]
    public void HaberResim_SiraNo_DogruAtanmali()
    {
        var r = new HaberResim { HaberYazisiId = 1, ResimUrl = "/img.jpg", Sira = 5 };
        Assert.Equal(5, r.Sira);
    }

    [Fact]
    public void KapakModeliDto_OlusturulmaTarihi_DefaultDateTimeMin()
    {
        var d = new VizitLink3D.Ortak.Modeller.Sektorler.KapakModeliDto();
        Assert.Equal(default, d.OlusturulmaTarihi);
    }

    [Fact]
    public void MenuOgesi_YeniSekmede_VarsayilanFalse()
    {
        var m = new MenuOgesi { Baslik = "Test", Url = "test" };
        Assert.False(m.YeniSekmede);
    }

    [Fact]
    public void MenuOgesi_Konum_VarsayilanAnaMenu()
    {
        var m = new MenuOgesi { Baslik = "Test", Url = "test" };
        Assert.Equal("AnaMenu", m.Konum);
    }

    [Fact]
    public void MedyaKullanim_AlanAdi_VarsayilanNull()
    {
        var k = new MedyaKullanim { MedyaId = 1, EntiteAdi = "Test", EntiteId = 1 };
        Assert.Null(k.AlanAdi);
    }

    [Fact]
    public void MedyaKlasoru_Renk_VarsayilanNull()
    {
        var k = new MedyaKlasoru { Ad = "Test" };
        Assert.Null(k.Renk);
        Assert.Null(k.Ikon);
    }

    [Fact]
    public void Lisans_LisansTipi_VarsayilanYillik()
    {
        var l = new Lisans
        {
            BirincilDomain = "test.com",
            LisansAnahtari = "hmac-key"
        };
        Assert.Equal("Yillik", l.LisansTipi);
    }

    [Fact]
    public void AISaglayicisi_SiraNo_VarsayilanSifir()
    {
        var s = new AISaglayicisi { Ad = "Test" };
        Assert.Equal(0, s.SiraNo);
    }

    [Fact]
    public void Referans_WebSite_VarsayilanNull()
    {
        var r = new Referans { Ad = "Test" };
        Assert.Null(r.WebSite);
        Assert.Null(r.Aciklama);
    }
}
