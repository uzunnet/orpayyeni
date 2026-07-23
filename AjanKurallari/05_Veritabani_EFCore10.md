---
name: veritabani-efcore10
description: EF Core 10 + SQLite/PostgreSQL Code-First migration disiplini. Türkçe karakter yasağı tablo/sütun adında, audit alanları, soft delete, JsonIgnore, index stratejisi, N+1 önleme, AsNoTracking, AsSplitQuery, ExecuteUpdate/Delete, JSON column, parçalı migration, otomatik yedek.
status: TAMAM
---

# 🗃 VERİTABANI / EF CORE 10

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [02_CSharp_Disiplini.md](02_CSharp_Disiplini.md)

---

## 1. 🚫 YASAKLAR

```
❌ PgAdmin / DBeaver / SSMS ile elle tablo açmak / değiştirmek
❌ Doğrudan SQL ile ALTER TABLE / CREATE TABLE
❌ CREATE TABLE IF NOT EXISTS (geçici çözüm kabul edilmez)
❌ Tablo / sütun adında Türkçe karakter (Ş İ Ğ Ü Ö Ç)
❌ Fiziksel DELETE (soft delete kullan)
❌ Senkron sorgu (.First(), .ToList(), .Count())
❌ N+1 sorgusu (her döngüde DB çağrısı)
❌ Tracking gereksiz (salt-okunurda AsNoTracking)
❌ Migration adı İngilizce / anlamsız ("Update1", "Changes")
❌ DB yedeği almadan production migration
❌ DateTime.Now (UTC değil — saat dilimi sorunu)
❌ EF migration'da hardcoded sektör/marka kelimesi
```

---

## 2. 📛 İSİMLENDİRME

### 2.1 Tablo Adı
- **PascalCase Türkçe çoğul + ASCII**
- Dönüşüm: Ş→S, İ→I, Ğ→G, Ü→U, Ö→O, Ç→C, ş→s, ı→i, ğ→g, ü→u, ö→o, ç→c

```
✅ Kullanicilar, Urunler, Musteriler, Siparisler, IletisimMesajlari
❌ Users, Kullanıcılar, Şubeler, Çalısanlar, Sİstem
```

### 2.2 Sütun Adı
- **PascalCase Türkçe + ASCII**

```
✅ OlusturulmaTarihi, KullaniciId, Aciklama, SiraNo
❌ CreatedAt, kullanici_id, AÇıklama, sira_no
```

### 2.3 Index Adı
- Tek sütun: `IX_TabloAdi_SutunAdi`
- Composite: `IX_TabloAdi_Sutun1_Sutun2`
- Unique: `IX_TabloAdi_SutunAdi_Unique`

### 2.4 Foreign Key Adı
- `FK_TabloAdi_HedefTablo_SutunAdi`
- Örnek: `FK_Urunler_Kategoriler_KategoriId`

### 2.5 Migration Adı (Türkçe Açıklayıcı)
```
✅ UrunKategorileriVeUrunlerEklendi
✅ KullaniciyeTotpAlaniEklendi
✅ MedyaHavuzuEklendi
✅ MusteriyeAdresAlanlariEklendi

❌ Update1, Update2
❌ Changes, NewMigration
❌ AddProduct, FixBug
```

---

## 3. 🏗 ENTITY TASARIMI

### 3.1 Temel İskelet (Her Entity'de Olması Gereken)
```csharp
namespace [PROJE_ADI].Ortak.Modeller.Urunler;

public class Urun
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }   // multi-tenant altyapı (nullable — opsiyonel)

    // İş alanları
    public string Slug { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? KisaAciklama { get; set; }
    public string? Aciklama { get; set; }

    public int KategoriId { get; set; }
    [JsonIgnore]
    public Kategori? Kategori { get; set; }

    public decimal? Fiyat { get; set; }
    public int StokAdedi { get; set; }
    public bool AktifMi { get; set; } = true;
    public bool OneCikan { get; set; }
    public int SiraNo { get; set; }

    // === AUDIT (zorunlu) ===
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GuncellenmeTarihi { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public int? GuncelleyenKullaniciId { get; set; }

    // === SOFT DELETE (zorunlu) ===
    public bool SilindiMi { get; set; }
    public DateTime? SilinmeTarihi { get; set; }
}
```

### 3.2 [JsonIgnore] Zorunlu Alanlar
Şifre / hash / token / secret içeren her alan:
```csharp
[System.Text.Json.Serialization.JsonIgnore]
public string SifreHash { get; set; } = string.Empty;

[System.Text.Json.Serialization.JsonIgnore]
public string? PinHash { get; set; }

[System.Text.Json.Serialization.JsonIgnore]
public string? WebAuthnPublicKey { get; set; }

[System.Text.Json.Serialization.JsonIgnore]
public string? TotpAnahtari { get; set; }

[System.Text.Json.Serialization.JsonIgnore]
public string? ApiKeyEncrypted { get; set; }
```

### 3.3 Navigation Property — [JsonIgnore] Zorunlu
Sonsuz döngüyü önler:
```csharp
public int KategoriId { get; set; }

[JsonIgnore]
public Kategori? Kategori { get; set; }

[JsonIgnore]
public ICollection<UrunResim> Resimler { get; set; } = new List<UrunResim>();
```

### 3.4 DateTime UTC Zorunlu
```csharp
// ✅
public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

// ❌ Lokal — saat dilimi sorunu
public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
```

### 3.5 String Default
```csharp
public string Ad { get; set; } = string.Empty;     // ✅ null warning yok
public string? Aciklama { get; set; }              // ✅ gerçekten opsiyonel
public string Ad { get; set; }                     // ❌ null warning
```

### 3.6 Yerelleştirme Tablosu (Çoklu Dil)
```csharp
public class UrunYerellestirme
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    [JsonIgnore]
    public Urun? Urun { get; set; }

    public string Dil { get; set; } = "tr";   // tr, en
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
}
```

---

## 4. 🔗 DBCONTEXT

### 4.1 DbSet Tanımı
```csharp
namespace [PROJE_ADI].Api.VeriTabani;

public class [PROJE_ADI]DbContext(DbContextOptions<[PROJE_ADI]DbContext> secenekler)
    : DbContext(secenekler)
{
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<UrunYerellestirme> UrunYerellestirmeleri => Set<UrunYerellestirme>();
    // ...
}
```

### 4.2 OnModelCreating
```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    // === UNIQUE INDEX'LER ===
    b.Entity<Urun>()
        .HasIndex(u => u.Slug)
        .IsUnique();

    b.Entity<Kullanici>()
        .HasIndex(k => k.Eposta)
        .IsUnique();

    // Composite unique (yerelleştirme)
    b.Entity<UrunYerellestirme>()
        .HasIndex(y => new { y.UrunId, y.Dil })
        .IsUnique();

    // === STRING MAX LENGTH ===
    b.Entity<Urun>()
        .Property(u => u.Slug).HasMaxLength(200);
    b.Entity<Urun>()
        .Property(u => u.Ad).HasMaxLength(200);

    // === SOFT DELETE GLOBAL FİLTRE ===
    b.Entity<Urun>()
        .HasQueryFilter(u => !u.SilindiMi);
    b.Entity<Kategori>()
        .HasQueryFilter(k => !k.SilindiMi);

    // === İLİŞKİLER (Cascade DİKKAT) ===
    b.Entity<Urun>()
        .HasOne(u => u.Kategori)
        .WithMany(c => c.Urunler)
        .HasForeignKey(u => u.KategoriId)
        .OnDelete(DeleteBehavior.Restrict);   // ❌ Cascade kullanma — Restrict tercih

    // === DECIMAL HASSASİYET ===
    b.Entity<Urun>()
        .Property(u => u.Fiyat)
        .HasPrecision(18, 2);

    base.OnModelCreating(b);
}
```

### 4.3 Connection String

`appsettings.json` (geliştirme):
```json
{
  "ConnectionStrings": {
    "[PROJE_ADI]": "Data Source=[proje].db"
  }
}
```

`appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "[PROJE_ADI]": "Host=db;Database=[proje];Username=...;Password=${DB_PASS}"
  }
}
```

---

## 5. 🚀 MIGRATION WORKFLOW

### 5.1 Tam Akış (Bash)
```bash
# 1. DB yedek al (anayasa zorunluluk)
cp [proje].db "Yedekler/db/[proje]_$(date +%Y%m%d_%H%M)_oncesi.db"

# 2. Migration oluştur (Türkçe açıklayıcı ad)
cd [PROJE_ADI].Api
dotnet ef migrations add UrunVeKategoriEklendi

# 3. SQL önizle (paranoid kontrol)
dotnet ef migrations script > "../Yedekler/sql/$(date +%Y%m%d)_UrunVeKategoriEklendi.sql"

# 4. Uygula
dotnet ef database update

# 5. Test çalıştır
cd ..
dotnet test

# 6. Sonrası yedek
cp [PROJE_ADI].Api/[proje].db "Yedekler/db/[proje]_$(date +%Y%m%d_%H%M)_sonrasi.db"
```

### 5.2 Migration Geri Alma
```bash
# Belirli migration'a dön
dotnet ef database update OncekiMigrationAdi

# Son migration'ı sil (henüz uygulanmadıysa)
dotnet ef migrations remove
```

### 5.3 Parçalı Migration (Zorunlu — AGENTS.md anti-pattern)
Tek dev migration'a 20 tablo yığma. Her iş paketi için ayrı migration:
```
1. UrunVeKategoriEklendi
2. MedyaHavuzuEklendi
3. AIAltyapisiEklendi
4. PazarlamaModulleriEklendi
5. KullaniciyePasskeyEklendi
```

### 5.4 Migration Üretildikten Sonra Kontrol
```bash
# Yeni migration ne yapacak?
dotnet ef migrations script LastMigration > onizle.sql
cat onizle.sql   # gözden geçir
```

---

## 6. 🔍 INDEX STRATEJİSİ

### 6.1 Zorunlu Index'ler

| Alan | Index Tipi |
|---|---|
| `Slug` | Unique |
| `Eposta` (Kullanici) | Unique |
| `Hash` (Medya) | Unique (partial — null hariç) |
| Foreign Key (`KategoriId` vb.) | Normal (EF otomatik) |
| `OlusturulmaTarihi` (sık filtre) | Normal |
| `(EntiteId, Dil)` (yerelleştirme) | Composite unique |
| `(FirmaId, Slug)` (multi-tenant) | Composite unique |
| `AktifMi` + `SilindiMi` | Filtre indeksli kompozit |

### 6.2 Partial Index (PostgreSQL — Null Hariç)
```csharp
b.Entity<Medya>()
    .HasIndex(m => m.Hash)
    .IsUnique()
    .HasFilter("\"Hash\" IS NOT NULL");   // PostgreSQL syntax
```

### 6.3 Filtered Index
```csharp
b.Entity<Urun>()
    .HasIndex(u => u.SiraNo)
    .HasFilter("\"AktifMi\" = true AND \"SilindiMi\" = false");
```

---

## 7. 🎯 SORGU KURALLARI

### 7.1 Async ZORUNLU
```csharp
// ❌
var u = _db.Urunler.First(x => x.Id == id);
var sayisi = _db.Urunler.Count();

// ✅
var u = await _db.Urunler.FirstOrDefaultAsync(x => x.Id == id);
var sayisi = await _db.Urunler.CountAsync();
```

### 7.2 N+1 Önleme (Kritik!)
```csharp
// ❌ N+1 — her urun için DB çağrısı
var urunler = await _db.Urunler.ToListAsync();
foreach (var u in urunler)
    u.Kategori = await _db.Kategoriler.FindAsync(u.KategoriId);

// ✅ EAGER LOAD
var urunler = await _db.Urunler
    .Include(u => u.Kategori)
    .ToListAsync();
```

### 7.3 AsNoTracking (Salt-Okunur Liste)
```csharp
var liste = await _db.Urunler
    .AsNoTracking()
    .Where(u => u.AktifMi)
    .ToListAsync();
```
**Kural:** Kayıt güncellenmiyorsa → `AsNoTracking()` (RAM tasarrufu + hız).

### 7.4 AsSplitQuery (Multi-Include Patlamasını Önle)
```csharp
var urun = await _db.Urunler
    .Include(u => u.Resimler)
    .Include(u => u.Yorumlar)
    .Include(u => u.Yerellestirmeler)
    .AsSplitQuery()                  // Kartezyen çarpımı önler
    .FirstOrDefaultAsync(u => u.Id == id);
```

### 7.5 Projection (DTO'ya Doğrudan)
```csharp
var ozetler = await _db.Urunler
    .Where(u => u.AktifMi)
    .Select(u => new UrunOzetDto
    {
        Id = u.Id,
        Ad = u.Ad,
        Slug = u.Slug,
        KategoriAdi = u.Kategori!.Ad
    })
    .ToListAsync();
```

### 7.6 ExecuteUpdate / ExecuteDelete (EF 7+ — Entity Yüklemeden Toplu)
```csharp
// Toplu güncelleme
await _db.Urunler
    .Where(u => u.KategoriId == eskiKategoriId)
    .ExecuteUpdateAsync(s => s.SetProperty(u => u.KategoriId, yeniKategoriId));

// Toplu soft delete
await _db.Urunler
    .Where(u => u.OlusturulmaTarihi < esik)
    .ExecuteUpdateAsync(s => s
        .SetProperty(u => u.SilindiMi, true)
        .SetProperty(u => u.SilinmeTarihi, DateTime.UtcNow));

// Toplu fiziksel sil (sadece çok eski soft delete'leri temizleme — admin script)
await _db.AuditLoglar
    .IgnoreQueryFilters()
    .Where(a => a.ZamanDamgasi < DateTime.UtcNow.AddYears(-7))
    .ExecuteDeleteAsync();
```

### 7.7 Bulk Insert (1000+ Kayıt)
```csharp
// EFCore.BulkExtensions ile
await _db.BulkInsertAsync(buyukListe);
await _db.BulkUpdateAsync(buyukListe);
await _db.BulkDeleteAsync(buyukListe);
```
**Kural:** 100'den fazla kayıt için `BulkInsertAsync`. Normal `AddRange` + `SaveChangesAsync` 100'ün altında OK.

### 7.8 CancellationToken Yayma
```csharp
public async Task<List<Urun>> ListeleAsync(CancellationToken iptal)
{
    return await _db.Urunler.AsNoTracking().ToListAsync(iptal);
}
```

---

## 8. 🗑 SOFT DELETE

### 8.1 Entity
```csharp
public bool SilindiMi { get; set; }
public DateTime? SilinmeTarihi { get; set; }
```

### 8.2 Global Query Filter (OnModelCreating)
```csharp
b.Entity<Urun>().HasQueryFilter(u => !u.SilindiMi);
```

### 8.3 Sil Metodu
```csharp
public async Task SilAsync(int id, int kullaniciId)
{
    var urun = await _db.Urunler.FindAsync(id)
        ?? throw new KaynakBulunamadiException($"Urun {id} bulunamadı.");

    urun.SilindiMi = true;
    urun.SilinmeTarihi = DateTime.UtcNow;
    urun.GuncelleyenKullaniciId = kullaniciId;
    urun.GuncellenmeTarihi = DateTime.UtcNow;

    await _db.SaveChangesAsync();
}
```

### 8.4 Geri Alma (Süper Admin)
```csharp
public async Task GeriAlAsync(int id)
{
    var urun = await _db.Urunler
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Id == id)
        ?? throw new KaynakBulunamadiException(...);

    urun.SilindiMi = false;
    urun.SilinmeTarihi = null;
    await _db.SaveChangesAsync();
}
```

### 8.5 Kalıcı Sil (Çöp Kutusu Temizleme — 30 Gün Sonra)
```csharp
var esik = DateTime.UtcNow.AddDays(-30);
await _db.Urunler
    .IgnoreQueryFilters()
    .Where(u => u.SilindiMi && u.SilinmeTarihi < esik)
    .ExecuteDeleteAsync();
```

---

## 9. 📦 JSON COLUMN (PostgreSQL JSONB)

### 9.1 Entity
```csharp
public class Urun
{
    public string? TeknikOzelliklerJson { get; set; }

    [NotMapped]
    public TeknikOzellikler? TeknikOzellikler
    {
        get => string.IsNullOrEmpty(TeknikOzelliklerJson)
            ? null
            : JsonSerializer.Deserialize<TeknikOzellikler>(TeknikOzelliklerJson);
        set => TeknikOzelliklerJson = value is null
            ? null
            : JsonSerializer.Serialize(value);
    }
}

public record TeknikOzellikler(
    string Malzeme,
    double KalinlikMm,
    double AgirlikKg,
    Dictionary<string, string>? EkBilgiler = null);
```

### 9.2 EF Core Config (JSONB)
```csharp
b.Entity<Urun>()
    .Property(u => u.TeknikOzelliklerJson)
    .HasColumnType("jsonb");   // PostgreSQL
```

### 9.3 PostgreSQL JSONB Sorgu
```csharp
var urunler = await _db.Urunler
    .Where(u => EF.Functions.JsonContains(u.TeknikOzelliklerJson, "{\"malzeme\":\"ahsap\"}"))
    .ToListAsync();
```

---

## 10. 💾 YEDEK POLİTİKASI

### 10.1 Otomatik Gündelik (Hangfire)
```csharp
// Program.cs
RecurringJob.AddOrUpdate<IYedeklemeServisi>(
    "gunluk-db-yedek",
    s => s.YedekAlAsync(),
    Cron.Daily(int.Parse(yapilandirma["00_PROJE_BILGISI:yedek:saat"]!.Split(':')[0])));
```

### 10.2 Manuel Yedek Komutu
```csharp
public interface IYedeklemeServisi
{
    Task YedekAlAsync(string aciklama = "manuel");
    Task<List<string>> YedekListele();
    Task GeriYukleAsync(string yedekDosyaAdi);
}
```

### 10.3 Saklama Politikası
- `00_PROJE_BILGISI.yedek.saklama_gun`: 30 (varsayılan)
- 30 günden eski yedekler otomatik silinir (Hangfire ile)
- Production yedekleri ayrıca **dış konuma** (S3, MinIO, R2) kopyalanmalı

---

## 11. 🔍 PERFORMANS İPUÇLARI (Sorun → Çözüm)

| Sorun | Çözüm | Detay |
|---|---|---|
| Yavaş liste | `AsNoTracking()` + projection | RAM + CPU tasarruf |
| Kartezyen patlama | `AsSplitQuery()` | 2+ collection Include'da |
| N+1 sorgu | `Include()` veya projection | Lazy loading YASAK |
| Yavaş count | Filtre kolonuna index | `WHERE` index'siz patlar |
| Sayfalama yavaş | Keyset pagination | Skip/Take 1000+ sayfada yavaş |
| Toplu update | `ExecuteUpdateAsync` | Entity yüklemeden |
| 1000+ insert | `BulkInsertAsync` | EFCore.BulkExtensions |
| Aynı sorgu tekrar | FusionCache (`OnbellekYonetici`) | Bkz. 08_Performans |
| JSON arama | JSONB + `EF.Functions.JsonContains` | PostgreSQL |
| Migration uzun | Parçala (her paket ayrı migration) | Token + risk azalır |

### 11.1 Keyset Pagination (Modern)
```csharp
// Skip/Take yerine — büyük sayfalarda hızlı
public async Task<List<Urun>> SonrakiSayfaAsync(int sonId, int boyut = 20)
{
    return await _db.Urunler
        .AsNoTracking()
        .Where(u => u.Id > sonId)
        .OrderBy(u => u.Id)
        .Take(boyut)
        .ToListAsync();
}
```

---

## 12. 🌍 MULTI-TENANT (Opsiyonel Altyapı)

`00_PROJE_BILGISI.multi_tenant.aktif` `true` ise:

### 12.1 Tüm Entity'lerde `FirmaId`
```csharp
public class Urun
{
    public int? FirmaId { get; set; }   // nullable — başlangıçta tek firma
}
```

### 12.2 Global Query Filter (Tenant İzolasyon)
```csharp
b.Entity<Urun>().HasQueryFilter(u =>
    !u.SilindiMi && u.FirmaId == _kiraciServisi.MevcutFirmaId);
```

### 12.3 KiraciServisi
```csharp
public class KiraciServisi(IHttpContextAccessor hca)
{
    public int? MevcutFirmaId =>
        hca.HttpContext?.Items["FirmaId"] as int?;
}
```

---

## 13. 📋 ÖZ-DENETİM (18 Madde)

```
[ ] 1. Tablo/sütun adı ASCII (Türkçe karakter YOK)
[ ] 2. Audit alanları (OlusturulmaTarihi, GuncellenmeTarihi)
[ ] 3. Soft delete (SilindiMi + global query filter)
[ ] 4. [JsonIgnore] şifre/hash/navigation alanlarda
[ ] 5. DateTime.UtcNow (DateTime.Now YASAK)
[ ] 6. string = string.Empty default
[ ] 7. Unique index slug/eposta/hash
[ ] 8. Composite unique yerelleştirmede (EntiteId, Dil)
[ ] 9. Migration adı Türkçe açıklayıcı
[ ] 10. Migration öncesi DB yedek alındı
[ ] 11. dotnet ef migrations script ile önizleme
[ ] 12. Async sorgu (FirstAsync, ToListAsync, vb.)
[ ] 13. N+1 yok (Include veya projection)
[ ] 14. Salt-okunurda AsNoTracking
[ ] 15. Multi-Include'da AsSplitQuery
[ ] 16. Toplu işlemde ExecuteUpdateAsync veya BulkInsertAsync
[ ] 17. Cascade Delete dikkatli (Restrict tercih)
[ ] 18. CancellationToken yayılıyor
```

---

*Versiyon: 1.0 | Tarih: 2026-05-14 | Bağlı: [02_CSharp_Disiplini.md](02_CSharp_Disiplini.md), [08_Performans_Cache_Render.md](08_Performans_Cache_Render.md)*
