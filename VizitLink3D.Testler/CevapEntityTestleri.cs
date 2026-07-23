using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Cevap<T> ve entity model son testleri.
/// </summary>
public class CevapEntityTestleri
{
    [Fact]
    public void Cevap_Basarili_VarsayilanMesajlaCalisir()
    {
        var c = Cevap<int>.Basarili(42);
        Assert.Equal("Islem basarili.", c.Mesaj);
    }

    [Fact]
    public void Cevap_Hata_BosHataListesiyleCalisir()
    {
        var c = Cevap<string>.Hata("Hata", new List<string>());
        Assert.Empty(c.Hatalar);
    }

    [Fact]
    public void KapiKategorisi_SeoAnahtarKelimeler_VarsayilanNull()
    {
        var k = new KapiKategorisi { Ad = "Test", Slug = "t" };
        Assert.Null(k.SeoAnahtarKelimeler);
    }

    [Fact]
    public void KapakModeli_UcBoyutluModelUrl_NullBaslangic()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Null(m.ModelDosyaYolu);
    }

    [Fact]
    public void KapakModeli_TeknikOzelliklerJson_NullBaslangic()
    {
        var m = new VizitLink3D.Api.Modeller.KapakModeli { ModelAdi = "T", Slug = "t" };
        Assert.Null(m.TeknikOzelliklerJson);
    }

    [Fact]
    public void Medya_AltMetin_NullBaslangic()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Null(m.AltMetin);
        Assert.Null(m.Aciklama);
    }

    [Fact]
    public void Medya_YukleyenKullaniciId_NullBaslangic()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Null(m.YukleyenKullaniciId);
    }

    [Fact]
    public void Medya_MiniaturYolu_NullBaslangic()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Null(m.MiniaturYolu);
        Assert.Null(m.KaynakUrl);
    }

    [Fact]
    public void Medya_TipVeKaynak_VarsayilanDegerleriDogru()
    {
        var m = new Medya { Ad = "test.jpg" };
        Assert.Equal(MedyaTipi.Resim, m.Tip);
        Assert.Equal(MedyaKaynagi.Yerel, m.Kaynak);
    }

    [Fact]
    public void AICagrisiKaydi_KullaniciId_NullBaslangic()
    {
        var k = new VizitLink3D.Ortak.Modeller.AI.AICagrisiKaydi { SaglayiciId = 1 };
        Assert.Null(k.KullaniciId);
    }
}
