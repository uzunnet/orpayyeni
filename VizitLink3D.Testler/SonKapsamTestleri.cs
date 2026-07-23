using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Son kapsam ve validasyon testleri.
/// </summary>
public class SonKapsamTestleri
{
    [Fact]
    public void Cevap_HataMesaji_BosOlamaz()
    {
        var c = Cevap<int>.Hata("Bir hata oluştu");
        Assert.NotEmpty(c.Mesaj);
    }

    [Fact]
    public void Cevap_TumAlanlar_BasariliDurumdaDogru()
    {
        var c = Cevap<string>.Basarili("veri", "Tamam");
        Assert.True(c.BasariliMi);
        Assert.Equal("veri", c.Veri);
        Assert.Equal("Tamam", c.Mesaj);
        Assert.Empty(c.Hatalar);
    }

    [Fact]
    public void Firma_EnlemBoylam_VarsayilanNull()
    {
        var f = new Firma { Ad = "Test", Slug = "t" };
        Assert.Null(f.Enlem);
        Assert.Null(f.Boylam);
    }

    [Fact]
    public void Firma_AktifSablonId_VarsayilanNull()
    {
        var f = new Firma { Ad = "Test", Slug = "t" };
        Assert.Null(f.AktifSablonId);
    }

    [Fact]
    public void KapakModeli_SeoAnahtarKelimeler_VarsayilanNull()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Null(m.SeoAnahtarKelimeler);
    }

    [Fact]
    public void KapakModeli_KullanimAlanlariJson_VarsayilanNull()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Null(m.KullanimAlanlariJson);
    }

    [Fact]
    public void Slayt_ArkaplanResimMobil_VarsayilanNull()
    {
        var s = new Slayt { Baslik = "T" };
        Assert.Null(s.ArkaplanResimMobil);
    }

    [Fact]
    public void Slayt_BaslangicBitisTarihleri_Nullable()
    {
        var s = new Slayt { Baslik = "T" };
        Assert.Null(s.BaslangicTarihi);
        Assert.Null(s.BitisTarihi);
    }

    [Fact]
    public void IletisimMesaji_EtiketlerJson_VarsayilanNull()
    {
        var m = new IletisimMesaji { AdSoyad = "A", Eposta = "a@a.com", Mesaj = "M" };
        Assert.Null(m.EtiketlerJson);
    }

    [Fact]
    public void BultenAbonesi_DogrulamaToken_VarsayilanNull()
    {
        var b = new BultenAbonesi { Eposta = "t@t.com" };
        Assert.Null(b.DogrulamaToken);
        Assert.False(b.DogrulandiMi);
    }
}
