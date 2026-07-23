---
name: csharp-disiplini
description: C# 14 + .NET 10 backend kod yazımı disiplini. Async/await, DI, dosya boyutu, partial class, magic number yasakları, null safety, naming, exception. Python/JS/Go/Rust/Java yasak — sadece C#. .NET 10 ve C# 14'ün yeni özellikleri (field keyword, partial constructors, extension blocks, collection expressions).
---

# 🔵 C# DİSİPLİNİ — C# 14 + .NET 10

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [00_PROJE_BILGISI.md](00_PROJE_BILGISI.md)
> **Görev:** Backend C# kod üretimi — controller, service, model, helper, extension.

---

## 1. 🚫 BU PROJEDE YASAK DİLLER

```
❌ Python (*.py)
❌ JavaScript / TypeScript iş mantığında (sadece izole wrapper içinde)
❌ Bash / PowerShell script iş mantığı
❌ Go, Rust, Java, PHP, Ruby, Kotlin, Swift, F#, VB
```

**İZİNLİ:** C# 14 / .NET 10, Razor, CSS, HTML, SQL (sadece EF migration üretimi), Markdown.

Cross-platform için bile MAUI / WPF / Blazor — **hep C#**.

---

## 2. 📂 DOSYA YAPISI

### 2.1 Dosya Boyutu
- **Max:** 1500 satır → aşılırsa partial class veya servis böl
- **İdeal:** 500-800 satır
- **Uyarı:** 1200 satır

### 2.2 Dosya Adı
- Türkçe + PascalCase: `KullaniciServisi.cs`, `UrunKontrolcu.cs`
- Suffix yerine ayrı sınıf: `_Yardimci`, `_Uzantilar` YASAK

### 2.3 File-Scoped Namespace (C# 10+)
```csharp
// ✅ DOĞRU
namespace [PROJE_ADI].Api.Moduller.Urunler;

public class UrunServisi { ... }

// ❌ ESKİ STİL
namespace [PROJE_ADI].Api.Moduller.Urunler
{
    public class UrunServisi { ... }
}
```

`[PROJE_ADI]` — `00_PROJE_BILGISI.md`'den okur.

---

## 3. 📛 İSİMLENDİRME

| Tür | Format | Örnek |
|---|---|---|
| Sınıf | PascalCase Türkçe | `KullaniciServisi` |
| Arayüz | I + PascalCase | `IKullaniciServisi` |
| Metot | Fiil + PascalCase + Async | `KullaniciEkleAsync()` |
| Property | PascalCase | `OlusturulmaTarihi` |
| Özel alan | _camelCase | `_kullaniciAdi` |
| Yerel değişken | camelCase | `kullaniciAdi` |
| Parametre | camelCase | `int kullaniciId` |
| Enum | PascalCase | `MedyaTipi.Resim` |
| Const | BUYUK_SNAKE | `MAKSIMUM_DENEME` |
| Generic Type | T + PascalCase | `TKaynak`, `TVeri` |
| Record | PascalCase + Komutu/Sorgusu/Dto | `UrunOlusturKomutu` |

### Async Metot
```csharp
// ✅
public async Task<Urun> UrunGetirAsync(int id)

// ❌
public async Task<Urun> UrunGetir(int id)          // Async eki yok
public async Task<Urun> GetProductAsync(int id)    // İngilizce
```

### Boolean
```csharp
// ✅
public bool AktifMi { get; set; }
public bool SilindiMi { get; set; }

// ❌
public bool Active { get; set; }
public bool IsActive { get; set; }
```

---

## 4. ⚡ C# 14 YENİ ÖZELLİKLER (KULLAN!)

### 4.1 `field` Contextual Keyword
Property getter/setter içinde compiler-generated backing field'a erişim:
```csharp
// ✅ C# 14
public string Ad
{
    get => field;
    set => field = value?.Trim() ?? string.Empty;
}

// ❌ ESKİ
private string _ad = string.Empty;
public string Ad
{
    get => _ad;
    set => _ad = value?.Trim() ?? string.Empty;
}
```

### 4.2 Partial Instance Constructors
```csharp
public partial class UrunServisi
{
    public partial UrunServisi(IDbContext db);   // tanım
}

public partial class UrunServisi
{
    public partial UrunServisi(IDbContext db)    // implementasyon
    {
        _db = db;
    }
}
```

### 4.3 Extension Blocks (Static + Instance)
```csharp
// ✅ C# 14
extension(string)
{
    public static string Slugify(this string metin) => ...;
    public bool BosMu => string.IsNullOrWhiteSpace(this);
}

// ❌ ESKİ (sadece static method extension)
public static class StringUzantilari { ... }
```

### 4.4 `nameof` Unbound Generics
```csharp
var ad = nameof(List<>);   // "List"
```

---

## 5. ⚡ .NET 10 KOLLEKSİYON İFADELERİ

```csharp
// ✅ MODERN
int[] sayilar = [1, 2, 3];
List<string> liste = ["a", "b", "c"];
Dictionary<string, int> harita = new() { ["bir"] = 1 };

// Spread
int[] birlesik = [..ilk, ..ikinci, 99];
```

---

## 6. ⏱ ASYNC / AWAIT

### 6.1 Tüm I/O Async
```csharp
// ❌
var k = _db.Urunler.First(x => x.Id == id);

// ✅
var k = await _db.Urunler.FirstOrDefaultAsync(x => x.Id == id);
```

### 6.2 `.Result` / `.Wait()` YASAK
```csharp
// ❌ DEADLOCK
var sonuc = _servis.GetirAsync().Result;

// ✅
var sonuc = await _servis.GetirAsync();
```

### 6.3 CancellationToken Yayma
```csharp
public async Task<List<Urun>> ListeleAsync(CancellationToken iptal)
{
    return await _db.Urunler.ToListAsync(iptal);
}
```

### 6.4 IAsyncEnumerable (Streaming)
```csharp
public async IAsyncEnumerable<Urun> AkisAsync(
    [EnumeratorCancellation] CancellationToken iptal = default)
{
    await foreach (var u in _db.Urunler.AsAsyncEnumerable().WithCancellation(iptal))
        yield return u;
}
```

### 6.5 ConfigureAwait
- Controller'da gereksiz (no synchronization context)
- Library kodunda `.ConfigureAwait(false)` tercih

---

## 7. 💉 DEPENDENCY INJECTION

### 7.1 Primary Constructor (C# 12+, varsayılan)
```csharp
// ✅ MODERN
public class UrunServisi(IDbContext db, ILogger<UrunServisi> log) : IUrunServisi
{
    public async Task<Urun?> GetirAsync(int id) => await db.Urunler.FindAsync(id);
}

// ✅ KLASİK (gerekirse)
public class UrunServisi : IUrunServisi
{
    private readonly IDbContext _db;
    public UrunServisi(IDbContext db) => _db = db;
}
```

### 7.2 `new` ile Servis YASAK (İş Mantığında)
```csharp
// ❌
var servis = new KullaniciServisi();

// ✅
public UrunKontrolcu(IKullaniciServisi servisi) { ... }
```

### 7.3 Service Lifetime
| Lifetime | Kullanım |
|---|---|
| `Scoped` | DbContext, business service (varsayılan) |
| `Singleton` | Cache, config reader, stateless |
| `Transient` | Hafif, tekrar oluşturulması ucuz |

### 7.4 Statik Servis YASAK
İstisna: Saf yardımcı (string extension, math) — state'siz.

---

## 8. 🛡 NULL SAFETY

### 8.1 Nullable Reference Types AÇIK
```xml
<Nullable>enable</Nullable>
```

### 8.2 Argüman Null Kontrol
```csharp
// ✅ MODERN (.NET 6+)
public void Kaydet(Urun urun)
{
    ArgumentNullException.ThrowIfNull(urun);
    ...
}

// ❌ ESKİ
if (urun == null) throw new ArgumentNullException(nameof(urun));
```

### 8.3 Default String
```csharp
public string Ad { get; set; } = string.Empty;     // ✅
public string? Aciklama { get; set; }              // ✅ gerçekten opsiyonel
public string Ad { get; set; }                     // ❌ null warning
```

### 8.4 Null-Conditional
```csharp
var uzunluk = urun?.Aciklama?.Length ?? 0;
var ad = istek.Ad ?? "Bilinmiyor";
```

---

## 9. 🚨 EXCEPTION YÖNETİMİ

### 9.1 Kontrolcüde Try-Catch YASAK
```csharp
// ❌
[HttpPost]
public async Task<IActionResult> Olustur(UrunDto d)
{
    try { ... } catch (Exception ex) { return BadRequest(ex.Message); }
}

// ✅
[HttpPost]
public async Task<Cevap<UrunDto>> Olustur(UrunDto d)
{
    var sonuc = await _s.OlusturAsync(d);
    return Cevap<UrunDto>.Basarili(sonuc);
}
```
`HataYonetimiMiddleware` tüm exception'ları yakalar.

### 9.2 Custom Exception (Servis Katmanı)
```csharp
public class KaynakBulunamadiException(string mesaj) : Exception(mesaj);

var urun = await _db.Urunler.FindAsync(id)
    ?? throw new KaynakBulunamadiException($"Ürün {id} bulunamadı.");
```

### 9.3 Specific Catch (Özel Durum)
```csharp
try { await _disApi.CagirAsync(); }
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
{
    await Task.Delay(5000);
    await _disApi.CagirAsync();
}
```
Çıplak `catch (Exception)` **YASAK**.

---

## 10. 🎯 Cevap<T> ZARFI

```csharp
public class Cevap<T>
{
    public bool BasariliMi { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public List<string> Hatalar { get; set; } = [];
    public T? Veri { get; set; }

    public static Cevap<T> Basarili(T veri, string mesaj = "İşlem başarılı.")
        => new() { BasariliMi = true, Veri = veri, Mesaj = mesaj };

    public static Cevap<T> Hata(string mesaj, List<string>? hatalar = null)
        => new() { BasariliMi = false, Mesaj = mesaj, Hatalar = hatalar ?? [] };
}
```

---

## 11. 🔢 MAGIC NUMBER / STRING YASAK

```csharp
// ❌
if (deneme > 5) { ... }
if (rol == "Admin") { ... }

// ✅
private const int MAKSIMUM_DENEME = 5;
private const string ROL_ADMIN = "Admin";

if (deneme > MAKSIMUM_DENEME) { ... }
```

Veya `appsettings.json`'dan:
```csharp
var dakika = int.Parse(_yapilandirma["JwtAyarlari:GecerlilikDakika"]!);
```

---

## 12. 🔧 PATTERN MATCHING & RECORDS

### Switch Expression
```csharp
var mesaj = sonuc switch
{
    { BasariliMi: true } => "Tamam",
    { Hatalar.Count: > 0 } => string.Join(", ", sonuc.Hatalar),
    _ => "Bilinmeyen durum"
};
```

### Record (Immutable DTO / Komut / Sorgu)
```csharp
public record UrunOlusturKomutu(
    string Ad,
    string Slug,
    int KategoriId,
    string? Aciklama) : IRequest<Cevap<UrunDto>>;
```

---

## 13. 📋 LINQ

### 13.1 Tek Sorguda Bitir
```csharp
// ❌ N+1
var liste = await _db.Urunler.ToListAsync();
foreach (var u in liste)
    u.Kategori = await _db.Kategoriler.FindAsync(u.KategoriId);

// ✅
var liste = await _db.Urunler.Include(u => u.Kategori).ToListAsync();
```

### 13.2 Projection
```csharp
var dtolar = await _db.Urunler
    .Where(u => u.AktifMi)
    .Select(u => new UrunOzetDto(u.Id, u.Ad, u.Slug))
    .ToListAsync();
```

### 13.3 AsNoTracking (Salt-Okunur)
```csharp
var liste = await _db.Urunler.AsNoTracking().ToListAsync();
```

### 13.4 AsSplitQuery (Multi-Include)
```csharp
var urun = await _db.Urunler
    .Include(u => u.Resimler)
    .Include(u => u.Yorumlar)
    .AsSplitQuery()
    .FirstOrDefaultAsync(u => u.Id == id);
```

---

## 14. 📊 LOG (Yapısal)

```csharp
// ✅
_logger.LogInformation(
    "Ürün oluşturuldu. {UrunId} {Slug} {KullaniciId}",
    urun.Id, urun.Slug, kullaniciId);

// ❌
_logger.LogInformation($"Ürün {urun.Id} oluşturuldu");
```

**ASLA log'a:** SifreHash, JWT, API key, IBAN, TC kimlik, AI prompt (kısaltılmış 500 char OK audit için).

---

## 15. 📋 ÖZ-DENETİM

```
[ ] %100 Türkçe (framework hariç)
[ ] async/await tam (Result/Wait yok)
[ ] DI ile inject (new yok)
[ ] ArgumentNullException.ThrowIfNull veya nullable
[ ] Kontrolcüde try-catch YOK
[ ] Cevap<T> dönüyor (endpoint)
[ ] Magic number/string YOK
[ ] N+1 yok (Include/Projection)
[ ] Yapısal log + gizli alan yok
[ ] Dosya < 1500 satır
[ ] XML summary public API'lerde
[ ] DRY ihlali yok
[ ] CancellationToken yayılıyor
[ ] AsNoTracking salt-okunurda
[ ] Test yazıldı (min 5)
[ ] C# 14 özellikleri (field, primary ctor, collection expr)
```

---

*Versiyon: 1.0 | Bağlı: [AGENTS.md](../AGENTS.md), [01_BASLA.md](01_BASLA.md)*
