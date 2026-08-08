using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.SuperAdmin.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class SuperAdminKullaniciEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuperAdminKullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciAdi = table.Column<string>(type: "TEXT", nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", nullable: false),
                    SifreHash = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdminKullanicilar", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SuperAdminKullanicilar",
                columns: new[] { "Id", "AdSoyad", "AktifMi", "KullaniciAdi", "OlusturulmaTarihi", "SifreHash" },
                values: new object[] { 1, "Super Admin", true, "admin", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$oGvCQINSfviF8Uv9asWtAexTV/8P73fR5G1d4FWXsIj8X7M1R8Fe6" });

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminKullanicilar_KullaniciAdi",
                table: "SuperAdminKullanicilar",
                column: "KullaniciAdi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuperAdminKullanicilar");
        }
    }
}
