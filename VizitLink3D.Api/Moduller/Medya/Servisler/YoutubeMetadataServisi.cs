namespace VizitLink3D.Api.Moduller.Medya.Servisler;

public interface IYoutubeMetadataServisi
{
    Task<YoutubeVideoBilgisi?> BilgiGetirAsync(string url);
    string? VideoIdCozumle(string url);
}

public class YoutubeVideoBilgisi
{
    public string VideoId { get; set; } = "";
    public string Baslik { get; set; } = "";
    public string? KapakResmiUrl { get; set; }
    public int? SureSaniye { get; set; }
    public string? EmbedUrl { get; set; }
}

public class YoutubeMetadataServisi : IYoutubeMetadataServisi
{
    private readonly HttpClient _http;

    public YoutubeMetadataServisi(HttpClient http) => _http = http;

    public string? VideoIdCozumle(string url)
    {
        if (url.Contains("youtube.com/watch?v="))
            return url.Split("v=")[1].Split("&")[0];
        if (url.Contains("youtu.be/"))
            return url.Split("youtu.be/")[1].Split("?")[0];
        return null;
    }

    public async Task<YoutubeVideoBilgisi?> BilgiGetirAsync(string url)
    {
        var videoId = VideoIdCozumle(url);
        if (videoId == null) return null;

        // TODO: oEmbed API cagrisi
        return new YoutubeVideoBilgisi
        {
            VideoId = videoId,
            Baslik = $"YouTube Video ({videoId})",
            KapakResmiUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg",
            EmbedUrl = $"https://www.youtube.com/embed/{videoId}"
        };
    }
}
