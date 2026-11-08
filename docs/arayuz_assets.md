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

## 7 Kasım · G0 sonrası dikey telefon düzeni

Oyun dikeydir, temel boyut 360×640 (telefonda mantıksal ekran ~360×800). Gerçek telefon denemesinden sonra:

- Oyun ekranında başlık bandı yalnız hükümdar seçiminde görünür. Oyunda yer karta, son ekranında iki düğmeye kalır.
- Kart, sayaç ve göstergelerle birlikte sığacak kadar küçülür; en fazla %75'e iner, daha uzun kartta sayfa kayar.
- Telefonda kaydırma alanları kamera deliğinin altından başlar; zemin tam ekrandır.
- Kartın altındaki klavye ipucu telefonda gizlenir.

Ayrıntı ve ekran ölçüleri: [Android G0 notu](android_g0.md).

## 8 Kasım · Karakter ve mekân çizimleri

Yeni SVG çizimler kartın üstünde bir sahne olarak gösterilir: konuşan karakter, kendi mekânının önünde durur. Dosyalar `game/Assets/karakterler/` (12 portre) ve `game/Assets/mekanlar/` (6 mekân) altındadır; rol → çizim eşlemesi `game/Betikler/Kartlar/KarakterGorselleri.cs` dosyasındadır.

| Rol (tema.json) | Portre | Mekân |
| --- | --- | --- |
| vezir, kadi, elci, akinci_beyi, kervan_basi | aynı adlı çizim | Yönetim → taht salonu, Ordu → kale surları, kervan başı → kervansaray |
| saray_hatun | `valide_hatun` | taht salonu |
| gozcu_basi | `casusbasi` | kale surları |
| koy_kethudasi | `koy_yaslisi` | köy/tarla |
| dervis | `kam_ata` | köy/tarla |
| hekim | `bas_hekim` | köy/tarla |
| hazinedar | `defterdar` | hazine odası |
| ahi_ustasi | `ahi_babasi` | çarşı |
| Diğer 11 rol | yok (yalnız mekân) | alanına göre; hancı → kervansaray, tüccar ve demirci → çarşı |

- Portre önce portre anahtarıyla (ör. `hekim_kadin`), sonra rol anahtarıyla aranır; "Çift" rollere ileride kadın/erkek ayrı çizim eklemek için eşlemeye bir satır yeterlidir. Şimdiki üç "Çift" çizim (elçi, hekim, casusbaşı) iki cinsiyette de kullanılır.
- Sahne 112 birim yüksekliğindedir. Kart en küçük ölçekte (%75) bile ekrana sığmıyorsa metin küçülmez; önce sahne kısalır, 56 birimin altına inecekse gizlenir. 360×800 telefonda en uzun Altın Set kartları sahneyle birlikte kaydırmadan sığar; 360×640 gibi kısa ekranda sahne kısalır ya da gizlenir.
- `KartGorunumuTesti` her rolün mekânı olduğunu, eşlemedeki her dosyanın bulunduğunu ve portresiz rolde önceki portrenin kalmadığını denetler.

## 8 Kasım · Zeminler ve kullanılmayan parçalar

Zemin her ekranda neredeyse siyah görünüyordu: koyu kumaş, siyaha yakın rengin üstünde %32 opaklıkla döşeniyordu. Artık zemin tam opak bir `NinePatchRect`; arka plan görsellerinin altın köşeleri ekranın köşelerinde kalır. Menüler lacivert desenli, oyun lacivert fırçalı, son ekranı bordo kadife zemindedir (eşleşmeler [paket README'sinde](../game/Assets/ui/README.md)). Son ekranında taç yerine sona sebep olan göstergenin simgesi görünür; son ekranı ve sonuç düğmeleri menülerdeki gibi simgelidir; ana menüde başlığın altında hilalli ayraç vardır.

Paketin 97 parçasından hâlâ kullanılmayanlar, sonraki işler için:

| Grup | Kullanılmayan | Olası yer |
| --- | --- | --- |
| Zemin | `bg_charcoal_stone`, `bg_crimson_pattern`, `bg_gold_brushed`, `bg_parchment`, `bg_dark_fabric` | Kazanma sonu (altın), ekran çeşitliliği |
| Simge | `icon_07` ev, `14` duraklat, `15` onay, `16` iptal, `17`/`18` oklar (18 kullanılıyor), `19` uyarı, `20` kupa, `21` yıldız, `22` kartlar, `23`/`24` artı/eksi | Rekor (kupa), tehlikedeki gösterge (uyarı), ses ayarı (artı/eksi), onay pencereleri |
| Ayraç/rozet | `widget_05` ayraç, `widget_13`/`14` madalyon ve kalkan, `widget_15` parşömen rozet, `widget_16` yıldız | Son başlığı altı, rekor rozeti |
| Çubuk | `widget_01`–`04`, `07`, `08` | Gösterge çubuklarının çerçevesi |
| Perde/bant | `widget_10`, `tab_label_15` | Ana menü başlığı |
| Diğer | `buttons/` 8 parça, `cards/` 7 çerçeve, `panels_frames/` 11 bant, `tab_label` 16 parça, `atlases/` 7 atlas | Atlaslar ayrı PNG'lerin kaynağıdır; oyunda gerekmez |

## 8 Kasım · Taht salonu, konuşma kutusu ve harita

Kart kaydırma kaldırıldı; oyun ekranı artık bir sahnedir. Oyun mantığı (`OyunYoneticisi`: kart, karar, sonuç, son, kayıt) değişmedi.

- **Taht salonu:** `taht_salonu.svg` önünde seçilen hükümdar tahtta oturur (`karakterler/hukumdar_bey.svg`, `hukumdar_hatun.svg`). Kartın konuşanı sağdan yürüyerek gelir, tahtın önünde durur; sıradaki kartta çıkıp yenisi gelir.
- **Konuşma kutusu** (`DiyalogKutusu`): parşömen kutu, lacivert isim plakası. Söz harf harf akar; kutuya dokunmak, tıklamak ya da Enter tamamlar. Altı satırı aşan söz sayfalara bölünür (▼ işareti). Söz bitince iki seçenek açılır, altlarında etkilenen göstergeler ▲ ▼ ile yazar. Sonuç aynı kutuda anlatılır, altında sayısal etkiler ve Devam Et vardır.
- **Harita** (`harita/krallik_haritasi.svg`, `KrallikHaritasi`): üstten bakışlı krallık; saray, hazine, kale, çarşı, kervansaray, köy. Ok tuşları mekândan mekâna atlar, Enter ya da dokunma girer. Göstergesi uçta (0–15 ya da 85–100) olan mekânın adı bordo zemindedir.
- **Mekânlar:** `Veri/mekanlar.json` her mekânın sözcüsünü, göstergesini ve beş aralık için durum cümlesini tutar (kale → ordu, köy → halk, hazine ve çarşı → para, kervansaray → yönetim). Mekânda sözcü ve oranın diğer karakterleri durur; çizim çöküşte soluk, taşkınlıkta sıcak tonlanır. Kartlar yalnız taht salonunda gelir.
- **Yeni çizimler:** eksik 11 rolün portresi (başkâtip, beylerbeyi, soytarı, kale kumandanı, sınır beyi, çiftçi, ozan, çoban, demirci usta, hancı, tüccar) aynı düz SVG tarzında çizildi; 23 rolün hepsinin artık portresi var.
- **Dikey telefon:** göstergeler, sahne, konuşma ve seçenekler 360×800'de kaydırmadan sığar (Altın Set'in en uzun kartları dahil). Harita ve Kaydet ve Çık düğmeleri sahnenin üst köşelerindedir; ayrı satır kaplamaz.

### Aynı gün, ikinci tur: tam ekran sahne, kaydırılan kart, açılır istatistikler

- **Tam ekran sahne:** mekân çizimi bütün ekranın arkasındadır. Çizimler yatay (400×300) olduğu için önemli bölge (taht salonunda tahtın tepesinden konuşanın ayağına; mekânda sakinler) istatistik çubuğu ile kart arasındaki görünür alana sığacak kadar büyütülür; dikey ekranda çizim her zaman ekranın enini doldurur. Çizimin taşmadığı yerler çizimin kendi üst (duvar/gök) ve alt (zemin) rengiyle dolar. Görünür alan her karede izlenir: istatistikler açılınca ya da kart uzayınca sahne yeniden yerleşir.
- **Kaydırılan kart:** konuşma kutusu kartın kendisidir (`DiyalogKutusu` → `KaydirmaKontrolu`). Söz akarken dokunma yazıyı tamamlar; söz bitince kart sağa/sola kaydırılır, yattıkça seçenek ve etki okları görünür, eşikte bırakınca karar verilir. Klavye: ← → önizler, Enter ya da aynı ok onaylar. Seçenek düğmeleri kaldırıldı.
- **İstatistik çubuğu:** üstte dört gösterge simgesi, sayısı ve yıl; uçtaki gösterge kırmızı yazar. Dokununca ayrıntılı çubuklar (ad, değer, yıl/karar) açılır, yine dokununca kapanır; oyun kapalı başlar. Son ekranında ayrıntılı göstergeler her zaman görünür.

### Aynı gün, üçüncü tur: sabit kart, kıpırdamayan sahne

- Kartın metin alanı sabit yüksekliktedir: telefonda 8 satır, daha geniş kartta genişliğe göre en az 4 satır. Eylem alanı da her kartta 100 birimdir; böylece kart söz, sonuç ve mekân kartında aynı boydadır. Destedeki bütün sözler ve sonuçlar telefonda tek sayfaya sığar (H1 testi 1800 metni dener); sığmayan söz eşit sayfalara bölünür.
- Sahnenin görünür alanı istatistik çubuğunun altından kartın üstüne kadardır ve yalnız ekran boyundan hesaplanır: dokunma, kaydırma, sayfa geçişi ya da açılan istatistikler sahneyi büyütüp küçültmez. Ayrıntılı istatistikler yarı saydam lacivert kutuda sahnenin üstüne açılır.
