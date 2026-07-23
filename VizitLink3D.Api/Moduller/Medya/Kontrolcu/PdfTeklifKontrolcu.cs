using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace VizitLink3D.Api.Moduller.Medya.Kontrolcu;

[ApiController]
[Route("api/teklif")]
public class PdfTeklifKontrolcu : ControllerBase
{
    public PdfTeklifKontrolcu()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [HttpPost("pdf")]
    public IActionResult PdfTeklifOlustur([FromBody] PdfTeklifIstegi istek)
    {
        var belge = Document.Create(konteyner =>
        {
            konteyner.Page(sayfa =>
            {
                sayfa.Size(PageSizes.A4);
                sayfa.Margin(40);
                sayfa.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                sayfa.Header()
                    .Row(row =>
                    {
                        row.RelativeItem().Text("VizitLink3D").Bold().FontSize(20);
                        row.ConstantItem(120).Text($"Tarih: {DateTime.UtcNow:dd.MM.yyyy}").FontSize(9);
                    });

                sayfa.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        col.Item().Text("TEKLİF FORMU").Bold().FontSize(16).FontColor("#1A1A27");
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#C8952A");

                        col.Item().Text($"Model: {istek.ModelAdi} ({istek.ModelKodu})").Bold();
                        col.Item().Text($"Renk: {istek.RenkAdi} ({istek.RenkKodu})");
                        col.Item().Text($"Malzeme: {istek.Malzeme}");
                        col.Item().Text($"Genişlik: {istek.GenislikMm} mm");
                        col.Item().Text($"Yükseklik: {istek.YukseklikMm} mm");

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("İletişim Bilgileri:").Bold();
                        col.Item().Text($"Ad Soyad: {istek.AdSoyad}");
                        col.Item().Text($"E-posta: {istek.Eposta}");
                        col.Item().Text($"Telefon: {istek.Telefon}");

                        col.Item().PaddingVertical(15).Text("Bu teklif otomatik oluşturulmuştur. En kısa sürede sizinle iletişime geçilecektir.").FontSize(9).Italic();
                    });

                sayfa.Footer()
                    .AlignCenter()
                    .Text("VizitLink3D © 2024 — Ankara Akyurt | www.3dvizitlink.com.tr").FontSize(8);
            });
        });

        var pdf = belge.GeneratePdf();
        return File(pdf, "application/pdf", $"VizitLink3D_Teklif_{istek.ModelKodu}.pdf");
    }

    public class PdfTeklifIstegi
    {
        public string ModelAdi { get; set; } = "";
        public string ModelKodu { get; set; } = "";
        public string RenkAdi { get; set; } = "";
        public string RenkKodu { get; set; } = "";
        public string Malzeme { get; set; } = "";
        public int GenislikMm { get; set; }
        public int YukseklikMm { get; set; }
        public string AdSoyad { get; set; } = "";
        public string Eposta { get; set; } = "";
        public string Telefon { get; set; } = "";
    }
}
