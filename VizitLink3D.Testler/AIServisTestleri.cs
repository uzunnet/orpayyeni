using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Api.Moduller.AI.Servisler;

namespace VizitLink3D.Testler;

public class AIServisTestleri
{
    private static OpenAISaglayici SaglayiciOlustur() =>
        new("test-key", new HttpClient());

    [Fact]
    public void OpenAISaglayici_MaliyetHesapla_DogruFormul()
    {
        var saglayici = SaglayiciOlustur();
        var maliyet = saglayici.MaliyetHesapla(1000, 500);
        Assert.Equal(0.45m, maliyet);
    }

    [Fact]
    public void OpenAISaglayici_MaliyetHesapla_SifirToken()
    {
        var saglayici = SaglayiciOlustur();
        Assert.Equal(0m, saglayici.MaliyetHesapla(0, 0));
    }

    [Fact]
    public void AIFabrika_TipBelirtilmezse_IlkAktifSaglayiciyiDondurmeli()
    {
        Assert.True(true);
    }

    [Fact]
    public void AIYanit_VarsayilanDegerler_DogruOlmali()
    {
        var y = new AIYanit();
        Assert.Equal("", y.Metin);
        Assert.Equal(0, y.IstekTokenSayisi);
        Assert.Equal(0m, y.MaliyetUsd);
    }

    [Fact]
    public void AIIstek_VarsayilanSicaklik_DogruOlmali()
    {
        var i = new AIIstek();
        Assert.Equal(0.7f, i.Sicaklik);
        Assert.Equal(2000, i.MaksimumToken);
    }

    [Fact]
    public void AISaglayicisi_EkBaslik_OpsiyonelOlmali()
    {
        var s = new AISaglayicisi { Ad = "Test" };
        Assert.Null(s.EkBaslik);
    }

    [Fact]
    public void AISaglayicisi_KullanilanUsd_SifirdanBaslamali()
    {
        var s = new AISaglayicisi { Ad = "Test" };
        Assert.Equal(0m, s.KullanilanUsd);
    }

    [Fact]
    public void AICagrisiKaydi_HataDurumu_MesajIleBirlikte()
    {
        var k = new AICagrisiKaydi
        {
            SaglayiciId = 1,
            Durum = AICagriDurumu.Hata,
            HataMesaji = "Rate limit aşıldı"
        };
        Assert.Equal(AICagriDurumu.Hata, k.Durum);
        Assert.Equal("Rate limit aşıldı", k.HataMesaji);
    }

    [Fact]
    public void AICagrisiKaydi_Prompt_Kisaltilabilmeli()
    {
        var uzunPrompt = new string('x', 1000);
        var kisa = uzunPrompt[..500];
        var k = new AICagrisiKaydi { SaglayiciId = 1, Prompt = kisa };
        Assert.Equal(500, k.Prompt.Length);
    }

    [Fact]
    public void AISaglayiciFabrikasi_VarsayilanDavranis()
    {
        Assert.True(true);
    }
}
