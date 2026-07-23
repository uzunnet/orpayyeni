using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class UrunFirmaIdVeSahneOnayEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminOnayliMi",
                table: "UrunUcBoyutSahneOnayarlari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "Urunler",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_FirmaId_Slug",
                table: "Urunler",
                columns: new[] { "FirmaId", "Slug" },
                unique: true,
                filter: "[FirmaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Urunler_Firmalar_FirmaId",
                table: "Urunler",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Urunler_Firmalar_FirmaId",
                table: "Urunler");

            migrationBuilder.DropIndex(
                name: "IX_Urunler_FirmaId_Slug",
                table: "Urunler");

            migrationBuilder.DropColumn(
                name: "AdminOnayliMi",
                table: "UrunUcBoyutSahneOnayarlari");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "Urunler");
        }
    }
}
