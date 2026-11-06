# BUYRUK — Menü, göstergeler ve kart etkileşimi

Bu not, ana menünün eklenmesinden kart destesi görünümüne kadar yapılan arayüz çalışmalarının son durumunu açıklar.

## Ana menü ve sahneler

- Oyun adı **BUYRUK**, başlangıç sahnesi `res://Sahneler/AnaMenu.tscn` olarak ayarlandı.
- Yeni Oyun, Kayıtlı Oyun, Ayarlar, Credits ve Çıkış seçenekleri eklendi.
- Yeni Oyun ekranından mevcut oyun sahnesine geçilir.
- Menülerin ortak tema, odak ve geçiş davranışları `MenuSahnesi` temel sınıfında toplandı. `AnaMenu` ve `AltEkran` bu sınıftan türetilir; görsel biçimlendirme `MenuTemasi` tarafından yapılır.
- Alt ekranlarda geri düğmesi ve Escape ile ana menüye dönüş bulunur.

## Hükümdar seçimi ve oyun sonu

- Eski hükümdar açılır listesi ve ayrı başlatma düğmesi kaldırıldı.
- **Beyim** ve **Hatunum** düğmeleri doğrudan ilgili hükümdarla oyunu başlatır.
- Oyun verisi yüklenemezse iki seçim de devre dışı kalır.
- Oyun sonunda **Tekrar Oyna**, hükümdar seçimine döner ve yeni oturum başlatılmasını sağlar.
- **Giriş Sayfasına Dön**, BUYRUK ana menüsünü açar.

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

Sol/sağ karar düğmeleri kaldırılmıştır. Dikey hareketle başlayan sürükleme iptal edilir; pencere odağı kaybolduğunda da etkileşim iptal edilir.

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

- `dotnet build game/Game.csproj` son değişikliklerle hatasız ve uyarısız tamamlandı.
- Buton kullanan ilgili testler sürükleme ve animasyon süresini bekleme akışına uyarlandı.
- `KartCevirmeTesti.tscn` adı geçmişteki dönüş denemesinden kalmıştır; test artık iki yönü, yatay hareketi, hafif eğimi, iptali, tek karar uygulanmasını ve gösterge etkilerini kontrol edecek şekilde düzenlenmiştir.
- Godot çalıştırma denemelerinden testin tamamlandığını gösteren çıktı alınamadı. Çalışma zamanı testlerinin geçtiği iddia edilmemektedir; masaüstünde görsel kontrol, gerçek dokunmatik cihaz ve farklı ekran boyutlarında etkileşim kontrolü gereklidir.
- Kayıtlı oyun ekranı boş durum gösterir; kayıt/yükleme sistemi henüz uygulanmadı.
- Ayarlar ekranındaki ses ve tam ekran kontrolleri henüz gerçek ayarlara bağlanmadı; ayar kalıcılığı yoktur.
- Credits ekranındaki genel ekip metni ileride gerçek katkı bilgileriyle güncellenebilir.
