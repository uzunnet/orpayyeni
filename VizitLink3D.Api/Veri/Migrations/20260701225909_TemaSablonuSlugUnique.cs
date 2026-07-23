using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class TemaSablonuSlugUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Her slug icin sadece en yeni (MAX Id) kaydi tut, digerlerini fiziksel sil.
            // SQLite filtered unique index desteklemedigi icin SilindiMi = 1 olanlari da kaldirmak zorundayiz.
            migrationBuilder.Sql(
                "DELETE FROM TemaSablonlari WHERE Id NOT IN (SELECT MAX(Id) FROM TemaSablonlari GROUP BY Slug)");

            migrationBuilder.CreateIndex(
                name: "IX_TemaSablonlari_Slug",
                table: "TemaSablonlari",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemaSablonlari_Slug",
                table: "TemaSablonlari");
        }
    }
}
