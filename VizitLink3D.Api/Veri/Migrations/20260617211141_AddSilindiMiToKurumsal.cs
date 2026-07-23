using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class AddSilindiMiToKurumsal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "Subeler",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "Subeler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "Sertifikalar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "Sertifikalar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "Kataloglar",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "Kataloglar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "EkipUyeleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "EkipUyeleri",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "Subeler");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "Subeler");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "Sertifikalar");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "Sertifikalar");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "Kataloglar");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "Kataloglar");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "EkipUyeleri");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "EkipUyeleri");
        }
    }
}
