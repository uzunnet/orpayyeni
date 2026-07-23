using VizitLink3D.Api.Modeller;
using VizitLink3D.Api.VeriTabani;
using VizitLink3D.Ortak.Modeller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VizitLink3D.Api.Kontrolculer.Icerik;

[ApiController]
[Route("api/kapak-modelleri")]
public class KapakModeliKontrolcu(VizitLink3DDbContext vt, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<Cevap<List<KapakModeli>>> Listele(
        [FromQuery] bool? oneCikan,
        [FromQuery] string? kategori,
        [FromQuery] string? modelTuru,
        [FromQuery] string? arama,
        [FromQuery] int adet = 0)
    {
        var sorgu = vt.KapakModelleri
            .AsNoTracking()
            .Where(k => !k.SilindiMi)
            .AsQueryable();

        if (oneCikan.HasValue) sorgu = sorgu.Where(k => k.OneCikanMi == oneCikan.Value);
        if (!string.IsNullOrWhiteSpace(kategori)) sorgu = sorgu.Where(k => k.Kategori == kategori);
        if (!string.IsNullOrWhiteSpace(modelTuru)) sorgu = sorgu.Where(k => k.ModelTuru == modelTuru);
        if (!string.IsNullOrWhiteSpace(arama))
        {
            sorgu = sorgu.Where(k =>
                k.ModelAdi.Contains(arama) ||
                k.ModelKodu.Contains(arama) ||
                k.Kategori.Contains(arama));
        }

        sorgu = sorgu
            .OrderByDescending(k => k.OneCikanMi)
            .ThenBy(k => k.SiraNo)
            .ThenByDescending(k => k.OlusturulmaTarihi);

        if (adet > 0)
        {
            sorgu = sorgu.Take(adet);
        }

        return Cevap<List<KapakModeli>>.Basarili(await sorgu.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cevap<KapakModeli>>> Getir(int id)
    {
        var model = await vt.KapakModelleri.FindAsync(id);
        if (model is null || model.SilindiMi) return NotFound(Cevap<KapakModeli>.Hata("Kapak modeli bulunamadı."));
        return Cevap<KapakModeli>.Basarili(model);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<Cevap<KapakModeli>>> SlugIleGetir(string slug)
    {
        var model = await vt.KapakModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(k => !k.SilindiMi && k.Slug == slug);

        if (model is null) return NotFound(Cevap<KapakModeli>.Hata("Kapak modeli bulunamadı."));
        return Cevap<KapakModeli>.Basarili(model);
    }

    [HttpGet("benzer/{id:int}")]
    public async Task<Cevap<List<KapakModeli>>> BenzerleriGetir(int id, [FromQuery] int adet = 6)
    {
        var model = await vt.KapakModelleri
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == id && !k.SilindiMi);

        if (model is null)
        {
            return Cevap<List<KapakModeli>>.Basarili([]);
        }

        var liste = await vt.KapakModelleri
            .AsNoTracking()
            .Where(k => !k.SilindiMi && k.Id != id && k.ModelTuru == model.ModelTuru)
            .Where(k => k.Kategori == model.Kategori || k.OneCikanMi)
            .OrderByDescending(k => k.Kategori == model.Kategori)
            .ThenByDescending(k => k.OneCikanMi)
            .ThenBy(k => k.SiraNo)
            .Take(adet)
            .ToListAsync();

        return Cevap<List<KapakModeli>>.Basarili(liste);
    }

    [HttpGet("cok-izlenen")]
    public async Task<Cevap<List<KapakModeli>>> CokIzlenen(
        [FromQuery] string? modelTuru,
        [FromQuery] int adet = 6)
    {
        var sayfalar = await vt.ZiyaretKayitlari
            .AsNoTracking()
            .Where(z => z.Sayfa != null && (z.Sayfa.StartsWith("/kapak/") || z.Sayfa.StartsWith("/kapi/")))
            .GroupBy(z => z.Sayfa!)
            .OrderByDescending(g => g.Count())
            .Take(adet * 3)
            .Select(g => g.Key)
            .ToListAsync();

        var idler = sayfalar
            .Select(KapakModelIdCikar)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        List<KapakModeli> modeller;
        if (idler.Any())
        {
            var sorgu = vt.KapakModelleri
                .AsNoTracking()
                .Where(k => !k.SilindiMi && idler.Contains(k.Id));

            if (!string.IsNullOrWhiteSpace(modelTuru))
            {
                sorgu = sorgu.Where(k => k.ModelTuru == modelTuru);
            }

            var bulunanlar = await sorgu.ToListAsync();
            modeller = idler
                .Select(id => bulunanlar.FirstOrDefault(k => k.Id == id))
                .Where(k => k is not null)
                .Cast<KapakModeli>()
                .Take(adet)
                .ToList();
        }
        else
        {
            var sorgu = vt.KapakModelleri
                .AsNoTracking()
                .Where(k => !k.SilindiMi);

            if (!string.IsNullOrWhiteSpace(modelTuru))
            {
                sorgu = sorgu.Where(k => k.ModelTuru == modelTuru);
            }

            modeller = await sorgu
                .OrderByDescending(k => k.OneCikanMi)
                .ThenBy(k => k.SiraNo)
                .Take(adet)
                .ToListAsync();
        }

        return Cevap<List<KapakModeli>>.Basarili(modeller);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<Cevap<KapakModeli>>> Ekle([FromBody] KapakModeli model)
    {
        model.OlusturulmaTarihi = DateTime.UtcNow;
        model.Slug = SlugGarantiEt(model);
        vt.KapakModelleri.Add(model);
        await vt.SaveChangesAsync();
        return CreatedAtAction(nameof(Getir), new { id = model.Id }, Cevap<KapakModeli>.Basarili(model));
    }

    [HttpPut("{id:int}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<Cevap<KapakModeli>>> Guncelle(int id, [FromBody] KapakModeli model)
    {
        var mevcut = await vt.KapakModelleri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi) return NotFound(Cevap<KapakModeli>.Hata("Kapak modeli bulunamadı."));

        model.Id = id;
        model.Slug = SlugGarantiEt(model);
        vt.Entry(mevcut).CurrentValues.SetValues(model);
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Cevap<KapakModeli>.Basarili(mevcut);
    }

    [HttpDelete("{id:int}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<Cevap<bool>>> Sil(int id)
    {
        var mevcut = await vt.KapakModelleri.FindAsync(id);
        if (mevcut is null || mevcut.SilindiMi) return NotFound(Cevap<bool>.Hata("Kapak modeli bulunamadı."));

        mevcut.SilindiMi = true;
        mevcut.SilinmeTarihi = DateTime.UtcNow;
        mevcut.GuncellenmeTarihi = DateTime.UtcNow;
        await vt.SaveChangesAsync();
        return Cevap<bool>.Basarili(true);
    }

    [HttpPost("yukle/{tur}")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<Cevap<string>>> DosyaYukle(string tur, IFormFile dosya)
    {
        if (dosya == null || dosya.Length == 0)
        {
            return BadRequest(Cevap<string>.Hata("Dosya seçilmedi."));
        }

        var dosyaAdi = $"{Guid.NewGuid()}{Path.GetExtension(dosya.FileName)}";
        var altDizin = tur.Equals("3d", StringComparison.OrdinalIgnoreCase)
            ? "3d"
            : tur.Contains("kapi", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine("kapi-modelleri", "yuklenenler")
                : "kapaklar";
        var dizin = Path.Combine(env.WebRootPath, "medya", altDizin);

        if (!Directory.Exists(dizin))
        {
            Directory.CreateDirectory(dizin);
        }

        var dosyaYolu = Path.Combine(dizin, dosyaAdi);

        using (var stream = new FileStream(dosyaYolu, FileMode.Create))
        {
            await dosya.CopyToAsync(stream);
        }

        var relativePath = $"/medya/{altDizin.Replace('\\', '/')}/{dosyaAdi}";
        return Cevap<string>.Basarili(relativePath, "Dosya yüklendi.");
    }

    private static int KapakModelIdCikar(string url)
    {
        var parcalar = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parcalar.Length >= 2 && int.TryParse(parcalar[1], out var id) ? id : 0;
    }

    private static string SlugGarantiEt(KapakModeli model)
    {
        if (!string.IsNullOrWhiteSpace(model.Slug))
        {
            return model.Slug.Trim().ToLowerInvariant();
        }

        var kaynak = string.IsNullOrWhiteSpace(model.ModelKodu) ? model.ModelAdi : model.ModelKodu;
        return kaynak
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Replace("--", "-");
    }
}

