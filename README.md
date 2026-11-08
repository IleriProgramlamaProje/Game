# Game · BUYRUK · Godot + C#

İleri Programlama okul projesinin oyun istemcisi. Oyunun adı **BUYRUK**. Önceki Unity iskeleti yerine Godot kullanılır. Python/AI ve belgeler kardeş depolardadır.

**Geliştirmeye başlamadan:** [Oyun geliştirme planı](docs/oyun_gelistirme_plani.md) — haftalık görevler, Godot yapısı, kabul testleri ve devir kontrol listeleri.

[Dönem 1 oyun raporu](https://github.com/IleriProgramlamaProje/Docs/blob/main/raporlar/donem-1-oyun.md) (Docs) dönemin işlerini, testleri ve devri, [geliştirme raporu](docs/BUYRUK_DEVELOPMENT_REPORT.md) kayıt/yükleme ve rekor çalışmalarını açıklar.

## Sürümler

- **Godot 4.7.2 stable · .NET sürümü** (standart/Steam sürümü C# içermez).
- **.NET SDK 10.0.402**, `global.json` ile sabitlenmiştir; aynı SDK özellik bandındaki güncel yamalar kabul edilir.
- C# hedefi: **net10.0**.
- İşleyici: **Compatibility / GL Compatibility**.

Godot .NET editörü ve dışa aktarma şablonları resmî sürüm arşivinden alınır: https://godotengine.org/download/archive/4.7.2-stable/

## Açma ve çalıştırma

1. .NET SDK'yı kurun: https://dotnet.microsoft.com/download/dotnet/10.0
2. Godot **.NET** sürümünü indirip arşivden çıkarın; beraberindeki `GodotSharp` klasörünü koruyun.
3. Godot proje yöneticisinde `game/project.godot` dosyasını içe aktarın.
4. Projeyi derleyip **F5** ile oyunu çalıştırın; ana sahne `Sahneler/AnaMenu.tscn`'dir. Oyun **dikeydir**: telefonda dikeye kilitlenir, masaüstünde 450×800 pencereyle açılır (temel boyut 360×640, pencere büyütülebilir). **Yeni Oyun → Başla** ile oyun ekranı açılır. **Beyim** ya da **Hatunum** ile tahta çıkın.
5. Taht salonu bütün ekranı kaplar: hükümdar tahtta oturur, kartın konuşanı yürüyerek gelir ve sözü alttaki kartta harf harf akar. Karta dokunmak, tıklamak ya da **Enter** yazıyı tamamlar, uzun sözde sonraki sayfaya geçer. Söz bitince kartı fare veya parmakla **sağa/sola kaydırın**; kart yattıkça o yöndeki seçenek ve etkilediği göstergeler (▲ ▼) görünür, eşik geçilince **bırakın**, vazgeçmek için ortaya geri getirin. Klavyede **← / →** kartı o yöne yatırıp önizler; **Enter** ya da aynı ok onaylar, ters ok vazgeçer. Üstteki istatistik çubuğu dört göstergeyi ve yılı özetler; dokununca ayrıntılı çubuklar açılır, yine dokununca kapanır.
6. Sonuç aynı kartta anlatılır; **Devam Et** ile sıradaki konuşan gelir. Sahnenin köşesindeki **Harita** üstten bakışlı krallık haritasını açar: ok tuşları mekândan mekâna atlar, **Enter** ya da dokunma içeri girer. Çarşı, kale, köy, kervansaray ve hazine odasında o yerin sözcüsü krallığın o alandaki durumunu anlatır (göstergesi uçtaysa haritada adı kırmızıdır); kartlar yalnız taht salonunda gelir. **Esc** ya da geri tuşu mekândan haritaya, haritadan taht salonuna döner. Bir gösterge 0/100 olduğunda ilgili son açılır; **Tekrar Oyna** yeni hükümdar seçimine, **Ana Menüye Dön** menüye döner. Her beş kabul edilmiş karar bir yıldır. Telefonda **geri tuşu** ekrandaki düğmelerle aynı işi yapar: alt ekranlarda ve hükümdar seçiminde ana menüye döner, taht salonunda **Kaydet ve Çık** gibi ilerlemeyi kaydedip menüye döner, sonda ana menüye döner, ana menüde uygulamadan çıkar.

İlerleme otomatik kaydedilir. **Kayıtlı Oyun** ekranından bekleyen karta veya karar sonucuna dönülebilir. Yeni hükümdarlık önceki kayıtları korur ve destenin rastgele bir olayından başlar; her oyun aynı kartla açılmaz. Tamamlanan oyunlar aynı ekrandaki geçmiş ve rekor bölümüne eklenir; son ekranı yeni rekoru ya da en iyi hükümdarlığı gösterir.

**F6 yalnız açık sahneyi çalıştırır**: `Oyun.tscn` açıkken menüsüz doğrudan oyun, eski `Baslangic.tscn` açıksa boş pencere ve `C# ortamı hazır` mesajı beklenir. Ayarlar (ses düzeyi, masaüstünde tam ekran) `user://ayarlar.cfg` dosyasında saklanır.

Oyun, altın setten seçilmiş 600 kartlık yerel desteyle ağ bağlantısı olmadan oynanır. Deste `python3 araclar/deste_olustur.py` ile kardeş AI deposundaki altın setten yeniden üretilir. [H2 durum ve devir notu](docs/h2_durum.md), kalan ekip/veri işlerini; [veri notu](game/Veri/README.md), deste biçimini ve tema kaynağını açıklar.

Repo kökünden derleme:

```sh
dotnet build game/Game.csproj
```

Muhammed'in mevcut çalışma alanına .NET editörü, repoya dahil edilmeden `../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/` altında kuruldu. Bu bilgisayarda **Game repo kökünden** editörü açmak için:

```sh
../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64 --editor --path game
```

Önceden açık standart Steam editörünü projeyi kaydederek kapatın; aynı projeyi iki editörde eşzamanlı düzenlemeyin.

## Arayüz görselleri

Kullanıcının sağladığı paket `game/Assets/ui/` altındadır. Ana menü, alt menüler ve oyunun açılış, kart, sonuç ve son ekranları lacivert kumaş zemin, altın çerçeveler ve parşömen okuma alanlarıyla ortak `krallik_tema.tres` temasını kullanır. Türkçe Noto Serif ve sembol yedeği lisanslarıyla paketlenmiştir. [Arayüz notu](docs/arayuz_assets.md) görsel eşleşmeleri ve kapsamı açıklar.

PNG kaynakları **Git LFS** ile saklanır. Git LFS kurulu bir bilgisayarda klonlama/güncelleme sonrasında:

```sh
git lfs install
git lfs pull
```

## İlk C# dosyaları

- `game/Game.csproj`: Godot .NET SDK ve hedef framework.
- `game/Baslangic.cs`: `_Ready()` içinde C# çalışma zamanı doğrulaması.
- `game/Baslangic.tscn`: Scriptin bağlı olduğu başlangıç sahnesi.
- `game/project.godot`: Ana sahne (`AnaMenu.tscn`), C# ve Compatibility ayarları.
- `game/Sahneler/AnaMenu.tscn`, `YeniOyun.tscn`, `KayitliOyunlar.tscn`, `Ayarlar.tscn`, `Credits.tscn`: BUYRUK menüleri. `MenuSahnesi` ortak yaşam döngüsü ve geçişleri, `AltEkran` geri/Escape/Android geri tuşu davranışını, `GuvenliAlan` telefonun kamera deliği boşluğunu, `AyarlarEkrani` ve `OyunAyarlari` kalıcı ayarları yönetir.
- `game/Sahneler/Oyun.tscn`: Açılış, tam ekran sahne (taht salonu / mekân) üstünde istatistik çubuğu ve kaydırılan konuşma kartı, harita ve son panelleri.
- `game/Betikler/Arayuz/OyunEkranlari.cs`: Veri yükleme, gösterge/sonuç/son sunumu, giriş ve ekran odağı.
- `game/Betikler/Oyun/OyunDurumu.cs`: Godot'tan bağımsız gösterge değerleri, değişim bildirimleri ve oyun akışı.
- `game/Betikler/Cekirdek/`: Yıl sayacı ve sekiz son kontrolü.
- `game/Betikler/Arayuz/AcilisEkrani.cs`, `Gostergeler.cs` + `GostergeBileseni.cs`: açılış ve gösterge bileşenleri. `SahneGorunumu.cs`: tam ekran mekân çizimi, tahtta oturan hükümdar, yürüyerek gelen konuşan ve mekân sakinleri. `DiyalogKutusu.cs` (`KaydirmaKontrolu.cs` üstüne): harf harf akan, sayfalanan konuşma kartı; söz bitince sağa/sola kaydırma, eğim, çıkış animasyonu ve klavye kararı. `KrallikHaritasi.cs`: üstten bakışlı harita ve mekân düğmeleri.
- `game/Betikler/Kartlar/KarakterGorselleri.cs`, `Mekanlar.cs` ve `game/Veri/mekanlar.json`: rol → portre/mekân eşlemesi; haritadaki mekânlar, sözcüleri ve gösterge aralığına göre durum cümleleri. Çizimler `game/Assets/karakterler/`, `mekanlar/`, `harita/` altındadır.
- `game/Betikler/Kartlar/`: Kart doğrulama, yerel deste, tema ve oturum boyunca sabit karakter adları/hitap.
- `game/Testler/SahneIskeletiTesti.tscn`: H1 çekirdek; tam ekran taht salonu, harf harf akan ve sayfalanan konuşma, kart kaydırma, açılır kapanır istatistikler, yürüyerek gelen konuşan, harita ve mekânlar; fare/dokunma/klavye, telefondaki dokunma sırası, geri tuşu ve beş boyutta (dikey telefon 360×800 dahil) yerleşim kontrolleri.
- `game/Testler/H2OyunTesti.tscn`: Sekiz son, sonuç ekranı, yıl sayacı, yeniden oyun ve bozuk desteden toparlanma.
- `game/Testler/MenuTesti.tscn`: Menülerin teması, dört boyutta yerleşimi, düğme geçişleri, Escape, Android geri tuşu, dikey telefon ayarları, güvenli alan ve ses ayarı.
- `game/Testler/KartCevirmeTesti.tscn`, `KaydirmaKontroluTesti.tscn`: Kartın iki yöne kaydırılması, eğimi, iptali, önizlemesi ve tek karar.
- `game/Testler/GorselVeriTesti.tscn`: Her rolün portresi ve mekânı, iki hükümdar çizimi, harita verisi, durum aralıkları ve bozuk mekân verisinin reddi.

## Sahne iskeleti testi

Önce `dotnet build game/Game.csproj` çalıştırın. Muhammed'in bilgisayarında **Game repo kökünden**:

```sh
../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64 --headless --path game res://Testler/SahneIskeletiTesti.tscn
```

Diğer bilgisayarlarda aynı argümanları kurulu Godot .NET çalıştırılabilir dosyasına verin. Test başarıda `H1 TESTLERİ BAŞARILI` yazar ve 0 koduyla çıkar; başarısızlıkta hata ve 1 koduyla çıkar. `--headless` olmadan çalıştırıldığında ayrıca `.godot/testler/` altında yerel ekran görüntüleri oluşturur. Bu çıktılar Git'e girmez.

Kontroller: kart/deste doğrulama, başlangıç ve sınırlar, hitap/isimler, eski/çift girişin reddi; Godot viewport'una sentetik fare sürüklemesi ve tıklaması, çoklu dokunma, telefondaki taklit fare + dokunma ve klavye (← → Enter, Esc) girdisi; konuşmanın kutuya sığması, uzun metnin eksiksiz sayfalanması; harita adlarının üst üste binmemesi; 1152×648, 923×520, 360×640, 360×800, 640×360 yerleşimleri. Fiziksel dokunmatik, ikinci bilgisayar ve APK doğrulaması ayrıca gerekir.

H2 testi için aynı derlemeden sonra:

```sh
../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64 --headless --path game res://Testler/H2OyunTesti.tscn
```

Başarı çıktısı `H2 TESTLERİ BAŞARILI` ve çıkış kodu 0'dır. Bu test kontrollü desteleri geçici `user://` dosyasına yazar ve temizler; proje destesini değiştirmez. Boş deste testi sırasında üç adet `Deste boş olamaz` uyarısı beklenir. Grafik modunda `.godot/testler/h2/` altına sekiz son ve sonuç ekranları kaydedilir. Linux'ta ekransız grafik kontrolü için komutun başına `xvfb-run -a` eklenebilir ve `--headless` kaldırılır.

Ömer'in bileşen testleri de birleşik sahneye uyarlanmıştır. Aynı Godot komutunda son argüman olarak sırasıyla `res://Testler/OyunDurumuTesti.tscn`, `YedekDesteTesti.tscn`, `GorselVeriTesti.tscn`, `KaydirmaKontroluTesti.tscn`, `KartCevirmeTesti.tscn`, `AcilisEkraniTesti.tscn`, `OyunAkisiTesti.tscn` ve `MenuTesti.tscn` kullanın (her dosya `res://Testler/` altındadır). Menü testi gerçek `user://ayarlar.cfg` dosyasını değiştirirse sonunda eski hâline döndürür.

Tüm testleri tek seferde, Game repo kökünden:

```sh
for t in game/Testler/*.tscn; do ../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64 --headless --path game "res://Testler/$(basename "$t")" || echo "BAŞARISIZ: $t"; done
``` Yedek deste testi hem JSON dizi destesini hem Ömer'in `kartlar` kapsayıcılı, kimlik içeren destesini doğrular.

`.godot/`, derleme çıktıları, APK/AAB ve imzalama anahtarları Git dışında tutulur. `.cs.uid`, `.tscn`, `.csproj` ve kaynak `.import` tanımları kaynaklarla birlikte saklanır.

## Android durumu

**G0 yapıldı (7 Kasım 2026):** debug APK gerçek telefonda (POCO X5 Pro 5G, Android 14, arm64) kuruldu; altın set destesiyle üç tam oyun dokunarak, modelsiz oynandı. Bulunan sekiz sorun düzeltildi ve testlere eklendi: Android'de C# derlenmiyordu (C# 13); oyun yatay açılıyordu (artık dikey); dokunmayla ses kaydırıcısı ve sayfa kaydırma çalışmıyordu; geri tuşu uygulamayı kapatıyordu; dikeyde oyun ekranı sığmıyordu; kamera deliği sayacın üstüne denk geliyordu; geri tuşu odak çerçevesi açıyordu; telefonda klavye ipucu görünüyordu. Ayrıntı, komutlar ve kalanlar: [Android G0 notu](docs/android_g0.md). Ömer'in kayıt/rekor çalışmasıyla birleşik sürüm de 8 Kasım'da aynı telefonda denendi: iki tam oyun, geri tuşuyla kaydedip çıkma, kayıttan birebir dönüş, uygulama kapanınca kaydın korunması, rekor ve geçmiş.

Gerekenler: aynı Godot sürümünün **.NET export templates** paketi, OpenJDK (bu bilgisayarda 21), Android SDK (build-tools 36) ve dışa aktarma ayarındaki **Android** hazır ayarı. Masaüstü `net10.0`, Android `net9.0` / **C# 13** ile ayrı derlenir (Godot şablonu böyle ister); `Game.csproj` dil sürümünü 13'e sabitler, böylece C# 14'e özgü yazım masaüstünde de hata verir. Release çıktısı için imza anahtarı ekip kararıdır; anahtar repoya girmez.

- https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html
- https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html

## Doğrulama · 26 Ekim 2026

- Resmî Linux .NET editör arşivi SHA-256 özeti, GitHub release API'deki özetle doğrulandı.
- `dotnet build game/Game.csproj`: başarılı.
- Godot .NET ile headless import: başarılı.
- Başlangıç sahnesi headless çalıştırıldı ve C# konsol çıktısı görüldü.
- Grafik editör ve ilk C# sahnesi çalıştırıldı; 1.7 sahne iskeleti üç boyutta headless ve grafik testten geçti.
- İkinci geliştiricinin bilgisayarı, gerçek dokunmatik/klavye etkileşimi ve Android APK henüz doğrulanmadı. (7 Kasım: Android APK ve gerçek dokunma doğrulandı, bkz. [G0 notu](docs/android_g0.md).)
