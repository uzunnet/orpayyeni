using FluentValidation;
using VizitLink3D.Ortak.Modeller.Urunler;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;

/// <summary>
/// Public konfigüratör slug sorgusu validasyonu.
/// </summary>
public class PublicKonfiguratorSorguDogrulayici : AbstractValidator<string>
{
    public PublicKonfiguratorSorguDogrulayici()
    {
        RuleFor(s => s)
            .NotEmpty().WithMessage("Ürün slug'ı zorunludur.")
            .MaximumLength(256).WithMessage("Slug en fazla 256 karakter olabilir.")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug sadece küçük harf, rakam ve tire içerebilir.");
    }
}

/// <summary>
/// Public seçim kaydetme DTO validasyonu.
/// </summary>
public class PublicSecimKaydetDogrulayici : AbstractValidator<PublicSecimKaydetDto>
{
    public PublicSecimKaydetDogrulayici()
    {
        RuleFor(x => x.UrunId)
            .GreaterThan(0).WithMessage("Ürün ID zorunludur.");

        RuleFor(x => x.Secimler)
            .NotEmpty().WithMessage("En az bir parça seçimi gereklidir.");

        RuleFor(x => x.MusteriNotu)
            .MaximumLength(2000).WithMessage("Müşteri notu en fazla 2000 karakter olabilir.");

        RuleForEach(x => x.Secimler).SetValidator(new PublicParcaSecimiDogrulayici());

        // Aynı parça ID'si birden fazla kez gönderilemez
        RuleFor(x => x.Secimler)
            .Must(s => s.Select(x => x.ParcaId).Distinct().Count() == s.Count)
            .WithMessage("Aynı parça için birden fazla seçim gönderilemez.");
    }
}

/// <summary>
/// Tek parça seçimi validasyonu.
/// </summary>
public class PublicParcaSecimiDogrulayici : AbstractValidator<PublicParcaSecimiDto>
{
    public PublicParcaSecimiDogrulayici()
    {
        RuleFor(x => x.ParcaId)
            .GreaterThan(0).WithMessage("Parça ID zorunludur.");

        RuleFor(x => x.SeciliRenkId)
            .GreaterThan(0).When(x => x.SeciliRenkId.HasValue)
            .WithMessage("Geçersiz renk ID.");

        RuleFor(x => x.SeciliMalzemeId)
            .GreaterThan(0).When(x => x.SeciliMalzemeId.HasValue)
            .WithMessage("Geçersiz malzeme ID.");

        RuleFor(x => x.SeciliKaplamaId)
            .GreaterThan(0).When(x => x.SeciliKaplamaId.HasValue)
            .WithMessage("Geçersiz kaplama ID.");

        RuleFor(x => x.Aci)
            .InclusiveBetween(-360, 360).When(x => x.Aci.HasValue)
            .WithMessage("Açı -360 ile 360 derece arasında olmalıdır.");
    }
}
