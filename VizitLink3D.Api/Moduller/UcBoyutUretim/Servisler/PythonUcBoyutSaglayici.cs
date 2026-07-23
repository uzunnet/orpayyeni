namespace VizitLink3D.Api.Moduller.UcBoyutUretim.Servisler;

/// <summary>
/// Yerel Python servisine (TripoSR) HTTP ile baglanip resimden statik (tek parca) 3D mesh uretir.
/// Uretilen model FUGA gibi parca-parca renklendirilemez - sadece dondurulebilir onizleme icindir.
/// </summary>
public class PythonUcBoyutSaglayici(HttpClient http, IConfiguration config)
{
    private string TabanUrl => config["UcBoyutUretim:PythonServisUrl"] ?? "http://127.0.0.1:8100";

    public async Task<(bool BasariliMi, byte[]? GlbVerisi, string? HataMesaji)> UretAsync(
        Stream resimStream, string dosyaAdi, CancellationToken iptal = default)
    {
        try
        {
            using var icerik = new MultipartFormDataContent();
            using var resimIcerik = new StreamContent(resimStream);
            resimIcerik.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            icerik.Add(resimIcerik, "dosya", dosyaAdi);

            var yanit = await http.PostAsync($"{TabanUrl}/uret", icerik, iptal);
            if (!yanit.IsSuccessStatusCode)
            {
                var hataMetni = await yanit.Content.ReadAsStringAsync(iptal);
                return (false, null, $"Python servisi hatasi ({yanit.StatusCode}): {hataMetni}");
            }

            var glbVerisi = await yanit.Content.ReadAsByteArrayAsync(iptal);
            return (true, glbVerisi, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<bool> SaglikTestiAsync(CancellationToken iptal = default)
    {
        try
        {
            var yanit = await http.GetAsync($"{TabanUrl}/saglik", iptal);
            return yanit.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
