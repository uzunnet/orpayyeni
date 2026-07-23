namespace VizitLink3D.Ortak.Yardimcilar;

public static class KatalogYolu
{
    private const string GenelKatalogKoku = "medya/";

    public static string? GuvenliGenelKatalogYolu(string? yol)
    {
        if (string.IsNullOrWhiteSpace(yol))
        {
            return null;
        }

        string temizYol;
        try
        {
            temizYol = Uri.UnescapeDataString(yol)
                .Replace('\\', '/')
                .TrimStart('/');
        }
        catch (UriFormatException)
        {
            return null;
        }

        if (!temizYol.StartsWith(GenelKatalogKoku, StringComparison.OrdinalIgnoreCase)
            || !Path.GetExtension(temizYol).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            || temizYol.Split('/').Any(parca => parca is "." or ".."))
        {
            return null;
        }

        return temizYol;
    }
}
