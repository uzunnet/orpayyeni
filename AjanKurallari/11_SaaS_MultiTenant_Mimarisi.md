---
name: "saas-multi-tenant"
description: "SaaS multi-tenant mimarisinin kuralları, domain tabanlı tenant tespiti, firma başına ayrı veritabanı yalıtımı, bağımsız SuperAdmin, yetki ayrımı"
metadata:
  type: "project"
  updated: "2026-08-06"
---

# 11. SaaS Multi-Tenant Mimarisi

> VizitLink3D, tek bir platformda birden fazla müşteri (firma/tenant) hizmet eden **Software as a Service (SaaS)** uygulamasıdır. Her firma `firmalar/{slug}/` klasöründe kendi **bağımsız SQLite veritabanına** (`{slug}.db`), kendi **medya klasörüne** (`medya/`) ve kendi **statik dil dosyalarına** (`i18n/`) sahiptir. Platform yönetimi **tamamen bağımsız** `VizitLink3D.SuperAdmin` projesi tarafından yürütülür.

---

## 🏗️ MİMARİ GENEL BAKIŞ

### Katmanlar

```
┌──────────────────────────────────────────────────┐
│              İnternet (Kullanıcı)                │
└─────────────────────────┬────────────────────────┘
                          │  Host / Domain
┌─────────────────────────▼────────────────────────┐
│  FirmaCozumlemeMiddleware (VizitLink3D.Api)      │
│  └─ YALNIZCA Host/Domain → Slug eşlemesi         │
│     (https://test-firma.com → "test-firma")        │
│     Query parametresi (örn. ?firma=) YASAK       │
│  HttpContext.Items["FirmaSlug"] = "test-firma"    │
└─────────────────────────┬────────────────────────┘
                          │
        ┌─────────────────┴─────────────────┐
        │                                   │
┌───────▼───────┐                   ┌───────▼───────┐
│   Firma A     │                   │   Firma B     │
│ firmalar/a/   │                   │ firmalar/b/   │
│ ├── a.db      │                   │ ├── b.db      │
│ ├── medya/    │                   │ ├── medya/    │
│ └── i18n/     │                   │ └── i18n/     │
└───────────────┘                   └───────────────┘

┌──────────────────────────────────────────────────┐
│  VizitLink3D.SuperAdmin (BAĞIMSIZ PROJE, :5200)  │
│  ├── superadmin.db (Firmalar, Moduller, Lisans)  │
│  ├── Firma oluşturma / klasör & DB basma         │
│  └── Firma API'si ile kod/process paylaşmaz      │
└──────────────────────────────────────────────────┘
```

### Şekette Tanımlanan Tenant Tespiti

`00_PROJE_BILGISI.md`'de:

```yaml
multi_tenant:
  aktif: true
  tenant_tespit: "domain"
  db_stratejisi: "database_per_tenant"
  superadmin_port: 5200
  superadmin_db: "superadmin.db"
  firma_db_yolu: "firmalar/{slug}/{slug}.db"
  firma_medya_yolu: "firmalar/{slug}/medya"
  firma_i18n_yolu: "firmalar/{slug}/i18n"
```

---

## 🔍 TENANT TESPİTİ (API Tarafı)

### 1. FirmaCozumlemeMiddleware
**Dosya:** `VizitLink3D.Api/AraYazilimlar/FirmaCozumlemeMiddleware.cs`

- **Tenant tespiti YALNIZCA isteğin `Host` (Domain) başlığından yapılır.** (`test-firma.com` → slug `test-firma`)
- Query parametresi (`?firma=`, `?firmaId=`) **KESİNLİKLE YASAKTIR** — ne geliştirmede ne de üretimde kabul edilir.
- Tenant, SuperAdmin'in `superadmin.db`'sindeki `Firmalar` tablosundaki Domain eşlemesiyle çözülür.
- **Fallback YOKTUR:** Bilinmeyen/geçersiz domain → hata yanıtı döner. "Varsayılan firma" kavramı kaldırılmıştır.

### 2. HttpContext.Items Şeması

Middleware işleminden sonra şu alanlar kullanılabilir:

```csharp
baglam.Items["FirmaId"]               // int
baglam.Items["FirmaSlug"]             // "test-firma" | "ornek-mobilya"
baglam.Items["FirmaDomain"]           // "test-firma.com"
baglam.Items["FirmaAd"]               // "Gold Banyo"
baglam.Items["VeriTabaniYolu"]        // "firmalar/test-firma/test-firma.db"
baglam.Items["MedyaKlasoru"]          // "firmalar/test-firma/medya"
baglam.Items["DilKlasoru"]            // "firmalar/test-firma/i18n"
baglam.Items["FirmaCozumlemeKaynagi"] // "DomainMapping"
```

### 3. API Endpoint'lerinde Tenant Kullanımı

```csharp
// ❌ YASAK — hardcoded slug
var url = "api/orpay/sayfa-icerigi/ayarlar";

// ❌ YASAK — query parametresi
var url = "api/sayfa-icerigi/ayarlar?firma=test-firma";

// ✅ DOĞRU — endpoint firmayı Host üzerinden çözer, URL'de slug yok
var url = "api/firma/bilgi";
```

---

## 👥 YETKİ AYRIMLAMASI (RBAC)

> **SuperAdmin tamamen bağımsız bir projedir** (`VizitLink3D.SuperAdmin`). Firma API'si ile aynı process/kod tabanında çalışmaz; sadece `VizitLink3D.Ortak` DLL'ini paylaşır. Bu yüzden Firma API'sinde "SuperAdmin" rolü **tanımlanmaz**.

### Firma Tarafı Roller (VizitLink3D.Api)

| Rol | İzinler | Doğrulama |
|---|---|---|
| **FirmaAdmin** | Kendi firması: tüm işler | `Kimlik.Rol == "FirmaAdmin" && Kimlik.FirmaId == HttpContext.Items["FirmaId"]` |
| **Editor** | İçerik yaz/düzenle | `Kimlik.Rol == "Editor"` + içerik sahipliği kontrolü |
| **Kullanici** | Sadece görüntüle | `Kimlik.Rol == "Kullanici"` |

### SuperAdmin Tarafı (VizitLink3D.SuperAdmin)

- `superadmin.db` üzerinden firmaları, modülleri ve lisansları yönetir.
- Tokenları **farklı bir imzalama anahtarı** ile üretilir; firma API tokenlarıyla asla karıştırılmaz.
- Firma verilerine doğrudan erişmez; firma oluşturma sırasında `firmalar/{slug}/` klasörünü ve `{slug}.db`'yi kurar.

### Koruma Mekanizması

**Kontroller'de:**

```csharp
[Authorize]
[HttpPost("api/sayfa-icerigi/kaydet")]
public async Task<Cevap<bool>> IcerigiKaydetAsync([FromBody] IcerigiKaydetDto dto)
{
    var firmaId = HttpContext.Items["FirmaId"] as int? ?? 0;
    var kullaniciFirmaId = User.FindFirstValue("FirmaId") is { } fmi && int.TryParse(fmi, out var kf) ? kf : 0;

    // ✅ Doğru: Kullanıcı yalnızca KENDİ firmasının verisiyle işlem yapabilir
    if (kullaniciFirmaId != firmaId)
        return Cevap<bool>.Izinsiz("Başka firmaya erişim yok");

    // ... işlem devam
}
```

### JWT Claim'leri

Token'da şu alanlar ZORUNLUDUR:

```json
{
  "sub": "kullanici-uuid",
  "Eposta": "admin@gold.com",
  "Rol": "FirmaAdmin",
  "FirmaId": 1,
  "FirmaSlug": "test-firma"
}
```

---

## 📊 VERİTABANI İZOLASYONU

### Strateji: Firma Başına Ayrı Veritabanı (Database Per Tenant)

```
Strateji: Firma Başına Ayrı SQLite DB (Database Per Tenant)
Sebep:   - Veriler fiziksel olarak tamamen izole (cross-tenant erişim imkânsız)
         - Arıza yalıtımı
         - Yedek ve taşıma operasyonları bağımsız
         - Skalabilite
```

### Veritabanı Dosya Organizasyonu

```
firmalar/
├── test-firma/
│   ├── test-firma.db        ← Firma özel veritabanı
│   ├── medya/              ← Firma görselleri / dosyaları
│   └── i18n/               ← Firma statik dil dosyaları (tr.json, en.json)
├── ornek-mobilya/
│   ├── ornek-mobilya.db
│   ├── medya/
│   └── i18n/
└── ...
```

> ⚠ Platform yönetimi SuperAdmin'in `superadmin.db` dosyasındadır. Firma DB'leri asla platform verisi içermez ve `Yedekler/db/` kökünde düz `.db` dosyaları barındırılmaz.

### DbContext Dinamik Ayarları

```csharp
// FirmaCozumlemeMiddleware'in doldurduğu slug üzerinden bağlantı yolu belirlenir
var firmaSlug = context.Items["FirmaSlug"]?.ToString();
var dbPath = Path.Combine("firmalar", firmaSlug, $"{firmaSlug}.db");

services.AddDbContextPool<VizitLink3DDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);
```

---

## 📁 MEDYA DEPOLAMA İZOLASYONU

### Klasör Organizasyonu

```
firmalar/{slug}/medya/
├── urunler/
├── icerik/
├── slaytlar/
├── haberler/
├── galeri/
└── ...
```

### Dosya Yükleme Kontrolleri

```csharp
// ✅ DOĞRU — firma kendi medya klasörüne yazar
var firmaSlug = HttpContext.Items["FirmaSlug"]?.ToString();
var klasor = Path.Combine("firmalar", firmaSlug, "medya", "urunler");
File.WriteAllBytes(Path.Combine(klasor, dosyaAdi), veriler);

// ❌ YASAK — hardcoded klasör / başka firmanın klasörüne yazma
var klasor = Path.Combine("wwwroot/medya", "partner-x", "icerik");
```

---

## 🖥️ FRONTEND'DE TENANT KULLANIMI

### Domain Tabanlı Çözüm

- UI (Blazor WASM), firma tespiti için **hiçbir zaman `?firma=` veya `?firmaId=` query parametresi kullanmaz.**
- Sayfa hangi domain'den yüklendiyse o firmanın UI'ı gelir; API tarafı `Host` başlığından firmayı çözer.
- `localStorage`'da `FirmaSlug` / `FirmaId` yalnızca **görsel/deneyim kolaylığı** için tutulur; firma tespit mekanizması değildir ve API'ye path/query olarak gönderilmez.

```csharp
// ❌ YASAK — query parametresi
var url = $"api/sayfa-icerigi/ayarlar?firma={_firmaSlug}&dil={dil.AktifDil}";

// ✅ DOĞRU — endpoint firmayı Host üzerinden çözer, URL'de slug/parametre yok
var url = $"api/firma/bilgi?dil={dil.AktifDil}";
```

> Not: UI tarafında slug'ı bilmek sadece görsel/deneyim amaçlıdır (ör. logo klasörü, tema). Yetkilendirme ve veri izolasyonu HER ZAMAN API tarafında `Host` çözümlemesiyle yapılır.

---

## 🔒 GÜVENLİK KONTROL LİSTESİ

```
[ ] Firma tespiti YALNIZCA Host/Domain üzerinden mi yapılıyor (?firma= yok)?
[ ] Query parametresi (firma/firmaId) hiçbir yerde kabul edilmiyor mu?
[ ] HttpContext.Items["FirmaSlug"] endpoint'lerde kullanılıyor mu?
[ ] Veritabanı bağlantısı "firmalar/{slug}/{slug}.db" yolundan mı açılıyor?
[ ] Medya dosyaları "firmalar/{slug}/medya/" klasörüne mi yazılıyor?
[ ] Dil dosyaları "firmalar/{slug}/i18n/{dil}.json"dan mı okunuyor?
[ ] SuperAdmin ayrı projede (:5200) ve ayrı "superadmin.db" ile mi çalışıyor?
[ ] SuperAdmin ve firma JWT imzalama anahtarları ayrı mı?
[ ] JWT token'da FirmaId + FirmaSlug claim'leri var mı?
[ ] Cross-tenant erişim testi yapıldı (firma-a → firma-b)?
[ ] Bilinmeyen domain için hata yanıtı dönülüyor mu (fallback firma yok)?
```

---

## 📋 İLGİLİ DOSYALAR

| Dosya | Amaç |
|---|---|
| `VizitLink3D.SuperAdmin/` | Bağımsız SuperAdmin projesi (port 5200, superadmin.db) |
| `VizitLink3D.Api/AraYazilimlar/FirmaCozumlemeMiddleware.cs` | Domain tabanlı tenant tespiti |
| `00_PROJE_BILGISI.md` | Tenant tespit & DB stratejisi |
| `07_Guvenlik_Passkey_JWT.md` | JWT & Rol yönetimi |
| `05_Veritabani_EFCore10.md` | DB izolasyon tekniği |
| `AGENTS.md` Kural 25–26 | SuperAdmin bağımsızlığı + query parametre yasağı |

---

## 🚨 ORTAK HATALAR

1. **Hardcoded tenant slug** — `"orpay"` veya `"test-firma"`'yu endpoint'lere gömmek
   - Çözüm: HttpContext.Items["FirmaSlug"] üzerinden çöz
2. **Query parametresi ile firma seçme** — `?firma=test-firma` veya `?firmaId=1`
   - Çözüm: Bu parametreler kabul edilmez; tenant yalnızca Host/Domain'den çözülür
3. **Firma verisini SuperAdmin DB'sine / kök DB'ye yazmak**
   - Çözüm: Her firma kendi `firmalar/{slug}/{slug}.db` dosyasına yazar; SuperAdmin sadece `superadmin.db` kullanır
4. **Medya dosyalarını global/wwwroot klasörüne yazmak**
   - Çözüm: `firmalar/{slug}/medya/` yapısı zorunludur
5. **Tenant-aware olmayan DbContext** — `new VizitLink3DDbContext()` ile sabit yola bağlanmak
   - Çözüm: DI ile dbcontext enjekte et; bağlantı yolu middleware'in çözdüğü slug'dan hesaplanır
6. **Varsayılan/fallback firma kullanmak** — bilinmeyen domain'de varsayılan firmaya düşmek
   - Çözüm: Fallback YOK; bilinmeyen domain hata döner

---

*Son güncelleme: 2026-08-06*
*Versiyon: 2.0 (Database-per-Tenant + Bağımsız SuperAdmin)*
