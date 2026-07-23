namespace VizitLink3D.Api.Servisler;

public class KiraciServisi(IHttpContextAccessor? hca)
{
    public int? MevcutFirmaId =>
        hca?.HttpContext?.Items["FirmaId"] as int?;

    public string? MevcutDomain =>
        hca?.HttpContext?.Items["FirmaDomain"] as string;

    public string? MevcutSlug =>
        hca?.HttpContext?.Items["FirmaSlug"] as string;

    public string? MevcutAd =>
        hca?.HttpContext?.Items["FirmaAd"] as string;
}
