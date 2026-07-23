-- ═══════════════════════════════════════════════════════════
-- DesaDoor — UrunUcBoyutParcalari Tohum Verisi
-- Luna Duşakabin (UrunId=1) için örnek parçalar
-- ═══════════════════════════════════════════════════════════

-- Önce Luna Duşakabin için bir 3B model kaydı oluştur (yoksa)
INSERT INTO UrunUcBoyutModelleri (UrunId, MedyaId, ModelYolu, ModelTipi, DosyaBoyutuByte, Versiyon, VarsayilanMi, AktifMi, OlusturulmaTarihi, SilindiMi)
SELECT 1, 0, 'modeller/luna_dusakabin.glb', 'Glb', 0, 1, 1, 1, '2026-01-01 00:00:00', 0
WHERE NOT EXISTS (SELECT 1 FROM UrunUcBoyutModelleri WHERE UrunId = 1 AND VarsayilanMi = 1);

-- Parçaları ekle (UrunUcBoyutModeliId = ilk modelin Id'si)
-- ModelId, yukarıdaki INSERT'ten sonra otomatik atanır; SQLite'da son eklenen rowid alınır

-- 1. Cam Panel
INSERT INTO UrunUcBoyutParcalari (UrunUcBoyutModeliId, MeshAdi, GorunenAd, ParcaGrubuId, SecilebilirMi, RenklenebilirMi, MalzemeDegisebilirMi, GizlenebilirMi, HareketliMi, HareketTipi, SiraNo, AktifMi)
VALUES (
    (SELECT Id FROM UrunUcBoyutModelleri WHERE UrunId = 1 AND VarsayilanMi = 1 LIMIT 1),
    'CamPanel_Mesh', 'Cam Panel', NULL, 1, 1, 0, 0, 0, 'Yok', 1, 1
);

-- 2. Alüminyum Profil
INSERT INTO UrunUcBoyutParcalari (UrunUcBoyutModeliId, MeshAdi, GorunenAd, ParcaGrubuId, SecilebilirMi, RenklenebilirMi, MalzemeDegisebilirMi, GizlenebilirMi, HareketliMi, HareketTipi, SiraNo, AktifMi)
VALUES (
    (SELECT Id FROM UrunUcBoyutModelleri WHERE UrunId = 1 AND VarsayilanMi = 1 LIMIT 1),
    'AluminyumProfil_Mesh', 'Alüminyum Profil', NULL, 1, 1, 0, 0, 0, 'Yok', 2, 1
);

-- 3. Kulp
INSERT INTO UrunUcBoyutParcalari (UrunUcBoyutModeliId, MeshAdi, GorunenAd, ParcaGrubuId, SecilebilirMi, RenklenebilirMi, MalzemeDegisebilirMi, GizlenebilirMi, HareketliMi, HareketTipi, SiraNo, AktifMi)
VALUES (
    (SELECT Id FROM UrunUcBoyutModelleri WHERE UrunId = 1 AND VarsayilanMi = 1 LIMIT 1),
    'Kulp_Mesh', 'Kulp', NULL, 1, 0, 0, 0, 0, 'Yok', 3, 1
);

-- 4. Ray
INSERT INTO UrunUcBoyutParcalari (UrunUcBoyutModeliId, MeshAdi, GorunenAd, ParcaGrubuId, SecilebilirMi, RenklenebilirMi, MalzemeDegisebilirMi, GizlenebilirMi, HareketliMi, HareketTipi, SiraNo, AktifMi)
VALUES (
    (SELECT Id FROM UrunUcBoyutModelleri WHERE UrunId = 1 AND VarsayilanMi = 1 LIMIT 1),
    'Ray_Mesh', 'Ray', NULL, 0, 0, 0, 0, 1, 'Surme', 4, 1
);

-- 5. Menteşe
INSERT INTO UrunUcBoyutParcalari (UrunUcBoyutModeliId, MeshAdi, GorunenAd, ParcaGrubuId, SecilebilirMi, RenklenebilirMi, MalzemeDegisebilirMi, GizlenebilirMi, HareketliMi, HareketTipi, SiraNo, AktifMi)
VALUES (
    (SELECT Id FROM UrunUcBoyutModelleri WHERE UrunId = 1 AND VarsayilanMi = 1 LIMIT 1),
    'Mentese_Mesh', 'Menteşe', NULL, 0, 0, 0, 0, 0, 'Yok', 5, 1
);
