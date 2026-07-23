using FluentValidation;
using VizitLink3D.Ortak.Modeller.Ayarlar;

namespace VizitLink3D.Api.Moduller.Ayarlar.Dogrulayicilar;

public class ResimOptimizasyonuAyarDogrulayici : AbstractValidator<ResimOptimizasyonuAyarDto>
{
    public ResimOptimizasyonuAyarDogrulayici()
    {
        RuleFor(x => x.MaksimumKenar)
            .InclusiveBetween(256, 4096)
            .WithMessage("Maksimum kenar değeri 256 ile 4096 piksel arasında olmalıdır.");

        RuleFor(x => x.Kalite)
            .InclusiveBetween(50, 100)
            .WithMessage("Kalite değeri 50 ile 100 arasında olmalıdır.");
    }
}
