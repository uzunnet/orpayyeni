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

    [Theory]
    [InlineData("/MEDYA/ANASAYFA-SLAYT-1.PNG", "/medya/anasayfa-slayt-1.webp")]
    [InlineData("/Medya/Anasayfa-Slayt-2.png", "/medya/anasayfa-slayt-2.webp")]
    public void Guncelle_BuyukKucukHarfFarkindaDaDonusturur(string giris, string beklenen)
    {
        Assert.Equal(beklenen, AnaSayfaSlaytYolu.Guncelle(giris));
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
