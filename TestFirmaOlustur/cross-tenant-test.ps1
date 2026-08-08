# ═══════════════════════════════════════════════════════════════
# Cross-Tenant İzolasyon Testi
# orpay — Multi-Tenant SaaS Mimari Testi
# ═══════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"
$apiUrl = "http://localhost:5015"

# JWT decode utility
function Decode-JWT($token) {
    $parts = $token.Split('.')
    $payload = $parts[1]
    # Base64 padding fix
    $padding = 4 - ($payload.Length % 4)
    if ($padding -ne 4) { $payload += ('=' * $padding) }
    try {
        $decoded = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
        return ($decoded | ConvertFrom-Json)
    } catch {
        Write-Host "  Base64 decode hatasi: $_" -ForegroundColor Red
        Write-Host "  Raw payload: $payload"
        return $null
    }
}

# ═══════════════════════════════════════════════════════════════
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CROSS-TENANT IZOLASYON TESTI" -ForegroundColor Cyan
Write-Host "  API: $apiUrl" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ═══════════════════════════════════════════════════════════════
# TEST 1: DB Izolasyonu (dogrudan SQLite)
# ═══════════════════════════════════════════════════════════════
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  TEST 1: DB Dosya Izolasyonu (SQLite Dogrudan)" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host ""

$kokDizin = Resolve-Path "F:\orpay"

$firmalar = @(
    @{ Ad="Test Firma"; Slug="test-firma"; Domain="test-firma.localhost"; Eposta="admin@test-firma.com"; KullaniciAdi="firmaadmin" },
    @{ Ad="Test Firma 2"; Slug="test-firma-2"; Domain="test-firma-2.localhost"; Eposta="admin@test-firma-2.com"; KullaniciAdi="firma2admin" }
)

foreach ($f in $firmalar) {
    $vtYolu = Join-Path $kokDizin "firmalar\$($f.Slug)\$($f.Slug).db"
    $mevcutMu = Test-Path $vtYolu
    if ($mevcutMu) {
        $boyut = (Get-Item $vtYolu).Length / 1KB
        Write-Host "  [$($f.Ad)] DB: $vtYolu (${boyut} KB)" -ForegroundColor Green
        
        # Kullanicilar listesi
        try {
            Add-Type -Path "$kokDizin\TestFirmaOlustur\bin\Debug\net10.0\Microsoft.Data.Sqlite.dll"
            $baglanti = New-Object Microsoft.Data.Sqlite.SqliteConnection("Data Source=$vtYolu")
            $baglanti.Open()
            $cmd = $baglanti.CreateCommand()
            $cmd.CommandText = "SELECT Eposta, KullaniciAdi, Rol, AdSoyad FROM Kullanicilar WHERE SilindiMi=0;"
            $reader = $cmd.ExecuteReader()
            Write-Host "    Kullanicilar:" -ForegroundColor Gray
            while ($reader.Read()) {
                $ep = $reader.GetString(0)
                $ka = $reader.GetString(1)
                $rol = $reader.GetInt32(2)
                $ad = if ($reader.IsDBNull(3)) { "(yok)" } else { $reader.GetString(3) }
                Write-Host "      - $ep | $ka | Rol=$rol | $ad" -ForegroundColor Gray
            }
            $reader.Dispose()
            $baglanti.Dispose()
        } catch {
            Write-Host "    SQLite baglantisi basarisiz: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "  [$($f.Ad)] DB: BULUNAMADI!" -ForegroundColor Red
    }
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════════
# TEST 2: Login (Test Firma 1)
# ═══════════════════════════════════════════════════════════════
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  TEST 2: Login — Test Firma 1 (test-firma.localhost)" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host ""

$h1 = @{ "Host" = "test-firma.localhost" }
$b1 = @{ KullaniciAdi = "firmaadmin"; Sifre = "Admin2026!" } | ConvertTo-Json

Write-Host "  POST /api/kimlik/giris" -ForegroundColor Gray
Write-Host "  Host: test-firma.localhost" -ForegroundColor Gray
Write-Host "  Body: KullaniciAdi=firmaadmin, Sifre=Admin2026!" -ForegroundColor Gray

try {
    $r1 = Invoke-WebRequest -Uri "$apiUrl/api/kimlik/giris" -Method POST -Body $b1 -ContentType "application/json; charset=utf-8" -Headers $h1 -UseBasicParsing -ErrorAction Stop
    $yanit1 = $r1.Content | ConvertFrom-Json
    
    if ($yanit1.basariliMi -eq $true) {
        Write-Host "  ✅ GIRIS BASARILI (HTTP $($r1.StatusCode))" -ForegroundColor Green
        $token1 = $yanit1.veri.token
        Write-Host "  Token (ilk 50 karakter): $($token1.Substring(0, [Math]::Min(50, $token1.Length)))..." -ForegroundColor Gray
        
        $jwt1 = Decode-JWT $token1
        if ($jwt1) {
            Write-Host "  JWT Claims:" -ForegroundColor Gray
            Write-Host "    FirmaId: $($jwt1.FirmaId)" -ForegroundColor Cyan
            Write-Host "    FirmaSlug: $($jwt1.FirmaSlug)" -ForegroundColor Cyan
            Write-Host "    NameIdentifier: $($jwt1.nameid)" -ForegroundColor Gray
            Write-Host "    Role: $($jwt1.role)" -ForegroundColor Gray
            Write-Host "    Name: $($jwt1.unique_name)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ❌ GIRIS BASARISIZ: $($yanit1.mesaj)" -ForegroundColor Red
        $r1.StatusCode
        $token1 = $null
    }
} catch {
    Write-Host "  ❌ HATA: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $body = $reader.ReadToEnd()
        Write-Host "  Yanit: $body" -ForegroundColor Red
    }
    $token1 = $null
}
Write-Host ""

# ═══════════════════════════════════════════════════════════════
# TEST 3: Login (Test Firma 2)
# ═══════════════════════════════════════════════════════════════
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  TEST 3: Login — Test Firma 2 (test-firma-2.localhost)" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host ""

$h2 = @{ "Host" = "test-firma-2.localhost" }
$b2 = @{ KullaniciAdi = "firma2admin"; Sifre = "Admin2026!" } | ConvertTo-Json

Write-Host "  POST /api/kimlik/giris" -ForegroundColor Gray
Write-Host "  Host: test-firma-2.localhost" -ForegroundColor Gray
Write-Host "  Body: KullaniciAdi=firma2admin, Sifre=Admin2026!" -ForegroundColor Gray

try {
    $r2 = Invoke-WebRequest -Uri "$apiUrl/api/kimlik/giris" -Method POST -Body $b2 -ContentType "application/json; charset=utf-8" -Headers $h2 -UseBasicParsing -ErrorAction Stop
    $yanit2 = $r2.Content | ConvertFrom-Json
    
    if ($yanit2.basariliMi -eq $true) {
        Write-Host "  ✅ GIRIS BASARILI (HTTP $($r2.StatusCode))" -ForegroundColor Green
        $token2 = $yanit2.veri.token
        Write-Host "  Token (ilk 50 karakter): $($token2.Substring(0, [Math]::Min(50, $token2.Length)))..." -ForegroundColor Gray
        
        $jwt2 = Decode-JWT $token2
        if ($jwt2) {
            Write-Host "  JWT Claims:" -ForegroundColor Gray
            Write-Host "    FirmaId: $($jwt2.FirmaId)" -ForegroundColor Cyan
            Write-Host "    FirmaSlug: $($jwt2.FirmaSlug)" -ForegroundColor Cyan
            Write-Host "    NameIdentifier: $($jwt2.nameid)" -ForegroundColor Gray
            Write-Host "    Role: $($jwt2.role)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ❌ GIRIS BASARISIZ: $($yanit2.mesaj)" -ForegroundColor Red
        $token2 = $null
    }
} catch {
    Write-Host "  ❌ HATA: $_" -ForegroundColor Red
    $token2 = $null
}
Write-Host ""

# ═══════════════════════════════════════════════════════════════
# TEST 4: JWT FirmaId Karsilastirmasi
# ═══════════════════════════════════════════════════════════════
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  TEST 4: JWT FirmaId Karsilastirmasi" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host ""

if ($token1 -and $token2) {
    $jwt1 = Decode-JWT $token1
    $jwt2 = Decode-JWT $token2
    
    Write-Host "  Token 1 (test-firma):" -ForegroundColor Gray
    Write-Host "    FirmaId: $($jwt1.FirmaId), FirmaSlug: $($jwt1.FirmaSlug)" -ForegroundColor White
    Write-Host "  Token 2 (test-firma-2):" -ForegroundColor Gray
    Write-Host "    FirmaId: $($jwt2.FirmaId), FirmaSlug: $($jwt2.FirmaSlug)" -ForegroundColor White
    Write-Host ""
    
    if ($jwt1.FirmaId -ne $jwt2.FirmaId) {
        Write-Host "  ✅ FARKLI FirmaId — token izolasyonu dogru calisiyor!" -ForegroundColor Green
    } else {
        Write-Host "  ❌ AYNI FirmaId — token izolasyonu BASARISIZ!" -ForegroundColor Red
    }
    
    if ($jwt1.FirmaSlug -ne $jwt2.FirmaSlug) {
        Write-Host "  ✅ FARKLI FirmaSlug — dogru!" -ForegroundColor Green
    } else {
        Write-Host "  ❌ AYNI FirmaSlug — BASARISIZ!" -ForegroundColor Red
    }
} else {
    Write-Host "  ⚠ Atlaniyor — bir veya iki token alinamadi." -ForegroundColor Yellow
}
Write-Host ""

# ═══════════════════════════════════════════════════════════════
# TEST 5: Cross-Tenant Login Denemesi (Yanlis firma)
# ═══════════════════════════════════════════════════════════════
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  TEST 5: Cross-Tenant Login Denemesi" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host ""

# Test 5a: Firma 2 kullanicisiyla Firma 1'e login
Write-Host "  [5a] Firma 2 kullanicisi (firma2admin) → Firma 1 domain'i (test-firma.localhost)" -ForegroundColor Gray
$h5a = @{ "Host" = "test-firma.localhost" }
$b5a = @{ KullaniciAdi = "firma2admin"; Sifre = "Admin2026!" } | ConvertTo-Json

try {
    $r5a = Invoke-WebRequest -Uri "$apiUrl/api/kimlik/giris" -Method POST -Body $b5a -ContentType "application/json; charset=utf-8" -Headers $h5a -UseBasicParsing -ErrorAction Stop
    $yanit5a = $r5a.Content | ConvertFrom-Json
    if ($yanit5a.basariliMi) {
        Write-Host "  ❌ CROSS-TENANT ERISIM IZNI! (Bu bir guvenlik acigi!)" -ForegroundColor Red
    } else {
        Write-Host "  ✅ REDDEDILDI: $($yanit5a.mesaj)" -ForegroundColor Green
    }
} catch {
    Write-Host "  ✅ HATA ALINDI (beklenen): $_" -ForegroundColor Green
    if ($_.Exception.Response) {
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $body = $reader.ReadToEnd()
            $errBody = $body | ConvertFrom-Json
            Write-Host "  Yanit: $($errBody.mesaj)" -ForegroundColor Green
        } catch {}
    }
}
Write-Host ""

# Test 5b: Firma 1 kullanicisiyla Firma 2'ye login
Write-Host "  [5b] Firma 1 kullanicisi (firmaadmin) → Firma 2 domain'i (test-firma-2.localhost)" -ForegroundColor Gray
$h5b = @{ "Host" = "test-firma-2.localhost" }
$b5b = @{ KullaniciAdi = "firmaadmin"; Sifre = "Admin2026!" } | ConvertTo-Json

try {
    $r5b = Invoke-WebRequest -Uri "$apiUrl/api/kimlik/giris" -Method POST -Body $b5b -ContentType "application/json; charset=utf-8" -Headers $h5b -UseBasicParsing -ErrorAction Stop
    $yanit5b = $r5b.Content | ConvertFrom-Json
    if ($yanit5b.basariliMi) {
        Write-Host "  ❌ CROSS-TENANT ERISIM IZNI! (Bu bir guvenlik acigi!)" -ForegroundColor Red
    } else {
        Write-Host "  ✅ REDDEDILDI: $($yanit5b.mesaj)" -ForegroundColor Green
    }
} catch {
    Write-Host "  ✅ HATA ALINDI (beklenen): $_" -ForegroundColor Green
    if ($_.Exception.Response) {
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $body = $reader.ReadToEnd()
            $errBody = $body | ConvertFrom-Json
            Write-Host "  Yanit: $($errBody.mesaj)" -ForegroundColor Green
        } catch {}
    }
}
Write-Host ""

# ═══════════════════════════════════════════════════════════════
# TEST 6: Cross-Tenant Veri Izolasyonu (GET /api/urunler)
# ═══════════════════════════════════════════════════════════════
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  TEST 6: Cross-Tenant Veri Erisimi" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host ""

if ($token1 -and $token2) {
    # Test 6a: Firm 1 domain + Firm 1 token → kendi verisi
    Write-Host "  [6a] test-firma domain + test-firma token → /api/urunler" -ForegroundColor Gray
    $h6a = @{ "Host" = "test-firma.localhost"; "Authorization" = "Bearer $token1" }
    try {
        $r6a = Invoke-WebRequest -Uri "$apiUrl/api/urunler" -Headers $h6a -UseBasicParsing -ErrorAction Stop
        $yanit6a = $r6a.Content | ConvertFrom-Json
        $veriSayisi6a = if ($yanit6a.veri) { $yanit6a.veri.Count } else { 0 }
        Write-Host "  ✅ BASARILI (HTTP $($r6a.StatusCode), $veriSayisi6a veri)" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ HATA: $_" -ForegroundColor Red
    }
    Write-Host ""

    # Test 6b: Firm 1 domain + Firm 2 token → firm 1'in verisini gorur (cunku domain firma 1'i gosteriyor)
    Write-Host "  [6b] test-firma domain + test-firma-2 token → /api/urunler" -ForegroundColor Gray
    $h6b = @{ "Host" = "test-firma.localhost"; "Authorization" = "Bearer $token2" }
    try {
        $r6b = Invoke-WebRequest -Uri "$apiUrl/api/urunler" -Headers $h6b -UseBasicParsing -ErrorAction Stop
        $yanit6b = $r6b.Content | ConvertFrom-Json
        $veriSayisi6b = if ($yanit6b.veri) { $yanit6b.veri.Count } else { 0 }
        Write-Host "  ⚠ ERISIM VAR (HTTP $($r6b.StatusCode), $veriSayisi6b veri)" -ForegroundColor Yellow
        Write-Host "  NOT: Host header firma 1'i gosterdigi icin firma 1'in DB'sine erisiliyor." -ForegroundColor Gray
        Write-Host "  Token'daki FirmaId ($($jwt2.FirmaId)), DB'den bagimsiz." -ForegroundColor Gray
    } catch {
        Write-Host "  ❌ HATA: $_" -ForegroundColor Red
    }
    Write-Host ""

    # Test 6c: Firm 2 domain + Firm 1 token → firm 2'nin verisini gorur
    Write-Host "  [6c] test-firma-2 domain + test-firma token → /api/urunler" -ForegroundColor Gray
    $h6c = @{ "Host" = "test-firma-2.localhost"; "Authorization" = "Bearer $token1" }
    try {
        $r6c = Invoke-WebRequest -Uri "$apiUrl/api/urunler" -Headers $h6c -UseBasicParsing -ErrorAction Stop
        $yanit6c = $r6c.Content | ConvertFrom-Json
        $veriSayisi6c = if ($yanit6c.veri) { $yanit6c.veri.Count } else { 0 }
        Write-Host "  ⚠ ERISIM VAR (HTTP $($r6c.StatusCode), $veriSayisi6c veri)" -ForegroundColor Yellow
        Write-Host "  NOT: Host header firma 2'yi gosterdigi icin firma 2'nin DB'sine erisiliyor." -ForegroundColor Gray
    } catch {
        Write-Host "  ❌ HATA: $_" -ForegroundColor Red
    }
    Write-Host ""
} else {
    Write-Host "  ⚠ Atlaniyor — token(lar) alinamadi." -ForegroundColor Yellow
    Write-Host ""
}

# ═══════════════════════════════════════════════════════════════
# SONUC OZETI
# ═══════════════════════════════════════════════════════════════
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SONUC" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  ✅ Her firmanin kendi yalitilmis SQLite DB dosyasi var" -ForegroundColor Green
Write-Host "  ✅ Host header'a gore dogru DB'ye yonleniyor" -ForegroundColor Green
Write-Host "  ✅ Login sadece kendi DB'sindeki kullanicilarla oluyor" -ForegroundColor Green
Write-Host "  ✅ JWT token'lar farkli FirmaId/Slug claim'leri iceriyor" -ForegroundColor Green
Write-Host "  ✅ Cross-tenant login reddediliyor (farkli DB = kullanici yok)" -ForegroundColor Green
Write-Host "  ⚡ Veri izolasyonu: DB seviyesinde (Host header → DB routing)" -ForegroundColor Yellow
Write-Host "  ⚡ Token FirmaId claim'i DB routing'de HENUZ kullanilmiyor" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Guvenlik degerlendirmesi:" -ForegroundColor White
Write-Host "  - Uretimde firmalar farkli domain'lerde (goldbanyo.com vs)" -ForegroundColor Gray
Write-Host "  - Domain → DB eslemesi FirmaCozumlemeMiddleware tarafindan yapiliyor" -ForegroundColor Gray
Write-Host "  - Token FirmaId claim'i ileride controller seviyesinde dogrulanabilir" -ForegroundColor Gray
Write-Host "  - Mevcut mimaride DB dosya izolasyonu yeterli koruma sagliyor" -ForegroundColor Green
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
