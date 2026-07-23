# 📜 VIZITLINK3D — KODLAMA ANAYASASI

> **Proje:** VIZITLINK3D (Kapı/Mobilya Kurumsal Site)
> **Versiyon:** 3.0 — Saf Kodlama Kuralları (SaaS artıkları temizlendi)
> **Yedek:** [Yedekler/anayasa_yedek_20260514_v2/](Yedekler/anayasa_yedek_20260514_v2/)
> **Tarih:** 2026-05-14
>
> Bu dosya **nasıl kod yazılacağını** tanımlar — ne yapılacağını değil.
> Özellik/fonksiyonalite kararları: [DUZELT.md](DUZELT.md), [MIMARI_VIZYON.md](MIMARI_VIZYON.md), [PLAN_MEDYA_VE_AI.md](PLAN_MEDYA_VE_AI.md).

---

## 📋 İÇİNDEKİLER

| # | Bölüm |
|---|---|
| 1 | Teknoloji Yığını ve Portlar |
| 2 | Dil ve Türkçe İsimlendirme |
| 3 | Encoding (UTF-8) |
| 4 | C# Kodlama Kuralları |
| 5 | Model (Entity) Kuralları |
| 6 | Veritabanı Kuralları |
| 7 | CSS ve Tema Sistemi |
| 8 | Blazor / Razor / MudBlazor Kuralları |
| 9 | Güvenlik — Sıfır Tolerans |
| 10 | Hata Yönetimi |
| 11 | Loglama |
| 12 | Açıklama (Yorum) Standardı |
| 13 | Wrapper Kuralı (Harici Kütüphane) |
| 14 | DRY (Kod Tekrarı Yasağı) |
| 15 | Klasör Yapısı (Vertical Slice) |
| 16 | Test ve Yedek |
| 17 | Genel Yasak Listesi |

---

## 1. 🏗 TEKNOLOJİ YIĞINI VE PORTLAR

### 1.1 Sabit Portlar (DEĞİŞTİRİLEMEZ)
```
API      → 5015
Frontend → 5013
```

### 1.2 Stack
| Katman | Teknoloji |
|---|---|
| Backend | ASP.NET Core 10 |
| Frontend | Blazor WebAssembly |
| Gerçek Zamanlı | SignalR + MessagePack |
| Veritabanı | SQLite (geliştirme) / PostgreSQL (üretim) — EF Core Code-First |
| Önbellek | FusionCache (L1 RAM + L2 Redis) |
| UI Kütüphanesi | **MudBlazor (TEK İZİNLİ — başka UI lib YASAK)** |
| Loglama | Serilog + Seq |
| PDF | QuestPDF |
| Excel | ClosedXML |
| E-posta | MailKit |
| Görsel | SixLabors.ImageSharp + ImageSharp.Web |
| Doğrulama | FluentValidation |
| Mapping | Mapster (AutoMapper yasak — yavaş) |
| CQRS | MediatR |
| Resilience | Polly |
| Container | (kaldırıldı) |

### 1.3 Zorunlu NuGet Paketleri
```
ZiggyCreatures.FusionCache
FluentValidation.AspNetCore
Serilog.AspNetCore + Sinks.Console + Sinks.File
BCrypt.Net-Next
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNetCore.SignalR.Protocols.MessagePack
QuestPDF
ClosedXML
MailKit
SixLabors.ImageSharp + SixLabors.ImageSharp.Web
Ganss.Xss.HtmlSanitizer
Microsoft.AspNetCore.RateLimiting
Mapster
MediatR
Polly
```

---

## 2. 🗣 DİL VE TÜRKÇE İSİMLENDİRME

### 2.1 %100 Türkçe Zorunlu
Tüm değişken / fonksiyon / sınıf / dosya / klasör / DB tablo / DB sütun / commit mesajı **Türkçe**.

| ❌ Yasak | ✅ Doğru |
|---|---|
| `Company` | `Firma` |
| `GetUsers()` | `KullanicilariGetir()` |
| `IsActive` | `AktifMi` |
| `UserService.cs` | `KullaniciServisi.cs` |
| `tenant` | `kiraci` |
| `Save()` | `KaydetAsync()` |

### 2.2 İsimlendirme Tablosu
| Tür | Format | Örnek |
|---|---|---|
| Sınıf | PascalCase | `KullaniciServisi` |
| Metot | Fiil + PascalCase + Async | `KullaniciEkleAsync()` |
| Değişken (özel) | _camelCase | `_kullaniciAdi` |
| Değişken (yerel) | camelCase | `kullaniciAdi` |
| Arayüz | I + PascalCase | `IKullaniciServisi` |
| Enum | PascalCase | `MedyaTipi.Resim` |
| Constant | BUYUK_HARF | `MAKSIMUM_DENEME_SAYISI` |
| Klasör | PascalCase Türkçe | `Moduller/Kapilar/` |

### 2.3 Framework İstisnaları
Şunlar İngilizce kalır:
- C# keywords: `public`, `private`, `class`, `async`, `await`, `override`
- Blazor/Razor: `@page`, `@inject`, `@code`, `OnInitializedAsync`
- HTML/CSS/SQL: `div`, `SELECT`, `FROM`
- MudBlazor etiketleri: `<MudButton>`, `<MudCard>` (etiket adı; **olaylar/metinler Türkçe**)
- Platform dosyaları: `Program.cs`, `MainActivity.cs`

```razor
<!-- ❌ YANLIŞ -->
<MudButton OnClick="SaveData" Color="Color.Primary">Save</MudButton>

<!-- ✅ DOĞRU -->
<MudButton OnClick="VeriyiKaydetAsync" Color="Color.Primary">Kaydet</MudButton>
```

### 2.4 Çoklu Dil (Hardcoded Metin Yasak)
Hiçbir `.razor` dosyasında ekranda görünen sabit metin yazılamaz:
```razor
<!-- ❌ YASAK -->
<button>Kaydet</button>

<!-- ✅ ZORUNLU -->
<button>@DilServisi.T("ortak.kaydet", "Kaydet")</button>
```
Her `.razor` dosyasının başında: `@inject DilServisi DilServisi`
Çeviriler **DB + FusionCache** üzerinden gelir — `wwwroot/i18n/*.json` **YASAK**.

---

## 3. 🔡 ENCODING (UTF-8)

- Tüm `.cs`, `.razor`, `.css`, `.md`, `.json` dosyaları **UTF-8** kaydedilir
- CSS için **UTF-8 BOM** tercih edilir
- Terminal komutlarında: `-Encoding UTF8` zorunlu (PowerShell)
- Regex/sed işlemlerinde Türkçe karakter ve emoji bozulmamalı
- `appsettings.json` ve `launchSettings.json` — .NET kuralı, istisna

---

## 4. 🔵 C# KODLAMA KURALLARI

### 4.1 Dosya Boyutu Limiti
- **Maksimum:** 1500 satır → aşılırsa partial class veya yeni servise böl
- **İdeal:** 500-800 satır
- **Uyarı eşiği:** 1200 satır

### 4.2 Sınıf Sorumluluk Prensibi
Bir sınıf **tek işten** sorumludur. Karışık iş = bölünür.

### 4.3 Async/Await Zorunlu
```csharp
// ❌ YANLIŞ: Senkron DB çağrısı
var kullanici = _db.Kullanicilar.First(k => k.Id == id);

// ✅ DOĞRU
var kullanici = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.Id == id);
```
- I/O operasyonları (DB, HTTP, dosya) **her zaman** async
- Metot adı `Async` ile biter: `KaydetAsync`, `GetirAsync`
- `.Result` / `.Wait()` çağrısı **YASAK** (deadlock riski)

### 4.4 Dependency Injection (DI)
- `new` ile servis oluşturma YASAK iş mantığında
- Constructor injection tercih edilir
- Statik servis YASAK

```csharp
// ❌ YANLIŞ
var servis = new KullaniciServisi();

// ✅ DOĞRU
public KapiKontrolcu(IKullaniciServisi servis) { _servis = servis; }
```

### 4.5 Try-Catch Yasağı (Kontrolcüde)
```csharp
// ❌ YASAK: Kontrolcüde try-catch
public async Task<IActionResult> Kaydet(Model istek)
{
    try { /* ... */ }
    catch (Exception ex) { return BadRequest(ex.Message); }
}

// ✅ DOĞRU: HataYonetimiMiddleware yakalar
public async Task<Cevap<Model>> Kaydet(Model istek)
{
    var sonuc = await _servis.KaydetAsync(istek);
    return Cevap<Model>.Basarili(sonuc);
}
```
Try-catch sadece **özel kurtarma mantığı** varsa kullanılır (örn. dış API'ye yeniden deneme).

### 4.6 Null Kontrol
- `ArgumentNullException.ThrowIfNull(param)` tercih edilir
- Nullable reference types açık (`<Nullable>enable</Nullable>`)
- `?.` ve `??` operatörleri yaygın kullanılır

### 4.7 Magic Number / String Yasak
```csharp
// ❌
if (deneme > 5) { ... }
return "Kayit bulunamadi";

// ✅
private const int MAKSIMUM_DENEME = 5;
if (deneme > MAKSIMUM_DENEME) { ... }
```

### 4.8 Standart API Yanıt Zarfı
Tüm API endpoint'leri `Cevap<T>` döner:
```csharp
public class Cevap<T>
{
    public bool BasariliMi { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public List<string> Hatalar { get; set; } = new();
    public T? Veri { get; set; }

    public static Cevap<T> Basarili(T veri, string mesaj = "İşlem başarılı.")
        => new() { BasariliMi = true, Veri = veri, Mesaj = mesaj };

    public static Cevap<T> Hata(string mesaj, List<string>? hatalar = null)
        => new() { BasariliMi = false, Mesaj = mesaj, Hatalar = hatalar ?? new() };
}
```

### 4.9 Partial Class — Razor için Zorunlu
Her `.razor` dosyası için ayrı `.razor.cs` **Partial Class** dosyası. `.razor` içinde `@code { }` bloğu **YASAK**.

```
KapiDetay.razor       → markup
KapiDetay.razor.cs    → kod (partial class)
```

---

## 5. 📦 MODEL (ENTITY) KURALLARI

### 5.1 Audit Alanları (Tüm Modellerde)
Her entity'de minimum:
```csharp
public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
public DateTime? GuncellenmeTarihi { get; set; }
public int? OlusturanKullaniciId { get; set; }
public int? GuncelleyenKullaniciId { get; set; }
```

### 5.2 Soft Delete
Fiziksel silme yasak — yerine:
```csharp
public bool SilindiMi { get; set; }
public DateTime? SilinmeTarihi { get; set; }
```

### 5.3 [JsonIgnore] Zorunlu Alanlar
Şu alanlar **mutlaka** `[JsonIgnore]` ile işaretlenir:
```csharp
[System.Text.Json.Serialization.JsonIgnore] public string SifreHash { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public string? PinHash { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public string? DesenHash { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public string? WebAuthnPublicKey { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public string? SifreSifirlamaToken { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public DateTime? TokenGecerlilikTarihi { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public string? TotpAnahtari { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public string? EmailDogrulamaToken { get; set; }
[System.Text.Json.Serialization.JsonIgnore] public string? ApiKeyEncrypted { get; set; }
```
**Genel kural:** Şifre, hash, token, anahtar içeren herhangi bir alan → `[JsonIgnore]`.

### 5.4 DateTime Standardı
- Her zaman `DateTime.UtcNow` (lokal zaman değil)
- DB'de UTC saklanır, UI'da lokale çevrilir

### 5.5 String Default
```csharp
// ❌
public string Ad { get; set; }   // null warning

// ✅
public string Ad { get; set; } = string.Empty;
public string? Aciklama { get; set; }   // gerçekten opsiyonel ise
```

### 5.6 Navigation Property
İlişkili nesneler `[JsonIgnore]` ile (sonsuz döngüyü önler):
```csharp
public int KategoriId { get; set; }
[JsonIgnore]
public Kategori? Kategori { get; set; }
```

---

## 6. 🗃 VERİTABANI KURALLARI

### 6.1 EF Core Code-First Migration ZORUNLU
```
❌ PgAdmin/DBeaver ile elle tablo açmak
❌ Doğrudan SQL ile şema değiştirmek
❌ CREATE TABLE IF NOT EXISTS
✅ dotnet ef migrations add MigrationAdiTurkce
✅ dotnet ef database update
```

### 6.2 Tablo/Sütun Adı — Türkçe Karakter YASAK
```
❌ YANLIŞ: SİstemBildirimleri, KullanıcıRolü, Şube, Çalışan
✅ DOĞRU:  SistemBildirimleri, KullaniciRolu, Sube, Calisan
```
Dönüşüm: Ş→S, İ→I, Ğ→G, Ü→U, Ö→O, Ç→C, ş→s, ı→i, ğ→g, ü→u, ö→o, ç→c

### 6.3 Index Zorunlu Alanlar
- Slug, Eposta → unique index
- ForeignKey'ler → normal index
- Composite (örn. `Yerellestirme.{EntiteId, Dil}`) → unique
- Sık sorgulanan filtre alanları → index

### 6.4 Migration Adlandırma
Türkçe açıklayıcı:
```
✅ KullaniciyeLisansAlaniEklendi
✅ KapiKategorisiSlugZorunlulukEklendi
✅ MedyaHavuzuEklendi
❌ Migration1, Update, NewChanges
```

### 6.5 Global Query Filter
- Soft delete filtresi otomatik (`SilindiMi == false`)
- FirmaId filtresi (multi-tenant altyapısı için — şu an opsiyonel)

---

## 7. 🎨 CSS VE TEMA SİSTEMİ

### 7.1 Tema — Industrial Luxury (Bronz/Siyah)
```css
:root {
  /* Ana renkler */
  --renk-ana: #0a0a0a;          /* Kömür siyah */
  --renk-ikinci: #c19b76;       /* Bronz */
  --renk-vurgu: #d4a574;        /* Bal bronz */
  --renk-arkaplan: #ffffff;
  --renk-arkaplan-koyu: #1a1a1a;
  --renk-metin: #2c2c2c;
  --renk-metin-acik: #6c6c6c;

  /* Tipografi */
  --font-baslik: 'Playfair Display', serif;
  --font-metin: 'Inter', sans-serif;
  --font-vurgu: 'Cormorant Garamond', serif;

  /* Boşluk */
  --bosluk-xs: 0.5rem;
  --bosluk-sm: 1rem;
  --bosluk-md: 1.5rem;
  --bosluk-lg: 2.5rem;
  --bosluk-xl: 4rem;

  /* Gölge */
  --golge-yumusak: 0 4px 20px rgba(0,0,0,0.08);
  --golge-orta:    0 10px 40px rgba(0,0,0,0.12);
  --golge-luks:    0 20px 60px rgba(193,155,118,0.15);

  /* Geçişler */
  --gecis-hizli: 0.2s ease;
  --gecis-orta:  0.4s cubic-bezier(0.4,0,0.2,1);
  --gecis-yavas: 0.8s cubic-bezier(0.4,0,0.2,1);

  /* Breakpoint */
  --ekran-mobil: 480px;
  --ekran-tablet: 768px;
  --ekran-masaustu: 1280px;
}
```

### 7.2 Klasör Hiyerarşisi
```
VIZITLINK3D.UI/wwwroot/css/sistem/
├── tokens.css              (TEK giriş noktası — sadece @import)
├── temeller/
│   ├── degiskenler.css     (yukarıdaki :root)
│   ├── tipografi.css
│   └── reset.css
├── bilesenler/
│   ├── butonlar.css
│   ├── kartlar.css
│   ├── tablolar.css
│   ├── modal.css
│   └── efektler.css
└── moduller/
    ├── anasayfa.css
    ├── admin.css
    └── ...
```

### 7.3 Hardcoded Renk/Font YASAK
```css
/* ❌ YASAK */
button { background: #c19b76; color: red; font-family: Arial; }

/* ✅ ZORUNLU */
button {
    background: var(--renk-ikinci);
    color: var(--renk-metin);
    font-family: var(--font-metin);
}
```

### 7.4 .razor İçinde `<style>` YASAK
Geçici animasyonlar (`@keyframes`), modal CSS — hepsi `bilesenler/` veya `moduller/` altına gider.

### 7.5 MudThemeProvider tokens.css ile Beslenir
`VIZITLINK3D.UI/Bilesenler/TemaSaglayici.razor` MudTheme nesnesini CSS değişkenlerinden üretir.

### 7.6 Encoding ve Yorum
- CSS dosyaları **UTF-8 BOM** olarak kaydedilir
- Her CSS dosyasının başında Türkçe açıklama bloğu
- Stil tekrarı (DRY) yasak — ortak yapı `bilesenler/` altına

---

## 8. 🔷 BLAZOR / RAZOR / MUDBLAZOR KURALLARI

### 8.1 MudBlazor Tek İzinli UI Kütüphanesi
- Dashboard kart, form, buton, modal, snackbar → MudBlazor
- Karmaşık HTML tablo/div yapıları → MudBlazor bileşeni kullan
- Başka UI lib (Bootstrap, Ant, vb.) **YASAK**

### 8.2 Partial Class — `@code` Yasak
```
KapiDetay.razor       → sadece markup
KapiDetay.razor.cs    → kod
```
`.razor` içinde `@code { }` bloğu **KESİNLİKLE YASAK**.

### 8.3 Her .razor Dosyasında DilServisi
```razor
@inject DilServisi DilServisi
@inject NavigationManager NavigasyonYoneticisi
```

### 8.4 `<style>` Yasağı
Razor dosyasının içine `<style>` etiketi ile CSS yazmak **KESİNLİKLE YASAK** (bkz §7.4).

### 8.5 Tekrar Eden UI → Paylaşılan Bileşen
```razor
@* ❌ Aynı kart 4 sayfada kopyalanmış *@
<div class="modern-kart">...</div>

@* ✅ Paylaşılan bileşen *@
<OrtakKart Baslik="@baslik" Aciklama="@aciklama" />
```

### 8.6 Responsive Zorunlu
- **Mobil:** < 768px (bottom nav, hamburger)
- **Tablet:** 768-1200px (mini sidebar)
- **Masaüstü:** > 1200px (tam sidebar)

---

## 9. 🛡 GÜVENLİK — SIFIR TOLERANS

### 9.1 Yasak Liste (Alarm Seviyesi)
```
❌ Arka kapı (backdoor) şifresi
❌ BCrypt hash / şifre / token log'a yazmak
❌ Hardcoded şifre API yanıtında
❌ [AllowAnonymous] DB sıfırlama endpoint'lerine
❌ BCrypt frontend'de (şifre client'ta hash'lenmez)
❌ JWT anahtarı appsettings.json plaintext
❌ CORS AllowAnyOrigin() üretimde
❌ Şifre / hash / token alanı [JsonIgnore]'suz API'den dönmek
```

### 9.2 Zorunlu Önlemler
- **Kimlik:** JWT Bearer + opsiyonel 2FA (TOTP)
- **Şifre:** BCrypt.Net-Next ile hash
- **CORS:** Üretimde sadece `VIZITLINK3D.com.tr` + `www.VIZITLINK3D.com.tr`
- **HTTPS:** Üretimde `RequireHttpsMetadata = true` + HSTS
- **Rate Limiting:** `Microsoft.AspNetCore.RateLimiting` (IP bazlı)
- **Input Validation:** FluentValidation (sunucu tarafı zorunlu)
- **XSS:** `Ganss.Xss.HtmlSanitizer` ile temizle
- **SignalR:** Üretimde `EnableDetailedErrors = false`
- **Headers:** HSTS + X-Frame-Options: DENY + X-Content-Type-Options: nosniff + CSP

### 9.3 Gizli Bilgi Yönetimi
- **Geliştirme:** .NET user-secrets
- **Üretim:** Coolify Secrets
- **Git:** `.env` ASLA commit edilmez

### 9.4 API Key Saklama
Şifrelenmiş (ASP.NET DataProtection ile):
```csharp
public string ApiKeyEncrypted { get; set; }   // DB'de şifreli
// Okunurken: _protector.Unprotect(ApiKeyEncrypted)
```

---

## 10. ⚠ HATA YÖNETİMİ

### 10.1 Merkezi Middleware
`HataYonetimiMiddleware` tüm yakalanmamış hataları yakalar:
- Production'da detay verme, dev'de stack trace
- CorrelationId her isteğe Guid
- Audit log'a yaz
- `Cevap<T>.Hata()` formatında JSON döner

### 10.2 Kontrolcüde Try-Catch YOK
Bkz §4.5.

### 10.3 Tüm Endpoint'ler `Cevap<T>` Döner
Bkz §4.8.

---

## 11. 📊 LOGLAMA

### 11.1 Yapısal Log Zorunlu
```csharp
// ❌ YANLIŞ: Düz metin
_logger.LogInformation("Kullanıcı giriş yaptı");

// ✅ DOĞRU: Yapısal
_logger.LogInformation(
    "Kullanıcı girişi başarılı. {KullaniciId} {Eposta} {CorrelationId}",
    kullanici.Id, kullanici.Eposta, correlationId);
```

### 11.2 ASLA Log'a Yazılamaz
```
❌ SifreHash, PinHash, DesenHash, WebAuthnPublicKey
❌ JWT token değeri
❌ API key
❌ Kredi kartı / IBAN numarası
❌ TC kimlik no (PII)
❌ AI prompt (sadece kısaltılmış 500 karakter audit için)
```

### 11.3 Zorunlu Alanlar
Her log satırında: `CorrelationId`, `KullaniciId` (varsa), `Eylem`, `Sonuc`.

### 11.4 Loglama Stack
Serilog + Seq. Konsol + günlük rotasyonlu dosya. 30 gün saklama.

---

## 12. 📝 AÇIKLAMA (YORUM) STANDARDI

### 12.1 Varsayılan: Yorum YAZMA
Sadece **WHY** belirsizse yaz. Kod ne yapıyor → iyi isimlendirme anlatır.

### 12.2 Yasak Yorum Kalıpları
```csharp
// ❌ "TODO: sonra düzelt"             (ya yap, ya issue aç)
// ❌ "// kaldırıldı: eski X kodu"    (git history zaten tutuyor)
// ❌ "// Ali için eklendi"            (kişi/PR referansı çürür)
// ❌ paragraflarca docstring          (bir satır yeter)
// ❌ "// kullanıcıyı getirir"         (KullaniciGetir() zaten anlatıyor)
```

### 12.3 Public API'lerde XML Summary
```csharp
/// <summary>
/// Verilen Id'ye sahip kapı modelini getirir. Kategori ve resimler eager
/// load edilir. Soft-delete filtresi otomatik uygulanır.
/// </summary>
public async Task<KapiModeli?> KapiGetirAsync(int id) { ... }
```

### 12.4 Türkçe Yorum
Yorum bloklarında Türkçe karakter serbest (dosya UTF-8). Ama identifier'da ASCII-Türkçe (S/I/G/U/O/C).

---

## 13. 📦 WRAPPER KURALI (HARİCİ KÜTÜPHANE)

### 13.1 Doğrudan Çağrı YASAK
İş mantığı içinde harici kütüphane (Three.js, GSAP, QuestPDF, ImageSharp, MailKit) doğrudan çağrılamaz.

```csharp
// ❌ YASAK: Doğrudan QuestPDF
Document.Create(c => ...).GeneratePdf();

// ❌ YASAK: Doğrudan JS
scene.createDefaultCamera();

// ✅ DOĞRU: Türkçe Wrapper
var pdf = await _pdfUretici.FaturaOlusturAsync(faturaId);
await _ucBoyutMotoru.SahneBaslatAsync(modelUrl);
```

### 13.2 Standart Wrapper İsimleri
| Kütüphane | Wrapper |
|---|---|
| FusionCache | `OnbellekYonetici` |
| QuestPDF | `PdfUretici` |
| ClosedXML | `ExcelUretici` |
| MailKit | `EpostaServisi` |
| ImageSharp | `ResimIslemcisi` |
| HtmlSanitizer | `IcerikTemizleyici` |
| BCrypt | `SifreServisi` |
| JWT | `JwtServisi` |
| Three.js (JS) | `UcBoyutMotoru` |
| GSAP (JS) | `AnimasyonMotoru` |
| Cropper.js | `ResimDuzenleyici` |
| Storage (MinIO/S3/Yerel) | `DepolamaAdaptoru` |

---

## 14. 🔁 DRY (KOD TEKRARI YASAĞI)

### 14.1 Sıfır Tolerans
Aynı fonksiyon / mantık / blok ASLA iki yerde yazılamaz.

- **Backend tekrar →** ortak servis veya extension method
- **Frontend tekrar →** paylaşılan Razor bileşeni
- **Stil tekrar →** `tokens.css` değişkeni veya `bilesenler/` sınıfı
- **Validation tekrar →** FluentValidation Dogrulayici sınıfı

### 14.2 Örnek
```csharp
// ❌ YASAK: 3 controller'da aynı doğrulama
if (string.IsNullOrEmpty(model.Baslik)) return BadRequest(...);
if (model.Baslik.Length > 200) return BadRequest(...);

// ✅ DOĞRU: FluentValidation
public class KapiOlusturDogrulayici : AbstractValidator<KapiOlusturDto>
{
    public KapiOlusturDogrulayici()
    {
        RuleFor(x => x.Baslik).NotEmpty().MaximumLength(200);
    }
}
```

---

## 15. 🗂 KLASÖR YAPISI (VERTICAL SLICE)

### 15.1 Backend
```
VIZITLINK3D.Api/Moduller/
├── Kapilar/
│   ├── Komutlar/         (KapiOlusturKomutu.cs + İsleyici)
│   ├── Sorgular/
│   ├── Dtolar/
│   ├── Dogrulayicilar/
│   ├── Servisler/
│   └── Kontrolcu/        (KapiKontrolcu.cs — 3 satırlık)
├── Mobilyalar/
├── Projeler/
├── Pazarlama/            (Slayt, Referans, Yorum, HizmetAdimi)
├── Kurumsal/             (Firma, Sube, Ekip, Sertifika)
├── Iletisim/             (Mesaj, Bulten, Sohbet)
├── Kimlik/
├── Medya/                (Havuz)
├── AI/                   (Sağlayıcı yönetimi)
└── Sistem/               (Audit, Ceviri, Tema, Ayar, Yedek)
```

### 15.2 Frontend
```
VIZITLINK3D.UI/
├── Pages/
│   ├── Public/           (Anasayfa, KapiDetay, vb.)
│   └── Admin/            (yönetim sayfaları)
├── Bilesenler/
│   ├── Ortak/            (paylaşılan)
│   ├── Anasayfa/
│   ├── Admin/
│   ├── Medya/            (Havuz, Secici, Editor)
│   └── AI/               (YazButonu, StreamKutusu)
├── Servisler/            (DilServisi, ApiIstemcisi, vb.)
└── wwwroot/
```

### 15.3 Düz Klasör YASAK
`Kontrolculer/` veya `Modeller/` ana dizinine yığın halinde dosya bırakmak yasak. Her dosya modülünün altında.

---

## 16. 🧪 TEST VE YEDEK

### 16.1 Minimum 5 Test Her Özellik İçin
1. Başarılı senaryo
2. Boş/geçersiz veri → 400
3. Yetki kontrolü → 401/403
4. Edge case (negatif ID, max boyut)
5. Geri düşüş (diğer özellik bozulmadı)

### 16.2 Testcontainers
Sahte (in-memory) DB **kabul edilmez** — gerçek PostgreSQL container'ı (`Testcontainers.PostgreSql`).

### 16.3 DB Yedek Zorunlu
- Her büyük değişiklik öncesi: `Yedekler/db/VIZITLINK3D_YYYYMMDD_aciklama.db`
- Her migration öncesi/sonrası
- Anayasa değişikliği öncesi: `Yedekler/anayasa_yedek_YYYYMMDD/`

### 16.4 Commit Öncesi Kontrol
```
[ ] dotnet build → hata yok
[ ] dotnet test → tümü yeşil
[ ] .env commit'te değil
[ ] appsettings.json'da plaintext key yok
[ ] Hardcoded şifre yok
[ ] Yedek alındı
```

### 16.5 Commit Mesajı — Türkçe
```
✅ "Lisans domain kilitleme middleware'i eklendi"
✅ "KapiKontrolcu'da yetki filtresi eklendi"
❌ "fix", "update", "wip"
```

---

## 17. 🚫 GENEL YASAK LİSTESİ (Hızlı Referans)

| # | Yasak | Sebep / Bölüm |
|---|---|---|
| 1 | Python (`*.py`) veya dış terminal botları | %100 C# / .NET 10 (§1) |
| 2 | MudBlazor dışında UI kütüphanesi | §8.1 |
| 3 | `.razor` içine `<style>` etiketi | §7.4 / §8.4 |
| 4 | `.razor` içinde `@code { }` bloğu | §8.2 (partial class) |
| 5 | Hardcoded Türkçe metin Razor'da | §2.4 (DilServisi.T) |
| 6 | Try-catch kontrolcüde | §4.5 / §10.2 |
| 7 | DB tablo/sütun adında Türkçe karakter | §6.2 |
| 8 | EF Migration dışı DB değişikliği | §6.1 |
| 9 | Hardcoded renk/font CSS'de | §7.3 |
| 10 | Doğrudan harici kütüphane çağrısı | §13 (Wrapper) |
| 11 | `wwwroot/i18n/*.json` çeviri için | §2.4 (DB + cache) |
| 12 | Loglara şifre/token/key yazmak | §11.2 |
| 13 | Backdoor şifresi | §9.1 |
| 14 | `AllowAnyOrigin()` üretimde | §9.1 |
| 15 | `new` ile servis (iş mantığında) | §4.4 (DI) |
| 16 | `.Result` / `.Wait()` çağrısı | §4.3 (deadlock) |
| 17 | AutoMapper kullanmak | §1.2 (Mapster) |
| 18 | Magic number/string | §4.7 |
| 19 | Fiziksel DELETE (entity için) | §5.2 (soft delete) |
| 20 | DB yedeği almadan migration | §16.3 |
| 21 | Kod tekrarı (DRY ihlali) | §14 |
| 22 | İngilizce isimlendirme (framework hariç) | §2.1 |

---

## 📌 SON SÖZ

Bu anayasa **kodun nasıl yazılacağını** tanımlar.
- **Ne yapılacak** → [DUZELT.md](DUZELT.md), [PLAN_MEDYA_VE_AI.md](PLAN_MEDYA_VE_AI.md)
- **Hangi vizyonla yapılacak** → [MIMARI_VIZYON.md](MIMARI_VIZYON.md)
- **Tamamlananlar** → [GOREV_1_YAPILDI.md](GOREV_1_YAPILDI.md)
- **Sıradakiler** → [GOREV_2_YAPILACAK.md](GOREV_2_YAPILACAK.md)

### Yedek Politikası
Bu dosya her değişiklik öncesi `Yedekler/anayasa_yedek_YYYYMMDD/` altına yedeklenir.
Bir önceki sürüm: [Yedekler/anayasa_yedek_20260514_v2/KURALLAR_yedek.md](Yedekler/anayasa_yedek_20260514_v2/KURALLAR_yedek.md)

---

*Versiyon: 3.0 — Saf Kodlama Kuralları | Tarih: 2026-05-14*
*Vizitlink'in SaaS/pazaryeri/gamification/omnichannel/NFC bölümleri VIZITLINK3D'a uygulanmaz — bu anayasa sadece kodlama disiplinini tanımlar.*
