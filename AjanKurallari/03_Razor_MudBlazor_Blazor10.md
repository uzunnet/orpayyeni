---
name: razor-mudblazor-blazor10
description: Blazor 10 + MudBlazor + Razor üretim uzmanı. Partial class zorunluluğu, style/code yasağı, DilServisi, responsive, Blazor 10 yenilikleri (PersistentState, AddValidation, ValidatableType, ReconnectModal, InputHidden, Passkey auth, otomatik asset preloading).
---

# 🔷 RAZOR / BLAZOR 10 / MUDBLAZOR

> **Önkoşul:** [AGENTS.md](../AGENTS.md), [00_PROJE_BILGISI.md](00_PROJE_BILGISI.md), [02_CSharp_Disiplini.md](02_CSharp_Disiplini.md)

---

## 1. 🚫 YASAKLAR

```
❌ .razor içinde @code { } bloğu          → partial class
❌ .razor içinde <style> etiketi           → wwwroot/css/sistem/
❌ Hardcoded Türkçe metin                  → DilServisi.T()
❌ MudBlazor dışında UI lib                → istisna: 09_Coklu_Platform onayı
❌ Hardcoded renk Color="#xxx"              → Color.Primary (tema rolü)
❌ Karmaşık HTML tablo/div                  → MudDataGrid / MudGrid
❌ Inline style attribute (dinamik olmadıkça)
❌ JS doğrudan InvokeAsync                  → Wrapper servis
❌ @inject olmadan Razor sayfa
❌ PDF dosyasını ham tarayıcı sekmesinde açmak → BlazorPdf/PdfGosterici
```

---

## 2. 📁 DOSYA YAPISI

```
Sayfalar/
├── UrunDetay.razor       ← sadece markup
└── UrunDetay.razor.cs    ← partial class kod
```

**`.razor` örneği:**
```razor
@page "/urun/{Slug}"
@inject DilServisi DilServisi
@inject IUrunServisi UrunServisi
@inject NavigationManager NavigasyonYoneticisi

<PageTitle>@DilServisi.T("urun.detay.baslik", "Ürün Detayı")</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="py-8">
    @if (_yukleniyor)
    {
        <MudProgressCircular Indeterminate="true" />
    }
    else if (_urun is null)
    {
        <BulunamadiKomponenti />
    }
    else
    {
        <UrunDetayGorunumu Urun="_urun" />
    }
</MudContainer>
```

**`.razor.cs` örneği:**
```csharp
namespace [PROJE_ADI].UI.Pages.Public;

public partial class UrunDetay
{
    [Parameter] public string Slug { get; set; } = string.Empty;

    private UrunDto? _urun;
    private bool _yukleniyor = true;

    protected override async Task OnInitializedAsync()
    {
        _urun = await UrunServisi.SlugIleGetirAsync(Slug);
        _yukleniyor = false;
    }
}
```

---

## 3. 🌍 DilServisi (Hardcoded Metin Yasak)

```razor
@inject DilServisi DilServisi

<!-- ❌ -->
<MudButton>Kaydet</MudButton>

<!-- ✅ -->
<MudButton>@DilServisi.T("ortak.kaydet", "Kaydet")</MudButton>
```

**Anahtar standardı:** `bolum.alt-bolum.amac`
- `ortak.kaydet`, `ortak.iptal`, `ortak.sil`
- `urun.detay.aciklama`
- `menu.anasayfa`

Çeviriler **DB + FusionCache** (JSON dosya YASAK).

---

## 4. ⚡ BLAZOR 10 YENİLİKLERİ (KULLAN!)

### 4.1 `[PersistentState]` Attribute (KRİTİK!)
Circuit eviction'da state'i otomatik persist + reconnect'te restore:
```csharp
public partial class UrunListele
{
    [PersistentState]
    public string AramaKelimesi { get; set; } = string.Empty;

    [PersistentState]
    public int Sayfa { get; set; } = 1;

    protected override async Task OnInitializedAsync()
    {
        // AramaKelimesi ve Sayfa otomatik restore edilir
        await ListeleAsync();
    }
}
```

### 4.2 `[SupplyParameterFromPersistentComponentState]`
Prerender ↔ interactive geçişte state taşıma (boilerplate'siz):
```csharp
[SupplyParameterFromPersistentComponentState]
public List<UrunOzetDto>? Urunler { get; set; }
```

### 4.3 `AddValidation()` + `[ValidatableType]`
Nested form + collection validation:
```csharp
// Program.cs
builder.Services.AddValidation();

// Model
[ValidatableType]
public class UrunDto
{
    [Required] public string Ad { get; set; } = string.Empty;
    public List<ResimDto> Resimler { get; set; } = [];   // her resim de validate edilir
}
```

### 4.4 `ReconnectModal` Komponenti
Tema ile uyumlu, CSP uyumlu:
```razor
@* App.razor *@
<ReconnectModal>
    <ReconnectingTemplate>
        <MudAlert Severity="Severity.Warning">
            @DilServisi.T("baglanti.kopukluk", "Bağlantı koptu — yeniden deneniyor...")
        </MudAlert>
    </ReconnectingTemplate>
</ReconnectModal>
```

### 4.5 `InputHidden` Komponenti
Form içinde gizli alan (CSRF, tracking):
```razor
<EditForm Model="_dto" OnValidSubmit="KaydetAsync">
    <InputHidden @bind-Value="_dto.Id" />
    ...
</EditForm>
```

### 4.6 NavLink Query/Fragment Ignore
```razor
<NavLink href="/urunler"
         Match="NavLinkMatch.Prefix"
         IgnoreQueryAndFragmentOnMatching="true">
    Ürünler
</NavLink>
```

### 4.7 blazor.web.js Otomatik Asset Preloading
Server side: Link headers
WASM: high-priority download
**Yapılacak:** Hiçbir şey, Blazor 10 otomatik.

### 4.8 WebAssembly Hot Reload
Geliştirmede `.razor.cs` değişikliği anlık yansır — `dotnet watch` zorunlu.

---

## 5. 🎨 MUDBLAZOR KULLANIMI

### 5.1 Bileşen Seçim Tablosu
| Amaç | Bileşen |
|---|---|
| Buton | `<MudButton>` |
| Form input | `<MudTextField>`, `<MudSelect>`, `<MudCheckBox>`, `<MudSwitch>`, `<MudRadioGroup>` |
| Tablo / liste | `<MudDataGrid>` (sıralama/filtre/page hazır) |
| Card | `<MudCard>` |
| Dialog | `<MudDialog>` + `IDialogService` |
| Snackbar | `ISnackbar.Add(...)` |
| Sidebar / Drawer | `<MudDrawer>` + `<MudNavMenu>` |
| Tab | `<MudTabs>` |
| Date picker | `<MudDatePicker>` |
| Chart | `<MudChart>` (basit) veya **LiveCharts wrapper** (gelişmiş) |
| Autocomplete | `<MudAutocomplete>` |
| Tree view | `<MudTreeView>` |

### 5.2 Tema Rolleri (Hardcoded Renk Yasak)
```razor
<!-- ❌ -->
<MudButton Style="background-color: #c19b76">Kaydet</MudButton>

<!-- ✅ -->
<MudButton Color="Color.Primary" Variant="Variant.Filled">Kaydet</MudButton>
```
`MudThemeProvider` `tokens.css` + `00_PROJE_BILGISI.tema.*` üzerinden beslenir.

### 5.3 Form + Validation
```razor
<EditForm Model="_dto" OnValidSubmit="KaydetAsync">
    <FluentValidationValidator />
    <MudTextField @bind-Value="_dto.Ad"
                  Label="@DilServisi.T('urun.ad','Ürün Adı')"
                  Required="true"
                  Immediate="true" />
    <MudButton ButtonType="ButtonType.Submit"
               Color="Color.Primary"
               Variant="Variant.Filled">
        @DilServisi.T("ortak.kaydet", "Kaydet")
    </MudButton>
</EditForm>
```
FluentValidation: `VIZITLINK3D.Api/Moduller/Urunler/Dogrulayicilar/UrunDtoDogrulayici.cs`

### 5.4 Dialog
```csharp
private async Task SilOnayAcAsync(int id)
{
    var param = new DialogParameters
    {
        ["Baslik"] = DilServisi.T("ortak.sil-onay", "Silmek istediğinize emin misiniz?")
    };
    var dialog = await DialogServisi.ShowAsync<SilmeOnayDialogu>("", param);
    var sonuc = await dialog.Result;
    if (!sonuc.Canceled && sonuc.Data is bool onay && onay)
        await SilAsync(id);
}
```

### 5.5 Snackbar
```csharp
[Inject] private ISnackbar Snackbar { get; set; } = default!;

Snackbar.Add(
    DilServisi.T("urun.kaydedildi", "Ürün kaydedildi"),
    Severity.Success);
```

### 5.6 MudDataGrid (Endüstriyel Standart)
```razor
<MudDataGrid Items="@_urunler"
             Filterable="true"
             SortMode="SortMode.Multiple"
             Hover="true"
             Dense="true"
             Striped="true">
    <Columns>
        <PropertyColumn Property="x => x.Ad" Title="@DilServisi.T('urun.ad','Ad')" />
        <PropertyColumn Property="x => x.Fiyat" Title="@DilServisi.T('urun.fiyat','Fiyat')"
                        Format="C2" />
        <TemplateColumn>
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit"
                               OnClick="@(() => DuzenleAsync(context.Item.Id))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
    <PagerContent>
        <MudDataGridPager />
    </PagerContent>
</MudDataGrid>
```

### 5.7 PDF Görüntüleme Standardı — BlazorPdf

Bu projede PDF **görüntüleme** standardı [`BlazorPdf`](https://blazorpdf.info/) + sistem içi `PdfGosterici` sayfasıdır. BlazorPdf, Blazor içeriği içinde inline PDF görüntüleme, sayfalama, yakınlaştırma, yazdırma ve indirme akışlarını destekler; .NET 8/9/10 uyumludur.

**Kural:**
```razor
<!-- ❌ Ham PDF dosyasını yeni sekmede açma -->
<MudButton Href="@pdfUrl" Target="_blank">PDF Aç</MudButton>

<!-- ✅ Sistem içi göstericiye yönlendir -->
<MudButton Href="@PdfGostericiUrl(kayit)">
    @DilServisi.T("belge.goruntule", "Görüntüle")
</MudButton>
```

**Uygulama standardı:**
- Public ve admin ekranlarında PDF linkleri `/pdf-gosterici?dosya=...&baslik=...&donus=...` rotasına gider.
- PDF üretme/raporlama tarafı `QuestPDF` wrapper ile kalır; PDF görüntüleme tarafı `BlazorPdf` ile yapılır.
- `Gotho.BlazorPdf` / `Gotho.BlazorPdf.MudBlazor` doğrudan rastgele sayfalara serpiştirilmez; ortak gösterici veya Türkçe wrapper/bileşen üzerinden kullanılır.
- JPG/PNG belge önizlemeleri de aynı gösterici mantığıyla ele alınır.

---

## 6. 📐 RESPONSIVE

### 6.1 Breakpoint
| Cihaz | Genişlik | UI |
|---|---|---|
| Mobil | < 768px | Bottom nav + hamburger |
| Tablet | 768-1200px | Mini sidebar (72px) |
| Masaüstü | > 1200px | Tam sidebar (260px) |

### 6.2 MudGrid
```razor
<MudGrid>
    <MudItem xs="12" sm="6" md="4" lg="3">
        <UrunKart Urun="@urun" />
    </MudItem>
</MudGrid>
```

### 6.3 MudHidden
```razor
<MudHidden Breakpoint="Breakpoint.SmAndDown">
    <!-- masaüstü -->
</MudHidden>
<MudHidden Breakpoint="Breakpoint.MdAndUp">
    <!-- mobil/tablet -->
</MudHidden>
```

### 6.4 Touch Optimize
- Min buton 44x44 px
- Hover: `@media (hover: hover)` ile sarmala

---

## 7. 🧩 BİLEŞEN MİMARİSİ

### 7.1 Paylaşılan Bileşen
```razor
@* Bilesenler/Ortak/OrtakKart.razor *@
<MudCard Class="@SinifAdi">
    <MudCardContent>
        <MudText Typo="Typo.h5">@Baslik</MudText>
        <MudText Typo="Typo.body2">@Aciklama</MudText>
    </MudCardContent>
    @ChildContent
</MudCard>
```
```csharp
// OrtakKart.razor.cs
public partial class OrtakKart
{
    [Parameter] public string Baslik { get; set; } = string.Empty;
    [Parameter] public string? Aciklama { get; set; }
    [Parameter] public string? SinifAdi { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
```

### 7.2 EventCallback
```csharp
[Parameter] public EventCallback<int> UrunSecildi { get; set; }

await UrunSecildi.InvokeAsync(urun.Id);
```

### 7.3 CascadingParameter
```razor
<CascadingValue Value="_mevcutKullanici">
    @ChildContent
</CascadingValue>

@* Alt bileşen *@
[CascadingParameter] public Kullanici? MevcutKullanici { get; set; }
```

### 7.4 Lifecycle Sırası
```
1. SetParametersAsync
2. OnInitialized / OnInitializedAsync (1 kez)
3. OnParametersSet / OnParametersSetAsync
4. OnAfterRender / OnAfterRenderAsync (DOM hazır — JS interop burada)
5. Dispose / DisposeAsync
```

JS interop **mutlaka** `OnAfterRenderAsync` + `firstRender` check:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
        await UcBoyutMotoru.SahneBaslatAsync(_canvasId);
}
```

---

## 8. ⚡ PERFORMANS

### 8.1 Virtualize (Uzun Liste)
```razor
<Virtualize Items="@_urunler" Context="urun" ItemSize="120">
    <UrunKart Urun="@urun" />
</Virtualize>
```

### 8.2 @key Direktifi
```razor
@foreach (var urun in _urunler)
{
    <UrunKart @key="urun.Id" Urun="@urun" />
}
```

### 8.3 ShouldRender (Dikkatli)
Gerekmedikçe override etme — Blazor zaten optimize.

### 8.4 Lazy Loading Sayfalar
```xml
<ItemGroup>
    <BlazorWebAssemblyLazyLoad Include="AdminSayfalari.dll" />
</ItemGroup>
```

---

## 9. 🔌 JS INTEROP (Wrapper Üzerinden)

### 9.1 Doğrudan Çağrı YASAK
```csharp
// ❌
await JsRuntime.InvokeVoidAsync("threejs.createScene", canvasId);

// ✅
await _ucBoyutMotoru.SahneBaslatAsync(canvasId, modelUrl);
```

### 9.2 Wrapper Örneği
```csharp
public class UcBoyutMotoru(IJSRuntime js) : IAsyncDisposable
{
    private IJSObjectReference? _modul;

    public async Task SahneBaslatAsync(string canvasId, string modelUrl)
    {
        _modul ??= await js.InvokeAsync<IJSObjectReference>(
            "import", "./js/uc-boyut-motoru.js");
        await _modul.InvokeVoidAsync("sahneBaslat", canvasId, modelUrl);
    }

    public async ValueTask DisposeAsync()
    {
        if (_modul is not null) await _modul.DisposeAsync();
    }
}
```

`IJSObjectReference` **mutlaka** dispose edilir (memory leak).

---

## 10. 🛡 GÜVENLİK (UI)

### 10.1 Authorize Attribute
```razor
@page "/yonetim/urunler"
@attribute [Authorize(Roles = "Admin")]
```

### 10.2 AuthorizeView
```razor
<AuthorizeView Roles="Admin">
    <Authorized>
        <MudButton OnClick="YeniEkleAsync">Yeni</MudButton>
    </Authorized>
    <NotAuthorized>
        <p>@DilServisi.T("yetki.yok", "Yetkiniz yok.")</p>
    </NotAuthorized>
</AuthorizeView>
```

### 10.3 XSS Koruması
- `@variable` otomatik escape — güvenli
- `@((MarkupString)html)` **SAKINCALI** — sadece `IcerikTemizleyici` ile sanitize edilmiş içerikte

### 10.4 Passkey Authentication (Blazor 10 Yeni!)
Detay: [07_Guvenlik_Passkey_JWT.md](07_Guvenlik_Passkey_JWT.md).

---

## 11. 📋 ÖZ-DENETİM

```
[ ] .razor + .razor.cs ayrı (partial class)
[ ] .razor içinde @code YOK
[ ] .razor içinde <style> YOK
[ ] @inject DilServisi var
[ ] Tüm metin DilServisi.T() ile
[ ] Hardcoded renk YOK (Color.Primary, vb.)
[ ] MudBlazor bileşenleri
[ ] Responsive (xs/sm/md/lg)
[ ] Liste için Virtualize / @key
[ ] JS Wrapper ile (InvokeAsync doğrudan yok)
[ ] Authorize attribute admin sayfalarda
[ ] FluentValidation form
[ ] Lifecycle (OnAfterRenderAsync firstRender)
[ ] IJSObjectReference dispose
[ ] Blazor 10: [PersistentState] uygun yerde
[ ] Blazor 10: AddValidation() Program.cs'te
```

---

*Versiyon: 1.0 | Bağlı: [02_CSharp_Disiplini.md](02_CSharp_Disiplini.md), [04_CSS_Tema_Stitch_Entegrasyonu.md](04_CSS_Tema_Stitch_Entegrasyonu.md)*
