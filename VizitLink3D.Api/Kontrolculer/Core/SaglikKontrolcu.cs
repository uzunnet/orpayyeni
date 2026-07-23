using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Core;

[ApiController]
[Route("api/saglik")]
public class SaglikKontrolcu : ControllerBase
{
    private readonly VeriTabani.VizitLink3DDbContext _vt;

    public SaglikKontrolcu(VeriTabani.VizitLink3DDbContext vt)
    {
        _vt = vt;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dbSaglikli = await _vt.Database.CanConnectAsync();
        return Ok(new
        {
            durum = dbSaglikli ? "sağlıklı" : "hata",
            veritabani = dbSaglikli,
            zaman = DateTime.UtcNow,
            surum = "1.0.0"
        });
    }
}

