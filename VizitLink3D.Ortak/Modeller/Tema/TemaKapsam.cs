namespace VizitLink3D.Ortak.Modeller.Tema;

/// <summary>
/// Tema şablonunun hangi tarafta kullanılabileceğini belirler.
/// Admin panel ve site (frontend) birbirinden bağımsız tema yönetir.
/// </summary>
public enum TemaKapsam
{
    /// <summary>Sadece admin panelinde görünür, frontend'te kullanılamaz.</summary>
    Sadece_Admin = 0,

    /// <summary>Sadece site (frontend) tarafında görünür, admin panelinde kullanılamaz.</summary>
    Sadece_Site = 1,

    /// <summary>Hem admin hem site tarafında kullanılabilir.</summary>
    Her_ikisi = 2
}
