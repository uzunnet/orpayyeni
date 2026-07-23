namespace VizitLink3D.Api.Moduller.Medya.Servisler;

public interface IDepolamaAdaptoru
{
    Task<string> YukleAsync(Stream dosya, string dosyaAdi, string klasor, CancellationToken iptal = default);
    Task SilAsync(string dosyaYolu, CancellationToken iptal = default);
    Task<Stream?> GetirAsync(string dosyaYolu, CancellationToken iptal = default);
    string UrlOlustur(string dosyaYolu);
    bool Varmi(string dosyaYolu);
}
