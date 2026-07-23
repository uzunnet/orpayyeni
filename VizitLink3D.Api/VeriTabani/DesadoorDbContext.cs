using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.Servisler;
using VizitLink3D.Ortak.Modeller;
using VizitLink3D.Ortak.Modeller.AI;
using VizitLink3D.Ortak.Modeller.Medya;
using VizitLink3D.Ortak.Modeller.Urunler;
using VizitLink3D.Ortak.Modeller.Renkler;
using VizitLink3D.Ortak.Modeller.Malzemeler;
using VizitLink3D.Ortak.Modeller.Tema;
using VizitLink3D.Ortak.Modeller.Guvenlik;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.VeriTabani;

public class VizitLink3DDbContext(DbContextOptions<VizitLink3DDbContext> secenekler, KiraciServisi kiraciServisi) : DbContext(secenekler)
{
    private readonly KiraciServisi _kiraci = kiraciServisi;
    // Mevcut tablolar
    public DbSet<KapakModeli> KapakModelleri => Set<KapakModeli>();
    public DbSet<MenuOgesi> MenuOgeleri => Set<MenuOgesi>();
    public DbSet<SayfaIcerigi> SayfaIcerikleri => Set<SayfaIcerigi>();
    public DbSet<GaleriGorseli> GaleriGorselleri => Set<GaleriGorseli>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<IletisimMesaji> IletisimMesajlari => Set<IletisimMesaji>();
    public DbSet<CanliSohbetMesaji> CanliSohbetMesajlari => Set<CanliSohbetMesaji>();

    // Sektor (sektor siniflandirmasi)
    public DbSet<Sektor> Sektorler => Set<Sektor>();

    // Kapi
    public DbSet<KapiKategorisi> KapiKategorileri => Set<KapiKategorisi>();
    public DbSet<KapiKategorisiYerellestirme> KapiKategorisiYerellestirmeleri => Set<KapiKategorisiYerellestirme>();
    public DbSet<KapiModeliResim> KapiModeliResimleri => Set<KapiModeliResim>();
    public DbSet<KapiModeliYerellestirme> KapiModeliYerellestirmeleri => Set<KapiModeliYerellestirme>();
    public DbSet<MobilyaKategorisi> MobilyaKategorileri => Set<MobilyaKategorisi>();
    public DbSet<MobilyaKategorisiYerellestirme> MobilyaKategorisiYerellestirmeleri => Set<MobilyaKategorisiYerellestirme>();
    public DbSet<MobilyaUrunu> MobilyaUrunleri => Set<MobilyaUrunu>();
    public DbSet<MobilyaUrunuYerellestirme> MobilyaUrunuYerellestirmeleri => Set<MobilyaUrunuYerellestirme>();

    // Yeni tablolar — Icerik / Pazarlama
    public DbSet<ProjeKategorisi> ProjeKategorileri => Set<ProjeKategorisi>();
    public DbSet<Proje> Projeler => Set<Proje>();
    public DbSet<ProjeResim> ProjeResimleri => Set<ProjeResim>();
    public DbSet<Slayt> Slaytlar => Set<Slayt>();
    public DbSet<Referans> Referanslar => Set<Referans>();
    public DbSet<MusteriYorumu> MusteriYorumlari => Set<MusteriYorumu>();
    public DbSet<HizmetAdimi> HizmetAdimlari => Set<HizmetAdimi>();

    // Yeni tablolar — Bilgilendirme
    public DbSet<SikSorulanSoru> SikSorulanSorular => Set<SikSorulanSoru>();
    public DbSet<Sertifika> Sertifikalar => Set<Sertifika>();
    public DbSet<Katalog> Kataloglar => Set<Katalog>();
    public DbSet<BultenAbonesi> BultenAboneleri => Set<BultenAbonesi>();
    public DbSet<EpostaSablonu> EpostaSablonlari => Set<EpostaSablonu>();
    public DbSet<Sube> Subeler => Set<Sube>();
    public DbSet<EkipUyesi> EkipUyeleri => Set<EkipUyesi>();
    public DbSet<SistemAyari> SistemAyarlari => Set<SistemAyari>();
    public DbSet<TanitimVideo> TanitimVideolari => Set<TanitimVideo>();

    // Yeni tablolar — Sistem
    public DbSet<Ceviri> Ceviriler => Set<Ceviri>();
    public DbSet<Dil> Diller => Set<Dil>();
    public DbSet<Lisans> Lisanslar => Set<Lisans>();
    public DbSet<AuditLog> AuditLoglar => Set<AuditLog>();
    public DbSet<ZiyaretKaydi> ZiyaretKayitlari => Set<ZiyaretKaydi>();

    // Firma (zaten Ortak'ta, DbSet olarak eklenmeli)
    public DbSet<Firma> Firmalar => Set<Firma>();

    // Tema
    public DbSet<TemaSablonu> TemaSablonlari => Set<TemaSablonu>();
    public DbSet<FirmaTemaAtama> FirmaTemaAtamalari => Set<FirmaTemaAtama>();
    public DbSet<TemaRevizyonu> TemaRevizyonlari => Set<TemaRevizyonu>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<HaberYazisi> Haberler => Set<HaberYazisi>();

    // Medya Havuzu
    public DbSet<Medya> Medyalar => Set<Medya>();
    public DbSet<MedyaKlasoru> MedyaKlasorleri => Set<MedyaKlasoru>();
    public DbSet<MedyaKullanim> MedyaKullanimlari => Set<MedyaKullanim>();

    // AI Asistan Altyapisi
    public DbSet<AISaglayicisi> AISaglayicilari => Set<AISaglayicisi>();
    public DbSet<AICagrisiKaydi> AICagrisiKayitlari => Set<AICagrisiKaydi>();

    // Urun Yonetimi (3D Konfigurator)
    public DbSet<UrunAilesi> UrunAilesileri => Set<UrunAilesi>();
    public DbSet<UrunKategori> UrunKategorileri => Set<UrunKategori>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<UrunYerellestirme> UrunYerellestirmeleri => Set<UrunYerellestirme>();
    public DbSet<UrunMedya> UrunMedyalari => Set<UrunMedya>();
    public DbSet<UrunUcBoyutParcasi> UrunUcBoyutParcalari => Set<UrunUcBoyutParcasi>();
    public DbSet<UrunParcaGrubu> UrunParcaGruplari => Set<UrunParcaGrubu>();
    public DbSet<UrunParcaEslemesi> UrunParcaEslemeleri => Set<UrunParcaEslemesi>();
    public DbSet<RalRengi> RalRenkleri => Set<RalRengi>();
    public DbSet<RenkKatalogu> RenkKataloglari => Set<RenkKatalogu>();
    public DbSet<Malzeme> Malzemeler => Set<Malzeme>();
    public DbSet<KaplamaSecenegi> KaplamaSecenekleri => Set<KaplamaSecenegi>();
    public DbSet<UrunParcaRenkSecenegi> UrunParcaRenkSecenekleri => Set<UrunParcaRenkSecenegi>();
    public DbSet<UrunParcaMalzemeSecenegi> UrunParcaMalzemeSecenekleri => Set<UrunParcaMalzemeSecenegi>();
    public DbSet<UrunKonfigurasyonSablonu> UrunKonfigurasyonSablonlari => Set<UrunKonfigurasyonSablonu>();
    public DbSet<UrunKonfigurasyonKurali> UrunKonfigurasyonKurallari => Set<UrunKonfigurasyonKurali>();
    public DbSet<UrunUcBoyutModeli> UrunUcBoyutModelleri => Set<UrunUcBoyutModeli>();
    public DbSet<UrunUcBoyutSahneOnayari> UrunUcBoyutSahneOnayarlari => Set<UrunUcBoyutSahneOnayari>();
    public DbSet<MusteriKonfigurasyonu> MusteriKonfigurasyonlari => Set<MusteriKonfigurasyonu>();
    public DbSet<MusteriKonfigurasyonParcasi> MusteriKonfigurasyonParcalari => Set<MusteriKonfigurasyonParcasi>();
    public DbSet<TeklifIstegi> TeklifIstekleri => Set<TeklifIstegi>();
    public DbSet<TeklifIstegiParcasi> TeklifIstegiParcalari => Set<TeklifIstegiParcasi>();
    public DbSet<UrunPdfKaynagi> UrunPdfKaynaklari => Set<UrunPdfKaynagi>();
    public DbSet<PdfSayfaGorseli> PdfSayfaGorselleri => Set<PdfSayfaGorseli>();
    public DbSet<IsTakipKaydi> IsTakipKayitlari => Set<IsTakipKaydi>();
    public DbSet<SayfaDuzenAyari> SayfaDuzenAyarlari => Set<SayfaDuzenAyari>();

    // Guvenlik — Tenant API Anahtarlari
    public DbSet<FirmaApiAnahtari> FirmaApiAnahtarlari => Set<FirmaApiAnahtari>();

    // Guvenlik — Embed Oturum Nonce Kayitlari (replay korumasi)
    public DbSet<EmbedOturumNonceKaydi> EmbedOturumNonceKayitlari => Set<EmbedOturumNonceKaydi>();

    protected override void OnConfiguring(DbContextOptionsBuilder secenekler)
    {
        base.OnConfiguring(secenekler);

        // Multi-tenant: Firma slug'ına göre DB dosyasını belirle
        // KiraciServisi'den firma slug'ını oku, yoksa varsayılan değer kullan
        if (!secenekler.IsConfigured)
        {
            var firmaSlug = _kiraci?.MevcutSlug ?? "vizitlink3d";
            var dbPath = System.IO.Path.Combine(
                System.IO.Directory.GetCurrentDirectory(),
                "VizitLink3D.Api",
                $"{firmaSlug}.db"
            );
            secenekler.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelOlusturucu)
    {
        // MenuOgesi iliskileri
        modelOlusturucu.Entity<MenuOgesi>()
            .HasMany(m => m.AltMenuler)
            .WithOne(m => m.UstMenu)
            .HasForeignKey(m => m.UstMenuId)
            .OnDelete(DeleteBehavior.Restrict);

        // SayfaIcerigi composite unique index
        modelOlusturucu.Entity<SayfaIcerigi>()
            .HasIndex(s => new { s.FirmaId, s.Bolum, s.Anahtar, s.Dil })
            .IsUnique();
        modelOlusturucu.Entity<SayfaIcerigi>()
            .HasOne(s => s.Firma)
            .WithMany()
            .HasForeignKey(s => s.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
        modelOlusturucu.Entity<SayfaIcerigi>()
            .HasQueryFilter(s => !s.SilindiMi);

        // KapiKategorisiYerellestirme composite unique index
        modelOlusturucu.Entity<KapiKategorisiYerellestirme>()
            .HasIndex(y => new { y.KapiKategorisiId, y.Dil })
            .IsUnique();

        // KapiModeliResim iliskisi
        modelOlusturucu.Entity<KapiModeliResim>()
            .HasOne(r => r.KapakModeli)
            .WithMany(k => k.GaleriResimleri)
            .HasForeignKey(r => r.KapakModeliId)
            .OnDelete(DeleteBehavior.Cascade);

        // KapiModeliYerellestirme composite unique
        modelOlusturucu.Entity<KapiModeliYerellestirme>()
            .HasIndex(y => new { y.KapakModeliId, y.Dil })
            .IsUnique();

        // MobilyaKategorisiYerellestirme
        modelOlusturucu.Entity<MobilyaKategorisiYerellestirme>()
            .HasIndex(y => new { y.MobilyaKategorisiId, y.Dil })
            .IsUnique();

        // MobilyaUrunuYerellestirme
        modelOlusturucu.Entity<MobilyaUrunuYerellestirme>()
            .HasIndex(y => new { y.MobilyaUrunuId, y.Dil })
            .IsUnique();

        // ProjeResim iliskisi
        modelOlusturucu.Entity<ProjeResim>()
            .HasOne(r => r.Proje)
            .WithMany(p => p.Resimler)
            .HasForeignKey(r => r.ProjeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ceviri composite unique index (Anahtar + Dil)
        modelOlusturucu.Entity<Ceviri>()
            .HasIndex(c => new { c.Anahtar, c.Dil })
            .IsUnique();

        // Dil Kod unique
        modelOlusturucu.Entity<Dil>()
            .HasIndex(d => d.Kod)
            .IsUnique();

        // KapakModeli Slug unique
        modelOlusturucu.Entity<KapakModeli>()
            .HasIndex(k => k.Slug)
            .IsUnique();

        // Kullanici Eposta ve KullaniciAdi unique
        modelOlusturucu.Entity<Kullanici>()
            .HasIndex(k => k.Eposta)
            .IsUnique();

        modelOlusturucu.Entity<Kullanici>()
            .HasIndex(k => k.KullaniciAdi)
            .IsUnique();

        // === SOFT DELETE GLOBAL QUERY FILTER (anayasa §8) ===
        modelOlusturucu.Entity<MenuOgesi>()
            .HasQueryFilter(m => !m.SilindiMi);
        modelOlusturucu.Entity<KapakModeli>()
            .HasQueryFilter(k => !k.SilindiMi);
        modelOlusturucu.Entity<Kullanici>()
            .HasQueryFilter(k => !k.SilindiMi);
        modelOlusturucu.Entity<Medya>()
            .HasQueryFilter(m => !m.SilindiMi);
        modelOlusturucu.Entity<Sertifika>()
            .HasQueryFilter(s => !s.SilindiMi);
        modelOlusturucu.Entity<Katalog>()
            .HasQueryFilter(k => !k.SilindiMi);
        modelOlusturucu.Entity<Sube>()
            .HasQueryFilter(s => !s.SilindiMi);
        modelOlusturucu.Entity<EkipUyesi>()
            .HasQueryFilter(e => !e.SilindiMi);

        // MedyaKlasoru self-referencing
        modelOlusturucu.Entity<MedyaKlasoru>()
            .HasMany(k => k.AltKlasorler)
            .WithOne(k => k.UstKlasor)
            .HasForeignKey(k => k.UstKlasorId)
            .OnDelete(DeleteBehavior.Restrict);

        // MedyaKullanim index
        modelOlusturucu.Entity<MedyaKullanim>()
            .HasIndex(m => new { m.EntiteAdi, m.EntiteId });

        modelOlusturucu.Entity<MedyaKullanim>()
            .HasIndex(m => m.MedyaId);

        // Baslangic verisi — yonetici
        modelOlusturucu.Entity<Kullanici>().HasData(new Kullanici
        {
            Id = 1,
            KullaniciAdi = "admin",
            SifreHash = "$2a$11$nt1W5l252hapG97qf8lIlOORhjfjq5RiX/pmTk.4tIZwuJrsuwslm",
            AdSoyad = "VIZITLINK3D Yonetici",
            Eposta = "admin@3dvizitlink.com.tr",
            Rol = Rol.SuperAdmin,
            EmailDogrulandiMi = true,
            AktifMi = true,
            OlusturulmaTarihi = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Baslangic verisi — Urun Ailesi ve Urun seed verisi, gercek ortamda
        // TohumVerisi sinifi uzerinden yapilir. HasData satirlari yeni veri tabani
        // olusturulurken EF migration'inin referans modelidir.
        // modelOlusturucu.Entity<UrunAilesi>().HasData(
        //     new UrunAilesi { Id = 1, Ad = "Duşakabin", Slug = "dusakabin", Aciklama = "Modern duşakabin sistemleri", VarsayilanDetaySablonu = "DusakabinKonfigurator", SiraNo = 1, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        //     new UrunAilesi { Id = 2, Ad = "Banyo Dolabı", Slug = "banyo-dolabi", Aciklama = "Banyo dolabı modelleri", VarsayilanDetaySablonu = "BanyoKonfigurator", SiraNo = 2, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        //     new UrunAilesi { Id = 3, Ad = "Vestiyer", Slug = "vestiyer", Aciklama = "Vestiyer sistemleri", VarsayilanDetaySablonu = "Endustriyel3D", SiraNo = 3, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        //     new UrunAilesi { Id = 4, Ad = "Kapı", Slug = "kapi", Aciklama = "İç ve dış kapı modelleri", VarsayilanDetaySablonu = "KapiKonfigurator", SiraNo = 4, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        //     new UrunAilesi { Id = 5, Ad = "Dolap Kapağı", Slug = "dolap-kapagi", Aciklama = "Mobilya kapak sistemleri", VarsayilanDetaySablonu = "KapakKonfigurator", SiraNo = 5, AktifMi = true, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        // );

        // Baslangic verisi — Ornek Urunler (yeni veritabani referansi)
        // modelOlusturucu.Entity<Urun>().HasData(
        //     new Urun { Id = 1, Slug = "dusakabin-luna", Kod = "DSK-001", Ad = "Luna Duşakabin", KisaAciklama = "Çerçevesiz temperli cam, krom profil", UrunAilesiId = 1, AktifMi = true, OneCikanMi = true, YeniMi = true, SiraNo = 1, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        //     new Urun { Id = 2, Slug = "banyo-dolabi-aria", Kod = "BD-001", Ad = "Aria Banyo Dolabı", KisaAciklama = "LED aydınlatmalı, yumuşak kapanır kapak", UrunAilesiId = 2, AktifMi = true, OneCikanMi = true, YeniMi = true, SiraNo = 1, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        //     new Urun { Id = 3, Slug = "kapi-imperial", Kod = "KPI-001", Ad = "Imperial Kapı", KisaAciklama = "Masif ahşap görünümlü, bronz kulp", UrunAilesiId = 4, AktifMi = true, OneCikanMi = true, SiraNo = 1, OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        // );



        // Urun Yonetimi (3D Konfigurator) — index + soft delete filtreleri + tenant FK
        modelOlusturucu.Entity<Urun>()
            .HasIndex(u => u.Slug).IsUnique();
        modelOlusturucu.Entity<Urun>()
            .HasQueryFilter(u => !u.SilindiMi);
        modelOlusturucu.Entity<Urun>()
            .HasOne(u => u.Firma)
            .WithMany()
            .HasForeignKey(u => u.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
        modelOlusturucu.Entity<Urun>()
            .HasIndex(u => new { u.FirmaId, u.Slug })
            .IsUnique()
            .HasFilter("[FirmaId] IS NOT NULL");
        modelOlusturucu.Entity<UrunAilesi>()
            .HasQueryFilter(a => !a.SilindiMi);
        modelOlusturucu.Entity<UrunKategori>()
            .HasQueryFilter(k => !k.SilindiMi);
        modelOlusturucu.Entity<UrunUcBoyutModeli>()
            .HasQueryFilter(m => !m.SilindiMi);
        modelOlusturucu.Entity<UrunUcBoyutSahneOnayari>()
            .HasQueryFilter(s => !s.SilindiMi);
        modelOlusturucu.Entity<UrunUcBoyutSahneOnayari>()
            .HasIndex(s => new { s.UrunUcBoyutModeliId, s.Kod })
            .IsUnique();
        modelOlusturucu.Entity<UrunUcBoyutSahneOnayari>()
            .HasOne(s => s.UrunUcBoyutModeli)
            .WithMany()
            .HasForeignKey(s => s.UrunUcBoyutModeliId)
            .OnDelete(DeleteBehavior.Restrict);
        modelOlusturucu.Entity<UrunParcaGrubu>()
            .HasQueryFilter(g => !g.SilindiMi);
        modelOlusturucu.Entity<UrunParcaGrubu>()
            .HasIndex(g => new { g.UrunId, g.Ad });
        modelOlusturucu.Entity<UrunMedya>()
            .HasQueryFilter(m => !m.SilindiMi);
        modelOlusturucu.Entity<Malzeme>()
            .HasQueryFilter(m => !m.SilindiMi);
        modelOlusturucu.Entity<KaplamaSecenegi>()
            .HasQueryFilter(k => !k.SilindiMi);
        modelOlusturucu.Entity<MusteriKonfigurasyonu>()
            .HasQueryFilter(m => !m.SilindiMi);
        modelOlusturucu.Entity<MusteriKonfigurasyonu>()
            .HasOne(k => k.Firma)
            .WithMany()
            .HasForeignKey(k => k.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
        modelOlusturucu.Entity<MusteriKonfigurasyonParcasi>()
            .HasQueryFilter(p => !p.SilindiMi);
        modelOlusturucu.Entity<TeklifIstegi>()
            .HasQueryFilter(t => !t.SilindiMi);
        modelOlusturucu.Entity<UrunUcBoyutParcasi>()
            .HasQueryFilter(p => !p.SilindiMi);
        modelOlusturucu.Entity<UrunUcBoyutParcasi>()
            .HasIndex(p => new { p.UrunUcBoyutModeliId, p.MantiksalKod })
            .IsUnique()
            .HasFilter("[MantiksalKod] IS NOT NULL");
        modelOlusturucu.Entity<UrunPdfKaynagi>()
            .HasQueryFilter(p => !p.SilindiMi);
        modelOlusturucu.Entity<PdfSayfaGorseli>()
            .HasQueryFilter(p => !p.SilindiMi);

        // === Tenant API Anahtari ===
        modelOlusturucu.Entity<FirmaApiAnahtari>()
            .HasIndex(a => a.ApiKeyHash)
            .IsUnique();
        modelOlusturucu.Entity<FirmaApiAnahtari>()
            .HasIndex(a => new { a.FirmaId, a.AnahtarAd })
            .IsUnique();
        modelOlusturucu.Entity<FirmaApiAnahtari>()
            .HasOne(a => a.Firma)
            .WithMany()
            .HasForeignKey(a => a.FirmaId)
            .OnDelete(DeleteBehavior.Restrict);
        modelOlusturucu.Entity<FirmaApiAnahtari>()
            .HasQueryFilter(a => !a.SilindiMi);

        // === Embed Oturum Nonce Kaydi ===
        modelOlusturucu.Entity<EmbedOturumNonceKaydi>()
            .HasIndex(n => n.NonceHash)
            .IsUnique();
        modelOlusturucu.Entity<EmbedOturumNonceKaydi>()
            .HasQueryFilter(n => !n.SilindiMi);

        // Bagimli entity'ler icin bos filter (EF global query filter tutarliligi)
        modelOlusturucu.Entity<KapiModeliResim>()
            .HasQueryFilter(r => true);
        modelOlusturucu.Entity<KapiModeliYerellestirme>()
            .HasQueryFilter(y => true);
        modelOlusturucu.Entity<MedyaKullanim>()
            .HasQueryFilter(k => true);
        modelOlusturucu.Entity<TeklifIstegiParcasi>()
            .HasQueryFilter(p => true);
        modelOlusturucu.Entity<UrunParcaEslemesi>()
            .HasQueryFilter(e => !e.SilindiMi);
        modelOlusturucu.Entity<RalRengi>()
            .HasQueryFilter(r => !r.SilindiMi);
        modelOlusturucu.Entity<IsTakipKaydi>()
            .HasQueryFilter(i => !i.SilindiMi);
        modelOlusturucu.Entity<TemaSablonu>()
            .HasQueryFilter(t => !t.SilindiMi);

        modelOlusturucu.Entity<TemaSablonu>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelOlusturucu.Entity<TemaSablonu>()
            .HasIndex(t => t.Kapsam);
    }

    public override int SaveChanges()
    {
        SoftDeleteUygula();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken iptal = default)
    {
        SoftDeleteUygula();
        return await base.SaveChangesAsync(iptal);
    }

    private void SoftDeleteUygula()
    {
        foreach (var giris in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted))
        {
            // SilindiMi ozelligi olan entity'leri soft delete yap
            var silindiMiProp = giris.Properties.FirstOrDefault(p => p.Metadata.Name == "SilindiMi");
            if (silindiMiProp != null)
            {
                giris.State = EntityState.Modified;
                silindiMiProp.CurrentValue = true;

                var silinmeTarihiProp = giris.Properties.FirstOrDefault(p => p.Metadata.Name == "SilinmeTarihi");
                if (silinmeTarihiProp != null)
                    silinmeTarihiProp.CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
