using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class RalRengiSoftDeleteEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GuncellenmeTarihi",
                table: "RalRenkleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "RalRenkleri",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SilinmeTarihi",
                table: "RalRenkleri",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuncellenmeTarihi",
                table: "RalRenkleri");

            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "RalRenkleri");

            migrationBuilder.DropColumn(
                name: "SilinmeTarihi",
                table: "RalRenkleri");
        }
    }
}
