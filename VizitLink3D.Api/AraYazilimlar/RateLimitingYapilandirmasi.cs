using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace VizitLink3D.Api.AraYazilimlar;

public static class RateLimitingYapilandirmasi
{
    public static IServiceCollection RateLimitingEkle(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Genel", opt =>
            {
                opt.PermitLimit = 1000;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            options.AddFixedWindowLimiter("Giris", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });

            // Embed/widget istekleri: iframe kaynaklı, daha sıkı limit
            options.AddFixedWindowLimiter("Embed", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });

            // Sunucular-arası entegrasyon: daha yüksek limit, batch işlemlere uygun
            options.AddFixedWindowLimiter("Entegrasyon", opt =>
            {
                opt.PermitLimit = 300;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 20;
            });

            options.RejectionStatusCode = 429;
        });

        return services;
    }
}
