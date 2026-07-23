using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Ortak.Yardimcilar;

public static class MedyaHavuzuYolu
{
    private const string DosyaYoluOnEki = "/api/medya/dosya/";
    private const string OrpayKatalogYoluOnEki = "/medya/";

    public static bool HavuzDosyaYoluMu(string? yol)
    {
        return MedyaIdBul(yol).HasValue;
    }

    public static string TamUrl(string yol, string apiTabanUrl)
    {
        var medyaId = MedyaIdBul(yol)
            ?? throw new ArgumentException("Geçerli bir medya havuzu dosya yolu gerekli.", nameof(yol));

        return $"{apiTabanUrl.TrimEnd('/')}{DosyaYoluOnEki}{medyaId}";
    }

    public static List<string> UrunGalerisiOlustur(
        Urun urun,
        IEnumerable<UrunMedya> medyalar,
        string apiTabanUrl)
    {
        ArgumentNullException.ThrowIfNull(urun);
        ArgumentNullException.ThrowIfNull(medyalar);

        var etkinMedyalar = medyalar
            .Where(m => !m.SilindiMi)
            .Where(m => m.MedyaTuru.Equals("Gorsel", StringComparison.OrdinalIgnoreCase)
                || m.MedyaTuru.Equals("Resim", StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.SiraNo)
            .ToList();

        var sonuc = new List<string>();

        if (etkinMedyalar.Count == 0 && urun.AnaGorselMedyaId is > 0)
            sonuc.Add($"{apiTabanUrl.TrimEnd('/')}{DosyaYoluOnEki}{urun.AnaGorselMedyaId.Value}");

        sonuc.AddRange(etkinMedyalar
            .Select(m => GaleriUrlBul(m.MedyaUrl, apiTabanUrl))
            .Where(yol => !string.IsNullOrWhiteSpace(yol))
            .Select(yol => yol!));

        return sonuc.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static long? MedyaIdBul(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
            return null;

        var temizYol = yol.Trim();
        if (Uri.TryCreate(temizYol, UriKind.Absolute, out var tamAdres))
            temizYol = tamAdres.AbsolutePath;

        if (!temizYol.StartsWith(DosyaYoluOnEki, StringComparison.OrdinalIgnoreCase))
            return null;

        var idMetni = temizYol[DosyaYoluOnEki.Length..];
        return long.TryParse(idMetni, out var medyaId) && medyaId > 0 ? medyaId : null;
    }

    private static string? GaleriUrlBul(string? yol, string apiTabanUrl)
    {
        var medyaId = MedyaIdBul(yol);
        if (medyaId.HasValue)
            return $"{apiTabanUrl.TrimEnd('/')}{DosyaYoluOnEki}{medyaId.Value}";

        if (string.IsNullOrWhiteSpace(yol))
            return null;

        var temizYol = yol.Trim();
        return temizYol.StartsWith(OrpayKatalogYoluOnEki, StringComparison.OrdinalIgnoreCase)
            ? temizYol
            : null;
    }
}
