using VizitLink3D.Ortak.Yardimcilar;

namespace VizitLink3D.Testler;

public sealed class OrpaySssIcerigiTestleri
{
    [Fact]
    public void OnUcAdetOrpaySssKaydiVardir()
    {
        Assert.Equal(13, OrpaySssIcerigi.Kayitlar.Count);
    }

    [Fact]
    public void TumSorularDoluOlmalidir()
    {
        Assert.All(OrpaySssIcerigi.Kayitlar, kayit => Assert.False(string.IsNullOrWhiteSpace(kayit.Soru)));
    }

    [Fact]
    public void TumCevaplarDoluOlmalidir()
    {
        Assert.All(OrpaySssIcerigi.Kayitlar, kayit => Assert.False(string.IsNullOrWhiteSpace(kayit.Cevap)));
    }

    [Fact]
    public void EskiFirmaAdiIcermez()
    {
        Assert.DoesNotContain(OrpaySssIcerigi.Kayitlar, kayit =>
            kayit.Soru.Contains("VizitLink3D", StringComparison.OrdinalIgnoreCase)
            || kayit.Cevap.Contains("VizitLink3D", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void HerKaydinKategorisiVardir()
    {
        Assert.All(OrpaySssIcerigi.Kayitlar, kayit => Assert.False(string.IsNullOrWhiteSpace(kayit.KategoriAdi)));
    }
}
