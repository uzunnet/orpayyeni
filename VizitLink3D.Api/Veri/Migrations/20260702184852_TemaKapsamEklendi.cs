using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class TemaKapsamEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kapsam",
                table: "TemaSablonlari",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_TemaSablonlari_Kapsam",
                table: "TemaSablonlari",
                column: "Kapsam");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TemaSablonlari_Kapsam",
                table: "TemaSablonlari");

            migrationBuilder.DropColumn(
                name: "Kapsam",
                table: "TemaSablonlari");
        }
    }
}
