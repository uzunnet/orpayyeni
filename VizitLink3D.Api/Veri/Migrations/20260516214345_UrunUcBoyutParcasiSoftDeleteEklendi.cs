using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class UrunUcBoyutParcasiSoftDeleteEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellenmeTarihi",
                table: "UrunUcBoyutParcalari",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OlusturulmaTarihi",
                table: "UrunUcBoyutParcalari",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "UrunUcBoyutParcalari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "UrunUcBoyutParcalari",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuncellenmeTarihi",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "OlusturulmaTarihi",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "UrunUcBoyutParcalari");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "UrunUcBoyutParcalari");
        }
    }
}
