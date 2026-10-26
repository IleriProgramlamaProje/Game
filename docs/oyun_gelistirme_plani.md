# Oyun Geliştirme Planı · Godot + C#

**Takvim:** 26 Ekim – 20 Aralık 2026 · **Son güncelleme:** 26 Ekim 2026

Bu belge, oyun ekibinin sırayla uygulayacağı çalışma planıdır. Merkezî `Docs` deposundaki oyun kapsamını, görev numaralarını, tarihleri ve rotasyonu korur; Unity'ye özgü uygulama adımlarını **Godot + C#** karşılıklarına uyarlar. Bir görev burada yazıyor diye uygulanmış sayılmaz.

## 1. Kaynaklar ve kararların önceliği

| Merkezî kaynak | Ne için okunur? |
| --- | --- |
| [yol_haritasi.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/yol_haritasi.md) | Haftalar, görev numaraları, teslimler ve Git çalışma düzeni |
| [kart_formati.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/kart_formati.md) | Kart metni, seçenekler, etkiler, JSON önerisi ve doğrulama kuralları |
| [tema.json](https://github.com/IleriProgramlamaProje/Docs/blob/main/tema.json) | Göstergeler, hitap, karakterler, açılış ve onaylı son metinleri |
| [ekip.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/ekip.md) | Başkan/yardımcı rotasyonu ve rapor sorumluları |
| [kararlar.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/kararlar.md) | Tarihli kararlar; aynı konuda en yeni karar esas alınır |
| [kurulum.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/kurulum.md) | Genel hazırlık; Unity bölümleri Godot kurulumu için kullanılmaz |
| [Game README](../README.md) | Bu reponun gerçek sürümleri ve çalıştırma komutları |

**Çelişki halinde:** Onaylı ürün/veri kuralları burada tek taraflı değiştirilmez. Sorun PR'da belirtilir, iki ekip birlikte netleştirir ve merkezî karar günlüğüne yazılır. Kart JSON'u merkezî belgede hâlâ öneri olarak işaretlidir; mevcut alanlar başlangıç sözleşmesi kabul edilip değişiklikler iki ekiple görüşülür.

**Motor geçişinin sınırı:** Game deposunda Godot kurulmuştur; merkezî belgelerde Unity adları hâlâ vardır. Bu belge, o belgelerin tamamının ekipçe güncellendiği anlamına gelmez. Öğretmen/ekip motor geçişini teyit etmeli. `7.4y` görevindeki Unity Sentis adımı Godot'a aynen taşınamaz; zorunlu teslim kapsamına eklenmez.

**Adlandırma:** Merkezî belgelerde “Muhammet” yazan ekip üyesi bu çalışma alanında Muhammed olarak anılır; aşağıdaki rotasyon aynı kişiyi ifade eder.

## 2. Hedef ürün ve değişmeyecek kurallar

- Türk tarzı krallıkta geçen, birbirinden bağımsız kartlarla karar verilen bir oyun.
- Akış: **Açılış → Kart → Kaydırma/karar → Sonuç → Devam Et → sonraki kart veya son ekranı**.
- Dört gösterge: **Yönetim, Halk, Para, Ordu**. Başlangıç 50, sınırlar 0–100. **100 de kaybetme sonudur**, güvenli hedef değildir.
- Etkiler ±5, ±10, ±15; seçenek başına 1–3 gösterge. İki seçenek de en az bir göstergeyi düşürür.
- Sürükleme sırasında seçenek ve etki **okları**; karar sonrasında sonuç cümlesi ve **sayısal etkiler** gösterilir.
- Kart başlığı unvan + isimdir. İsimler her yeni oyunun başında oyun tarafından atanır; model isim üretmez. Çift cinsiyetli rollerin isim/portre seçimi de oyun başında yapılır.
- Oyuncu seçimine göre `{hitap}` → **Beyim / Hatunum**. Metinde yoksa zorla eklenmez.
- Kart metni hedefi 20–35, üst sınırı 50 kelime; seçenekler 2–5 kelimelik karar cümleleridir. Her seçeneğin sonuç cümlesi vardır.
- Sekiz kaybetme sonu + hedef süreye ulaşınca kazanma sonu; kazandıktan sonra rekor için devam edilebilir.
- **Dizüstü:** Godot istemcisi + yerel Python kart sunucusu; internet gerekmeden çalışır.
- **Android APK:** Ana teslimde yerel yedek desteyle çalışır; telefonda Python/model çalıştırmak zorunlu değildir.
- Oyunda haricî ücretli hazır model API'si kullanılmaz. Model eğitimi/üretimi AI deposunun sorumluluğudur.

## 3. Başlangıç durumu ve ilk yapılacaklar

### Doğrulanan temel

- [x] Özel `IleriProgramlamaProje/Game` reposu ve `main` bağlantısı oluşturuldu.
- [x] Godot **4.7.2 .NET**, **.NET SDK 10.0.402**, **net10.0** ve Compatibility işleyicisi ayarlandı.
- [x] `game/Baslangic.cs` ve `game/Baslangic.tscn` oluşturuldu; C# derlemesi ve sahne çalıştırması doğrulandı.
- [x] Grafik editör ve başlangıç sahnesinin çalıştığı görüldü; konsolda `C# ortamı hazır` yazıyor.

### Sıradaki işler

- [ ] İki oyun ekibi üyesi projeyi kendi bilgisayarında açıp C# çıktısını doğrulasın.
- [ ] Godot motor geçişi ve merkezî belgelerin güncellenmesi ekip toplantısında teyit edilsin.
- [ ] **G0 Android erken denemesi** yapılsın: aşağıdaki risk kapısı.
- [ ] `1.7` tek ana sahne ve ekran panelleri kurulsun.
- [ ] `1.8` oyun durumu ve dört gösterge uygulansın.
- [ ] `1.9` on kartlık yerel örnek deste okunsun.

Henüz kart ekranı, kaydırma, kayıt, model bağlantısı veya APK tamamlanmış değildir. Onay kutuları ancak PR + test kanıtıyla işaretlenir.

### G0 · Android C# risk kapısı — ek teknik öneri

Merkezî takvimde oynanabilir ilk APK `3.10`'dadır; o teslim korunur. **Küçük kurulum denemesi ilk haftaya çekilir**, çünkü Godot 4 C# Android desteği deneysel olarak belgelenmiştir.

- Aynı sürümün **.NET export templates** paketi, OpenJDK 17 ve Godot'un resmî Android araçları kurulur. Unity'nin SDK/NDK sürümleri kopyalanmaz.
- Gerçek telefonda C# ile bir etiket/düğme, dokunma ve paketlenmiş küçük JSON okuma denenir.
- APK'da JSON'un bulunması, Türkçe metinlerin görünmesi ve uygulamanın tekrar açılması kontrol edilir.
- Telefon modeli, Android sürümü, mimari, .NET/SDK araç sürümleri ve hata varsa tam çıktı issue'ya yazılır. Debug testi sonrası teslim için release çıktısı da ayrıca denenir.
- Deneme başarısızsa önce bağımlılıklar/uyumluluk araştırılır; çözüm teslimi tehdit ediyorsa ekip motor kararını yeniden değerlendirir. Çalışmayan APK riski sonraki haftalara sessizce taşınmaz.

Kaynak: [Godot Android dışa aktarma](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html). **Mevcut net10.0 hedefiyle Android dışa aktarımı henüz doğrulanmadı.**

## 4. Ekip ve devir takvimi

Görevler kişi başına parçalanmaz; **başkan + yardımcı eşli çalışır**. Sahne dosyalarının çakışmaması için o an düzenleyen kişi ekipte belirtilir.

| Dönem | Tarihler | Oyun başkanı + yardımcı | Pazar teslimi |
| --- | --- | --- | --- |
| 1 · H1–H2 | 26 Eki – 8 Kas | **Kara** + Muhammed | Yedek desteyle açılıştan sekiz sona kadar oynanabilir oyun |
| 2 · H3–H4 | 9 – 22 Kas | **Muhammed** + Mert | Oynanabilir APK, kayıt, kazanma, görseller, denge raporu |
| 3 · H5–H6 | 23 Kas – 6 Ara | **Mert** + Sokul | Gerçek model bağlantısı ve güvenilir yedek deste geçişi |
| 4 · H7–H8 | 7 – 20 Ara | **Sokul** + Kara | Dış test, son APK ve oyun sunumu |

Başkan pazartesiden itibaren yeni yardımcıya mevcut durumu anlatır. Çarşamba öğle arası toplantısında engeller konuşulur. Dönem raporları **Docs deposundaki** `raporlar/donem-N-oyun.md` dosyasına yazılır: 8 Kas Kara, 22 Kas Muhammed, 6 Ara Mert, 20 Ara Sokul.

## 5. Godot uygulama yapısı — öneri

Aşağıdakiler **hedef yapı**, mevcut dosya listesi değildir. İhtiyaç oluşmadan boş sınıf/dosya üretilmez; basit tutulur.

```text
Game/
├── README.md
├── global.json
├── docs/oyun_gelistirme_plani.md
└── game/                         # Godot res:// kökü
    ├── project.godot
    ├── Game.csproj
    ├── Baslangic.cs / Baslangic.tscn   # mevcut ortam doğrulaması
    ├── Sahneler/
    │   ├── Oyun.tscn             # tek ana sahne
    │   └── Arayuz/               # gerekirse tekrar kullanılabilir alt sahneler
    ├── Betikler/
    │   ├── Cekirdek/             # durum, akış, göstergeler, son, yıl, kayıt
    │   ├── Kartlar/              # kart modeli, kaynaklar, seçim, hitap
    │   ├── Arayuz/               # paneller, kart görünümü, kaydırma
    │   └── Ag/                  # Python HTTP bağlantısı
    ├── Veri/                    # tema.json, yedek_deste.json, sonra rol_konu.json
    ├── Gorseller/               # portreler, sonlar, arayüz
    └── Sesler/
```

- Ana sahne bir `Control` kökü; açılış, oyun, sonuç ve son ekranı ayrı paneller olarak tasarlanır. Alt sahneler kullanılabilir; her kartta yeni ana sahne yüklenmez.
- `OyunDurumu`, gösterge/son hesapları ve kart veri sınıfları mümkün olduğunca Godot düğümlerinden bağımsız C# tutulur. Arayüz iş kurallarının sahibi olmaz.
- `OyunYoneticisi` geçişleri ve kararın **yalnızca bir kez** uygulanmasını yönetir. Sonuç/bekleme/son ekranında yeni kaydırma kabul edilmez.
- Yerel veri **`res://Veri/`**, oyuncu kaydı **`user://`** üzerinden okunur/yazılır. Paketlenmiş `res://` içine kayıt yazılmaz. Export ayarlarında JSON dahil edildiği gerçek çıktıda kontrol edilir.
- `tema.json` ve karakter verilerinin kaynakları Docs/AI depolarıdır. Oyunun çalışması için bu kardeş klasörlere çalışma zamanında bağımlılık kurulmaz; gerekli küçük veriler kaynak revizyonu not edilerek Game içine alınır.
- Yeni ana sahne çalıştıktan sonra `project.godot` ana sahnesi değiştirilir. Mevcut başlangıç sahnesi rastgele silinmez.
- Kaydırma mesafesi ve UI yerleşimi piksel sabitlerine bağlanmadan ölçeklenir; `Control` anchors/container yapıları kullanılır. Ekran yönü ve referans çözünürlük henüz ekip kararı değildir.

### Unity terimlerinin Godot karşılığı

| Eski plan | Bu repo |
| --- | --- |
| `Oyun.unity`, Canvas/paneller | `Oyun.tscn`, `Control`/Container panelleri |
| `MonoBehaviour`, Inspector | Gerektiğinde `Node`/`Control` türevleri, `[Export]` alanlar |
| `StreamingAssets` | Paketlenen `res://Veri/`; kayıt için ayrı `user://` |
| `UnityWebRequest` | Godot C# `HttpRequest` |
| Unity `.meta` varlık kimlikleri | Godot'un ürettiği `.uid`/`.cs.uid` ve kaynak `.import` tanımları |
| Unity Sentis | Doğrudan karşılığı varsayılmaz; ayrı fizibilite, kapsam dışı |

## 6. Haftalık görevler ve kabul ölçütleri

**Görev kodları merkezî yol haritasıyla aynıdır.** Bir göreve başlamadan bağımlılığı tamamlayın. Her satır ilgili haftanın oyun başkanı + yardımcısınındır.

### H1 · 26 Ekim – 1 Kasım — ilk kart kaydırılıyor

Ortak işler `1.0a–1.0d`: kart biçimini okuma, 23 rol için kör tahmin testine katılım, teslim sorumlusu kurası ve karar günlüğü. Merkezî `1.0b` kabul metnindeki eski “15/16 rol” ifadesi 25 Ekim'deki **23 rol** kararıyla çelişir; yeni karar esas alınır.

| Durum | Kod | İş / önerilen parça | Bitti sayılması için | Ön koşul |
| --- | --- | --- | --- | --- |
| [ ] | 1.7 | `Oyun.tscn`, açılış/oyun/sonuç/son panelleri | İki geliştiricinin bilgisayarında açılır, tek ana sahne çalışır | C# kurulumu |
| [ ] | 1.8 | `OyunDurumu`, `Gostergeler`, `GostergeCubugu` | Dört değer 50 başlar; etki doğru göstergeyi değiştirir, 0–100 sınırı test edilir | 1.7 |
| [ ] | 1.9 | `Kart`, `IKartKaynagi`, `YedekDeste` | On kartlık JSON okunur; eksik/bozuk veri kontrollü hata verir | 1.0a |
| [ ] | 1.10 | `KartGorunumu`, `MetinYerlestirici` | Başlık unvan + oturumda sabit isim; hitap doğru, model metnine isim eklenmez | 1.9 |
| [ ] | 1.11 | `KaydirmaKontrolu` | Fare/dokunma önizlemesi; eşik altı sürükleme iptal, eşik üstü tek karar; önizleme durumu değiştirmez | 1.8, 1.10 |
| [ ] | 1.12 | `AcilisEkrani` | Onaylı açılış metni, cinsiyet seçimi ve Tahta Çık çalışır | 1.8 |
| [ ] | 1.13 | `OyunYoneticisi` | Açılış → kart → yeni kart temel döngüsü; çift giriş aynı etkiyi iki kez uygulamaz | 1.11, 1.12 |

### H2 · 2 – 8 Kasım — modelsiz tam oyun

| Durum | Kod | İş | Bitti sayılması için | Ön koşul |
| --- | --- | --- | --- | --- |
| [ ] | 2.5 | `SonucEkrani` | Seçilen sonucun cümlesi + işaretli sayılar + Devam Et; diğer seçeneğin sonucu görünmez | 1.13 |
| [ ] | 2.6 | `SonKontrolu`, `SonEkrani` | Dört göstergenin 0/100 durumunda tema dosyasındaki doğru son; Yeniden Dene tam sıfırlama yapar | 1.8 |
| [ ] | 2.7 | `YilSayaci` | Şimdilik 5 onaylanmış karar = 1 yıl; sürükleme önizlemesi sayılmaz | 1.13 |
| [ ] | 2.8 | Altın setten yedek deste | Onaylı kartlar JSON'a alınır; kaynak/lisans korunur; yerel deste gerçekten kullanılır | 1.9, altın set |
| [ ] | 2.9 | Uçtan uca modelsiz oyun | Açılış → karar → sonuç → Devam Et → son → yeniden oyun; ağ gerekmez | 2.5–2.8 |

**8 Kas devir kapısı:** Sekiz sonun her biri kontrollü test verisiyle gösterilir; çalışan sürüm, yarım işler ve çalıştırma adımları Kara'nın dönem raporundadır.

### H3 · 9 – 15 Kasım — kayıt, kazanma, oynanabilir APK

| Durum | Kod | İş | Bitti sayılması için | Ön koşul |
| --- | --- | --- | --- | --- |
| [ ] | 3.7 | `KartSecici`, rol–konu verisi | İki ekibin onayladığı anahtarlarla konuşan/konu eşleşir; uygunsuz eşleşme seçilmez | karakter kuralları |
| [ ] | 3.8 | Kazanma + rekor | Yapılandırılan hedef yılda kazanma; devam edince aynı kazanma ekranı sürekli tekrarlanmaz | 2.6, 2.7 |
| [ ] | 3.9 | `KayitSistemi` | Kapat/aç sonrası göstergeler, yıl, hitap, karakter atamaları ve gerekli kart/akış bilgisi tutarlı döner; bozuk kayıt kontrollü ele alınır | oyun durumu |
| [ ] | 3.10 | İlk oynanabilir APK | Gerçek Android telefonda modelsiz tam oyun ve yeniden başlatma denenir; G0 tek başına bu görevi bitirmez | G0, 2.9 |

Hedef yıl henüz kesin değildir (`tema.json` içinde `null`). Deneme hedefi gerekiyorsa açıkça test ayarı olarak işaretlenir; ürün kararı gibi yazılmaz.

### H4 · 16 – 22 Kasım — görseller, ses ve denge

| Durum | Kod | İş | Bitti sayılması için | Ön koşul |
| --- | --- | --- | --- | --- |
| [ ] | 4.7 | Görseller | 32 portre + açılış + sekiz son + kazanma ≈42 görsel, tutarlı tarz, doğru rol/cinsiyet eşleşmesi | 23 rol |
| [ ] | 4.8 | Arayüz ve ses | Küçük/büyük ekran, Türkçe karakterler, uzun metin ve dokunma hedefleri denenir; kaynak/lisans notları tutulur | oyun ekranları |
| [ ] | 4.9 | Denge testi | Ekip 10+ oyun oynar; süre, son dağılımı ve sorunlar kaydedilir; hedef yıl/oyun süresi, kart-yıl oranı, etki kademeleri ve oyun adı toplantıda netleşir | 3.10 |

±5/±10/±15 değerleri mevcut kuraldır; denge testi bunları tek taraflı değiştirmez. Değişiklik iki ekiple onaylanır. **22 Kas:** `Docs/raporlar/denge_testi.md` ve Muhammed'in `donem-2-oyun.md` raporu.

### H5 · 23 – 29 Kasım — sahte sunucu ve güvenilir yedek

| Durum | Kod | İş | Bitti sayılması için | Ön koşul |
| --- | --- | --- | --- | --- |
| [ ] | 5.4 | `PythonBaglantisi`, `ModelKartKaynagi` | İki ekip API'yi onaylar; AI sahte sunucusundan gelen kart masaüstünde gösterilir | 5.2 sözleşme, 5.3 sahte sunucu |
| [ ] | 5.5 | Yedek desteye geçiş | Sunucu kapalı, HTTP hatalı, 3 sn zaman aşımı veya bozuk kartta oyun kilitlenmeden yerel kartla devam eder | 5.4 |
| [ ] | 5.6 | Bekleme durumu | İstek sürerken yüklenme görünür; giriş kilitlenir; cevap/fallback sonrası açılır | 5.4 |

### H6 · 30 Kasım – 6 Aralık — gerçek model entegrasyonu

| Durum | Kod | İş | Bitti sayılması için | Ön koşul |
| --- | --- | --- | --- | --- |
| [ ] | 6.5 | Gerçek Python sunucusu | Dizüstünde gerçek model kartı oynanır; sunucu kapatılınca yedek deste akışı bozulmaz | AI 6.4, oyun 5.5 |
| [ ] | 6.6 | Model metni görünümü | Geçerli uzun/kısa metin, hitap, başlık ve sonuçlar taşmadan gösterilir | 6.5 |

**6 Ara devir kapısı:** Model/veri konumu, sürümler, hata senaryoları, gerçek/fallback testi ve çalıştırma adımları Mert'in dönem raporuna yazılır. Ağırlıklar Game reposuna eklenmez.

### H7 · 7 – 13 Aralık — dış test

- [ ] **7.5o:** En az beş ekip dışı oyuncuyla test; cihaz, gözlem, anlaşılmayan kararlar ve hatalar `Docs/raporlar/oyun_testi.md` içine yazılır.
- [ ] **7.6o:** Teslimi engelleyen hatalar önce çözülür; denge değişiklikleri önce/sonra karşılaştırılır. Yeni büyük özellik açılmaz.
- [ ] Modeli telefona gömme veya Godot içi ONNX çalıştırma, zorunlu işlerden zaman çalmaz; ayrı fizibilite olmadan başlanmaz.

### H8 · 14 – 20 Aralık — teslim ve prova

- [ ] **8.1:** Son görsel/ses düzeltmeleri; eksik kaynak/lisans notlarını kapatma.
- [ ] **8.2:** Son APK'yı teslim sorumlusuyla gerçek telefonda internetsiz, yedek desteyle deneme.
- [ ] **8.3:** Dizüstünde Godot + Python model sunucusunu Wi-Fi kapalıyken sıfırdan çalıştırma; sunucu kapalı senaryosunu gösterme.
- [ ] **8.5:** Sokul + Kara oyun bölümünü hazırlar: oynanış, canlı gösterim ve test sonuçları. `8.4` model bölümü YZ ekibindedir.
- [ ] **8.5b:** Herkesle tam prova; model, bağlantı veya cihaz sorunu için yedek gösterim yolu.
- [ ] **8.6:** Teslim sorumlusuyla README'yi güncelleme; başka biri yalnız belgeleri okuyarak kurabilsin.
- [ ] **20 Ara:** Sokul'un `Docs/raporlar/donem-4-oyun.md` raporu, son kaynak commit'i ve test edilen APK sürümü kaydedilsin.

## 7. Veri ve Python bağlantısı

### Kart verisi

Oyun modelin etiketli ham çıktısını değil, Python dönüştürücüsünün ürettiği JSON'u tüketir. Mevcut önerilen alanlar:

| Alan | Oyun tarafı sorumluluğu |
| --- | --- |
| `konusan`, `konu` | Onaylı rol/konu anahtarlarını kullanmak; başlık/portreyi yerel atamadan bulmak |
| `metin` | Türkçe metni göstermek ve yalnız `{hitap}` yer tutucusunu çözmek |
| `sol`, `sag` | Her birinde `etiket`, `etki`, `sonuc` okumak |
| `etki` | Yalnız `yonetim`, `halk`, `para`, `ordu`; sayısal ±5/±10/±15 değerleri; seçenek başına 1–3 etki ve en az bir negatif |
| `kaynak`, `lisans` | Kaynağı/lisansı korumak; `sadece_ders` içeriği yayımlanacak ürün için uygun varsaymamak |

`akinci_beyi` / `akıncı_beyi`, `maas` / `maaş` gibi merkezî örnek farklılıkları için kendiliğinden anahtar dönüşümü yapılmaz. İki ekip `tema.json`/karakter verisiyle uyumlu tek anahtar listesini sözleşmede kesinleştirir. Eski örnekteki “Defterdar” yerine güncel karakter listesindeki “Hazinedar” kararı esas alınır.

Yerel destenin dosya kapsayıcısı (JSON dizi vb.), sürüm bilgisi ve boş deste davranışı ilk veri PR'ında belgelenir. Deste bitince ne olacağı/tekrar sıklığı da seçim kuralı olarak netleştirilir; belgelerde yokmuş gibi yeni zorunluluk uydurulmaz.

### HTTP sözleşmesi — merkezî öneriyi koruyan taslak

- Masaüstünde yerel adres: `http://localhost:8000`.
- `POST /kart` gövdesi: `yonetim`, `halk`, `para`, `ordu`, `konu`, `konusan`.
- Başarılı cevap: yukarıdaki kart JSON'u; **3 saniyede yanıt yoksa yedek deste**.
- Yetkili son sözleşme **Docs deposunda ileride oluşturulacak `api.md`** dosyasıdır; şu an tamamlanmış dosya gibi kabul edilmez. `5.2` sırasında HTTP durumları, zorunlu alanlar ve örnekler iki ekiple onaylanır.
- Godot tarafında `HttpRequest`, asenkron tamamlanma ve açık zaman aşımı kullanılır; ana iş parçacığı bloklanmaz. Aynı istek/kart için etki iki kez uygulanmaz.
- Geç gelen cevap, yeniden başlatılmış oyun veya gösterilmiş fallback kartını ezmemeli; istek iptali/oturum eşleştirmesi test edilir.
- Ham metnin dil/biçim doğrulaması AI tarafındadır; istemci yine eksik alan, geçersiz gösterge/etki ve bozuk JSON'u reddeder.
- **Telefonun `localhost` adresi dizüstü değildir.** Ana APK teslimi offline destedir. Ağlı telefon denemesi ayrıca yapılırsa sunucu adresi ve Android INTERNET izni ayrıca ayarlanır; yerel sunucu gereksiz yere internete açılmaz.

## 8. Test ve “bitti” tanımı

Repo kökünde mevcut derleme komutu:

```sh
dotnet build game/Game.csproj
```

F5: ana sahne; F6: açık sahne. Oynanış testi F5 ile yapılır. Muhammed'in bilgisayarına özgü editör komutu [README](../README.md) içindedir; `.tools` diğer arkadaşlarda otomatik var sayılmaz.

**Henüz otomatik oyun test projesi yok.** İlk çekirdek mantık PR'ında test yaklaşımı seçilip çalıştırma komutu README'ye eklenir. Test projesi olmadan `dotnet test` çalıştırmak testlerin varlığını/başarısını kanıtlamaz. Çekirdek hesaplar için otomatik, sahne/giriş/telefon için manuel test birlikte kullanılır.

Her ilgili PR'da şu senaryoların sonucu yazılır:

- [ ] Gösterge başlangıçları 50; negatif/pozitif etkiler ve sınır aşımı; 0 ve 100 sonları.
- [ ] Seçenek önizlemesi değerleri değiştirmiyor; karar bir kez uygulanıyor; çift tık/çoklu dokunma tekrarlamıyor.
- [ ] İki sınır aynı kararda aşılırsa hangi sonun seçileceği; kazanma ile kaybetme aynı turdaysa öncelik **henüz açık karar**. Kural belirlenip test edilmeli.
- [ ] Sonuç cümlesi → Devam Et → gerekiyorsa son ekranı sırası, yıl sayımı ve yeniden başlatma.
- [ ] Hitap, rol/isim/portre oturum tutarlılığı; Türkçe karakterler ve uzun metin.
- [ ] Bozuk/boş deste, bozuk kayıt, kayıt sonrası devam; kullanıcıya anlaşılır hata.
- [ ] Sunucu kapalı, yavaş, hatalı JSON/HTTP; fallback ve geç gelen cevap.
- [ ] Export edilmiş oyunda JSON ve görseller var; APK gerçek cihazda offline çalışıyor.

**Bir görevin bitmesi için:** derleme başarılı, kabul ölçütü denenmiş, ilgili test eklenmiş/sonucu yazılmış, yeni hata yok, çalıştırma belgesi güncel ve PR en az bir kişi tarafından incelenmiş olmalı. Göreve göre otomatik test veya manuel kanıt seçilir; yapılmayan doğrulama açık yazılır.

## 9. Git, iş takibi ve dönem teslimi

1. Güncel `main` üzerinden `oyun/<konu>` dalı açın; tek PR'da tek anlaşılır iş yapın.
2. Issue/PR başlığında görev kodunu kullanın: `1.8 — Gösterge hesapları` gibi. Bağımlılığı ve kabul ölçütünü yazın.
3. Commit mesajını Türkçe ve yapılan işe özel yazın. Ekip geliştirmesinde doğrudan `main`'e yazmayın.
4. PR'a test adımları/sonuçları ve UI değişiyorsa ekran görüntüsü ekleyin; en az bir onaydan sonra birleştirin.
5. Görevin onay kutusunu yalnız birleşen ve doğrulanan iş için işaretleyin. Yarım işler issue/dal bağlantısıyla yazılır.
6. Her pazar haftalık etiketi ekipçe `hafta-N` olarak oluşturun; etiketin gerçekten test edilen commit'i gösterdiğini kontrol edin.

`.godot/`, `bin/obj`, APK/AAB çıktıları, imzalama anahtarları ve şifreler Git'e girmez. `.cs.uid`, sahneler, `.csproj`, `project.godot` ve kaynak varlıklar takip edilir. Görsel/ses eklemeye başlamadan **Game reposunda** Git LFS kuralları ayrıca kurulup kontrol edilir; eski Unity reposundaki ayarların buraya taşındığı varsayılmaz. Model ağırlıkları Hugging Face/Drive'da kalır. Test edilen APK ekipçe seçilen dağıtım yerinde tutulur, rapora bağlantı + kaynak commit'i yazılır.

Dönem sonunda başkanın kontrol listesi:

- [ ] `main` çalışıyor; tamamlanan işler PR ile birleşti.
- [ ] Yarım işler, engeller ve ilgili dallar/issue'lar yazıldı.
- [ ] Yeni yardımcı için ilk üç adım ve güncel kurulum/çalıştırma komutları yazıldı.
- [ ] Denenen cihazlar, testler ve APK/model/veri konumları raporda mevcut.
- [ ] Ürün kuralı/API değiştiyse merkezî Docs güncellendi; Game kopyası/sözleşmesiyle çelişmiyor.
- [ ] Dönemin devir paketi tamam; `Docs/raporlar/donem-N-oyun.md` teslim edildi.

## 10. Karar bekleyenler

Aşağıdakiler bu planla kendiliğinden kesinleşmez:

- Merkezî belgelerde Godot geçişinin ve öğretmenin motor beklentisinin teyidi.
- Gerçek cihazda Godot C# Android/net10.0 uyumluluğu; export araçlarının test edilmiş sürümleri.
- 28 Ekim kurasıyla teslim sorumlusu.
- Ekran yönü, referans çözünürlük ve desteklenecek telefonlar.
- Rol/konu anahtarlarının yazımı, deste dosyası kapsayıcısı ve seçim/tekrar politikası.
- Aynı kararda birden fazla sonun önceliği; kayıt alma anı ve bozuk kayıt davranışı.
- Hedef yıl/oyun süresi, kart-yıl oranının son değeri ve oyun adı — H4 denge testi.
- İki ekibin onaylayacağı `api.md` — H5.

Bu kararlar toplantıda netleştiğinde ilgili görev, test ve merkezî karar günlüğü birlikte güncellenir.
