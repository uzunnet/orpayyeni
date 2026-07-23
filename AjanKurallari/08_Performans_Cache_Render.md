---
name: performans-cache-render
description: FusionCache L1+L2 Redis, stampede protection, fail-safe; EF performans (AsNoTracking, AsSplitQuery, projection, ExecuteUpdate); Blazor render (Virtualize, @key, [PersistentState]); Lazy loading; ImageSharp.Web CDN; SignalR MessagePack; Brotli; Lighthouse 90+ hedef.
status: TAMAM
---

# ⚡ PERFORMANS / CACHE / RENDER

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [05_Veritabani_EFCore10.md](05_Veritabani_EFCore10.md), [03_Razor_MudBlazor_Blazor10.md](03_Razor_MudBlazor_Blazor10.md)

---

## 1. 🎯 HEDEF METRİKLER

| Metrik | Hedef | Mükemmel |
|---|---|---|
| LCP (Largest Contentful Paint) | < 2.5s | < 1.8s |
| FID (First Input Delay) | < 100ms | < 50ms |
| CLS (Cumulative Layout Shift) | < 0.1 | < 0.05 |
| TTFB (Time to First Byte) | < 600ms | < 300ms |
| Lighthouse Performance | 80+ | 90+ |
| Bundle (Blazor WASM) | < 5 MB | < 3 MB |
| API yanıt (p95) | < 500ms | < 200ms |
| DB sorgu (p95) | < 100ms | < 50ms |
| Cache hit oranı | > 70% | > 90% |

---

## 2. 🚀 FUSIONCACHE (L1 + L2)

### 2.1 Niçin FusionCache?
- ✅ Çift katman (L1 memory + L2 Redis)
- ✅ Stampede protection (aynı anda 100 istek → 1 DB sorgusu)
- ✅ Fail-safe (Redis çökse bile L1'den döner)
- ✅ Jitter (TTL'e rastgele ekleme — cache patlamasını önler)
- ✅ Eager refresh (TTL'in %80'inde arka planda yeniler)
- ✅ Tag invalidation

### 2.2 Kurulum
```csharp
// Program.cs
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromMinutes(10),
        Priority = CacheItemPriority.Normal,

        // Fail-safe
        IsFailSafeEnabled = true,
        FailSafeMaxDuration = TimeSpan.FromHours(2),
        FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

        // Stampede
        FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
        FactoryHardTimeout = TimeSpan.FromSeconds(2),

        // Jitter
        JitterMaxDuration = TimeSpan.FromSeconds(2),

        // Eager refresh
        EagerRefreshThreshold = 0.8f
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .WithDistributedCache(new RedisCache(new RedisCacheOptions
    {
        Configuration = builder.Configuration.GetConnectionString("Redis")
    }));
```

### 2.3 OnbellekYonetici (Wrapper)
```csharp
public class OnbellekYonetici(IFusionCache cache, ILogger<OnbellekYonetici> log)
{
    public async Task<T> GetirVeyaOlusturAsync<T>(
        string anahtar,
        Func<CancellationToken, Task<T>> uretici,
        TimeSpan? sure = null,
        CancellationToken iptal = default)
    {
        return await cache.GetOrSetAsync(
            anahtar,
            uretici,
            opt =>
            {
                if (sure.HasValue) opt.SetDuration(sure.Value);
            },
            token: iptal);
    }

    public async Task SilAsync(string anahtar) =>
        await cache.RemoveAsync(anahtar);

    /// <summary>
    /// Desen ile sil — örn: "urun:*"
    /// (Redis SCAN + DEL)
    /// </summary>
    public async Task SilDesenAsync(string desen)
    {
        // Redis SCAN ile match eden anahtarlar
        // FusionCache tag-invalidation tercih edilebilir
        log.LogInformation("Cache desen silindi: {Desen}", desen);
    }

    public async Task EtiketleAsync(string anahtar, params string[] etiketler) =>
        await cache.SetAsync(anahtar, etiketler);
}
```

### 2.4 Kullanım (Servis İçinde)
```csharp
public async Task<List<UrunOzetDto>> ListeleAsync(int kategoriId)
{
    var anahtar = $"urun:liste:kat:{kategoriId}";

    return await _onbellek.GetirVeyaOlusturAsync(
        anahtar,
        async iptal => await _db.Urunler
            .AsNoTracking()
            .Where(u => u.KategoriId == kategoriId)
            .ProjectToType<UrunOzetDto>()
            .ToListAsync(iptal),
        TimeSpan.FromMinutes(15));
}

// Güncelleme sonrası cache temizle
public async Task GuncelleAsync(int id, UrunDto dto)
{
    // ... DB güncelle ...
    await _onbellek.SilDesenAsync("urun:*");
}
```

### 2.5 Cache Anahtar Standardı
```
[entite]:[islem]:[parametre]
─────────────────────────────
urun:liste:kat:5
urun:detay:42
urun:slug:membran-101
kategori:tumu
ceviri:tr
ayar:tema
kullanici:profil:7
```

### 2.6 TTL Önerileri
| Veri | TTL |
|---|---|
| Çeviri (dil) | 30 dk |
| Kategori listesi | 1 saat |
| Ürün listesi | 10 dk |
| Ürün detayı | 30 dk |
| Sistem ayarları | 5 dk |
| Kullanıcı profili | 2 dk |
| Anasayfa slayt | 15 dk |
| Authorize check | 1 dk |

---

## 3. 🗃 EF CORE PERFORMANS (Detay: 05_)

### 3.1 Hızlı Kontrol Listesi
| Sorun | Çözüm |
|---|---|
| Yavaş liste | `AsNoTracking() + projection` |
| N+1 | `Include()` veya `ProjectToType<>()` |
| Multi-collection | `AsSplitQuery()` |
| Toplu update | `ExecuteUpdateAsync` |
| 1000+ insert | `BulkInsertAsync` |
| Yavaş count | Filtre kolonuna index |
| Yavaş sayfa | Keyset pagination |

### 3.2 ProjectToType (Mapster + EF)
```csharp
// Tek seferde DTO'ya — minimum kolon DB'den çekilir
var liste = await _db.Urunler
    .Where(u => u.AktifMi)
    .ProjectToType<UrunOzetDto>()
    .ToListAsync();
```

### 3.3 Compiled Query (Aşırı Sık Sorgu)
```csharp
private static readonly Func<[PROJE_ADI]DbContext, int, Task<Urun?>> _urunGetir =
    EF.CompileAsyncQuery((..[PROJE_ADI]DbContext ctx, int id) =>
        ctx.Urunler.FirstOrDefault(u => u.Id == id));

public async Task<Urun?> HizliGetirAsync(int id) => await _urunGetir(_db, id);
```

### 3.4 DbContext Pool
```csharp
builder.Services.AddDbContextPool<[PROJE_ADI]DbContext>(opt =>
    opt.UseNpgsql(connectionString));
```

---

## 4. 🎨 BLAZOR RENDER OPTİMİZASYONU

### 4.1 Virtualize (Uzun Liste)
```razor
<Virtualize Items="@_urunler" Context="urun" ItemSize="120" OverscanCount="3">
    <UrunKart Urun="@urun" />
</Virtualize>
```
- Sadece görünen + 3 overscan render edilir
- 10.000 ürün = sorunsuz

### 4.2 ItemsProvider (Server-Side Pagination)
```razor
<Virtualize ItemsProvider="UrunSaglaAsync" Context="urun" ItemSize="120">
    <UrunKart Urun="@urun" />
</Virtualize>

@code-equivalent:
private async ValueTask<ItemsProviderResult<UrunDto>> UrunSaglaAsync(
    ItemsProviderRequest istek)
{
    var sonuc = await UrunServisi.ListeleAsync(
        sayfa: istek.StartIndex / 20 + 1,
        boyut: istek.Count);
    return new ItemsProviderResult<UrunDto>(sonuc.Veri!, toplamSayi);
}
```

### 4.3 @key Direktifi
```razor
@foreach (var urun in _urunler)
{
    <UrunKart @key="urun.Id" Urun="@urun" />
}
```
Liste değişiminde DOM yeniden kullanılır (re-render değil).

### 4.4 ShouldRender (Dikkatli)
**Kural:** Genelde override etme — Blazor zaten optimize. Sadece **gerçekten** problem varsa:
```csharp
private string _onceki = string.Empty;

protected override bool ShouldRender()
{
    if (Veri.Ad == _onceki) return false;
    _onceki = Veri.Ad;
    return true;
}
```

### 4.5 RenderFragment Cache
```csharp
private RenderFragment? _onbellekliBaslik;

protected override void OnParametersSet()
{
    _onbellekliBaslik ??= @<h1>@Baslik</h1>;
}
```

### 4.6 StateHasChanged Dikkatli
- Her event handler sonunda Blazor otomatik çağırır
- **Async** içinde manuel: `await InvokeAsync(StateHasChanged)`
- Sık çağırma = render patlaması

---

## 5. 💾 [PersistentState] (BLAZOR 10 YENİ)

### 5.1 Niçin?
Circuit eviction'da (kullanıcı sekmeyi minimize etti, server hafıza temizledi) **state otomatik korunur + restore edilir**.

### 5.2 Kullanım
```csharp
public partial class UrunListele
{
    [PersistentState]
    public string AramaKelimesi { get; set; } = string.Empty;

    [PersistentState]
    public int Sayfa { get; set; } = 1;

    [PersistentState]
    public List<int> SecilenKategoriler { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        // AramaKelimesi, Sayfa, SecilenKategoriler otomatik restore
        await ListeleAsync();
    }
}
```

### 5.3 [SupplyParameterFromPersistentComponentState]
Prerender ↔ interactive geçişte state taşıma:
```csharp
[SupplyParameterFromPersistentComponentState]
public List<UrunOzetDto>? Urunler { get; set; }
```

---

## 6. 📦 LAZY LOADING (BUNDLE OPTİMİZE)

### 6.1 Sayfa Assembly Lazy Load
```xml
<!-- [PROJE_ADI].UI.csproj -->
<ItemGroup>
    <BlazorWebAssemblyLazyLoad Include="[PROJE_ADI].UI.Yonetim.dll" />
    <BlazorWebAssemblyLazyLoad Include="[PROJE_ADI].UI.UcBoyut.dll" />
</ItemGroup>
```

### 6.2 Route Bazlı Yükleme (App.razor)
```razor
<Router AppAssembly="@typeof(App).Assembly"
        OnNavigateAsync="@OnNavigateAsync"
        AdditionalAssemblies="@_yuklenmisModuller">
    ...
</Router>

@code-equivalent:
private async Task OnNavigateAsync(NavigationContext ctx)
{
    if (ctx.Path.StartsWith("yonetim"))
    {
        var dlls = await _yukleyici.YukleAsync("[PROJE_ADI].UI.Yonetim.dll");
        _yuklenmisModuller = _yuklenmisModuller.Concat(dlls).ToList();
    }
}
```

---

## 7. 🖼 IMAGESHARP.WEB (ON-THE-FLY CDN)

### 7.1 Kurulum
```csharp
// Program.cs
builder.Services.AddImageSharp()
    .Configure<PhysicalFileSystemCacheOptions>(opt =>
    {
        opt.CacheFolder = "is-cache";
    })
    .SetCache<PhysicalFileSystemCache>()
    .SetCacheHash<SHA256CacheHash>();

app.UseImageSharp();
```

### 7.2 URL Parametreleri
```
/medya/urun-101.jpg                       → orijinal
/medya/urun-101.jpg?w=400                 → 400px genişlik
/medya/urun-101.jpg?w=400&q=80            → kalite 80
/medya/urun-101.jpg?fmt=webp              → format dönüşüm
/medya/urun-101.jpg?w=400&fit=crop        → crop
/medya/urun-101.jpg?w=400&fmt=webp&q=80   → kombinasyon
```

### 7.3 Razor Kullanımı
```razor
<picture>
    <source srcset="@($"/medya/{urun.Resim}?w=800&fmt=avif&q=80")" type="image/avif" />
    <source srcset="@($"/medya/{urun.Resim}?w=800&fmt=webp&q=80")" type="image/webp" />
    <img src="@($"/medya/{urun.Resim}?w=800&q=80")"
         alt="@urun.Ad"
         loading="lazy"
         decoding="async" />
</picture>
```

### 7.4 Responsive (srcset)
```razor
<img srcset="@($"/medya/{r}?w=400 400w, /medya/{r}?w=800 800w, /medya/{r}?w=1600 1600w")"
     sizes="(max-width: 768px) 100vw, 50vw"
     src="@($"/medya/{r}?w=800")"
     alt="..." />
```

### 7.5 Cache-Control
Production nginx config:
```nginx
location /medya/ {
    expires 7d;
    add_header Cache-Control "public, immutable";
}
```

---

## 8. 📡 SIGNALR + MESSAGEPACK

### 8.1 Performans Kazancı
- JSON → MessagePack: **5x hız**, **%80 bant tasarruf**
- Binary protokol — küçük payload, hızlı parse

### 8.2 Konfig — Detay: [06_API_Servisler_MediatR.md](06_API_Servisler_MediatR.md) §13

---

## 9. 🗜 COMPRESSION (Brotli + Gzip)

### 9.1 ASP.NET Core
```csharp
builder.Services.AddResponseCompression(opt =>
{
    opt.EnableForHttps = true;
    opt.Providers.Add<BrotliCompressionProvider>();
    opt.Providers.Add<GzipCompressionProvider>();
    opt.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream", "application/wasm"]);
});

builder.Services.Configure<BrotliCompressionProviderOptions>(opt =>
    opt.Level = CompressionLevel.Optimal);

app.UseResponseCompression();
```

### 9.2 Nginx (Production)
```nginx
brotli on;
brotli_comp_level 6;
brotli_types text/plain text/css application/json application/javascript application/wasm;

gzip on;
gzip_vary on;
gzip_comp_level 6;
```

---

## 10. ⚡ OUTPUT CACHE (.NET 7+)

### 10.1 Kurulum
```csharp
builder.Services.AddOutputCache(opt =>
{
    opt.AddPolicy("Anasayfa", b => b
        .Expire(TimeSpan.FromMinutes(5))
        .SetVaryByQuery("dil"));

    opt.AddPolicy("UrunListe", b => b
        .Expire(TimeSpan.FromMinutes(2))
        .SetVaryByQuery("sayfa", "kategori"));
});

app.UseOutputCache();
```

### 10.2 Kullanım
```csharp
[HttpGet]
[OutputCache(PolicyName = "UrunListe")]
public async Task<Cevap<List<UrunOzetDto>>> Listele(...) { ... }
```

---

## 11. 📊 ASSET PRELOADING (Blazor 10 OTOMATİK)

Blazor 10 yenisi:
- **Server side:** Link headers (kritik asset'ler için)
- **WASM:** High-priority download

**Yapılacak:** Hiçbir şey, otomatik.

`blazor.web.js` Blazor 10'da **%76 küçük** (183 KB → 43 KB).

---

## 12. 🛠 PROFİL ÇIKARMA

### 12.1 dotnet-counters (CLI)
```bash
dotnet-counters monitor --process-id 1234
dotnet-counters monitor -n [PROJE_ADI].Api --counters System.Runtime,Microsoft.AspNetCore.Hosting
```

### 12.2 MiniProfiler (Dev)
```csharp
builder.Services.AddMiniProfiler(opt =>
{
    opt.RouteBasePath = "/profiler";
}).AddEntityFramework();

app.UseMiniProfiler();
```
URL: `/profiler/results`

### 12.3 Application Insights / OpenTelemetry (Prod)
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter());
```

---

## 13. 🎯 OPTİMİZASYON CHECK LISTESI (DEPLOY ÖNCESİ)

```
[ ] Bundle boyutu < 5 MB (Blazor WASM)
[ ] Brotli + Gzip aktif
[ ] CSS minify edilmiş (production build)
[ ] JS minify edilmiş
[ ] Image lazy loading (loading="lazy")
[ ] Critical CSS inline
[ ] Font preload (<link rel="preload" as="font">)
[ ] FusionCache hit oranı > %70
[ ] DB sorgu yok (cache miss durumunda)
[ ] N+1 sorgu yok (EF profiler kontrol)
[ ] AsNoTracking() salt-okunurda
[ ] Lazy load admin assembly
[ ] CDN aktif (statik asset)
[ ] HTTP/2 veya HTTP/3
[ ] Service Worker cache (PWA)
[ ] Lighthouse skoru ≥ 90
```

---

## 14. 📋 ÖZ-DENETİM (12 Madde)

```
[ ] 1. FusionCache L1 + L2 (Redis) yapılandırılmış
[ ] 2. OnbellekYonetici wrapper kullanılıyor (doğrudan IMemoryCache YOK)
[ ] 3. Cache anahtar standardı uyumlu ([entite]:[islem]:[param])
[ ] 4. EF AsNoTracking salt-okunur listede
[ ] 5. EF AsSplitQuery multi-Include'da
[ ] 6. ProjectToType (Mapster) ile projection
[ ] 7. Blazor Virtualize uzun listede (>50)
[ ] 8. @key direktifi liste render'da
[ ] 9. [PersistentState] uygun yerde (Blazor 10)
[ ] 10. Lazy loading admin assembly
[ ] 11. ImageSharp.Web URL parametreleri (w=, fmt=)
[ ] 12. SignalR MessagePack aktif
```

---

*Versiyon: 1.0 | Tarih: 2026-05-14 | Bağlı: [05_Veritabani_EFCore10.md](05_Veritabani_EFCore10.md), [03_Razor_MudBlazor_Blazor10.md](03_Razor_MudBlazor_Blazor10.md)*
