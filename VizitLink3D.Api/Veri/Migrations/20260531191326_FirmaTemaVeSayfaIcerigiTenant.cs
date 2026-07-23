using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class FirmaTemaVeSayfaIcerigiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SayfaIcerikleri_Bolum_Anahtar_Dil",
                table: "SayfaIcerikleri");

            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "SayfaIcerikleri",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminTema",
                table: "Firmalar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteTema",
                table: "Firmalar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE SayfaIcerikleri
                SET FirmaId = (SELECT Id FROM Firmalar WHERE Slug = 'vizitlink3d' LIMIT 1)
                WHERE FirmaId IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE Firmalar
                SET AdminTema = COALESCE(AdminTema, 'endustri-karanlik'),
                    SiteTema = COALESCE(SiteTema, 'endustri-karanlik')
                WHERE Slug = 'vizitlink3d';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_SayfaIcerikleri_FirmaId_Bolum_Anahtar_Dil",
                table: "SayfaIcerikleri",
                columns: new[] { "FirmaId", "Bolum", "Anahtar", "Dil" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SayfaIcerikleri_Firmalar_FirmaId",
                table: "SayfaIcerikleri",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SayfaIcerikleri_Firmalar_FirmaId",
                table: "SayfaIcerikleri");

            migrationBuilder.DropIndex(
                name: "IX_SayfaIcerikleri_FirmaId_Bolum_Anahtar_Dil",
                table: "SayfaIcerikleri");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "SayfaIcerikleri");

            migrationBuilder.DropColumn(
                name: "AdminTema",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "SiteTema",
                table: "Firmalar");

            migrationBuilder.CreateIndex(
                name: "IX_SayfaIcerikleri_Bolum_Anahtar_Dil",
                table: "SayfaIcerikleri",
                columns: new[] { "Bolum", "Anahtar", "Dil" },
                unique: true);
        }
    }
}
