using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Son DTO ve audit model testleri.
/// </summary>
public class DtoAuditTestleri
{
    [Fact]
    public void EpostaSablonu_VarsayilanAktif_TrueOlmali()
    {
        var s = new EpostaSablonu { Ad = "Hoş Geldiniz" };
        Assert.True(s.AktifMi);
        Assert.Equal("", s.IcerikHtml);
    }

    [Fact]
    public void BultenAbonesi_IptalTarihi_VarsayilanNullOlmali()
    {
        var b = new BultenAbonesi { Eposta = "t@t.com" };
        Assert.Null(b.IptalTarihi);
    }

    [Fact]
    public void Sube_Eposta_VarsayilanNullOlmali()
    {
        var s = new Sube { Ad = "Bursa" };
        Assert.Null(s.Eposta);
    }

    [Fact]
    public void EkipUyesi_Resim_VarsayilanNullOlmali()
    {
        var e = new EkipUyesi { AdSoyad = "Ali" };
        Assert.Null(e.Resim);
        Assert.Null(e.Linkedin);
    }

    [Fact]
    public void TanitimVideo_KapakResim_VarsayilanNullOlmali()
    {
        var v = new TanitimVideo { Baslik = "Fabrika Turu" };
        Assert.Null(v.KapakResim);
        Assert.Null(v.VideoUrl);
    }

    [Fact]
    public void SikSorulanSoru_FaydaliMi_VarsayilanFalseOlmali()
    {
        var s = new SikSorulanSoru { Soru = "Test?", Cevap = "Test." };
        Assert.False(s.FaydaliMi);
    }

    [Fact]
    public void Sertifika_PdfDosya_VarsayilanNullOlmali()
    {
        var s = new Sertifika { Ad = "ISO" };
        Assert.Null(s.PdfDosya);
        Assert.Null(s.Resim);
    }

    [Fact]
    public void Katalog_SayfaSayisi_VarsayilanNullOlmali()
    {
        var k = new Katalog { Baslik = "T" };
        Assert.Null(k.SayfaSayisi);
    }

    [Fact]
    public void Kullanici_TercihEdilenDil_VarsayilanTrOlmali()
    {
        var k = new Kullanici { KullaniciAdi = "test", Eposta = "t@t.com" };
        Assert.Equal("tr", k.TercihEdilenDil);
    }

    [Fact]
    public void Firma_DemoMu_VarsayilanFalseOlmali()
    {
        var f = new Firma { Ad = "Test", Slug = "t" };
        Assert.False(f.DemoMu);
    }
}
