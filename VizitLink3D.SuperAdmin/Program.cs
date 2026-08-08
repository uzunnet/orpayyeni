using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using VizitLink3D.SuperAdmin.Components;
using VizitLink3D.SuperAdmin.IServisler;
using VizitLink3D.SuperAdmin.Servisler;
using VizitLink3D.SuperAdmin.VeriTabani;

var yapici = WebApplication.CreateBuilder(args);

// MudBlazor servisleri
yapici.Services.AddMudServices();

yapici.Services.AddControllers();

// SQLite veritabani — bagimsiz superadmin.db
yapici.Services.AddDbContext<SuperAdminDbContext>(secenek =>
{
    secenek.UseSqlite("Data Source=superadmin.db");
    secenek.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// JWT Kimlik Dogrulama
var jwtAnahtar = yapici.Configuration["Jwt:Anahtar"] ?? "VizitLink3D_SuperAdmin_Gizli_Anahtari_2026";
var jwtYayinci = yapici.Configuration["Jwt:Yayinci"] ?? "VizitLink3D.SuperAdmin";
var jwtIzleyici = yapici.Configuration["Jwt:Izleyici"] ?? "VizitLink3D.SuperAdmin.UI";

yapici.Services.AddAuthentication(secenekler =>
{
    secenekler.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    secenekler.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(secenekler =>
{
    secenekler.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar)),
        ValidateIssuer = true,
        ValidIssuer = jwtYayinci,
        ValidateAudience = true,
        ValidAudience = jwtIzleyici,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

yapici.Services.AddAuthorization();

// Auth state provider ve giris servisi
yapici.Services.AddScoped<SuperAdminAuthStateProvider>();
yapici.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SuperAdminAuthStateProvider>());
yapici.Services.AddHttpClient();
yapici.Services.AddScoped<GirisServisi>();

// Firma olusturma servisi
yapici.Services.AddScoped<FirmaOlusturmaServisi>();

// Razor bileşenleri ve interaktif sunucu tarafi
yapici.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var uygulama = yapici.Build();

// Veritabani migrasyonu (otomatik)
using (var kapsam = uygulama.Services.CreateScope())
{
    var vt = kapsam.ServiceProvider.GetRequiredService<SuperAdminDbContext>();
    await vt.Database.MigrateAsync();

    // Varsayilan SuperAdmin kullanicisini olustur
    if (!vt.SuperAdminKullanicilar.Any())
    {
        vt.SuperAdminKullanicilar.Add(new SuperAdminKullanici
        {
            KullaniciAdi = "admin",
            AdSoyad = "Super Admin",
            SifreHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin2026!"),
            AktifMi = true,
            OlusturulmaTarihi = DateTime.UtcNow
        });
        await vt.SaveChangesAsync();
    }
}

if (!uygulama.Environment.IsDevelopment())
{
    uygulama.UseExceptionHandler("/Hata", createScopeForErrors: true);
    uygulama.UseHsts();
}

uygulama.UseAntiforgery();
uygulama.MapStaticAssets();

uygulama.UseAuthentication();
uygulama.UseAuthorization();

uygulama.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

uygulama.MapControllers();

await uygulama.RunAsync();
