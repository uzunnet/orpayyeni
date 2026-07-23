namespace VizitLink3D.Ortak.Modeller.Kimlik;

public class TokenYenilemeIstegi
{
    [System.Text.Json.Serialization.JsonIgnore]
    public string Token { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}
