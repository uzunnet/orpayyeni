using System.Text.Json.Serialization;

namespace VizitLink3D.Ortak.Modeller;

/// <summary>
/// Dinamik sayfa goruntuleme sistemi DTO'lari.
/// Admin panelden yapilandirilan sayfalar bu modellerle public UI'a aktarilir.
/// </summary>

public class SayfaGorunumDto
{
    public string Slug { get; set; } = "";
    public string Baslik { get; set; } = "";
    public string? SeoBaslik { get; set; }
    public string? SeoAciklama { get; set; }
    public string SayfaTipi { get; set; } = "Dinamik";
    public List<SayfaBolumuDto> Bolumler { get; set; } = [];
}

public class SayfaBolumuDto
{
    public string BolumKodu { get; set; } = "";
    public string BolumTipi { get; set; } = "MetinGorsel";
    public string? Baslik { get; set; }
    public string? AltBaslik { get; set; }
    public string? Aciklama { get; set; }
    public string? GorselUrl { get; set; }
    public string? GorselMobilUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ButonMetni { get; set; }
    public string? ButonLink { get; set; }
    public string? ButonMetni2 { get; set; }
    public string? ButonLink2 { get; set; }
    public string? ArkaPlanRengi { get; set; }
    public string? MetinRengi { get; set; }
    public string? AnimasyonTipi { get; set; } = "fade";
    public int AnimasyonGecikme { get; set; } = 100;
    public int AnimasyonSure { get; set; } = 800;
    public int Sira { get; set; }
    public List<BlokDto> Bloklar { get; set; } = [];
}

public class BlokDto
{
    public string BlokTipi { get; set; } = "Metin";
    public string? Icerik { get; set; }
    public string? GorselUrl { get; set; }
    public string? Link { get; set; }
    public string? Ikon { get; set; }
    public int Sira { get; set; }
}
