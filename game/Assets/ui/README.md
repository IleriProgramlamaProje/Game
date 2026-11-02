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
| Zemin | `backgrounds/bg_dark_fabric.png` — sahnede `AtlasTexture` ile süslü kenarların içi seçilir, düşük opaklıkta döşenir |
| Oyun adı | `panels_frames/panel_frame_03.png` |
| Kart ve anlatı alanları | `cards/card_frame_07.png` |
| Karar düğmeleri, hükümdar seçicisi | `buttons/button_04.png` |
| Tahta Çık / Devam Et / Yeniden Dene | `buttons/button_10.png` |
| Yönetim / Halk / Para / Ordu | `icons/icon_02.png` / `03` / `04` / `05` |
| Son ekranındaki geçici hükümdarlık simgesi | `icons/icon_01.png` |

Seçilen iki düğme, kart ve başlık görselinin import ayarında `process/size_limit=256` kullanılır. Bu, süslerin nine-patch kenar kalınlığını ekrana uyarlar; kaynak PNG çözünürlükleri değişmez. Diğer resimler özgün import boyutunda durur. Tema hover, basılı, pasif ve klavye odağı stillerini içerir. Sayılar gerçek `ProgressBar` değerleridir; hazır widget'lardaki doluluk oyunun durumu yerine kullanılmaz.

Paketin README ve manifest dosyası kullanıcıdan gelen varlıkların kaynağıdır; ayrı bir görsel lisans belgesi eklenmemiştir. H4'ün portre/son illüstrasyonları ve sesleri bu entegrasyonun kapsamı değildir.
