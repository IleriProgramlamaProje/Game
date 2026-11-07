# Assets ile arayüz · 2 Kasım 2026

Kullanıcının sağladığı 104 PNG'lik UI paketi, mevcut Godot oyununun dört ekranına bağlandı. Oyun adı, kartlar, karar kuralları ve H1/H2 akışı korunur. Görsel kaynak eşleşmeleri [paket README'sinde](../game/Assets/ui/README.md), ortak stiller `game/Assets/ui/krallik_tema.tres` dosyasındadır.

## Uygulananlar

- Lacivert kumaş zeminin süssüz iç bölgesi sahnede `AtlasTexture` ile seçilip düşük opaklıkta döşenir; kaynak PNG değiştirilmez.
- Oyun adı altın çerçeveli lacivert bantta; kart, açılış anlatısı, sonuç ve son metinleri parşömen üzerinde koyu mürekkeple gösterilir. Okuma alanlarının kenar boşlukları köşe süslerinden ayrılır.
- Normal düğmeler lacivert; Beyim/Hatunum, Devam Et, Tekrar Oyna ve menüdeki Yeni Oyun bordo stildedir. Hover/basılı/pasif/klavye odağı ayrı stillere sahiptir. (7 Kasım: karar düğmeleri kaldırıldı; karar sürükleme veya ← → Enter ile verilir.)
- Yönetim, Halk, Para ve Ordu için paketteki bina, halk, akçe ve kılıç ikonları kullanılır. Değerler metinle ve gerçek dolum çubuklarıyla gösterilir; hazır doluluk resimleri kullanılmaz. İkonlar giriş yakalamaz.
- Noto Serif Regular/Bold yazı tipleri ve oklar/eksi için DejaVu Sans yedeği uygulamayla paketlenir; sistem fontuna bağlılık kurulmaz. Kaynak ve lisans dosyaları `fonts/` altındadır.
- Sürüklenen kartın odağı, parşömen üzerinde görülebilen koyu bir çizgiyle işaretlenir. Kartı kaydırma, çoklu dokunma engeli ve klavye kararları aynı akışta kalır.

## Düzen

İçerik 640 birim genişliğe kadar ortalanır, dar ekranda yanlarda en az 16 birim kalır. Dört gösterge iki sütundur; etiketler ikonun yanında kalan alanı kullanır ve satıra sarılabilir. Metinler ve düğmeler içerikle büyür; dikey kaydırma ve odağı görünür alana taşıma korunur. Düğmeler en az 48 birimdir. Görünür başlıklar 24, anlatı 18–20, yardımcı metin ve göstergeler 16 birimdir. Kartın sürükleme önizlemesi 16 birimlik metni ve etki oklarını gösterir.

## Doğrulama

- `dotnet build game/Game.csproj`: sıfır uyarı/hata.
- Ömer'in altı bileşen testi başarılı: oyun durumu, iki deste biçimi, kart görünümü, kaydırma, açılış ve oyun akışı.
- H1 ve H2 testleri 1152×648, 360×640 ve 640×360 boyutlarında çalıştırıldı. H1; fare, çoklu dokunma ve Enter girdilerini viewport'a gönderir. H2; sekiz son, yıl, sonuç, yeniden oyun ve bozuk desteden toparlanmayı denetler.
- Xvfb grafik turunda H1 249 ve H2 1127 kontrol başarılı; açılış, dar ekran kaydırma önizlemesi ve yatay sonuç ekranı görsel olarak incelendi. Son süs/metin kenar boşluğu düzenlemesi headless H1/H2 ile ayrıca kontrol edildi.
- PNG dosyaları Git LFS ile saklanır. Klonlayan bilgisayarda Git LFS ve `git lfs pull` gerekir.

Bu çalışma arayüzün ilk görsel entegrasyonudur. Karakter portreleri, sonlara özel illüstrasyonlar, sesler ve fiziksel Android cihaz testi H4/cihaz kabulü kapsamında kalır. Paketlenen görseller kullanıcı tarafından sağlandı; ayrı bir görsel lisans belgesi pakette bulunmuyor. H1'in varsayılan Godot görünümü [tarihsel notta](arayuz_h1_notlari.md) korunur.

## 7 Kasım · BUYRUK menüleri ve deste

`kara` dalındaki BUYRUK menüleri ve desteli kart etkileşimi bu temaya bağlandı; ayrıntılar [BUYRUK notunun](buyruk_arayuz_ve_kart_guncellemeleri.md) birleşme bölümündedir. Yeni eşleşmeler:

| Kullanım | Dosya / tema türü |
| --- | --- |
| Arka deste kartları | `cards/card_frame_06.png` (lacivert, `DeckBackNavy`), `cards/card_frame_05.png` (bordo, `DeckBackCrimson`) |
| Ayarlar paneli | `cards/card_frame_06.png` (`NavyPanel`) |
| Ana menü arması | `icons/icon_01.png` |
| Menü düğme ikonları | Yeni Oyun `icon_13`, Kayıtlı Oyun `icon_06`, Ayarlar `icon_10`, Emeği Geçenler `icon_11`, Çıkış `icon_12`, geri `icon_08` |
| Parşömen üstü metin | `ParchmentLabel`, `ParchmentHeading` |
| Menü başlığı / ikincil metin | `MenuTitle`, `MutedLabel` |

Dikkat: kart çerçevesi dosya numaraları atlastaki sırayla aynı değildir; `card_frame_05` bordo, `card_frame_06` laciverttir. İki dosyanın içe aktarma ayarında, ana kartla aynı nine-patch kalınlığı için `process/size_limit=256` kullanılır.
