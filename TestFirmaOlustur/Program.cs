using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using System.Net.Sockets;
using System.Text;

// ═══════════════════════════════════════════════════════════════
// Test Firması Oluşturma Aracı
// Kullanim:
//   dotnet run                  → Firma olustur (varsayilan)
//   dotnet run -- test-db       → Test 1: DB tablolarini listele
//   dotnet run -- test-api      → Test 2: API login test
//   dotnet run -- test-all      → Her iki testi calistir
// ═══════════════════════════════════════════════════════════════

// Test modu kontrolü
if (args.Length > 0)
{
    switch (args[0])
    {
        case "test-db":
            await Test1_DbTablolariniListele();
            return 0;
        case "test-api":
            await Test2_ApiLoginTest();
            return 0;
        case "test-all":
            await Test1_DbTablolariniListele();
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine();
            await Test2_ApiLoginTest();
            return 0;
        case "test-firma2":
            await Firma2Olustur();
            return 0;
    }
}

var kokDizin = FindSolutionRoot();
var superAdminVtYolu = Path.Combine(kokDizin, "VizitLink3D.SuperAdmin", "superadmin.db");
var firmaSlug = "test-firma";
var firmaKlasoru = Path.Combine(kokDizin, "firmalar", firmaSlug);
var firmaVtYolu = Path.Combine(firmaKlasoru, $"{firmaSlug}.db");
var medyaKlasoru = Path.Combine(firmaKlasoru, "medya");
var i18nKlasoru = Path.Combine(firmaKlasoru, "i18n");

Console.WriteLine($"🔧 Test Firma Oluşturma Aracı");
Console.WriteLine($"   Çözüm kökü: {kokDizin}");
Console.WriteLine($"   SuperAdmin DB: {superAdminVtYolu}");
Console.WriteLine($"   Firma klasörü: {firmaKlasoru}");
Console.WriteLine($"   Firma DB: {firmaVtYolu}");
Console.WriteLine();

// ──────────────────────────────────────────────
// ADIM 1: superadmin.db mevcut mu kontrol et
// ──────────────────────────────────────────────
if (!File.Exists(superAdminVtYolu))
{
    Console.WriteLine($"❌ HATA: superadmin.db bulunamadı: {superAdminVtYolu}");
    return 1;
}
Console.WriteLine("✅ superadmin.db bulundu.");

// ──────────────────────────────────────────────
// ADIM 2: Firma zaten var mı kontrol et
// ──────────────────────────────────────────────
int firmaId = 0;
using (var baglanti = new SqliteConnection($"Data Source={superAdminVtYolu}"))
{
    await baglanti.OpenAsync();

    // Firma zaten var mı? (Idempotent: varsa ID'yi al, yoksa INSERT)
    using var kontrolCmd = baglanti.CreateCommand();
    kontrolCmd.CommandText = "SELECT Id FROM Firmalar WHERE Slug = @slug;";
    kontrolCmd.Parameters.AddWithValue("@slug", firmaSlug);
    var mevcutIdObj = await kontrolCmd.ExecuteScalarAsync();

    if (mevcutIdObj is not null)
    {
        firmaId = Convert.ToInt32(mevcutIdObj);
        Console.WriteLine($"ℹ Firma zaten mevcut (ID: {firmaId}), SuperAdmin DB'ye tekrar INSERT yapılmadı.");
    }
    else
    {
        Console.WriteLine();
        Console.WriteLine("📝 SuperAdmin DB'ye firma kaydı ekleniyor...");

        using var firmaCmd = baglanti.CreateCommand();
        firmaCmd.CommandText = @"
            INSERT INTO Firmalar (
                Ad, Unvan, Slug, AciklamaKisa, Aciklama,
                Domain, YedekDomain, Logo, Favicon, Eposta,
                Telefon1, Telefon2, Whatsapp, Adres, Sehir, Ilce, PostaKodu, Ulke,
                Enlem, Boylam, CalismaSaatleri, KurulusYili,
                Twitter, Facebook, Instagram, YoutubeKanal, Pinterest, LinkedIn, TiktokKanal,
                TasarimRengi1, TasarimRengi2, TasarimRengi3, AdminTema, SiteTema,
                MenuYatayAralik, MenuDikeyPadding, LogoMaxYukseklik,
                YetkiliAdSoyad, VergiNo, VergiDairesi,
                AktifMi, DemoMu, AktifSablonId,
                AktifModulKodlariJson, Sektor, MedyaKlasoru, PaketTipi, MaxKullaniciSayisi,
                OlusturulmaTarihi, GuncellenmeTarihi
            ) VALUES (
                @ad, @unvan, @slug, @aciklamaKisa, @aciklama,
                @domain, @yedekDomain, @logo, @favicon, @eposta,
                @telefon1, @telefon2, @whatsapp, @adres, @sehir, @ilce, @postaKodu, @ulke,
                @enlem, @boylam, @calismaSaatleri, @kurulusYili,
                @twitter, @facebook, @instagram, @youtube, @pinterest, @linkedin, @tiktok,
                @tasarimRengi1, @tasarimRengi2, @tasarimRengi3, @adminTema, @siteTema,
                @menuYatayAralik, @menuDikeyPadding, @logoMaxYukseklik,
                @yetkiliAdSoyad, @vergiNo, @vergiDairesi,
                @aktifMi, @demoMu, @aktifSablonId,
                @aktifModulKodlariJson, @sektor, @medyaKlasoru, @paketTipi, @maxKullaniciSayisi,
                @olusturulmaTarihi, @guncellenmeTarihi
            );
            SELECT last_insert_rowid();";

        firmaCmd.Parameters.AddWithValue("@ad", "Test Firma");
        firmaCmd.Parameters.AddWithValue("@unvan", "Test Firma Ltd. Şti.");
        firmaCmd.Parameters.AddWithValue("@slug", firmaSlug);
        firmaCmd.Parameters.AddWithValue("@aciklamaKisa", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@aciklama", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@domain", "test-firma.localhost");
        firmaCmd.Parameters.AddWithValue("@yedekDomain", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@logo", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@favicon", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@eposta", "admin@test-firma.com");
        firmaCmd.Parameters.AddWithValue("@telefon1", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@telefon2", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@whatsapp", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@adres", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@sehir", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@ilce", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@postaKodu", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@ulke", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@enlem", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@boylam", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@calismaSaatleri", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@kurulusYili", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@twitter", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@facebook", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@instagram", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@youtube", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@pinterest", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@linkedin", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@tiktok", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@tasarimRengi1", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@tasarimRengi2", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@tasarimRengi3", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@adminTema", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@siteTema", "modern");
        firmaCmd.Parameters.AddWithValue("@menuYatayAralik", 30);
        firmaCmd.Parameters.AddWithValue("@menuDikeyPadding", 20);
        firmaCmd.Parameters.AddWithValue("@logoMaxYukseklik", 60);
        firmaCmd.Parameters.AddWithValue("@yetkiliAdSoyad", "Test Yetkili");
        firmaCmd.Parameters.AddWithValue("@vergiNo", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@vergiDairesi", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@aktifMi", 1L);
        firmaCmd.Parameters.AddWithValue("@demoMu", 1L);
        firmaCmd.Parameters.AddWithValue("@aktifSablonId", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@aktifModulKodlariJson", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@sektor", "Test");
        firmaCmd.Parameters.AddWithValue("@medyaKlasoru", DBNull.Value);
        firmaCmd.Parameters.AddWithValue("@paketTipi", "Standart");
        firmaCmd.Parameters.AddWithValue("@maxKullaniciSayisi", 5);
        firmaCmd.Parameters.AddWithValue("@olusturulmaTarihi", DateTime.UtcNow.ToString("o"));
        firmaCmd.Parameters.AddWithValue("@guncellenmeTarihi", DBNull.Value);

        firmaId = Convert.ToInt32(await firmaCmd.ExecuteScalarAsync());
        Console.WriteLine($"✅ Firma eklendi. ID: {firmaId}");
    }

    // ──────────────────────────────────────────────
    // ADIM 4: Varsayılan modülleri ata
    // ──────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine("📝 Varsayılan modüller atanıyor...");

    // Varsayılan modül ID'leri (SuperAdminDbContext HasData'dan)
    var varsayilanModulIdleri = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 22, 23 };

    using var modulKontrolCmd = baglanti.CreateCommand();
    modulKontrolCmd.CommandText = "SELECT COUNT(*) FROM FirmaModulAtamalari WHERE FirmaId = @firmaId;";
    modulKontrolCmd.Parameters.AddWithValue("@firmaId", firmaId);
    var mevcutAtamaSayisi = Convert.ToInt64(await modulKontrolCmd.ExecuteScalarAsync());

    if (mevcutAtamaSayisi > 0)
    {
        Console.WriteLine($"⚠ {mevcutAtamaSayisi} modül ataması zaten mevcut. Atlanıyor.");
    }
    else
    {
        using var atamaCmd = baglanti.CreateCommand();
        atamaCmd.CommandText = @"
            INSERT INTO FirmaModulAtamalari (FirmaId, ModulId, AtanmaTarihi)
            VALUES (@firmaId, @modulId, @tarih);";

        var tarihParam = atamaCmd.Parameters.Add("@tarih", SqliteType.Text);
        var firmaParam = atamaCmd.Parameters.Add("@firmaId", SqliteType.Integer);
        var modulParam = atamaCmd.Parameters.Add("@modulId", SqliteType.Integer);

        firmaParam.Value = firmaId;
        tarihParam.Value = DateTimeOffset.UtcNow.ToString("o");

        int eklenen = 0;
        foreach (var modulId in varsayilanModulIdleri)
        {
            modulParam.Value = modulId;
            await atamaCmd.ExecuteNonQueryAsync();
            eklenen++;
        }

        Console.WriteLine($"✅ {eklenen} varsayılan modül ataması eklendi.");
    }
}

// ──────────────────────────────────────────────
// ADIM 5: Firma klasörlerini oluştur
// ──────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("📁 Firma klasör yapısı oluşturuluyor...");

Directory.CreateDirectory(firmaKlasoru);
Directory.CreateDirectory(medyaKlasoru);
Directory.CreateDirectory(i18nKlasoru);

Console.WriteLine($"✅ Klasörler hazır:");
Console.WriteLine($"   {firmaKlasoru}");
Console.WriteLine($"   {medyaKlasoru}");
Console.WriteLine($"   {i18nKlasoru}");

// ──────────────────────────────────────────────
// ADIM 6: EnsureCreated() ile firma DB şemasını oluştur
// ──────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("🗄️  Firma veritabanı şeması oluşturuluyor (EnsureCreated)...");

try
{
    if (File.Exists(firmaVtYolu))
    {
        Console.WriteLine($"⚠ {firmaVtYolu} zaten mevcut. Siliniyor...");
        File.Delete(firmaVtYolu);
    }

    var secenekler = new DbContextOptionsBuilder<VizitLink3DDbContext>();
    secenekler.UseSqlite($"Data Source={firmaVtYolu}");

    // FirmaOlusturmaServisi'ndeki gibi null KiraciServisi geç
    // OnConfiguring'de IsConfigured=true olduğu için fallback çalışmaz
    await using var context = new VizitLink3DDbContext(secenekler.Options, null!);
    await context.Database.EnsureCreatedAsync();

    Console.WriteLine($"✅ Firma veritabanı şeması oluşturuldu: {firmaVtYolu}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ EnsureCreated başarısız: {ex.Message}");
    Console.WriteLine($"   Detay: {ex.InnerException?.Message}");
    Console.WriteLine("   Ham SQL ile devam ediliyor...");
    // EnsureCreated başarısız olursa, SQL ile en azından Kullanicilar tablosunu oluştur
    await KullanicilarTablosunuHamSqlIleOlustur(firmaVtYolu);
}

// ──────────────────────────────────────────────
// ADIM 6.5: Firma DB'ye Firma kaydı ekle (FK için gerekli)
// ──────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("📝 Firma DB'ye Firma kaydı ekleniyor (FK constraint)...");

using (var fkVtBaglantisi = new SqliteConnection($"Data Source={firmaVtYolu}"))
{
    await fkVtBaglantisi.OpenAsync();

    using var fkKontrolCmd = fkVtBaglantisi.CreateCommand();
    fkKontrolCmd.CommandText = "SELECT COUNT(*) FROM Firmalar WHERE Id = @id;";
    fkKontrolCmd.Parameters.AddWithValue("@id", firmaId);
    var fkMevcut = Convert.ToInt64(await fkKontrolCmd.ExecuteScalarAsync());

    if (fkMevcut > 0)
    {
        Console.WriteLine($"   ℹ Firma DB'de Firma kaydı zaten mevcut (ID: {firmaId}).");
    }
    else
    {
        // FK constraint'i devre disi birak, INSERT yap, tekrar aktif et
        using var pragmaCmd = fkVtBaglantisi.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = OFF;";
        await pragmaCmd.ExecuteNonQueryAsync();

        using var fkEkleCmd = fkVtBaglantisi.CreateCommand();
        fkEkleCmd.CommandText = @"
            INSERT INTO Firmalar (Id, Ad, Unvan, Slug, Domain, AktifMi, DemoMu, SiteTema, 
                MenuYatayAralik, MenuDikeyPadding, LogoMaxYukseklik, PaketTipi, MaxKullaniciSayisi, 
                OlusturulmaTarihi, Eposta)
            VALUES (@id, @ad, @unvan, @slug, @domain, 1, 1, 'modern', 30, 20, 60, 'Standart', 5, 
                @tarih, @eposta);";
        fkEkleCmd.Parameters.AddWithValue("@id", firmaId);
        fkEkleCmd.Parameters.AddWithValue("@ad", "Test Firma");
        fkEkleCmd.Parameters.AddWithValue("@unvan", "Test Firma Ltd. Şti.");
        fkEkleCmd.Parameters.AddWithValue("@slug", "test-firma");
        fkEkleCmd.Parameters.AddWithValue("@domain", "test-firma.localhost");
        fkEkleCmd.Parameters.AddWithValue("@tarih", DateTimeOffset.UtcNow.ToString("o"));
        fkEkleCmd.Parameters.AddWithValue("@eposta", "admin@test-firma.com");
        await fkEkleCmd.ExecuteNonQueryAsync();

        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
        await pragmaCmd.ExecuteNonQueryAsync();

        Console.WriteLine($"   ✅ Firma DB'ye Firma kaydı eklendi (ID: {firmaId}).");
    }
}

// ──────────────────────────────────────────────
// ADIM 7: FirmaAdmin kullanıcısı ekle
// ──────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("👤 FirmaAdmin kullanıcısı ekleniyor...");

var sifreHash = BCrypt.Net.BCrypt.HashPassword("Admin2026!");
Console.WriteLine($"   BCrypt hash üretildi: {sifreHash[..20]}...");

using (var firmaBaglantisi = new SqliteConnection($"Data Source={firmaVtYolu}"))
{
    await firmaBaglantisi.OpenAsync();

    // FirmaAdmin kullanıcısı zaten var mı? (Seed data SuperAdmin ekler, biz FirmaAdmin istiyoruz)
    using var sayacCmd = firmaBaglantisi.CreateCommand();
    sayacCmd.CommandText = "SELECT COUNT(*) FROM Kullanicilar WHERE Rol = @rol;";
    sayacCmd.Parameters.AddWithValue("@rol", (int)Rol.FirmaAdmin);
    var mevcutFirmaAdminSayisi = Convert.ToInt64(await sayacCmd.ExecuteScalarAsync());

    if (mevcutFirmaAdminSayisi > 0)
    {
        Console.WriteLine($"⚠ {mevcutFirmaAdminSayisi} FirmaAdmin kullanıcısı zaten mevcut.");
        // FirmaId null/sifir ise guncelle (geriye donuk duzeltme)
        using var guncelleCmd = firmaBaglantisi.CreateCommand();
        guncelleCmd.CommandText = "UPDATE Kullanicilar SET FirmaId = @firmaId WHERE (FirmaId IS NULL OR FirmaId = 0) AND Rol = @rol;";
        guncelleCmd.Parameters.AddWithValue("@firmaId", firmaId);
        guncelleCmd.Parameters.AddWithValue("@rol", (int)Rol.FirmaAdmin);
        var guncellenen = await guncelleCmd.ExecuteNonQueryAsync();
        if (guncellenen > 0)
            Console.WriteLine($"   ✅ {guncellenen} kullanıcıya FirmaId={firmaId} eklendi.");
    }
    else
    {
        using var kullaniciCmd = firmaBaglantisi.CreateCommand();
        kullaniciCmd.CommandText = @"
            INSERT INTO Kullanicilar
                (FirmaId, Eposta, SifreHash, AdSoyad, KullaniciAdi, Rol, EmailDogrulandiMi, 
                 IkiAdimDogrulamaAktif, TelefonDogrulandiMi, AktifMi, 
                 KilitlendiMi, BasarisizGirisDenemesi, SilindiMi,
                 TercihEdilenDil, OlusturulmaTarihi)
            VALUES
                (@firmaId, @eposta, @sifre, @adSoyad, @kullaniciAdi, @rol, @emailDogrulandiMi, 
                 @ikiAdim, @telefonDog, @aktifMi, 
                 @kilit, @deneme, @silindi,
                 @dil, @tarih);";
        kullaniciCmd.Parameters.AddWithValue("@firmaId", firmaId);
        kullaniciCmd.Parameters.AddWithValue("@eposta", "admin@test-firma.com");
        kullaniciCmd.Parameters.AddWithValue("@sifre", sifreHash);
        kullaniciCmd.Parameters.AddWithValue("@adSoyad", "Firma Admin");
        kullaniciCmd.Parameters.AddWithValue("@kullaniciAdi", "firmaadmin");
        kullaniciCmd.Parameters.AddWithValue("@rol", (int)Rol.FirmaAdmin);
        kullaniciCmd.Parameters.AddWithValue("@emailDogrulandiMi", 1L);
        kullaniciCmd.Parameters.AddWithValue("@ikiAdim", 0L);
        kullaniciCmd.Parameters.AddWithValue("@telefonDog", 0L);
        kullaniciCmd.Parameters.AddWithValue("@aktifMi", 1L);
        kullaniciCmd.Parameters.AddWithValue("@kilit", 0L);
        kullaniciCmd.Parameters.AddWithValue("@deneme", 0L);
        kullaniciCmd.Parameters.AddWithValue("@silindi", 0L);
        kullaniciCmd.Parameters.AddWithValue("@dil", "tr");
        kullaniciCmd.Parameters.AddWithValue("@tarih", DateTime.UtcNow.ToString("o"));
        await kullaniciCmd.ExecuteNonQueryAsync();

        Console.WriteLine("✅ FirmaAdmin kullanıcısı eklendi.");
        Console.WriteLine($"   FirmaId: {firmaId}");
        Console.WriteLine($"   Eposta: admin@test-firma.com");
        Console.WriteLine($"   Şifre: Admin2026!");
        Console.WriteLine($"   Rol: FirmaAdmin (2)");
    }
}

// ──────────────────────────────────────────────
// ADIM 8: Dil dosyalarını oluştur
// ──────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("🌐 Dil dosyaları oluşturuluyor...");

var trJson = @"{
  ""anasayfa"": ""Anasayfa"",
  ""hakkimizda"": ""Hakkımızda"",
  ""urunler"": ""Ürünler"",
  ""galeri"": ""Galeri"",
  ""iletisim"": ""İletişim"",
  ""blog"": ""Blog"",
  ""site_basligi"": ""Test Firma"",
  ""site_aciklamasi"": ""Test Firma — SaaS Demo""
}";

var enJson = @"{
  ""anasayfa"": ""Home"",
  ""hakkimizda"": ""About Us"",
  ""urunler"": ""Products"",
  ""galeri"": ""Gallery"",
  ""iletisim"": ""Contact"",
  ""blog"": ""Blog"",
  ""site_basligi"": ""Test Company"",
  ""site_aciklamasi"": ""Test Company — SaaS Demo""
}";

foreach (var (dil, icerik) in new[] { ("tr", trJson), ("en", enJson) })
{
    var yol = Path.Combine(i18nKlasoru, $"{dil}.json");
    if (File.Exists(yol))
    {
        Console.WriteLine($"⚠ {yol} zaten mevcut. Atlanıyor.");
    }
    else
    {
        await File.WriteAllTextAsync(yol, icerik, new System.Text.UTF8Encoding(false));
        Console.WriteLine($"✅ {yol} oluşturuldu.");
    }
}

// ──────────────────────────────────────────────
// ÖZET
// ──────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine("🎉 TEST FİRMASI BAŞARIYLA OLUŞTURULDU!");
Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine($"   Firma Adı:       Test Firma");
Console.WriteLine($"   Slug:            test-firma");
Console.WriteLine($"   Domain:          test-firma.localhost");
Console.WriteLine($"   Admin Eposta:    admin@test-firma.com");
Console.WriteLine($"   Admin Şifre:     Admin2026!");
Console.WriteLine($"   SuperAdmin DB:   {superAdminVtYolu}");
Console.WriteLine($"   Firma DB:        {firmaVtYolu}");
Console.WriteLine($"   Medya Klasörü:   {medyaKlasoru}");
Console.WriteLine($"   i18n Klasörü:    {i18nKlasoru}");
Console.WriteLine($"   Paket:           Standart (5 kullanıcı)");
Console.WriteLine($"   Modüller:        15 varsayılan modül atandı");
Console.WriteLine("═══════════════════════════════════════════════");

return 0;

// ──────────────────────────────────────────────
// Yardımcı metotlar
// ──────────────────────────────────────────────

static string FindSolutionRoot()
{
    // Derleme çıktı dizininden (bin/Debug/netXX) yukarı çıkarak aranacak işaretler
    string[] kokIsaretleri = { "VizitLink3D.SuperAdmin", "VizitLink3D.Api", "AGENTS.md" };

    // 1) AppContext.BaseDirectory'den yukarı doğru ara
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        foreach (var isaret in kokIsaretleri)
            if (Directory.Exists(Path.Combine(dir.FullName, isaret)) || 
                File.Exists(Path.Combine(dir.FullName, isaret)))
                return dir.FullName;
        dir = dir.Parent;
    }
    
    // 2) Environment variable
    var envPath = Environment.GetEnvironmentVariable("ORPAY_ROOT");
    if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        return envPath;
    
    // 3) Çalışma dizininden yukarı doğru ara
    dir = new DirectoryInfo(Environment.CurrentDirectory);
    while (dir != null)
    {
        foreach (var isaret in kokIsaretleri)
            if (Directory.Exists(Path.Combine(dir.FullName, isaret)) || 
                File.Exists(Path.Combine(dir.FullName, isaret)))
                return dir.FullName;
        dir = dir.Parent;
    }
    
    // 4) Hardcoded fallback
    if (Directory.Exists(@"F:\orpay\VizitLink3D.SuperAdmin"))
        return @"F:\orpay";
    
    throw new InvalidOperationException(
        "Çözüm kök dizini bulunamadı. Lütfen ORPAY_ROOT environment variable'ı ayarlayın.");
}

static async Task KullanicilarTablosunuHamSqlIleOlustur(string vtYolu)
{
    using var baglanti = new SqliteConnection($"Data Source={vtYolu}");
    await baglanti.OpenAsync();

    using var cmd = baglanti.CreateCommand();
    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Kullanicilar (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FirmaId INTEGER,
            AdSoyad TEXT NOT NULL DEFAULT '',
            Eposta TEXT NOT NULL DEFAULT '',
            Telefon TEXT,
            KullaniciAdi TEXT NOT NULL DEFAULT '',
            SifreHash TEXT NOT NULL DEFAULT '',
            PinHash TEXT,
            DesenHash TEXT,
            WebAuthnPublicKey TEXT,
            SifreSifirlamaToken TEXT,
            TokenGecerlilikTarihi TEXT,
            EmailDogrulamaToken TEXT,
            TotpAnahtari TEXT,
            RefreshToken TEXT,
            RefreshTokenBitisTarihi TEXT,
            Rol INTEGER NOT NULL DEFAULT 0,
            EmailDogrulandiMi INTEGER NOT NULL DEFAULT 0,
            IkiAdimDogrulamaAktif INTEGER NOT NULL DEFAULT 0,
            TelefonDogrulandiMi INTEGER NOT NULL DEFAULT 0,
            AktifMi INTEGER NOT NULL DEFAULT 1,
            KilitlendiMi INTEGER NOT NULL DEFAULT 0,
            BasarisizGirisDenemesi INTEGER NOT NULL DEFAULT 0,
            KilitAcmaTarihi TEXT,
            SonGirisTarihi TEXT,
            SonGirisIP TEXT,
            ProfilResmiUrl TEXT,
            TercihEdilenDil TEXT DEFAULT 'tr',
            OlusturulmaTarihi TEXT NOT NULL,
            GuncellenmeTarihi TEXT,
            OlusturanKullaniciId INTEGER,
            SilindiMi INTEGER NOT NULL DEFAULT 0,
            SilinmeTarihi TEXT
        );";
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("✅ Kullanicilar tablosu ham SQL ile oluşturuldu.");
}

// ═══════════════════════════════════════════════════════════════
// TEST 1: Firma DB tablolarını listele
// ═══════════════════════════════════════════════════════════════
static async Task Test1_DbTablolariniListele()
{
    var kokDizin = FindSolutionRoot();
    var firmaVtYolu = Path.Combine(kokDizin, "firmalar", "test-firma", "test-firma.db");

    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine("📊 TEST 1: Firma DB Tablo Listesi");
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine($"   DB Yolu: {firmaVtYolu}");
    Console.WriteLine();

    using var baglanti = new SqliteConnection($"Data Source={firmaVtYolu}");
    await baglanti.OpenAsync();

    // Tablolari listele
    using var tabloCmd = baglanti.CreateCommand();
    tabloCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
    using var reader = await tabloCmd.ExecuteReaderAsync();

    var tablolar = new List<string>();
    while (await reader.ReadAsync())
        tablolar.Add(reader.GetString(0));
    await reader.DisposeAsync();

    Console.WriteLine($"📋 Toplam {tablolar.Count} tablo:");
    Console.WriteLine(new string('─', 50));
    foreach (var t in tablolar)
        Console.WriteLine($"   • {t}");
    Console.WriteLine();

    // Kullanici sayisi
    using var sayacCmd = baglanti.CreateCommand();
    sayacCmd.CommandText = "SELECT COUNT(*) FROM Kullanicilar;";
    var kullaniciSayisi = Convert.ToInt64(await sayacCmd.ExecuteScalarAsync());
    Console.WriteLine($"👤 Kullanicilar tablosunda {kullaniciSayisi} kayıt:");
    Console.WriteLine(new string('─', 50));

    // Kullanici listesi
    using var kullaniciCmd = baglanti.CreateCommand();
    kullaniciCmd.CommandText = "SELECT Eposta, Rol, AdSoyad FROM Kullanicilar;";
    using var kReader = await kullaniciCmd.ExecuteReaderAsync();
    while (await kReader.ReadAsync())
    {
        var eposta = kReader.GetString(0);
        var rolInt = kReader.GetInt32(1);
        var rolAdi = Enum.IsDefined(typeof(Rol), rolInt) ? ((Rol)rolInt).ToString() : $"Bilinmeyen({rolInt})";
        var adSoyad = kReader.IsDBNull(2) ? "(yok)" : kReader.GetString(2);
        Console.WriteLine($"   • {eposta} | Rol: {rolAdi} | Ad: {adSoyad}");
    }
    await kReader.DisposeAsync();

    Console.WriteLine();
    Console.WriteLine("✅ Test 1 tamamlandı.");
}

// ═══════════════════════════════════════════════════════════════
// TEST 2: API login endpoint'ini test et
// ═══════════════════════════════════════════════════════════════
static async Task Test2_ApiLoginTest()
{
    var kokDizin = FindSolutionRoot();
    var apiVtYolu = Path.Combine(kokDizin, "VizitLink3D.Api", "vizitlink3d.db");
    
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine("🔐 TEST 2: API Login Testi");
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine();

    // ── Adım 2a: vizitlink3d.db'de Firmalar tablosuna test-firma ekle ──
    Console.WriteLine("📝 Adım 1: vizitlink3d.db'de Firma kaydı kontrol ediliyor...");
    Console.WriteLine($"   API DB: {apiVtYolu}");
    
    if (!File.Exists(apiVtYolu))
    {
        Console.WriteLine("❌ HATA: vizitlink3d.db bulunamadı! API çalışıyor mu?");
        Console.WriteLine($"   Beklenen yol: {apiVtYolu}");
        return;
    }

    int firmaId;
    using (var baglanti = new SqliteConnection($"Data Source={apiVtYolu}"))
    {
        await baglanti.OpenAsync();

        // Firmalar tablosu var mi?
        using var kontrolCmd = baglanti.CreateCommand();
        kontrolCmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Firmalar';";
        var firmaTablosuVar = Convert.ToInt64(await kontrolCmd.ExecuteScalarAsync()) > 0;

        if (!firmaTablosuVar)
        {
            Console.WriteLine("❌ HATA: Firmalar tablosu vizitlink3d.db'de yok!");
            return;
        }

        // test-firma zaten var mi?
        using var mevcutCmd = baglanti.CreateCommand();
        mevcutCmd.CommandText = "SELECT Id, Domain FROM Firmalar WHERE Slug = 'test-firma';";
        using var mReader = await mevcutCmd.ExecuteReaderAsync();
        if (await mReader.ReadAsync())
        {
            firmaId = mReader.GetInt32(0);
            var domain = mReader.IsDBNull(1) ? "(null)" : mReader.GetString(1);
            Console.WriteLine($"   ℹ test-firma zaten mevcut (ID: {firmaId}, Domain: {domain})");
        }
        else
        {
            Console.WriteLine("   📝 test-firma kaydı ekleniyor...");
            using var ekleCmd = baglanti.CreateCommand();
            ekleCmd.CommandText = @"
                INSERT INTO Firmalar (Ad, Unvan, Slug, Domain, AktifMi, DemoMu, SiteTema, MenuYatayAralik, MenuDikeyPadding, LogoMaxYukseklik, PaketTipi, MaxKullaniciSayisi, OlusturulmaTarihi, Eposta)
                VALUES ('Test Firma', 'Test Firma Ltd. Sti.', 'test-firma', 'test-firma.localhost', 1, 1, 'modern', 30, 20, 60, 'Standart', 5, @tarih, 'admin@test-firma.com');
                SELECT last_insert_rowid();";
            ekleCmd.Parameters.AddWithValue("@tarih", DateTimeOffset.UtcNow.ToString("o"));
            firmaId = Convert.ToInt32(await ekleCmd.ExecuteScalarAsync());
            Console.WriteLine($"   ✅ test-firma eklendi. ID: {firmaId}, Domain: test-firma.localhost");
        }
    }

    Console.WriteLine();

    // ── Adım 2b: API'ye login isteği gönder ──
    Console.WriteLine("📡 Adım 2: API'ye login isteği gönderiliyor...");
    Console.WriteLine("   Hedef: http://localhost:5015/api/kimlik/giris");
    Console.WriteLine("   Host header: test-firma.localhost");
    Console.WriteLine();

    try
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync("localhost", 5015);

        var stream = tcp.GetStream();
        
        var govde = """{"KullaniciAdi":"firmaadmin","Sifre":"Admin2026!"}""";
        var istek = $"""
            POST /api/kimlik/giris HTTP/1.1
            Host: test-firma.localhost
            Content-Type: application/json
            Content-Length: {Encoding.UTF8.GetByteCount(govde)}
            Connection: close

            {govde}
            """.ReplaceLineEndings("\r\n");

        var istekBytes = Encoding.UTF8.GetBytes(istek);
        await stream.WriteAsync(istekBytes);
        await stream.FlushAsync();

        // Yanıtı oku
        using var okuyucu = new StreamReader(stream, Encoding.UTF8);
        var yanit = await okuyucu.ReadToEndAsync();

        Console.WriteLine("📨 API Yanıtı:");
        Console.WriteLine(new string('─', 70));
        
        // Baslik ve govdeyi ayir
        var bosSatirIndex = yanit.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (bosSatirIndex > 0)
        {
            var basliklar = yanit[..bosSatirIndex];
            var govdeYanit = yanit[(bosSatirIndex + 4)..];
            
            // Status line
            var ilkSatirSonu = basliklar.IndexOf('\r');
            if (ilkSatirSonu > 0)
            {
                var durumSatiri = basliklar[..ilkSatirSonu];
                Console.WriteLine($"   Durum: {durumSatiri}");
                
                if (durumSatiri.Contains("200"))
                    Console.WriteLine("   ✅ BAŞARILI: Giriş yapıldı!");
                else if (durumSatiri.Contains("401"))
                    Console.WriteLine("   ⚠ BAŞARISIZ: Kimlik doğrulama hatası (401)");
                else if (durumSatiri.Contains("400"))
                    Console.WriteLine("   ⚠ BAŞARISIZ: Geçersiz istek (400)");
                else
                    Console.WriteLine($"   ⚠ Beklenmeyen durum kodu");
            }
            
            // Govde (ilk 500 karakter)
            Console.WriteLine();
            Console.WriteLine("   Yanıt gövdesi:");
            var govdeOzet = govdeYanit.Length > 500 ? govdeYanit[..500] + "..." : govdeYanit;
            Console.WriteLine($"   {govdeOzet}");
        }
        else
        {
            Console.WriteLine("   (Ham yanıt, ayrıştırılamadı)");
            Console.WriteLine($"   {yanit[..Math.Min(500, yanit.Length)]}");
        }
        
        Console.WriteLine(new string('─', 70));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Bağlantı hatası: {ex.Message}");
        Console.WriteLine("   API çalışıyor mu? (localhost:5015)");
    }

    Console.WriteLine();
    Console.WriteLine("✅ Test 2 tamamlandı.");
}

// ═══════════════════════════════════════════════════════════════
// TEST 3: İkinci test firması oluştur (test-firma-2)
// ═══════════════════════════════════════════════════════════════
static async Task Firma2Olustur()
{
    var kokDizin = FindSolutionRoot();
    var superAdminVtYolu = Path.Combine(kokDizin, "VizitLink3D.SuperAdmin", "superadmin.db");
    var apiVtYolu = Path.Combine(kokDizin, "VizitLink3D.Api", "vizitlink3d.db");
    var firmaSlug = "test-firma-2";
    var firmaKlasoru = Path.Combine(kokDizin, "firmalar", firmaSlug);
    var firmaVtYolu = Path.Combine(firmaKlasoru, $"{firmaSlug}.db");
    var medyaKlasoru = Path.Combine(firmaKlasoru, "medya");
    var i18nKlasoru = Path.Combine(firmaKlasoru, "i18n");

    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine("🏗️  TEST 3: İkinci Test Firması Oluşturma");
    Console.WriteLine($"   Firma Slug:     {firmaSlug}");
    Console.WriteLine($"   Firma Adı:      Test Firma 2");
    Console.WriteLine($"   Domain:         test-firma-2.localhost");
    Console.WriteLine($"   Admin Eposta:   admin@test-firma-2.com");
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine();

    // ── ADIM 1: superadmin.db kontrol ──
    if (!File.Exists(superAdminVtYolu))
    {
        Console.WriteLine($"❌ HATA: superadmin.db bulunamadı: {superAdminVtYolu}");
        return;
    }
    Console.WriteLine("✅ superadmin.db bulundu.");

    // ── ADIM 2: SuperAdmin DB'ye firma kaydı ekle ──
    int firmaId;
    using (var baglanti = new SqliteConnection($"Data Source={superAdminVtYolu}"))
    {
        await baglanti.OpenAsync();

        using var kontrolCmd = baglanti.CreateCommand();
        kontrolCmd.CommandText = "SELECT Id FROM Firmalar WHERE Slug = @slug;";
        kontrolCmd.Parameters.AddWithValue("@slug", firmaSlug);
        var mevcutIdObj = await kontrolCmd.ExecuteScalarAsync();

        if (mevcutIdObj is not null)
        {
            firmaId = Convert.ToInt32(mevcutIdObj);
            Console.WriteLine($"ℹ Firma zaten mevcut (ID: {firmaId}), SuperAdmin DB'ye tekrar INSERT yapılmadı.");
        }
        else
        {
            Console.WriteLine("📝 SuperAdmin DB'ye firma kaydı ekleniyor...");

            using var firmaCmd = baglanti.CreateCommand();
            firmaCmd.CommandText = @"
                INSERT INTO Firmalar (
                    Ad, Unvan, Slug, AciklamaKisa, Aciklama,
                    Domain, YedekDomain, Logo, Favicon, Eposta,
                    Telefon1, Telefon2, Whatsapp, Adres, Sehir, Ilce, PostaKodu, Ulke,
                    Enlem, Boylam, CalismaSaatleri, KurulusYili,
                    Twitter, Facebook, Instagram, YoutubeKanal, Pinterest, LinkedIn, TiktokKanal,
                    TasarimRengi1, TasarimRengi2, TasarimRengi3, AdminTema, SiteTema,
                    MenuYatayAralik, MenuDikeyPadding, LogoMaxYukseklik,
                    YetkiliAdSoyad, VergiNo, VergiDairesi,
                    AktifMi, DemoMu, AktifSablonId,
                    AktifModulKodlariJson, Sektor, MedyaKlasoru, PaketTipi, MaxKullaniciSayisi,
                    OlusturulmaTarihi, GuncellenmeTarihi
                ) VALUES (
                    @ad, @unvan, @slug, @aciklamaKisa, @aciklama,
                    @domain, @yedekDomain, @logo, @favicon, @eposta,
                    @telefon1, @telefon2, @whatsapp, @adres, @sehir, @ilce, @postaKodu, @ulke,
                    @enlem, @boylam, @calismaSaatleri, @kurulusYili,
                    @twitter, @facebook, @instagram, @youtube, @pinterest, @linkedin, @tiktok,
                    @tasarimRengi1, @tasarimRengi2, @tasarimRengi3, @adminTema, @siteTema,
                    @menuYatayAralik, @menuDikeyPadding, @logoMaxYukseklik,
                    @yetkiliAdSoyad, @vergiNo, @vergiDairesi,
                    @aktifMi, @demoMu, @aktifSablonId,
                    @aktifModulKodlariJson, @sektor, @medyaKlasoru, @paketTipi, @maxKullaniciSayisi,
                    @olusturulmaTarihi, @guncellenmeTarihi
                );
                SELECT last_insert_rowid();";

            firmaCmd.Parameters.AddWithValue("@ad", "Test Firma 2");
            firmaCmd.Parameters.AddWithValue("@unvan", "Test Firma 2 Ltd. Şti.");
            firmaCmd.Parameters.AddWithValue("@slug", firmaSlug);
            firmaCmd.Parameters.AddWithValue("@aciklamaKisa", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@aciklama", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@domain", "test-firma-2.localhost");
            firmaCmd.Parameters.AddWithValue("@yedekDomain", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@logo", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@favicon", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@eposta", "admin@test-firma-2.com");
            firmaCmd.Parameters.AddWithValue("@telefon1", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@telefon2", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@whatsapp", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@adres", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@sehir", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@ilce", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@postaKodu", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@ulke", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@enlem", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@boylam", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@calismaSaatleri", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@kurulusYili", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@twitter", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@facebook", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@instagram", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@youtube", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@pinterest", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@linkedin", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@tiktok", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@tasarimRengi1", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@tasarimRengi2", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@tasarimRengi3", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@adminTema", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@siteTema", "modern");
            firmaCmd.Parameters.AddWithValue("@menuYatayAralik", 30);
            firmaCmd.Parameters.AddWithValue("@menuDikeyPadding", 20);
            firmaCmd.Parameters.AddWithValue("@logoMaxYukseklik", 60);
            firmaCmd.Parameters.AddWithValue("@yetkiliAdSoyad", "Test Yetkili 2");
            firmaCmd.Parameters.AddWithValue("@vergiNo", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@vergiDairesi", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@aktifMi", 1L);
            firmaCmd.Parameters.AddWithValue("@demoMu", 1L);
            firmaCmd.Parameters.AddWithValue("@aktifSablonId", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@aktifModulKodlariJson", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@sektor", "Test");
            firmaCmd.Parameters.AddWithValue("@medyaKlasoru", DBNull.Value);
            firmaCmd.Parameters.AddWithValue("@paketTipi", "Standart");
            firmaCmd.Parameters.AddWithValue("@maxKullaniciSayisi", 5);
            firmaCmd.Parameters.AddWithValue("@olusturulmaTarihi", DateTime.UtcNow.ToString("o"));
            firmaCmd.Parameters.AddWithValue("@guncellenmeTarihi", DBNull.Value);

            firmaId = Convert.ToInt32(await firmaCmd.ExecuteScalarAsync());
            Console.WriteLine($"✅ Firma eklendi. ID: {firmaId}");
        }

        // ── ADIM 3: Varsayılan modüller ata ──
        Console.WriteLine();
        Console.WriteLine("📝 Varsayılan modüller atanıyor...");

        var varsayilanModulIdleri = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 22, 23 };

        using var modulKontrolCmd = baglanti.CreateCommand();
        modulKontrolCmd.CommandText = "SELECT COUNT(*) FROM FirmaModulAtamalari WHERE FirmaId = @firmaId;";
        modulKontrolCmd.Parameters.AddWithValue("@firmaId", firmaId);
        var mevcutAtamaSayisi = Convert.ToInt64(await modulKontrolCmd.ExecuteScalarAsync());

        if (mevcutAtamaSayisi > 0)
        {
            Console.WriteLine($"⚠ {mevcutAtamaSayisi} modül ataması zaten mevcut. Atlanıyor.");
        }
        else
        {
            using var atamaCmd = baglanti.CreateCommand();
            atamaCmd.CommandText = @"
                INSERT INTO FirmaModulAtamalari (FirmaId, ModulId, AtanmaTarihi)
                VALUES (@firmaId, @modulId, @tarih);";

            var tarihParam = atamaCmd.Parameters.Add("@tarih", SqliteType.Text);
            var firmaParam = atamaCmd.Parameters.Add("@firmaId", SqliteType.Integer);
            var modulParam = atamaCmd.Parameters.Add("@modulId", SqliteType.Integer);

            firmaParam.Value = firmaId;
            tarihParam.Value = DateTimeOffset.UtcNow.ToString("o");

            int eklenen = 0;
            foreach (var modulId in varsayilanModulIdleri)
            {
                modulParam.Value = modulId;
                await atamaCmd.ExecuteNonQueryAsync();
                eklenen++;
            }

            Console.WriteLine($"✅ {eklenen} varsayılan modül ataması eklendi.");
        }
    }

    // ── ADIM 4: vizitlink3d.db'ye firma ekle ──
    Console.WriteLine();
    Console.WriteLine("📝 API DB (vizitlink3d.db)'ye firma kaydı ekleniyor...");
    
    if (!File.Exists(apiVtYolu))
    {
        Console.WriteLine($"❌ HATA: vizitlink3d.db bulunamadı: {apiVtYolu}");
        Console.WriteLine("   API çalışıyor olmalı! Önce API'yi başlatın.");
        return;
    }

    using (var apiBaglantisi = new SqliteConnection($"Data Source={apiVtYolu}"))
    {
        await apiBaglantisi.OpenAsync();

        using var mevcutCmd = apiBaglantisi.CreateCommand();
        mevcutCmd.CommandText = "SELECT Id, Domain FROM Firmalar WHERE Slug = @slug;";
        mevcutCmd.Parameters.AddWithValue("@slug", firmaSlug);
        using var mReader = await mevcutCmd.ExecuteReaderAsync();
        if (await mReader.ReadAsync())
        {
            var mevcutId = mReader.GetInt32(0);
            var domain = mReader.IsDBNull(1) ? "(null)" : mReader.GetString(1);
            Console.WriteLine($"   ℹ {firmaSlug} zaten mevcut (ID: {mevcutId}, Domain: {domain})");
        }
        else
        {
            mReader.Dispose();
            using var ekleCmd = apiBaglantisi.CreateCommand();
            ekleCmd.CommandText = @"
                INSERT INTO Firmalar (Ad, Unvan, Slug, Domain, AktifMi, DemoMu, SiteTema, MenuYatayAralik, MenuDikeyPadding, LogoMaxYukseklik, PaketTipi, MaxKullaniciSayisi, OlusturulmaTarihi, Eposta)
                VALUES (@ad, @unvan, @slug, @domain, 1, 1, 'modern', 30, 20, 60, 'Standart', 5, @tarih, @eposta);
                SELECT last_insert_rowid();";
            ekleCmd.Parameters.AddWithValue("@ad", "Test Firma 2");
            ekleCmd.Parameters.AddWithValue("@unvan", "Test Firma 2 Ltd. Şti.");
            ekleCmd.Parameters.AddWithValue("@slug", firmaSlug);
            ekleCmd.Parameters.AddWithValue("@domain", "test-firma-2.localhost");
            ekleCmd.Parameters.AddWithValue("@tarih", DateTimeOffset.UtcNow.ToString("o"));
            ekleCmd.Parameters.AddWithValue("@eposta", "admin@test-firma-2.com");
            var yeniFirmaId = Convert.ToInt32(await ekleCmd.ExecuteScalarAsync());
            Console.WriteLine($"   ✅ {firmaSlug} eklendi. ID: {yeniFirmaId}, Domain: test-firma-2.localhost");
        }
    }

    // ── ADIM 5: Firma klasörlerini oluştur ──
    Console.WriteLine();
    Console.WriteLine("📁 Firma klasör yapısı oluşturuluyor...");

    Directory.CreateDirectory(firmaKlasoru);
    Directory.CreateDirectory(medyaKlasoru);
    Directory.CreateDirectory(i18nKlasoru);

    Console.WriteLine($"✅ Klasörler hazır:");
    Console.WriteLine($"   {firmaKlasoru}");
    Console.WriteLine($"   {medyaKlasoru}");
    Console.WriteLine($"   {i18nKlasoru}");

    // ── ADIM 6: EnsureCreated ile firma DB şeması oluştur ──
    Console.WriteLine();
    Console.WriteLine("🗄️  Firma veritabanı şeması oluşturuluyor (EnsureCreated)...");

    try
    {
        if (File.Exists(firmaVtYolu))
        {
            Console.WriteLine($"⚠ {firmaVtYolu} zaten mevcut. Siliniyor...");
            File.Delete(firmaVtYolu);
        }

        var secenekler = new DbContextOptionsBuilder<VizitLink3DDbContext>();
        secenekler.UseSqlite($"Data Source={firmaVtYolu}");

        await using var context = new VizitLink3DDbContext(secenekler.Options, null!);
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine($"✅ Firma veritabanı şeması oluşturuldu: {firmaVtYolu}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ EnsureCreated başarısız: {ex.Message}");
        Console.WriteLine($"   Detay: {ex.InnerException?.Message}");
        Console.WriteLine("   Ham SQL ile devam ediliyor...");
        await KullanicilarTablosunuHamSqlIleOlustur(firmaVtYolu);
    }

    // ── ADIM 6.5: Firma DB'ye Firma kaydı ekle (FK için gerekli) ──
    Console.WriteLine();
    Console.WriteLine("📝 Firma DB'ye Firma kaydı ekleniyor (FK constraint)...");

    using (var fkVtBaglantisi = new SqliteConnection($"Data Source={firmaVtYolu}"))
    {
        await fkVtBaglantisi.OpenAsync();

        using var fkKontrolCmd = fkVtBaglantisi.CreateCommand();
        fkKontrolCmd.CommandText = "SELECT COUNT(*) FROM Firmalar WHERE Id = @id;";
        fkKontrolCmd.Parameters.AddWithValue("@id", firmaId);
        var fkMevcut = Convert.ToInt64(await fkKontrolCmd.ExecuteScalarAsync());

        if (fkMevcut > 0)
        {
            Console.WriteLine($"   ℹ Firma DB'de Firma kaydı zaten mevcut (ID: {firmaId}).");
        }
        else
        {
            using var pragmaCmd = fkVtBaglantisi.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA foreign_keys = OFF;";
            await pragmaCmd.ExecuteNonQueryAsync();

            using var fkEkleCmd = fkVtBaglantisi.CreateCommand();
            fkEkleCmd.CommandText = @"
                INSERT INTO Firmalar (Id, Ad, Unvan, Slug, Domain, AktifMi, DemoMu, SiteTema, 
                    MenuYatayAralik, MenuDikeyPadding, LogoMaxYukseklik, PaketTipi, MaxKullaniciSayisi, 
                    OlusturulmaTarihi, Eposta)
                VALUES (@id, @ad, @unvan, @slug, @domain, 1, 1, 'modern', 30, 20, 60, 'Standart', 5, 
                    @tarih, @eposta);";
            fkEkleCmd.Parameters.AddWithValue("@id", firmaId);
            fkEkleCmd.Parameters.AddWithValue("@ad", "Test Firma 2");
            fkEkleCmd.Parameters.AddWithValue("@unvan", "Test Firma 2 Ltd. Şti.");
            fkEkleCmd.Parameters.AddWithValue("@slug", "test-firma-2");
            fkEkleCmd.Parameters.AddWithValue("@domain", "test-firma-2.localhost");
            fkEkleCmd.Parameters.AddWithValue("@tarih", DateTimeOffset.UtcNow.ToString("o"));
            fkEkleCmd.Parameters.AddWithValue("@eposta", "admin@test-firma-2.com");
            await fkEkleCmd.ExecuteNonQueryAsync();

            pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
            await pragmaCmd.ExecuteNonQueryAsync();

            Console.WriteLine($"   ✅ Firma DB'ye Firma kaydı eklendi (ID: {firmaId}).");
        }
    }

    // ── ADIM 7: FirmaAdmin kullanıcısı ekle ──
    Console.WriteLine();
    Console.WriteLine("👤 FirmaAdmin kullanıcısı ekleniyor...");

    var sifreHash = BCrypt.Net.BCrypt.HashPassword("Admin2026!");
    Console.WriteLine($"   BCrypt hash üretildi: {sifreHash[..20]}...");

    using (var firmaBaglantisi = new SqliteConnection($"Data Source={firmaVtYolu}"))
    {
        await firmaBaglantisi.OpenAsync();

        using var sayacCmd = firmaBaglantisi.CreateCommand();
        sayacCmd.CommandText = "SELECT COUNT(*) FROM Kullanicilar WHERE Eposta = @eposta;";
        sayacCmd.Parameters.AddWithValue("@eposta", "admin@test-firma-2.com");
        var mevcutKullaniciSayisi = Convert.ToInt64(await sayacCmd.ExecuteScalarAsync());

        if (mevcutKullaniciSayisi > 0)
        {
            Console.WriteLine($"⚠ admin@test-firma-2.com zaten mevcut.");
            // FirmaId null/sifir ise guncelle (geriye donuk duzeltme)
            using var guncelleCmd = firmaBaglantisi.CreateCommand();
            guncelleCmd.CommandText = "UPDATE Kullanicilar SET FirmaId = @firmaId WHERE (FirmaId IS NULL OR FirmaId = 0) AND Eposta = @eposta;";
            guncelleCmd.Parameters.AddWithValue("@firmaId", firmaId);
            guncelleCmd.Parameters.AddWithValue("@eposta", "admin@test-firma-2.com");
            var guncellenen = await guncelleCmd.ExecuteNonQueryAsync();
            if (guncellenen > 0)
                Console.WriteLine($"   ✅ Kullaniciya FirmaId={firmaId} eklendi.");
        }
        else
        {
            using var kullaniciCmd = firmaBaglantisi.CreateCommand();
            kullaniciCmd.CommandText = @"
                INSERT INTO Kullanicilar
                    (FirmaId, Eposta, SifreHash, AdSoyad, KullaniciAdi, Rol, EmailDogrulandiMi, 
                     IkiAdimDogrulamaAktif, TelefonDogrulandiMi, AktifMi, 
                     KilitlendiMi, BasarisizGirisDenemesi, SilindiMi,
                     TercihEdilenDil, OlusturulmaTarihi)
                VALUES
                    (@firmaId, @eposta, @sifre, @adSoyad, @kullaniciAdi, @rol, @emailDogrulandiMi, 
                     @ikiAdim, @telefonDog, @aktifMi, 
                     @kilit, @deneme, @silindi,
                     @dil, @tarih);";
            kullaniciCmd.Parameters.AddWithValue("@firmaId", firmaId);
            kullaniciCmd.Parameters.AddWithValue("@eposta", "admin@test-firma-2.com");
            kullaniciCmd.Parameters.AddWithValue("@sifre", sifreHash);
            kullaniciCmd.Parameters.AddWithValue("@adSoyad", "Firma 2 Admin");
            kullaniciCmd.Parameters.AddWithValue("@kullaniciAdi", "firma2admin");
            kullaniciCmd.Parameters.AddWithValue("@rol", (int)Rol.FirmaAdmin);
            kullaniciCmd.Parameters.AddWithValue("@emailDogrulandiMi", 1L);
            kullaniciCmd.Parameters.AddWithValue("@ikiAdim", 0L);
            kullaniciCmd.Parameters.AddWithValue("@telefonDog", 0L);
            kullaniciCmd.Parameters.AddWithValue("@aktifMi", 1L);
            kullaniciCmd.Parameters.AddWithValue("@kilit", 0L);
            kullaniciCmd.Parameters.AddWithValue("@deneme", 0L);
            kullaniciCmd.Parameters.AddWithValue("@silindi", 0L);
            kullaniciCmd.Parameters.AddWithValue("@dil", "tr");
            kullaniciCmd.Parameters.AddWithValue("@tarih", DateTime.UtcNow.ToString("o"));
            await kullaniciCmd.ExecuteNonQueryAsync();

            Console.WriteLine("✅ FirmaAdmin kullanıcısı eklendi.");
            Console.WriteLine($"   FirmaId: {firmaId}");
            Console.WriteLine($"   Epasta: admin@test-firma-2.com");
            Console.WriteLine($"   Kullanıcı Adı: firma2admin");
            Console.WriteLine($"   Şifre: Admin2026!");
            Console.WriteLine($"   Rol: FirmaAdmin (2)");
        }
    }

    // ── ADIM 8: Dil dosyaları oluştur ──
    Console.WriteLine();
    Console.WriteLine("🌐 Dil dosyaları oluşturuluyor...");

    var trJson = @"{
  ""anasayfa"": ""Anasayfa"",
  ""hakkimizda"": ""Hakkımızda"",
  ""urunler"": ""Ürünler"",
  ""galeri"": ""Galeri"",
  ""iletisim"": ""İletişim"",
  ""blog"": ""Blog"",
  ""site_basligi"": ""Test Firma 2"",
  ""site_aciklamasi"": ""Test Firma 2 — SaaS Demo""
}";

    var enJson = @"{
  ""anasayfa"": ""Home"",
  ""hakkimizda"": ""About Us"",
  ""urunler"": ""Products"",
  ""galeri"": ""Gallery"",
  ""iletisim"": ""Contact"",
  ""blog"": ""Blog"",
  ""site_basligi"": ""Test Company 2"",
  ""site_aciklamasi"": ""Test Company 2 — SaaS Demo""
}";

    foreach (var (dil, icerik) in new[] { ("tr", trJson), ("en", enJson) })
    {
        var yol = Path.Combine(i18nKlasoru, $"{dil}.json");
        if (File.Exists(yol))
        {
            Console.WriteLine($"⚠ {yol} zaten mevcut. Atlanıyor.");
        }
        else
        {
            await File.WriteAllTextAsync(yol, icerik, new UTF8Encoding(false));
            Console.WriteLine($"✅ {yol} oluşturuldu.");
        }
    }

    // ── ÖZET ──
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine("🎉 TEST FİRMA 2 BAŞARIYLA OLUŞTURULDU!");
    Console.WriteLine("═══════════════════════════════════════════");
    Console.WriteLine($"   Firma Adı:       Test Firma 2");
    Console.WriteLine($"   Slug:            {firmaSlug}");
    Console.WriteLine($"   Domain:          test-firma-2.localhost");
    Console.WriteLine($"   Admin Kullanıcı: firma2admin");
    Console.WriteLine($"   Admin Eposta:    admin@test-firma-2.com");
    Console.WriteLine($"   Admin Şifre:     Admin2026!");
    Console.WriteLine($"   Firma DB:        {firmaVtYolu}");
    Console.WriteLine($"   Medya Klasörü:   {medyaKlasoru}");
    Console.WriteLine($"   i18n Klasörü:    {i18nKlasoru}");
    Console.WriteLine("═══════════════════════════════════════════");
}
