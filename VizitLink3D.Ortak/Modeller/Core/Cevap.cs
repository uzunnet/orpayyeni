namespace VizitLink3D.Ortak.Modeller;

public class Cevap<T>
{
    public bool BasariliMi { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public List<string> Hatalar { get; set; } = new();
    public T? Veri { get; set; }

    public static Cevap<T> Basarili(T veri, string mesaj = "Islem basarili.")
        => new() { BasariliMi = true, Veri = veri, Mesaj = mesaj };

    public static Cevap<T> Hata(string mesaj, List<string>? hatalar = null)
        => new() { BasariliMi = false, Mesaj = mesaj, Hatalar = hatalar ?? new List<string>() };
}
