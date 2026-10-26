# Game · Godot + C#

İleri Programlama okul projesinin oyun istemcisi. Önceki Unity iskeleti yerine Godot kullanılır. Python/AI ve belgeler kardeş depolardadır.

**Geliştirmeye başlamadan:** [Oyun geliştirme planı](docs/oyun_gelistirme_plani.md) — haftalık görevler, Godot yapısı, kabul testleri ve devir kontrol listeleri.

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
4. Projeyi derleyip **F5** ile `Sahneler/Oyun.tscn` ana sahnesini çalıştırın. Geliştirme önizlemesinde Açılış, Oyun, Sonuç ve Son düğmeleri panelleri değiştirir; henüz oyun kuralları uygulanmaz. **F6 yalnız açık sahneyi çalıştırır**: eski `Baslangic.tscn` açıksa boş pencere ve `C# ortamı hazır` mesajı beklenir.

Repo kökünden derleme:

```sh
dotnet build game/Game.csproj
```

Muhammed'in mevcut çalışma alanına .NET editörü, repoya dahil edilmeden `../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/` altında kuruldu. Bu bilgisayarda **Game repo kökünden** editörü açmak için:

```sh
../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64 --editor --path game
```

Önceden açık standart Steam editörünü projeyi kaydederek kapatın; aynı projeyi iki editörde eşzamanlı düzenlemeyin.

## İlk C# dosyaları

- `game/Game.csproj`: Godot .NET SDK ve hedef framework.
- `game/Baslangic.cs`: `_Ready()` içinde C# çalışma zamanı doğrulaması.
- `game/Baslangic.tscn`: Scriptin bağlı olduğu başlangıç sahnesi.
- `game/project.godot`: Ana sahne, C# ve Compatibility ayarları.
- `game/Sahneler/Oyun.tscn`: Tek ana sahne, dört panel ve geliştirme önizleme düğmeleri (1.7).
- `game/Betikler/Arayuz/OyunEkranlari.cs`: Aynı sahnede yalnız seçili paneli gösterir; oyun durumu/etki hesabı yapmaz.
- `game/Testler/SahneIskeletiTesti.tscn`: Üç pencere boyutunda panel geçişi ve yerleşim kontrolleri.

## Sahne iskeleti testi

Önce `dotnet build game/Game.csproj` çalıştırın. Muhammed'in bilgisayarında **Game repo kökünden**:

```sh
../.tools/Godot_v4.7.2-stable_mono_linux_x86_64/Godot_v4.7.2-stable_mono_linux.x86_64 --headless --path game res://Testler/SahneIskeletiTesti.tscn
```

Diğer bilgisayarlarda aynı argümanları kurulu Godot .NET çalıştırılabilir dosyasına verin. Test başarıda `SAHNE TESTİ BAŞARILI` yazar ve 0 koduyla çıkar; başarısızlıkta hata ve 1 koduyla çıkar. `--headless` olmadan çalıştırıldığında ayrıca `.godot/testler/` altında yerel ekran görüntüleri oluşturur. Bu çıktılar Git'e girmez.

Kontroller: açılışta doğru panel, dört düğmenin sinyal bağlantısı, tek görünür panel/seçili düğme, geçersiz ekran seçiminin reddi ve 1152×648, 360×640, 640×360 yerleşimleri. Bu test **gerçek fare/dokunma/klavye girişini taklit etmez**; düğmenin `Pressed` sinyalini tetikler. Elle Tab/Shift+Tab, Enter/Space, fare tıklaması ve seçili düğmeye tekrar basma da denenmeli. Bu testler APK veya ikinci bilgisayar doğrulaması değildir.

`.godot/`, derleme çıktıları, APK/AAB ve imzalama anahtarları Git dışında tutulur. `.cs.uid`, `.tscn`, `.csproj` ve kaynak `.import` tanımları kaynaklarla birlikte saklanır.

## Android durumu

**Henüz kurulup test edilmedi.** Masaüstü derleme başarısı APK üretildiği anlamına gelmez. Godot 4 C# Android desteği resmî belgelerde hâlâ deneysel olarak belirtiliyor. İlk oyun özelliklerinden önce gerçek cihaz üzerinde küçük bir APK denemesi yapılmalı.

Gerekenler: aynı Godot sürümünün **.NET export templates** paketi, OpenJDK 17 ve Godot Android rehberindeki SDK/NDK araçları. Unity araç sürümleri otomatik olarak devralınmamalı. Android için .NET 9 veya sonrası gerektiği belgelenmiştir; bu projedeki .NET 10 hedefiyle gerçek dışa aktarma ayrıca doğrulanacak.

- https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html
- https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html

## Doğrulama · 26 Ekim 2026

- Resmî Linux .NET editör arşivi SHA-256 özeti, GitHub release API'deki özetle doğrulandı.
- `dotnet build game/Game.csproj`: başarılı.
- Godot .NET ile headless import: başarılı.
- Başlangıç sahnesi headless çalıştırıldı ve C# konsol çıktısı görüldü.
- Grafik editör ve ilk C# sahnesi çalıştırıldı; 1.7 sahne iskeleti üç boyutta headless ve grafik testten geçti.
- İkinci geliştiricinin bilgisayarı, gerçek dokunmatik/klavye etkileşimi ve Android APK henüz doğrulanmadı.
