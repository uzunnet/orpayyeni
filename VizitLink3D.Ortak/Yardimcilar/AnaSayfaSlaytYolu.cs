namespace VizitLink3D.Ortak.Yardimcilar;

public static class AnaSayfaSlaytYolu
{
    private static readonly IReadOnlyDictionary<string, string> GuncelYollar =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["/medya/anasayfa-slayt-1.png"] = "/medya/anasayfa-slayt-1.webp",
            ["/medya/anasayfa-slayt-2.png"] = "/medya/anasayfa-slayt-2.webp",
            ["/medya/anasayfa-slayt-3.png"] = "/medya/anasayfa-slayt-3.webp"
        };

    public static string Guncelle(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return string.Empty;

        var temizYol = yol.Trim();
        return GuncelYollar.TryGetValue(temizYol, out var guncelYol)
            ? guncelYol
            : temizYol;
    }
}
