---
name: test-derleme-pipeline
description: xUnit + Testcontainers (gerçek PostgreSQL) + bUnit (Blazor) + Playwright (E2E). Minimum 5 test her özellik. Pre-commit kontrol listesi, DB yedek protokolü, GitHub Actions CI, Coolify deploy, SonarQube kalite, k6 yük testi, smoke test.
status: TAMAM
---

> ⚠ **TEST MİMARİSİ KURALI:** 
> - **Yerel geliştirme (dev):** Testler `Microsoft.Data.Sqlite` in-memory moduyla çalışır. 0.1sn'de biter, Docker gerektirmez.
> - **CI/CD (sunucu):** `Testcontainers.PostgreSql` ile gerçek PostgreSQL kullanılır. SQLite'a özgü SQL yazımı testlerde patlayabilir, dikkat!
> - **KURAL:** Yerel testlerde Testcontainers KULLANILMAZ. Sadece CI/CD pipeline'ında zorunludur.

# 🧪 TEST / DERLEME / PIPELINE

> **Önkoşul:** [AGENTS.md](../AGENTS.md), tüm 02-09 dosyaları
> **Felsefe:** Test yazılmayan kod = çalışmayan kod (varsayım).

---

## 1. 📐 TEST PROJESİ İSKELETİ

### 1.1 Proje Oluştur
```bash
cd [PROJE_KOK]
dotnet new xunit -n [PROJE_ADI].Testler
dotnet sln add [PROJE_ADI].Testler/[PROJE_ADI].Testler.csproj

cd [PROJE_ADI].Testler
dotnet add reference ../[PROJE_ADI].Api/[PROJE_ADI].Api.csproj
dotnet add reference ../[PROJE_ADI].Ortak/[PROJE_ADI].Ortak.csproj

# Kritik NuGet'ler
dotnet add package Microsoft.AspNetCore.Mvc.Testing
# YEREL GELISTIRME: SQLite in-memory (hizli, Docker gerekmez)
dotnet add package Microsoft.Data.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
# CI/CD: Testcontainers (gercek PostgreSQL)
dotnet add package Testcontainers.PostgreSql
dotnet add package FluentAssertions
dotnet add package bunit.web
dotnet add package Moq
dotnet add package Bogus              # fake data
dotnet add package coverlet.collector # code coverage
```

### 1.2 Klasör Yapısı
```
[PROJE_ADI].Testler/
├── Moduller/
│   ├── Urunler/
│   │   ├── UrunKontrolcuTestleri.cs       (entegrasyon)
│   │   ├── UrunOlusturIsleyiciTestleri.cs (birim)
│   │   └── UrunOlusturDogrulayiciTestleri.cs
│   ├── Kimlik/
│   └── ...
├── Yardimcilar/
│   ├── DbFixture.cs                       (Testcontainers)
│   ├── WebAppFactory.cs
│   └── BogusVeriUretici.cs
├── Bilesenler/                            (bUnit Razor)
│   └── UrunKartTestleri.cs
└── E2E/                                   (Playwright)
    └── GirisAkisTestleri.cs
```

---

## 2. 🎯 MİNİMUM 5 TEST (HER ÖZELLİK)

### 2.1 Şablon: `UrunKontrolcuTestleri.cs`
```csharp
public class UrunKontrolcuTestleri : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _http;

    public UrunKontrolcuTestleri(WebAppFactory factory) =>
        _http = factory.CreateClient();

    // ───── 1. Başarılı Senaryo ─────
    [Fact]
    public async Task Olustur_GecerliVeri_BasariliDoner()
    {
        // Arrange
        var komut = new UrunOlusturKomutu("Test Ürün", "test-urun", 1, "Açıklama", 100m);

        // Act
        var cevap = await _http.PostAsJsonAsync("/api/urunler", komut);

        // Assert
        cevap.StatusCode.Should().Be(HttpStatusCode.OK);
        var sonuc = await cevap.Content.ReadFromJsonAsync<Cevap<UrunDto>>();
        sonuc!.BasariliMi.Should().BeTrue();
        sonuc.Veri!.Slug.Should().Be("test-urun");
    }

    // ───── 2. Boş / Geçersiz Veri → 400 ─────
    [Fact]
    public async Task Olustur_BosAd_DogrulamaHatasiDoner()
    {
        var komut = new UrunOlusturKomutu("", "test", 1, null, null);

        var cevap = await _http.PostAsJsonAsync("/api/urunler", komut);

        cevap.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var sonuc = await cevap.Content.ReadFromJsonAsync<Cevap<object>>();
        sonuc!.Hatalar.Should().Contain(h => h.Contains("zorunlu"));
    }

    // ───── 3. Yetki Kontrolü → 401/403 ─────
    [Fact]
    public async Task Olustur_YetkisizKullanici_403Doner()
    {
        _http.DefaultRequestHeaders.Clear();   // token yok

        var cevap = await _http.PostAsJsonAsync("/api/urunler",
            new UrunOlusturKomutu("X", "x", 1, null, null));

        cevap.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ───── 4. Edge Case (Negatif/Sınır) ─────
    [Fact]
    public async Task Detay_NegatifId_404Doner()
    {
        var cevap = await _http.GetAsync("/api/urunler/-1");
        cevap.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ───── 5. Geri Düşüş (Diğer Endpoint Bozulmadı) ─────
    [Fact]
    public async Task Liste_HalaCalisir()
    {
        var cevap = await _http.GetAsync("/api/urunler");
        cevap.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## 3. 🐳 TESTCONTAINERS — GERÇEK POSTGRESQL

### 3.1 DbFixture
```csharp
public class DbFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync() => await Container.StartAsync();
    public async Task DisposeAsync() => await Container.DisposeAsync();
}
```

### 3.2 WebAppFactory
```csharp
public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly DbFixture _dbFixture = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(servisler =>
        {
            // Gerçek DbContext'i kaldır
            servisler.RemoveAll(typeof(DbContextOptions<[PROJE_ADI]DbContext>));

            // Testcontainers PostgreSQL ekle
            servisler.AddDbContext<[PROJE_ADI]DbContext>(opt =>
                opt.UseNpgsql(_dbFixture.ConnectionString));

            // Mock servisler (email, AI, vb.)
            servisler.AddSingleton<IEpostaServisi, MockEpostaServisi>();
            servisler.AddSingleton<IAISaglayici, MockAISaglayici>();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        // Migration uygula + seed
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<[PROJE_ADI]DbContext>();
        await db.Database.MigrateAsync();
        await TestVerisiYukleAsync(db);
    }

    public new async Task DisposeAsync() => await _dbFixture.DisposeAsync();
}
```

### 3.3 In-Memory DB YASAK
```csharp
// ❌ YASAK — gerçek davranışı yansıtmaz
opt.UseInMemoryDatabase("test");

// ✅ DOĞRU — Testcontainers gerçek PostgreSQL
opt.UseNpgsql(_dbFixture.ConnectionString);
```

---

## 4. 🧱 BIRIM TEST (Handler)

```csharp
public class UrunOlusturIsleyiciTestleri
{
    [Fact]
    public async Task Handle_GecerliKomut_UrunOlusturulur()
    {
        // Arrange
        var opt = new DbContextOptionsBuilder<[PROJE_ADI]DbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        using var db = new [PROJE_ADI]DbContext(opt);
        db.Kategoriler.Add(new Kategori { Id = 1, Ad = "Test" });
        await db.SaveChangesAsync();

        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<UrunDto>(It.IsAny<Urun>()))
              .Returns(new UrunDto { Id = 1, Ad = "Test" });

        var isleyici = new UrunOlusturIsleyici(db, mapper.Object);

        // Act
        var sonuc = await isleyici.Handle(
            new UrunOlusturKomutu("Test", "test", 1, null, null),
            CancellationToken.None);

        // Assert
        sonuc.BasariliMi.Should().BeTrue();
        db.Urunler.Should().HaveCount(1);
    }
}
```
**Not:** Handler testlerinde **InMemory DB** OK — DB davranışı entegrasyon testinde Testcontainers ile.

---

## 5. ✅ FluentValidation Test

```csharp
public class UrunOlusturDogrulayiciTestleri
{
    private readonly UrunOlusturDogrulayici _dogrulayici;

    public UrunOlusturDogrulayiciTestleri()
    {
        var db = ... ;   // InMemory veya mock
        _dogrulayici = new UrunOlusturDogrulayici(db);
    }

    [Theory]
    [InlineData("", "Ad zorunlu")]
    [InlineData("a", "")]                              // geçerli
    [InlineData(null, "Ad zorunlu")]
    public async Task Ad_Dogrulama(string ad, string beklenenHata)
    {
        var sonuc = await _dogrulayici.ValidateAsync(
            new UrunOlusturKomutu(ad, "test", 1, null, null));

        if (string.IsNullOrEmpty(beklenenHata))
            sonuc.IsValid.Should().BeTrue();
        else
            sonuc.Errors.Should().Contain(e => e.ErrorMessage.Contains(beklenenHata));
    }
}
```

---

## 6. 🎨 bUnit (Blazor Bileşen)

```csharp
public class UrunKartTestleri : TestContext
{
    public UrunKartTestleri()
    {
        Services.AddSingleton<DilServisi>(new DilServisi());
    }

    [Fact]
    public void Render_UrunVerisiyle_AdGoster()
    {
        var urun = new UrunDto { Id = 1, Ad = "Test Ürün", Slug = "test" };

        var cut = RenderComponent<UrunKart>(p => p.Add(u => u.Urun, urun));

        cut.Find(".urun-ad").TextContent.Should().Be("Test Ürün");
    }

    [Fact]
    public void Click_SilButon_EventTetiklenir()
    {
        int tetiklenenId = 0;
        var cut = RenderComponent<UrunKart>(p => p
            .Add(u => u.Urun, new UrunDto { Id = 42 })
            .Add(u => u.OnSil, EventCallback.Factory.Create<int>(this, id => tetiklenenId = id)));

        cut.Find("button.sil").Click();

        tetiklenenId.Should().Be(42);
    }
}
```

---

## 7. 🎭 PLAYWRIGHT (E2E)

### 7.1 Kurulum
```bash
dotnet add package Microsoft.Playwright
playwright install
```

### 7.2 Test
```csharp
public class GirisAkisTestleri : PageTest
{
    [Test]
    public async Task GirisYap_GecerliKimlik_DashboardAcilir()
    {
        await Page.GotoAsync("https://localhost:5013/giris");

        await Page.FillAsync("input[name='eposta']", "admin@[PROJE_ADI].com");
        await Page.FillAsync("input[name='sifre']", "Sifre123!");
        await Page.ClickAsync("button[type='submit']");

        await Expect(Page).ToHaveURLAsync(new Regex(".*/yonetim/dashboard"));
        await Expect(Page.GetByText("Hoşgeldiniz")).ToBeVisibleAsync();
    }

    [Test]
    public async Task UrunEkle_GorulurListede()
    {
        await GirisYapAsync();
        await Page.GotoAsync("/yonetim/urunler/yeni");

        await Page.FillAsync("input[name='ad']", "E2E Test Ürün");
        await Page.FillAsync("input[name='slug']", "e2e-test");
        await Page.ClickAsync("button:has-text('Kaydet')");

        await Page.GotoAsync("/urunler");
        await Expect(Page.GetByText("E2E Test Ürün")).ToBeVisibleAsync();
    }
}
```

---

## 8. 📱 MAUI / WPF Test

### 8.1 MAUI UITest (Alpha)
```csharp
[TestFixture]
public class MobilGirisTestleri : MauiUITestBase
{
    [Test]
    public void AnaSayfa_AcilirAcilmaz_LogoGorunur()
    {
        var logo = App.WaitForElement("logo");
        logo.Should().NotBeNull();
    }
}
```

### 8.2 WPF UI (FlaUI)
```csharp
[Fact]
public void Pencere_Acilir()
{
    using var app = FlaUI.Core.Application.Launch("[PROJE_ADI].MasaUstu.exe");
    using var automation = new UIA3Automation();
    var pencere = app.GetMainWindow(automation);

    pencere.Title.Should().Be("[PROJE_ADI]");
}
```

---

## 9. ✅ PRE-COMMIT KONTROL LİSTESİ

`.git/hooks/pre-commit` (veya husky/lint-staged):
```bash
#!/bin/bash
set -e

echo "▸ Build..."
dotnet build --no-restore || exit 1

echo "▸ Test..."
dotnet test --no-build --verbosity minimal || exit 1

echo "▸ .env taranıyor..."
if git diff --cached --name-only | grep -E '\.env$'; then
  echo "❌ .env commit'te! İptal."
  exit 1
fi

echo "▸ Plaintext key taranıyor..."
if git diff --cached -G 'GizliAnahtar.*=.*"[^"]+"' | grep -q 'GizliAnahtar'; then
  echo "❌ Plaintext gizli anahtar tespit edildi."
  exit 1
fi

echo "▸ Hardcoded şifre taranıyor..."
if git diff --cached -G '(password|sifre|secret).*=\s*"[^"]+"' --pickaxe-regex | grep -i 'password\|secret'; then
  echo "⚠ Olası hardcoded sifre — manuel kontrol."
fi

echo "✓ Tüm kontroller geçti."
```

---

## 10. 💾 DB YEDEK PROTOKOLÜ

### 10.1 Manuel (Migration Öncesi)
```bash
# Tarih damgalı yedek
cp [PROJE_ADI].Api/[proje].db \
   "Yedekler/db/[proje]_$(date +%Y%m%d_%H%M)_$(git rev-parse --short HEAD).db"
```

### 10.2 Otomatik (Hangfire)
Detay: [05_Veritabani_EFCore10.md](05_Veritabani_EFCore10.md) §10

### 10.3 Production Yedek (Off-Site)
```bash
# Cron: günlük 02:00
pg_dump $DB_URL | gzip > /backups/[proje]-$(date +%Y%m%d).sql.gz
aws s3 cp /backups/[proje]-$(date +%Y%m%d).sql.gz s3://yedek-kova/
```

---

## 11. 🚀 GITHUB ACTIONS CI

`.github/workflows/ci.yml`:
```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  build-test:
    runs-on: ubuntu-latest

    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: test
        options: >-
          --health-cmd pg_isready --health-interval 10s
          --health-timeout 5s --health-retries 5
        ports: [5432:5432]

    steps:
      - uses: actions/checkout@v4

      - name: .NET 10 Kur
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test
        run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"

      - name: Coverage Rapor
        uses: codecov/codecov-action@v4


  playwright-e2e:
    runs-on: ubuntu-latest
    needs: build-test

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Playwright Kur
        run: |
          dotnet build [PROJE_ADI].Testler
          pwsh [PROJE_ADI].Testler/bin/Debug/net10.0/playwright.ps1 install

      - name: E2E Çalıştır
        run: dotnet test [PROJE_ADI].Testler --filter Category=E2E
```

---

## 12. 🚀 DEPLOY

### 12.1 Coolify Otomatik
1. GitHub repo'yu Coolify'a bağla
2. Branch: `main`
3. Build pack: nixpacks
4. Env var'lar: Coolify Secrets'tan
5. Git push → otomatik build + deploy + blue/green

---

## 13. 📊 KOD KALİTE GATE

### 13.1 SonarQube / SonarCloud
```yaml
- name: SonarQube Scan
  uses: SonarSource/sonarcloud-github-action@master
  env:
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
  with:
    args: >
      -Dsonar.projectKey=[PROJE_ADI]
      -Dsonar.organization=[ORG]
      -Dsonar.coverage.exclusions=**/Migrations/**
```

### 13.2 Hedefler
- **Code coverage:** %70+ (kritik mantık %90+)
- **Code smells:** A rating
- **Duplications:** < %3
- **Bugs:** 0 blocker / critical
- **Security hotspots:** 0

### 13.3 CodeQL (GitHub Native — Güvenlik)
```yaml
- name: CodeQL Init
  uses: github/codeql-action/init@v3
  with:
    languages: csharp

- name: Build
  run: dotnet build

- name: CodeQL Analyze
  uses: github/codeql-action/analyze@v3
```

---

## 14. ⚡ PERFORMANS / YÜK TESTİ (k6)

`yuk-test.js`:
```javascript
import http from 'k6/http';
import { sleep, check } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 100 },
    { duration: '1m', target: 500 },
    { duration: '30s', target: 1000 },
    { duration: '1m', target: 0 }
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],   // %95 < 500ms
    http_req_failed: ['rate<0.01']      // < %1 hata
  }
};

export default function () {
  const r = http.get('https://[PROJE_ADI].com/api/urunler');
  check(r, { 'status 200': (res) => res.status === 200 });
  sleep(1);
}
```

Çalıştır:
```bash
k6 run yuk-test.js
```

---

## 15. 🚨 SMOKE TEST (Deploy Sonrası)

`smoke.sh`:
```bash
#!/bin/bash
URL="https://[00_PROJE_BILGISI.url_birincil]"

# 1. Sağlık
curl -f "$URL/api/health" || { echo "❌ Health fail"; exit 1; }

# 2. Public liste
curl -f "$URL/api/urunler" || { echo "❌ List fail"; exit 1; }

# 3. Anasayfa
curl -f "$URL/" -o /dev/null || { echo "❌ Home fail"; exit 1; }

# 4. PWA manifest
curl -f "$URL/manifest.json" || { echo "❌ Manifest fail"; exit 1; }

# 5. Statik asset
curl -f "$URL/css/sistem/tokens.css" || { echo "❌ CSS fail"; exit 1; }

echo "✓ Smoke test geçti."
```

---

## 16. 📋 ÖZ-DENETİM (12 Madde)

```
[ ] 1. [PROJE_ADI].Testler projesi var, sln'de
[ ] 2. Her özellik için minimum 5 test
[ ] 3. Testcontainers gerçek PostgreSQL (in-memory YASAK entegrasyon'da)
[ ] 4. WebAppFactory ile entegrasyon testi
[ ] 5. bUnit ile Razor bileşen testi
[ ] 6. Playwright ile E2E (kritik akış: giriş, kaydet, liste)
[ ] 7. FluentAssertions kullanılıyor (.Should()...)
[ ] 8. Pre-commit hook (build + test + .env scan)
[ ] 9. DB yedek protokolü (manuel + Hangfire + production off-site)
[ ] 10. GitHub Actions CI (build + test + coverage)
[ ] 11. SonarQube veya CodeQL aktif (coverage > %70)
[ ] 12. Smoke test deploy sonrası otomatik çalışıyor
```

---

*Versiyon: 1.0 | Tarih: 2026-05-14 | Bağlı: tüm 02-09*
