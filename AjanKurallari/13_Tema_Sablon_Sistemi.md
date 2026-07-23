---
name: tema-sablon-sistemi
description: Tema = farklı bir site. Renk, tipografi, şekil, animasyon, layout, ikonografi, boşluk ritmi — hepsi temayla değişir. 20+ temaya ölçeklenebilir şablon mimarisi, Stitch import, super admin yönetim, frontend seçici. Tüm AI ajanlar (Claude, Cursor, Copilot, Windsurf) için ZORUNLU.
status: AKTIF — zorunlu
---

# 🎨 TEMA ŞABLON SİSTEMİ (Templates — Tema = Farklı Bir Site)

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [00_PROJE_BILGISI.md](00_PROJE_BILGISI.md) (`tema.*`), [04_CSS_Tema_Stitch_Entegrasyonu.md](04_CSS_Tema_Stitch_Entegrasyonu.md)

---

## 1. 🎯 TEMEL FELSEFE: TEMA = FARKLI BİR SİTE

> ⚠ **EN ÖNEMLİ KURAL:** Tema değişimi **sadece renk değişimi DEĞİLDİR**. Tema değiştiğinde site **görsel kimlik, davranış, layout ve his** olarak **tamamen farklı bir websitesi** olur.

Bir tema değişikliği şunların HEPSİNİ değiştirir:

| Kategori | Tema A → Tema B örneği |
|---|---|
| **Renk paleti** | Beyaz/altın → Onyx/glowing-gold |
| **Tipografi ailesi** | Noto Serif → Playfair Display (tüm başlık, gövde, mono) |
| **Tipografi skalası** | 1.125 ratio (modüler) → 1.333 ratio (dramatik, lux) |
| **Şekil dili** | Keskin 2-4px köşe → Yumuşak 8-16px yuvarlak |
| **Border stili** | 1px solid düz → 1px + outer glow (altın ışıma) |
| **Animasyon hızı** | 0.2s hızlı → 0.6s yavaş, dramatik cubic-bezier |
| **Hover davranışı** | translateY(-2px) → translateY(-6px) + glow pulse |
| **Layout grid** | 4 sütun sabit → 12 sütun fluid veya tek sütun |
| **Hero tipi** | Slider → Parallax → Fullscreen video |
| **Kart stili** | Solid (elevation) → Glassmorphism (frosted) → Frameless (image only) |
| **İkon seti** | Material Icons → Phosphor → Lucide |
| **Boşluk ritmi** | 8px base → 4px sıkı veya 12px ferah |
| **Gölge felsefesi** | Drop shadow → Glow shadow (altın) → Shadowless |
| **Header** | Solid bar → Glassmorphism frosted → Hidden (sidebar) |
| **Footer** | Çok sütunlu → Minimal logo + slogan → Tam ekran manifesto |

**Sonuç:** Aynı içerik, **iki farklı websitesi** hissi.

---

## 2. 🎯 AMAÇ

Sistem **tek-tek hardcoded tema** yerine **şablon-tabanlı** çalışır:
- 20+ temaya sorunsuz ölçeklenir
- Yeni tema eklemek için **kod değişikliği YOK** (sadece dosya + DB satırı)
- 3 ekleme yöntemi: **Elle (form)** | **Stitch MCP import** | **Manuel CSS yapıştırma**
- Super admin panelinden yönetim
- Frontend header'dan tek tıkla değişim, DB'ye kayıt
- Tüm görsel kimlik (renk + tipografi + şekil + animasyon + layout) temadan gelir

> ⚠ **Bu dosya HER yeni tema işinde, HER tema değişikliğinde, HER layout güncellemesinde okunur.**

---

## 3. 🗂 KLASÖR YAPISI (ÖLÇEKLENEBİLİR)

```
[Proje Kökü]/VizitLink3D.Api/wwwroot/css/temalar/
├── _sistem/                          ← ORTAK (tüm temalar, override edilmez)
│   ├── ortak-bilesenler.css          (.urun-kart, .btn, .navbar TEMEL class'lar)
│   ├── animasyon-ortak.css           (marka-akis, glow-dans, shimmer, vb.)
│   └── efektler-ortak.css            (glassmorphism, cam-yuzey, border-glow)
│
├── gold-luxury-dark/                 ← VARSAYILAN (default, ücretsiz)
│   ├── manifest.json                 (meta: ad, slug, premium, fiyat, shape, motion, layout, ...)
│   ├── tokens.css                    (renk/font/boşluk/gölge/radius)
│   ├── bilesenler.css                (kart/btn/navbar varyasyonları)
│   ├── animasyonlar.css              (tema-özgü keyframes)
│   └── ekran-goruntusu.jpg           (admin paneldeki küçük önizleme)
│
├── aurelian-onyx/                    ← Stitch'ten import (premium, glassmorphism)
├── midnight-noir/                    ← placeholder (kare köşe, dramatik animasyon)
├── marble-rose/                      ← placeholder (yuvarlak, sakin animasyon)
├── copper-bronze/                    ← placeholder (endüstriyel, hızlı animasyon)
├── sage-stone/                       ← placeholder (doğal, organik)
├── ocean-azure/                      ← placeholder (deniz temalı, yatay layout)
├── ember-red/                        ← placeholder (sıcak, cesur)
├── royal-purple/                     ← placeholder (asil, mistik)
├── ivory-champagne/                  ← placeholder (krem, lüks)
└── noir-graphite/                    ← placeholder (koyu, minimal)
```

**Kritik kural:** Her temanın kendi klasörü → 20 tema yüklense bile **CSS çakışması yok**. `:root[data-tema-id="..."]` seçicisi sadece aktif temayı hedefler.

---

## 4. 📄 MANIFEST.JSON ŞEMASI (GÖRSEL KİMLİK)

`/wwwroot/css/temalar/{slug}/manifest.json`:

```json
{
  "id": "aurelian-onyx",
  "kod": "AURELIAN_ONYX",
  "ad": "Aurelian Onyx",
  "slug": "aurelian-onyx",
  "aciklama": "Playfair Display + Space Grotesk, glassmorphism, glowing gold — Stitch AI lüks",
  "kaynak": "stitch",
  "stitchProjeId": "13800263520330366969",
  "aktif": true,
  "varsayilanMi": false,
  "premium": true,
  "fiyat": 0,
  "paraBirimi": "TRY",
  "thumbnailUrl": "/css/temalar/aurelian-onyx/ekran-goruntusu.jpg",
  "olusturulmaTarihi": "2026-06-30T00:00:00Z",
  "guncellenmeTarihi": null,
  "versiyon": 1,

  "renkler": {
    "birincil": "#050505",
    "ikincil": "#1e2020",
    "vurgu": "#d4af37",
    "vurguAcik": "#e9c349",
    "vurguKoyu": "#a07020",
    "arkaPlan": "#121414",
    "arkaPlan2": "#1a1a1a",
    "yuzey": "#1e2020",
    "yuzeyHover": "#282a2b",
    "cizgi": "rgba(255, 255, 255, 0.10)",
    "metin": "#e2e2e2",
    "metinIkincil": "#c4c7c7",
    "metinSoluk": "#8e9192",
    "metinTers": "#121414",
    "basari": "#4a7c59",
    "uyari": "#c9a449",
    "hata": "#ffb4ab",
    "bilgi": "#4a6c8c"
  },

  "tipografi": {
    "baslikAilesi": "Playfair Display",
    "baslikFallback": "'Noto Serif', serif",
    "baslikAgirlik": 700,
    "baslikHarfAraligi": "-0.02em",
    "govdeAilesi": "Space Grotesk",
    "govdeFallback": "'Manrope', system-ui, sans-serif",
    "govdeAgirlik": 400,
    "vurguAilesi": "Cormorant Garamond",
    "monoAilesi": "JetBrains Mono",
    "boyutSkalaRatio": 1.333,
    "baslikBuyuklukClamp": "clamp(2.5rem, 6vw, 5rem)"
  },

  "geometri": {
    "koseSm": 2,
    "koseMd": 4,
    "koseLg": 8,
    "koseXl": 16,
    "koseTam": 9999,
    "borderKalinlik": 1,
    "borderStil": "solid"
  },

  "golgeler": {
    "sm": "0 2px 8px rgba(0, 0, 0, 0.4)",
    "md": "0 4px 20px rgba(0, 0, 0, 0.5)",
    "lg": "0 10px 40px rgba(0, 0, 0, 0.6)",
    "xl": "0 20px 60px rgba(0, 0, 0, 0.7)",
    "vurgu": "0 0 15px rgba(212, 175, 55, 0.30)",
    "glowStil": "altin"
  },

  "glassmorphism": {
    "aktif": true,
    "blur": "20px",
    "blurSaturate": 1.8,
    "bgOpacity": 0.06,
    "borderOpacity": 0.10,
    "yariSaydam": true
  },

  "animasyon": {
    "hizi": "yavas",
    "gecisHizli": "0.15s ease",
    "gecisNormal": "0.3s cubic-bezier(0.4, 0, 0.2, 1)",
    "gecisYavas": "0.6s cubic-bezier(0.22, 1, 0.36, 1)",
    "cubicBezier": "cubic-bezier(0.22, 1, 0.36, 1)",
    "hoverYukseklik": 6,
    "hoverOlcek": 1.05,
    "scrollReveal": true,
    "magneticCursor": true,
    "pariltiEfekti": true,
    "shimmerEfekti": true,
    "tip": "dramatik-yumusak"
  },

  "layout": {
    "header": "glassmorphism",
    "footer": "minimal",
    "heroTipi": "fullscreen-slider-overlay",
    "kartStili": "glass",
    "icerikGenislik": 1440,
    "kenarBosluk": 80,
    "sutunSayisi": 4,
    "bolumAyirici": "cizgi"
  },

  "ikonSeti": "Material Icons Outlined",

  "glassmorphismAktif": true,
  "premium": true,

  "etiketler": ["premium", "glassmorphism", "karanlik", "altin", "lux", "editorial"]
}
```

**Manifest karşılaştırma örneği** (2 temanın farkı):

| Alan | gold-luxury-dark | aurelian-onyx |
|---|---|---|
| `tipografi.baslikAilesi` | Noto Serif | Playfair Display |
| `tipografi.boyutSkalaRatio` | 1.250 (kompakt) | 1.333 (dramatik) |
| `geometri.koseMd` | 4 | 4 (keskin) |
| `glassmorphism.aktif` | false | true |
| `animasyon.hizi` | normal | yavas |
| `animasyon.tip` | hizli-enerjik | dramatik-yumusak |
| `layout.heroTipi` | slider | fullscreen-slider-overlay |
| `layout.kartStili` | solid-elevation | glass |
| `layout.header` | solid-with-border | glassmorphism |

---

## 5. 🎨 TOKENS.CSS ŞABLONU

`/wwwroot/css/temalar/{slug}/tokens.css`:

```css
/*
 * {TEMA ADI} — tokens.css
 * CokluTemaServisi tarafından manifest.json'dan otomatik üretilir.
 * Manuel düzenleme ÖNERİLMEZ.
 */

:root[data-tema-id="{slug}"] {
    /* === RENK === */
    --tema-birincil: #050505;
    --tema-ikincil: #1e2020;
    --tema-vurgu: #d4af37;
    --tema-vurgu-acik: #e9c349;
    --tema-vurgu-koyu: #a07020;
    --tema-arkaplan: #121414;
    --tema-arkaplan-2: #1a1a1a;
    --tema-yuzey: #1e2020;
    --tema-yuzey-hover: #282a2b;
    --tema-cizgi: rgba(255, 255, 255, 0.10);
    --tema-cizgi-acik: rgba(255, 255, 255, 0.05);
    --tema-metin: #e2e2e2;
    --tema-metin-ikincil: #c4c7c7;
    --tema-metin-soluk: #8e9192;
    --tema-metin-ters: #121414;
    --tema-basari: #4a7c59;
    --tema-uyari: #c9a449;
    --tema-hata: #ffb4ab;
    --tema-bilgi: #4a6c8c;

    /* === TİPOGRAFİ === */
    --tema-font-baslik: 'Playfair Display', 'Noto Serif', serif;
    --tema-font-govde: 'Space Grotesk', 'Manrope', system-ui, sans-serif;
    --tema-font-vurgu: 'Cormorant Garamond', serif;
    --tema-font-mono: 'JetBrains Mono', monospace;
    --tema-baslik-agirlik: 700;
    --tema-baslik-harf-araligi: -0.02em;

    /* === BOŞLUK === */
    --tema-bosluk-xs: 0.25rem;
    --tema-bosluk-sm: 0.5rem;
    --tema-bosluk-md: 1rem;
    --tema-bosluk-lg: 1.5rem;
    --tema-bosluk-xl: 2.5rem;
    --tema-bosluk-2xl: 4rem;
    --tema-bosluk-3xl: 6rem;

    /* === GEOMETRİ (köşe, border) === */
    --tema-kose-sm: 2px;
    --tema-kose-md: 4px;
    --tema-kose-lg: 8px;
    --tema-kose-xl: 16px;
    --tema-kose-tam: 9999px;
    --tema-border-kalinlik: 1px;
    --tema-border-stil: solid;

    /* === GÖLGE === */
    --tema-golge-sm: 0 2px 8px rgba(0, 0, 0, 0.4);
    --tema-golge-md: 0 4px 20px rgba(0, 0, 0, 0.5);
    --tema-golge-lg: 0 10px 40px rgba(0, 0, 0, 0.6);
    --tema-golge-xl: 0 20px 60px rgba(0, 0, 0, 0.7);
    --tema-golge-vurgu: 0 0 15px rgba(212, 175, 55, 0.30);

    /* === GLASSMORPHİSM (sadece glassmorphism.aktif=true ise) === */
    --tema-cam-bg: rgba(255, 255, 255, 0.06);
    --tema-cam-cizgi: rgba(255, 255, 255, 0.10);
    --tema-cam-blur: blur(20px) saturate(180%);
    --tema-cam-glow: 0 0 15px rgba(212, 175, 55, 0.25);

    /* === ANİMASYON === */
    --tema-gecis-hizli: 0.15s ease;
    --tema-gecis-normal: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    --tema-gecis-yavas: 0.6s cubic-bezier(0.22, 1, 0.36, 1);
    --tema-bezier: cubic-bezier(0.22, 1, 0.36, 1);

    /* === LAYOUT === */
    --tema-icerik-genislik: 1440px;
    --tema-kenar-bosluk: 80px;
    --tema-sutun-sayisi: 4;

    /* === LEGACY UYUMLULUK (eski --vizit-* kullanımları için) === */
    --vizit-primary: var(--tema-birincil);
    --vizit-accent: var(--tema-vurgu);
    --vizit-bg-base: var(--tema-arkaplan);
    --vizit-text: var(--tema-metin);
    --vizit-text-inverse: var(--tema-metin-ters);
    --vizit-font-serif: var(--tema-font-baslik);
    --vizit-font-sans: var(--tema-font-govde);
    --vizit-radius-md: var(--tema-kose-md);
    --vizit-transition: var(--tema-gecis-normal);
    --vizit-shadow-md: var(--tema-golge-md);
    --vizit-shadow-lux: var(--tema-golge-vurgu);
    --vizit-border: var(--tema-cizgi);
}
```

---

## 6. 🧩 BİLEŞENLER.CSS (TEMA-ÖZGÜ GÖRSEL KİMLİK)

`/wwwroot/css/temalar/{slug}/bilesenler.css`:

```css
/*
 * {TEMA ADI} — bilesenler.css
 * Tema-özgü component varyasyonları. _sistem/ortak-bilesenler.css'i override eder.
 * Her tema farklı kart, buton, navbar, footer görünümü tanımlar.
 */

:root[data-tema-id="aurelian-onyx"] {

    /* ─── BAŞLIKLAR (tipografi hiyerarşisi) ─── */
    h1, h2, h3, h4 {
        font-family: var(--tema-font-baslik);
        font-weight: var(--tema-baslik-agirlik);
        letter-spacing: var(--tema-baslik-harf-araligi);
        line-height: 1.1;
    }
    h1 { font-size: clamp(2.5rem, 6vw, 5rem); }
    h2 { font-size: clamp(2rem, 4vw, 3.5rem); }
    h3 { font-size: clamp(1.5rem, 3vw, 2.5rem); }

    /* ─── NAVBAR (glassmorphism varyantı) ─── */
    .navbar {
        background: color-mix(in srgb, var(--tema-birincil) 75%, transparent);
        backdrop-filter: var(--tema-cam-blur);
        -webkit-backdrop-filter: var(--tema-cam-blur);
        border-bottom: 1px solid var(--tema-cam-cizgi);
        box-shadow: 0 1px 0 0 var(--tema-cam-glow), 0 10px 30px rgba(0,0,0,0.4);
        transition: var(--tema-gecis-normal);
    }

    .navbar-link {
        font-family: var(--tema-font-govde);
        font-weight: 600;
        letter-spacing: 0.05em;
        color: var(--tema-metin);
        position: relative;
    }

    .navbar-link.aktif::after {
        content: "";
        position: absolute;
        bottom: -4px; left: 50%;
        transform: translateX(-50%);
        width: 4px; height: 4px;
        background: var(--tema-vurgu);
        border-radius: 50%;
        box-shadow: 0 0 8px var(--tema-vurgu);
    }

    /* ─── ÜRÜN KARTI (glassmorphism varyantı) ─── */
    .urun-kart {
        background: var(--tema-cam-bg);
        backdrop-filter: var(--tema-cam-blur);
        -webkit-backdrop-filter: var(--tema-cam-blur);
        border: 1px solid var(--tema-cam-cizgi);
        border-radius: var(--tema-kose-md);
        box-shadow: var(--tema-golge-md);
        overflow: hidden;
        transition: transform var(--tema-gecis-yavas),
                    box-shadow var(--tema-gecis-yavas),
                    border-color var(--tema-gecis-normal);
    }

    .urun-kart::before {
        content: "";
        position: absolute;
        top: 0; left: 0; right: 0; height: 1px;
        background: linear-gradient(90deg, transparent, var(--tema-vurgu), transparent);
        opacity: 0.4;
    }

    .urun-kart:hover {
        transform: translateY(-6px);
        border-color: var(--tema-vurgu);
        box-shadow: var(--tema-cam-glow), var(--tema-golge-lg);
    }

    .urun-kart-gorsel {
        aspect-ratio: 4/3;
        overflow: hidden;
        transition: transform var(--tema-gecis-yavas);
    }

    .urun-kart:hover .urun-kart-gorsel {
        transform: scale(1.05);
    }

    .urun-kart-baslik {
        font-family: var(--tema-font-baslik);
        font-weight: 600;
        color: var(--tema-metin);
    }

    .urun-kart-fiyat {
        font-family: var(--tema-font-baslik);
        font-style: italic;
        color: var(--tema-vurgu);
        font-weight: 600;
    }

    /* ─── BUTONLAR ─── */
    .btn-birincil {
        background: var(--tema-vurgu);
        color: var(--tema-metin-ters);
        border: 1px solid var(--tema-vurgu);
        border-radius: var(--tema-kose-md);
        padding: var(--tema-bosluk-sm) var(--tema-bosluk-lg);
        font-family: var(--tema-font-govde);
        font-weight: 600;
        letter-spacing: 0.02em;
        cursor: pointer;
        transition: all var(--tema-gecis-normal);
    }

    .btn-birincil:hover {
        background: var(--tema-vurgu-acik);
        box-shadow: 0 0 25px var(--tema-vurgu);
        transform: translateY(-2px);
    }

    .btn-ghost {
        background: transparent;
        color: var(--tema-metin);
        border: 1px solid var(--tema-cam-cizgi);
        backdrop-filter: var(--tema-cam-blur);
        border-radius: var(--tema-kose-md);
        padding: var(--tema-bosluk-sm) var(--tema-bosluk-lg);
        font-family: var(--tema-font-govde);
        transition: all var(--tema-gecis-normal);
    }

    .btn-ghost:hover {
        border-color: var(--tema-vurgu);
        color: var(--tema-vurgu);
    }

    /* ─── HERO SLIDER ─── */
    .hero-overlay {
        background: linear-gradient(135deg,
            color-mix(in srgb, var(--tema-birincil) 85%, transparent) 0%,
            color-mix(in srgb, var(--tema-arkaplan) 65%, transparent) 100%);
        backdrop-filter: blur(2px);
    }

    .hero-baslik {
        font-family: var(--tema-font-baslik);
        font-weight: var(--tema-baslik-agirlik);
        letter-spacing: var(--tema-baslik-harf-araligi);
        color: var(--tema-metin);
        font-size: clamp(2.5rem, 6vw, 5rem);
    }

    .hero-alt-baslik {
        font-family: var(--tema-font-govde);
        font-weight: 600;
        letter-spacing: 0.2em;
        text-transform: uppercase;
        color: var(--tema-vurgu);
    }

    .hero-aciklama {
        font-family: var(--tema-font-govde);
        color: var(--tema-metin-ikincil);
        font-size: 1.125rem;
        line-height: 1.6;
    }

    /* ─── FOOTER ─── */
    .footer {
        background: var(--tema-birincil);
        color: var(--tema-metin);
        border-top: 1px solid var(--tema-cam-cizgi);
        padding: var(--tema-bosluk-3xl) var(--tema-bosluk-lg);
    }

    .footer-baslik {
        font-family: var(--tema-font-govde);
        font-weight: 600;
        letter-spacing: 0.1em;
        text-transform: uppercase;
        color: var(--tema-vurgu);
    }

    .footer-link {
        color: var(--tema-metin-ikincil);
        font-family: var(--tema-font-govde);
        transition: color var(--tema-gecis-hizli);
    }

    .footer-link:hover {
        color: var(--tema-vurgu);
    }

    /* ─── DİĞER (MUD BLAZOR UYUMLULUĞU) ─── */
    .mud-button-filled { background: var(--tema-vurgu) !important; color: var(--tema-metin-ters) !important; }
    .mud-button-outlined { border-color: var(--tema-vurgu) !important; color: var(--tema-vurgu) !important; }
    .mud-paper { background: var(--tema-yuzey) !important; color: var(--tema-metin) !important; }
    .mud-input { color: var(--tema-metin) !important; }
    .mud-typography { color: var(--tema-metin) !important; }
}
```

**Fark:** gold-luxury-dark teması `.urun-kart`'ı **solid + elevation** ile, aurelian-onyx ise **glassmorphism** ile tanımlar. Aynı component, iki tamamen farklı görünüm.

---

## 7. 🎬 ANİMASYONLAR.CSS (TEMA-ÖZGÜ MOTION)

`/wwwroot/css/temalar/{slug}/animasyonlar.css`:

```css
/*
 * {TEMA ADI} — animasyonlar.css
 * Tema-özgü keyframe ve motion preset'leri.
 * Hız, easing, hover mesafesi, scroll reveal — her temaya göre değişir.
 */

:root[data-tema-id="aurelian-onyx"] {
    /* Marka parıltı (altın gradient flow) */
    @keyframes tema-marka-akis {
        0%, 100% { background-position: 0% 50%; }
        50%      { background-position: 100% 50%; }
    }

    /* Glow dans (altın pulse) */
    @keyframes tema-glow-dans {
        0%, 100% { box-shadow: 0 0 10px var(--tema-vurgu); }
        50%      { box-shadow: 0 0 30px var(--tema-vurgu-acik), 0 0 60px var(--tema-vurgu); }
    }

    /* Float (yumuşak yüzme) */
    @keyframes tema-float {
        0%, 100% { transform: translateY(0); }
        50%      { transform: translateY(-8px); }
    }

    /* Shimmer (cam parıltı) */
    @keyframes tema-shimmer {
        0%   { opacity: 0.6; transform: translateX(-100%) skewX(-15deg); }
        50%  { opacity: 0.2; }
        100% { opacity: 0.6; transform: translateX(200%) skewX(-15deg); }
    }

    /* Scroll reveal (dramatik, yavaş) */
    @keyframes tema-goruntu-belir {
        from { opacity: 0; transform: translateY(40px); }
        to   { opacity: 1; transform: translateY(0); }
    }

    /* Uygulama sınıfları */
    .anim-marka-akis { animation: tema-marka-akis 3s ease infinite; }
    .anim-glow-dans { animation: tema-glow-dans 2.4s ease-in-out infinite; }
    .anim-float { animation: tema-float 4s ease-in-out infinite; }
    .anim-shimmer { animation: tema-shimmer 0.8s ease-in-out forwards; }
    .anim-goruntu-belir {
        animation: tema-goruntu-belir var(--tema-gecis-yavas) var(--tema-bezier) forwards;
    }
}
```

**Fark:** gold-luxury-dark `.anim-glow-dans`'ı olmayabilir (sade shadow teması). aurelian-onyx her animasyonu tanımlar. **Tema = farklı motion dili.**

---

## 8. 🏷 CSS CLASS NAMING CONVENTION

| Kullanım | Class Adı | Örnek |
|---|---|---|
| Tema genel override | `:root[data-tema-id="{slug}"]` | `:root[data-tema-id="aurelian-onyx"]` |
| Eski uyumluluk | `:root[data-site-tema="{slug}"]` | (geriye uyum, yeni yazılımda kullanma) |
| Tema-özgü varyant | `.{component}--{slug}` | `.urun-kart--aurelian`, `.btn--copper` |
| Tema-özgü modifier | `.{component}.{slug}-tema` | `.navbar.aurelian-tema` |
| Tema prefix'li BEM | `.tema-{slug}__{component}` | `.tema-aurelian__hero-title` |

**Razor kullanımı:**
```razor
<div class="urun-kart urun-kart--@_aktifTemaSlug">...</div>
<button class="btn-birincil">@Buton Metni</button>
```

**Önemli:** Tema değişkenleri `var(--tema-*)` ile, **hardcoded renk/font/şekil/animasyon ASLA**. Tüm görsel kimlik temadan gelir.

---

## 9. ➕ YENİ TEMA EKLEME (3 YÖNTEM)

### 9.1 Yöntem A — Elle (Super Admin Form)

`/admin/super/temalar/yeni` formu:
- **Kimlik:** `ad`, `kod`, `slug` (URL-safe, küçük harf, tire)
- **Fiyatlandırma:** `premium`, `fiyat`, `paraBirimi`
- **Renkler:** 18 alan (birincil, vurgu, arkaPlan, metin, vb.)
- **Tipografi:** 9 alan (baslikAilesi, govdeAilesi, agirlik, harfAraligi, boyutSkalaRatio)
- **Geometri:** 6 alan (koseSm/Md/Lg/Xl/Tam, border)
- **Gölgeler:** 6 alan (sm, md, lg, xl, vurgu, glowStil)
- **Glassmorphism:** 5 alan (aktif, blur, opacity, vb.)
- **Animasyon:** 10 alan (hiz, easing, hover, scroll, magnetic, vb.)
- **Layout:** 8 alan (header, footer, heroTipi, kartStili, sutunSayisi, vb.)
- **İkon seti:** 1 alan
- **Görsel:** thumbnail yükle

**Sonuç:** `CokluTemaServisi` klasörü oluşturur → `manifest.json` + `tokens.css` + `bilesenler.css` + `animasyonlar.css` placeholder'lar yazar.

### 9.2 Yöntem B — Stitch MCP Import

`/admin/super/temalar/stitch-import`:
1. **Stitch proje URL/ID** gir (örn. `13800263520330366969`)
2. Sistem `stitch_get_project` ile projeyi çeker
3. `designTheme.bodyFont`, `customColor`, `typography`, `namedColors`, `spacing` parse edilir
4. `tokens.css` otomatik üretilir
5. `bilesenler.css` glassmorphism aktifse glassmorphism varyasyonlarla, değilse solid varyasyonlarla yazılır
6. `animasyonlar.css` tipografi ve renk paletine göre üretilir
7. `ekran-goruntusu.jpg` `thumbnailScreenshot.downloadUrl`'den indirilir
8. `manifest.json` oluşturulur
9. **Önizleme onayı** → kaydet

**Kod:** `VizitLink3D.Api/Moduller/Tema/Servisler/StitchImporterServisi.cs`

### 9.3 Yöntem C — Manuel CSS Yapıştırma

`/admin/super/temalar/manuel`:
1. Form: `ad`, `slug`, `glassmorphismAktif`, layout tipi
2. Textarea: **tokens.css** (tüm :root tanımları)
3. Textarea: **bilesenler.css** (component varyasyonları)
4. Textarea: **animasyonlar.css** (keyframes)
5. Kaydet → dosyalar yazılır

---

## 10. 🗄 VERİTABANI ŞEMASI

```csharp
public class TemaSablonu
{
    public long Id { get; set; }
    public string Kod { get; set; } = "";            // AURELIAN_ONYX
    public string Ad { get; set; } = "";             // Aurelian Onyx
    public string Slug { get; set; } = "";           // aurelian-onyx
    public string Aciklama { get; set; } = "";
    public string Kaynak { get; set; } = "elle";     // varsayilan | stitch | manuel | elle
    public string? StitchProjeId { get; set; }
    public bool GlassmorphismAktif { get; set; }
    public bool Premium { get; set; }
    public decimal Fiyat { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public string? ThumbnailUrl { get; set; }
    public bool Aktif { get; set; } = true;
    public bool VarsayilanMi { get; set; }
    public string Etiketler { get; set; } = "";
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int Versiyon { get; set; } = 1;
    public bool SilindiMi { get; set; }

    // Görsel kimlik (manifest'ten deserialize)
    public string RenklerJson { get; set; } = "{}";
    public string TipografiJson { get; set; } = "{}";
    public string GeometriJson { get; set; } = "{}";
    public string GolgelerJson { get; set; } = "{}";
    public string GlassmorphismJson { get; set; } = "{}";
    public string AnimasyonJson { get; set; } = "{}";
    public string LayoutJson { get; set; } = "{}";
    public string IkonSeti { get; set; } = "Material Icons";
}

public class FirmaTemaAtama
{
    public long Id { get; set; }
    public int FirmaId { get; set; }
    public long TemaSablonId { get; set; }
    public bool Aktif { get; set; } = true;
    public DateTime AtamaTarihi { get; set; } = DateTime.UtcNow;
    public string? OzelDegiskenlerJson { get; set; }
}
```

**DB'den dinamik yükleme:** Sayfa açılışında `CokluTemaServisi.TumTemalariGetir()` DB'den çeker. manifest.json dosyaları **cache**'te tutulur (`FusionCache`, 7 gün TTL).

---

## 11. 👑 SUPER ADMIN PANELİ

**Yol:** `/admin/super/temalar`

### 11.1 Liste Sayfası
- Tüm `tema_sablonlari` (aktif + pasif + silinen)
- Kart grid: **thumbnail + ad + slug + premium + fiyat + etiketler + aktiflik**
- **Önizleme butonu** (iframe)
- Aksiyonlar: Düzenle / Pasif Yap / Aktif Et / Sil / Versiyon Geçmişi

### 11.2 Yeni Tema (3 sekme)
- **Elle Form** (Yöntem A — 50+ alan)
- **Stitch'ten İçe Aktar** (Yöntem B — proje ID + önizleme)
- **Manuel CSS** (Yöntem C — textarea)

### 11.3 Tema Düzenle
- Aynı form, mevcut değerler dolu
- **Canlı önizleme** (sağda iframe)
- **Versiyon geçmişi** (geri al)
- **CSS dosya editörü** (modal, syntax highlight)

### 11.4 Önizleme
- Farklı cihaz boyutları (mobile/tablet/desktop)
- Farklı sayfalar (anasayfa, ürün listesi, ürün detay, iletişim)
- **Side-by-side** (2 temayı karşılaştır)

---

## 12. 🖥 FRONTEND TEMA SEÇİCİ

**Yer:** Header'da, dil seçicinin yanında.

**Bileşen:** `<TemaSecici />` (`VizitLink3D.UI/Bilesenler/Tema/TemaSecici.razor`)

**Davranış:**
1. Tıkla → dropdown açılır
2. Tüm aktif temalar (DB'den)
3. Her satır: thumbnail + ad + premium rozeti + etiketler
4. Tıkla → `localStorage.setItem("temaId", slug)` + `document.documentElement.setAttribute("data-tema-id", slug)`
5. Aktif temanın `tokens.css` + `bilesenler.css` + `animasyonlar.css` **lazy load**
6. CSS değişkenleri anında güncellenir, **sayfa yenilenmez**
7. `POST /api/firma-tema` ile DB'ye kaydet
8. `TemaGuncellendi` SignalR event'i broadcast → tüm açık sekmeler

**Premium kontrolü:**
- `localStorage.firmaPremiumSeviye` >= gerekli → serbest
- Aksi → "Bu tema premium. Yükseltmek için tıklayın" snackbar

---

## 13. 🔄 AKıŞ ŞEMASI

```
[Super Admin: /admin/super/temalar]
  ├── Elle Form ───────┐
  ├── Stitch Import ───┤
  └── Manuel CSS ──────┤
                       ↓
              [CokluTemaServisi.TemaEkleAsync]
                       ↓
       ┌───────────────┼───────────────┐
       ↓               ↓               ↓
  DB kayıt       Klasör oluştur   CSS dosyaları yaz
  (tema_sablonlari) (wwwroot/css/  (tokens.css,
                    temalar/{slug})  bilesenler.css,
                                     animasyonlar.css,
                                     manifest.json)
                       ↓
            [Tema Secici (Frontend)]
                       ↓
         data-tema-id="{slug}" set
                       ↓
           Lazy load CSS dosyaları
                       ↓
             Sayfa anında farklı site olur
```

---

## 14. 📊 TEMA GÖRSEL KİMLİK MATRİSİ (10 PLACEHOLDER)

| # | Slug | Renk Özeti | Tipografi | Şekil | Animasyon | Layout | Cam? |
|---|---|---|---|---|---|---|---|
| 1 | **gold-luxury-dark** | Beyaz/altın | Noto Serif + Manrope | 4px | hızlı | slider, 4-sütun | ✗ |
| 2 | **aurelian-onyx** | Onyx/glowing-gold | Playfair + Space Grotesk | 4px | dramatik | overlay, 4-sütun | ✓ |
| 3 | **midnight-noir** | Saf siyah/gümüş | Inter + Inter | 0px (kare) | ani, keskin | liste, 1-sütun | ✗ |
| 4 | **marble-rose** | Krem/rose-gold | Cormorant + Inter | 16px (yuvarlak) | sakin, yavaş | grid, 2-sütun | ✗ |
| 5 | **copper-bronze** | Bakır/yeşil | Bebas Neue + IBM Plex | 2px (endüstriyel) | hızlı, mekanik | dikey, 3-sütun | ✗ |
| 6 | **sage-stone** | Sage/taş | Fraunces + DM Sans | 8px (organik) | yumuşak, doğal | grid, 3-sütun | ✗ |
| 7 | **ocean-azure** | Mavi/beyaz | Playfair + Manrope | 12px | akıcı, dalgalı | slider, 3-sütun | ✓ |
| 8 | **ember-red** | Koyu kırmızı/siyah | Anton + Roboto | 0px (keskin) | dramatik, agresif | 1-sütun hero | ✗ |
| 9 | **royal-purple** | Mor/altın | Cinzel + Lato | 6px (asil) | yavaş, dramatik | 4-sütun, vignette | ✓ |
| 10 | **ivory-champagne** | Krem/şampanya | Italiana + Italiana Sans | 24px (lüks yuvarlak) | yavaş, sakin | 2-sütun, geniş | ✗ |
| 11 | **noir-graphite** | Grafit/bakır | JetBrains Mono + Mono | 0px (teknik) | mekanik, hızlı | 4-sütun, monospace | ✗ |
| 12-20 | (placeholder) | … | … | … | … | … | … |

**Her placeholder, Stitch import veya elle doldurulur.**

---

## 15. ✅ ÖZ-DENETİM (Tema Ekleme Öncesi)

```
[ ] /wwwroot/css/temalar/{slug}/ klasörü var mı?
[ ] manifest.json TÜM alanları içeriyor mu (renkler, tipografi, geometri, glassmorphism, animasyon, layout)?
[ ] tokens.css :root[data-tema-id="{slug}"] kapsamında mı?
[ ] tokens.css hardcoded renk/font/boşluk YOK mu? (tümü --tema-*)
[ ] tokens.css geriye uyumlu --vizit-* alias'ları var mı?
[ ] bilesenler.css sadece tema-özgü override'ları içeriyor mu?
[ ] bilesenler.css farklı tipografi/şekil/animasyon tanımlıyor mu? (sadece renk değil)
[ ] animasyonlar.css tema-özgü keyframe'ler içeriyor mu?
[ ] class isimleri convention'a uyuyor mu (.urun-kart--{slug} vb.)?
[ ] DB'de tema_sablonlari satırı eklendi mi?
[ ] Yeni tema Tema Secici dropdown'ında görünüyor mu?
[ ] Aktif edilince sayfa GERÇEKTEN farklı görünüyor mu (sadece renk değil)?
[ ] Tema değişimi SignalR ile broadcast oluyor mu?
[ ] Build 0 hata mı?
[ ] Tüm mevcut sayfalar yeni temayla doğru render oluyor mu?
[ ] TEMA = FARKLI SİTE felsefesi sağlandı mı? (şekil, animasyon, layout da değişti mi?)
```

---

## 16. 🚫 YASAKLAR (Tema İşlerinde)

| # | Yasak | Sebep |
|---|---|---|
| 1 | Tema-özgü değerleri `_sistem/ortak-*.css` dosyalarına yazmak | Tüm temaları bozar |
| 2 | Hardcoded renk/font/şekil/animasyon (tokens.css'te) | Tema değişmez |
| 3 | Tema adında Türkçe karakter (slug'da) | URL sorunu |
| 4 | Aynı slug ile iki tema | Çakışma |
| 5 | `data-tema-id` yerine sadece `data-site-tema` kullanmak (yeni yazılımda) | Yeni mimari |
| 6 | Tema CSS'ini sadece API projesinde tutmak (Blazor WASM göremez) | ROBOCOPY / build adımı gerekli |
| 7 | `:root` seviyesinde tema değerleri (global etki) | Tüm temaları override eder |
| 8 | `!important` (specificity ile çöz) | Override edilemez hale gelir |
| 9 | tokens.css'te @import kullanmak | Performans, lazy load bozulur |
| 10 | Tema değişikliğini sadece JS ile yapmak (CSS dosyası değişmeden) | Refresh sonrası eski tema |
| 11 | **Tema değişimi = sadece renk değişimi sanmak** | TEMA = FARKLI SİTE felsefesi ihlali |
| 12 | Tipografi/şekil/animasyon değiştirmeden sadece renk değiştirmek | Yarım tema |
| 13 | Layout değiştirmeden sadece renk değiştirmek | Yarım tema |
| 14 | Aynı component için 2 farklı temada aynı class yapısı (override edilemez) | Çakışma |

---

## 17. 📋 MANIFEST HIZLI ŞABLON

```json
{
  "id": "{slug}",
  "kod": "{UPPER_SNAKE_CASE}",
  "ad": "{Tema Görünen Adı}",
  "slug": "{slug}",
  "aciklama": "{Kısa açıklama}",
  "kaynak": "elle",
  "stitchProjeId": null,
  "aktif": true,
  "varsayilanMi": false,
  "premium": false,
  "fiyat": 0,
  "paraBirimi": "TRY",
  "thumbnailUrl": "/css/temalar/{slug}/ekran-goruntusu.jpg",
  "olusturulmaTarihi": "{ISO8601}",
  "versiyon": 1,
  "renkler": { "birincil": "#000000", "ikincil": "#222222", "vurgu": "#C5A059", "arkaPlan": "#ffffff", "metin": "#1A1A1A" },
  "tipografi": { "baslikAilesi": "Noto Serif", "govdeAilesi": "Manrope", "boyutSkalaRatio": 1.250 },
  "geometri": { "koseSm": 2, "koseMd": 4, "koseLg": 8, "koseXl": 16, "koseTam": 9999 },
  "golgeler": { "sm": "0 2px 8px rgba(0,0,0,0.04)", "md": "0 4px 20px rgba(0,0,0,0.06)", "lg": "0 10px 40px rgba(0,0,0,0.10)" },
  "glassmorphism": { "aktif": false, "blur": "12px", "bgOpacity": 0.7, "borderOpacity": 0.3 },
  "animasyon": { "hizi": "normal", "hoverYukseklik": 4, "scrollReveal": false, "magneticCursor": false },
  "layout": { "header": "solid", "heroTipi": "slider", "kartStili": "solid-elevation", "sutunSayisi": 4 },
  "ikonSeti": "Material Icons",
  "glassmorphismAktif": false,
  "etiketler": []
}
```

---

## 18. 🔗 BAĞLANTILAR

- **AGENTS.md** — evrensel giriş
- [04_CSS_Tema_Stitch_Entegrasyonu.md](04_CSS_Tema_Stitch_Entegrasyonu.md) — eski tekil tema yöntemi (geriye uyum)
- [11_SaaS_MultiTenant_Mimarisi.md](11_SaaS_MultiTenant_Mimarisi.md) — firma-tema ataması
- [99_YASAKLAR_HIZLI_REFERANS.md](99_YASAKLAR_HIZLI_REFERANS.md) — tüm yasaklar

---

*Versiyon: 2.0 — Haziran 2026 | **TEMA = FARKLI BİR SİTE** felsefesi. 20+ temaya ölçeklenebilir. Tüm görsel kimlik (renk + tipografi + şekil + animasyon + layout + ikonografi + boşluk ritmi) temadan gelir. Tüm AI ajanlar (Claude, Cursor, Copilot, Windsurf, Gemini) için ZORUNLU kural dosyası.*

---

## 19. 🚀 SİSTEM GENİŞLEME MİMARİSİ (Gelecek-Proof)

Bu bölüm, **tema ekleme/düzenleme/değiştirme sırasında sistemin bozulmamasını** ve **ileride her firmanın farklı ihtiyaçlarına göre genişleyebilmesini** garanti eder.

### 19.1 🛡 TEMA GEÇİŞİ GÜVENLİĞİ (Sıfır Hata, Sıfır Flicker)

Tema değişimi **anlık, sorunsuz, state korumalı** olmalı.

**Akış (TemaSecici → TemaServisi):**

```csharp
public class TemaGecisServisi
{
    // 1. ÖN İŞLEM — mevcut state'i snapshot al
    public async Task<TemaGecisSonuc> GecisYapAsync(string yeniTemaSlug)
    {
        var eskiSlug = AktifSlugGetir();
        var snapshot = SayfaStateSnapshotAl(); // scroll, form, modal
        
        try
        {
            // 2. CSS dosyalarını lazy load
            await CssDosyalariniYukleAsync(yeniTemaSlug);
            
            // 3. CSS değişkenlerini set et
            await JS.InvokeVoidAsync("eval",
                $"document.documentElement.setAttribute('data-tema-id', '{yeniTemaSlug}')");
            
            // 4. Kısa transition için body'ye "gecis-yapiliyor" class'ı ekle
            await JS.InvokeVoidAsync("eval",
                "document.body.classList.add('tema-gecis-yapiliyor')");
            
            await Task.Delay(50); // CSS'in render olmasını bekle
            
            // 5. State'i geri yükle
            SayfaStateGeriYukle(snapshot);
            
            // 6. localStorage + DB kaydet
            await LocalStorageKaydet(yeniTemaSlug);
            await ApiKaydet(yeniTemaSlug);
            
            // 7. SignalR broadcast
            await TemaHub.BroadcastAsync(yeniTemaSlug);
            
            // 8. "gecis-yapiliyor" class'ını kaldır
            await JS.InvokeVoidAsync("eval",
                "document.body.classList.remove('tema-gecis-yapiliyor')");
            
            // 9. Analytics
            log.LogInformation("Tema gecis basarili: {Eski} → {Yeni}", eskiSlug, yeniTemaSlug);
            return TemaGecisSonuc.Basarili(yeniSlug);
        }
        catch (Exception ex)
        {
            // 10. ROLLBACK — eski temaya geri dön
            await JS.InvokeVoidAsync("eval",
                $"document.documentElement.setAttribute('data-tema-id', '{eskiSlug}')");
            log.LogError(ex, "Tema gecis basarisiz, rollback yapildi");
            return TemaGecisSonuc.Hata("Tema yüklenemedi, eski tema korundu");
        }
    }
}
```

**Flicker önleme (CSS):**

```css
/* Tüm temalar için geçiş yumuşatma */
:root {
    --tema-gecis-renkler: background-color 0.3s ease,
                          color 0.3s ease,
                          border-color 0.3s ease,
                          box-shadow 0.3s ease;
}

body, .navbar, .urun-kart, .btn-birincil, .hero-overlay, .footer {
    transition: var(--tema-gecis-renkler);
}

/* Geçiş sırasında sayfa karartılır (flicker önleme) */
body.tema-gecis-yapiliyor::before {
    content: "";
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.02);
    pointer-events: none;
    z-index: 99999;
}
```

**Lazy load stratejisi (JS):**

```javascript
// tema-yukle.js — global, TemaSecici inject eder
window.temaYukle = async function(slug) {
    const cacheKey = `tema-css-${slug}`;
    
    // 1. Cache kontrol
    if (sessionStorage.getItem(cacheKey) === 'yuklendi') return;
    
    // 2. CSS dosyalarını inject
    const dosyalar = ['tokens', 'bilesenler', 'animasyonlar'];
    for (const dosya of dosyalar) {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = `/css/temalar/${slug}/${dosya}.css`;
        link.dataset.temaParca = `${slug}-${dosya}`;
        document.head.appendChild(link);
    }
    
    // 3. Yüklenene kadar bekle
    await Promise.all(
        dosyalar.map(d => 
            new Promise(resolve => {
                const l = document.querySelector(`[data-tema-parca="${slug}-${d}"]`);
                l.onload = resolve;
                l.onerror = resolve;
            })
        )
    );
    
    sessionStorage.setItem(cacheKey, 'yuklendi');
};
```

**Özet:** Tema geçişi **asenkron, rollback'li, state korumalı, flicker önlemeli, lazy load cache'li**.

### 19.2 🔄 GERİYE UYUMLULUK (Backward Compatibility)

Yeni tema sistemi eklendiğinde **mevcut hiçbir sayfa, component veya API bozulmamalı**.

**Token alias'ları (zorunlu):**

```css
/* Her tokens.css dosyası ESKİ --vizit-* ve --aureli-* alias'larını da tanımlamalı */
:root[data-tema-id="gold"] {
    --vizit-primary: var(--tema-birincil);
    --vizit-accent: var(--tema-vurgu);
    --vizit-bg-base: var(--tema-arkaplan);
    --vizit-text: var(--tema-metin);
    --vizit-text-inverse: var(--tema-metin-ters);
    --vizit-font-serif: var(--tema-font-baslik);
    --vizit-font-sans: var(--tema-font-govde);
    --vizit-radius-md: var(--tema-kose-md);
    --vizit-transition: var(--tema-gecis-normal);
    --vizit-shadow-md: var(--tema-golge-md);
    --vizit-border: var(--tema-cizgi);
    --aureli-glass-bg: var(--tema-cam-bg);
    --aureli-glass-border: var(--tema-cam-cizgi);
    --aureli-glow: var(--tema-cam-glow);
    --aureli-blur: 20px;
}
```

**Attribute alias (zorunlu):**

```javascript
// JS — eski data-site-tema desteği
function setTema(slug) {
    document.documentElement.setAttribute('data-tema-id', slug);
    document.documentElement.setAttribute('data-site-tema', slug); // ESKİ YÖNTEM
}
```

**Class alias (zorunlu):**

```css
/* Tema bilesenler.css — eski class'lar da override edilmeli */
:root[data-tema-id="gold"] {
    .vizit-navbar { /* ... */ }
    .vizit-teklif-btn { /* ... */ }
    .gb-urun-kart { /* ... */ }
    .hero-slider-overlay { /* ... */ }
}
```

**3 katmanlı uyumluluk:**
1. **Token seviyesi** — eski `--vizit-*` hâlâ çalışır
2. **Attribute seviyesi** — eski `data-site-tema` hâlâ çalışır
3. **Class seviyesi** — eski `vizit-*`, `gb-urun-kart` hâlâ çalışır

### 19.3 🧩 PLUGIN / EKLETNİ MEKANİZMASI (Firma Bazlı Genişleme)

Her firma ileride **kendi eklentisini** isteyebilir (örn. ödeme altyapısı, chat widget, özel form, marka bileşenleri).

**Eklenti klasör yapısı:**

```
wwwroot/eklentiler/
├── _sistem/                              ← ORTAK eklenti altyapısı
│   ├── eklenti-yukleyici.js              (runtime injection)
│   └── eklenti-izinleri.json             (güvenlik whitelist)
│
├── varsayilan/                           ← TÜM firmalar için ortak
│   ├── canli-sohbet/
│   │   ├── manifest.json                 (eklenti meta)
│   │   ├── canli-sohbet.js
│   │   └── canli-sohbet.css
│   └── analitik/
│       └── ...
│
├── {firmaId}/                            ← FİRMA ÖZEL
│   ├── ozel-odeme/
│   │   ├── manifest.json
│   │   └── ...
│   └── marka-bilesenleri/
│       └── ...
```

**manifest.json (eklenti):**

```json
{
  "id": "canli-sohbet",
  "ad": "Canlı Sohbet",
  "versiyon": "1.0.0",
  "aktif": true,
  "firmaId": null,
  "temaBagimliligi": null,
  "yukleSirasi": 100,
  "izinler": ["dom-manipulasyon", "network", "storage"],
  "jsYolu": "canli-sohbet.js",
  "cssYolu": "canli-sohbet.css",
  "componentTipi": "blazor | vanilla-js | iframe",
  "configSchema": { "renk": "#FFD700", "pozisyon": "sag-alt" }
}
```

**Eklenti yükleme akışı (JS):**

```javascript
// Eklenti yükleyici
async function eklentileriYukle(firmaId) {
    const liste = await fetch(`/api/eklentiler?firmaId=${firmaId}`).then(r => r.json());
    
    // Aktif ve temaya uygun eklentileri filtrele
    const uygun = liste.filter(e => 
        e.aktif && 
        (!e.temaBagimliligi || e.temaBagimliligi.includes(aktifTemaSlug))
    );
    
    // Sıraya göre yükle
    for (const eklenti of uygun.sort((a, b) => a.yukleSirasi - b.yukleSirasi)) {
        await eklentiYukle(eklenti);
    }
}

async function eklentiYukle(eklenti) {
    if (eklenti.componentTipi === 'blazor') {
        // Blazor component render
        const root = document.querySelector(eklenti.containerSelector || 'body');
        Blazor.rootComponents.add(root, eklenti.componentName);
    } else if (eklenti.componentTipi === 'vanilla-js') {
        // Script inject
        const script = document.createElement('script');
        script.src = `/eklentiler/${eklenti.firmaId || 'varsayilan'}/${eklenti.id}/${eklenti.jsYolu}`;
        document.body.appendChild(script);
    }
}
```

**Tema-eklenti etkileşimi:** Eklenti `temaBagimliligi` alanı ile belirli temalarda aktif olur. Örn. glassmorphism eklentisi sadece `aurelian-onyx` ve `gold` temalarında çalışır.

### 19.4 🏢 FİRMA BAZLI ESNEKLİK (Multi-Tenant)

Her firma **kendi tema setini, eklentisini, layout'unu** görebilir.

**FirmaTemaAtama (DB):**

```csharp
public class FirmaTemaAtama
{
    public long Id { get; set; }
    public int FirmaId { get; set; }
    public long TemaSablonId { get; set; }
    public bool Aktif { get; set; }
    public string? OzelDegiskenlerJson { get; set; }  // tema override (örn. farklı vurgu rengi)
}
```

**Firma bazlı tema listeleme (API):**

```csharp
[HttpGet("aktif-temalar")]
public async Task<IActionResult> AktifTemalariListele([FromHeader] int firmaId)
{
    var temalar = await _db.FirmaTemaAtamalari
        .Where(f => f.FirmaId == firmaId && f.Aktif)
        .Join(_db.TemaSablonlari, f => f.TemaSablonId, t => t.Id, (f, t) => t)
        .ToListAsync();
    
    return Ok(new Cevap<object> { Veri = temalar });
}
```

**Firma bazlı override:**

```css
/* Firma #5 için özel vurgu rengi */
:root[data-firma-id="5"][data-tema-id="gold"] {
    --tema-vurgu: #FF1744; /* firma özel altın yerine kırmızı */
}
```

**Eklenti izolasyonu (güvenlik):** Her firma sadece kendi eklentilerini görebilir/yükleyebilir. Tenant sınırı RBAC ile korunur.

### 19.5 ➕ SUPER ADMİN GENİŞLETME AKIŞI

Super admin yeni eklenti/component/tema eklediğinde **mevcut sistem bozulmaz**:

**Eklenti ekleme:**
1. Super admin `/admin/super/eklentiler/yeni` formu doldurur
2. **manifest.json** + JS/CSS dosyaları `/wwwroot/eklentiler/{firmaId|varsayilan}/` altına yüklenir
3. DB'ye `eklentiler` tablosuna kayıt
4. Tema ile uyumluluk kontrolü (varsa `temaBagimliligi` set et)
5. **Mevcut sayfalar yeniden yüklenir** (gerekirse SignalR broadcast)

**Tema ekleme (zaten var):** 3 yöntem — Elle Form, Stitch Import, Manuel CSS. Mevcut 7+1 temaya ek olarak 8. tema (gold) eklendi → tüm mevcut temalar çakışmadan çalışmaya devam eder.

**Component ekleme (ileride):** Blazor component library → `/VizitLink3D.UI/Bilesenler/{Eklenti}/` → super admin firma bazlı aktifleştirir.

**Layout ekleme (ileride):** `/VizitLink3D.UI/Layout/{EklentiDuzen}.razor` → manifest'te layout adı → TemaSecici'den seçilebilir.

### 19.6 🔄 MİGRATİON STRATEJİSİ (Mevcut → Yeni Sistem)

**Adım 1 — Mevcut 2 temayı yeni klasöre taşı:**
- `gold-luxury-dark` → `/wwwroot/css/temalar/gold-luxury-dark/`
- `aurelian-onyx` → `/wwwroot/css/temalar/aurelian-onyx/`
- `gold` (yeni) → `/wwwroot/css/temalar/gold/` ← BU OTURUMDA EKLENDİ

**Adım 2 — Eski override'ları yeni sisteme bağla:**
- `degiskenler.css` içindeki `:root[data-site-tema="..."]` blokları → ilgili temanın `tokens.css`'ine taşındı
- `vizitlink3d.css` içindeki `:root[data-site-tema="aurelian-onyx"]` → `gold/bilesenler.css`'e taşındı

**Adım 3 — CokluTemaServisi güncelle:**
- Sözlük tabanlı → DB + dosya tabanlı
- Her tema `TemaKatalog` ile

**Adım 4 — TemaYonetimi güncelle:**
- 6+1 sabit tema → DB'den dinamik tema listesi
- "Yeni Tema Ekle" butonu (super admin)

**Adım 5 — Frontend TemaSecici:**
- Mevcut localStorage tabanlı → API + DB + SignalR
- Lazy load + cache + rollback

**Geriye uyumluluk:** `data-site-tema` hâlâ desteklenir → eski tema seçili kullanıcılar etkilenmez.

### 19.7 ✅ SÜREKLİ DOĞRULAMA (Smoke Test)

Tema değişikliği sonrası **otomatik test**:

```csharp
[Fact]
public async Task TemaGecisi_TumTemalar_Bozulmamali()
{
    var temalar = await _temaServisi.TumTemalariGetir();
    foreach (var tema in temalar.Where(t => t.Aktif))
    {
        // Aktif et
        await _temaServisi.TemaYukleVeUygulaAsync(tema.Slug);
        
        // Kritik sayfaları test et
        var anasayfa = await _client.GetAsync("/");
        var urunler = await _client.GetAsync("/urunler");
        var detay = await _client.GetAsync("/urun/lugano-plus-100");
        var iletisim = await _client.GetAsync("/iletisim");
        
        // Hepsi 200 dönmeli
        Assert.Equal(200, (int)anasayfa.StatusCode);
        Assert.Equal(200, (int)urunler.StatusCode);
        Assert.Equal(200, (int)detay.StatusCode);
        Assert.Equal(200, (int)iletisim.StatusCode);
        
        // Token'lar yüklü mü
        var html = await anasayfa.Content.ReadAsStringAsync();
        Assert.Contains($"data-tema-id=\"{tema.Slug}\"", html);
    }
}
```

### 19.8 🌐 DİL SİSTEMİ UYUMLULUĞU (i18n Koruması) — KRİTİK

> ⚠ **TEMA EKLERKEN/DEĞİŞTİRİRKEN DİL SİSTEMİ ASLA BOZULMAZ.**

Mevcut `DilServisi.T("anahtar", "Varsayılan")` yapısı korunmalı, hardcoded metin eklenmemeli.

**Tema ekleme sırasında dil kuralları:**

```csharp
// YANLIŞ — tema adı hardcoded
new TemaKatalog("gold", "Gold", "Altın lüks tema", ...);

// DOĞRU — tema adı çeviri anahtarı
new TemaKatalog(
    Ad: "tema.gold.ad",                              // çeviri anahtarı
    Baslik: dil.T("tema.gold.ad", "Gold"),           // localized
    Aciklama: dil.T("tema.gold.aciklama",
        "Altın yoğunluklu lüks tema — koyu zemin üzerinde parlayan altın, glassmorphism, Playfair Display. Gold Banyo markası için varsayılan."),
    ...
);
```

**manifest.json'da dil yapısı:**

```json
{
  "id": "gold",
  "kod": "GOLD",
  "adAnahtar": "tema.gold.ad",                    // çeviri anahtarı
  "adVarsayilan": "Gold",                         // fallback (tr)
  "adVarsayilanEn": "Gold",                       // İngilizce fallback
  "aciklamaAnahtar": "tema.gold.aciklama",
  "aciklamaVarsayilan": "Altın yoğunluklu lüks tema...",
  "aciklamaVarsayilanEn": "Gold-rich luxury theme...",
  "etiketlerAnahtar": "tema.gold.etiketler",
  ...
}
```

**DB şemasında çeviri desteği (TemaSablonu):**

```csharp
public class TemaSablonu
{
    // ... mevcut alanlar
    
    // Dil-bağımsız slug + kod
    public string Kod { get; set; } = "";
    public string Slug { get; set; } = "";
    
    // Çeviri anahtarları (DilServisi.T ile çekilir)
    public string AdAnahtar { get; set; } = "";           // tema.gold.ad
    public string AciklamaAnahtar { get; set; } = "";     // tema.gold.aciklama
    public string EtiketlerAnahtar { get; set; } = "";    // tema.gold.etiketler
    
    // Fallback değerler (DB'de offline çalışma için)
    public string AdVarsayilanTr { get; set; } = "";
    public string AdVarsayilanEn { get; set; } = "";
    public string AciklamaVarsayilanTr { get; set; } = "";
    public string AciklamaVarsayilanEn { get; set; } = "";
}
```

**Razor'da tema adı gösterme:**

```razor
@* YANLIŞ *@
<h1>@_seciliTema.Ad</h1>

@* DOĞRU *@
<h1>@dil.T(_seciliTema.AdAnahtar, _seciliTema.AdVarsayilanTr)</h1>
<p>@dil.T(_seciliTema.AciklamaAnahtar, _seciliTema.AciklamaVarsayilanTr)</p>
```

**Tema manifest çevirileri (DB'ye seed edilir):**

```sql
INSERT INTO ceviriler (anahtar, dil, deger) VALUES
  ('tema.gold.ad', 'tr', 'Gold'),
  ('tema.gold.ad', 'en', 'Gold'),
  ('tema.gold.aciklama', 'tr', 'Altın yoğunluklu lüks tema...'),
  ('tema.gold.aciklama', 'en', 'Gold-rich luxury theme...'),
  ('tema.aurelian-onyx.ad', 'tr', 'Aurelian Onyx'),
  ('tema.aurelian-onyx.ad', 'en', 'Aurelian Onyx'),
  ('tema.aurelian-onyx.aciklama', 'tr', 'Playfair + Space Grotesk...'),
  ('tema.aurelian-onyx.aciklama', 'en', 'Playfair + Space Grotesk...');
```

**Font seçiminde Türkçe karakter desteği (zorunlu):**

Bir tema eklerken seçilen fontlar **Türkçe karakterleri (Ş, İ, Ğ, Ü, Ö, Ç, ı, ş, ğ) desteklemeli**:

| Font | Türkçe desteği | Not |
|---|---|---|
| Noto Serif | ✓ | Evrensel, tüm diller |
| Playfair Display | ✓ | Latin extended |
| Manrope | ✓ | Latin extended |
| Space Grotesk | ✓ | Latin extended |
| Cormorant Garamond | ✓ | Latin extended |
| Inter | ✓ | Latin extended |
| Roboto | ✓ | Tüm Google Fonts |
| Bebas Neue | ✗ | Sadece Latin temel — Türkçe karakterlerde sorun |
| Anton | ✗ | Sadece Latin temel |
| Oswald | ⚠ | Kısıtlı |

**Font seçim kuralı:** Bir temada Türkçe karakter içeren metin gösterilecekse font **Latin Extended** veya **Unicode tam** olmalı. Bebas Neue/Anton gibi fontlar **sadece logo/başlık** gibi kısa metinlerde kullanılabilir.

**Tema CSS'inde dil-bağımsız kurallar:**

```css
/* YANLIŞ — hardcoded metin */
.tema-baslik { content: "Hoş geldiniz"; }

/* DOĞRU — sadece görsel, metin Razor'da */
.tema-baslik { font-family: var(--tema-font-baslik); }
```

**Tema değişimi sırasında dil-cache:**

```csharp
// Tema değişince dil cache'i temizleme — dil değişmez
public async Task TemaGecisYapAsync(string yeniTemaSlug)
{
    // Dil cache'i korunur (DilServisi cache'i tema-bağımsız)
    // Sadece tema CSS'i değişir
    await TemaCssYukleAsync(yeniTemaSlug);
    await SetAttributeAsync("data-tema-id", yeniTemaSlug);
}
```

**ÖZET — Tema + Dil entegrasyonu:**

| Katman | Tema bağımlı mı? | Dil bağımlı mı? |
|---|---|---|
| CSS renk/font/şekil | ✓ (temadan) | ✗ |
| Razor metin | ✗ | ✓ (DilServisi.T) |
| Tema adı/açıklaması | ✓ (slug) | ✓ (çeviri anahtarı) |
| Layout | ✓ (manifest'ten) | ✗ |
| Component | ✗ (ortak) | ✓ (DilServisi) |
| Eklenti | ⚠ (tema bağımlılığı olabilir) | ✓ (DilServisi) |

**Tema eklerken kontrol:**
- [ ] Tema adı `adAnahtar` + `adVarsayilanTr` + `adVarsayilanEn` olarak mı tutuldu?
- [ ] Açıklama `aciklamaAnahtar` + fallback'leri ile mı?
- [ ] Seçilen fontlar Türkçe karakterleri destekliyor mu?
- [ ] Hiçbir hardcoded metin CSS'te yok mu?
- [ ] Razor'da `dil.T(...)` ile mi gösteriliyor?
- [ ] Çeviriler DB'ye seed edildi mi?

---

## 20. 📌 SONUÇ VE FELSEFE

**TEMA ŞABLON SİSTEMİ = GELECEK-PROOF MİMARİ:**

1. **Bugün:** 3 tema (gold-luxury-dark + aurelian-onyx + gold) çalışıyor
2. **Yarın:** Super admin Stitch'ten 4. temayı import eder → tüm sistem genişler, mevcut temalar bozulmaz
3. **Gelecek:** Her firma kendi temasını, eklentisini, layout'unu ister → plugin sistemi ile izole şekilde çalışır
4. **TEMA = FARKLI SİTE:** Renk + tipografi + şekil + animasyon + layout + ikonografi + boşluk ritmi hepsi temadan gelir
5. **Güvenli:** Lazy load, rollback, state koruma, flicker önleme
6. **Geriye uyumlu:** Token alias, attribute alias, class alias — eski sayfalar bozulmaz

> **Yeni tema eklemek = sadece dosya + DB satırı. Kod değişikliği SIFIR.**

**Tüm AI'lar (Claude, Cursor, Copilot, Windsurf) bu kuralları okur ve uygular.**

---

*Versiyon: 2.1 — Haziran 2026 | + Sistem Genişleme Mimarisi (Gelecek-Proof). Tüm AI ajanlar için ZORUNLU.*
