# G0 · Android cihaz denemesi · 7 Kasım 2026

**Sonuç: geçti.** Godot 4.7.2 C# oyunu gerçek bir Android telefonda kuruldu, açıldı ve altın set destesiyle modelsiz olarak baştan sona oynandı. Deneme sekiz sorun buldu. Sekizi de düzeltildi, otomatik testlere eklendi ve telefonda yeniden denendi.

Deneme Muhammed'in bilgisayarında ve telefonunda yapıldı. Plan maddesi: [geliştirme planı, G0](oyun_gelistirme_plani.md#g0--android-c-risk-kapısı--ek-teknik-öneri). G0, 3.10'u (ilk oynanabilir APK) bitirmez: release imzası ve ikinci telefon orada denenecek.

## Ortam

| | |
|---|---|
| Telefon | POCO X5 Pro 5G (`22101320G`, redwood), Android 14, `arm64-v8a`, 1080×2400, 440 dpi |
| Grafik | Adreno 642L, OpenGL ES 3.2, Godot **Compatibility** işleyicisi |
| Motor | Godot 4.7.2.stable.mono.official, .NET dışa aktarma şablonları 4.7.2.stable.mono |
| C# | Masaüstü `net10.0`; Android `net9.0` ve **C# 13** (Godot şablonu böyle ister) |
| Araçlar | .NET SDK 10.0.403, OpenJDK 21.0.13, Android SDK build-tools 36.1.0, platform android-36.1 |
| APK | Debug, `org.ileriprogramlama.buyruk`, 0.2.0 (kod 2), minSdk 24, targetSdk 36, yalnız arm64, ~124 MB, INTERNET izni yok |

## Nasıl denenir

Game repo kökünden. `<godot-net>` Godot 4.7.2 .NET çalıştırılabilir dosyasıdır; Android SDK ve JDK yolları editör ayarlarında (Editör → Editör Ayarları → Dışa Aktar → Android) tanımlı olmalıdır.

```sh
<godot-net> --headless --path game --export-debug "Android" ../exports/buyruk.apk
adb install -r exports/buyruk.apk
adb shell monkey -p org.ileriprogramlama.buyruk -c android.intent.category.LAUNCHER 1
adb logcat | grep -E "godot|DOTNET|SCRIPT ERROR|USER ERROR|Exception"
```

- `exports/` Git dışındadır; APK repoya girmez.
- **Xiaomi/POCO:** `adb install`, `INSTALL_FAILED_USER_RESTRICTED` verirse Geliştirici seçeneklerinde **USB üzerinden yükle** açılır ve telefondaki onaya basılır.
- Masaüstünde C# derlenip Android'de derlenmeyen kodu erken görmek için: `dotnet build game/Game.csproj -p:GodotTargetPlatform=android`.

## Denenenler

Tüm kontroller son dikey derlemeyle telefonda parmakla (adb dokunma olayları) yapıldı; ilk tur yatay derlemeyle de yapılmıştı.

| Kontrol | Sonuç |
|---|---|
| Kurulum, açılış, .NET çalışma zamanı | ✅ Soğuk açılış ~5 sn; bellek ~270 MB |
| Ekran yönü | ✅ Dikey (`screenOrientation=1`); telefon yana çevrilince de dikey kalıyor |
| Kamera deliği | ✅ İçerik deliğin altından başlıyor (sayaç 66 → 159 piksel; delik 0–94) |
| Türkçe karakterler (İ, ı, ş, ğ, Â) | ✅ Menü, kart, sonuç ve son metinlerinde doğru |
| Paketlenmiş JSON (tema + 600 kartlık deste) | ✅ Altın set kartları yüklendi (`res://Veri/`) |
| Menü: Yeni Oyun, Kayıtlı Oyun, Ayarlar, Emeği Geçenler, Çıkış | ✅ Dokunmayla açılıyor; Geri Dön / Ana Menüye Dön düğmeleri çalışıyor; Çıkış uygulamayı kapatıyor |
| Android geri tuşu | ✅ Alt ekranlar ve hükümdar seçimi → ana menü; ana menü → çıkış; kart/sonuç ekranında etkisiz; son → ana menü |
| Ayarlar: ses kaydırıcısı | ✅ Dokunma %30, sürükleme %85; ekran değişince ve uygulama yeniden açılınca korunuyor (`user://ayarlar.cfg`) |
| Tam ekran düğmesi | ✅ Telefonda gizli (uygulama zaten tam ekran) |
| Oyun ekranı yerleşimi | ✅ Sayaç, dört gösterge ve kartın tamamı kaydırmadan görünüyor |
| Kart: sağa/sola karar, önizleme | ✅ Önizlemede karar, etki okları ve "Seçmek için bırak"; bırakınca karar uygulanıyor |
| Kart: kısa sürükleme, ortaya bırakma, dikey sürükleme, sistem iptali | ✅ Dördü de karar vermiyor |
| Sayfayı parmakla kaydırma | ✅ Taşan ekranlarda çalışıyor |
| Tam oyun | ✅ Üç oyun dokunarak sona kadar oynandı: Beyim hep sağ 9 karar (1 yıl); Hatunum sağ-sol sırayla 13 karar (2 yıl); Hatunum ilk karar sağ, sonra hep sol 18 karar (3 yıl). Üçü de "Para İsyanı" ile bitti |
| Son ekranı | ✅ Tekrar Oyna ve Ana Menüye Dön birlikte görünüyor; ikisi de çalışıyor |
| Arka plana alıp dönme | ✅ Ana Ekran tuşu → geri dönüş: aynı kart ve sayaç |
| Uygulamayı kapatıp yeniden açma | ✅ Ana menüyle açılıyor, ayarlar korunuyor |
| Günlük (logcat) | ✅ Oyunun sürecinde `SCRIPT ERROR`, `USER ERROR` ya da yakalanmamış istisna yok |

Sağ üst kenardaki ince dikey çubuk oyunun değil, MIUI'nin oyunlar için gösterdiği kenar tutamacıdır; telefonun ana ekranında da görünür.

## Bulunan sorunlar ve düzeltmeler

1. **Android'de C# derlenmiyordu** (`CS9260`). Android derlemesi C# 13 kullanır; menü kodundaki iki satır C# 14'e özgü `?.` atamasıydı ve masaüstünde sorun çıkarmıyordu. Satırlar C# 13'e uygun yazıldı. `Game.csproj` dil sürümünü 13'e sabitler; aynı hata artık masaüstü derlemesinde de görünür. (`2a8bd4f`)
2. **Oyun telefonda yatay açılıyordu.** Godot'nun varsayılan yönü yataydır; kartın alt kısmı ekrandan taşıyordu. Oyun **dikeye** kilitlendi ve temel boyut **360×640** oldu (`canvas_items` + `expand`). Bu telefonda mantıksal ekran 360×800'dür ve arayüz birimleri yaklaşık dp gibi okunur: 48 birimlik düğme 48 dp. Karar Muhammed'indir ve toplantıda ekibe duyurulacak. Masaüstü pencere 450×800 açılır ve büyütülebilir.
3. **Dokunmayla ses kaydırıcısı ve sayfa kaydırma çalışmıyordu.** Projede `emulate_mouse_from_touch=false` vardı. Godot'nun kaydırıcısı (`HSlider`) ve kaydırma alanı (`ScrollContainer`) dokunmayı taklit edilen fare olaylarıyla alır. Ayar Godot varsayılanına (açık) döndü.
   - Kart artık taklit fareyi değil, dokunmanın kendisini izler. Böylece parmak kimliği ve dokunma iptali korunur; örneğin sürüklerken bildirim perdesi çekilirse karar verilmez. Kart yana kayarken taklit fare sayfayı aşağı yukarı oynatmaz.
4. **Android geri tuşu her ekranda uygulamayı kapatıyordu** (Godot varsayılanı `quit_on_go_back`). Artık her ekran kendi geri davranışını seçer:

   | Ekran | Geri tuşu |
   |---|---|
   | Ana menü | Uygulamadan çıkar |
   | Alt ekranlar | Ana menüye döner (Esc ve Geri düğmesi gibi) |
   | Hükümdar seçimi, son ekranı | Ana menüye döner |
   | Oyun ortası (kart/sonuç) | Etkisizdir; "Ana Menüye Dön" de yalnız sonda açıktır, kazara yapılan geri hareketi oyunu kaybettirmez. **Güncelleme:** Ömer'in kayıt sistemiyle birleşince "Kaydet ve Ana Menüye Dön" ile aynı işi yapar; ilerleme kaybolmaz |

5. **Dikey telefonda oyun ekranı sığmıyordu.** Kart tek başına ~640 birim tutuyor; başlık bandı ve göstergelerle 800 birimi aşıyordu. Oyun, kartı göstermek için göstergeleri ekranın dışına itiyordu.
   - Kart, kaydırma alanında kendisine kalan yüksekliğe sığacak kadar küçülür; okunurluk için en fazla %75'e iner (`KartGorunumu.EnAzOlcek`). Daha uzun kartta sayfa kayar.
   - Ölçek kartın ortasından uygulandığı için küçülen kart aşağı kayıyordu; bu da düzeltildi.
   - Başlık bandı yalnız hükümdar seçiminde görünür. Böylece oyunda yer karta, son ekranında iki düğmeye kalır.
6. **Kamera deliği sayacın üstüne denk geliyordu.** Android oyunu deliğin çevresine de çizer (`shortEdges`); POCO X5 Pro'da delik üst ortada, yazı o bölgedeydi. Telefonda kaydırma alanları Godot'nun bildirdiği güvenli alanın altından başlar (`GuvenliAlan.cs`); zemin tam ekran kalır. Masaüstünde bu boşluk eklenmez.
7. **Geri tuşu kartta klavye odak çerçevesini açıyordu.** Android geri tuşu bir de tuş olayı gönderir. Kart çerçeveyi artık yalnız gezinme tuşlarında (oklar, Enter, Esc, Tab) gösterir.
8. **Telefonda klavye ipucu gösteriliyordu.** Kartın altındaki "Klavyede ← → ile seç…" yazısı telefonda gizlenir; kartın üstündeki "← Sola veya sağa sürükle →" yeterlidir.

**Testler:** Her düzeltmenin otomatik testi var:
- `SahneIskeletiTesti`: telefondaki olay sırası (önce taklit fare, sonra dokunma), iptal edilen dokunma, geri tuşu, 360×800'de kart ile göstergelerin ve sondaki iki düğmenin sığması, geri/ses tuşunda odak çerçevesi.
- `MenuTesti`: dikey yön ve temel boyut, `quit_on_go_back`, ana menü ve alt ekranlarda geri tuşu, güvenli alan hesabı.

Düzeltme 3, 4, 5 ve 7 geçici olarak geri alındığında ilgili test başarısız oluyor (mutasyon denemesi).

## 8 Kasım · kayıt ve rekorla birleşik sürüm

Ömer'in `kara` dalındaki kayıt/yükleme, rekor ve deste kimlikleri `main` ile birleştirildi (`oyun/kara-main-birlesim`). Aynı telefonda yeniden denendi:

| Kontrol | Sonuç |
|---|---|
| Rastgele başlangıç | ✅ İki oyunun ilk kartları farklı (Kadı İlyas, Saray Hatun Gül); önceden her oyun aynı kartla açılıyordu |
| Geri tuşu oyun ortasında | ✅ İlerlemeyi kaydedip ana menüye döndü |
| Kayıtlı Oyun → devam | ✅ Ayrılırken ve geri yüklenince ekran görüntüsü piksel piksel aynı: kart, karakter adı, sayaç, göstergeler |
| Uygulamayı kapatıp açma | ✅ Kayıt yerinde; oyun kaldığı yerden sürdü |
| Tam oyun 1 (Hatunum, sağ-sol sırayla) | ✅ 35 karar, 7 yıl, "Para İsyanı"; son ekranında "Yeni rekor!" |
| Tam oyun 2 (Beyim, hep sağ) | ✅ 16 karar, 3 yıl, "Halk İsyanı"; son ekranında "Rekorun: 7 yıl · 35 karar" |
| Son ekranı | ✅ Rekor satırıyla birlikte Tekrar Oyna ve Ana Menüye Dön görünüyor |
| Kayıt ekranı | ✅ Devam eden kayıt ikonlu ve tarihli; geçmişte iki hükümdarlık, rekor vurgulu |
| Ayarlar, Emeği Geçenler, geri tuşu, Çıkış | ✅ Ses %85 yeniden kurulumdan sonra da korunmuş |
| Günlük (logcat) | ✅ Oyunun sürecinde hata yok |

Bulunan sorun: rekor satırı iki satır tutunca son ekranında "Ana Menüye Dön" ekranın altına taşıyordu (testlerde kayıt olmadığı için görünmemişti). Rekor tek satıra indi, taç görseli küçüldü, sonda sayfa iki düğmeyi gösterecek kadar kayar. H1 testi artık kayıtlı oyunla 360×800'de son ekranını denetler; düzeltme geri alınınca başarısız oluyor.

## Kalanlar

- **Release çıktısı:** İmza anahtarı (keystore) ekip kararıdır. Anahtar ve parolası repoya girmez. 3.10'da denenecek.
- **APK boyutu:** Debug APK ~124 MB; .NET çalışma zamanı ve görseller. Release ve kırpma ile 3.10'da yeniden ölçülecek.
- **İkinci telefon:** Farklı Android sürümü ve ekran oranıyla 3.10'da.
- ~~**Oyun ortasında çıkış**~~ — kayıt sistemiyle çözüldü: geri tuşu ilerlemeyi kaydedip ana menüye döner.
- **Denge:** Üç farklı oynayışın üçü de "Para İsyanı" ile bitti; hazine çok hızlı tükeniyor. 4.9 denge testine not.
- ~~**Kart sırası**~~ — her hükümdarlık artık destenin rastgele bir olayından başlar; konum kayda yazılır. Rol–konu seçimi 3.7'de.
