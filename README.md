# Game · Godot + C#

İleri Programlama okul projesinin oyun istemcisi. Önceki Unity iskeleti yerine Godot kullanılır. Python/AI ve belgeler kardeş depolardadır.

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
4. Projeyi derleyip F6/F5 ile başlangıç sahnesini çalıştırın. Konsolda `C# ortamı hazır` görülmelidir. Henüz oyun arayüzü yoktur; boş pencere beklenir.

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
- Grafik pencere, dokunmatik ve Android APK testi henüz yapılmadı.
