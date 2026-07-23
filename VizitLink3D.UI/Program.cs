using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using VizitLink3D.UI;
using VizitLink3D.UI.Servisler;
using Gotho.BlazorPdf;
using MudBlazor.Extensions;
using MudExtensions.Services;
using MudBlazor.Services;

var yapici = WebAssemblyHostBuilder.CreateDefault(args);
yapici.RootComponents.Add<App>("#app");
yapici.RootComponents.Add<HeadOutlet>("head::after");

var temelAdres = yapici.HostEnvironment.BaseAddress;
var yapilandirmaAdresi = yapici.Configuration["ApiTemelUrl"];
var temelUri = new Uri(temelAdres);
var host = temelUri.Host.ToLowerInvariant();

static bool YerelAgAdresiMi(string hst)
{
    if (hst.StartsWith("192.168.") || hst.StartsWith("10."))
        return true;
    if (hst.StartsWith("172."))
    {
        var parcalar = hst.Split('.');
        if (parcalar.Length > 1 && int.TryParse(parcalar[1], out var ikinciKisim))
            return ikinciKisim >= 16 && ikinciKisim <= 31;
    }
    return false;
}

string apiUrl;
if (host == "localhost" || host == "127.0.0.1")
{
    apiUrl = !string.IsNullOrEmpty(yapilandirmaAdresi) ? yapilandirmaAdresi : "http://localhost:5215";
}
else if (YerelAgAdresiMi(host))
{
    // Private IP (mobil/ag) — ayni host + 5215
    apiUrl = $"http://{host}:5215";
}
else
{
    // Production domain — same-origin (nginx /api proxy)
    apiUrl = temelAdres.TrimEnd('/') + "/";
}

yapici.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(yapici.HostEnvironment.BaseAddress) });
yapici.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

yapici.Services.AddScoped<ApiIstemcisi>(sp =>
{
    var istemci = new HttpClient { BaseAddress = new Uri(apiUrl) };
    return new ApiIstemcisi(istemci, sp.GetRequiredService<IJSRuntime>());
});

yapici.Services.AddMudServices();
yapici.Services.AddMudExtensions();
yapici.Services.AddBlazorPdfViewer();
yapici.Services.AddValidation();

yapici.Services.AddScoped<DilServisi>();
yapici.Services.AddScoped<KimlikServisi>();
yapici.Services.AddScoped<FirmaBilgisiServisi>();
yapici.Services.AddScoped<UcBoyutServisi>();
yapici.Services.AddScoped<AnimasyonMotoruServisi>();
yapici.Services.AddScoped<BildirimServisi>();
yapici.Services.AddScoped<AdminCeviriServisi>();

await yapici.Build().RunAsync();
