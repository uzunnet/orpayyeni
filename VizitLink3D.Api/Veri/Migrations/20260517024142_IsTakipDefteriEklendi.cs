using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class IsTakipDefteriEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IsTakipKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<string>(type: "TEXT", nullable: false),
                    Oncelik = table.Column<string>(type: "TEXT", nullable: false),
                    Kategori = table.Column<string>(type: "TEXT", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsTakipKayitlari", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IsTakipKayitlari");
        }
    }
}
