using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller.Urunler;

/// <summary>
/// Ürün formu (Admin) — tam yazma yetkisi
/// </summary>
public class UrunAdminDto
{
    public int? Id { get; set; } // null ise yeni ürün
    public string Slug { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? KisaAciklama { get; set; }
    public string? Aciklama { get; set; }
    public int UrunAilesiId { get; set; }
    public int? UrunKategoriId { get; set; }
    public bool AktifMi { get; set; } = true;
    public bool OneCikanMi { get; set; }
    public bool YeniMi { get; set; }
    public decimal? Fiyat { get; set; }
    public string? Birim { get; set; }
    public int SiraNo { get; set; }
    public long? AnaGorselMedyaId { get; set; }
    public int? VarsayilanUcBoyutModeliId { get; set; }
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }

    // Yerellestirme (TR/EN)
    public Dictionary<string, UrunYerellestirmeAdminDto> Yerellestirmeler { get; set; } = new();

    // Medya
    public List<UrunMedyaAdminDto> Medyalar { get; set; } = new();

    // 3D modeli
    public UrunUcBoyutAdminDto? UcBoyutModeli { get; set; }

    // Renk seçenekleri
    public List<RenkSecenekAdminDto> RenkSecenekleri { get; set; } = new();

    // Malzeme seçenekleri
    public List<MalzemeSecenekAdminDto> MalzemeSecenekleri { get; set; } = new();
}

public class UrunYerellestirmeAdminDto
{
    public int? Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
}

public class UrunMedyaAdminDto
{
    public int? Id { get; set; }
    public long MedyaId { get; set; }
    public string? AltMetni { get; set; }
    public int SiraNo { get; set; }
}

public class UrunUcBoyutAdminDto
{
    public int? Id { get; set; }
    public long MedyaId { get; set; }
    public string? Aciklama { get; set; }
    public List<UcBoyutParcaAdminDto> Parcalar { get; set; } = new();
}

public class UcBoyutParcaAdminDto
{
    public int? Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? TekstureKodu { get; set; }
    public bool RenklendirilebilirMi { get; set; }
    public bool MalzemeSecenegiVarMi { get; set; }
}

public class RenkSecenekAdminDto
{
    public int? Id { get; set; }
    public int RalRengiId { get; set; }
    public int? UcBoyutParcaId { get; set; }
}

public class MalzemeSecenekAdminDto
{
    public int? Id { get; set; }
    public int MalzemeId { get; set; }
    public decimal? UretimMaliyet { get; set; }
    public int? UcBoyutParcaId { get; set; }
}

/// <summary>
/// DTO doğrulama için FluentValidation sınıfı
/// </summary>
public class UrunAdminDtoDogrulamasi
{
    public static bool DogrulamaKurallimiVar => true;

    public static string Dogrula(UrunAdminDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Ad))
            return "Ürün adı boş bırakılamaz";

        if (string.IsNullOrWhiteSpace(dto.Slug))
            return "Slug boş bırakılamaz";

        if (dto.Slug.Contains(" "))
            return "Slug boşluk içeremez";

        if (dto.UrunAilesiId <= 0)
            return "Ürün ailesi seçilmelidir";

        return string.Empty; // Başarılı
    }
}
