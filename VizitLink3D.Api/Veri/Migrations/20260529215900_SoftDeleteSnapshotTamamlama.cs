using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteSnapshotTamamlama : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminOnayliMi",
                table: "UrunUcBoyutParcalari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MalzemeTipiKisiti",
                table: "UrunUcBoyutParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParcaTipi",
                table: "UrunUcBoyutParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellenmeTarihi",
                table: "UrunPdfKaynaklari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "UrunPdfKaynaklari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "UrunPdfKaynaklari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "SayfaIcerikleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "SayfaIcerikleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellenmeTarihi",
                table: "PdfSayfaGorselleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "PdfSayfaGorselleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "PdfSayfaGorselleri",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminOnayliMi",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "MalzemeTipiKisiti",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "ParcaTipi",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "GuncellenmeTarihi",
                table: "UrunPdfKaynaklari");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "UrunPdfKaynaklari");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "UrunPdfKaynaklari");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "SayfaIcerikleri");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "SayfaIcerikleri");

            migrationBuilder.DropColumn(
                name: "GuncellenmeTarihi",
                table: "PdfSayfaGorselleri");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "PdfSayfaGorselleri");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "PdfSayfaGorselleri");
        }
    }
}
