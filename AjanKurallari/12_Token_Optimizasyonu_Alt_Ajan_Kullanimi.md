---
name: token-optimizasyonu-alt-ajan-kullanimi
description: Alt ajan (subagent) seçim tablosu, token tasarrufu önlemleri, paralel tool çağrı stratejisi, büyük dosya okuma kuralları. Tüm AI ajanlar (M3 supervisor, coder-agir, coder-hizli, explore, yazici) bu dosyaya uyar.
status: AKTIF — zorunlu
---

# ⚡ TOKEN OPTİMİZASYONU & ALT AJAN KULLANIMI

> **AMAÇ:** M3 (supervisor) + tüm alt ajanlar (coder-hizli, coder-agir, explore, yazici, general) birlikte çalışırken **toplam token maliyetini minimize etmek** ve **görev döngüsünü hızlandırmak**.
>
> **KURAL:** Bu dosyadaki tüm önlemler **zorunludur**. Supervisor karar verirken ve alt ajan iş yaparken bu kurallara uyar.

---

## 1. ALT AJAN SEÇİM TABLOSU (M3 SUPERVISOR İÇİN)

> Supervisor (M3) bir görev aldığında **aşağıdaki tabloya göre** doğru ajana yönlendirir. Asla yanlış ajan seçmez.

| Görev Tipi | Yönlendirilecek Ajan | Model | Ücret |
|---|---|---|---|
| Basit düzeltme, tek dosya küçük ekleme, hızlı iterasyon, "X satırı düzelt", "şu renk değişsin" | **coder-hizli** (öncelikli) | opencode-zen/deepseek-v4-flash | ÜCRETSİZ |
| Free kota bitti / 429 / 503 aldıysa | **coder-hizli-yedek** (otomatik) | deepseek/deepseek-v4-flash | ÜCRETLİ |
| Karmaşık mimari, çok dosyalı refactoring, derin algoritma, "tasarım yap", "sıfırdan modül yaz" | **coder-agir** | deepseek/deepseek-v4-pro | ÜCRETLİ garantili |
| Genel yazma, dosya düzenleme, orta düzey, "şu dosyayı güncelle", "test ekle" | **yazici** | opencode-zen/mimo-v2.5 | ÜCRETSİZ |
| Sadece araştırma, okuma, "X dosyasında ne var", "nerede kullanılıyor" | **explore** | opencode-zen/mimo-v2.5 | ÜCRETSİZ |
| Karmaşık olmayan genel iş, paralel araştırma | **general** | opencode-zen/deepseek-v4-flash | ÜCRETSİZ |

> ⚠ **ASLA** `llama.cpp` veya yerel model ağır iş için kullanılmaz. Ağır iş = daima DeepSeek V4 Pro veya Flash.

### 1.1. ALT AJANA GÖREV VERİRKEN

- Görev prompt'u **kısa, net, eksiksiz** olmalı.
- "Gerekli olan şu: X. Bunu yap. Sonucu kısa özetle." formatı.
- **Gereksiz bağlam** verme — alt ajan zaten AGENTS.md'yi okur.
- Alt ajan **kanıt (build çıktısı, test sayısı, dosya yolu:satır)** ile dönmeli.

---

## 2. TOKEN TASARRUFU — 12 ALTIN KURAL

> Bu kurallar **M3 supervisor + tüm alt ajanlar** için bağlayıcıdır.

### 🔴 KURAL 1 — TEK SEferlik OKUMA
Bir dosyayı **bir oturumda ikinci kez okuma**. Gerekirse hafızada tutulan özeti kullan. Read tool maliyetlidir.

### 🔴 KURAL 2 — OFFSET + LIMIT İLE OKU
500 satırdan büyük dosyalarda:
```
read(filePath, offset=200, limit=100)
```
Asla "tüm dosyayı bir seferde oku" yapma. **Grep** ile önce satırı bul, sonra o bölgeyi oku.

### 🔴 KURAL 3 — PARALEL TOOL ÇAĞRISI
Birbirinden bağımsız tool çağrılarını **tek mesajda, paralel** yap:
```
✓ İYİ: Aynı anda 3 read, 2 glob, 1 grep
✗ KÖTÜ: Sırayla 6 mesaj, her birinde 1 tool
```
M3 supervisor bu kurala özellikle uyar. Sıralı tool çağrısı = gereksiz tur = gereksiz token.

### 🔴 KURAL 4 — KISA YANIT
Kullanıcıya verilen yanıt **mümkün olan en kısa** olmalı. Açıklama gerekliyse madde işareti, kısa cümle. Roman yazma.

### 🔴 KURAL 5 — TEKRAR ETME
Aynı bilgiyi iki kez yazma. "Yukarıda söyledim" de. Alt ajan çıktısı zaten döndüyse tekrar etme, **kullanıcıya özetle**.

### 🔴 KURAL 6 — "BİLMİYORUM" DEMEK YASAK
Bilmeyince "dokümantasyonu oku" deyip geçme. **context7_query-docs** veya **webfetch** ile gerçekten araştır, sonra kısa özetle.

### 🔴 KURAL 7 — KÜÇÜK DEĞİŞİKLİKTE TAM DOSYA YAZMA
Bir dosyada 3 satır değişecekse → **edit** tool kullan. Asla `write` ile tüm dosyayı tekrar yazma.

### 🔴 KURAL 8 — GEREKSIZ DOSYA LİSTELEME
"Bu klasörde ne var?" sorusu → **glob** ile pattern ver. `Get-ChildItem -Recurse` yapma.

### 🔴 KURAL 9 — UZUN BUILD ÇIKTISI KIRP
`dotnet build` çıktısı 200 satırı aşarsa **sadece hata/uyarı satırlarını** göster. Build başarılıysa tek satır: `Build: 0 hata, 0 uyarı, 42 sn.`

### 🔴 KURAL 10 — ALT AJANDAN KISA ÖZET İSTE
Alt ajana verilen prompt'un sonunda:
```
"Sonucunu 5 satırı geçmeyecek şekilde özetle. Sadece ne yaptın, hangi dosyalar, kanıt."
```
Bu, alt ajanın gereksiz uzun çıktı vermesini engeller.

### 🔴 KURAL 11 — "BANA DOKUNMA" PRENSİBİ
Kullanıcı yeni mesaj yazmadıkça **hiçbir ajan kendi başına yeni görev başlatmaz**. Idle'a düş, bekle.

### 🔴 KURAL 12 — TEKRARLAYAN BAĞLAMI CACHE'LE
Aynı proje bağlamını (marka adı, port, tema renkleri, modül listesi) her mesajda baştan yazma. **00_PROJE_BILGISI.md**'den tek satırla referans ver:
```
"00_PROJE_BILGISI: Gold Banyo, port 5115/5113, tema Gold Luxury Modern."
```

---

## 3. PARALEL ÇALIŞMA MATRİSİ

| Durum | Strateji |
|---|---|
| Birden fazla bağımsız dosya okunacak | Paralel read (tek mesajda) |
| Birden fazla bağımsız dosya yazılacak | Önce hepsini paralel read, sonra paralel write |
| Build + test paralel çalışabilir | Tek mesajda iki bash çağrısı |
| Birden fazla araştırma görevi | **explore** ajanına paralel sub-task |
| Build uzun sürüyorsa | Timeout=180000, çıktıyı dosyaya yaz, sonra grep ile oku |

---

## 4. DENETÇİ (KULLANICI) İÇİN ÖZET FORMATI

> M3 supervisor, kullanıcıya her alt ajan görevi sonrası **şu formatta** rapor verir. Bu format hem kısa hem kanıt içerir:

```
📋 [Görev Adı]
   Yapılan: [1-2 cümle]
   Ajan/Model: [örn. coder-hizli / DeepSeek V4 Flash Free]
   Değişen: [dosya:yol:satır] (örn. Foo.cs:42-55)
   Kanıt: [build: 0 hata | test: 7/7 ✓ | screenshot: yol/png]
   Sonraki: [denetçi onayı bekleniyor / otomatik geçti]
```

---

## 5. FREE KOTA TÜKENDİ ALGORİTMASI (M3)

```
1. coder-hizli çağır (DeepSeek V4 Flash Free, Zen üzerinden)
2. 429 / 503 / "quota exceeded" / "free tier limit" hatası geldi mi?
   ├─ HAYIR → Sonuç geldi, devam
   └─ EVET → OTOMATIK coder-hizli-yedek'e yönlendir
             Kullanıcıya kısa not: "Free kota tükendi, ücretli yedek ajana geçildi."
3. coder-hizli-yedek de hata verdi mi?
   ├─ HAYIR → Devam
   └─ EVET → Supervisor DURUR, kullanıcıya rapor verir
```

---

## 6. YASAKLAR (TOKEN İSRAFI YAPAN DAVRANIŞLAR)

| # | Yasak | Sebep |
|---|---|---|
| 1 | Tüm solution'ı `Get-ChildItem -Recurse` ile listeleme | Token israfı + yavaş |
| 2 | 500+ satırlık dosyayı tam okuma | Grep + offset+limit kullan |
| 3 | Build çıktısının tamamını kullanıcıya gösterme | Sadece hata/uyarı |
| 4 | Alt ajan sonucunu "olduğu gibi" kopyalama | Özetle |
| 5 | Aynı bilgiyi birden fazla tool ile tekrar getirme | Cache'le |
| 6 | Yorum/açıklama satırlarını uzun uzun yazma | Kısa + öz |
| 7 | Kullanıcıya "Şimdi şunu yapacağım, önce şunu okuyacağım..." diye uzun niyet beyanı | Doğrudan yap, kısa bildir |
| 8 | Kullanıcı sormadan ek dosya/kütüphane önerme | İstenmeden ekleme |
| 9 | Aynı tool'u 2+ kez çağırma (aynı argümanlarla) | Veriyi hafızada tut |
| 10 | "Görsel olarak açıklayayım" deyip ASCII sanat yapma | Metin yeterli |
| 11 | Build sırasında `--no-restore --no-build` zincirlerini yanlış kurma | Tek sefer doğru parametre |
| 12 | Ücretsiz ajanı zorla ücretli olmayan bir iş için kullanma | Tabloya uy |

---

## 7. HIZLI KONTROL LİSTESİ (HER GÖREV BAŞINDA)

```
[ ] Görev tipi doğru mu? (tabloya bak, uygun ajana yönlendir)
[ ] Paralel yapılabilecek tool çağrıları tek mesajda mı?
[ ] Büyük dosyalar offset+limit ile mi okunuyor?
[ ] Alt ajana kısa ve net prompt mu veriliyor?
[ ] Yanıt kullanıcıya kısa özet olarak mı gidiyor?
[ ] Build/test çıktısı kırpılmış mı?
[ ] Aynı bilgi tekrar yazılmıyor mu?
[ ] Kullanıcı yeni mesaj yazmadan yeni görev başlatılmıyor mu?
```

---

## 8. BAĞLANTILAR

- **AGENTS.md** — evrensel giriş
- **AjanKurallari/00_PROJE_BILGISI.md** — proje konfigürasyonu
- **AjanKurallari/01_BASLA.md** — indeks
- **AjanKurallari/99_YASAKLAR_HIZLI_REFERANS.md** — kod yasakları
- **Supervisor sistem prompt'u** — akış tanımı

---

*Versiyon: 1.0 — Haziran 2026 | Token maliyeti minimizasyonu + alt ajan disiplini*
*Bu dosya M3 supervisor ve tüm alt ajanlar için ZORUNLUDUR.*
