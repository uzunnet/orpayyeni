using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class LisansPlanlariVeDemoModu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DemoMu",
                table: "Lisanslar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SureYil",
                table: "Lisanslar",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SuresizMi",
                table: "Lisanslar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE Lisanslar
                SET SuresizMi = 1,
                    SureYil = NULL,
                    LisansTipi = 'Suresiz'
                WHERE LisansTipi IN ('Omurboyu', 'Suresiz');
                """);

            migrationBuilder.Sql("""
                UPDATE Lisanslar
                SET SureYil = 1
                WHERE LisansTipi = 'Yillik' AND SureYil IS NULL;
                """);

            migrationBuilder.Sql("""
                INSERT INTO Lisanslar
                    (FirmaId, BirincilDomain, YedekDomain, BaslangicTarihi, BitisTarihi, LisansTipi, LisansAnahtari, AktifMi, SonDogrulamaTarihi, OlusturulmaTarihi, GuncellenmeTarihi, DemoMu, SureYil, SuresizMi)
                SELECT f.Id, COALESCE(f.Domain, '3dvizitlink.com.tr'), f.YedekDomain, '2026-01-01 00:00:00', '9999-12-31 00:00:00', 'Suresiz', '', 1, NULL, CURRENT_TIMESTAMP, NULL, 0, NULL, 1
                FROM Firmalar f
                WHERE f.Slug = 'vizitlink3d'
                  AND NOT EXISTS (SELECT 1 FROM Lisanslar l WHERE l.FirmaId = f.Id);
                """);

            migrationBuilder.Sql("""
                UPDATE MenuOgeleri
                SET Sira = Sira + 1
                WHERE Konum = 'AdminSol'
                  AND UstMenuId = (SELECT Id FROM MenuOgeleri WHERE Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Sistem' AND SilindiMi = 0 LIMIT 1)
                  AND Sira >= 5
                  AND SilindiMi = 0;
                """);

            migrationBuilder.Sql("""
                INSERT INTO MenuOgeleri
                    (FirmaId, Baslik, Url, UstMenuId, Sira, AktifMi, YeniSekmede, Ikon, Konum, GerekliRol, SuperAdminGerekliMi, YetkiAnahtari, KilitliMi, SistemMenusuMu, SilindiMi, SilinmeTarihi, OlusturulmaTarihi, GuncellenmeTarihi)
                SELECT NULL, 'Lisans Yonetimi', 'admin/lisans-yonetimi',
                    (SELECT Id FROM MenuOgeleri WHERE Konum = 'AdminSol' AND UstMenuId IS NULL AND Baslik = 'Sistem' AND SilindiMi = 0 LIMIT 1),
                    5, 1, 0, 'VerifiedUser', 'AdminSol', NULL, 1, NULL, 0, 0, 0, NULL, CURRENT_TIMESTAMP, NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM MenuOgeleri
                    WHERE Konum = 'AdminSol' AND Url = 'admin/lisans-yonetimi' AND SilindiMi = 0
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoMu",
                table: "Lisanslar");

            migrationBuilder.DropColumn(
                name: "SureYil",
                table: "Lisanslar");

            migrationBuilder.DropColumn(
                name: "SuresizMi",
                table: "Lisanslar");
        }
    }
}
