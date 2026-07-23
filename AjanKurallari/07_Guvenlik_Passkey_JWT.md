---
name: guvenlik-passkey-jwt
description: Endüstriyel güvenlik — JWT Bearer + Refresh rotation, BCrypt, 2FA TOTP, Passkey (Blazor 10 yeni WebAuthn), [JsonIgnore] zorunlu, CORS dinamik, CSP+HSTS, rate limiting, API key DataProtection şifreli, append-only audit log HMAC zincir, PII filtre, OWASP ASVS.
status: TAMAM
---

# 🛡 GÜVENLİK — PASSKEY + JWT + BCrypt

> **Önkoşul:** [AGENTS.md](../AGENTS.md), `00_PROJE_BILGISI.guvenlik.*`, [06_API_Servisler_MediatR.md](06_API_Servisler_MediatR.md)
> **Standart:** OWASP ASVS Level 2

---

## 1. 🚫 SIFIR TOLERANS YASAKLAR

```
❌ Arka kapı (backdoor) şifresi — KOD İÇİNDE ASLA
❌ BCrypt hash / şifre / token log'a yazmak
❌ Hardcoded şifre API yanıtında
❌ [AllowAnonymous] DB sıfırlama / yedek geri yükleme endpoint'lerine
❌ BCrypt frontend'de (şifre client'ta hash'lenmez)
❌ JWT anahtarı appsettings.json içinde plaintext
❌ CORS AllowAnyOrigin() üretimde
❌ SifreHash / PinHash / TotpAnahtari [JsonIgnore]'suz API'den dönmek
❌ SignalR üretimde EnableDetailedErrors = true
❌ Refresh token rotate edilmeden uzun ömürlü
❌ API key DB'de düz metin
❌ Çıplak catch (Exception) — özellikle login'de bilgi sızdırır
```

---

## 2. 🔑 JWT BEARER + REFRESH ROTATION

### 2.1 Kurulum (Program.cs)
```csharp
var jwtAnahtar = builder.Configuration["JwtAyarlari:GizliAnahtar"]!;
var dakika = int.Parse(builder.Configuration["00_PROJE_BILGISI:guvenlik:jwt_gecerlilik_dakika"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        opt.SaveToken = false;   // memory'de tutma
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtAyarlari:Ihrac"],
            ValidAudience = builder.Configuration["JwtAyarlari:Hedef"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar)),
            ClockSkew = TimeSpan.Zero
        };
    });
```

### 2.2 JwtServisi (Wrapper)
```csharp
namespace [PROJE_ADI].Api.Moduller.Kimlik.Servisler;

public class JwtServisi(IConfiguration config)
{
    public string ErisimTokeniUret(Kullanici k)
    {
        var iddialar = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, k.Id.ToString()),
            new(ClaimTypes.Email, k.Eposta),
            new(ClaimTypes.Role, k.Rol.ToString())
        };

        var anahtar = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["JwtAyarlari:GizliAnahtar"]!));
        var imza = new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256);
        var dakika = int.Parse(config["00_PROJE_BILGISI:guvenlik:jwt_gecerlilik_dakika"]!);

        var token = new JwtSecurityToken(
            issuer: config["JwtAyarlari:Ihrac"],
            audience: config["JwtAyarlari:Hedef"],
            claims: iddialar,
            expires: DateTime.UtcNow.AddMinutes(dakika),
            signingCredentials: imza);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string YenilemeTokeniUret() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
```

### 2.3 Refresh Token Rotation
**Kural:** Her refresh isteğinde **yeni** refresh token üretilir, eskisi geçersiz olur.
```csharp
public async Task<Cevap<TokenDto>> YenileAsync(string eskiToken)
{
    var oturum = await _db.Oturumlar
        .FirstOrDefaultAsync(o => o.YenilemeToken == eskiToken && o.AktifMi);

    if (oturum is null || oturum.SonGecerlilik < DateTime.UtcNow)
        throw new YetkisizException("Geçersiz refresh token.");

    // Eski token'ı iptal et
    oturum.AktifMi = false;

    // Yeni token üret (rotation)
    var yeni = new OturumBilgisi
    {
        KullaniciId = oturum.KullaniciId,
        YenilemeToken = _jwt.YenilemeTokeniUret(),
        SonGecerlilik = DateTime.UtcNow.AddDays(7),
        AktifMi = true
    };
    _db.Oturumlar.Add(yeni);
    await _db.SaveChangesAsync();

    return Cevap<TokenDto>.Basarili(new TokenDto(
        _jwt.ErisimTokeniUret(oturum.Kullanici!),
        yeni.YenilemeToken));
}
```

---

## 3. 🔒 BCRYPT (BCrypt.Net-Next)

### 3.1 SifreServisi Wrapper
```csharp
public class SifreServisi(IConfiguration config)
{
    private readonly int _isYuku =
        int.Parse(config["00_PROJE_BILGISI:guvenlik:bcrypt_work_factor"] ?? "12");

    public string Hash(string sifre) =>
        BCrypt.Net.BCrypt.HashPassword(sifre, _isYuku);

    public bool Dogrula(string sifre, string hash) =>
        BCrypt.Net.BCrypt.Verify(sifre, hash);
}
```

### 3.2 İSTEMCİDE BCRYPT YASAK
```csharp
// ❌ YASAK: Blazor WASM içinde BCrypt
var hash = BCrypt.Net.BCrypt.HashPassword(sifre);   // bu hash UI'de bilinir = güvenlik açığı

// ✅ DOĞRU: Şifre HTTPS üzerinden ham gönder, sunucu hash'ler
await _http.PostAsJsonAsync("/api/kimlik/kayit", new { Sifre = sifre, ... });
```

### 3.3 Work Factor Önerisi
- **Dev:** 10 (hızlı test)
- **Production:** 12 (varsayılan)
- **Yüksek güvenlik:** 14 (yavaş ama güçlü)

---

## 4. 🔐 2FA — TOTP (OtpNet)

### 4.1 IkiAdimDogrulamaServisi
```csharp
using OtpNet;

public class IkiAdimDogrulamaServisi
{
    public string AnahtarUret() =>
        Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

    public string QrUrlUret(string anahtar, string eposta, string urunAdi)
    {
        // otpauth://totp/[urunAdi]:[eposta]?secret=...&issuer=[urunAdi]
        return $"otpauth://totp/{urunAdi}:{eposta}" +
               $"?secret={anahtar}&issuer={urunAdi}&algorithm=SHA1&digits=6&period=30";
    }

    public bool Dogrula(string anahtar, string kullaniciKodu)
    {
        var totp = new Totp(Base32Encoding.ToBytes(anahtar));
        return totp.VerifyTotp(kullaniciKodu, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}
```

### 4.2 Kayıt Akışı
1. Kullanıcı 2FA açar → `AnahtarUret()` → DB'ye `[JsonIgnore] TotpAnahtari` olarak kaydet
2. `QrUrlUret(...)` → QR kodu kullanıcıya göster (Google Authenticator tarar)
3. Kullanıcı ilk kodu girer → `Dogrula(...)` → başarılıysa `IkiAdimDogrulamaAktif = true`
4. Sonraki girişlerde TOTP kodu istenir

---

## 5. 🆔 PASSKEY (Blazor 10 YENİ — WebAuthn)

### 5.1 Niçin Passkey?
- Şifresiz giriş (parmak izi, yüz, donanım anahtarı)
- Phishing'e karşı dayanıklı
- Blazor 10 ile yerleşik destek

### 5.2 Kurulum
```csharp
// Program.cs
builder.Services.AddIdentity<Kullanici, IdentityRole<int>>()
    .AddEntityFrameworkStores<[PROJE_ADI]DbContext>()
    .AddPasskeySignInManager();

builder.Services.Configure<PasskeyOptions>(opt =>
{
    opt.ServerDomain = builder.Configuration["00_PROJE_BILGISI:url_birincil"]!;
    opt.ServerName = builder.Configuration["00_PROJE_BILGISI:firma_adi"]!;
    opt.UserVerification = UserVerificationRequirement.Preferred;
    opt.AuthenticatorAttachment = AuthenticatorAttachment.Platform;
});
```

### 5.3 Kayıt Akışı
```csharp
[HttpPost("passkey/kayit-baslat")]
[Authorize]
public async Task<Cevap<PasskeyOlusturmaSecenekleri>> KayitBaslat()
{
    var kullanici = await _userManager.GetUserAsync(User);
    var secenekler = await _passkeyHandler.OlusturmaSeceneklerUretAsync(kullanici!);
    return Cevap<PasskeyOlusturmaSecenekleri>.Basarili(secenekler);
}

[HttpPost("passkey/kayit-tamamla")]
[Authorize]
public async Task<Cevap<bool>> KayitTamamla(PasskeyKayitCevap cevap)
{
    var kullanici = await _userManager.GetUserAsync(User);
    var sonuc = await _passkeyHandler.KayitDogrulaAsync(kullanici!, cevap);
    return sonuc.Succeeded
        ? Cevap<bool>.Basarili(true, "Passkey kaydedildi.")
        : Cevap<bool>.Hata("Kayıt başarısız.");
}
```

### 5.4 Giriş Akışı
```csharp
[HttpPost("passkey/giris-baslat")]
public async Task<Cevap<PasskeyDogrulamaSecenekleri>> GirisBaslat(string eposta)
{
    var secenekler = await _passkeyHandler.DogrulamaSeceneklerUretAsync(eposta);
    return Cevap<PasskeyDogrulamaSecenekleri>.Basarili(secenekler);
}

[HttpPost("passkey/giris-tamamla")]
public async Task<Cevap<TokenDto>> GirisTamamla(PasskeyGirisCevap cevap)
{
    var kullanici = await _passkeyHandler.GirisDogrulaAsync(cevap);
    if (kullanici is null) return Cevap<TokenDto>.Hata("Doğrulama başarısız.");

    return Cevap<TokenDto>.Basarili(new TokenDto(
        _jwt.ErisimTokeniUret(kullanici),
        _jwt.YenilemeTokeniUret()));
}
```

### 5.5 İstemci (Blazor)
```csharp
// Passkey wrapper servisi
public class PasskeyServisi(IJSRuntime js)
{
    public async Task<string?> KayitYapAsync(PasskeyOlusturmaSecenekleri secenekler)
    {
        var modul = await js.InvokeAsync<IJSObjectReference>("import", "./js/passkey.js");
        return await modul.InvokeAsync<string?>("passkeyKayit", secenekler);
    }
}
```

---

## 6. 🔐 [JsonIgnore] ZORUNLU ALANLAR

| Alan | Niçin |
|---|---|
| `SifreHash` | BCrypt hash UI'de görünürse offline kırma açığı |
| `PinHash` | Kısa PIN — daha hassas |
| `DesenHash` | Mobil çizim deseni hash |
| `WebAuthnPublicKey` | Passkey public key |
| `SifreSifirlamaToken` | Email reset link içinde gelir |
| `TokenGecerlilikTarihi` | Token süresi |
| `TotpAnahtari` | 2FA secret |
| `EmailDogrulamaToken` | Doğrulama link |
| `ApiKeyEncrypted` | API key (zaten şifreli, yine de gizli) |
| Navigation property | Sonsuz JSON döngü |

```csharp
[System.Text.Json.Serialization.JsonIgnore]
public string SifreHash { get; set; } = string.Empty;
```

---

## 7. 🌍 CORS (Dinamik Whitelist)

```csharp
// Program.cs
var izinliDomainler = new[]
{
    $"https://{builder.Configuration["00_PROJE_BILGISI:url_birincil"]}",
    $"https://{builder.Configuration["00_PROJE_BILGISI:url_yedek"]}"
};

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Sıkı", politika =>
    {
        politika
            .WithOrigins(izinliDomainler)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()   // SignalR için
            .WithExposedHeaders("X-Correlation-Id");
    });

    if (builder.Environment.IsDevelopment())
    {
        opt.AddPolicy("Gevsek", p => p
            .WithOrigins("http://localhost:5013", "https://localhost:5013")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    }
});

app.UseCors(app.Environment.IsDevelopment() ? "Gevsek" : "Sıkı");
```

**❌ YASAK:** `AllowAnyOrigin()` üretimde — credentials ile birlikte tehlike.

---

## 8. 🛡 GÜVENLİK HEADER'LARI

### 8.1 GuvenlikHeaderlariMiddleware
```csharp
public class GuvenlikHeaderlariMiddleware(RequestDelegate sonraki)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var h = ctx.Response.Headers;

        h["X-Frame-Options"] = "DENY";
        h["X-Content-Type-Options"] = "nosniff";
        h["Referrer-Policy"] = "strict-origin-when-cross-origin";
        h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(self)";
        h["Cross-Origin-Opener-Policy"] = "same-origin";

        // CSP — sıkı
        h["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'wasm-unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline' fonts.googleapis.com; " +
            "font-src 'self' fonts.gstatic.com; " +
            "img-src 'self' data: blob: https:; " +
            "connect-src 'self' wss: https:; " +
            "frame-ancestors 'none';";

        await sonraki(ctx);
    }
}
```

### 8.2 HSTS (Üretim)
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();   // varsayılan 30 gün
    app.UseHttpsRedirection();
}
```

---

## 9. 🚦 RATE LIMITING

Detay: [06_API_Servisler_MediatR.md](06_API_Servisler_MediatR.md) §11.

**Önerilen politikalar:**
| Politika | Pencere | Limit |
|---|---|---|
| `genel` | 5 dk | 1000 |
| `giris` | 1 dk | 5 |
| `yazma` | 1 dk | 30 |
| `medya-yukle` | 1 dk | 10 |
| `ai-cagri` | 1 dk | 20 |

---

## 10. 🧹 INPUT VALIDATION

Detay: [06_API_Servisler_MediatR.md](06_API_Servisler_MediatR.md) §6.
- FluentValidation **sunucu tarafı zorunlu**
- Client-side sadece UI feedback (güvenlik kararı değil)

---

## 11. 🛡 XSS — HtmlSanitizer Wrapper

```csharp
public class IcerikTemizleyici
{
    private readonly HtmlSanitizer _sanitizer;

    public IcerikTemizleyici()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.UnionWith(["p", "strong", "em", "u", "a", "br", "ul", "ol", "li", "h1", "h2", "h3"]);
        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedSchemes.Add("https");
    }

    public string Temizle(string kirli) => _sanitizer.Sanitize(kirli);
}
```

Kullanım:
```csharp
urun.Aciklama = _temizleyici.Temizle(istek.Aciklama);
```

**Razor:** `@variable` otomatik escape — güvenli. `@((MarkupString)kirli)` **YASAK** sanitize edilmeden.

---

## 12. 📡 SIGNALR GÜVENLİK

```csharp
builder.Services.AddSignalR(opt =>
{
    opt.EnableDetailedErrors = builder.Environment.IsDevelopment();   // ❌ üretimde true
    opt.MaximumReceiveMessageSize = 32 * 1024;   // 32 KB
});

// Hub'da JWT zorunlu
[Authorize]
public class UrunHub : Hub { ... }
```

---

## 13. 🔑 API KEY DATAPROTECTION (Şifreli Saklama)

### 13.1 Entity
```csharp
public class AISaglayicisi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;

    [JsonIgnore]
    public string ApiKeyEncrypted { get; set; } = string.Empty;   // DB'de şifreli
}
```

### 13.2 Servis
```csharp
public class ApiKeyServisi(IDataProtectionProvider saglayici)
{
    private readonly IDataProtector _koruyucu = saglayici.CreateProtector("AISaglayicisi.ApiKey");

    public string Sifrele(string acikKey) => _koruyucu.Protect(acikKey);
    public string Coz(string sifreliKey) => _koruyucu.Unprotect(sifreliKey);
}
```

### 13.3 Production'da DataProtection Saklama
```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/var/keys"))
    .SetApplicationName("[PROJE_ADI]")
    .ProtectKeysWithCertificate(certificate);   // veya Azure Key Vault
```

---

## 14. 📜 AUDIT LOG (Append-Only + HMAC Zincir)

### 14.1 Entity
```csharp
public class AuditLog
{
    public long Id { get; set; }
    public DateTime ZamanDamgasi { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = string.Empty;

    public int? KullaniciId { get; set; }
    public int? FirmaId { get; set; }

    public string Eylem { get; set; } = string.Empty;       // "Urun.Olusturuldu"
    public string? EskiDeger { get; set; }                  // JSON
    public string? YeniDeger { get; set; }                  // JSON

    public string? IPAdresi { get; set; }
    public string? Tarayici { get; set; }

    public string ImzaHash { get; set; } = string.Empty;    // SHA256(prev_hash + kayit_data)
}
```

### 14.2 EF Interceptor (Otomatik Yazma)
```csharp
public class AuditInterceptor(IHttpContextAccessor hca) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData ev,
        InterceptionResult<int> sonuc,
        CancellationToken iptal = default)
    {
        var ctx = ev.Context!;
        var degisiklikler = ctx.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog);

        foreach (var d in degisiklikler)
        {
            var log = new AuditLog
            {
                Eylem = $"{d.Entity.GetType().Name}.{d.State}",
                EskiDeger = d.State == EntityState.Modified
                    ? JsonSerializer.Serialize(d.OriginalValues.ToObject()) : null,
                YeniDeger = d.State != EntityState.Deleted
                    ? JsonSerializer.Serialize(d.CurrentValues.ToObject()) : null,
                KullaniciId = ctx.UserId(hca),
                IPAdresi = hca.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                CorrelationId = hca.HttpContext?.Items["CorrelationId"]?.ToString() ?? ""
            };

            log.ImzaHash = await ImzaHesaplaAsync(ctx, log);
            ctx.Set<AuditLog>().Add(log);
        }

        return await base.SavingChangesAsync(ev, sonuc, iptal);
    }

    private static async Task<string> ImzaHesaplaAsync([PROJE_ADI]DbContext ctx, AuditLog log)
    {
        var oncekiHash = await ctx.AuditLoglar
            .OrderByDescending(a => a.Id)
            .Select(a => a.ImzaHash)
            .FirstOrDefaultAsync() ?? "GENESIS";

        var icerik = $"{oncekiHash}|{log.ZamanDamgasi:O}|{log.Eylem}|{log.YeniDeger}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(icerik)));
    }
}
```

### 14.3 Append-Only Garantisi
- DB seviyesinde: AuditLog tablosunda **DELETE/UPDATE yetkisi YOK** (uygulama kullanıcısına)
- Sadece INSERT yetkisi var
- HMAC zincir bütünlüğü doğrulama: bir kayıt değişirse sonraki hash'ler tutmaz

---

## 15. 🔐 GİZLİ BİLGİ YÖNETİMİ

### 15.1 Geliştirme
```bash
# user-secrets
dotnet user-secrets init
dotnet user-secrets set "JwtAyarlari:GizliAnahtar" "..."
dotnet user-secrets set "AISaglayicisi:OpenAI:ApiKey" "..."
```

### 15.2 Üretim
- **Coolify Secrets**
- `.env` dosyası `.gitignore`'da
- Kubernetes: `Secret` resource

### 15.3 YASAK
```
❌ appsettings.json içine şifre / token / key yazma (plaintext)
❌ .env commit et
❌ Slack / Discord / email ile düz metin gönder
❌ Log'a yaz (bkz §11.2 02_CSharp)
```

---

## 16. 👁 PII FİLTRESİ (Log Maskeleme)

### 16.1 Serilog Enricher
```csharp
public class PIIFiltreEnricher : ILogEventEnricher
{
    private static readonly Regex TC = new(@"\b\d{11}\b", RegexOptions.Compiled);
    private static readonly Regex Telefon = new(@"\b0\d{10}\b", RegexOptions.Compiled);
    private static readonly Regex Email = new(@"[\w\.-]+@[\w\.-]+", RegexOptions.Compiled);

    public void Enrich(LogEvent ev, ILogEventPropertyFactory pf)
    {
        foreach (var prop in ev.Properties.ToList())
        {
            if (prop.Value is ScalarValue sv && sv.Value is string s)
            {
                var maskeli = TC.Replace(s, "***TC***");
                maskeli = Telefon.Replace(maskeli, "***TEL***");
                maskeli = Email.Replace(maskeli, m =>
                    m.Value[0] + "***@" + m.Value.Split('@')[1]);

                ev.AddOrUpdateProperty(pf.CreateProperty(prop.Key, maskeli));
            }
        }
    }
}
```

### 16.2 Kayıt
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.With<PIIFiltreEnricher>()
    .WriteTo.Console()
    .CreateLogger();
```

---

## 17. 🚪 ENDPOINT GÜVENLİK SEVİYELERİ

### 17.1 [AllowAnonymous] Beyaz Liste (Sadece Bunlar!)
- `/api/kimlik/giris`
- `/api/kimlik/kayit`
- `/api/kimlik/sifre-sifirla-talep`
- `/api/kimlik/sifre-sifirla-onayla`
- `/api/kimlik/passkey/giris-baslat`
- `/api/iletisim/mesaj-gonder` (public form)
- `/api/bulten/abone-ol`
- `/api/urunler` (public liste — okuma)
- `/api/urunler/{slug}` (public detay)
- `/api/health` (sağlık kontrolü)

**Bunun dışında her şey `[Authorize]`.**

### 17.2 Rol Bazlı
```csharp
[Authorize]                       // Giriş yapan
[Authorize(Roles = "Admin")]      // Admin
[Authorize(Roles = "SuperAdmin")] // Süper admin
[Authorize(Policy = "AyniFirma")] // Firma izolasyonu (multi-tenant)
```

---

## 18. 📋 ÖZ-DENETİM (14 Madde)

```
[ ] 1. Backdoor şifresi / hardcoded kimlik YOK
[ ] 2. BCrypt sadece sunucu (frontend BCrypt YOK)
[ ] 3. [JsonIgnore] tüm gizli alanlarda
[ ] 4. JWT key user-secrets / env var (appsettings YOK)
[ ] 5. Refresh token rotation aktif
[ ] 6. CORS dinamik (00_PROJE_BILGISI'ten)
[ ] 7. AllowAnyOrigin() YOK (üretim)
[ ] 8. CSP + HSTS + güvenlik header'ları aktif
[ ] 9. Rate limiting endpoint başına
[ ] 10. FluentValidation sunucu tarafı zorunlu
[ ] 11. IcerikTemizleyici (XSS) kullanıcı HTML'inde
[ ] 12. SignalR EnableDetailedErrors = false (prod)
[ ] 13. API key DataProtection ile şifreli
[ ] 14. Audit log + HMAC zincir + append-only
```

---

*Versiyon: 1.0 | Tarih: 2026-05-14 | Standart: OWASP ASVS Level 2 | Bağlı: [06_API_Servisler_MediatR.md](06_API_Servisler_MediatR.md)*
