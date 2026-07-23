---
name: ajan-baslangic-indeksi
description: AjanKurallari klasörünün indeks dosyası. Hangi göreve hangi uzman dosya yüklenir, öz-denetim kontrol listesi, AI ajanın ilk yanıt formatı.
---

# 🌟 BAŞLA — Ajan Kuralları İndeksi

> **Sıra:** `AGENTS.md` → `00_PROJE_BILGISI.md` → bu dosya → uzman dosya → kod.

---

## 📚 UZMAN DOSYA HARİTASI

| Dosya | Uzmanlık |
|---|---|
| `02_CSharp_Disiplini.md` | C# 14 kod yazımı (async, DI, naming, partial class, magic number, null safety) |
| `03_Razor_MudBlazor_Blazor10.md` | Blazor 10 + MudBlazor — sayfa, bileşen, form, PersistentState |
| `04_CSS_Tema_Stitch_Entegrasyonu.md` | tokens.css + Stitch DESIGN.md → MudTheme + animasyon |
| `05_Veritabani_EFCore10.md` | EF Core 10 — migration, sorgu, index, soft delete, ExecuteUpdate |
| `06_API_Servisler_MediatR.md` | Cevap\<T\>, CQRS, MediatR, FluentValidation, Vertical Slice |
| `07_Guvenlik_Passkey_JWT.md` | Passkey, JWT, BCrypt, 2FA, JsonIgnore, CORS, CSP |
| `08_Performans_Cache_Render.md` | FusionCache, AsSplitQuery, Virtualize, lazy load |
| `09_Coklu_Platform_Web_Mobil_Masa.md` | Blazor WASM + MAUI + WPF + PWA + admin layout |
| `10_Test_Derleme_Pipeline.md` | xUnit, Testcontainers, pre-commit, CI/CD |
| `99_YASAKLAR_HIZLI_REFERANS.md` | 30+ yasak tek sayfa |

---

## 🎯 GÖREV → DOSYA EŞLEŞMESİ

```
"Yeni controller yaz"           → 02 + 06
"Sayfa / form / dialog"         → 03
"Tema rengini değiştir"         → 04
"Yeni tablo / migration"        → 05
"Yeni endpoint"                 → 06 + 07
"Giriş / Auth / Passkey"        → 07
"Yavaş çalışıyor / N+1"         → 08
"Mobil uygulama / MAUI"         → 09
"Test yaz / build hatası"       → 10
"Bir şey yapmadan kontrol"      → 99
```

---

## ✅ ÖZ-DENETİM KONTROL LİSTESİ (15 MADDE)

**Her kod üretiminden sonra ajan bu listeyi tek tek geçer:**

```
[ ] 1. AGENTS.md + 00_PROJE_BILGISI + ilgili uzman dosya okundu mu?
[ ] 2. %100 Türkçe? (sınıf, metot, değişken, DB, dosya, klasör)
[ ] 3. Hardcoded metin / renk / şifre yok mu?
[ ] 4. Try-catch kontrolcüde yok mu?
[ ] 5. .razor içinde <style> ve @code { } yok mu?
[ ] 6. Harici kütüphane Wrapper ile mi? (doğrudan çağrı yok)
[ ] 7. DB sütun adı ASCII mi? (Ş→S, İ→I, Ğ→G, Ü→U, Ö→O, Ç→C)
[ ] 8. [JsonIgnore] şifre/hash/token alanlarda mı?
[ ] 9. Cevap<T> dönüyor mu? (endpoint ise)
[ ] 10. DilServisi.T() ekran metinlerinde mi?
[ ] 11. tokens.css değişkenleri renk/font'ta mı?
[ ] 12. DRY ihlali yok mu? (kod tekrarı)
[ ] 13. Dosya < 1500 satır mı?
[ ] 14. Test yazıldı mı (minimum 5)?
[ ] 15. DB yedek alındı mı (büyük değişiklikse)?
```

**Tüm sorular ✅ olmadan görev tamamlanmış sayılmaz.**

---

## 🎬 İLK YANIT ŞABLONU

Her yeni sohbette ajanın ilk mesajı şu formatta:

```
🌟 AGENTS.md okundu — [00_PROJE_BILGISI.proje_adi] projesi için ajan başlatıldı.

   Konfigürasyon:
   ✓ Marka: [firma_adi]
   ✓ Sektör: [sektor]
   ✓ Domain: [url_birincil]
   ✓ Tema: [tema.varyant] ([tema.ikincil_renk])
   ✓ Modüller: [aktif modül listesi]

   Aktif uzman dosyalar: [göreve göre yüklenenler]
   Yasak listesi: ✓ aktif
   C# / .NET 10 dışı dil yasak: ✓

   Bekleniyor: Ustam'ın komutu.
```

---

## 📢 UYARI BİLDİRİM FORMATI

Kural ihlali tespit edilirse:

```
⚠ AJAN UYARISI
  Konum: [dosya:satır]
  Kural: [hangi dosya §X]
  İhlal: [kısa açıklama]
  Düzeltme: [ne yapıyor]
  Eylem: [otomatik düzeltiyor | onay bekliyor | reddediyor]
```

---

## 🔄 İHLAL DURUMUNDA AKIŞ

```
Ajan kod yazıyor
    ↓
Öz-denetim 15 madde kontrolü
    ↓
İhlal var mı?
    ├─ HAYIR → Kod tamamlandı, sun
    └─ EVET → Otomatik düzelt + uyarı bildirimi
              ↓
              Tekrar 15 madde kontrolü
              ↓
              Hala ihlal varsa → DURDUR + Ustam'a sor
```

---

## 📌 SIK YAPILAN HATALAR (DIKKAT)

1. **`@code { }` bloğunu .razor'a yazma** — partial class kullan (`.razor.cs`)
2. **DB sütun adında ı/ş** — ASCII'ye çevir
3. **Renk olarak `#c19b76` yazma** — `var(--ikincil-renk)` kullan
4. **`if (x == null) throw`** — `ArgumentNullException.ThrowIfNull(x)` modern
5. **`List<Kapi>`yi controller'da `.Where()` ile filtrele** — Repository / Specification katmanı
6. **AutoMapper ekleme** — Mapster zaten kurulu
7. **`appsettings.json`'a şifre yaz** — user-secrets veya env var

---

## 🛠 GÜNCEL TEKNOLOJI YIĞINI (00_PROJE_BILGISI'nden bağımsız sabit)

```
Backend:  ASP.NET Core 10 (.NET 10)
Dil:      C# 14
Frontend: Blazor WebAssembly + Blazor 10 (PersistentState, AddValidation, Passkey)
UI Lib:   MudBlazor (varsayılan)
Real-time: SignalR + MessagePack
DB:       SQLite (dev) / PostgreSQL (prod) + EF Core 10
Cache:    FusionCache (L1 + L2 Redis)
Log:      Serilog + Seq
PDF üretim:      QuestPDF (wrapper)
PDF görüntüleme: BlazorPdf / Gotho.BlazorPdf + PdfGosterici
Excel:    ClosedXML (wrapper)
Email:    MailKit (wrapper)
Image:    SixLabors.ImageSharp + ImageSharp.Web
Validation: FluentValidation
Mapping:  Mapster (AutoMapper YASAK)
CQRS:     MediatR
Auth:     JWT Bearer + 2FA TOTP + Passkey
Resilience: Polly
Container: (kaldırıldı)
```

---

*Versiyon: 1.0 | Bağlı: [AGENTS.md](../AGENTS.md), [00_PROJE_BILGISI.md](00_PROJE_BILGISI.md)*
