---
name: api-servisler-mediatr
description: API tasarımı — Cevap<T> zarfı, MediatR CQRS pipeline, FluentValidation, Mapster (AutoMapper YASAK), Vertical Slice klasör, HataYonetimiMiddleware, OpenAPI yerleşik, SignalR + MessagePack, rate limiting endpoint başına, API versioning.
status: TAMAM
---

# 🌐 API / SERVİSLER / MEDIATR

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [02_CSharp_Disiplini.md](02_CSharp_Disiplini.md), [05_Veritabani_EFCore10.md](05_Veritabani_EFCore10.md)

---

## 1. 🗂 VERTICAL SLICE KLASÖR YAPISI (ZORUNLU)

```
[PROJE_ADI].Api/Moduller/
├── Urunler/
│   ├── Komutlar/
│   │   ├── UrunOlusturKomutu.cs
│   │   ├── UrunGuncelleKomutu.cs
│   │   └── UrunSilKomutu.cs
│   ├── Sorgular/
│   │   ├── UrunListeleSorgusu.cs
│   │   ├── UrunDetaySorgusu.cs
│   │   └── UrunSlugIleSorgusu.cs
│   ├── Dtolar/
│   │   ├── UrunDto.cs
│   │   ├── UrunOzetDto.cs
│   │   └── UrunDetayDto.cs
│   ├── Dogrulayicilar/
│   │   ├── UrunOlusturDogrulayici.cs
│   │   └── UrunGuncelleDogrulayici.cs
│   ├── Profil/
│   │   └── UrunMapsterProfil.cs
│   └── Kontrolcu/
│       └── UrunKontrolcu.cs        ← 3 SATIRLIK (mediator.Send)
├── Musteriler/  (aynı yapı)
├── Siparisler/  (aynı yapı)
├── Kimlik/      (Giris, Kayit, Rol)
├── Medya/       (Havuz)
├── AI/          (Sağlayıcı yönetimi)
└── Sistem/      (Audit, Ceviri, Tema, Ayar, Yedek)
```

> ⚠ **CQRS KURALI:** Modül klasörlerinde `Servisler/` KULLANILMAZ. Tüm iş mantığı doğrudan Komut/Sorgu `Isleyici` (Handler) sınıflarına yazılır. `Servisler/` sadece System katmanında Wrappped dış kütüphane servisleri için kullanılır (örn: MailKit, Iyzico). Handler'da servis çağıran boş postacı (Anemic Domain Model) YASAKTIR.

**Felsefe:** Bir özellik = bir klasör. Ekip paralel çalışır, çakışma yok.

---

## 2. 🛣 ROUTE STANDARDI

### 2.1 REST Tablo
| Metot | Yol | İş |
|---|---|---|
| `GET` | `/api/urunler` | Listele (paginated) |
| `GET` | `/api/urunler/{id}` | Detay |
| `GET` | `/api/urunler/slug/{slug}` | Slug ile |
| `POST` | `/api/urunler` | Oluştur |
| `PUT` | `/api/urunler/{id}` | Güncelle |
| `PATCH` | `/api/urunler/{id}` | Kısmi güncelle |
| `DELETE` | `/api/urunler/{id}` | Sil (soft) |

### 2.2 İç İçe Kaynaklar
```
GET    /api/urunler/{urunId}/resimler
POST   /api/urunler/{urunId}/resimler
DELETE /api/urunler/{urunId}/resimler/{resimId}
```

### 2.3 Yasak Pattern'ler
```
❌ /api/getUrunler          (fiil yok — REST kuralı)
❌ /api/Urunler/Sil/5        (HTTP fiili route'a yazılmaz)
❌ /api/urun_listele         (snake_case)
✅ /api/urunler              (lowercase kebab, çoğul)
```

---

## 3. 🎯 Cevap<T> ZARFI

### 3.1 Sınıf
```csharp
public class Cevap<T>
{
    public bool BasariliMi { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public List<string> Hatalar { get; set; } = [];
    public T? Veri { get; set; }
    public string? CorrelationId { get; set; }

    public static Cevap<T> Basarili(T veri, string mesaj = "İşlem başarılı.")
        => new() { BasariliMi = true, Veri = veri, Mesaj = mesaj };

    public static Cevap<T> Hata(string mesaj, List<string>? hatalar = null)
        => new() { BasariliMi = false, Mesaj = mesaj, Hatalar = hatalar ?? [] };
}
```

### 3.2 Kullanım
**Her endpoint** `Cevap<T>` veya `Cevap<List<T>>` döner:
```csharp
// ✅
public async Task<Cevap<UrunDto>> OlusturAsync(UrunOlusturKomutu komut)
    => await _mediator.Send(komut);

// ❌
public async Task<IActionResult> Olustur(UrunDto dto) { ... }   // ham IActionResult
public async Task<UrunDto> Olustur(UrunDto dto) { ... }          // hata zarflı değil
```

### 3.3 İstemci Tarafı (Blazor)
```csharp
var cevap = await _http.PostAsJsonAsync<Cevap<UrunDto>>("/api/urunler", komut);
if (cevap.BasariliMi)
    Snackbar.Add(cevap.Mesaj, Severity.Success);
else
    Snackbar.Add(string.Join(", ", cevap.Hatalar), Severity.Error);
```

---

## 4. 🎭 MEDIATR — CQRS

### 4.1 Kurulum (Program.cs)
```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(DogrulamaIslem<,>));
    cfg.AddOpenBehavior(typeof(LoglamaIslem<,>));
    cfg.AddOpenBehavior(typeof(OnbellekIslem<,>));
    cfg.AddOpenBehavior(typeof(DenetimIslem<,>));
});
```

### 4.2 Komut (Command — State Değiştiren)
```csharp
namespace [PROJE_ADI].Api.Moduller.Urunler.Komutlar;

public record UrunOlusturKomutu(
    string Ad,
    string Slug,
    int KategoriId,
    string? Aciklama,
    decimal? Fiyat) : IRequest<Cevap<UrunDto>>;

public class UrunOlusturIsleyici(
    [PROJE_ADI]DbContext db,
    IMapper mapper) : IRequestHandler<UrunOlusturKomutu, Cevap<UrunDto>>
{
    public async Task<Cevap<UrunDto>> Handle(
        UrunOlusturKomutu istek,
        CancellationToken iptal)
    {
        var urun = new Urun
        {
            Ad = istek.Ad,
            Slug = istek.Slug,
            KategoriId = istek.KategoriId,
            Aciklama = istek.Aciklama,
            Fiyat = istek.Fiyat
        };

        db.Urunler.Add(urun);
        await db.SaveChangesAsync(iptal);

        return Cevap<UrunDto>.Basarili(
            mapper.Map<UrunDto>(urun),
            "Ürün oluşturuldu.");
    }
}
```

### 4.3 Sorgu (Query — Salt-Okunur)
```csharp
public record UrunListeleSorgusu(
    int Sayfa = 1,
    int Boyut = 20,
    int? KategoriId = null,
    string? Arama = null) : IRequest<Cevap<List<UrunOzetDto>>>;

public class UrunListeleIsleyici(
    [PROJE_ADI]DbContext db) : IRequestHandler<UrunListeleSorgusu, Cevap<List<UrunOzetDto>>>
{
    public async Task<Cevap<List<UrunOzetDto>>> Handle(
        UrunListeleSorgusu sorgu,
        CancellationToken iptal)
    {
        var q = db.Urunler.AsNoTracking().Where(u => u.AktifMi);

        if (sorgu.KategoriId is int k)
            q = q.Where(u => u.KategoriId == k);

        if (!string.IsNullOrWhiteSpace(sorgu.Arama))
            q = q.Where(u => u.Ad.Contains(sorgu.Arama));

        var liste = await q
            .OrderByDescending(u => u.OlusturulmaTarihi)
            .Skip((sorgu.Sayfa - 1) * sorgu.Boyut)
            .Take(sorgu.Boyut)
            .Select(u => new UrunOzetDto(u.Id, u.Ad, u.Slug, u.Fiyat))
            .ToListAsync(iptal);

        return Cevap<List<UrunOzetDto>>.Basarili(liste);
    }
}
```

### 4.4 Kontrolcü — 3 Satır
```csharp
namespace [PROJE_ADI].Api.Moduller.Urunler.Kontrolcu;

[ApiController]
[Route("api/urunler")]
public class UrunKontrolcu(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<UrunOzetDto>>> Listele([FromQuery] UrunListeleSorgusu sorgu)
        => await mediator.Send(sorgu);

    [HttpGet("{id:int}")]
    public async Task<Cevap<UrunDetayDto>> Detay(int id)
        => await mediator.Send(new UrunDetaySorgusu(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<Cevap<UrunDto>> Olustur(UrunOlusturKomutu komut)
        => await mediator.Send(komut);

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<Cevap<UrunDto>> Guncelle(int id, UrunGuncelleKomutu komut)
        => await mediator.Send(komut with { Id = id });

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<Cevap<bool>> Sil(int id)
        => await mediator.Send(new UrunSilKomutu(id));
}
```

**Kural:** Kontrolcüde `_db.Urunler.Where()` YASAK. Tüm iş `Komut/Sorgu` içine.

---

## 5. ⚙ PIPELINE BEHAVIORS (Otomatik Cross-Cutting)

Her MediatR isteği otomatik bu boru hattından geçer:
```
[İstek] → Dogrulama → Loglama → Onbellek → Handler → Denetim → [Cevap]
```

### 5.1 Dogrulama (FluentValidation Pipeline)
```csharp
public class DogrulamaIslem<TIstek, TCevap>(
    IEnumerable<IValidator<TIstek>> dogrulayicilar)
    : IPipelineBehavior<TIstek, TCevap>
    where TIstek : IRequest<TCevap>
{
    public async Task<TCevap> Handle(
        TIstek istek,
        RequestHandlerDelegate<TCevap> sonraki,
        CancellationToken iptal)
    {
        if (!dogrulayicilar.Any()) return await sonraki();

        var baglam = new ValidationContext<TIstek>(istek);
        var sonuclar = await Task.WhenAll(
            dogrulayicilar.Select(d => d.ValidateAsync(baglam, iptal)));

        var hatalar = sonuclar
            .SelectMany(s => s.Errors)
            .Where(h => h is not null)
            .Select(h => h.ErrorMessage)
            .ToList();

        if (hatalar.Count > 0)
            throw new DogrulamaException(hatalar);

        return await sonraki();
    }
}
```

### 5.2 Loglama
```csharp
public class LoglamaIslem<TIstek, TCevap>(ILogger<TIstek> log)
    : IPipelineBehavior<TIstek, TCevap>
    where TIstek : IRequest<TCevap>
{
    public async Task<TCevap> Handle(
        TIstek istek,
        RequestHandlerDelegate<TCevap> sonraki,
        CancellationToken iptal)
    {
        var ad = typeof(TIstek).Name;
        log.LogInformation("İstek başladı: {Ad}", ad);

        var saat = Stopwatch.StartNew();
        var cevap = await sonraki();
        saat.Stop();

        log.LogInformation("İstek bitti: {Ad} ({Ms}ms)", ad, saat.ElapsedMilliseconds);
        return cevap;
    }
}
```

### 5.3 Önbellek (FusionCache — Sorgular İçin)
```csharp
public class OnbellekIslem<TIstek, TCevap>(
    IOnbellekYonetici onbellek)
    : IPipelineBehavior<TIstek, TCevap>
    where TIstek : IRequest<TCevap>
{
    public async Task<TCevap> Handle(
        TIstek istek,
        RequestHandlerDelegate<TCevap> sonraki,
        CancellationToken iptal)
    {
        if (istek is not IOnbelleklenebilir<TCevap> obellekli)
            return await sonraki();

        return await onbellek.GetirVeyaOlusturAsync(
            obellekli.OnbellekAnahtari,
            async () => await sonraki(),
            obellekli.OnbellekSuresi);
    }
}

public interface IOnbelleklenebilir<TCevap>
{
    string OnbellekAnahtari { get; }
    TimeSpan OnbellekSuresi { get; }
}
```

### 5.4 Denetim (Audit Log)
Her komut sonrası `AuditLog` yazılır — bkz. [07_Guvenlik_Passkey_JWT.md](07_Guvenlik_Passkey_JWT.md) §14.

---

## 6. ✅ FLUENTVALIDATION

### 6.1 Dogrulayici Sınıfı
```csharp
namespace [PROJE_ADI].Api.Moduller.Urunler.Dogrulayicilar;

public class UrunOlusturDogrulayici : AbstractValidator<UrunOlusturKomutu>
{
    public UrunOlusturDogrulayici([PROJE_ADI]DbContext db)
    {
        RuleFor(x => x.Ad)
            .NotEmpty().WithMessage("Ürün adı zorunludur.")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .Matches("^[a-z0-9-]+$").WithMessage("Slug sadece küçük harf, rakam ve tire içerebilir.")
            .MustAsync(async (slug, iptal) =>
                !await db.Urunler.AnyAsync(u => u.Slug == slug, iptal))
            .WithMessage("Bu slug zaten kullanılıyor.");

        RuleFor(x => x.KategoriId)
            .GreaterThan(0)
            .MustAsync(async (id, iptal) =>
                await db.Kategoriler.AnyAsync(k => k.Id == id, iptal))
            .WithMessage("Geçersiz kategori.");

        RuleFor(x => x.Fiyat)
            .GreaterThanOrEqualTo(0).When(x => x.Fiyat.HasValue);

        RuleFor(x => x.Aciklama)
            .MaximumLength(5000);
    }
}
```

### 6.2 Otomatik Kayıt
```csharp
// Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

### 6.3 Blazor 10 Yenisi: AddValidation + [ValidatableType]
Nested form + collection:
```csharp
// Program.cs
builder.Services.AddValidation();

// Model
[ValidatableType]
public class UrunOlusturKomutu : IRequest<Cevap<UrunDto>>
{
    [Required] public string Ad { get; set; } = string.Empty;

    public List<ResimEkleDto> Resimler { get; set; } = [];   // collection da validate
}
```

---

## 7. 🗺 MAPSTER (AutoMapper YASAK)

### 7.1 Niçin AutoMapper YASAK?
- ❌ AutoMapper reflection ağırlık — **yavaş** (Mapster 4-10x hızlı)
- ❌ Convention'lar gizli, debug zor
- ❌ Konfig dağınık, profiler zor
- ✅ Mapster — derleme zamanı code gen, **kompakt**, **hızlı**

### 7.2 Konfig
```csharp
namespace [PROJE_ADI].Api.Moduller.Urunler.Profil;

public class UrunMapsterProfil : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Urun, UrunDto>();

        config.NewConfig<Urun, UrunOzetDto>()
            .Map(d => d.KategoriAdi, s => s.Kategori!.Ad);

        config.NewConfig<UrunOlusturKomutu, Urun>()
            .Ignore(d => d.Id)
            .Ignore(d => d.OlusturulmaTarihi);
    }
}
```

### 7.3 Program.cs
```csharp
var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();
```

### 7.4 Kullanım
```csharp
// DTO'ya dönüştür
var dto = mapper.Map<UrunDto>(urun);

// LINQ projection
var dtolar = await db.Urunler
    .ProjectToType<UrunOzetDto>()
    .ToListAsync();
```

---

## 8. 🛑 HATA YÖNETİMİ MIDDLEWARE

```csharp
namespace [PROJE_ADI].Api.AraYazilimlar;

public class HataYonetimiMiddleware(
    RequestDelegate sonraki,
    ILogger<HataYonetimiMiddleware> log,
    IWebHostEnvironment ortam)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        ctx.Items["CorrelationId"] = correlationId;
        ctx.Response.Headers["X-Correlation-Id"] = correlationId;

        try
        {
            await sonraki(ctx);
        }
        catch (DogrulamaException ex)
        {
            await CevapYazAsync(ctx, 400, Cevap<object>.Hata("Doğrulama hatası.", ex.Hatalar), correlationId);
        }
        catch (KaynakBulunamadiException ex)
        {
            await CevapYazAsync(ctx, 404, Cevap<object>.Hata(ex.Message), correlationId);
        }
        catch (YetkisizException ex)
        {
            await CevapYazAsync(ctx, 403, Cevap<object>.Hata(ex.Message), correlationId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "İşlenmemiş hata. {CorrelationId}", correlationId);

            var mesaj = ortam.IsDevelopment()
                ? $"{ex.Message}\n{ex.StackTrace}"
                : "Sunucu hatası oluştu.";

            await CevapYazAsync(ctx, 500, Cevap<object>.Hata(mesaj), correlationId);
        }
    }

    private static async Task CevapYazAsync(
        HttpContext ctx, int durumKodu, Cevap<object> cevap, string correlationId)
    {
        cevap.CorrelationId = correlationId;
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = durumKodu;
        await ctx.Response.WriteAsJsonAsync(cevap);
    }
}
```

Kayıt (Program.cs):
```csharp
app.UseMiddleware<HataYonetimiMiddleware>();
```

---

## 9. 🎯 KONTROLCÜ DİSİPLİNİ

### 9.1 3 Satır Kuralı
Kontrolcü sadece routing + auth + mediator.Send:
```csharp
[ApiController]
[Route("api/urunler")]
public class UrunKontrolcu(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("yazma")]
    public async Task<Cevap<UrunDto>> Olustur(UrunOlusturKomutu komut)
        => await mediator.Send(komut);
}
```

### 9.2 Yasaklar (Kontrolcüde)
```
❌ try-catch
❌ _db.Urunler.Where() / .ToListAsync()
❌ Mapping (mapper.Map())
❌ Business logic
❌ FluentValidation çağrısı (pipeline halletti)
❌ Cache kontrolü
```

---

## 10. 🔢 API VERSIONING

### 10.1 Kurulum
```csharp
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'VVV";
    opt.SubstituteApiVersionInUrl = true;
});
```

### 10.2 Kontrolcü
```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/urunler")]
public class UrunKontrolcu : ControllerBase
{
    [HttpGet, MapToApiVersion("1.0")]
    public async Task<Cevap<List<UrunOzetDto>>> ListeleV1(...) { ... }

    [HttpGet, MapToApiVersion("2.0")]
    public async Task<Cevap<List<UrunDetayDto>>> ListeleV2(...) { ... }
}
```

---

## 11. 🚦 RATE LIMITING (Endpoint Başına)

```csharp
// Program.cs
builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("genel", o =>
    {
        o.Window = TimeSpan.FromMinutes(5);
        o.PermitLimit = 1000;
    });

    opt.AddFixedWindowLimiter("giris", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 5;
    });

    opt.AddFixedWindowLimiter("yazma", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 30;
    });

    opt.RejectionStatusCode = 429;
});

app.UseRateLimiter();
```

Kontrolcüde:
```csharp
[HttpPost("/giris")]
[EnableRateLimiting("giris")]
public async Task<Cevap<TokenDto>> Giris(GirisKomutu k) => await mediator.Send(k);
```

---

## 12. 📘 OPENAPI (ASP.NET CORE 10 YERLEŞİK)

### 12.1 Swashbuckle YASAK
.NET 10 ile birlikte gelen **`Microsoft.AspNetCore.OpenApi`** kullan — Swashbuckle artık gerekmez.

### 12.2 Kurulum
```csharp
// Program.cs
builder.Services.AddOpenApi();

app.MapOpenApi();   // /openapi/v1.json

// Scalar UI (opsiyonel — Swagger UI yerine modern)
app.MapScalarApiReference();
```

### 12.3 Endpoint Açıklaması
```csharp
[HttpGet]
[EndpointSummary("Ürünleri listeler")]
[EndpointDescription("Aktif ürünleri sayfalı olarak döner.")]
public async Task<Cevap<List<UrunOzetDto>>> Listele() { ... }
```

---

## 13. 📡 SIGNALR + MESSAGEPACK

### 13.1 Kurulum
```csharp
// Program.cs
builder.Services.AddSignalR(opt =>
{
    opt.EnableDetailedErrors = builder.Environment.IsDevelopment();
})
.AddMessagePackProtocol();   // JSON yerine binary — 5x hız, %80 bant tasarruf
```

### 13.2 Hub (Türkçe Metot Adları)
```csharp
namespace [PROJE_ADI].Api.Hubs;

[Authorize]
public class UrunHub : Hub
{
    public async Task UrunGuncellendi(int urunId, UrunDto yeniHal)
    {
        await Clients.All.SendAsync("UrunGuncellendi", urunId, yeniHal);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("HosGeldiniz", Context.UserIdentifier);
        await base.OnConnectedAsync();
    }
}
```

### 13.3 Kayıt
```csharp
app.MapHub<UrunHub>("/hub/urun");
```

### 13.4 İstemci (Blazor)
```csharp
_baglanti = new HubConnectionBuilder()
    .WithUrl(Nav.ToAbsoluteUri("/hub/urun"))
    .AddMessagePackProtocol()
    .WithAutomaticReconnect([0, 2000, 5000, 10000])
    .Build();

_baglanti.On<int, UrunDto>("UrunGuncellendi", (id, yeniHal) =>
{
    // UI güncelle
    InvokeAsync(StateHasChanged);
});

await _baglanti.StartAsync();
```

---

## 14. 📋 ÖZ-DENETİM (15 Madde)

```
[ ] 1. Vertical Slice klasör (Moduller/<Ad>/{Komutlar,Sorgular,...})
[ ] 2. Kontrolcü 3 satırlık (sadece mediator.Send)
[ ] 3. Kontrolcüde try-catch YOK
[ ] 4. Kontrolcüde DB sorgu YOK (Komut/Sorgu içinde)
[ ] 5. Cevap<T> dönüyor (her endpoint)
[ ] 6. MediatR Komut/Sorgu record
[ ] 7. FluentValidation Dogrulayici sınıfı
[ ] 8. Mapster (AutoMapper YOK)
[ ] 9. HataYonetimiMiddleware kayıtlı
[ ] 10. Rate limiting [EnableRateLimiting("politika")]
[ ] 11. [Authorize] admin endpoint'lerde
[ ] 12. OpenAPI yerleşik (Swashbuckle YOK)
[ ] 13. SignalR Hub Türkçe + MessagePack
[ ] 14. API versioning (gerekirse)
[ ] 15. Route lowercase + çoğul (REST)
```

---

*Versiyon: 1.0 | Tarih: 2026-05-14 | Bağlı: [02_CSharp_Disiplini.md](02_CSharp_Disiplini.md), [07_Guvenlik_Passkey_JWT.md](07_Guvenlik_Passkey_JWT.md)*
