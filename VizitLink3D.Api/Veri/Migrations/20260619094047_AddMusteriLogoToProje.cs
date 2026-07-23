using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class AddMusteriLogoToProje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MusteriLogo",
                table: "Projeler",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SayfaDuzenAyarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SayfaKodu = table.Column<string>(type: "TEXT", nullable: false),
                    SayfaAdi = table.Column<string>(type: "TEXT", nullable: false),
                    SutunAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    SatirAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    SayfaBasinaAdet = table.Column<int>(type: "INTEGER", nullable: false),
                    SayfalamaAktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SayfaDuzenAyarlari", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SayfaDuzenAyarlari");

            migrationBuilder.DropColumn(
                name: "MusteriLogo",
                table: "Projeler");
        }
    }
}
