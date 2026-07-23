using System.Text.RegularExpressions;

namespace VizitLink3D.Api.Servisler;

public interface IPIIFiltreServisi
{
    string Filtrele(string metin);
}

public partial class PIIFiltreServisi : IPIIFiltreServisi
{
    [GeneratedRegex(@"\b\d{11}\b")]
    private static partial Regex TcKimlikRegex();

    [GeneratedRegex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b\d{10}\b")]
    private static partial Regex TelefonRegex();

    [GeneratedRegex(@"sk-[A-Za-z0-9]{20,}")]
    private static partial Regex ApiKeyRegex();

    public string Filtrele(string metin)
    {
        if (string.IsNullOrEmpty(metin)) return metin;

        metin = TcKimlikRegex().Replace(metin, "[TC_KIMLIK_GIZLI]");
        metin = EmailRegex().Replace(metin, "[EPOSTA_GIZLI]");
        metin = TelefonRegex().Replace(metin, "[TELEFON_GIZLI]");
        metin = ApiKeyRegex().Replace(metin, "[API_KEY_GIZLI]");

        return metin;
    }
}
