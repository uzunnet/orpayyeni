using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VizitLink3D.Api.AraYazilimlar;

/// <summary>
/// Firanin ilgili module yetkisi olup olmadigini kontrol eder.
/// [ModulGereksiz("blog")] attribute'u ile kullanilir.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ModulGereksizAttribute : Attribute, IAsyncActionFilter
{
    public string ModulKodu { get; }

    public ModulGereksizAttribute(string modulKodu)
    {
        ModulKodu = modulKodu;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var firmaModulleri = context.HttpContext.Items["AktifModulKodlari"] as string;

        if (string.IsNullOrEmpty(firmaModulleri) || !firmaModulleri.Contains(ModulKodu))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
