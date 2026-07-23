using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class IcerikEntitylerineSoftDeleteEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "Slaytlar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "Slaytlar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "SikSorulanSorular",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "SikSorulanSorular",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "Referanslar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "Referanslar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "MusteriYorumlari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "MusteriYorumlari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "HizmetAdimlari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "HizmetAdimlari",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "Slaytlar");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "Slaytlar");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "SikSorulanSorular");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "SikSorulanSorular");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "Referanslar");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "Referanslar");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "MusteriYorumlari");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "MusteriYorumlari");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "HizmetAdimlari");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "HizmetAdimlari");
        }
    }
}
