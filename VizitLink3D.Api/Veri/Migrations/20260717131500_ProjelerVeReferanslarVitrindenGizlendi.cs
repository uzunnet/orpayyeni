using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VizitLink3D.Api.VeriTabani;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    [DbContext(typeof(VizitLink3DDbContext))]
    [Migration("20260717131500_ProjelerVeReferanslarVitrindenGizlendi")]
    public partial class ProjelerVeReferanslarVitrindenGizlendi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "MenuOgeleri"
                SET "AktifMi" = 0
                WHERE "SilindiMi" = 0
                  AND "Konum" IN ('PublicHeader', 'PublicMobil', 'PublicFooterHizli')
                  AND LOWER(TRIM("Url", '/')) IN ('projeler', 'referanslar');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "MenuOgeleri"
                SET "AktifMi" = 1
                WHERE "SilindiMi" = 0
                  AND "Konum" IN ('PublicHeader', 'PublicMobil', 'PublicFooterHizli')
                  AND LOWER(TRIM("Url", '/')) IN ('projeler', 'referanslar');
                """);
        }
    }
}
