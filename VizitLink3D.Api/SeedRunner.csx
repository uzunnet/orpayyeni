using Microsoft.Data.Sqlite;

var dbPath = args.Length > 0 ? args[0] : "desadoor.db";
var sqlPath = args.Length > 1 ? args[1] : "tohum_verisi_ek.sql";

if (!File.Exists(dbPath)) { Console.WriteLine($"DB bulunamadı: {dbPath}"); return 1; }
if (!File.Exists(sqlPath)) { Console.WriteLine($"SQL bulunamadı: {sqlPath}"); return 1; }

using var conn = new SqliteConnection($"Data Source={dbPath}");
conn.Open();

var sql = File.ReadAllText(sqlPath);
using var cmd = conn.CreateCommand();
cmd.CommandText = sql;

try
{
    var rows = cmd.ExecuteNonQuery();
    Console.WriteLine($"OK: {rows} satır etkilendi");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"HATA: {ex.Message}");
    return 1;
}
