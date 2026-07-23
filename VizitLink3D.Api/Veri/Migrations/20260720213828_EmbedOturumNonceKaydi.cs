using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class EmbedOturumNonceKaydi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmbedOturumNonceKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NonceHash = table.Column<string>(type: "TEXT", nullable: false),
                    SonKullanmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SilindiMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    SilinmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmbedOturumNonceKayitlari", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmbedOturumNonceKayitlari_NonceHash",
                table: "EmbedOturumNonceKayitlari",
                column: "NonceHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmbedOturumNonceKayitlari");
        }
    }
}
