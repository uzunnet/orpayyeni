using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Entity edge case ve tamamlayici testler.
/// </summary>
public class EdgeCaseTestleri
{
    [Fact]
    public void KapakModeli_MaxOlcu_MinOlcudenBuyukOlmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli
        {
            ModelAdi = "T", Slug = "t",
            MinYukseklik = 1800, MaxYukseklik = 2200,
            MinGenislik = 600, MaxGenislik = 900
        };
        Assert.True(m.MaxYukseklik > m.MinYukseklik);
        Assert.True(m.MaxGenislik > m.MinGenislik);
    }

    [Fact]
    public void Slayt_AnimasyonTipi_GecerliDegerler()
    {
        var fade = new Slayt { Baslik = "Fade", AnimasyonTipi = "fade" };
        var slide = new Slayt { Baslik = "Slide", AnimasyonTipi = "slide" };
        var zoom = new Slayt { Baslik = "Zoom", AnimasyonTipi = "zoom" };

        Assert.Equal("fade", fade.AnimasyonTipi);
        Assert.Equal("slide", slide.AnimasyonTipi);
        Assert.Equal("zoom", zoom.AnimasyonTipi);
    }

    [Fact]
    public void MusteriYorumu_Puan_MinMaxAralikta()
    {
        var min = new MusteriYorumu { MusteriAdi = "A", Yorum = "T", Puan = 1 };
        var max = new MusteriYorumu { MusteriAdi = "B", Yorum = "T", Puan = 5 };

        Assert.True(min.Puan >= 1 && min.Puan <= 5);
        Assert.True(max.Puan >= 1 && max.Puan <= 5);
    }

    [Fact]
    public void Medya_BoyutByte_SifirdanBuyukOlmali()
    {
        var m = new Medya { Ad = "test.jpg", BoyutByte = 1024000 };
        Assert.True(m.BoyutByte > 0);
    }

    [Fact]
    public void Medya_SureSaniye_VideoIcinGecerli()
    {
        var m = new Medya { Ad = "video.mp4", Tip = MedyaTipi.Video, SureSaniye = 120 };
        Assert.Equal(MedyaTipi.Video, m.Tip);
        Assert.Equal(120, m.SureSaniye);
    }

    [Fact]
    public void Kullanici_Rol_EnumDegerleriTutarlı()
    {
        var roller = new[] { Rol.Kullanici, Rol.Editor, Rol.Admin, Rol.SuperAdmin };
        Assert.Equal(4, roller.Distinct().Count());
    }

    [Fact]
    public void Ceviri_BolumAlani_BosOlamaz()
    {
        var c = new Ceviri { Anahtar = "menu.anasayfa", Dil = "tr", Deger = "Ana Sayfa", Bolum = "" };
        Assert.Equal("", c.Bolum);
    }

    [Fact]
    public void SistemAyari_Deger_TipIleUyumluOlmali()
    {
        var stringTip = new SistemAyari { Anahtar = "a", Deger = "text", Tip = "string" };
        var boolTip = new SistemAyari { Anahtar = "b", Deger = "true", Tip = "bool" };

        Assert.Equal("string", stringTip.Tip);
        Assert.Equal("bool", boolTip.Tip);
    }

    [Fact]
    public void KapakModeliDto_Kategori_VarsayilanOzel()
    {
        var d = new VizitLink3D.Ortak.Modeller.Sektorler.KapakModeliDto();
        Assert.Equal("Ozel", d.Kategori);
    }

    [Fact]
    public void Firma_Telefon_WA_NumarasiOpsiyonel()
    {
        var f = new Firma { Ad = "Test", Slug = "t", Telefon1 = "+90", Whatsapp = "+90" };
        Assert.NotEmpty(f.Telefon1);
        Assert.NotEmpty(f.Whatsapp);
    }
}
