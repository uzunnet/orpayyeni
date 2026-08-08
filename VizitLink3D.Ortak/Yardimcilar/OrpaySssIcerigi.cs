namespace VizitLink3D.Ortak.Yardimcilar;

public sealed record OrpaySssKaydi(string Soru, string Cevap, string KategoriAdi);

public static class OrpaySssIcerigi
{
    public static IReadOnlyList<OrpaySssKaydi> Kayitlar { get; } =
    [
        new("Orpay ürünlerinde garanti süresi nedir?", "Orpay ürünleri, teslim tarihinden itibaren 2 yıl garanti kapsamındadır. Garanti koşulları ürün ve kullanım türüne göre teklif aşamasında ayrıca paylaşılır.", "Garanti"),
        new("Banyo mobilyası için ölçü nasıl alınır?", "Projeniz için ölçü, yerleşim planı ve tesisat noktalarını birlikte değerlendiriyoruz. Showroom veya iletişim kanallarımız üzerinden teklif talebi oluşturabilirsiniz.", "Proje"),
        new("Teslimat süresi ne kadardır?", "Teslimat süresi ürün modeli, ölçü, renk ve seçilen aksesuarlara göre değişir. Net termin bilgisi teklif ve sipariş onayı sırasında paylaşılır.", "Teslimat"),
        new("Ödeme seçenekleriniz nelerdir?", "Ödeme planı sipariş kapsamına göre belirlenir. Güncel ödeme ve taksit seçenekleri için satış ekibimizle iletişime geçebilirsiniz.", "Sipariş"),
        new("Ankara dışına teslimat yapıyor musunuz?", "Evet. Orpay, proje kapsamına göre Türkiye genelinde sevkiyat planlaması yapar. Montaj uygunluğu için bulunduğunuz konumu satış ekibimizle paylaşmanız yeterlidir.", "Teslimat"),
        new("Özel ölçü kapı üretimi yapıyor musunuz?", "Evet. Projenize özel ölçü ve tasarımlarda kapı yüzeyi ve ahşap çözümleri hazırlıyoruz.", "Proje"),
        new("Hangi malzeme ve renk seçenekleri sunuluyor?", "Koleksiyonlara göre farklı gövde, kapak, tezgâh, lavabo, kulp ve renk seçenekleri sunuyoruz. Güncel seçenekleri showroomumuzda veya katalog üzerinden inceleyebilirsiniz.", "Ürün"),
        new("Lavabo ve ayna ürünle birlikte mi geliyor?", "Lavabo, ayna ve aksesuarların dahil olup olmadığı seçtiğiniz modele ve teklife göre belirlenir. Teklifinizde tüm kalemler açık şekilde yer alır.", "Ürün"),
        new("Montaj hizmeti veriyor musunuz?", "Montaj hizmeti proje lokasyonuna ve sipariş kapsamına göre planlanır. Keşif ve montaj ihtiyacınızı teklif talebinizde belirtebilirsiniz.", "Hizmet"),
        new("Showroomunuzu ziyaret edebilir miyim?", "Evet. Orpay showroomunda koleksiyonları, malzemeleri ve renk alternatiflerini yakından inceleyebilirsiniz. Ziyaret öncesinde randevu oluşturmanız önerilir.", "Showroom"),
        new("Katalogdaki ürünlerde değişiklik yapılabilir mi?", "Birçok modelde ölçü, renk, kapak, lavabo ve aksesuar tercihlerine göre kişiselleştirme yapılabilir. Uygun seçenekler için satış ekibimizle görüşebilirsiniz.", "Ürün"),
        new("Projem için nasıl teklif alabilirim?", "İletişim sayfasındaki teklif formunu doldurabilir veya telefonla bize ulaşabilirsiniz. Ölçü, görsel ve beklentilerinizi paylaşmanız teklif sürecini hızlandırır.", "Teklif"),
        new("Satış sonrası destek sağlıyor musunuz?", "Evet. Ürün ve montajla ilgili ihtiyaçlarınız için satış sonrası destek ekibimizle iletişime geçebilirsiniz.", "Hizmet")
    ];
}

