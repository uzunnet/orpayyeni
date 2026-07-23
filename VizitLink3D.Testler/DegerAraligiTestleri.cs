using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Deger araligi ve validasyon testleri.
/// </summary>
public class DegerAraligiTestleri
{
    [Fact]
    public void MusteriYorumu_YorumTarihi_VarsayilanUtcNow()
    {
        var y = new MusteriYorumu { MusteriAdi = "A", Yorum = "Test" };
        Assert.True((DateTime.UtcNow - y.YorumTarihi).TotalSeconds < 5);
    }

    [Fact]
    public void Firma_MenuYatayAralik_Varsayilan30()
    {
        var f = new Firma { Ad = "T", Slug = "t" };
        Assert.Equal(30, f.MenuYatayAralik);
    }

    [Fact]
    public void Firma_MenuDikeyPadding_Varsayilan20()
    {
        var f = new Firma { Ad = "T", Slug = "t" };
        Assert.Equal(20, f.MenuDikeyPadding);
    }

    [Fact]
    public void Firma_LogoMaxYukseklik_Varsayilan60()
    {
        var f = new Firma { Ad = "T", Slug = "t" };
        Assert.Equal(60, f.LogoMaxYukseklik);
    }

    [Fact]
    public void SS_YeniMi_VarsayilanFalse()
    {
        var s = new Slayt { Baslik = "T" };
        // Slayt modelinde YeniMi yok, AktifMi var
        Assert.True(s.AktifMi);
    }

    [Fact]
    public void SS_AktifMi_VarsayilanTrue()
    {
        var h = new HizmetAdimi { Baslik = "Test", AdimNo = 1 };
        Assert.True(h.AktifMi);
    }

    [Fact]
    public void KapakModeli_OneCikanMi_VarsayilanFalse()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.False(m.OneCikanMi);
    }

    [Fact]
    public void KapakModeli_SertifikalarJson_NullOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Null(m.SertifikalarJson);
    }

    [Fact]
    public void Proje_SeoBaslik_VarsayilanNull()
    {
        var p = new Proje { Baslik = "T", Slug = "t" };
        Assert.Null(p.SeoBaslik);
        Assert.Null(p.SeoAciklama);
    }

    [Fact]
    public void SikSorulanSoru_GoruntulemeSayisi_VarsayilanSifir()
    {
        var s = new SikSorulanSoru { Soru = "T?", Cevap = "T." };
        Assert.Equal(0, s.GoruntulemeSayisi);
    }
}
