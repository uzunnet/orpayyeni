using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class LisansDomainGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Lisanslar
                SET BirincilDomain = 'vizitlink3d.uzunreklam.com',
                    YedekDomain = 'www.vizitlink3d.uzunreklam.com',
                    GuncellenmeTarihi = CURRENT_TIMESTAMP
                WHERE FirmaId IN (SELECT Id FROM Firmalar WHERE Slug = 'vizitlink3d')
                  AND BirincilDomain = '3dvizitlink.com.tr'
                  AND AktifMi = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Lisanslar
                SET BirincilDomain = '3dvizitlink.com.tr',
                    YedekDomain = 'www.3dvizitlink.com.tr',
                    GuncellenmeTarihi = CURRENT_TIMESTAMP
                WHERE FirmaId IN (SELECT Id FROM Firmalar WHERE Slug = 'vizitlink3d')
                  AND BirincilDomain = 'vizitlink3d.uzunreklam.com'
                  AND AktifMi = 1;
                """);
        }
    }
}
