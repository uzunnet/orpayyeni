---
# ╔══════════════════════════════════════════════════════════════════╗
# ║  BU DOSYA HER PROJEDE ELLE DOLDURULUR.                          ║
# ║  Diğer kural dosyaları bu değerlere REFERANS verir.              ║
# ╚══════════════════════════════════════════════════════════════════╝

# === PROJE KİMLİĞİ ===
proje_adi: "Orpay Orman Ürünleri"
firma_adi: "Orpay Orman Ürünleri Ltd. Şti."
firma_unvan: "Orpay Orman Ürünleri Limited Şirketi"
sektor: "orman ürünleri, kapı yüzeyleri, kasa pervaz, yapı malzemeleri"
slogan: ""
kurulus_yili: 1992

# === DOMAIN / URL ===
url_birincil: "orpayormanurunleri.com.tr"
url_yedek: "www.orpayormanurunleri.com.tr"
admin_url: "/admin"
api_base_url: "/api"

# === PORTLAR ===
port_api: 5215
port_ui: 5213
port_signalr: 5215

# === İLETİŞİM ===
iletisim:
  eposta: "iletisim@orpayormanurunleri.com.tr"
  telefon_1: "+90 366 313 61 71"
  telefon_2: "+90 366 313 61 72"
  whatsapp: ""
  adres: "Harsat Mahallesi Rıhtım Boyu Cd. No:242 Tosya/Kastamonu"
  fabrika: "Kurtbeli Mevkii Tosya Organize Sanayi Bölgesi Cad.1 No:1 Tosya/Kastamonu"
  sehir: "Kastamonu"
  ilce: "Tosya"
  posta_kodu: ""
  enlem: 41.000
  boylam: 34.000
  calisma_saatleri: "Pzt-Cmt 09:00-18:00"

# === SOSYAL MEDYA ===
sosyal:
  instagram: ""
  facebook: ""
  twitter: ""
  linkedin: ""
  youtube: ""
  pinterest: ""
  tiktok: ""

# === TEMA / RENK PALETİ ===
tema:
  varyant: "Endüstriyel Premium"
  ana_renk: "#0d0d0d"
  ana_renk_2: "#1a1a1a"
  ikincil_renk: "#e8c84a"
  ikincil_renk_2: "#c4a035"
  vurgu_renk: "#27ae60"
  vurgu_parlak: "#2ecc71"
  arkaplan: "#ffffff"
  arkaplan_yumusak: "#f8f6f2"
  arkaplan_koyu: "#0d0d0d"
  metin: "#2c2c2c"
  metin_acik: "#8a8a8a"
  metin_soluk: "#9a9a9a"
  basari: "#27ae60"
  uyari: "#e8c84a"
  hata: "#9b3d3d"
  bilgi: "#4a6c8c"

# === TİPOGRAFİ ===
font:
  baslik: "Manrope"
  metin: "Roboto"
  vurgu: "Manrope"
  mono: "JetBrains Mono"

# === STITCH (Google) ENTEGRASYONU ===
stitch:
  aktif: true
  design_md_yolu: "tasarim/DESIGN.md"
  hot_reload: false
  fallback_palet: "tema"

# === DİL / YERELLEŞTİRME ===
diller:
  varsayilan: "tr"
  destekli: ["tr", "en"]
  ceviri_kaynak: "db"

# === MODÜL AKTİVASYONU ===
moduller:
  Blog: true
  Galeri: true
  Iletisim: true
  Sohbet: true
  Medya_Havuzu: true
  AI_Asistan: true
  3D_Goruntu: true
  E_Ticaret: false
  Coklu_Dil: true
  PWA_Offline: false
  Audit_Log: true
  Yedekleme: false

# === GÜVENLİK ===
guvenlik:
  jwt_gecerlilik_dakika: 10080
  refresh_token_gun: 7
  bcrypt_work_factor: 12
  rate_limit_genel_per_5dk: 1000
  rate_limit_giris_per_dk: 5
  iki_adim_dogrulama: false
  passkey: false

# === DEPOLAMA ===
depolama:
  saglayici: "yerel"
  yerel_yol: "wwwroot/medya"
  max_resim_mb: 20
  max_video_mb: 500
  max_pdf_mb: 50
  max_glb_mb: 30

# === AI ENTEGRASYON (admin içi) ===
ai:
  varsayilan_saglayici: "openai"
  varsayilan_model: "gpt-4o-mini"
  fallback_saglayici: "anthropic"
  aylik_limit_usd: 100
  kullanici_gunluk_limit_cagri: 50
  streaming: true
  pii_filtre: true

# === MULTI-TENANT (SaaS) ===
multi_tenant:
  aktif: true
  tenant_tespit: "domain"

# === YEDEK / TEST ===
yedek:
  otomatik_gunluk: false
  saat: "02:00"
  saklama_gun: 30
  konum: "Yedekler/db/"

test:
  min_test_per_ozellik: 5
  postgres_testcontainer: false

# === DEPLOY ===
deploy:
  ortam: "development"
  https_zorunlu: false
  hsts_aktif: false
  csp_aktif: true
---

# VizitLink3D Proje Bilgisi

Bu dosya `AGENTS.md` standardına göre doldurulmuştur. Tüm kural dosyaları bu değerlere referans verir.

## Bağlantılar

- **Domain:** 3dvizitlink.com.tr
- **API:** http://localhost:5215
- **UI:** http://localhost:5213
- **Admin:** http://localhost:5213/admin/giris
- **Admin Kullanıcı:** admin / vizitlink3d2024
