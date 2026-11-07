# BUYRUK — Menü, göstergeler ve kart etkileşimi

Bu not, ana menünün eklenmesinden kart destesi görünümüne kadar yapılan arayüz çalışmalarının son durumunu açıklar.

## Ana menü ve sahneler

- Oyun adı **BUYRUK**, başlangıç sahnesi `res://Sahneler/AnaMenu.tscn` olarak ayarlandı.
- Yeni Oyun, Kayıtlı Oyun, Ayarlar, Credits ve Çıkış seçenekleri eklendi.
- Yeni Oyun ekranından mevcut oyun sahnesine geçilir.
- Menülerin ortak odak, yerleşim ve geçiş davranışları `MenuSahnesi` temel sınıfında toplandı. `AnaMenu` ve `AltEkran` bu sınıftan türetilir. Görünüm, oyun ekranıyla aynı `Assets/ui/krallik_tema.tres` temasından gelir (bkz. aşağıdaki birleşme bölümü).
- Alt ekranlarda geri düğmesi ve Escape ile ana menüye dönüş bulunur.

## Hükümdar seçimi ve oyun sonu

- Eski hükümdar açılır listesi ve ayrı başlatma düğmesi kaldırıldı.
- **Beyim** ve **Hatunum** düğmeleri doğrudan ilgili hükümdarla oyunu başlatır.
- Oyun verisi yüklenemezse iki seçim de devre dışı kalır.
- Oyun sonunda **Tekrar Oyna**, hükümdar seçimine döner ve yeni oturum başlatılmasını sağlar.
- **Ana Menüye Dön**, BUYRUK ana menüsünü açar.

## Yönetim, Halk, Para ve Ordu

- Dört gösterge için ortak `IGostergeBileseni` arayüzü eklendi.
- `GostergeBileseni`, etiket ve ilerleme çubuğunun güncellenmesini ve 0–100 değer kontrolünü yönetir.
- Gösterge paneli bileşenleri arayüzleri üzerinden kullanır; eksik ve yinelenen anahtarları denetler.
- İçteki ızgarada bulunan bileşenlerin keşfedilmesi için tüm alt düğümler taranır. Bu düzeltme kart etkilerinin çubuklara yansımamasını giderir.

## Kartla karar verme: son davranış

İlk dönüş denemelerinin ardından esas etkileşim yatay sürükleme olarak belirlendi. Son sürümde kart hareketine küçük bir görsel eğim eşlik eder.

1. Oyuncu kartı fare veya dokunmayla tutup sağa ya da sola sürükler.
2. Kart üzerinde ilgili karar metni ve gösterge etki okları görünür; ana anlatım metni korunur.
3. Seçim eşiği geçilmeden bırakılan kart, 0,18 saniyede merkeze ve sıfır eğime döner.
4. Eşik geçilip bırakıldığında kart seçilen yöne 0,2 saniyede çıkar; animasyon sonunda karar bir kez uygulanır ve sonuç ekranı gösterilir.
5. Çıkış animasyonu boyunca yeni sürüklemeler engellenir. Sonraki kart merkezde başlar.

Sol/sağ karar düğmeleri kaldırılmıştır. Dikey hareketle başlayan sürükleme iptal edilir; pencere odağı kaybolduğunda da etkileşim iptal edilir. Klavyede **← / →** kartı o yöne yatırıp önizler, **Enter** ya da aynı ok onaylar, ters ok veya **Esc** vazgeçer; onay aynı çıkış animasyonundan geçer.

### Mesafe ve görsel ayarlar

| Özellik | Son değer |
| --- | --- |
| Seçim eşiği | `max(24, kart genişliği × 0,11)` |
| Tutma sırasında azami yatay yer değiştirme | Seçim eşiğinin 1,5 katı |
| Üst kartın azami eğimi | Sağa/sola 5° |
| Sabit arka kartlar | İki dekoratif panel; −3° ve +2° açılı |

Seçim eşiği ve tutma sırasındaki azami hareket, önceki yatay sürükleme sürümünün yarısına indirilmiştir. Onaydan sonraki ekran dışına çıkış animasyonu bu mesafe sınırından bağımsızdır.

Alttaki iki kart, gerçek kart verisi veya sonraki kararın önizlemesi değildir; deste hissi veren, girdi almayan sabit görsellerdir. Üst kart hareket ettikçe görünür hale gelirler.

## Sorumluluklar

| Bileşen | Görev |
| --- | --- |
| `KaydirmaKontrolu` | Girdi, yatay hareket, hafif eğim, seçim eşiği, iptal ve çıkış animasyonu |
| `KartGorunumu` | Kart metni, karar önizlemesi, deste görünümü ve yerleşim |
| `OyunEkranlari` | Ekranlar arası akış, kararın oyun modeline iletilmesi ve sonuç sunumu |
| `IGostergeBileseni` / `GostergeBileseni` | Ortak gösterge sözleşmesi ve sunumu |

## Doğrulama ve kalan işler

- Bu dalın bileşen testleri ve yeni `KartCevirmeTesti`, birleşik dalda başsız ve Xvfb grafik modda çalıştırıldı; hepsi başarılı (ayrıntı: birleşme bölümü).
- `KartCevirmeTesti.tscn` adı geçmişteki dönüş denemesinden kalmıştır; test iki yönü, yatay hareketi, hafif eğimi, iptali, tek karar uygulanmasını ve gösterge etkilerini kontrol eder.
- Kayıtlı oyun ekranı boş durum gösterir; kayıt/yükleme sistemi henüz uygulanmadı.
- Gerçek dokunmatik cihaz ve farklı fiziksel ekranlarda etkileşim kontrolü hâlâ gereklidir.

## 7 Kasım · `main` ile birleşme (Assets teması + BUYRUK)

`kara` dalı, `main`'deki Assets temasıyla `oyun/arayuz-birlesimi` dalında birleştirildi. Tek çakışma `Sahneler/Oyun.tscn` idi; sahne yapısı bu dalınkiyle (kart yuvası, kart içi önizleme, Beyim/Hatunum, gösterge bileşenleri, ana menüye dönüş), görünüm `main`'inkiyle (kumaş zemin, bant başlık, ikonlu göstergeler, parşömen anlatı alanları) birleşti.

**Düzeltilen hatalar**

- Kart gösterildikten sonra ertelenen yerleşim `MerkezeYerlestir → Iptal()` üzerinden süren çıkış animasyonunu öldürüyordu; hızlı ardışık kararlarda karar hiç uygulanmıyordu. H2, kaydırma ve oyun akışı testlerinin başarısız olma nedeni buydu. Yerleşim artık onay animasyonunu, sürüklemeyi ve klavye önizlemesini bozmadan yalnız merkezi günceller.
- Arka deste kartları `_Ready` sırasında `AddChild` ile ekleniyordu (`Parent node is busy setting up children` hatası). Kartlar artık sahnede `DesteKarti0/1` olarak tanımlı.
- Kart yüksekliği, etiketler yeni genişliğe göre yeniden sarılmadan ölçülüyordu; geniş ekranda kart aşırı uzuyordu. Yükseklik artık asgari boyut her değiştiğinde yeniden uyarlanır.
- Menülerde sabit 480 birimlik sütun 360 genişlikteki telefonda taşıyordu; sütun artık ekran genişliğine uyar, içerik dikey kaydırılabilir.

**Görünüm**

- `OyunEkranlari.TemaUygula` ve `MenuTemasi` içindeki sabit renkli stiller kaldırıldı; tüm ekranlar `krallik_tema.tres` kullanır. Arka kartlar lacivert/bordo kart çerçeveleriyle, ana menü taç arması ve ikonlu düğmelerle, Ayarlar lacivert panelle gösterilir.
- Odak çerçevesi ekran geçişlerinde gizli verilir (`GrabFocus(true)`); yalnız klavyeyle gezinince görünür.

**Ayarlar**

Ses düzeyi ana ses kanalına, tam ekran (yalnız masaüstü) pencere kipine bağlandı ve `user://ayarlar.cfg` içinde saklanır.

**Testler** (başsız / Xvfb grafik): H1 294 / 318, H2 1076 / 1127, Menü 132 / 153 kontrol; Açılış, Kart Görünümü, Kaydırma, Kart Çevirme, Oyun Akışı, Oyun Durumu ve Yedek Deste testleri başarılı. Motor hatası yok. H1'e viewport'a gerçek fare sürüklemesi, çoklu dokunma ve klavye kararı kontrolleri geri eklendi.
