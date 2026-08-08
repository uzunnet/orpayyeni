using Microsoft.Data.Sqlite;
var baglanti = new SqliteConnection("Data Source=F:\\orpay\\firmalar\\test-firma\\test-firma.db");
baglanti.Open();
var cmd = baglanti.CreateCommand();
cmd.CommandText = "SELECT Id, Eposta, KullaniciAdi, AdSoyad, Rol FROM Kullanicilar";
var reader = cmd.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"ID:{reader.GetInt32(0)} | Eposta:{reader.GetString(1)} | KullaniciAdi:{reader.IsDBNull(2)?"(null)":reader.GetString(2)} | AdSoyad:{reader.GetString(3)} | Rol:{reader.GetInt64(4)}");
}
baglanti.Close();
