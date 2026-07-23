namespace VizitLink3D.Api.Sabitler;

public static class Sinirlar
{
    // Medya yükleme boyut sinirlari (00_PROJE_BILGISI depolama.*)
    public const long MAKSIMUM_RESIM_BYTE = 20 * 1024 * 1024;    // 20 MB
    public const long MAKSIMUM_VIDEO_BYTE = 500 * 1024 * 1024;   // 500 MB
    public const long MAKSIMUM_GLB_BYTE = 30 * 1024 * 1024;      // 30 MB

    // Sayfalama
    public const int VARSAYILAN_SAYFA_BOYUTU = 20;
    public const int MAKSIMUM_SAYFA_BOYUTU = 100;
    public const int MEDYA_SAYFA_BOYUTU = 50;

    // Dogrulama
    public const int MAKSIMUM_MESAJ_UZUNLUK = 5000;
    public const int MAKSIMUM_AD_SOYAD = 100;
}
