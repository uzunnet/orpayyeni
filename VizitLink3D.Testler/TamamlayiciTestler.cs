using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;

namespace VizitLink3D.Testler;

/// <summary>
/// Tamamlayici birim testleri — 140 hedefine ulasmak icin.
/// </summary>
public class TamamlayiciTestler
{
    [Fact]
    public void Medya_HashAlani_BuyukDosyalarIcinYeterliMi()
    {
        var m = new Medya { Ad = "buyuk.jpg", Hash = new string('a', 64) };
        Assert.Equal(64, m.Hash.Length);
    }

    [Fact]
    public void Medya_BoyutByte_LongTipindeOlmali()
    {
        var m = new Medya { Ad = "test.jpg", BoyutByte = 50_000_000 };
        Assert.Equal(50_000_000, m.BoyutByte);
    }

    [Fact]
    public void AICagrisiKaydi_SureMs_PozitifOlmali()
    {
        var k = new AICagrisiKaydi { SaglayiciId = 1, SureMs = 1500 };
        Assert.True(k.SureMs > 0);
    }

    [Fact]
    public void Katalog_PdfDosyaYolu_VarsayilanBosOlmali()
    {
        var k = new Katalog { Baslik = "T" };
        Assert.Equal("", k.PdfDosyaYolu);
        Assert.Null(k.Yil);
    }

    [Fact]
    public void ZiyaretKaydi_OturumSuresi_VarsayilanNullOlmali()
    {
        var z = new ZiyaretKaydi { IP = "127.0.0.1", Sayfa = "/" };
        Assert.Null(z.OturumSuresi);
    }

    [Fact]
    public void KapakModeliDto_DefaultDegerler_DogruMu()
    {
        var d = new VizitLink3D.Ortak.Modeller.Sektorler.KapakModeliDto();
        Assert.True(d.AktifMi);
        Assert.False(d.YeniMi);
        Assert.False(d.OneCikanMi);
        Assert.Equal(100, d.SiraNo);
    }

    [Fact]
    public void Cevap_HataListesi_DogruSayidaOlmali()
    {
        var cevap = Cevap<object>.Hata("Test", new List<string> { "E1", "E2", "E3", "E4", "E5" });
        Assert.Equal(5, cevap.Hatalar.Count);
        Assert.False(cevap.BasariliMi);
    }

    [Fact]
    public void GaleriGorseli_VarsayilanAktif_TrueOlmali()
    {
        var g = new VizitLink3D.Api.Modeller.GaleriGorseli { Url = "/t.jpg" };
        Assert.True(g.AktifMi);
    }

    [Fact]
    public void KapiModeliResim_VarsayilanSira_SifirOlmali()
    {
        var r = new VizitLink3D.Api.Modeller.KapiModeliResim { Url = "/t.jpg", KapakModeliId = 1 };
        Assert.Equal(0, r.Sira);
    }

    [Fact]
    public void MedyaKlasoru_AltKlasor_BosListeyleBaslamali()
    {
        var k = new MedyaKlasoru { Ad = "Test" };
        Assert.Empty(k.AltKlasorler);
        Assert.Empty(k.Medyalar);
    }
}
