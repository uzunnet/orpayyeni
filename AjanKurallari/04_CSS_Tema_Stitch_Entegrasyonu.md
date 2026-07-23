---
name: css-tema-stitch-entegrasyonu
description: tokens.css disiplini, klasör hiyerarşisi, dark mode, Stitch (Google) DESIGN.md → tokens.css → MudTheme akışı (build-time + runtime hot reload SignalR), GSAP/AOS/Lenis/Lottie wrapper isimleri, W3C DTCG standardı, glassmorphism efektleri.
---

# 🎨 CSS / TEMA / STITCH ENTEGRASYONU

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [00_PROJE_BILGISI.md](00_PROJE_BILGISI.md) (`tema.*` ve `stitch.*` bölümleri)

---

## 1. 🚫 YASAKLAR

```
❌ Hardcoded renk (#xxx, red, rgb()) — tokens kullan
❌ Hardcoded font ("Arial", "Inter") — var(--font-metin)
❌ Hardcoded boşluk (15px, 2rem) — var(--bosluk-md)
❌ .razor içinde <style> etiketi
❌ Inline style="..." (sadece dinamik değer zorunluysa)
❌ !important (specificity ile çöz)
❌ ID seçici (#id) — class kullan
❌ Stil tekrarı (DRY — bilesenler/ altına merkezi)
❌ JS animasyon doğrudan — Wrapper (AnimasyonMotoru)
```

---

## 2. 🗂 KLASÖR HİYERARŞİSİ (ZORUNLU)

```
[Proje].UI/wwwroot/css/sistem/
├── tokens.css                  ← TEK GİRİŞ (sadece @import)
├── temeller/
│   ├── degiskenler.css         (:root tokens — 00_PROJE_BILGISI'ten üretilir)
│   ├── reset.css               (modern CSS reset)
│   ├── tipografi.css           (font-face, başlık skala)
│   └── breakpoint.css          (responsive helper'lar)
├── bilesenler/
│   ├── butonlar.css
│   ├── kartlar.css
│   ├── tablolar.css
│   ├── modal.css
│   ├── form.css
│   ├── animasyon.css           (@keyframes, transition preset)
│   └── efektler.css            (glassmorphism, gradient, shadow)
└── moduller/
    ├── anasayfa.css
    ├── yonetim.css
    └── ...
```

### `tokens.css` İçeriği (Sadece İmport)
```css
/* TEK GİRİŞ — Stitch/00_PROJE_BILGISI'ten otomatik üretilir */
@import "temeller/degiskenler.css";
@import "temeller/reset.css";
@import "temeller/tipografi.css";
@import "temeller/breakpoint.css";

@import "bilesenler/butonlar.css";
@import "bilesenler/kartlar.css";
@import "bilesenler/tablolar.css";
@import "bilesenler/modal.css";
@import "bilesenler/form.css";
@import "bilesenler/animasyon.css";
@import "bilesenler/efektler.css";

@import "moduller/anasayfa.css";
@import "moduller/yonetim.css";
```

---

## 3. 🎨 `degiskenler.css` (00_PROJE_BILGISI'ten Üretilir)

**Bu dosya `StitchTemaServisi` tarafından otomatik yazılır** — elle düzenleme ÖNERİLMEZ.

Şablon (yer tutucular `[TEMA.X]` 00_PROJE_BILGISI'ten doldurulur):
```css
:root {
  /* === ANA RENKLER === */
  --ana-renk: [TEMA.ANA_RENK];           /* örn: #0a0a0a */
  --ana-renk-2: [TEMA.ANA_RENK_2];
  --ikincil-renk: [TEMA.IKINCIL_RENK];   /* marka rengi */
  --ikincil-renk-2: [TEMA.IKINCIL_RENK_2];
  --vurgu-renk: [TEMA.VURGU_RENK];
  --vurgu-parlak: [TEMA.VURGU_PARLAK];

  --arkaplan: [TEMA.ARKAPLAN];
  --arkaplan-yumusak: [TEMA.ARKAPLAN_YUMUSAK];
  --arkaplan-koyu: [TEMA.ARKAPLAN_KOYU];

  --metin: [TEMA.METIN];
  --metin-acik: [TEMA.METIN_ACIK];
  --metin-soluk: [TEMA.METIN_SOLUK];
  --metin-ters: #ffffff;

  --cizgi: #e8e6e0;
  --cizgi-koyu: #2a2a2a;

  --basari: [TEMA.BASARI];
  --uyari: [TEMA.UYARI];
  --hata: [TEMA.HATA];
  --bilgi: [TEMA.BILGI];

  /* === TİPOGRAFİ === */
  --font-baslik: '[FONT.BASLIK]', Georgia, serif;
  --font-metin: '[FONT.METIN]', -apple-system, sans-serif;
  --font-vurgu: '[FONT.VURGU]', serif;
  --font-mono: '[FONT.MONO]', Consolas, monospace;

  --boyut-xs: 0.75rem;
  --boyut-sm: 0.875rem;
  --boyut-md: 1rem;
  --boyut-lg: 1.25rem;
  --boyut-xl: 1.5rem;
  --boyut-2xl: 2rem;
  --boyut-3xl: 3rem;
  --boyut-4xl: 4.5rem;

  --kalin-acik: 300;
  --kalin-normal: 400;
  --kalin-orta: 500;
  --kalin-yari: 600;
  --kalin-koyu: 700;

  /* === BOŞLUK === */
  --bosluk-xs: 0.25rem;
  --bosluk-sm: 0.5rem;
  --bosluk-md: 1rem;
  --bosluk-lg: 1.5rem;
  --bosluk-xl: 2.5rem;
  --bosluk-2xl: 4rem;
  --bosluk-3xl: 6rem;

  /* === KÖŞE === */
  --kose-sm: 4px;
  --kose-md: 8px;
  --kose-lg: 12px;
  --kose-xl: 24px;
  --kose-tam: 9999px;

  /* === GÖLGE === */
  --golge-yumusak: 0 4px 20px rgba(0,0,0,0.08);
  --golge-orta:    0 10px 40px rgba(0,0,0,0.12);
  --golge-derin:   0 20px 60px rgba(0,0,0,0.18);
  --golge-marka:   0 20px 60px rgba([TEMA.IKINCIL_RENK_RGB], 0.25);
  --golge-ic:      inset 0 2px 8px rgba(0,0,0,0.06);

  /* === GEÇİŞ === */
  --gecis-hizli:    0.2s ease;
  --gecis-orta:     0.4s cubic-bezier(0.4, 0, 0.2, 1);
  --gecis-yavas:    0.8s cubic-bezier(0.4, 0, 0.2, 1);
  --gecis-yumusak:  0.6s cubic-bezier(0.22, 1, 0.36, 1);

  /* === BREAKPOINT === */
  --ekran-mobil: 480px;
  --ekran-tablet: 768px;
  --ekran-masaustu: 1280px;
  --ekran-genis: 1920px;

  /* === Z-INDEX === */
  --z-altta: -1;
  --z-normal: 1;
  --z-dropdown: 100;
  --z-sticky: 200;
  --z-modal: 1000;
  --z-toast: 1100;
  --z-tooltip: 1200;
}

/* === KOYU MOD === */
[data-tema="koyu"] {
  --arkaplan: [TEMA.ARKAPLAN_KOYU];
  --arkaplan-yumusak: #1a1a1a;
  --metin: #e8e6e0;
  --metin-acik: #b0aea8;
  --cizgi: #2a2a2a;
}
```

---

## 4. 🪟 GLASSMORPHISM (Marka İmza)

`bilesenler/efektler.css`:
```css
.cam-yuzey {
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border: 1px solid rgba(255, 255, 255, 0.12);
  box-shadow: var(--golge-orta);
}

.cam-yuzey-koyu {
  background: rgba(10, 10, 10, 0.6);
  backdrop-filter: blur(24px) saturate(180%);
  border: 1px solid var(--ikincil-renk);
}

.marka-parilti {
  background: linear-gradient(
    135deg,
    var(--ikincil-renk) 0%,
    var(--vurgu-renk) 50%,
    var(--ikincil-renk) 100%
  );
  background-size: 200% 200%;
  animation: marka-akis 3s ease infinite;
}

@keyframes marka-akis {
  0%, 100% { background-position: 0% 50%; }
  50%      { background-position: 100% 50%; }
}
```

---

## 5. 🎬 ANIMASYON KATMANI

### 5.1 Kütüphane → Wrapper Eşleşmesi
| Kütüphane | Amaç | Wrapper |
|---|---|---|
| **GSAP** | Timeline, tween, ScrollTrigger | `AnimasyonMotoru` |
| **AOS** | Scroll reveal | `KaydirmaAnimasyon` |
| **Lottie** | Vektörel JSON animasyon | `LottieOynatici` |
| **Lenis** | Smooth scroll | `YumusakKaydirma` |
| **Three.js** | 3D | `UcBoyutMotoru` |
| **Cropper.js** | Resim kırpma | `ResimDuzenleyici` |

### 5.2 CSS Transition Preset (`bilesenler/animasyon.css`)
```css
.gecis-fade   { transition: opacity var(--gecis-orta); }
.gecis-slide  { transition: transform var(--gecis-yumusak); }
.gecis-renk   { transition: background-color var(--gecis-hizli), color var(--gecis-hizli); }
.gecis-tum    { transition: all var(--gecis-orta); }

@keyframes belir {
  from { opacity: 0; transform: translateY(20px); }
  to   { opacity: 1; transform: translateY(0); }
}

@keyframes kaydir-sag {
  from { transform: translateX(-30px); opacity: 0; }
  to   { transform: translateX(0); opacity: 1; }
}

@keyframes nabız {
  0%, 100% { transform: scale(1); }
  50%      { transform: scale(1.05); }
}

@keyframes sonsuz-akis {
  from { transform: translateX(0); }
  to   { transform: translateX(-50%); }
}
```

### 5.3 GSAP ScrollTrigger (Wrapper Üzerinden)
```csharp
await _animasyonMotoru.ScrollTetikleyiciKurAsync(new
{
    Hedef = "#hero",
    Pin = true,
    Baslangic = "top top",
    Bitis = "+=100%",
    Scrub = true
});
```

### 5.4 Magnetic Cursor (Wrapper)
```csharp
await _animasyonMotoru.MagnetikButonAktifEtAsync("#ana-cta", maxKayma: 15);
```

---

## 6. 🎯 STITCH (GOOGLE) ENTEGRASYONU

### 6.1 Akış Genel Bakış
```
┌──────────────────────────────────────────────────────────┐
│  STITCH (stitch.withgoogle.com)                          │
│  → Tema oluştur, "Export DESIGN.md" tıkla                │
└──────────────────┬───────────────────────────────────────┘
                   ↓
        [tasarim/DESIGN.md] (git'te versiyonlu)
                   ↓
┌──────────────────────────────────────────────────────────┐
│  StitchTemaServisi.cs (Wrapper)                          │
│  - YamlDotNet ile front matter parse                     │
│  - System.Text.Json ile tokens parse                     │
│  - tokens.css üret + MudTheme nesnesi oluştur            │
└──────────────────┬───────────────────────────────────────┘
                   ↓
        [Build-time]                  [Runtime Hot Reload]
        tokens.css yazılır            SignalR broadcast →
        MudThemeProvider bind         tüm açık tarayıcılar
```

### 6.2 DESIGN.md Format (W3C DTCG)
Stitch çıktısı standart:
```markdown
---
name: "[Proje Tema]"
version: 1.0
tokens:
  color:
    primary:
      value: "#0a0a0a"
      type: color
    secondary:
      value: "#c19b76"
      type: color
  typography:
    heading:
      value: "Playfair Display"
      type: fontFamily
  spacing:
    md:
      value: "1rem"
      type: dimension
---

# [Marka] Design System
... markdown açıklamalar ...
```

### 6.3 C# Wrapper: `StitchTemaServisi`

**Konum:** `[Proje].Api/Moduller/Tema/Servisler/StitchTemaServisi.cs`

**NuGet:**
- `YamlDotNet` — front matter parse
- `System.Text.Json` — built-in
- `MudBlazor` — tema bind

**İskelet:**
```csharp
namespace [PROJE_ADI].Api.Moduller.Tema.Servisler;

public class StitchTemaServisi(
    IWebHostEnvironment ortam,
    IHubContext<TemaHub> temaHub,
    IOnbellekYonetici onbellek,
    ILogger<StitchTemaServisi> log)
{
    private const string CACHE_ANAHTAR = "stitch:tema:aktif";
    private const string CSS_HEDEF = "wwwroot/css/sistem/temeller/degiskenler.css";

    /// <summary>
    /// DESIGN.md dosyasını oku, parse et, DTO dön.
    /// </summary>
    public async Task<TemaDto> DesignMdOkuAsync(string yol)
    {
        var icerik = await File.ReadAllTextAsync(yol);
        // YamlDotNet ile front matter ayır
        // DTO'ya parse et
        return new TemaDto { /* ... */ };
    }

    /// <summary>
    /// TemaDto'dan tokens.css içeriğini üret.
    /// </summary>
    public string TokensCssUret(TemaDto tema) { /* string builder */ }

    /// <summary>
    /// TemaDto'dan MudBlazor MudTheme nesnesi üret.
    /// </summary>
    public MudTheme MudThemeOlustur(TemaDto tema)
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = tema.Renkler.AnaRenk,
                Secondary = tema.Renkler.IkincilRenk,
                // ...
            },
            Typography = new Typography
            {
                Default = new Default { FontFamily = [tema.Font.Metin] },
                H1 = new H1 { FontFamily = [tema.Font.Baslik] },
                // ...
            }
        };
    }

    /// <summary>
    /// Stitch DESIGN.md → tokens.css yaz + SignalR broadcast.
    /// </summary>
    public async Task SenkronEtAsync()
    {
        var yol = Path.Combine(ortam.ContentRootPath, "tasarim/DESIGN.md");
        if (!File.Exists(yol))
        {
            log.LogWarning("DESIGN.md bulunamadı: {Yol}", yol);
            return;
        }

        var tema = await DesignMdOkuAsync(yol);
        var css = TokensCssUret(tema);

        var cssYolu = Path.Combine(ortam.ContentRootPath, CSS_HEDEF);
        await File.WriteAllTextAsync(cssYolu, css);
        await onbellek.YazAsync(CACHE_ANAHTAR, tema, TimeSpan.FromDays(7));

        // Tüm açık tarayıcılara canlı yenileme sinyali
        await temaHub.Clients.All.SendAsync("TemaGuncellendi", tema);

        log.LogInformation("Stitch teması senkronize edildi. {Versiyon}", tema.Versiyon);
    }
}
```

### 6.4 SignalR Hub
```csharp
public class TemaHub : Hub
{
    // Client otomatik olarak "TemaGuncellendi" event'ini dinler
}
```

### 6.5 İstemci Tarafı (Razor)
```csharp
// App.razor.cs veya Layout
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _temaHubBaglantisi = new HubConnectionBuilder()
            .WithUrl(NavigasyonYoneticisi.ToAbsoluteUri("/temaHub"))
            .WithAutomaticReconnect()
            .Build();

        _temaHubBaglantisi.On<TemaDto>("TemaGuncellendi", async (tema) =>
        {
            await TemaServisi.UygulaAsync(tema);
            StateHasChanged();
        });

        await _temaHubBaglantisi.StartAsync();
    }
}
```

### 6.6 Build-Time Senkron
`Program.cs`:
```csharp
var app = builder.Build();

// Uygulama başlangıcında bir kez senkronize et
using (var scope = app.Services.CreateScope())
{
    var stitch = scope.ServiceProvider.GetRequiredService<StitchTemaServisi>();
    await stitch.SenkronEtAsync();
}
```

### 6.7 Admin'den Manuel Tetikleme
```csharp
// TemaKontrolcu
[HttpPost("stitch/senkron")]
[Authorize(Roles = "Admin")]
public async Task<Cevap<bool>> StitchSenkronEt()
{
    await _stitchTemaServisi.SenkronEtAsync();
    return Cevap<bool>.Basarili(true, "Tema güncellendi.");
}
```

### 6.8 Stitch Erişilemezse Fallback
`00_PROJE_BILGISI.md` `stitch.fallback_palet: "tema"` ise:
- DESIGN.md yoksa → `00_PROJE_BILGISI.tema.*` kullanılır
- Bu sayede Stitch opsiyonel kalır

---

## 7. 🌗 DARK MODE GEÇİŞİ

```csharp
public class TemaServisi(IJSRuntime js)
{
    public async Task TemaDegistirAsync(string tema)
    {
        // tema: "acik" veya "koyu"
        await js.InvokeVoidAsync("temaUygula", tema);
        await js.InvokeVoidAsync("localStorage.setItem", "tema", tema);
    }
}
```

JS (wrapper içinde — `wwwroot/js/tema.js`):
```javascript
window.temaUygula = (tema) => {
    document.documentElement.setAttribute('data-tema', tema);
};
```

---

## 8. 🎯 BUTONLAR (Örnek)

`bilesenler/butonlar.css`:
```css
.buton {
  display: inline-flex;
  align-items: center;
  gap: var(--bosluk-sm);
  padding: var(--bosluk-sm) var(--bosluk-lg);
  border: none;
  border-radius: var(--kose-md);
  font-family: var(--font-metin);
  font-weight: var(--kalin-orta);
  font-size: var(--boyut-md);
  cursor: pointer;
  transition: var(--gecis-orta);
}

.buton--ana {
  background: var(--ikincil-renk);
  color: var(--metin-ters);
}
.buton--ana:hover {
  background: var(--vurgu-renk);
  transform: translateY(-2px);
  box-shadow: var(--golge-marka);
}
```

---

## 9. 📋 ÖZ-DENETİM

```
[ ] tokens.css içinde sadece @import
[ ] :root tüm token tanımlı (00_PROJE_BILGISI'ten)
[ ] Hardcoded renk YOK
[ ] Hardcoded font YOK
[ ] Hardcoded boşluk YOK
[ ] !important YOK
[ ] ID seçici YOK
[ ] .razor içinde <style> YOK
[ ] Mobile-first (min-width)
[ ] @keyframes / GSAP wrapper
[ ] Glassmorphism efektler/efektler.css'te
[ ] CSS UTF-8 BOM
[ ] Her dosya başı Türkçe açıklama
[ ] Stitch: DESIGN.md yolu doğru
[ ] StitchTemaServisi DI'da kayıtlı
[ ] TemaHub map edilmiş
[ ] Hot reload SignalR aktif
```

---

*Versiyon: 1.0 | Bağlı: [03_Razor_MudBlazor_Blazor10.md](03_Razor_MudBlazor_Blazor10.md)*
*Stitch resmi: https://stitch.withgoogle.com/ | DTCG: W3C Design Tokens Format*
