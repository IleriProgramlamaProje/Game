# G0 · Android cihaz denemesi · 7 Kasım 2026

**Sonuç: geçti.** Godot 4.7.2 C# oyunu gerçek bir Android telefonda kuruldu, açıldı ve altın set destesiyle modelsiz olarak baştan sona oynandı. Deneme dört sorun buldu. Dördü de düzeltildi, testlere eklendi ve telefonda yeniden denendi.

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

| Kontrol | Sonuç |
|---|---|
| Kurulum, açılış, .NET çalışma zamanı | ✅ Soğuk açılış ~5 sn; bellek ~270 MB |
| Türkçe karakterler (İ, ı, ş, ğ, Â) | ✅ Menü, kart, sonuç ve son metinlerinde doğru |
| Paketlenmiş JSON (tema + 600 kartlık deste) | ✅ Altın set kartları yüklendi (`res://Veri/`) |
| Menü düğmeleri: Yeni Oyun, Kayıtlı Oyun, Ayarlar, Emeği Geçenler | ✅ Dokunmayla açılıyor; ekrandaki geri düğmesi ana menüye dönüyor |
| Android geri tuşu | ✅ Alt ekranlarda ana menüye dönüyor (düzeltme 4) |
| Ayarlar: ses kaydırıcısı | ✅ Dokunma %30, sürükleme %85; ekran değişince ve uygulama yeniden açılınca korunuyor (`user://ayarlar.cfg`) |
| Tam ekran düğmesi | ✅ Telefonda gizli (uygulama zaten tam ekran) |
| Kart sürükleme, önizleme, karar | ✅ Önizlemede karar ve etki okları; bırakınca karar uygulandı (Yönetim +5, Para −5) |
| Tam oyun | ✅ Hep sağa: 9 kararda "Para İsyanı" sonu (masaüstü benzetimiyle aynı); Tekrar Oyna → hükümdar seçimi → yeni oyun |
| Uygulamayı kapatıp yeniden açma | ✅ Ana menüyle açılıyor, ayarlar korunuyor |
| Günlük (logcat) | ✅ Oyundan `SCRIPT ERROR`, `USER ERROR` ya da yakalanmamış istisna yok |

Kart ve tam oyun turu, düzeltme 2'den önce yatay derlemeyle yapıldı. Dikey derlemeyle yeniden denenecek kontroller "Kalanlar" bölümündedir.

## Bulunan sorunlar ve düzeltmeler

1. **Android'de C# derlenmiyordu** (`CS9260`). Android derlemesi C# 13 kullanır; menü kodundaki iki satır C# 14'e özgü `?.` atamasıydı ve masaüstünde sorun çıkarmıyordu. Satırlar C# 13'e uygun yazıldı. `Game.csproj` dil sürümünü 13'e sabitler; aynı hata artık masaüstü derlemesinde de görünür. (`2a8bd4f`)
2. **Oyun telefonda yatay açılıyordu.** Godot'nun varsayılan yönü yataydır; kartın alt kısmı ekrandan taşıyordu. Oyun **dikeye** kilitlendi ve temel boyut **360×640** oldu (`canvas_items` + `expand`). Bu telefonda mantıksal ekran 360×800'dür ve arayüz birimleri yaklaşık dp gibi okunur: 48 birimlik düğme 48 dp. Karar Muhammed'indir ve toplantıda ekibe duyurulacak. Masaüstü pencere 450×800 açılır.
3. **Dokunmayla ses kaydırıcısı ve sayfa kaydırma çalışmıyordu.** Projede `emulate_mouse_from_touch=false` vardı. Godot'nun kaydırıcısı (`HSlider`) ve kaydırma alanı (`ScrollContainer`) dokunmayı taklit edilen fare olaylarıyla alır. Ayar Godot varsayılanına (açık) döndü.
   - Kart artık taklit fareyi değil, dokunmanın kendisini izler. Böylece parmak kimliği ve dokunma iptali korunur; örneğin sürüklerken bildirim perdesi çekilirse karar verilmez. Kart yana kayarken taklit fare sayfayı aşağı yukarı oynatmaz.
   - `SahneIskeletiTesti` telefondaki olay sırasını (önce taklit fare, sonra dokunma) dener. Düzeltme geri alındığında test başarısız oluyor.
4. **Android geri tuşu her ekranda uygulamayı kapatıyordu** (Godot varsayılanı `quit_on_go_back`). Artık her ekran kendi geri davranışını seçer:

   | Ekran | Geri tuşu |
   |---|---|
   | Ana menü | Uygulamadan çıkar |
   | Alt ekranlar | Ana menüye döner (Esc ve Geri düğmesi gibi) |
   | Hükümdar seçimi, son ekranı | Ana menüye döner |
   | Oyun ortası (kart/sonuç) | Etkisizdir; "Ana Menüye Dön" de yalnız sonda açıktır, kazara yapılan geri hareketi oyunu kaybettirmez |

   `MenuTesti` ve `SahneIskeletiTesti` her ekranı dener.

## Kalanlar

- **Release çıktısı:** İmza anahtarı (keystore) ekip kararıdır. Anahtar ve parolası repoya girmez. 3.10'da denenecek.
- **APK boyutu:** Debug APK ~124 MB; .NET çalışma zamanı ve görseller. Release ve kırpma ile 3.10'da yeniden ölçülecek.
- **İkinci telefon:** Farklı Android sürümü ve ekran oranıyla 3.10'da.
- **Oyun ortasında çıkış:** Geri tuşuyla onaylı çıkış istenirse küçük bir onay penceresi gerekir; kayıt (3.9) gelince yeniden düşünülür.
