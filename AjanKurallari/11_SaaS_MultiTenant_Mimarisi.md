---
name: "saas-multi-tenant"
description: "SaaS multi-tenant mimarisinin kuralları, tenant tespiti, veritabanı yalıtımı, yetki ayrımı"
metadata:
  type: "project"
  updated: "2026-06-26"
---

# 11. SaaS Multi-Tenant Mimarisi

> 3DVizitLink, tek bir kod tabanında birden fazla müşteri (tenant) hizmet eden **Software as a Service (SaaS)** uygulamasıdır. Her müşteri kendi veritabanı ve dosya depolama klasörüne sahiptir.

---

## 🏗️ MİMARİ GENEL BAKIŞ

### Multi-Tenant Katmanları

```
┌──────────────────────────────────────────────────┐
│         İnternet (Kullanıcı)            │
└─────────────────────────┬──────────────────────┘
                   │ Domain/Subdomain
┌─────────────────────────▼──────────────────────────┐
│  FirmaCozumlemeMiddleware (API)         │ ↓ Tenant tespiti burada
│  ├─ Domain → Firma maplemesi            │
│  ├─ Query param "firma=" (dev ortamda)  │
│  └─ HttpContext.Items["FirmaSlug"] = X  │
└─────────────────────────┬──────────────────────────┘
                   │
        ┌──────────────────┴──────────────┐
        │                     │
   ┌────▼────┐          ┌────▼────┐
   │ Firma A │          │ Firma B │
   │ DB: a.db│          │ DB: b.db│
   │ Media/a/│          │ Media/b/│
   └────────┘          └────────┘
```

### Şekette Tanımlanan Tenant Tespiti

`00_PROJE_BILGISI.md`'de:
```yaml
multi_tenant:
  aktif: true
  tenant_tespit: "domain"  # ↓ Bu kurala uyulmalı
```

---

## 🔍 TENANT TESPİTİ (API Tarafı)

### 1. FirmaCozumlemeMiddleware
**Dosya:** `VizitLink.Api/AraYazilimlar/FirmaCozumlemeMiddleware.cs`

- **Localhost (Geliştirme):** Query param `?firma=goldbanyo` ile override
- **Production:** Domain taraması (Firma.Domain veya Firma.YedekDomain)
- **Fallback:** `FirmaProfili.Slug` ("goldbanyo" — varsayılan firma)

### 2. HttpContext.Items Şeması

Middleware işleminden sonra şu alanlar kullanılabilir:

```csharp
baglam.Items["FirmaId"]              // int
baglam.Items["FirmaSlug"]            // "goldbanyo" | "partner-a" | ...
baglam.Items["FirmaDomain"]          // "goldbanyom.com.tr"
baglam.Items["FirmaAd"]              // "Gold Ban-yom"
baglam.Items["VeriTabaniYolu"]       // "goldbanyo.db"
baglam.Items["MedyaKlasoru"]         // "wwwroot/medya/goldbanyo"
baglam.Items["KiraciCozumlemeKaynagi"] // "LocalDatabase" | "SharedHost" | "DomainMapping"
```

### 3. API Endpoint'lerinde Tenant Slug Kullanımı

```csharp
// ❌ YASAK — hardcoded slug
var url = "api/3DVizitLink/sayfa-icerigi/ayarlar";

// ✅ DOĞRU — dynamic slug
var firmaSlug = HttpContext.Items["FirmaSlug"]?.ToString() ?? "goldbanyo";
var url = $"api/{firmaSlug}/sayfa-icerigi/ayarlar";
```

---

## 👥 YETKİ AYRIMLAMASI (RBAC)

### Roller Tanımı

| Rol | İzinler | Alan |
|---|---|---|
| **Super Admin** | Tüm firmalar, sistem ayarları | `Kimlik.Rol == "SuperAdmin"` |
| **Firma Admin** | Kendi firması: tüm işler | `Kimlik.Rol == "FirmaAdmin" && Kimlik.FirmaId == HttpContext.Items["FirmaId"]` |
| **Editor** | İçerik yazı/düzenle | `Kimlik.Rol == "Editor"` + İçerik kontrolü |
| **Viewer** | Sadece görüntüle | `Kimlik.Rol == "Viewer"` |

### Koruma Mekanizması

**Kontroller'de:**

```csharp
[Authorize]
[HttpPost("api/sayfa-icerigi/kaydet")]
public async Task<Cevap<bool>> IcerigiKaydetAsync([FromBody] IcerigiKaydtDto dto)
{
    var firmaId = HttpContext.Items["FirmaId"] as int? ?? 0;
    var kullaniciRol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    
    // ✅ Doğru: Firma Admin ise kendi firması mı?
    if (kullaniciRol == "FirmaAdmin")
    {
        var kullaniciiFirmaId = User.FindFirst("FirmaId") as int?;
        if (kullaniciiFirmaId != firmaId)
            return Cevap<bool>.Izinsiz("Başka firmaya erişim yok");
    }
    
    // ✅ Doğru: Super Admin her şeye erişebilir
    if (kullaniciRol != "SuperAdmin" && kullaniciRol != "FirmaAdmin")
        return Cevap<bool>.Izinsiz("Sadece Admin işlemi yapabilir");
    
    // ... işlem devam
}
```

### JWT Claim'leri

Token'da şu alanlar ZORUNLUDUR:

```json
{
  "sub": "kullanici-uuid",
  "KullaniciAdi": "admin@gold.com",
  "Rol": "FirmaAdmin",
  "FirmaId": 1,
  "FirmaSlug": "goldbanyo"
}
```

---

## 📊 VERİTABANI İZOLASYONU

### Multi-Tenant Veritabanı Stratejisi

```
Strategi: Firma Başına Ayrı DB (Database Per Tenant)
Sebep:   - Veriler tamamen izole
         - Arıza yalıtımı
         - Yedek ve taşıma operasyonları bağımsız
         - Skalabilite
```

### Veritabanı Dosya Organizasyonu

```
Yedekler/db/
├── goldbanyo.db              ↓ Varsayılan/Ana firma
├── partner-a.db              ↓ Multi-tenant Firma A
├── partner-b.db              ↓ Multi-tenant Firma B
└── README.md                 ↓ Hangi DB hangi firma?
```

### DbContext İni Ayarları

`Program.cs`'de:

```csharp
var firmaSlug = context.Items["FirmaSlug"]?.ToString() ?? FirmaProfili.Slug;
var dbPath = Path.Combine("Yedekler/db", $"{firmaSlug}.db");

services.AddDbContextPool<VizitLinkDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")
);
```

---

## 📁 MEDYA DEPOLAMA İZOLASYONU

### Klasör Organizasyonu

```
wwwroot/medya/
├── goldbanyo/                ↓ FirmaProfili.MedyaKlasoru
│   ├── icerik/
│   ├── urunler/
│   ├── projetler/
│   └── ...
├── partner-a/
│   └── ...
└── partner-b/
    └── ...
```

### Dosya Yükleme Kontrolleri

```csharp
// ✅ DOĞRU
var firmaSlug = HttpContext.Items["FirmaSlug"]?.ToString() ?? "goldbanyo";
var klasor = Path.Combine("wwwroot/medya", firmaSlug, "icerik");
File.WriteAllBytes(Path.Combine(klasor, dosyaAdi), veriler);

// ❌ YASAK — cross-tenant yükleme
var klasor = Path.Combine("wwwroot/medya", "partner-x", "icerik");
```

---

## 🖥️ FRONTEND'DE TENANT SLUG'I KULLANIMI

### API Call'larında Dinamik Slug

**VizitLinkDuzen.razor.cs & AdminDuzen.razor.cs:**

```csharp
// ❌ YASAK — hardcoded
var url = "api/3DVizitLink/sayfa-icerigi/ayarlar";

// ✅ DOĞRU — localStorage veya injected servis ile
private string _firmaSlug = "goldbanyo"; // varsayılan

protected override async Task OnInitializedAsync()
{
    _firmaSlug = await js.InvokeAsync<string>("localStorage.getItem", "FirmaSlug") 
                 ?? "goldbanyo";
    
    var url = $"api/{_firmaSlug}/sayfa-icerigi/ayarlar?dil={dil.AktifDil}";
    var settings = await api.GetAsync<Dictionary<string, string>>(url);
}
```

### localStorage Şeması

```javascript
// Sayfa yükleme sırasında set edilir
localStorage.setItem("FirmaSlug", "goldbanyo");
localStorage.setItem("FirmaId", "1");
localStorage.setItem("KullaniciRol", "FirmaAdmin");
```

---

## 🔒 GÜVENLİK KONTROL LİSTESİ

```
[ ] API endpoint'ler HttpContext.Items["FirmaSlug"] kullanıyor?
[ ] Kontroller'de Rol kontrolleri var (Super/Firma Admin)?
[ ] Dosya yükleme: FirmaSlug klasörüne mi yazılıyor?
[ ] Veritabanı sorguları: WHERE FirmaId = ? var mı?
[ ] Cross-tenant erişim testi yapıldı (firma-a → firma-b)?
[ ] JWT token'da FirmaId claim'i var mı?
[ ] Admin paneli: Çoklu firma yönetimi UI'ı var mı?
[ ] Yetki hiyerarşi: SuperAdmin > FirmaAdmin > Editor > Viewer?
```

---

## 📋 İLGİLİ DOSYALAR

| Dosya | Amaç |
|---|---|
| `FirmaCozumlemeMiddleware.cs` | Tenant tespiti |
| `FirmaProfili.cs` | Varsayılan firma config |
| `00_PROJE_BILGISI.md` | Tenant tespit stratejisi |
| `07_Guvenlik_Passkey_JWT.md` | JWT & Rol yönetimi |
| `05_Veritabani_EFCore10.md` | DB izolasyon tekniği |

---

## 🚨 ORTAK HATALAR

1. **Hardcoded tenant slug** — `"3DVizitLink"` veya `"goldbanyo"`'yu tüm endpoint'lere yapıştırmak
   - Çözüm: HttpContext.Items veya localStorage'dan çek

2. **Rol kontrolü olmadan veri işleme** — Firma Admin'i başka firmanın verilerine eriştirebilir
   - Çözüm: Her işlemde FirmaId kontrolü ve Rol doğrulaması

3. **Medya dosyaları global klasöre yazma** — `wwwroot/medya/icerik/` yerine tenant klasörü yok
   - Çözüm: `wwwroot/medya/{FirmaSlug}/icerik/` yapısını zorunlu kıl

4. **DB Migrasyonları: Tenant-aware olmayan sorgu** — `new VizitLinkDbContext()` vb.
   - Çözüm: DI ile dbcontext'i enjekte et (otomatik tenant tespiti)

---

*Son güncelleme: 2026-06-26*
*Versiyonu: 1.0*
