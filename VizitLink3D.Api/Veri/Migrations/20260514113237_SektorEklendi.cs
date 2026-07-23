using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class SektorEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sektorler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(type: "TEXT", nullable: false),
                    Kod = table.Column<string>(type: "TEXT", nullable: false),
                    Ikon = table.Column<string>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sektorler", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sektorler");
        }
    }
}
