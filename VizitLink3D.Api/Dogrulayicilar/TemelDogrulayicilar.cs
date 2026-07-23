using VizitLink3D.Api.Modeller;
using VizitLink3D.Ortak.Modeller;
using FluentValidation;

namespace VizitLink3D.Api.Dogrulayicilar;

/// <summary>
/// Tum temel DTO modelleri icin dogrulama kurallari.
/// </summary>
public class KapiKategorisiDogrulayici : AbstractValidator<KapiKategorisi>
{
    public KapiKategorisiDogrulayici() { RuleFor(x=>x.Ad).NotEmpty().MaximumLength(200); RuleFor(x=>x.Slug).NotEmpty().Matches("^[a-z0-9-]+$"); }
}

public class KapakModeliDogrulayici : AbstractValidator<KapakModeli>
{
    public KapakModeliDogrulayici()
    {
        RuleFor(x => x.ModelAdi).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
        RuleFor(x => x.Kategori).NotEmpty();
        RuleFor(x => x.SiraNo).GreaterThanOrEqualTo(0);
    }
}

public class ProjeDogrulayici : AbstractValidator<Proje>
{
    public ProjeDogrulayici() { RuleFor(x=>x.Baslik).NotEmpty().MaximumLength(300); RuleFor(x=>x.Slug).NotEmpty(); }
}

public class MusteriYorumuDogrulayici : AbstractValidator<MusteriYorumu>
{
    public MusteriYorumuDogrulayici() { RuleFor(x=>x.Yorum).NotEmpty().MaximumLength(2000); RuleFor(x=>x.Puan).InclusiveBetween(1,5); RuleFor(x=>x.MusteriAdi).NotEmpty(); }
}

public class IletisimMesajiDogrulayici : AbstractValidator<IletisimMesaji>
{
    public IletisimMesajiDogrulayici() { RuleFor(x=>x.AdSoyad).NotEmpty().MaximumLength(100); RuleFor(x=>x.Eposta).NotEmpty().EmailAddress(); RuleFor(x=>x.Mesaj).NotEmpty().MaximumLength(5000); }
}

public class KullaniciDogrulayici : AbstractValidator<Kullanici>
{
    public KullaniciDogrulayici() { RuleFor(x=>x.Eposta).NotEmpty().EmailAddress(); RuleFor(x=>x.KullaniciAdi).NotEmpty().MinimumLength(3); RuleFor(x=>x.AdSoyad).NotEmpty(); }
}

public class HaberYazisiDogrulayici : AbstractValidator<HaberYazisi>
{
    public HaberYazisiDogrulayici()
    {
        RuleFor(x => x.Baslik).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
        RuleFor(x => x.Ozet).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Icerik).NotEmpty();
    }
}

public class BultenAbonesiDogrulayici : AbstractValidator<BultenAbonesi>
{
    public BultenAbonesiDogrulayici() { RuleFor(x => x.Eposta).NotEmpty().EmailAddress(); }
}

public class KatalogDogrulayici : AbstractValidator<Katalog>
{
    public KatalogDogrulayici() { RuleFor(x => x.Baslik).NotEmpty().MaximumLength(300); RuleFor(x => x.SiraNo).GreaterThanOrEqualTo(0); }
}

public class SertifikaDogrulayici : AbstractValidator<Sertifika>
{
    public SertifikaDogrulayici() { RuleFor(x => x.Ad).NotEmpty().MaximumLength(300); }
}

public class SubeDogrulayici : AbstractValidator<Sube>
{
    public SubeDogrulayici() { RuleFor(x => x.Ad).NotEmpty().MaximumLength(200); RuleFor(x => x.Telefon).NotEmpty(); }
}

public class EkipUyesiDogrulayici : AbstractValidator<EkipUyesi>
{
    public EkipUyesiDogrulayici() { RuleFor(x => x.AdSoyad).NotEmpty().MaximumLength(150); RuleFor(x => x.Unvan).NotEmpty().MaximumLength(200); }
}
