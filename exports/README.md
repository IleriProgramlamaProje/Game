# BUYRUK APK

[buyruk.apk](buyruk.apk), 8 Kasım 2026 tarihinde üretilmiş Android debug paketidir. 23 Kasım'da mevcut dosya GitHub'a eklenmiştir. Sonraki kaynak kodu ve arayüz değişiklikleri için APK yeniden dışa aktarılmalıdır; bu dosyanın güncel `main`den derlendiği varsayılmaz.

PNG kaynakları gibi APK de **Git LFS** ile saklanır. GitHub dosya sayfasındaki indirme düğmesiyle APK indirilebilir. Depoyu klonlayanlar `git lfs install` ve `git lfs pull` çalıştırmalıdır.

SHA-256: `4df0b77bb9188fefc6db4c744ae40fa63d8f6c746828d2abc2571f625c7e217b`

Yeni debug sürümü Game deposu kökünden Godot .NET ile üretilir:

```sh
<godot-net> --headless --path game --export-debug "Android" ../exports/buyruk.apk
```

APK yenilendiğinde bu tarih ve SHA-256 da güncellenir. İmzalama anahtarları, yerel export kimlik bilgileri, `.idsig` yan dosyaları ve eski `kaydir-karar-ver-h1.apk` Git dışında tutulur.
