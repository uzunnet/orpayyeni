namespace VizitLink3D.Ortak.Yardimcilar;

public static class MobilMenuGorunumYardimcisi
{
    public const string KapaliMenuSinifi = "gb-mobil-menu";
    public const string AcikMenuSinifi = "gb-mobil-menu gb-mobil-menu--acik";
    public const string KapaliDugmeSinifi = "gb-mobil-menu-dugme";
    public const string AcikDugmeSinifi = "gb-mobil-menu-dugme gb-mobil-menu-dugme--acik";

    public static string MenuSinifi(bool acik) => acik ? AcikMenuSinifi : KapaliMenuSinifi;

    public static string DugmeSinifi(bool acik) => acik ? AcikDugmeSinifi : KapaliDugmeSinifi;
}
