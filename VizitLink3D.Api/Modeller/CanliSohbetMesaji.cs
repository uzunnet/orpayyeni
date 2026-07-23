namespace VizitLink3D.Api.Modeller;

public class CanliSohbetMesaji
{
    public int Id { get; set; }
    public string OturumId { get; set; } = string.Empty;
    public string GonderenAd { get; set; } = string.Empty;
    public string MesajMetni { get; set; } = string.Empty;
    public bool YoneticiMi { get; set; }
    public DateTime Tarih { get; set; } = DateTime.UtcNow;
    public bool OkunduMu { get; set; }
}
