namespace VizitLink3D.Api.AraYazilimlar;

public class ResimOptimizasyonMiddleware
{
    private readonly RequestDelegate _sonraki;

    public ResimOptimizasyonMiddleware(RequestDelegate sonraki) => _sonraki = sonraki;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/medya"))
        {
            context.Response.Headers.CacheControl = "public, max-age=604800";
        }
        await _sonraki(context);
    }
}

public static class ResimOptimizasyonMiddlewareUzantilari
{
    public static IApplicationBuilder ResimOptimizasyonKullan(this IApplicationBuilder builder)
        => builder.UseMiddleware<ResimOptimizasyonMiddleware>();
}
