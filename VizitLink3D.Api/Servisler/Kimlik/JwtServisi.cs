namespace VizitLink3D.Api.Servisler.Kimlik;

public class JwtServisi(IConfiguration yapilandirma)
{
    public string Anahtar => yapilandirma["Jwt:Anahtar"]!;
    public string Yayinci => yapilandirma["Jwt:Yayinci"] ?? "VizitLink3DAPI";
    public string Izleyici => yapilandirma["Jwt:Izleyici"] ?? "VizitLink3DUI";
    public int GecerlilikSuresiDakika => int.Parse(yapilandirma["Jwt:GecerlilikSuresiDakika"] ?? "60");
}
