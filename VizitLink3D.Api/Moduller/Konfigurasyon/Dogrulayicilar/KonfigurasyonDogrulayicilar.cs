using FluentValidation;
using VizitLink3D.Api.Moduller.Konfigurasyon.Dtolar;
using VizitLink3D.Api.Moduller.Konfigurasyon.Servisler;
using VizitLink3D.Api.VeriTabani;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Moduller.Konfigurasyon.Dogrulayicilar;

/// <summary>
/// Konfigürasyon oluşturma validasyonu:
/// - UrunId pozitif ve veritabanında mevcut olmalı
/// - En az bir parça olmalı
/// - Her parça geçerli bir UrunUcBoyutParcasiId'ye sahip olmalı
/// - Parçalar aynı ürüne ait olmalı
/// </summary>
public class KonfigurasyonOlusturDogrulayici : AbstractValidator<KonfigurasyonOlusturDto>
{
    public KonfigurasyonOlusturDogrulayici(VizitLink3DDbContext vt)
    {
        RuleFor(x => x.UrunId)
            .GreaterThan(0)
            .WithMessage("Ürün ID pozitif olmalıdır.")
            .MustAsync(async (urunId, iptal) =>
                await vt.Urunler.AnyAsync(u => u.Id == urunId && !u.SilindiMi, iptal))
            .WithMessage("Geçersiz ürün — ürün bulunamadı veya silinmiş.");

        RuleFor(x => x.Parcalar)
            .NotEmpty()
            .WithMessage("En az bir parça seçimi yapılmalıdır.");

        RuleForEach(x => x.Parcalar).ChildRules(parca =>
        {
            parca.RuleFor(p => p.UrunUcBoyutParcasiId)
                .GreaterThan(0)
                .WithMessage("Parça ID pozitif olmalıdır.");
        });

        RuleFor(x => x.Parcalar)
            .Must(parcalar => parcalar.Select(p => p.UrunUcBoyutParcasiId).Distinct().Count() == parcalar.Count)
            .WithMessage("Aynı parça birden fazla kez eklenemez.");
    }
}

/// <summary>
/// Konfigürasyon güncelleme validasyonu.
/// </summary>
public class KonfigurasyonGuncelleDogrulayici : AbstractValidator<KonfigurasyonGuncelleDto>
{
    public KonfigurasyonGuncelleDogrulayici()
    {
        RuleFor(x => x.Parcalar)
            .NotEmpty()
            .WithMessage("En az bir parça seçimi yapılmalıdır.");

        RuleForEach(x => x.Parcalar).ChildRules(parca =>
        {
            parca.RuleFor(p => p.UrunUcBoyutParcasiId)
                .GreaterThan(0)
                .WithMessage("Parça ID pozitif olmalıdır.");
        });

        RuleFor(x => x.Parcalar)
            .Must(parcalar => parcalar.Select(p => p.UrunUcBoyutParcasiId).Distinct().Count() == parcalar.Count)
            .WithMessage("Aynı parça birden fazla kez eklenemez.");
    }
}

/// <summary>
/// Embed oturum olusturma istegi validasyonu:
/// - HedefOrigin gecerli bir HTTPS URL olmali (localhost HTTP dahil)
/// - HedefOrigin path/query icermemeli (sadece origin)
/// </summary>
public class EmbedOturumIstekDogrulayici : AbstractValidator<EmbedOturumIstekDto>
{
    public EmbedOturumIstekDogrulayici()
    {
        RuleFor(x => x.HedefOrigin)
            .NotEmpty().WithMessage("Hedef origin zorunludur.")
            .Must(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                    return false;

                if (!origin.StartsWith("https://") && !origin.StartsWith("http://"))
                    return false;

                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;

                // Sadece origin olmali (path/query/fragment icermemeli)
                var path = uri.PathAndQuery;
                if (path != "/" && !string.IsNullOrEmpty(path))
                    return false;

                return true;
            }).WithMessage("Hedef origin gecerli bir URL olmali ve path/query icermemelidir. Orn: https://musteri-sitesi.com");
    }
}
