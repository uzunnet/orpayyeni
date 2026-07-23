using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class Paket2A_MultiTenantKonfigurator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DokuUygulanabilirMi",
                table: "UrunUcBoyutParcalari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HareketAyarlariJson",
                table: "UrunUcBoyutParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MantiksalKod",
                table: "UrunUcBoyutParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "UrunParcaGruplari",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellenmeTarihi",
                table: "UrunParcaGruplari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OlusturulmaTarihi",
                table: "UrunParcaGruplari",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "UrunParcaGruplari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "UrunParcaGruplari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "UrunParcaEslemeleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "UrunParcaEslemeleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UrunUcBoyutSahneOnayarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrunUcBoyutModeliId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    AyarlarJson = table.Column<string>(type: "TEXT", nullable: true),
                    VarsayilanMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunUcBoyutSahneOnayarlari", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrunUcBoyutParcalari_UrunUcBoyutModeliId_MantiksalKod",
                table: "UrunUcBoyutParcalari",
                columns: new[] { "UrunUcBoyutModeliId", "MantiksalKod" },
                unique: true,
                filter: "[MantiksalKod] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UrunParcaGruplari_UrunId_Ad",
                table: "UrunParcaGruplari",
                columns: new[] { "UrunId", "Ad" });

            migrationBuilder.CreateIndex(
                name: "IX_UrunUcBoyutSahneOnayarlari_UrunUcBoyutModeliId_Kod",
                table: "UrunUcBoyutSahneOnayarlari",
                columns: new[] { "UrunUcBoyutModeliId", "Kod" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrunUcBoyutSahneOnayarlari");

            migrationBuilder.DropIndex(
                name: "IX_UrunUcBoyutParcalari_UrunUcBoyutModeliId_MantiksalKod",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropIndex(
                name: "IX_UrunParcaGruplari_UrunId_Ad",
                table: "UrunParcaGruplari");

            migrationBuilder.DropColumn(
                name: "DokuUygulanabilirMi",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "HareketAyarlariJson",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "MantiksalKod",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "UrunParcaGruplari");

            migrationBuilder.DropColumn(
                name: "GuncellenmeTarihi",
                table: "UrunParcaGruplari");

            migrationBuilder.DropColumn(
                name: "OlusturulmaTarihi",
                table: "UrunParcaGruplari");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "UrunParcaGruplari");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "UrunParcaGruplari");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "UrunParcaEslemeleri");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "UrunParcaEslemeleri");
        }
    }
}
