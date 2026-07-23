using VizitLink3D.Ortak.Modeller;
using FluentValidation;

namespace VizitLink3D.Api.Dogrulayicilar;

/// <summary>
/// Kapi modeli (kapak/kapi) icin dogrulama kurallari.
/// </summary>
public class SlaytDogrulayici : AbstractValidator<Slayt>
{
    public SlaytDogrulayici()
    {
        RuleFor(x => x.Baslik).NotEmpty().WithMessage("Baslik bos birakilamaz.").MaximumLength(200);
        RuleFor(x => x.SiraNo).GreaterThanOrEqualTo(0);
    }
}

public class SSSDogrulayici : AbstractValidator<SikSorulanSoru>
{
    public SSSDogrulayici()
    {
        RuleFor(x => x.Soru).NotEmpty().WithMessage("Soru bos birakilamaz.").MaximumLength(500);
        RuleFor(x => x.Cevap).NotEmpty().WithMessage("Cevap bos birakilamaz.").MaximumLength(2000);
    }
}

public class HizmetAdimiDogrulayici : AbstractValidator<HizmetAdimi>
{
    public HizmetAdimiDogrulayici()
    {
        RuleFor(x => x.Baslik).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdimNo).GreaterThan(0);
    }
}

public class ReferansDogrulayici : AbstractValidator<Referans>
{
    public ReferansDogrulayici()
    {
        RuleFor(x => x.Ad).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Tip).NotEmpty();
    }
}
