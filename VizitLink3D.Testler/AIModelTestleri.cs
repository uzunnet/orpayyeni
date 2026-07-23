using VizitLink3D.Ortak.Modeller.AI;

namespace VizitLink3D.Testler;

/// <summary>
/// AI Asistan model testleri.
/// </summary>
public class AIModelTestleri
{
    [Fact]
    public void AISaglayicisi_VarsayilanModel_Gpt4oMiniOlmali()
    {
        var s = new AISaglayicisi { Ad = "OpenAI" };
        Assert.Equal("gpt-4o-mini", s.Model);
        Assert.Equal(AISaglayiciTipi.OpenAI, s.Tip);
        Assert.Equal(100, s.AylikLimitUsd);
    }

    [Fact]
    public void AISaglayicisi_ApiKey_JsonIgnoreOlmali()
    {
        var s = new AISaglayicisi { Ad = "Test", ApiKeyEncrypted = "gizli-key" };
        var json = System.Text.Json.JsonSerializer.Serialize(s);
        Assert.DoesNotContain("gizli-key", json);
    }

    [Fact]
    public void AICagrisiKaydi_BasariliDurum_VarsayilanOlmali()
    {
        var k = new AICagrisiKaydi { SaglayiciId = 1, KullanimAmaci = "MetinYaz" };
        Assert.Equal(AICagriDurumu.Basarili, k.Durum);
        Assert.Null(k.HataMesaji);
    }

    [Fact]
    public void AICagrisiKaydi_MaliyetTakibi_DogruCalismali()
    {
        var k = new AICagrisiKaydi
        {
            SaglayiciId = 1,
            IstekTokenSayisi = 100,
            CevapTokenSayisi = 200,
            ToplamMaliyetUsd = 0.000135m
        };
        Assert.Equal(0.000135m, k.ToplamMaliyetUsd);
        Assert.Equal(100, k.IstekTokenSayisi);
    }

    [Fact]
    public void AISaglayiciTipi_Enum_DogruDegerler()
    {
        Assert.Equal(0, (int)AISaglayiciTipi.OpenAI);
        Assert.Equal(1, (int)AISaglayiciTipi.Anthropic);
        Assert.Equal(2, (int)AISaglayiciTipi.Gemini);
    }

    [Fact]
    public void AICagriDurumu_Enum_DogruDegerler()
    {
        Assert.Equal(0, (int)AICagriDurumu.Basarili);
        Assert.Equal(1, (int)AICagriDurumu.Hata);
        Assert.Equal(2, (int)AICagriDurumu.LimitAsildi);
    }

    [Fact]
    public void AISaglayicisi_SonSifirlamaTarihi_DogruVarsayilan()
    {
        var s = new AISaglayicisi { Ad = "Test" };
        Assert.True((DateTime.UtcNow - s.SonSifirlamaTarihi).TotalSeconds < 5);
    }

    [Fact]
    public void AICagrisiKaydi_IdTipi_LongOlmali()
    {
        var k = new AICagrisiKaydi { SaglayiciId = 1 };
        Assert.True(k.Id >= 0);
    }
}
