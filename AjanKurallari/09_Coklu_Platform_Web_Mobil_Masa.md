---
name: coklu-platform-web-mobil-masa
description: Tek kod tabanı — Web (Blazor WASM/Server), Mobil (.NET MAUI + Blazor Hybrid), Masaüstü (WPF + WebView2 Blazor Hybrid), PWA. Paylaşılan *.Ortak kütüphanesi, admin 3-sütun layout, offline-first SQLite sync, MudBlazor alternatif tablosu, platform-spesifik kod ayrımı, push notification.
status: TAMAM
---

# 🖥📱💻 ÇOKLU PLATFORM — WEB / MOBİL / MASAÜSTÜ

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [03_Razor_MudBlazor_Blazor10.md](03_Razor_MudBlazor_Blazor10.md)
> **Felsefe:** Tek kod, çok platform. Razor bileşenleri **her yerde** çalışır.

---

## 1. 🗺 PLATFORM MATRİSİ

| Platform | Teknoloji | Render Mode | Ana Kullanım |
|---|---|---|---|
| **Web (Tarayıcı)** | Blazor WebAssembly + SSR | Interactive Auto | Ana site, admin paneli |
| **PWA** | Blazor WASM + Service Worker | WASM | Offline destek, install prompt |
| **Mobil (iOS/Android)** | .NET MAUI + Blazor Hybrid (BlazorWebView) | Native + WebView | Kullanıcı app'i, push, kamera, GPS |
| **Masaüstü (Windows)** | WPF + WebView2 + Blazor Hybrid | Native + WebView | Admin masaüstü, sistem tepsisi |
| **Masaüstü (Mac/Linux)** | .NET MAUI Desktop veya Avalonia + Blazor | Hybrid | Cross-platform desktop |
| **Kiosk / Kasa** | WPF veya MAUI (kilitli) | Hybrid | Tam ekran dokunmatik |

**Ortak:** %80+ kod paylaşılır (Razor bileşen + servis + DTO). Platform-spesifik %20 (kamera, GPS, dosya sistemi).

---

## 2. 📦 PAYLAŞILAN KÜTÜPHANE (`*.Ortak`)

### 2.1 Klasör Yapısı
```
[PROJE_ADI].Ortak/
├── Modeller/             (Entity, DTO, Enum — tüm platformlar paylaşır)
│   ├── Urunler/
│   ├── Kimlik/
│   └── Iletisim/
├── Dogrulayicilar/       (FluentValidation — platform-bağımsız)
├── Servisler/            (Interface'ler — implementasyon platform-spesifik)
│   ├── IUrunServisi.cs
│   └── IKimlikServisi.cs
├── Yardimcilar/          (extension method, helper)
└── Sabitler/             (const, enum)
```

### 2.2 Dependency Kuralı
```
Ortak  → Hiçbir şeye bağımlı değil (saf .NET 10)
Api    → Ortak'a bağımlı + EF Core + ASP.NET
UI     → Ortak'a bağımlı + MudBlazor + Blazor
MAUI   → Ortak'a + UI bileşenlerine bağımlı
WPF    → Ortak'a + UI bileşenlerine bağımlı
```

### 2.3 Yasaklar
```
❌ Ortak'a Blazor referansı (Razor yok)
❌ Ortak'a EF Core referansı
❌ Ortak'a HttpClient (platform yapılandırıyor)
```

---

## 3. 🌐 WEB (Blazor WebAssembly)

### 3.1 Render Mode (Blazor 10 Auto)
```razor
@page "/urunler"
@rendermode InteractiveAuto

<UrunListele />
```
- **Initial render:** SSR (hızlı first paint)
- **Interactive:** WASM (CSR — client-side)
- Otomatik geçiş — kullanıcı fark etmez

### 3.2 Program.cs (Standard)
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = ... });
builder.Services.AddScoped<DilServisi>();

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddInteractiveServerRenderMode();
app.Run();
```

### 3.3 Avantaj
- Tek kod tabanı (C#)
- Browser native (yükleme yok)
- SEO uyumlu (SSR ile)
- PWA olarak yüklenebilir

---

## 4. 📱 MOBİL (.NET MAUI + BLAZOR HYBRID)

### 4.1 Proje Yapısı
```bash
dotnet new maui-blazor -n [PROJE_ADI].Mobil
```

```
[PROJE_ADI].Mobil/
├── MauiProgram.cs
├── App.xaml
├── MainPage.xaml             ← BlazorWebView
├── Components/
│   └── Routes.razor           ← @page'leri içerir (Ortak Razor)
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── Windows/
│   └── MacCatalyst/
└── wwwroot/                  ← CSS, JS (web ile aynı)
```

### 4.2 MainPage.xaml (BlazorWebView)
```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:b="clr-namespace:Microsoft.AspNetCore.Components.WebView.Maui;assembly=Microsoft.AspNetCore.Components.WebView.Maui"
             xmlns:local="clr-namespace:[PROJE_ADI].UI.Pages;assembly=[PROJE_ADI].UI">

    <b:BlazorWebView HostPage="wwwroot/index.html">
        <b:BlazorWebView.RootComponents>
            <b:RootComponent Selector="#app" ComponentType="{x:Type local:Routes}" />
        </b:BlazorWebView.RootComponents>
    </b:BlazorWebView>
</ContentPage>
```

### 4.3 MauiProgram.cs
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "Inter");
                fonts.AddFont("PlayfairDisplay-Bold.ttf", "Playfair");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();

        // Platform-spesifik servisler
        builder.Services.AddSingleton<IKameraServisi, MauiKameraServisi>();
        builder.Services.AddSingleton<IKonumServisi, MauiKonumServisi>();
        builder.Services.AddSingleton<IBildirimServisi, MauiBildirimServisi>();

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://[00_PROJE_BILGISI.url_birincil]")
        });

        return builder.Build();
    }
}
```

### 4.4 Platform-Spesifik Servis (Kamera)
```csharp
public class MauiKameraServisi : IKameraServisi
{
    public async Task<byte[]?> FotografCekAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported) return null;

        var foto = await MediaPicker.Default.CapturePhotoAsync();
        if (foto is null) return null;

        using var akis = await foto.OpenReadAsync();
        using var ms = new MemoryStream();
        await akis.CopyToAsync(ms);
        return ms.ToArray();
    }
}
```

### 4.5 Push Notification (Firebase)
```csharp
public class MauiBildirimServisi : IBildirimServisi
{
    public async Task GosterAsync(string baslik, string mesaj)
    {
        var notification = new NotificationRequest
        {
            Title = baslik,
            Description = mesaj,
            BadgeNumber = 1
        };
        await LocalNotificationCenter.Current.Show(notification);
    }
}
```

---

## 5. 🖥 MASAÜSTÜ (WPF + WebView2 + Blazor Hybrid)

### 5.1 Proje
```bash
dotnet new wpf -n [PROJE_ADI].MasaUstu
dotnet add package Microsoft.AspNetCore.Components.WebView.Wpf
```

### 5.2 MainWindow.xaml
```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:blazor="clr-namespace:Microsoft.AspNetCore.Components.WebView.Wpf;assembly=Microsoft.AspNetCore.Components.WebView.Wpf"
        xmlns:routes="clr-namespace:[PROJE_ADI].UI.Pages;assembly=[PROJE_ADI].UI">

    <blazor:BlazorWebView HostPage="wwwroot\index.html" Services="{StaticResource services}">
        <blazor:BlazorWebView.RootComponents>
            <blazor:RootComponent Selector="#app" ComponentType="{x:Type routes:Routes}" />
        </blazor:BlazorWebView.RootComponents>
    </blazor:BlazorWebView>
</Window>
```

### 5.3 Sistem Tepsisi (System Tray)
```csharp
public partial class MainWindow : Window
{
    private NotifyIcon? _trayIcon;

    private void TraySimgeBaslat()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = new Icon("logo.ico"),
            Visible = true,
            Text = "[PROJE_ADI]",
            ContextMenuStrip = TepsiMenuOlustur()
        };
        _trayIcon.DoubleClick += (_, _) => Show();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized) Hide();
        base.OnStateChanged(e);
    }
}
```

### 5.4 Deploy
- **MSIX** (modern, Microsoft Store):
  ```bash
  dotnet publish -c Release -p:PublishProfile=MSIX
  ```
- **ClickOnce** (legacy ama yaygın)
- **WiX Installer** (özel kurulum)

---

## 6. 📲 PWA (PROGRESSIVE WEB APP)

### 6.1 manifest.json
```json
{
  "name": "[FIRMA_ADI]",
  "short_name": "[PROJE_ADI]",
  "start_url": "/",
  "display": "standalone",
  "theme_color": "[TEMA.ANA_RENK]",
  "background_color": "[TEMA.ARKAPLAN]",
  "icons": [
    { "src": "/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

### 6.2 Service Worker
```javascript
// wwwroot/service-worker.js
const cache_adi = '[PROJE_ADI]-v1';
const onbellek_dosyalari = [
  '/',
  '/css/sistem/tokens.css',
  '/manifest.json',
  '/icon-192.png'
];

self.addEventListener('install', e => {
  e.waitUntil(caches.open(cache_adi).then(c => c.addAll(onbellek_dosyalari)));
});

self.addEventListener('fetch', e => {
  e.respondWith(
    caches.match(e.request).then(cevap => cevap || fetch(e.request))
  );
});
```

### 6.3 Install Prompt
```razor
@inject IJSRuntime JS

<MudButton OnClick="KurAsync">Uygulamayı Yükle</MudButton>

@code-equivalent:
private async Task KurAsync() =>
    await JS.InvokeVoidAsync("pwaKurulumGoster");
```

---

## 7. 🛡 ADMİN LAYOUT (3 SÜTUN)

### 7.1 Hedef Görünüm
```
┌──────────────────────────────────────────────────────────┐
│ ÜST BAR — Logo, arama, bildirim, kullanıcı menüsü       │
├─────────┬────────────────────────────┬──────────────────┤
│ SOL     │ ORTA — Sayfa İçerik         │ SAĞ — Aktivite   │
│ DRAWER  │                             │ AKIŞI (SignalR)  │
│ 260px   │ Routing'le değişir          │ 320px            │
│ ↔72px   │                             │                  │
│         │                             │  🟢 Ali güncelle │
│ Menü    │  [Dashboard / İçerik vb.]   │  🟡 Yeni mesaj   │
│         │                             │  ⏱ Audit log     │
└─────────┴────────────────────────────┴──────────────────┘
```

### 7.2 MainLayout.razor (Yönetim)
```razor
@inherits LayoutComponentBase

<MudLayout>
    <MudAppBar Elevation="1">
        <MudIconButton Icon="@Icons.Material.Filled.Menu" OnClick="MenuToggle" />
        <MudText Typo="Typo.h6">@DilServisi.T("yonetim.baslik", "Yönetim")</MudText>
        <MudSpacer />
        <KomutPaleti />
        <BildirimZili />
        <KullaniciMenu />
    </MudAppBar>

    <MudDrawer @bind-Open="_solDrawerAcik" ClipMode="DrawerClipMode.Always" Variant="DrawerVariant.Mini">
        <DinamikMenu />
    </MudDrawer>

    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.False" Class="pa-6">
            @Body
        </MudContainer>
    </MudMainContent>

    <MudDrawer Anchor="Anchor.Right" @bind-Open="_sagDrawerAcik" Width="320px">
        <AktiviteAkisi />
    </MudDrawer>
</MudLayout>
```

### 7.3 Cihaz Bazlı Davranış
- **Masaüstü (>1280px):** Sol drawer açık (260px), sağ açık (320px)
- **Tablet (768-1280px):** Sol drawer mini (72px), sağ kapalı
- **Mobil (<768px):** Sol hamburger drawer, sağ alttan bottom sheet

---

## 8. 📐 RESPONSIVE STRATEJİSİ

| Cihaz | Genişlik | UI Patern |
|---|---|---|
| Mobil dikey | < 480px | Bottom nav (5 buton) + hamburger drawer |
| Mobil yatay / tablet dikey | 480-768px | Mini sidebar + bottom shortcuts |
| Tablet yatay | 768-1280px | Mini sidebar (ikon-only) + sağ drawer toggle |
| Masaüstü | > 1280px | Tam sidebar + 3 sütun |

**Touch optimizasyon:**
- Min hedef boyutu **44×44px**
- Hover yerine **active state** (mobile)
- Swipe gestures opsiyonel

---

## 9. 🔄 OFFLINE SENKRONİZASYON

### 9.1 Yerel SQLite
```csharp
// MAUI / WPF — yerel veritabanı
builder.Services.AddDbContext<YerelDbContext>(opt =>
    opt.UseSqlite($"Data Source={FileSystem.AppDataDirectory}/yerel.db"));
```

### 9.2 Senkronizasyon Servisi
```csharp
public class SenkronizasyonServisi(
    YerelDbContext yerel,
    IUrunServisi uzakServis,
    ILogger<SenkronizasyonServisi> log)
{
    public async Task GidenleriGonderAsync()
    {
        var bekleyenler = await yerel.BekleyenIslemler
            .Where(b => !b.GonderildiMi)
            .OrderBy(b => b.OlusturulmaTarihi)
            .ToListAsync();

        foreach (var islem in bekleyenler)
        {
            try
            {
                await uzakServis.IslemUygulaAsync(islem);
                islem.GonderildiMi = true;
                islem.GonderimTarihi = DateTime.UtcNow;
                await yerel.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Sync hatası. {Id}", islem.Id);
            }
        }
    }

    public async Task GelenleriAlAsync()
    {
        var sonSync = await yerel.SyncDurumu.Select(s => s.SonZaman).FirstOrDefaultAsync();
        var yeniler = await uzakServis.DegisikliklerAsync(sonSync);

        foreach (var u in yeniler)
            yerel.Urunler.Update(u);

        await yerel.SaveChangesAsync();
    }
}
```

### 9.3 Çakışma Stratejisi
- **Son kazanır** (varsayılan): server timestamp daha yeni → server kazanır
- **Manuel onay** (kritik veri): admin'e bildirim, çözüm bekler
- **Versiyonlama** (CRDT — opsiyonel): aynı kaydın çoklu sürüm birleştirme

### 9.4 Connectivity Kontrol
```csharp
public class BaglantiServisi
{
    public bool CevrimiciMi => Connectivity.NetworkAccess == NetworkAccess.Internet;

    public event Action<bool>? DurumDegisti;

    public BaglantiServisi()
    {
        Connectivity.ConnectivityChanged += (s, e) =>
            DurumDegisti?.Invoke(CevrimiciMi);
    }
}
```

---

## 10. 🎨 MUDBLAZOR ALTERNATİFLERİ

### 10.1 Karşılaştırma Tablosu

| Kütüphane | Bileşen | Lisans | Bundle | Güçlü Yön | Zayıf Yön |
|---|---|---|---|---|---|
| **MudBlazor** ⭐ | ~80 | MIT | Küçük | Saf C#, no-JS, SSR, ücretsiz, modern Material | Enterprise grid/scheduler sınırlı |
| **Radzen Blazor** | ~90 | MIT (Studio paid) | Orta | Güçlü DataGrid, WYSIWYG Studio | Tema esnekliği orta |
| **Telerik UI** | 110+ | Ticari ~$999/yıl | Büyük | Premium support, scheduler, pivot, en zengin | Pahalı, JS interop bağımlı |
| **Syncfusion** | 90+ | Ücretsiz (<$1M ciro) | Büyük | En geniş set, doc/Excel/PDF, 50+ chart | Lisans yönetimi karmaşık |
| **DevExpress** | ~80 | Ticari $1500+/yıl | Büyük | En güçlü grid + report | Pahalı, eski hava |
| **FluentUI Blazor** | ~60 | MIT | Orta | Microsoft resmi, M365 görünüm | Material değil, az bileşen |
| **Ant Design Blazor** | ~70 | MIT | Orta | Enterprise pattern, Çin pazarı | Türkçe/Batı zayıf |
| **BlazorBootstrap** | ~50 | MIT | Küçük | Bootstrap 5 tabanlı | BS5 JS çatışabilir |

### 10.2 Seçim Matrisi

| İhtiyaç | Öneri |
|---|---|
| Ücretsiz + dengeli + modern | **MudBlazor** (varsayılan) |
| Karmaşık DataGrid + ücretsiz | Radzen |
| Enterprise (scheduler, pivot, report) | Syncfusion (community) veya Telerik (paid) |
| Microsoft / M365 görünüm | FluentUI Blazor |
| Hızlı prototype | BlazorBootstrap |

### 10.3 Kural
- **Varsayılan:** MudBlazor (`AGENTS.md` zorunlu)
- **Alternatif gerekiyorsa:** Ustam onayı + bu kuralı güncelle

---

## 11. 🔀 PLATFORM-SPESİFİK KOD AYRIMI

### 11.1 Compile-Time Sembol
```csharp
#if ANDROID
    Android.Util.Log.Info("Tag", "Android'de çalışıyor");
#elif IOS
    Foundation.NSLog("iOS'ta çalışıyor");
#elif WINDOWS
    System.Diagnostics.Debug.WriteLine("Windows");
#elif MACCATALYST
    Console.WriteLine("Mac");
#endif
```

### 11.2 Partial Class — Platform Dosya Adlandırma
```
[PROJE_ADI].Mobil/
├── Servisler/
│   ├── KameraServisi.cs              ← Ortak interface implementation
│   ├── KameraServisi.Android.cs      ← Android-specific
│   ├── KameraServisi.iOS.cs          ← iOS-specific
│   └── KameraServisi.Windows.cs      ← Windows-specific
```

### 11.3 DI ile Platform Servisi
```csharp
// MAUI MauiProgram.cs
#if ANDROID
    builder.Services.AddSingleton<IKameraServisi, AndroidKameraServisi>();
#elif IOS
    builder.Services.AddSingleton<IKameraServisi, IOSKameraServisi>();
#endif
```

---

## 12. 🔔 PUSH NOTIFICATION

### 12.1 Web Push (VAPID)
```csharp
// Program.cs
builder.Services.AddWebPush(opt =>
{
    opt.Subject = "mailto:[iletisim.eposta]";
    opt.PublicKey = config["WebPush:PublicKey"];
    opt.PrivateKey = config["WebPush:PrivateKey"];
});

public class WebPushServisi(IPushService push)
{
    public async Task GonderAsync(WebPushAbonelik abone, string payload)
    {
        await push.SendNotificationAsync(abone.ToSubscription(), payload);
    }
}
```

### 12.2 Windows Toast Notification (WPF)
```csharp
using Microsoft.Toolkit.Uwp.Notifications;

new ToastContentBuilder()
    .AddText("Yeni Mesaj")
    .AddText("Müşteri Ali Demir size yazdı.")
    .Show();
```

### 12.3 MAUI Firebase Push
```csharp
public class MauiFirebaseServisi : IBildirimServisi
{
    public async Task TokenAlAsync()
    {
        // Firebase SDK token
    }

    public async Task GosterAsync(string baslik, string mesaj)
    {
        var notification = new NotificationRequest
        {
            Title = baslik,
            Description = mesaj,
            BadgeNumber = 1,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(1)
            }
        };
        await LocalNotificationCenter.Current.Show(notification);
    }
}
```

### 12.4 SignalR Canlı Toast (Kullanıcı Online Iken)
Detay: [06_API_Servisler_MediatR.md](06_API_Servisler_MediatR.md) §13

---

## 13. 🧪 TEST STRATEJİSİ

| Platform | Test Aracı |
|---|---|
| Razor bileşen | **bUnit** |
| API | xUnit + Testcontainers (PostgreSQL) |
| Web E2E | **Playwright** |
| MAUI UI | **MAUI UITest** (alpha) veya Appium |
| WPF UI | FlaUI veya White |

Detay: [10_Test_Derleme_Pipeline.md](10_Test_Derleme_Pipeline.md)

---

## 14. 🚢 BUILD & DEPLOY

### 14.1 Web (Build)
```bash
dotnet publish "[PROJE_ADI].Api/[PROJE_ADI].Api.csproj" -c Release -o /app/publish
```

### 14.2 MAUI Android (.aab)
```bash
dotnet publish -f net10.0-android -c Release \
    -p:AndroidPackageFormat=aab \
    -p:AndroidSigningKeyStore=mykey.keystore \
    -p:AndroidSigningKeyAlias=mykey
```

### 14.3 MAUI iOS (.ipa)
```bash
dotnet publish -f net10.0-ios -c Release \
    -p:CodesignKey="..." \
    -p:CodesignProvision="..."
```

### 14.4 WPF MSIX
```bash
dotnet publish -c Release -p:PublishProfile=MSIX
```

### 14.5 Coolify (Otomatik)
Git push → otomatik build → blue/green deploy. Detay: [10_Test_Derleme_Pipeline.md](10_Test_Derleme_Pipeline.md) §11.

---

## 15. 📋 ÖZ-DENETİM (13 Madde)

```
[ ] 1. Ortak kütüphanesinde platform-bağımlı kod YOK (saf .NET)
[ ] 2. Razor bileşenleri %80+ paylaşılabilir
[ ] 3. Platform servisleri interface üzerinden (IKameraServisi vb.)
[ ] 4. MAUI: Platforms/ klasör yapısı
[ ] 5. WPF: WebView2 + Blazor Hybrid
[ ] 6. PWA: manifest.json + service-worker.js
[ ] 7. Admin: 3 sütun layout (sol drawer + içerik + sağ aktivite)
[ ] 8. Responsive: mobil bottom nav, tablet mini sidebar, masaüstü tam
[ ] 9. Offline: yerel SQLite + sync servis
[ ] 10. MudBlazor varsayılan (alternatif Ustam onayı ile)
[ ] 11. Platform ayrımı: #if veya partial class file-conditional
[ ] 12. Push: Web Push + MAUI Firebase + WPF Toast wrapper
[ ] 13. Build: dotnet publish (web), MSIX (WPF), aab/ipa (MAUI)
```

---

*Versiyon: 1.0 | Tarih: 2026-05-14 | Bağlı: [03_Razor_MudBlazor_Blazor10.md](03_Razor_MudBlazor_Blazor10.md), [10_Test_Derleme_Pipeline.md](10_Test_Derleme_Pipeline.md)*
