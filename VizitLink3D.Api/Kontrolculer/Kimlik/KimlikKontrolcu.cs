using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;

namespace VizitLink3D.Api.Kontrolculer.Kimlik;

[ApiController]
[Route("api/kimlik")]
public class KimlikKontrolcu(VizitLink3DDbContext vt, IConfiguration yapilandirma) : ControllerBase
{
    public record GirisIstegi(string KullaniciAdi, string Sifre);

    [HttpPost("giris")]
    [EnableRateLimiting("Giris")]
    public async Task<Cevap<GirisYaniti>> Giris([FromBody] GirisIstegi istek)
    {
        if (string.IsNullOrWhiteSpace(istek.KullaniciAdi) || string.IsNullOrWhiteSpace(istek.Sifre))
            return Cevap<GirisYaniti>.Hata("Kullanici adi veya sifre hatali.");

        if (istek.KullaniciAdi.Length > 128 || istek.Sifre.Length > 256)
            return Cevap<GirisYaniti>.Hata("Kullanici adi veya sifre hatali.");

        var kullanici = await vt.Kullanicilar
            .FirstOrDefaultAsync(k => k.KullaniciAdi == istek.KullaniciAdi && k.AktifMi);

        if (kullanici is null || !BCrypt.Net.BCrypt.Verify(istek.Sifre, kullanici.SifreHash))
            return Cevap<GirisYaniti>.Hata("Kullanici adi veya sifre hatali.");

        if (kullanici.KilitlendiMi)
            return Cevap<GirisYaniti>.Hata("Hesabiniz kilitlenmistir.");

        kullanici.BasarisizGirisDenemesi = 0;
        kullanici.SonGirisTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();

        var token = TokenOlustur(kullanici);
        return Cevap<GirisYaniti>.Basarili(new GirisYaniti(
            token,
            kullanici.KullaniciAdi,
            kullanici.AdSoyad,
            kullanici.Rol.ToString(),
            kullanici.Eposta
        ), "Giris basarili.");
    }

    private string TokenOlustur(Kullanici kullanici)
    {
        var jwtAnahtar = Environment.GetEnvironmentVariable("VIZITLINK3D_JWT_KEY")
            ?? yapilandirma["Jwt:Anahtar"]
            ?? throw new InvalidOperationException("JWT anahtarı yapılandırılmamış.");
        var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar));
        var imzaLayici = new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256);
        var talepler = new[]
        {
            new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
            new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new Claim(ClaimTypes.Role, kullanici.Rol.ToString()),
            new Claim("Rol", kullanici.Rol.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: yapilandirma["Jwt:Yayinci"],
            audience: yapilandirma["Jwt:Izleyici"],
            claims: talepler,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: imzaLayici
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record GirisYaniti(string Token, string KullaniciAdi, string AdSoyad, string Rol, string Eposta);
