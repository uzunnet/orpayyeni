using Microsoft.AspNetCore.DataProtection;
using System.Text.RegularExpressions;

namespace VizitLink3D.Api.Moduller.AI.Servisler;

public partial class AIGuvenlikServisi
{
    private readonly IDataProtector _koruyucu;

    public AIGuvenlikServisi(IDataProtectionProvider saglayici)
    {
        _koruyucu = saglayici.CreateProtector("VizitLink3D.AI.ApiKey");
    }

    public string ApiKeySifrele(string apiKey)
    {
        return _koruyucu.Protect(apiKey);
    }

    public string ApiKeyCoz(string encryptedKey)
    {
        return _koruyucu.Unprotect(encryptedKey);
    }

    public string PIIFiltrele(string metin)
    {
        if (string.IsNullOrWhiteSpace(metin)) return metin;

        // TC Kimlik No (11 haneli)
        metin = TcKimlikRegex().Replace(metin, "***TC_KIMLIK***");

        // Telefon (Türkiye formatı)
        metin = TelefonRegex().Replace(metin, "***TELEFON***");

        // E-posta
        metin = EpostaRegex().Replace(metin, "***EPOSTA***");

        // Kredi kartı
        metin = KrediKartRegex().Replace(metin, "***KREDI_KARTI***");

        return metin;
    }

    [GeneratedRegex(@"\b[1-9]\d{10}\b")]
    private static partial Regex TcKimlikRegex();

    [GeneratedRegex(@"\b(0[5-7]\d{2})\s?\d{3}\s?\d{2}\s?\d{2}\b")]
    private static partial Regex TelefonRegex();

    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}")]
    private static partial Regex EpostaRegex();

    [GeneratedRegex(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b")]
    private static partial Regex KrediKartRegex();
}
