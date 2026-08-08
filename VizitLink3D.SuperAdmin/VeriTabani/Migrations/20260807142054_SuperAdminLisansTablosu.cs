using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.SuperAdmin.VeriTabani.Migrations
{
    /// <inheritdoc />
    public partial class SuperAdminLisansTablosu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lisanslar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    BirincilDomain = table.Column<string>(type: "TEXT", nullable: false),
                    YedekDomain = table.Column<string>(type: "TEXT", nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LisansTipi = table.Column<string>(type: "TEXT", nullable: false),
                    SureYil = table.Column<int>(type: "INTEGER", nullable: true),
                    SuresizMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    DemoMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    LisansAnahtari = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SonDogrulamaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lisanslar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lisanslar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdminLisansKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: true),
                    Tip = table.Column<string>(type: "TEXT", nullable: false),
                    BaslangicTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdminLisansKayitlari", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "SuperAdminKullanicilar",
                keyColumn: "Id",
                keyValue: 1,
                column: "SifreHash",
                value: "$2a$11$dfuOoZM6ocXVAGxj2UuXru6BoKA9RdpyPtrNt4XBxWAr6Zb32DceG");

            migrationBuilder.CreateIndex(
                name: "IX_Lisanslar_FirmaId_AktifMi",
                table: "Lisanslar",
                columns: new[] { "FirmaId", "AktifMi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lisanslar");

            migrationBuilder.DropTable(
                name: "SuperAdminLisansKayitlari");

            migrationBuilder.UpdateData(
                table: "SuperAdminKullanicilar",
                keyColumn: "Id",
                keyValue: 1,
                column: "SifreHash",
                value: "$2a$11$oGvCQINSfviF8Uv9asWtAexTV/8P73fR5G1d4FWXsIj8X7M1R8Fe6");
        }
    }
}
