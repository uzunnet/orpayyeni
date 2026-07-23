# AGENTS.md

> **Evrensel AI Ajan Giriş Dosyası** (Linux Foundation AAIF standardı — Aralık 2025)
> Bu projeyi açan **her** AI modeli (Claude Code, Codex, Cursor, Copilot agent, Gemini CLI, Windsurf) önce bu dosyayı okur.

---

## 🚨 İLK OKUMA ZORUNLU SIRASI

```
1. AGENTS.md                              (← bu dosya — evrensel giriş)
2. AjanKurallari/00_PROJE_BILGISI.md      (← bu projenin marka/sektör/port/renk bilgisi)
3. AjanKurallari/12_Token_Optimizasyonu_Alt_Ajan_Kullanimi.md (← alt ajan seçim tablosu + token tasarrufu, M3 supervisor ve tüm alt ajanlar için ZORUNLU)
4. Görevle ilgili uzman dosya             (AjanKurallari/02-10 arası)
5. AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md (kod yazmadan önce göz at)
```

**Bu sırayla okumadan tek satır kod YAZILAMAZ.**

---

## 🎯 BU PROJENİN KİMLİĞİ

Proje markası, sektör, domain, port, tema renkleri **`AjanKurallari/00_PROJE_BILGISI.md`** dosyasındadır.

Sen (AI ajan) bu dosyadaki YAML front matter'ı okur, değerlerini **bağlam** olarak kullanırsın. Kurallar dosyalarında `[PROJE_ADI]`, `[FIRMA_ADI]`, `[SEKTOR]`, `[ANA_RENK]` gibi yer tutucular varsa — `00_PROJE_BILGISI.md`'den çeker.

---

## 📁 KLASÖR HARİTASI

```
[Proje Kökü]/
├── AGENTS.md                              ← şu an buradasın
├── .claude/CLAUDE.md                      ← Claude Code: "AGENTS.md oku" dedirten redirect
├── .cursor/rules.mdc                      ← Cursor: aynı redirect
├── .github/copilot-instructions.md        ← GitHub Copilot: aynı redirect
└── AjanKurallari/
    ├── 00_PROJE_BILGISI.md                ← PROJE KONFİGÜRASYONU
    ├── 01_BASLA.md                        ← indeks + öz-denetim
    ├── 02_CSharp_Disiplini.md             ← C# 14 + .NET 10 kod disiplini
    ├── 03_Razor_MudBlazor_Blazor10.md     ← UI, Blazor 10 yeni özellikleri
    ├── 04_CSS_Tema_Stitch_Entegrasyonu.md ← tokens.css + Stitch DESIGN.md
    ├── 05_Veritabani_EFCore10.md          ← migration, sorgu, index
    ├── 06_API_Servisler_MediatR.md        ← Cevap<T>, CQRS, Vertical Slice
    ├── 07_Guvenlik_Passkey_JWT.md         ← Passkey, JWT, BCrypt, JsonIgnore
    ├── 08_Performans_Cache_Render.md      ← FusionCache, AsSplitQuery, PersistentState
    ├── 09_Coklu_Platform_Web_Mobil_Masa.md← Blazor WASM + MAUI + WPF + PWA
    ├── 10_Test_Derleme_Pipeline.md        ← xUnit, Testcontainers, CI/CD
    ├── 11_SaaS_MultiTenant_Mimarisi.md    ← Tenant, domain, izolasyon
    ├── 12_Token_Optimizasyonu_Alt_Ajan_Kullanimi.md ← alt ajan seçim tablosu + token tasarrufu (M3 + tüm alt ajanlar için ZORUNLU)
    ├── 13_Tema_Sablon_Sistemi.md       ← 20+ temaya ölçeklenebilir şablon mimarisi, Stitch import, super admin (TÜM AI'lar için ZORUNLU)
    └── 99_YASAKLAR_HIZLI_REFERANS.md      ← 30+ yasak tek sayfa
```

---

## 🤖 HANGİ GÖREVDE HANGİ DOSYA?

| Görev | Yüklenecek Dosya(lar) |
|---|---|
| Yeni controller, servis, business logic | `02_CSharp_Disiplini.md` + `06_API_Servisler_MediatR.md` |
| Yeni Razor sayfa, bileşen, form, dialog | `03_Razor_MudBlazor_Blazor10.md` |
| Stil, tema, animasyon, Stitch entegrasyon | `04_CSS_Tema_Stitch_Entegrasyonu.md` |
| Yeni entity, migration, sorgu optimize | `05_Veritabani_EFCore10.md` |
| API endpoint, MediatR, FluentValidation | `06_API_Servisler_MediatR.md` + `07_Guvenlik_Passkey_JWT.md` |
| Auth, yetki, şifre, Passkey, JWT | `07_Guvenlik_Passkey_JWT.md` |
| Yavaş kod, N+1, cache, lazy | `08_Performans_Cache_Render.md` |
| MAUI, WPF, PWA, admin layout | `09_Coklu_Platform_Web_Mobil_Masa.md` |
| Test, build, CI/CD | `10_Test_Derleme_Pipeline.md` |
| SaaS, multi-tenant, domain izolasyon | `11_SaaS_MultiTenant_Mimarisi.md` |
| **Alt ajan seçimi, token tasarrufu, paralel tool çağrıları** | **`12_Token_Optimizasyonu_Alt_Ajan_Kullanimi.md` (HER GÖREVDE OKUNUR)** |
| **Yeni tema ekleme, tema değiştirme, Stitch import, super admin tema yönetimi, 20+ ölçeklenebilir mimari, TEMA = FARKLI SİTE felsefesi** | **`13_Tema_Sablon_Sistemi.md (TEMA İŞLERİNDE ZORUNLU)`** |
| Hızlı yasak kontrolü | `99_YASAKLAR_HIZLI_REFERANS.md` |

Birden fazla görev = birden fazla dosya birlikte yüklenir.

---

## 🔒 KESİN EMİRLER (Ustam'ın Doğrudan Talimatı — Tartışmasız, İhlal Edilemez)

1. **Veritabanı dosyası ASLA silinmez / yeniden oluşturulmaz / drop edilmez.** (`vizitlink3d.db` ve her ortamdaki karşılığı.) Şema değişikliği gerekiyorsa EF Core migration ile **eklenir**, mevcut dosya yerinde kalır.
2. **Hiçbir model/entity/tablo ASLA silinmez.** Kullanılmayan bir alan/tablo bulunsa bile kaldırılmaz — en fazla kullanılmadığı not edilir, kullanıcıya sorulur.
3. **Menü yapısı (`MenuOgeleri`) ve admin yapısı (Admin sayfaları, kontrolcüleri, yetkilendirme akışı) ASLA silinmez.** Sadece **ekleme** veya (gerekirse) soft-delete (`SilindiMi=true`) yapılır; fiziksel `DELETE` çalıştırılmaz.
4. Bu 3 madde için **kullanıcı onayı bile istisna oluşturmaz** — konu geçtiğinde ajan silme dışında bir yol (migration, ekleme, `WHERE`'li güncelleme, soft delete) önerir; gerekiyorsa Ustam'a doğrudan açıklar ama silme işlemini kendisi yapmaz.

5. **Tüm statik dosyalar (resim, PDF, 3D model, video) medya havuzunda kategorili olmak zorundadır.** `wwwroot/medya/` altında konu bazlı klasörler kullanılır (`/medya/urunler/`, `/medya/anasayfa/`, `/medya/haberler/`, `/medya/kurumsal/`, `/medya/slaytlar/`, `/medya/iletisim/`, `/medya/3d-modeller/` vb.). Medya havuzu dışında kalan hiyerarşik dosya (img/, models/, goldbanyo/ vb.) taşınır. Model kafasına göre CSS/JS dosyası eklenmez — CSS token sistemi (`tokens.css`) ve tema yapısı kullanılır.

> Not: Daha önce (bu projede) tek seferlik, kullanıcı onaylı ve dar kapsamlı bir `MenuOgeleri` temizliği yapılmıştı (reseed tetiklemek için). Bu emirden sonra böyle bir işlem **bir daha yapılmaz** — reseed gerekiyorsa kod tarafında guard/versiyon mantığıyla çözülür, veri silinerek değil.

---

## ⛔ EVRENSEL YASAKLAR (Tüm AI'lar İçin)

1. **Python (`*.py`), JavaScript/TypeScript iş mantığında, Bash/PowerShell script** — bu proje **%100 C# / .NET 10**. İzinli diller: C#, Razor, CSS, HTML, SQL (sadece EF üretimi), Markdown.
2. **MudBlazor dışında UI kütüphanesi** (Bootstrap, Ant, Radzen vb.) — `09_Coklu_Platform`'da alternatif gerektiğinde değerlendirilir.
3. **`.razor` içinde `<style>` etiketi** — CSS hep `wwwroot/css/sistem/` altında.
4. **`.razor` içinde `@code { }` bloğu** — her sayfa **partial class** (`*.razor.cs`).
5. **Hardcoded Türkçe metin** Razor'da — `DilServisi.T("anahtar", "Varsayılan")`.
6. **Hardcoded renk/font/boşluk** CSS'de — `var(--ana-renk)` gibi tokens.css değişkeni.
7. **Try-catch kontrolcüde** — `HataYonetimiMiddleware` yakalar.
8. **DB tablo/sütun adında Türkçe karakter** (Ş, İ, Ğ, Ü, Ö, Ç) — ASCII (S, I, G, U, O, C).
   > ⚠ **ÖNEMLİ:** Bu yasak **SADECE DB tanımlayıcılarını** (tablo adı, sütun adı, indeks adı, constraint adı) kapsar.
   > **UI metinlerinde düzgün Türkçe ZORUNLUDUR:** Razor `dil.T("anahtar", "Varsayılan")` ikinci argümanı, Snackbar mesajları, dialog metinleri, PageTitle, Label, Placeholder gibi kullanıcıya gösterilen **tüm** metinler tam ve doğru Türkçe karakterlerle (Ş, İ, Ğ, Ü, Ö, Ç, ı, ş, ğ) yazılır.
   > `dil.T()` ilk argümanı (çeviri anahtarı) ASCII kalır — değişmez.
9. **EF Migration dışı DB değişikliği** — PgAdmin/DBeaver elle tablo açmak yasak.
10. **Doğrudan harici kütüphane çağrısı** — Türkçe Wrapper servisi üzerinden.
11. **Şifre / token / API key log'a yazmak** — yapısal log gizli alan filtresi.
12. **`new` ile servis** (iş mantığında) — DI ile inject.
13. **`.Result` / `.Wait()`** — deadlock; her zaman `await`.
14. **AutoMapper** — Mapster kullan (daha hızlı).
15. **`AllowAnyOrigin()`** üretimde — CORS spesifik domain (`00_PROJE_BILGISI` `url_birincil`).
16. **Backdoor şifresi** — kod içinde sabit kimlik bilgisi YOK.
17. **Fiziksel DELETE** — soft delete (`SilindiMi`).
18. **Magic number / string** — `const` veya `appsettings`.
19. **DilServisi statik JSON'dan yüklenir** (`wwwroot/i18n/*.json`) — hızlı açılış. DB + FusionCache sadece dinamik içerikler için API arka planda senkronize eder. WASM'de FusionCache kullanılmaz.
20. **DB yedeği almadan migration / büyük değişiklik**.
21. **Medya havuzu dışında dosya barındırmak** — tüm görseller, PDF'ler, 3D modeller `wwwroot/medya/` altında kategorili klasörlerde olmalı. `img/`, `models/`, `goldbanyo/` gibi dağınık klasörler yasaktır. Hariç: `favicon.png`, `icon-192.png` (PWA manifest), `index.html`, `manifest.json`, `service-worker.js`, `_framework/`, `css/sistem/`, `css/temalar/`, `js/`, `i18n/`.
22. **FusionCache ve Serilog SADECE Api projesinde** — Blazor WASM (UI) bu paketleri KULLANMAZ. Tarayıcıda çalışmayan Redis/serilog.AspNetCore paketleri UI'a eklenemez.
23. **Yeni entity'lerde `DateTimeOffset` kullanılır** — `DateTime.UtcNow` yerine `DateTimeOffset.UtcNow`. Mevcut kodda kademeli geçiş yapılacak.
24. **Modül klasörlerinde `Servisler/` KULLANILMAZ** — CQRS mimarisinde iş mantığı Handler sınıflarına yazılır. Servisler sadece System katmanında wrapper olarak kullanılır (Bölüm 13).

Tam liste: `AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md`.

---

## ✅ EVRENSEL ZORUNLULUKLAR

1. **%100 Türkçe isimlendirme** (sınıf, metot, değişken, dosya, klasör, DB) — framework istisnaları hariç.
2. **UTF-8** encoding her dosyada (CSS için BOM).
3. **`DilServisi.T()`** her ekran metni için.
4. **`tokens.css`** her renk/font/boşluk için.
5. **`Cevap<T>`** her API yanıtında.
6. **FluentValidation** her DTO için.
7. **Partial class** her `.razor` için.
8. **Wrapper** her harici kütüphane için.
9. **Vertical Slice** klasör yapısı (`Moduller/<ModulAdi>/...`).
10. **Soft delete** (`SilindiMi`).
11. **Audit alanları** (`OlusturulmaTarihi`, `GuncellenmeTarihi`).
12. **`[JsonIgnore]`** şifre/token/hash alanlarına.
13. **DB yedek** her büyük değişiklik öncesi (`Yedekler/db/`).
14. **Minimum 5 test** her özellik (Testcontainers — gerçek PostgreSQL).
15. **`DateTime.UtcNow`** (lokal değil).
16. **Test mimarisi:** Yerel geliştirmede SQLite in-memory (hızlı). CI/CD'de Testcontainers (gerçek PostgreSQL). Yerelde Docker gerektiren test yasaktır.
17. **Tema değişimi = sadece renk değişimi DEĞİLDİR** — şekil, animasyon, tipografi, layout, ikonografi, boşluk ritmi HEPSİ değişir. Detay: `13_Tema_Sablon_Sistemi.md`.

---

## 🔁 ÖZ-DENETİM PROTOKOLÜ (Kod Yazmadan ÖNCE)

Her kod üretiminden önce ajan kendine sorar:

```
[ ] AGENTS.md + 00_PROJE_BILGISI + ilgili uzman dosya okundu mu?
[ ] %100 Türkçe? (framework istisnaları hariç)
[ ] Hardcoded metin / renk / şifre yok?
[ ] Try-catch kontrolcüde yok?
[ ] .razor içinde <style> veya @code yok?
[ ] Harici kütüphane Wrapper ile?
[ ] DB sütun adı ASCII (Türkçe karakter yok)?
[ ] [JsonIgnore] şifre alanlarda?
[ ] Cevap<T> dönüyor?
[ ] DilServisi.T() ekran metinlerinde?
[ ] tokens.css değişkenleri renk/font'ta?
[ ] DRY ihlali yok?
[ ] Dosya < 1500 satır?
[ ] Test yazıldı (≥5)?
[ ] DB yedek alındı?
```

**Tüm sorular ✅ olmadan kod tamamlanmış sayılmaz.**

---

## 📢 UYARI BİLDİRİM FORMATI

Ajan kuralı ihlal eden bir durum görürse Ustam'a şu formatta uyarı verir:

```
⚠ AJAN UYARISI
  Konum: UrunKontrolcu.cs:42
  Kural: AGENTS.md §7 (Try-catch kontrolcüde yasak)
  Düzeltme: HataYonetimiMiddleware'e bırakıyorum.
  Eylem: Otomatik düzeltiyorum.
```

---

## 🎬 İLK YANIT FORMATI (Her Yeni Sohbet)

```
🌟 AGENTS.md okundu — [proje_adi] projesi için ajan başlatıldı.
   Konfig: 00_PROJE_BILGISI ✓
   Aktif uzman: [02_CSharp, 03_Razor] (göreve göre)
   Yasak listesi: ✓ aktif
   C# / .NET 10 dışı dil yasak ✓
   Bekleniyor: Ustam'ın komutu.
```

---

## 📚 BAĞLI EVRENSEL STANDARTLAR

- **AGENTS.md** — https://agents.md/ (Linux Foundation AAIF)
- **W3C Design Tokens (DTCG)** — Stitch entegrasyonu için (`04_CSS_Tema_Stitch`)
- **OpenAPI 3.1** — API kontrat
- **OWASP ASVS** — güvenlik kontrolleri (`07_Guvenlik`)

---

## 🛑 ANTİ-PATTERN'LER (Bilinen Hatalar)

1. **Aynı kuralı 5 dosyada tekrar yazma** — drift olur. Tek SoT (bu dosya + AjanKurallari/).
2. **AI tarafından üretilen uzun kural dosyası** — ETH araştırması: başarıyı %3 düşürür, maliyeti %20 arttırır. İnsan yazımı kısa kurallar kazanır.
3. **Marka adını kurallar içine gömme** — `00_PROJE_BILGISI` ile parametrize et.
4. **Sektör-spesifik örnekler** (kapı, emlak, restoran) — generic terimler (`Urun`, `Musteri`, `Siparis`).
5. **Hardcoded port / domain / renk** — `00_PROJE_BILGISI`'ten oku.

---

*Versiyon: 1.0 — Aralık 2025 AGENTS.md standardı + Türkçe endüstriyel C# disiplini*
*Bu dosya marka-bağımsızdır. Proje-spesifik bilgi: AjanKurallari/00_PROJE_BILGISI.md*
