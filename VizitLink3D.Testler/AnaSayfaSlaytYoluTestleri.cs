using VizitLink3D.Ortak.Yardimcilar;

namespace VizitLink3D.Testler;

public class AnaSayfaSlaytYoluTestleri
{
    [Theory]
    [InlineData("/medya/anasayfa-slayt-1.png", "/medya/anasayfa-slayt-1.webp")]
    [InlineData("/medya/anasayfa-slayt-2.png", "/medya/anasayfa-slayt-2.webp")]
    [InlineData("/medya/anasayfa-slayt-3.png", "/medya/anasayfa-slayt-3.webp")]
    public void Guncelle_EskiAnaSayfaSlaytiniWebPyeDonusturur(string eskiYol, string beklenen)
    {
        Assert.Equal(beklenen, AnaSayfaSlaytYolu.Guncelle(eskiYol));
    }

    [Fact]
    public void Guncelle_BuyukKucukHarfFarkindaDaDonusturur()
    {
        var sonuc = AnaSayfaSlaytYolu.Guncelle("/MEDYA/GOLD-KATALOG/ANASAYFA-SLAYT-1.PNG");

        Assert.Equal("/medya/anasayfa-slayt-1.webp", sonuc);
    }

    [Fact]
    public void Guncelle_DigerMedyaYolunuDegistirmez()
    {
        const string yol = "/medya/hermes-120-real-1.png";

        Assert.Equal(yol, AnaSayfaSlaytYolu.Guncelle(yol));
    }

    [Fact]
    public void Guncelle_HariciAdresiDegistirmez()
    {
        const string yol = "https://ornek.test/anasayfa-slayt-1.png";

        Assert.Equal(yol, AnaSayfaSlaytYolu.Guncelle(yol));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Guncelle_BosYoluBosDondurur(string? yol)
    {
        Assert.Equal(string.Empty, AnaSayfaSlaytYolu.Guncelle(yol));
    }
}
