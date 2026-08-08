using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.SuperAdmin.Servisler;

/// <summary>
/// SuperAdmin uzerinden yeni firma olusturuldugunda:
/// 1. firmalar/{slug}/ klasor yapisini olusturur (medya/, i18n/)
/// 2. firmalar/{slug}/{slug}.db veritabanini tam VizitLink3DDbContext semasiyla basar
/// 3. Varsayilan FirmaAdmin kullanicisini veritabanina yazar
/// 4. firmalar/{slug}/i18n/tr.json ve en.json varsayilan dil dosyalarini olusturur
/// </summary>
public class FirmaOlusturmaServisi
{
    private readonly IWebHostEnvironment _ortam;
    private readonly ILogger<FirmaOlusturmaServisi> _log;

    public FirmaOlusturmaServisi(IWebHostEnvironment ortam, ILogger<FirmaOlusturmaServisi> log)
    {
        _ortam = ortam;
        _log = log;
    }

    public async Task<bool> FirmaAltyapisiniOlustur(string slug, int firmaId, string ad, string domain, string lisansTipi = "Yillik")
    {
        try
        {
            var kokDizin = _ortam.ContentRootPath;
            var firmaKlasoru = Path.Combine(kokDizin, "firmalar", slug);
            var medyaKlasoru = Path.Combine(firmaKlasoru, "medya");
            var i18nKlasoru = Path.Combine(firmaKlasoru, "i18n");
            var vtYolu = Path.Combine(firmaKlasoru, $"{slug}.db");

            Directory.CreateDirectory(firmaKlasoru);
            Directory.CreateDirectory(medyaKlasoru);
            Directory.CreateDirectory(i18nKlasoru);

            _log.LogInformation("Firma klasor yapisi olusturuldu: {FirmaKlasoru}", firmaKlasoru);

            // Tam sema + FirmaAdmin
            await VeritabaniniBasAsync(vtYolu, slug, ad, domain);

            // Ana DB'ye (vizitlink3d.db) Firma + Lisans kaydi ekle
            await AnaDBFirmaEkleAsync(slug, firmaId, ad, domain);

            // Dil dosyalari
            DilDosyalariniOlustur(i18nKlasoru);

            // Ana DB'ye lisans kaydı ekle
            await AnaVtLisansEkleAsync(slug, firmaId, domain, lisansTipi);

            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Firma altyapisi olusturulurken hata: {Slug}", slug);
            return false;
        }
    }

    /// <summary>
    /// Ana veritabanina (vizitlink3d.db) Firma kaydi ekler.
    /// FirmaCozumlemeMiddleware bu DB'den domain bazli firma cozumleme yapar.
    /// </summary>
    private async Task AnaDBFirmaEkleAsync(string slug, int firmaId, string ad, string domain)
    {
        try
        {
            var kokDizin = _ortam.ContentRootPath;
            var anaVtYolu = Path.Combine(kokDizin, "vizitlink3d.db");
            if (!File.Exists(anaVtYolu))
            {
                anaVtYolu = Path.Combine(kokDizin, "..", "VizitLink3D.Api", "vizitlink3d.db");
            }
            if (!File.Exists(anaVtYolu))
            {
                _log.LogWarning("Ana VT bulunamadi, Firma kaydi atlaniyor: {Yol}", anaVtYolu);
                return;
            }

            await using var baglanti = new SqliteConnection($"Data Source={anaVtYolu}");
            await baglanti.OpenAsync();

            // Mevcut Firma kontrolu
            await using var kontrolCmd = baglanti.CreateCommand();
            kontrolCmd.CommandText = @"SELECT COUNT(*) FROM ""Firmalar"" WHERE ""Slug"" = @slug;";
            kontrolCmd.Parameters.AddWithValue("@slug", slug);
            var mevcut = Convert.ToInt64(await kontrolCmd.ExecuteScalarAsync());

            if (mevcut > 0)
            {
                // Guncelle (domain, aktif durum)
                await using var guncelleCmd = baglanti.CreateCommand();
                guncelleCmd.CommandText = @"
                    UPDATE ""Firmalar"" SET
                        ""Domain"" = @domain,
                        ""AktifMi"" = 1,
                        ""GuncellenmeTarihi"" = @guncelleme
                    WHERE ""Slug"" = @slug;";
                guncelleCmd.Parameters.AddWithValue("@domain", domain);
                guncelleCmd.Parameters.AddWithValue("@slug", slug);
                guncelleCmd.Parameters.AddWithValue("@guncelleme", DateTimeOffset.UtcNow.ToString("o"));
                await guncelleCmd.ExecuteNonQueryAsync();
            }
            else
            {
                // Ekle
                await using var ekleCmd = baglanti.CreateCommand();
                ekleCmd.CommandText = @"
                    INSERT INTO ""Firmalar""
                        (""Id"", ""Ad"", ""Unvan"", ""Slug"", ""Domain"", ""AktifMi"", ""OlusturulmaTarihi"")
                    VALUES
                        (@id, @ad, @unvan, @slug, @domain, 1, @tarih);";
                ekleCmd.Parameters.AddWithValue("@id", firmaId);
                ekleCmd.Parameters.AddWithValue("@ad", ad ?? slug);
                ekleCmd.Parameters.AddWithValue("@unvan", ad ?? slug);
                ekleCmd.Parameters.AddWithValue("@slug", slug);
                ekleCmd.Parameters.AddWithValue("@domain", domain);
                ekleCmd.Parameters.AddWithValue("@tarih", DateTimeOffset.UtcNow.ToString("o"));
                await ekleCmd.ExecuteNonQueryAsync();
            }

            _log.LogInformation("Ana DB'ye Firma eklendi: {Slug} ({Domain})", slug, domain);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Ana DB'ye Firma eklenirken hata: {Slug}", slug);
        }
    }

    /// <summary>
    /// Ana veritabanina (vizitlink3d.db) lisans kaydi ekler.
    /// LisansDogrulamaMiddleware bu DB'den domain bazli kontrol yapar.
    /// </summary>
    private async Task AnaVtLisansEkleAsync(string slug, int firmaId, string domain, string lisansTipi)
    {
        try
        {
            // Ana DB yolu: SuperAdmin ile ayni koku dizindeki vizitlink3d.db
            // veya ../VizitLink3D.Api/vizitlink3d.db
            var kokDizin = _ortam.ContentRootPath;
            var anaVtYolu = Path.Combine(kokDizin, "vizitlink3d.db");
            if (!File.Exists(anaVtYolu))
            {
                // API projesinin dizininde ara
                anaVtYolu = Path.Combine(kokDizin, "..", "VizitLink3D.Api", "vizitlink3d.db");
            }
            if (!File.Exists(anaVtYolu))
            {
                _log.LogWarning("Ana VT bulunamadi, lisans kaydi atlaniyor: {Yol}", anaVtYolu);
                return;
            }

            // Lisans sure hesapla
            var baslangic = DateTime.UtcNow;
            DateTime bitis;
            bool suresizMi = false;
            bool demoMu = false;
            int? sureYil = null;

            var tip = (lisansTipi ?? "Yillik").ToLowerInvariant();
            switch (tip)
            {
                case "suresiz":
                case "omurboyu":
                    bitis = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);
                    suresizMi = true;
                    break;
                case "demo":
                    bitis = baslangic.AddDays(14);
                    demoMu = true;
                    break;
                case "2yillik":
                case "2-yil":
                    bitis = baslangic.AddYears(2);
                    sureYil = 2;
                    break;
                case "3yillik":
                case "3-yil":
                    bitis = baslangic.AddYears(3);
                    sureYil = 3;
                    break;
                case "5yillik":
                case "5-yil":
                    bitis = baslangic.AddYears(5);
                    sureYil = 5;
                    break;
                default: // yillik
                    bitis = baslangic.AddYears(1);
                    sureYil = 1;
                    break;
            }

            // HMAC imzali lisans anahtari olustur
            var gizliAnahtar = Environment.GetEnvironmentVariable("VIZITLINK3D_LISANS_KEY") ?? "vizitlink3d-dev-key-2026";
            var icerik = $"{slug}_{domain}_{bitis:yyyyMMdd}";
            using var hmac = new System.Security.Cryptography.HMACSHA256(
                System.Text.Encoding.UTF8.GetBytes(gizliAnahtar));
            var hash = Convert.ToBase64String(hmac.ComputeHash(
                System.Text.Encoding.UTF8.GetBytes(icerik)));
            var lisansAnahtari = $"{icerik}_{hash}";

            await using var baglanti = new SqliteConnection($"Data Source={anaVtYolu}");
            await baglanti.OpenAsync();

            // Mevcut lisans kontrolu
            await using var kontrolCmd = baglanti.CreateCommand();
            kontrolCmd.CommandText = @"SELECT COUNT(*) FROM ""Lisanslar"" WHERE ""FirmaId"" = @firmaId;";
            kontrolCmd.Parameters.AddWithValue("@firmaId", firmaId);
            var mevcutSayisi = Convert.ToInt64(await kontrolCmd.ExecuteScalarAsync());

            if (mevcutSayisi > 0)
            {
                // Güncelle
                await using var guncelleCmd = baglanti.CreateCommand();
                guncelleCmd.CommandText = @"
                    UPDATE ""Lisanslar"" SET
                        ""BirincilDomain"" = @domain,
                        ""BaslangicTarihi"" = @baslangic,
                        ""BitisTarihi"" = @bitis,
                        ""LisansTipi"" = @tip,
                        ""SureYil"" = @sureYil,
                        ""SuresizMi"" = @suresizMi,
                        ""DemoMu"" = @demoMu,
                        ""LisansAnahtari"" = @anahtar,
                        ""AktifMi"" = 1,
                        ""GuncellenmeTarihi"" = @guncelleme
                    WHERE ""FirmaId"" = @firmaId;";
                guncelleCmd.Parameters.AddWithValue("@domain", domain);
                guncelleCmd.Parameters.AddWithValue("@baslangic", baslangic.ToString("o"));
                guncelleCmd.Parameters.AddWithValue("@bitis", bitis.ToString("o"));
                guncelleCmd.Parameters.AddWithValue("@tip", lisansTipi);
                guncelleCmd.Parameters.AddWithValue("@sureYil", (object?)sureYil ?? DBNull.Value);
                guncelleCmd.Parameters.AddWithValue("@suresizMi", suresizMi ? 1 : 0);
                guncelleCmd.Parameters.AddWithValue("@demoMu", demoMu ? 1 : 0);
                guncelleCmd.Parameters.AddWithValue("@anahtar", lisansAnahtari);
                guncelleCmd.Parameters.AddWithValue("@guncelleme", DateTime.UtcNow.ToString("o"));
                guncelleCmd.Parameters.AddWithValue("@firmaId", firmaId);
                await guncelleCmd.ExecuteNonQueryAsync();
            }
            else
            {
                // Ekle
                await using var ekleCmd = baglanti.CreateCommand();
                ekleCmd.CommandText = @"
                    INSERT INTO ""Lisanslar""
                        (""FirmaId"", ""BirincilDomain"", ""BaslangicTarihi"", ""BitisTarihi"",
                         ""LisansTipi"", ""SureYil"", ""SuresizMi"", ""DemoMu"",
                         ""LisansAnahtari"", ""AktifMi"", ""OlusturulmaTarihi"")
                    VALUES
                        (@firmaId, @domain, @baslangic, @bitis,
                         @tip, @sureYil, @suresizMi, @demoMu,
                         @anahtar, 1, @olusturma);";
                ekleCmd.Parameters.AddWithValue("@firmaId", firmaId);
                ekleCmd.Parameters.AddWithValue("@domain", domain);
                ekleCmd.Parameters.AddWithValue("@baslangic", baslangic.ToString("o"));
                ekleCmd.Parameters.AddWithValue("@bitis", bitis.ToString("o"));
                ekleCmd.Parameters.AddWithValue("@tip", lisansTipi);
                ekleCmd.Parameters.AddWithValue("@sureYil", (object?)sureYil ?? DBNull.Value);
                ekleCmd.Parameters.AddWithValue("@suresizMi", suresizMi ? 1 : 0);
                ekleCmd.Parameters.AddWithValue("@demoMu", demoMu ? 1 : 0);
                ekleCmd.Parameters.AddWithValue("@anahtar", lisansAnahtari);
                ekleCmd.Parameters.AddWithValue("@olusturma", DateTime.UtcNow.ToString("o"));
                await ekleCmd.ExecuteNonQueryAsync();
            }

            _log.LogInformation("Ana DB'ye lisans eklendi: {Domain} ({Tip})", domain, lisansTipi);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Ana DB'ye lisans eklenirken hata: {Domain}", domain);
        }
    }

    /// <summary>
    /// 1) VizitLink3DDbContext.EnsureCreated() ile tum tablolari (50+ DbSet) olusturur.
    /// 2) Varsayilan FirmaAdmin kullanicisini ekler.
    /// 
    /// FirmaOlusturmaServisi HTTP context'i olmadan calisir.
    /// Bu yuzden KiraciServisi null gecilir — OnConfiguring'de IsConfigured=true
    /// oldugu icin fallback calismaz.
    /// </summary>
    private async Task VeritabaniniBasAsync(string vtYolu, string slug, string ad, string domain)
    {
        // 1) VizitLink3DDbContext ile tam semayi uygula
        var secenekler = new DbContextOptionsBuilder<VizitLink3DDbContext>();
        secenekler.UseSqlite($"Data Source={vtYolu}");

        // FirmaOlusturmaServisi HTTP context'i olmadan calisir, null KiraciServisi gec
        await using var context = new VizitLink3DDbContext(secenekler.Options, null!);
        await context.Database.EnsureCreatedAsync();

        _log.LogInformation("Firma tam semasi uygulandi: {VtYolu}", vtYolu);

        await using var baglanti = new SqliteConnection($"Data Source={vtYolu}");
        await baglanti.OpenAsync();

        // Firma kaydini per-firm DB'ye ekle (FK'ler icin gerekli)
        await using var firmaEkle = baglanti.CreateCommand();
        firmaEkle.CommandText = @"INSERT OR IGNORE INTO ""Firmalar"" (""Id"", ""Ad"", ""Unvan"", ""Slug"", ""Domain"", ""AktifMi"", ""DemoMu"", ""MaxKullaniciSayisi"", ""MenuYatayAralik"", ""MenuDikeyPadding"", ""LogoMaxYukseklik"", ""OlusturulmaTarihi"") VALUES (1, @ad, @ad, @slug, @domain, 1, 0, 5, 8, 12, 48, @t);";
        firmaEkle.Parameters.AddWithValue("@ad", ad ?? slug);
        firmaEkle.Parameters.AddWithValue("@slug", slug);
        firmaEkle.Parameters.AddWithValue("@domain", domain ?? (slug + ".com"));
        firmaEkle.Parameters.AddWithValue("@t", DateTimeOffset.UtcNow.ToString("o"));
        await firmaEkle.ExecuteNonQueryAsync();

        await using var sayacKomutu = baglanti.CreateCommand();
        sayacKomutu.CommandText = @"SELECT COUNT(*) FROM ""Kullanicilar"";";
        var mevcutKullaniciSayisi = Convert.ToInt64(await sayacKomutu.ExecuteScalarAsync());

        if (mevcutKullaniciSayisi == 0)
        {
            var sifreHash = BCrypt.Net.BCrypt.HashPassword("Admin2026!");
            await using var cmd = baglanti.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ""Kullanicilar""
                    (""Eposta"", ""SifreHash"", ""AdSoyad"", ""KullaniciAdi"", ""Rol"", ""EmailDogrulandiMi"", ""AktifMi"", ""OlusturulmaTarihi"")
                VALUES
                    (@eposta, @sifre, @adSoyad, @kullaniciAdi, @rol, @emailDogrulandiMi, @aktifMi, @tarih);";
            cmd.Parameters.AddWithValue("@eposta", "admin@" + domain);
            cmd.Parameters.AddWithValue("@sifre", sifreHash);
            cmd.Parameters.AddWithValue("@adSoyad", "Firma Admin");
            cmd.Parameters.AddWithValue("@kullaniciAdi", "admin");
            cmd.Parameters.AddWithValue("@rol", (int)Rol.SuperAdmin);
            cmd.Parameters.AddWithValue("@emailDogrulandiMi", 1);
            cmd.Parameters.AddWithValue("@aktifMi", 1);
            cmd.Parameters.AddWithValue("@tarih", DateTimeOffset.UtcNow.ToString("o"));
            await cmd.ExecuteNonQueryAsync();

            _log.LogInformation("Varsayilan FirmaAdmin kullanicisi olusturuldu: {VtYolu}", vtYolu);

            // 3) Admin menulerini ekle
            await AdminMenuleriniEkleAsync(baglanti);
            _log.LogInformation("Admin menuleri eklendi: {VtYolu}", vtYolu);
        }

        _log.LogInformation("Firma veritabani basildi: {VtYolu}", vtYolu);
    }

    /// <summary>
    /// Varsayilan dil dosyalarini (tr.json, en.json) bos sozluk ({}) olarak olusturur.
    /// Mevcut dosyalarin uzerine yazilmaz — firma cevirileri korunur.
    /// </summary>
    private void DilDosyalariniOlustur(string i18nKlasoru)
    {
        foreach (var dil in new[] { "tr", "en" })
        {
            var yol = Path.Combine(i18nKlasoru, $"{dil}.json");

            if (File.Exists(yol))
            {
                _log.LogInformation("Dil dosyasi zaten mevcut, atlaniyor: {Yol}", yol);
                continue;
            }

            File.WriteAllText(yol, "{}", new System.Text.UTF8Encoding(false));
            _log.LogInformation("Varsayilan dil dosyasi olusturuldu: {Yol}", yol);
        }
    }

    private static async Task AdminMenuleriniEkleAsync(SqliteConnection baglanti)
    {
        await using var cmd = baglanti.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM MenuOgeleri WHERE Konum = 'AdminSol';";
        var mevcut = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        if (mevcut > 0) return;

        int nextId = 1;
        var tarih = DateTimeOffset.UtcNow.ToString("o");

        async Task<int> E(string b, string u, string i, int? ust, int s) {
            var ins = baglanti.CreateCommand();
            ins.CommandText = "INSERT INTO MenuOgeleri (Id,Baslik,Url,UstMenuId,Sira,Ikon,Konum,AktifMi,YeniSekmede,SilindiMi,SuperAdminGerekliMi,KilitliMi,SistemMenusuMu,OlusturulmaTarihi,FirmaId) VALUES (@id,@b,@u,@ust,@s,@i,'AdminSol',1,0,0,0,0,1,@t,1);";
            ins.Parameters.AddWithValue("@id", nextId++);
            ins.Parameters.AddWithValue("@b", b);
            ins.Parameters.AddWithValue("@u", u);
            ins.Parameters.AddWithValue("@ust", ust ?? (object)DBNull.Value);
            ins.Parameters.AddWithValue("@s", s);
            ins.Parameters.AddWithValue("@i", i ?? "");
            ins.Parameters.AddWithValue("@t", tarih);
            await ins.ExecuteNonQueryAsync();
            return nextId - 1;
        }

        await E("Gösterge Paneli","/admin/dashboard","dashboard",null,1);
        await E("İş Takip","/admin/is-takip","task_alt",null,2);

        var u1 = await E("Ürün ve 3D","","category",null,10);
        await E("Ürün Sihirbazı","/admin/urun-sihirbazi","auto_awesome",u1,1);
        await E("Ürünler","/admin/urun-yonetimi","inventory_2",u1,2);
        await E("Kapı/Kapak Modelleri","/admin/kapak-modeli-yonetimi","door_front",u1,3);
        await E("Ürün Aileleri","/admin/urun-ailesi-yonetimi","account_tree",u1,4);
        await E("Ürün Kategorileri","/admin/urun-kategori-yonetimi","folder",u1,5);
        await E("3D Model Yönetimi","/admin/uc-boyut-model-yonetimi","view_in_ar",u1,6);
        await E("Parça Eşleme","/admin/uc-boyut-parca-esleme","join_inner",u1,7);
        await E("RAL Renk Yönetimi","/admin/ral-renk-yonetimi","palette",u1,8);
        await E("Malzeme Yönetimi","/admin/malzeme-yonetimi","texture",u1,9);
        await E("Kaplama Yönetimi","/admin/kaplama-yonetimi","layers",u1,10);
        await E("Sahne Ayarları","/admin/sahne-ayarlari","tune",u1,11);
        await E("Konfigürasyon","/admin/konfigurasyon-sablonu-yonetimi","build",u1,12);

        var i1 = await E("İçerik ve Medya","","newspaper",null,20);
        await E("Ana Sayfa","/admin/anasayfa-yonetimi","home",i1,1);
        await E("Slayt Yönetimi","/admin/slayt-yonetimi","view_carousel",i1,2);
        await E("Sayfa İçerikleri","/admin/icerik-yonetimi","article",i1,3);
        await E("Sayfa Yönetimi","/admin/sayfa-yonetimi","description",i1,4);
        await E("Haber Yönetimi","/admin/haber-yonetimi","feed",i1,5);
        await E("SSS Yönetimi","/admin/sss-yonetimi","help",i1,6);
        await E("SEO Yönetimi","/admin/seo-yonetimi","search",i1,7);
        await E("Medya Havuzu","/admin/medya-havuzu","perm_media",i1,8);
        await E("Galeri","/admin/galeri","collections",i1,9);
        await E("PDF Katalog","/admin/pdf-katalog-yonetimi","picture_as_pdf",i1,10);
        await E("Katalog Yönetimi","/admin/katalog-yonetimi","book",i1,11);

        var m1 = await E("Müşteri ve Pazarlama","","groups",null,30);
        await E("Proje Yönetimi","/admin/proje-yonetimi","engineering",m1,1);
        await E("Referanslar","/admin/referans-yonetimi","verified_user",m1,2);
        await E("Müşteri Yorumları","/admin/yorum-yonetimi","rate_review",m1,3);
        await E("Hizmet Adımları","/admin/hizmet-adimi-yonetimi","account_tree",m1,4);
        await E("Bülten Aboneleri","/admin/bulten-yonetimi","email",m1,5);
        await E("E-posta Şablonları","/admin/eposta-sablonlari","mark_email_unread",m1,6);

        var il1 = await E("İletişim ve Destek","","mail",null,40);
        await E("Gelen Mesajlar","/admin/iletisim-mesajlari","inbox",il1,1);
        await E("Canlı Sohbet","/admin/canli-sohbet","chat",il1,2);
        await E("Teklif Yönetimi","/admin/teklif-yonetimi","description",il1,3);

        var o1 = await E("Organizasyon","","corporate_fare",null,50);
        await E("Şube Yönetimi","/admin/sube-yonetimi","store",o1,1);
        await E("Ekip Yönetimi","/admin/ekip-yonetimi","group",o1,2);

        var s1 = await E("Sistem","","settings",null,60);
        await E("Kullanıcı Yönetimi","/admin/kullanici-yonetimi","person",s1,1);
        await E("Dil ve Çeviri","/admin/ceviri-yonetimi","translate",s1,2);
        await E("AI Ayarları","/admin/ai-ayarlari","smart_toy",s1,3);
        await E("Tema Yönetimi","/admin/tema-yonetimi","palette",s1,4);
        await E("Lisans Yönetimi","/admin/lisans-yonetimi","key",s1,5);
        await E("API Entegrasyonları","/admin/api-ayarlari","api",s1,6);
        await E("Ayarlar","/admin/ayarlar","settings_applications",s1,7);
        await E("Denetim Log","/admin/denetim-log","history",s1,8);
        await E("Çöp Kutusu","/admin/cop-kutusu","delete",s1,9);
        await E("Menü Yönetimi","/admin/menu-yonetimi","menu",s1,10);
    }
}