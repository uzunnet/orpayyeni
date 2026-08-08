using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VizitLink3D.SuperAdmin.VeriTabani;

namespace VizitLink3D.SuperAdmin.Kontrolculer;

[ApiController]
[Route("api/super-admin/kimlik")]
public class SuperAdminKimlikKontrolcusu(SuperAdminDbContext vt, IConfiguration yapilandirma) : ControllerBase
{
    public record GirisIstegi(string KullaniciAdi, string Sifre);
    public record GirisYaniti(string Token, string KullaniciAdi, string AdSoyad);

    [HttpPost("giris")]
    public async Task<IActionResult> Giris([FromBody] GirisIstegi istek)
    {
        if (string.IsNullOrWhiteSpace(istek.KullaniciAdi) || string.IsNullOrWhiteSpace(istek.Sifre))
            return Unauthorized("Kullanici adi veya sifre hatali.");

        var kullanici = await vt.SuperAdminKullanicilar
            .FirstOrDefaultAsync(k => k.KullaniciAdi == istek.KullaniciAdi && k.AktifMi);

        if (kullanici is null || !BCrypt.Net.BCrypt.Verify(istek.Sifre, kullanici.SifreHash))
            return Unauthorized("Kullanici adi veya sifre hatali.");

        var token = TokenOlustur(kullanici);
        return Ok(new GirisYaniti(token, kullanici.KullaniciAdi, kullanici.AdSoyad));
    }

    private string TokenOlustur(SuperAdminKullanici kullanici)
    {
        var jwtAnahtar = yapilandirma["Jwt:Anahtar"] ?? "VizitLink3D_SuperAdmin_Gizli_Anahtari_2026";
        var jwtYayinci = yapilandirma["Jwt:Yayinci"] ?? "VizitLink3D.SuperAdmin";
        var jwtIzleyici = yapilandirma["Jwt:Izleyici"] ?? "VizitLink3D.SuperAdmin.UI";
        var sureDakika = int.TryParse(yapilandirma["Jwt:SureDakika"], out var s) ? s : 10080;

        var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar));
        var kimlikBilgileri = new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
            new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new Claim(ClaimTypes.Role, "SuperAdmin"),
            new Claim("Rol", "SuperAdmin")
        };

        var token = new JwtSecurityToken(
            issuer: jwtYayinci,
            audience: jwtIzleyici,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(sureDakika),
            signingCredentials: kimlikBilgileri
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
