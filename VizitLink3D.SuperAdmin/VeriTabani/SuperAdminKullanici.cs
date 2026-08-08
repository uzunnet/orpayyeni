namespace VizitLink3D.SuperAdmin.VeriTabani;

public class SuperAdminKullanici
{
    public int Id { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string SifreHash { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
