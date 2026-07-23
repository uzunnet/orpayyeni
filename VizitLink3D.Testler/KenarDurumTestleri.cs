using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Testler;

/// <summary>
/// Kenar durum ve tamamlayıcı testler.
/// </summary>
public class KenarDurumTestleri
{
    [Fact]
    public void KapakModeli_SilindiMiSonrasi_SilinmeTarihiAtanmali()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "Test", Slug = "test" };
        m.SilindiMi = true;
        m.SilinmeTarihi = DateTime.UtcNow;

        Assert.True(m.SilindiMi);
        Assert.NotNull(m.SilinmeTarihi);
    }

    [Fact]
    public void ProjeResim_ProjesizDahiDogruCalismali()
    {
        var r = new ProjeResim { Url = "/test.jpg", ProjeId = 1 };
        Assert.Equal("/test.jpg", r.Url);
        Assert.Equal(1, r.ProjeId);
    }

    [Fact]
    public void HaberResim_HaberYazisiOlmadanVarOlabilirMi()
    {
        var r = new HaberResim { HaberYazisiId = 1, ResimUrl = "/img.jpg", Sira = 1 };
        Assert.Equal(1, r.HaberYazisiId);
        Assert.Equal("/img.jpg", r.ResimUrl);
    }

    [Fact]
    public void MenuOgesi_HiyerarsikYapi_DerinOlabilirMi()
    {
        var l1 = new MenuOgesi { Baslik = "L1" };
        var l2 = new MenuOgesi { Baslik = "L2", UstMenu = l1 };
        l1.AltMenuler = new List<MenuOgesi> { l2 };

        Assert.Single(l1.AltMenuler);
        Assert.Equal(l1, l2.UstMenu);
    }

    [Fact]
    public void Ceviri_BosDeger_DogruluklaSaklanabilirMi()
    {
        var c = new Ceviri { Anahtar = "test.bos", Dil = "tr", Deger = "" };
        Assert.Equal("", c.Deger);
    }

    [Fact]
    public void SistemAyari_JsonTipi_DegerSaklayabilirMi()
    {
        var a = new SistemAyari
        {
            Anahtar = "site.renkler",
            Deger = "{\"ana\":\"#000\",\"ikincil\":\"#fff\"}",
            Tip = "json"
        };
        Assert.Contains("ana", a.Deger);
    }

    [Fact]
    public void Sertifika_GecersizHaleGeldiginde_NasilIsaretlenir()
    {
        var s = new Sertifika
        {
            Ad = "ISO 9001",
            GecerlilikTarihi = DateTime.UtcNow.AddDays(-1)
        };
        Assert.True(s.GecerlilikTarihi < DateTime.UtcNow);
    }

    [Fact]
    public void TanitimVideo_SureSaniye_VideoUzunluguDogruMu()
    {
        var v = new TanitimVideo { Baslik = "Test", SureSaniye = 180 };
        Assert.Equal(180, v.SureSaniye);
    }

    [Fact]
    public void EpostaSablonu_IcerikHtml_UzunMetinSaklayabilirMi()
    {
        var s = new EpostaSablonu
        {
            Ad = "Hoş Geldiniz",
            Konu = "VizitLink3D'a Hoş Geldiniz",
            IcerikHtml = "<h1>Merhaba</h1><p>Uzun içerik...</p>"
        };
        Assert.Contains("<h1>", s.IcerikHtml);
    }

    [Fact]
    public void Sube_CalismaSaatleri_MetinOlarakSaklanabilirMi()
    {
        var s = new Sube
        {
            Ad = "Bursa Showroom",
            CalismaSaatleri = "Pzt-Cum: 09:00-18:00, Cmt: 09:00-13:00"
        };
        Assert.Contains("09:00", s.CalismaSaatleri);
    }
}
