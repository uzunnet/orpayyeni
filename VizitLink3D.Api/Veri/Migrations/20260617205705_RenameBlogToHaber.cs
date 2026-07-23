using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VizitLink3D.Api.Veri.Migrations
{
    /// <inheritdoc />
    public partial class RenameBlogToHaber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogResim");

            migrationBuilder.DropTable(
                name: "BlogYazilari");

            migrationBuilder.CreateTable(
                name: "Haberler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Ozet = table.Column<string>(type: "TEXT", nullable: false),
                    Icerik = table.Column<string>(type: "TEXT", nullable: false),
                    AnaResimUrl = table.Column<string>(type: "TEXT", nullable: false),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Etiketler = table.Column<string>(type: "TEXT", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    OkunmaSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Haberler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Haberler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HaberResim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HaberYazisiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResimUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HaberResim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HaberResim_Haberler_HaberYazisiId",
                        column: x => x.HaberYazisiId,
                        principalTable: "Haberler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Haberler_FirmaId",
                table: "Haberler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_HaberResim_HaberYazisiId",
                table: "HaberResim",
                column: "HaberYazisiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HaberResim");

            migrationBuilder.DropTable(
                name: "Haberler");

            migrationBuilder.CreateTable(
                name: "BlogYazilari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnaResimUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false),
                    Etiketler = table.Column<string>(type: "TEXT", nullable: true),
                    Icerik = table.Column<string>(type: "TEXT", nullable: false),
                    OkunmaSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ozet = table.Column<string>(type: "TEXT", nullable: false),
                    SeoAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SeoBaslik = table.Column<string>(type: "TEXT", nullable: true),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogYazilari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogYazilari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BlogResim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlogYazisiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResimUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Sira = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogResim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogResim_BlogYazilari_BlogYazisiId",
                        column: x => x.BlogYazisiId,
                        principalTable: "BlogYazilari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogResim_BlogYazisiId",
                table: "BlogResim",
                column: "BlogYazisiId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogYazilari_FirmaId",
                table: "BlogYazilari",
                column: "FirmaId");
        }
    }
}
