using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using VizitLink3D.Api.VeriTabani;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    [DbContext(typeof(VizitLink3DDbContext))]
    [Migration("20260715150000_UrunMedyayaSilindiMiEklendi")]
    public partial class AddSilindiMiToUrunMedya : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SilindiMi",
                table: "UrunMedyalari",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SilindiMi",
                table: "UrunMedyalari");
        }
    }
}
