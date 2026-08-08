var db = "F:/orpay/VizitLink3D.SuperAdmin/firmalar/orpay/orpay.db";
using var conn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=" + db);
conn.Open();

// Oncelikle toplam sayiyi ve her Baslik'tan kac tane oldugunu goster
var cmd = conn.CreateCommand();
cmd.CommandText = "SELECT COUNT(*) FROM MenuOgeleri WHERE Konum = 'AdminSol' AND SilindiMi = 0;";
Console.WriteLine("Toplam menu (Aktif): " + cmd.ExecuteScalar());

cmd.CommandText = "SELECT COUNT(*) FROM MenuOgeleri WHERE Konum = 'AdminSol';";
Console.WriteLine("Toplam menu (Tumu): " + cmd.ExecuteScalar());

// Baslik bazli tekrar
Console.WriteLine("\nBaslik bazli dagilim:");
cmd.CommandText = "SELECT Baslik, COUNT(*) as Adet FROM MenuOgeleri WHERE Konum = 'AdminSol' GROUP BY Baslik ORDER BY Adet DESC LIMIT 20;";
using var r = cmd.ExecuteReader();
while (r.Read())
{
    var adet = r.GetInt32(1);
    if (adet > 1)
        Console.WriteLine($"  ⚠️ {r.GetString(0)}: {adet} adet");
}
r.Close();

// Silinmis menuler var mi?
Console.WriteLine("\nSilinmis menuler:");
cmd.CommandText = "SELECT COUNT(*) FROM MenuOgeleri WHERE SilindiMi = 1;";
Console.WriteLine("  SilindiMi=1: " + cmd.ExecuteScalar());
