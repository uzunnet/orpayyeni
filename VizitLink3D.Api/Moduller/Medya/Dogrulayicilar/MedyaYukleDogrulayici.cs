using FluentValidation;

namespace VizitLink3D.Api.Moduller.Medya.Dogrulayicilar;

public class MedyaYukleDogrulayici : AbstractValidator<IFormFile>
{
    private static readonly HashSet<string> IzinliMimeTipleri = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml",
        "video/mp4", "video/webm",
        "application/pdf",
        "model/gltf-binary", "model/gltf+json", "application/octet-stream"
    };

    public MedyaYukleDogrulayici()
    {
        RuleFor(x => x.Length).LessThanOrEqualTo(20 * 1024 * 1024).WithMessage("Dosya boyutu 20MB'i asamaz.");
        RuleFor(x => x.ContentType).Must(x => IzinliMimeTipleri.Contains(x)).WithMessage("Desteklenmeyen dosya tipi.");
    }
}
