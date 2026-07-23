using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class Paket2A_TenantIzolasyonuVeSahneOnayariFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_UrunUcBoyutSahneOnayarlari_UrunUcBoyutModelleri_UrunUcBoyutModeliId",
                table: "UrunUcBoyutSahneOnayarlari",
                column: "UrunUcBoyutModeliId",
                principalTable: "UrunUcBoyutModelleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UrunUcBoyutSahneOnayarlari_UrunUcBoyutModelleri_UrunUcBoyutModeliId",
                table: "UrunUcBoyutSahneOnayarlari");
        }
    }
}
