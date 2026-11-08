# Tahta Çıkmadan Önce — Godot UI Assets v1.1

Kullanıcının sağladığı UI paketi. Bu depodaki gerçek yolu **`res://Assets/ui/`** biçimindedir; Linux/Android üzerinde büyük/küçük harf önemlidir. Özgün PNG dosyaları korunmuştur.

## İçerik
- buttons/: 10 bağımsız PNG
- tabs_labels/: 17 bağımsız PNG
- panels_frames/: 12 bağımsız PNG
- cards/: 10 bağımsız PNG
- icons/: 24 bağımsız PNG
- widgets/: 16 bağımsız PNG
- backgrounds/: 8 bağımsız PNG
- atlases/: 7 orijinal atlas PNG'si

Toplam 104 PNG dosyası (97 ayrı öğe + 7 atlas). PNG'ler Godot 4 tarafından desteklenir.
Paketin üzerine `krallik_tema.tres` eklenerek mevcut `Sahneler/Oyun.tscn` sahnesine bağlandı. PNG dosyaları Git LFS ile saklanır; klonlama sonrası `git lfs pull` çalıştırın.

## Godot'ta kullanım
1. Tema kaynağı `res://Assets/ui/krallik_tema.tres` dosyasıdır. Sahnenin köküne atanır; düğme ve yazı stilleri buradan miras alınır.
2. Butonlar için TextureButton/TextureRect; paneller için TextureRect veya uygun parçalarda NinePatchRect kullan.
3. NinePatchRect kenar paylarını dosyaya göre elle ayarla; her görselin tam 9-slice kullanılabilirliği doğrulanmadı.
4. Metinleri görsellere gömmek yerine `Label` bileşenleriyle ekle.
5. `fonts/` altına Türkçe destekli Noto Serif Regular/Bold (SIL OFL 1.1) ve oklar/eksi işareti için DejaVu Sans yedeği eklendi. Fontlar sistemdeki Debian paketlerinden değiştirilmeden kopyalandı; lisans ve telif kayıtları aynı klasördedir.

## Dikkat
- Görseller 1254×1254 atlaslardan ayrıştırılmış orijinal çözünürlükte PNG parçalardır; yapay olarak büyütülmemiştir. Bu nedenle bütün öğeler 2K/4K değildir.
- Karakter, cami, hükümdar ve manzara illüstrasyonu yoktur.
- Asset'ler daha önce üretilmiş süslü UI atlaslarından alınmıştır; yeni bir görsel üretilmemiştir.
- Godot sahne entegrasyonu yapıldı; masaüstü, dar ve yatay pencere yerleşimleri test edildi. Gerçek Android cihaz testi henüz yapılmadı.

## Sahnedeki eşleşmeler

| Kullanım | Dosya |
| --- | --- |
| Menü zeminleri (Ana Menü, Yeni Oyun, Kayıtlı Oyunlar, Ayarlar, Emeği Geçenler) | `backgrounds/bg_navy_pattern.png` — tam ekran `NinePatchRect`, 42 px altın köşeler korunur; geniş ekranda desen yatayda döşenir |
| Oyun zemini (açılış, kart, sonuç) | `backgrounds/bg_navy_brushed.png` — aynı nine-patch, esnetilir |
| Son ekranı zemini | `backgrounds/bg_crimson_cloth.png` — son ekranı açılınca kodla değişir |
| Ana menüde başlık altı ayraç | `widgets/widget_06.png` |
| Oyun adı | `panels_frames/panel_frame_03.png` |
| Kart ve anlatı alanları | `cards/card_frame_07.png` |
| Normal düğmeler (menü, geri, ana menüye dön) | `buttons/button_04.png` |
| Beyim / Hatunum / Devam Et / Tekrar Oyna / Yeni Oyun | `buttons/button_10.png` |
| Arka deste kartları, Ayarlar paneli | `cards/card_frame_06.png` (lacivert), `cards/card_frame_05.png` (bordo) |
| Menü arması ve düğme ikonları | `icons/icon_01`, `06`, `08`, `10`–`13` |
| Yönetim / Halk / Para / Ordu | `icons/icon_02.png` / `03` / `04` / `05` |
| Son ekranı simgesi | Sebep olan göstergenin simgesi (`icon_02`–`05`); birden çok sebepte `icon_01` |
| Devam Et / Tekrar Oyna ve Veriyi yeniden yükle / Ana Menüye Dön / Kaydet ve Ana Menüye Dön | `icons/icon_18` / `icon_09` / `icon_08` / `icon_06` |

Seçilen iki düğme, üç kart çerçevesi ve başlık görselinin import ayarında `process/size_limit=256` kullanılır. Bu, süslerin nine-patch kenar kalınlığını ekrana uyarlar; kaynak PNG çözünürlükleri değişmez. Diğer resimler özgün import boyutunda durur. Tema hover, basılı, pasif ve klavye odağı stillerini içerir. Sayılar gerçek `ProgressBar` değerleridir; hazır widget'lardaki doluluk oyunun durumu yerine kullanılmaz.

Paketin README ve manifest dosyası kullanıcıdan gelen varlıkların kaynağıdır; ayrı bir görsel lisans belgesi eklenmemiştir. H4'ün portre/son illüstrasyonları ve sesleri bu entegrasyonun kapsamı değildir.
