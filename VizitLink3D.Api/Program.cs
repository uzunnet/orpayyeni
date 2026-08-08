using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Api.AraYazilimlar;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Api.Moduller.Konfigurasyon.Kontrolcu;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Net;
using System.Text;
using Microsoft.Extensions.FileProviders;
using VizitLink3D.Api.Hubs;

// Serilog yapilandirmasi (anayasa §15)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/vizitlink3d-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var yapici = WebApplication.CreateBuilder(args);
yapici.Host.UseSerilog();

// Veritabani
yapici.Services.AddHttpContextAccessor();
yapici.Services.AddDbContext<VizitLink3DDbContext>((sp, sec) =>
{
    var httpErisimi = sp.GetService<IHttpContextAccessor>();
    sec.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
       .AddInterceptors(new AuditInterceptor(httpErisimi));
    // DB yolu OnConfiguring icinde KiraciServisi ile dinamik olarak belirlenir
});

// LisansDogrulama icin her zaman ana VT'ye baglanan ayri context.
// Not: Temel sinif DbContextOptions<VizitLink3DDbContext> bekledigi icin
// AddDbContext<AnaVizitLink3DDbContext> derlenmez; options acikca uretilir.
yapici.Services.AddScoped(sp =>
{
    var secenekler = new DbContextOptionsBuilder<VizitLink3DDbContext>()
        .UseSqlite("Data Source=vizitlink3d.db")
        .Options;
    return new AnaVizitLink3DDbContext(secenekler);
});

// JWT Kimlik Dogrulama — anahtar yoksa public siteyi çökertmeden çalıştır
var jwtAnahtar = Environment.GetEnvironmentVariable("VIZITLINK3D_JWT_KEY")
    ?? yapici.Configuration["Jwt:Anahtar"];

if (!string.IsNullOrWhiteSpace(jwtAnahtar))
{
    yapici.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(sec =>
        {
            sec.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar)),
                ValidateIssuer = true,
                ValidIssuer = yapici.Configuration["Jwt:Yayinci"],
                ValidateAudience = true,
                ValidAudience = yapici.Configuration["Jwt:Izleyici"],
                ValidateLifetime = true
            };
        });
}
else
{
    Log.Warning("JWT anahtari bos oldugu icin kimlik dogrulama gecici olarak pasif.");
    yapici.Services.AddAuthentication();
}

yapici.Services.AddAuthorization();
yapici.Services.AddSignalR();

// Rate Limiting (anayasa §23.5)
yapici.Services.RateLimitingEkle();

            // CORS - Bu kurulum tek domain Orpay odaklidir, ama liste konfigden geldigi icin
// SaaS tarafina tekrar acilabilir.
var izinliDomainler = (yapici.Configuration.GetSection("Cors:IzinliDomainler").Get<string[]>()
    ?? ["http://localhost:3113", "https://localhost:3113", "http://localhost:5113", "https://localhost:5113", "http://localhost:5000", "https://localhost:5000"])
    .Concat(new[] { "http://localhost:5000", "https://localhost:5000" })
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();
yapici.Services.AddCors(sec => sec.AddDefaultPolicy(politika =>
{
    if (yapici.Environment.IsDevelopment())
    {
        politika.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        return;
    }

    politika.WithOrigins(izinliDomainler)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
}));

yapici.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
yapici.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();
yapici.Services.AddControllers();
yapici.Services.AddOpenApi();

// Onbellek ve Ceviri servisleri (anayasa §35)
yapici.Services.AddMemoryCache();
yapici.Services.AddSingleton<IOnbellekYonetici, OnbellekYonetici>();
yapici.Services.AddScoped<ICeviriServisi, CeviriServisi>();
yapici.Services.AddScoped<IOtomatikCeviriServisi, OtomatikCeviriServisi>();

// Medya Havuzu servisleri
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IDepolamaAdaptoru, VizitLink3D.Api.Moduller.Medya.Servisler.YerelDepolama>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IResimIslemcisi, VizitLink3D.Api.Moduller.Medya.Servisler.ResimIslemcisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IYoutubeMetadataServisi, VizitLink3D.Api.Moduller.Medya.Servisler.YoutubeMetadataServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Medya.Servisler.IMedyaServisi, VizitLink3D.Api.Moduller.Medya.Servisler.MedyaServisi>();

// API Key Sifreleme ve PII Filtre
yapici.Services.AddDataProtection();
yapici.Services.AddSingleton<IApiKeySifrelemeServisi, ApiKeySifrelemeServisi>();
yapici.Services.AddSingleton<IPIIFiltreServisi, PIIFiltreServisi>();

// Embed token servisi (DataProtection time-limited token)
// Scoped: EmbedNonceDeposu DbContext'e bagimli oldugu icin Singleton OLAMAZ.
// Multi-instance SaaS: DB unique constraint ile atomik one-time nonce tuketimi.
yapici.Services.AddScoped<IEmbedNonceDeposu, EmbedNonceDeposu>();
yapici.Services.AddScoped<IEmbedTokenServisi, EmbedTokenServisi>();

// Embed guvenlik ayarlari (appsettings.json → EmbedGuvenlik bolumu)
yapici.Services.Configure<EmbedGuvenlikAyarlari>(
    yapici.Configuration.GetSection("EmbedGuvenlik"));

// AI Asistan servisleri
yapici.Services.AddHttpClient();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.AIGuvenlikServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.AISaglayiciFabrikasi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.IAIMaliyetTakipServisi, VizitLink3D.Api.Moduller.AI.Servisler.AIMaliyetTakipServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.AI.Servisler.AIOrkestraServisi>();

// Resimden 3D uretim servisi (yerel Python/TripoSR servisine HTTP koprusu)
// CPU'da uretim ~1 dakika surebildigi icin varsayilan HttpClient timeout'u yetersiz kalir.
yapici.Services.AddHttpClient<VizitLink3D.Api.Moduller.UcBoyutUretim.Servisler.PythonUcBoyutSaglayici>(istemci =>
{
    istemci.Timeout = TimeSpan.FromMinutes(5);
});

// Kapak → Urun goc servisi
yapici.Services.AddScoped<KapakGocServisi>();
yapici.Services.AddScoped<UrunMedyaBaglamaServisi>();
yapici.Services.AddScoped<PdfCozumlemeServisi>();
yapici.Services.AddScoped<PdfOnizlemeServisi>();
yapici.Services.AddScoped<PdfUygulamaAlanServisi>();
yapici.Services.AddScoped<MedyaGocServisi>();

// Multi-tenant servisleri
yapici.Services.AddScoped<KiraciServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Tema.Servisler.StitchTemaServisi>();
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Tema.Servisler.CokluTemaServisi>();

// 3D Konfigüratör — tenant sahiplik doğrulayıcı
yapici.Services.AddScoped<VizitLink3D.Api.Moduller.Urunler.Servisler.IUcBoyutModelSahiplikDogrulayici, VizitLink3D.Api.Moduller.Urunler.Servisler.UcBoyutModelSahiplikDogrulayici>();

// Lisans servisi
yapici.Services.AddScoped<VizitLink3D.Api.Servisler.Kimlik.LisansServisi>();

var uygulama = yapici.Build();

// Veritabanini otomatik olustur ve baslangic verilerini tohumla
using (var kapsam = uygulama.Services.CreateScope())
{
    var vt = kapsam.ServiceProvider.GetRequiredService<VizitLink3DDbContext>();
    var webEnv = kapsam.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    // SQLite performans ayarlari: WAL modu okuma/yazmayi birbirini kilitlemeden
    // calistirir (coklu istek altinda hizli kalir), busy_timeout kilit
    // catismalarinda "database is locked" hatasi yerine kisa sure bekler.
    vt.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    vt.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");
    vt.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");

    var migrationAtla = string.Equals(Environment.GetEnvironmentVariable("VIZITLINK3D_SKIP_MIGRATION"), "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("VIZITLINK3D_SKIP_MIGRATION"), "true", StringComparison.OrdinalIgnoreCase);

    if (migrationAtla)
    {
        Log.Information("Veritabani migrasyonu ortam degiskeni ile atlandi.");
    }
    else
    {
    // SQLite EF migration kilidi, yarida kalan onceki calismalardan kalabiliyor.
    // Ilk acilista guvenli sekilde temizleyip migrasyonu devam ettiriyoruz.
    vt.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS \"__EFMigrationsLock\";");
    await vt.Database.MigrateAsync();
    }

    // Tohum verisi sadece Development ortaminda calissin (best practice).
    // Production'da DB zaten dolu, tohum verisi gereksiz islem + potansiyel AuditLog dongusu riski tasir.
    // Gerekirse FOR_SEED=1 ortam degiskeni ile zorla calistirilabilir.
    if (webEnv.IsDevelopment() || Environment.GetEnvironmentVariable("FORCE_SEED") == "1")
    {
        await VizitLink3D.Api.VeriTabani.TohumVerisi.TohumlaAsync(vt);
    }

    // DeepSeek API anahtarını şifreli tohumla (env veya config'den; kayıt varsa dokunma)
    var deepSeekKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
        ?? yapici.Configuration["AI:DeepSeekApiKey"];
    if (!string.IsNullOrWhiteSpace(deepSeekKey)
        && !vt.AISaglayicilari.Any(s => s.Tip == VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.DeepSeek))
    {
        var sifreleyici = kapsam.ServiceProvider.GetRequiredService<VizitLink3D.Api.Servisler.IApiKeySifrelemeServisi>();
        vt.AISaglayicilari.Add(new VizitLink3D.Ortak.Modeller.AI.AISaglayicisi
        {
            Tip = VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.DeepSeek,
            Ad = "DeepSeek (kod yazıcı)",
            ApiKeyEncrypted = sifreleyici.Sifrele(deepSeekKey),
            Model = "deepseek-chat",
            AylikLimitUsd = 50,
            AktifMi = true,
            SiraNo = 1
        });
        await vt.SaveChangesAsync();
        Log.Information("DeepSeek sağlayıcısı şifreli API anahtarıyla tohumlandı.");
    }

    // OpenCode Zen modellerini tohumla (2-6. modeller — paralel işçiler).
    // Ücretsiz modeller aktif; ücretli olanlar bakiye yüklenene kadar pasif.
    var zenKey = Environment.GetEnvironmentVariable("OPENCODE_ZEN_KEY")
        ?? Environment.GetEnvironmentVariable("OPENCODE_ZEN_KEY", EnvironmentVariableTarget.User);
    if (!string.IsNullOrWhiteSpace(zenKey)
        && !vt.AISaglayicilari.Any(s => s.Tip == VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.OpenCodeZen))
    {
        var sifreleyiciZen = kapsam.ServiceProvider.GetRequiredService<VizitLink3D.Api.Servisler.IApiKeySifrelemeServisi>();
        var zenSifreli = sifreleyiciZen.Sifrele(zenKey);
        var zenModeller = new (string Ad, string Model, bool Aktif, int Sira)[]
        {
            ("Zen DeepSeek V4 Flash Free (işçi)", "deepseek-v4-flash-free", true,  2),
            ("Zen MiMo v2.5 Free (işçi)",         "mimo-v2.5-free",         true,  3),
            ("Zen MiniMax M3 (planlayıcı)",       "minimax-m3",             false, 4),
            ("Zen MiniMax M2.7 (analiz)",         "minimax-m2.7",           false, 5),
            ("Zen DeepSeek V4 Flash (ücretli)",   "deepseek-v4-flash",      false, 6)
        };
        foreach (var (ad, model, aktif, sira) in zenModeller)
        {
            vt.AISaglayicilari.Add(new VizitLink3D.Ortak.Modeller.AI.AISaglayicisi
            {
                Tip = VizitLink3D.Ortak.Modeller.AI.AISaglayiciTipi.OpenCodeZen,
                Ad = ad, Model = model, AktifMi = aktif, SiraNo = sira,
                ApiKeyEncrypted = zenSifreli, AylikLimitUsd = 25
            });
        }
        await vt.SaveChangesAsync();
        Log.Information("OpenCode Zen modelleri tohumlandı (2 aktif ücretsiz + 3 pasif ücretli).");
    }
}

if (uygulama.Environment.IsDevelopment())
    uygulama.MapOpenApi();

// ==================== MIDDLEWARE SIRASI ====================
// 1. Hata yonetimi (en dis katman)
uygulama.UseMiddleware<HataYonetimiMiddleware>();

// 2. Firma cozumleme (multi-tenant)
uygulama.UseMiddleware<FirmaCozumlemeMiddleware>();

// 3. Lisans dogrulama
uygulama.UseMiddleware<LisansDogrulamaMiddleware>();

// 3. Guvenlik header'lari
uygulama.UseMiddleware<GuvenlikHeaderlariMiddleware>();

// 4. Routing
uygulama.UseRouting();

// 5. Rate limiting
uygulama.UseRateLimiter();

// 6. CORS
uygulama.UseCors();

// 7. API Anahtari dogrulama (embed/public endpoint'ler icin — CORS'tan sonra, Auth'tan once)
uygulama.UseMiddleware<ApiAnahtarDogrulamaMiddleware>();

// 8. Statik dosyalar (3D modeller icin GLB/GLTF MIME)
var saglayici = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
saglayici.Mappings[".glb"] = "model/gltf-binary";
saglayici.Mappings[".gltf"] = "model/gltf+json";
saglayici.Mappings[".wasm"] = "application/wasm";
saglayici.Mappings[".dll"] = "application/octet-stream";
saglayici.Mappings[".pdb"] = "application/octet-stream";
saglayici.Mappings[".dat"] = "application/octet-stream";
saglayici.Mappings[".js"] = "text/javascript";
saglayici.Mappings[".mjs"] = "text/javascript";
saglayici.Mappings[".css"] = "text/css";
saglayici.Mappings[".map"] = "application/json";
saglayici.Mappings[".br"] = "application/octet-stream";

uygulama.UseDefaultFiles();
uygulama.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = saglayici,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

// FAZ 4.4: Dinamik Medya Havuzu - Resimler ve PDF'ler tenant'a ait wwwroot/firmalar/{slug}/medya klasorunden sunulur.
// Varsayilan UseStaticFiles zaten wwwroot erisimini actigi icin ek middleware gerekmez.


uygulama.ResimOptimizasyonKullan();

// 8. Kimlik dogrulama
uygulama.UseAuthentication();

// 9. Yetkilendirme
uygulama.UseAuthorization();

// 10. API Controller'lari
uygulama.MapControllers();

// 11. SignalR Hub
uygulama.MapHub<SohbetHub>("/hubs/sohbet");
uygulama.MapHub<BildirimHub>("/hubs/bildirim");
uygulama.MapHub<AIHub>("/hubs/ai");
uygulama.MapHub<SahneAyarHub>("/hubs/sahne-ayar");
uygulama.MapHub<TemaHub>("/hubs/tema");

// 12. UI varsa default dosya + SPA fallback
if (System.IO.File.Exists(Path.Combine(uygulama.Environment.WebRootPath, "index.html"))) {
    uygulama.MapFallbackToFile("index.html");
}

// 13. Health check endpoint (Docker healthcheck icin)
uygulama.MapGet("/api/health", () => Results.Ok(new { durum = "saglikli", zaman = System.DateTime.UtcNow }));

uygulama.Run();
