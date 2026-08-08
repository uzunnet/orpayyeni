---
name: yasaklar-hizli-referans
description: Tüm yasakların tek sayfada özet tablosu. Kod yazmadan önce 30 saniyede taranır. Her yasağın yanında ilgili uzman dosya §X referansı.
status: TAMAM — tablo + referans (içerik doldurulmuş; sürekli güncellenir)
---

# 🚫 YASAKLAR — HIZLI REFERANS

> **Kullanım:** Kod yazmadan ÖNCE bu tabloyu tara. İhlal varsa düzelt → sonra yaz.

---

## 🔒 KESİN EMİRLER (Ustam'ın Talimatı — Onay Bile İstisna Değil)

| # | Yasak | Sebep / İlgili Dosya |
|---|---|---|
| 0a | DB dosyasını silmek/yeniden oluşturmak | [AGENTS.md](../AGENTS.md) §KESİN EMİRLER |
| 0b | Herhangi bir model/entity/tablo silmek | [AGENTS.md](../AGENTS.md) §KESİN EMİRLER |
| 0c | Menü (`MenuOgeleri`) veya admin yapısını silmek | [AGENTS.md](../AGENTS.md) §KESİN EMİRLER |

---

## 🔴 KIRMIZI ÇİZGİLER (TEK İHLAL = REDDEDİLİR)

| # | Yasak | Sebep / İlgili Dosya |
|---|---|---|
| 1 | Python (`*.py`), JS/TS iş mantığı, Go, Rust, Java, PHP | %100 C# / .NET 10 ([AGENTS.md](../AGENTS.md) §Yasaklar) |
| 2 | Bash / PowerShell script iş mantığı | C# servis kullan |
| 3 | MudBlazor dışında UI lib | `03_Razor...` §1 |
| 4 | `.razor` içinde `<style>` etiketi | `03_Razor...` §1, `04_CSS...` §1 |
| 5 | `.razor` içinde `@code { }` bloğu | `03_Razor...` §2 (partial class) |
| 6 | Hardcoded Türkçe metin Razor'da | `03_Razor...` §3 (DilServisi.T) |
| 7 | Hardcoded renk/font/boşluk CSS'de | `04_CSS...` §1 (tokens.css) |
| 8 | Try-catch kontrolcüde | `02_CSharp...` §9.1 (HataYonetimiMiddleware) |
| 9 | DB tablo/sütun adında Türkçe karakter | `05_DB...` §2 (Ş→S, İ→I, Ğ→G, Ü→U, Ö→O, Ç→C) |
| 10 | EF Migration dışı DB değişikliği | `05_DB...` §5 |
| 11 | Doğrudan harici kütüphane çağrısı | [AGENTS.md](../AGENTS.md) §Yasaklar (Wrapper) |
| 12 | Hardcoded port / domain / renk | `00_PROJE_BILGISI`'ten oku |
| 13 | Backdoor şifresi / hardcoded kimlik | `07_Guvenlik...` §1 |
| 14 | Şifre/token/key log'a yazmak | `02_CSharp...` §14, `07_Guvenlik...` §16 |
| 15 | `AllowAnyOrigin()` üretimde | `07_Guvenlik...` §7 |
| 16 | `[JsonIgnore]`'suz şifre/hash alanı | `07_Guvenlik...` §6 |
| 17 | `new` ile servis (iş mantığında) | `02_CSharp...` §7.2 (DI) |
| 18 | `.Result` / `.Wait()` | `02_CSharp...` §6.2 (deadlock) |
| 19 | AutoMapper kullanmak | `06_API...` §7 (Mapster) |
| 20 | Magic number / string | `02_CSharp...` §11 |
| 21 | Fiziksel DELETE (entity) | `05_DB...` §8 (soft delete) |
| 22 | `wwwroot/i18n/*.json` olmadan çeviri | `DilServisi` statik JSON'dan yükler, DB sadece dinamik içerik için |
| 23 | `AddOpenAPI` yerine eski Swashbuckle | `06_API...` §12 |
| 24 | DB yedeği almadan migration | `05_DB...` §5.2 + `10_Test...` §9 |
| 25 | Kod tekrarı (DRY ihlali) | [AGENTS.md](../AGENTS.md) ✅ Zorunluluklar |
| 26 | İngilizce isimlendirme (framework hariç) | `02_CSharp...` §3, [AGENTS.md] §Yasaklar |
| 27 | Sektör-spesifik örnek (kapı/emlak/restoran) | Tüm dosyalar — generic terim kullan |
| 28 | Brand adı (VIZITLINK3D/CanEmlak/vb.) | `[PROJE_ADI]` placeholder, `00_PROJE_BILGISI`'ten |
| 29 | Dosya > 1500 satır | `02_CSharp...` §2.1 (partial class böl) |
| 30 | Yerelde Docker/Testcontainers test'te | `10_Test...` §1 (SQLite in-memory dev'de) |
| 31 | Inline `style="..."` (dinamik olmadıkça) | `04_CSS...` §1 |
| 32 | `!important` CSS | `04_CSS...` §1 |
| 33 | ID seçici (`#id`) CSS | `04_CSS...` §1 |
| 34 | SignalR Hub adı İngilizce | `06_API...` §13 |
| 35 | FusionCache/Serilog UI (WASM) projede | `AGENTS.md` §22 — SADECE Api projesinde |
| 36 | `DateTime.UtcNow` yeni entity'de | `AGENTS.md` §23 — `DateTimeOffset.UtcNow` kullan |
| 37 | Modülde `Servisler/` klasörü | `AGENTS.md` §24 — CQRS'de iş mantığı Handler'a |
| 38 | `EnableDetailedErrors = true` (prod) | `07_Guvenlik...` §12 |
| 39 | PDF dosyasını ham tarayıcı sekmesinde açmak | `03_Razor...` §5.7 (BlazorPdf/PdfGosterici) |
| 40 | Medya havuzu dışında dosya barındırmak | `wwwroot/medya/` altında kategorili klasörler (`/medya/urunler/`, `/medya/anasayfa/`, `/medya/haberler/`, `/medya/kurumsal/`, `/medya/slaytlar/`, `/medya/iletisim/`, `/medya/3d-modeller/`). `img/`, `models/`, `eski-marka-ad/` yasak. Hariç: PWA manifest, `_framework/`, `css/sistem/`, `css/temalar/`, `js/`, `i18n/` |

---

## 🟡 SARI UYARILAR (TARTIŞMA AÇIK — GEREKÇE LAZIM)

| # | Durum | Açıklama |
|---|---|---|
| 1 | `IMemoryCache` doğrudan kullanmak | FusionCache wrapper tercih (§08) |
| 2 | `try-catch` servis katmanında | Sadece özel kurtarma mantığı varsa |
| 3 | `[AllowAnonymous]` endpoint | Liste: `/auth/giris`, `/iletisim`, `/kart/{slug}`, public read |
| 4 | `@((MarkupString)html)` | Sadece `IcerikTemizleyici` ile sanitize edilmiş içerikte |
| 5 | `ShouldRender` override | Çoğunlukla gereksiz — Blazor zaten optimize |
| 6 | `AsTracking()` listede | Sadece sonra güncellenecekse, salt-okunurda AsNoTracking |

---

## ✅ ZORUNLU KONTROLLER (KOD ÜRETMEDEN ÖNCE)

```
[ ] AGENTS.md + 00_PROJE_BILGISI + ilgili uzman dosya okundu mu?
[ ] %100 Türkçe? (framework hariç)
[ ] Hardcoded metin/renk/şifre/port YOK?
[ ] Try-catch kontrolcüde YOK?
[ ] .razor içinde <style> ve @code YOK?
[ ] Harici kütüphane Wrapper ile?
[ ] DB sütun adı ASCII?
[ ] [JsonIgnore] gizli alanlarda?
[ ] Cevap<T> dönülüyor (endpoint)?
[ ] DilServisi.T() ekran metinlerinde?
[ ] tokens.css değişkenleri renk/font'ta?
[ ] DRY ihlali yok?
[ ] Dosya < 1500 satır?
[ ] Test yazıldı (≥5)?
[ ] DB yedek alındı?
[ ] Brand adı `[PROJE_ADI]` placeholder?
[ ] Sektör-spesifik örnek YOK?
[ ] PDF görüntüleme BlazorPdf/PdfGosterici üzerinden mi?
```

---

## 📢 İHLAL TESPİT EDİLDİĞİNDE

```
⚠ AJAN UYARISI
  Konum: [dosya:satır]
  İhlal: [yasak #N — kısa açıklama]
  Düzeltme: [ne yapıyor]
  Referans: [hangi dosya §X]
```

---

*Versiyon: 1.3 — 47 yasak | Bağlı: tüm AjanKurallari/ dosyaları (özellikle 13_Tema_Sablon_Sistemi.md)*
*Bu liste sürekli güncellenir — yeni yasak çıkarsa Ustam'ın onayıyla eklenir.*

---

## 🎨 TEMA + DİL ETKİLEŞİM YASAKLARI (13_Tema_Sablon_Sistemi.md §19.8)

| # | Yasak | Referans |
|---|---|---|
| 41 | Tema adı/açıklaması hardcoded (Türkçe/İngilizce) | §13 §19.8 |
| 42 | Tema CSS'inde `content: "metin"` (Türkçe/İngilizce) | §13 §19.8 |
| 43 | Türkçe karakter desteklemeyen font (Bebas Neue, Anton) gövde metin için | §13 §19.8 |
| 44 | Tema değişikliği sırasında `DilServisi` cache'ini temizleme | §13 §19.8 |
| 45 | Tema manifest'inde `ad`/`aciklama` çeviri anahtarı olmadan yazma | §13 §19.8 |
| 46 | Tema-bazlı sayfada `dil.T(...)` yerine hardcoded string | §05 §13 §19.8 |
| 47 | `:root[data-tema-id="..."]` seçicisinde dil-bağımlı içerik | §13 §19.8 |


- **Türkçe karakter kırpımı** — UI metinlerinde S, I, G, Ü, Ö, Ç, i, s, g zorunludur. ASCII'ye indirgeme YASAK.