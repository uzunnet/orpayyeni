using System.Text.Json;
using VizitLink3D.UI.Models;
using VizitLink3D.Ortak.Modeller;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.JSInterop;

namespace VizitLink3D.UI.Servisler;

public class ApiIstemcisi(HttpClient http, IJSRuntime js)
{
    private const string VarsayilanFirmaSlug = "orpay";
    public Uri? BaseAddress => http.BaseAddress;
    public string ApiBaseUrl => http.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5115";

    private async Task BasliklariAyarlaAsync()
    {
        try
        {
            var token = await js.InvokeAsync<string?>("localStorage.getItem", "vizitlink3d_token");
            if (!string.IsNullOrEmpty(token))
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var firma = await js.InvokeAsync<string?>("localStorage.getItem", "aktif_firma");
            if (string.IsNullOrWhiteSpace(firma) || firma == "platform")
            {
                firma = VarsayilanFirmaSlug;
                await js.InvokeVoidAsync("localStorage.setItem", "aktif_firma", firma);
            }

            if (!string.IsNullOrEmpty(firma))
            {
                http.DefaultRequestHeaders.Remove("X-Firma");
                http.DefaultRequestHeaders.Add("X-Firma", firma);
            }
        }
        catch { /* localStorage erişilemezse token olmadan devam edilir */ }
    }

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            await BasliklariAyarlaAsync();
            var response = await http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return default;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errBody = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[ApiIstemcisi GET] Base: {ApiBaseUrl}, Url: {url}, Status: {response.StatusCode}, Body: {errBody?.Substring(0, Math.Min(200, errBody?.Length ?? 0))}");
                return default;
            }

            var medyaTuru = response.Content.Headers.ContentType?.MediaType;
            if (medyaTuru is null || !medyaTuru.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine($"[ApiIstemcisi GET] Base: {ApiBaseUrl}, Url: {url}, beklenen JSON yerine {medyaTuru ?? "bilinmeyen içerik"} döndü.");
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();
            var yanit = JsonSerializer.Deserialize<Cevap<T>>(json, _jsonOpts);
            return yanit?.BasariliMi == true ? yanit.Veri : default;
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi GET HATA] Base: {ApiBaseUrl}, Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return default;
        }
    }

    public async Task<Cevap<T>?> PostAsync<T>(string url, object govde)
    {
        try
        {
            await BasliklariAyarlaAsync();
            var yanit = await http.PostAsJsonAsync(url, govde);
            return await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonOpts);
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi POST HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<T>?> PutAsync<T>(string url, object govde)
    {
        try
        {
            await BasliklariAyarlaAsync();
            var yanit = await http.PutAsJsonAsync(url, govde);
            return await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonOpts);
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi PUT HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<object>?> PatchAsync(string url, object? govde = null)
    {
        try
        {
            await BasliklariAyarlaAsync();
            var istek = new HttpRequestMessage(HttpMethod.Patch, url);
            if (govde != null)
                istek.Content = JsonContent.Create(govde);
            var yanit = await http.SendAsync(istek);
            return await yanit.Content.ReadFromJsonAsync<Cevap<object>>();
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi PATCH HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<object>?> DeleteAsync(string url)
    {
        try
        {
            await BasliklariAyarlaAsync();
            var yanit = await http.DeleteAsync(url);
            return await yanit.Content.ReadFromJsonAsync<Cevap<object>>();
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi DELETE HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }

    public async Task<Cevap<T>?> PostMultipartAsync<T>(string url, MultipartFormDataContent icerik)
    {
        try
        {
            await BasliklariAyarlaAsync();
            var yanit = await http.PostAsync(url, icerik);
            return await yanit.Content.ReadFromJsonAsync<Cevap<T>>(_jsonOpts);
        }
        catch (Exception hata)
        {
            Console.Error.WriteLine($"[ApiIstemcisi POSTMULTIPART HATA] Url: {url}, Hata: {hata.Message}, Detay: {hata}");
            return null;
        }
    }
}



