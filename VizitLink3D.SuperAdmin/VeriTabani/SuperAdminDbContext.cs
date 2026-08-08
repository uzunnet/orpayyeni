using Microsoft.EntityFrameworkCore;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.SuperAdmin.VeriTabani;

public class SuperAdminDbContext : DbContext
{
    public SuperAdminDbContext(DbContextOptions<SuperAdminDbContext> secenekler) : base(secenekler) { }

    public DbSet<Firma> Firmalar => Set<Firma>();
    public DbSet<Modul> Moduller => Set<Modul>();
    public DbSet<FirmaModulAtama> FirmaModulAtamalari => Set<FirmaModulAtama>();
    public DbSet<Lisans> Lisanslar => Set<Lisans>();
    public DbSet<SuperAdminKullanici> SuperAdminKullanicilar => Set<SuperAdminKullanici>();
    public DbSet<SuperAdminLisansKaydi> SuperAdminLisansKayitlari => Set<SuperAdminLisansKaydi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Firma>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Domain).IsUnique();
        });

        modelBuilder.Entity<Modul>(entity =>
        {
            entity.HasIndex(e => e.Kod).IsUnique();
        });

        modelBuilder.Entity<SuperAdminKullanici>(entity =>
        {
            entity.HasIndex(e => e.KullaniciAdi).IsUnique();
        });

        modelBuilder.Entity<FirmaModulAtama>(entity =>
        {
            entity.HasOne(e => e.Firma)
                  .WithMany()
                  .HasForeignKey(e => e.FirmaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Modul)
                  .WithMany()
                  .HasForeignKey(e => e.ModulId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lisans>(entity =>
        {
            entity.HasOne(e => e.Firma)
                  .WithMany()
                  .HasForeignKey(e => e.FirmaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.FirmaId, e.AktifMi });
        });

        // Varsayilan SuperAdmin kullanicisi
        modelBuilder.Entity<SuperAdminKullanici>().HasData(
            new SuperAdminKullanici
            {
                Id = 1,
                KullaniciAdi = "admin",
                AdSoyad = "Super Admin",
                SifreHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin2026!"),
                AktifMi = true,
                OlusturulmaTarihi = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 24 varsayilan modul
        modelBuilder.Entity<Modul>().HasData(
            new Modul { Id = 1, Kod = "blog", Ad = "Blog", Kategori = "Icerik", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 2, Kod = "galeri", Ad = "Galeri", Kategori = "Icerik", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 3, Kod = "iletisim", Ad = "Iletisim Formu", Kategori = "Iletisim", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 4, Kod = "sohbet", Ad = "Canli Sohbet", Kategori = "Iletisim", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 5, Kod = "medya_havuzu", Ad = "Medya Havuzu", Kategori = "Medya", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 6, Kod = "ai_asistan", Ad = "AI Asistan", Kategori = "AI", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 7, Kod = "3d_goruntu", Ad = "3D Goruntu", Kategori = "Gorsel", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 8, Kod = "urunler", Ad = "Urun Yonetimi", Kategori = "E-Ticaret", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 9, Kod = "haberler", Ad = "Haber Yonetimi", Kategori = "Icerik", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 10, Kod = "sayfalar", Ad = "Sayfa Yonetimi", Kategori = "Icerik", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 11, Kod = "menu_yonetimi", Ad = "Menu Yonetimi", Kategori = "Yonetim", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 12, Kod = "tema_yonetimi", Ad = "Tema Yonetimi", Kategori = "Tasarim", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 13, Kod = "proje_yonetimi", Ad = "Proje Yonetimi", Kategori = "Is", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 14, Kod = "slayt_yonetimi", Ad = "Slayt Yonetimi", Kategori = "Icerik", VarsayilanMi = true, SistemModuluMu = false },
            new Modul { Id = 15, Kod = "referanslar", Ad = "Referanslar", Kategori = "Pazarlama", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 16, Kod = "sertifikalar", Ad = "Sertifikalar", Kategori = "Kurumsal", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 17, Kod = "sss", Ad = "SSS", Kategori = "Icerik", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 18, Kod = "katalog", Ad = "Katalog Yonetimi", Kategori = "Pazarlama", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 19, Kod = "bayi_yonetimi", Ad = "Bayi Yonetimi", Kategori = "Is", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 20, Kod = "ekip_yonetimi", Ad = "Ekip Yonetimi", Kategori = "Kurumsal", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 21, Kod = "pwa_offline", Ad = "PWA Offline", Kategori = "Teknik", VarsayilanMi = false, SistemModuluMu = false },
            new Modul { Id = 22, Kod = "audit_log", Ad = "Audit Log", Kategori = "Guvenlik", VarsayilanMi = true, SistemModuluMu = true },
            new Modul { Id = 23, Kod = "lisans_yonetimi", Ad = "Lisans Yonetimi", Kategori = "Sistem", VarsayilanMi = true, SistemModuluMu = true },
            new Modul { Id = 24, Kod = "bildirimler", Ad = "Bildirimler", Kategori = "Iletisim", VarsayilanMi = false, SistemModuluMu = false }
        );
    }
}
