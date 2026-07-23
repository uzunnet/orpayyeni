using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Medya Havuzu model testleri.
/// </summary>
public class MedyaModelTestleri
{
    [Fact]
    public void Medya_VarsayilanTip_ResimOlmali()
    {
        var m = new Medya();
        Assert.Equal(MedyaTipi.Resim, m.Tip);
        Assert.Equal(MedyaKaynagi.Yerel, m.Kaynak);
    }

    [Fact]
    public void Medya_SoftDelete_VarsayilanFalseOlmali()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.False(m.SilindiMi);
        Assert.Null(m.SilinmeTarihi);
    }

    [Fact]
    public void Medya_KullanimSayisi_SifirdanBaslamali()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Equal(0, m.KullanimSayisi);
    }

    [Fact]
    public void MedyaKlasoru_VarsayilanDegerler_DogruOlmali()
    {
        var k = new MedyaKlasoru { Ad = "Test Klasör", Slug = "test" };
        Assert.True(k.AktifMi);
        Assert.Equal(0, k.SiraNo);
        Assert.Empty(k.AltKlasorler);
        Assert.Empty(k.Medyalar);
    }

    [Fact]
    public void MedyaKullanim_GerekliAlanlar_DoluOlmali()
    {
        var k = new MedyaKullanim
        {
            MedyaId = 1,
            EntiteAdi = "KapiModeli",
            EntiteId = 42,
            AlanAdi = "KapakResim"
        };

        Assert.Equal(1, k.MedyaId);
        Assert.Equal("KapiModeli", k.EntiteAdi);
        Assert.Equal(42, k.EntiteId);
    }

    [Fact]
    public void MedyaTipi_EnumDegerleri_DogruMu()
    {
        Assert.Equal(0, (int)MedyaTipi.Resim);
        Assert.Equal(1, (int)MedyaTipi.Video);
        Assert.Equal(2, (int)MedyaTipi.Pdf);
        Assert.Equal(3, (int)MedyaTipi.Glb);
        Assert.Equal(4, (int)MedyaTipi.Ses);
        Assert.Equal(5, (int)MedyaTipi.Diger);
    }

    [Fact]
    public void MedyaKaynagi_EnumDegerleri_DogruMu()
    {
        Assert.Equal(0, (int)MedyaKaynagi.Yerel);
        Assert.Equal(1, (int)MedyaKaynagi.Youtube);
        Assert.Equal(2, (int)MedyaKaynagi.Vimeo);
        Assert.Equal(3, (int)MedyaKaynagi.Url);
        Assert.Equal(4, (int)MedyaKaynagi.AIUretim);
        Assert.Equal(5, (int)MedyaKaynagi.StokFotograf);
    }
}
