using FluentValidation;
using VizitLink3D.Api.Moduller.Urunler.Dtolar;
using VizitLink3D.Ortak.Modeller.Urunler;
using System.Text.Json;

namespace VizitLink3D.Api.Moduller.Urunler.Dogrulayicilar;

/// <summary>
/// Parça upsert validasyonu: MeshAdi zorunlu, GorunenAd zorunlu,
/// MantiksalKod ASCII, HareketTipi geçerli enum, HareketAyarlariJson geçerli JSON.
/// </summary>
public class UcBoyutParcaUpsertDogrulayici : AbstractValidator<UcBoyutParcaUpsertDto>
{
    public UcBoyutParcaUpsertDogrulayici()
    {
        RuleFor(x => x.MeshAdi)
            .NotEmpty().WithMessage("Mesh adı zorunludur.")
            .MaximumLength(256).WithMessage("Mesh adı en fazla 256 karakter olabilir.");

        RuleFor(x => x.GorunenAd)
            .NotEmpty().WithMessage("Görünen ad zorunludur.")
            .MaximumLength(256).WithMessage("Görünen ad en fazla 256 karakter olabilir.");

        RuleFor(x => x.MantiksalKod)
            .MaximumLength(128).WithMessage("Mantıksal kod en fazla 128 karakter olabilir.")
            .Matches(@"^[a-zA-Z0-9\-_]*$").WithMessage("Mantıksal kod sadece ASCII harf, rakam, tire ve alt çizgi içerebilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.MantiksalKod));

        RuleFor(x => x.HareketTipi)
            .Must(ht => string.IsNullOrWhiteSpace(ht) || Enum.TryParse<HareketTuru>(ht, out _))
            .WithMessage($"Geçersiz hareket tipi. Geçerli değerler: {string.Join(", ", Enum.GetNames<HareketTuru>())}");

        RuleFor(x => x.HareketAyarlariJson)
            .Must(GecerliJsonMi)
            .WithMessage("Hareket ayarları geçerli bir JSON olmalıdır.")
            .When(x => !string.IsNullOrWhiteSpace(x.HareketAyarlariJson));

        RuleFor(x => x.SiraNo)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra numarası 0 veya daha büyük olmalıdır.");
    }

    private static bool GecerliJsonMi(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try { JsonDocument.Parse(json); return true; }
        catch { return false; }
    }
}

/// <summary>
/// Toplu parça upsert validasyonu.
/// </summary>
public class UcBoyutParcaTopluUpsertDogrulayici : AbstractValidator<UcBoyutParcaTopluUpsertDto>
{
    public UcBoyutParcaTopluUpsertDogrulayici()
    {
        RuleFor(x => x.Parcalar)
            .NotEmpty().WithMessage("En az bir parça gönderilmelidir.");

        RuleForEach(x => x.Parcalar)
            .SetValidator(new UcBoyutParcaUpsertDogrulayici());

        RuleFor(x => x.Parcalar)
            .Must(p => p.Select(x => x.MeshAdi).Distinct(StringComparer.OrdinalIgnoreCase).Count() == p.Count)
            .WithMessage("Aynı mesh adı birden fazla kez gönderilemez.");
    }
}

/// <summary>
/// Grup DTO validasyonu.
/// </summary>
public class UcBoyutGrupDogrulayici : AbstractValidator<UcBoyutGrupDto>
{
    public UcBoyutGrupDogrulayici()
    {
        RuleFor(x => x.Ad)
            .NotEmpty().WithMessage("Grup adı zorunludur.")
            .MaximumLength(128).WithMessage("Grup adı en fazla 128 karakter olabilir.");

        RuleFor(x => x.SiraNo)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra numarası 0 veya daha büyük olmalıdır.");
    }
}

/// <summary>
/// Sahne önayarı DTO validasyonu:
/// - Ad ve Kod zorunlu
/// - Kod ASCII slug formatında
/// - AyarlarJson geçerli JSON, script/HTML kabul edilmez
/// </summary>
public class UcBoyutSahneOnayariDogrulayici : AbstractValidator<UcBoyutSahneOnayariDto>
{
    public UcBoyutSahneOnayariDogrulayici()
    {
        RuleFor(x => x.Ad)
            .NotEmpty().WithMessage("Önayar adı zorunludur.")
            .MaximumLength(128).WithMessage("Önayar adı en fazla 128 karakter olabilir.");

        RuleFor(x => x.Kod)
            .NotEmpty().WithMessage("Önayar kodu zorunludur.")
            .MaximumLength(64).WithMessage("Önayar kodu en fazla 64 karakter olabilir.")
            .Matches(@"^[a-zA-Z0-9\-_]+$").WithMessage("Önayar kodu sadece ASCII harf, rakam, tire ve alt çizgi içermelidir.");

        RuleFor(x => x.AyarlarJson)
            .Must(GecerliVeGuvenliJsonMu)
            .WithMessage("Ayarlar JSON geçerli olmalı ve script/HTML içermemelidir.")
            .When(x => !string.IsNullOrWhiteSpace(x.AyarlarJson));

        RuleFor(x => x.SiraNo)
            .GreaterThanOrEqualTo(0).WithMessage("Sıra numarası 0 veya daha büyük olmalıdır.");
    }

    private static bool GecerliVeGuvenliJsonMu(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var raw = doc.RootElement.GetRawText().ToLowerInvariant();
            // Script/HTML enjeksiyonu reddet
            if (raw.Contains("<script") || raw.Contains("javascript:") || raw.Contains("onerror=") ||
                raw.Contains("onload=") || raw.Contains("&#") || raw.Contains("&lt;script"))
                return false;
            return true;
        }
        catch { return false; }
    }
}
