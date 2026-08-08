using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Migrations
{
    /// <inheritdoc />
    public partial class FirmaAlanlariEkleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AktifModulKodlariJson",
                table: "Firmalar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxKullaniciSayisi",
                table: "Firmalar",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MedyaKlasoru",
                table: "Firmalar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaketTipi",
                table: "Firmalar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sektor",
                table: "Firmalar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrunMedyalari_UrunId",
                table: "UrunMedyalari",
                column: "UrunId");

            migrationBuilder.AddForeignKey(
                name: "FK_UrunMedyalari_Urunler_UrunId",
                table: "UrunMedyalari",
                column: "UrunId",
                principalTable: "Urunler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrunMedyalari_Urunler_UrunId",
                table: "UrunMedyalari");

            migrationBuilder.DropIndex(
                name: "IX_UrunMedyalari_UrunId",
                table: "UrunMedyalari");

            migrationBuilder.DropColumn(
                name: "AktifModulKodlariJson",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "MaxKullaniciSayisi",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "MedyaKlasoru",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "PaketTipi",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "Sektor",
                table: "Firmalar");
        }
    }
}
