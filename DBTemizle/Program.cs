using Microsoft.Data.Sqlite;

var cs = "Data Source=I:\\orpay\\VizitLink3D.Api\\vizitlink3d.db";
using var conn = new SqliteConnection(cs);
conn.Open();

var tables = new[] {
    "Slaytlar", "SayfaIcerikleri", "GaleriGorselleri",
    "IletisimMesajlari", "CanliSohbetMesajlari",
    "KapakModeliResimleri", "KapakModeliYerellestirmeleri", "KapakModelleri",
    "KapiKategorisiYerellestirmeleri", "KapiKategorileri",
    "MobilyaKategorisiYerellestirmeleri", "MobilyaKategorileri",
    "MobilyaUrunuYerellestirmeleri", "MobilyaUrunleri",
    "ProjeResimleri", "Projeler", "ProjeKategorileri",
    "Referanslar", "MusteriYorumlari", "HizmetAdimlari",
    "SikSorulanSorular", "Sertifikalar", "Kataloglar",
    "BultenAboneleri", "EpostaSablonlari", "Subeler", "EkipUyeleri",
    "TanitimVideolari", "Haberler", "Kategoriler",
    "MedyaKullanimlari", "Medyalar", "MedyaKlasorleri",
    "UrunParcaRenkSecenekleri", "UrunParcaMalzemeSecenekleri",
    "UrunParcaEslemeleri", "UrunParcaGruplari",
    "UrunUcBoyutSahneOnayarlari", "UrunUcBoyutModelleri", "UrunUcBoyutParcalari",
    "UrunMedyalari", "UrunYerellestirmeleri",
    "UrunKonfigurasyonKurallari", "UrunKonfigurasyonSablonlari",
    "Urunler", "UrunKategorileri", "UrunAilesileri",
    "MusteriKonfigurasyonParcalari", "MusteriKonfigurasyonlari",
    "TeklifIstegiParcalari", "TeklifIstekleri",
    "UrunPdfKaynaklari", "PdfSayfaGorselleri",
    "RalRenkleri", "RenkKataloglari", "Malzemeler", "KaplamaSecenekleri",
    "AICagrisiKayitlari", "ZiyaretKayitlari", "AuditLoglar",
    "IsTakipKayitlari", "SayfaDuzenAyarlari",
    "EmbedOturumNonceKayitlari", "Ceviriler", "Lisanslar",
    "TemaRevizyonlari", "FirmaTemaAtamalari", "SistemAyarlari",
    "FirmaApiAnahtarlari"
};

var toplam = 0;
foreach (var t in tables)
{
    try
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM \"" + t + "\"";
        int etkilenen = cmd.ExecuteNonQuery();
        if (etkilenen > 0)
        {
            Console.WriteLine("  " + t + ": " + etkilenen + " satir silindi");
            toplam += etkilenen;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("  " + t + ": atlandi");
    }
}
Console.WriteLine("\nToplam " + toplam + " satir silindi.");
Console.WriteLine("Kalan: MenuOgeleri, Kullanicilar, Firmalar, TemaSablonlari, Diller");
