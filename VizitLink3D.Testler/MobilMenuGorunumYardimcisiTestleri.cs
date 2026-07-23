using VizitLink3D.Ortak.Yardimcilar;

namespace VizitLink3D.Testler;

public sealed class MobilMenuGorunumYardimcisiTestleri
{
    [Fact]
    public void MenuKapaliykenYalnizcaTemelSinifiDondurur()
    {
        Assert.Equal("gb-mobil-menu", MobilMenuGorunumYardimcisi.MenuSinifi(false));
    }

    [Fact]
    public void MenuAcikkenAcikSinifiniDondurur()
    {
        Assert.Equal("gb-mobil-menu gb-mobil-menu--acik", MobilMenuGorunumYardimcisi.MenuSinifi(true));
    }

    [Fact]
    public void DugmeKapaliykenYalnizcaTemelSinifiDondurur()
    {
        Assert.Equal("gb-mobil-menu-dugme", MobilMenuGorunumYardimcisi.DugmeSinifi(false));
    }

    [Fact]
    public void DugmeAcikkenAcikSinifiniDondurur()
    {
        Assert.Equal("gb-mobil-menu-dugme gb-mobil-menu-dugme--acik", MobilMenuGorunumYardimcisi.DugmeSinifi(true));
    }

    [Fact]
    public void AcikVeKapaliSiniflariBirbirindenFarklidir()
    {
        Assert.NotEqual(
            MobilMenuGorunumYardimcisi.MenuSinifi(false),
            MobilMenuGorunumYardimcisi.MenuSinifi(true));
    }
}
