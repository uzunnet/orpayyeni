namespace VizitLink3D.Ortak.Yardimcilar;

public static class MobilMenuGorunumYardimcisi
{
    public const string KapaliMenuSinifi = "orpay-mobil-menu";
    public const string AcikMenuSinifi = "orpay-mobil-menu orpay-mobil-menu--acik";
    public const string KapaliDugmeSinifi = "orpay-mobil-menu-dugme";
    public const string AcikDugmeSinifi = "orpay-mobil-menu-dugme orpay-mobil-menu-dugme--acik";

    public static string MenuSinifi(bool acik) => acik ? AcikMenuSinifi : KapaliMenuSinifi;

    public static string DugmeSinifi(bool acik) => acik ? AcikDugmeSinifi : KapaliDugmeSinifi;
}
