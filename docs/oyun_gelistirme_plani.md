# Oyun Geliştirme Planı · Godot + C#

**Takvim:** 26 Ekim – 20 Aralık 2026 · **Son güncelleme:** 2 Kasım 2026

Bu belge, oyun ekibinin sırayla uygulayacağı çalışma planıdır. Merkezî `Docs` deposundaki oyun kapsamını, görev numaralarını, tarihleri ve rotasyonu korur; Unity'ye özgü uygulama adımlarını **Godot + C#** karşılıklarına uyarlar. Bir görev burada yazıyor diye uygulanmış sayılmaz.

## 1. Kaynaklar ve kararların önceliği

| Merkezî kaynak | Ne için okunur? |
| --- | --- |
| [yol_haritasi.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/yol_haritasi.md) | Haftalar, görev numaraları, teslimler ve Git çalışma düzeni |
| [kart_formati.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/kart_formati.md) | Kart metni, seçenekler, etkiler, JSON önerisi ve doğrulama kuralları |
| [tema.json](https://github.com/IleriProgramlamaProje/Docs/blob/main/tema.json) | Göstergeler, hitap, karakterler, açılış ve onaylı son metinleri |
| [ekip.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/ekip.md) | Başkan/yardımcı rotasyonu ve rapor sorumluları |
| [kararlar.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/kararlar.md) | Tarihli kararlar; aynı konuda en yeni karar esas alınır |
| [kurulum.md](https://github.com/IleriProgramlamaProje/Docs/blob/main/kurulum.md) | Üç depo, Python ve Godot kurulumu; eski Unity araştırması arşivdedir |
| [Game README](../README.md) | Bu reponun gerçek sürümleri ve çalıştırma komutları |

**Çelişki halinde:** Onaylı ürün/veri kuralları burada tek taraflı değiştirilmez. Sorun PR'da belirtilir, iki ekip birlikte netleştirir ve merkezî karar günlüğüne yazılır. Kart JSON'u merkezî belgede hâlâ öneri olarak işaretlidir; mevcut alanlar başlangıç sözleşmesi kabul edilip değişiklikler iki ekiple görüşülür.

**Motor geçişinin sınırı:** Game deposunda Godot kurulmuştur; merkezî güncel kurulum ve rehberler bu duruma uyarlandı. Tarihli Unity kararları ve araştırmaları geçmiş kayıt olarak korunur. Bu teknik güncelleme ekip/öğretmen onayı yerine geçmez. Öğretmen/ekip motor geçişini teyit etmeli. `7.4y` görevindeki Unity Sentis adımı Godot'a aynen taşınamaz; zorunlu teslim kapsamına eklenmez.

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
- [ ] Godot motor geçişi ve güncellenen merkezî belgeler ekip toplantısında teyit edilsin.
- [ ] **G0 Android erken denemesi** yapılsın: aşağıdaki risk kapısı.
- [ ] `1.7` tek ana sahne ve ekran panelleri: yerel uygulama hazır (`Sahneler/Oyun.tscn`, `OyunEkranlari.cs`), üç boyutta headless/grafik test geçti. İkinci geliştiricinin bilgisayarı ve gerçek giriş kontrolleri bekliyor; görev henüz kapalı değil.
- [ ] `1.8` oyun durumu ve dört gösterge: yerel uygulama ve otomatik sınır testleri hazır; inceleme/birleşme bekliyor.
- [ ] `1.9` on kartlık yerel örnek deste okunuyor; bozuk veri ve başa dönüş testleri hazır; inceleme/birleşme bekliyor.

Yerel H1/H2 uygulamasında kart gösterimi, kaydırma, sonuç, sekiz son ve yıl sayacı örnek desteyle çalışır. Onaylı altın set, kayıt, model bağlantısı ve tam oyun APK doğrulaması bekler. Ayrıntılar [H2 durum notundadır](h2_durum.md). Onay kutuları ancak PR + test kanıtıyla işaretlenir.

### G0 · Android C# risk kapısı — ek teknik öneri

Merkezî takvimde oynanabilir ilk APK `3.10`'dadır; o teslim korunur. **Küçük kurulum denemesi ilk haftaya çekilir**, çünkü Godot 4 C# Android desteği deneysel olarak belgelenmiştir.

- Aynı sürümün **.NET export templates** paketi, OpenJDK 17 ve Godot'un resmî Android araçları kurulur. Unity'nin SDK/NDK sürümleri kopyalanmaz.
- Gerçek telefonda C# ile bir etiket/düğme, dokunma ve paketlenmiş küçük JSON okuma denenir.
- APK'da JSON'un bulunması, Türkçe metinlerin görünmesi ve uygulamanın tekrar açılması kontrol edilir.
- Telefon modeli, Android sürümü, mimari, .NET/SDK araç sürümleri ve hata varsa tam çıktı issue'ya yazılır. Debug testi sonrası teslim için release çıktısı da ayrıca denenir.
- Deneme başarısızsa önce bağımlılıklar/uyumluluk araştırılır; çözüm teslimi tehdit ediyorsa ekip motor kararını yeniden değerlendirir. Çalışmayan APK riski sonraki haftalara sessizce taşınmaz.

Kaynak: [Godot Android dışa aktarma](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html). **Mevcut net10.0 hedefiyle Android dışa aktarımı henüz doğrulanmadı.**

## 4. Ekip ve devir takvimi

Merkezî plana göre **başkan + yardımcı eşli çalışır**; aynı alandaki iki kişi bütün görevlerden birlikte sorumludur. Aşağıdaki kişi bazlı tablolar bu atamayı görünür kılar; işi yalnız tek kişinin bildiği parçalara ayırmaz. Sahne dosyalarının çakışmaması için o an düzenleyen kişi ekipte belirtilir.

**Çalışma önerisi:** Başkan haftanın sırasını, entegrasyonu ve teslim kontrolünü yönlendirir; yardımcı uygulama, test ve belgelemeyi birlikte yürütür. Kod yazan/test eden rolleri görevler arasında değiştirilir. Bu uygulama önerisi, merkezî plandaki onaylı rol ve görev atamalarını değiştirmez.

| Dönem | Tarihler | Oyun başkanı + yardımcı | Pazar teslimi |
| --- | --- | --- | --- |
| 1 · H1–H2 | 26 Eki – 8 Kas | **Kara** + Muhammed | Yedek desteyle açılıştan sekiz sona kadar oynanabilir oyun |
| 2 · H3–H4 | 9 – 22 Kas | **Muhammed** + Mert | Oynanabilir APK, kayıt, kazanma, görseller, denge raporu |
| 3 · H5–H6 | 23 Kas – 6 Ara | **Mert** + Sokul | Gerçek model bağlantısı ve güvenilir yedek deste geçişi |
| 4 · H7–H8 | 7 – 20 Ara | **Sokul** + Kara | Dış test, son APK ve oyun sunumu |

Başkan pazartesiden itibaren yeni yardımcıya mevcut durumu anlatır. Çarşamba öğle arası toplantısında engeller konuşulur. Dönem raporları **Docs deposundaki** `raporlar/donem-N-oyun.md` dosyasına yazılır: 8 Kas Kara, 22 Kas Muhammed, 6 Ara Mert, 20 Ara Sokul.

### 4.1 Hafta hafta kişi bazlı görev dağılımı

Bu bölüm **dört kişinin bütün haftasını** gösterir. Oyun görevlerinin ayrıntılı kabul ölçütleri bölüm 6'dadır; YZ görevlerinin ayrıntıları merkezî yol haritasındadır ve **AI reposunda** uygulanır. Tablodaki görev aralıkları başkan ile yardımcının **ortak sorumluluğudur**. Bunlar yapılacak iş atamalarıdır, tamamlanma kaydı değildir.

#### Hazırlık · 22–25 Ekim — eksikse H1 başında tamamlanır

| Kişi | Sorumluluk |
| --- | --- |
| Mert | Koordinasyon, repo/Drive erişimleri (`Hz.1`); öğretmen hız ölçümü ve üretici çalıştırma (`Hz.3–Hz.5`); kendi kurulum/torba işleri (`Hz.2`, `Hz.6`) |
| Sokul | Kendi kurulum eksikleri ve ortak torba hazırlığı (`Hz.2`, `Hz.6`); H1'de Mert ile YZ üretim hattına katılım |
| Kara | Kendi kurulum eksikleri ve ortak torba hazırlığı (`Hz.2`, `Hz.6`); oyun ekibinin Godot sürüm/kurulum kontrolü |
| Muhammed | Kendi kurulum eksikleri ve ortak torba hazırlığı (`Hz.2`, `Hz.6`); Game reposundaki C# başlangıcını Kara'ya aktarma |

#### H1 · 26 Ekim – 1 Kasım

| Kişi | Alan · rol / eş | Atanan görevler | Hafta içi ek sorumluluk / çıktı |
| --- | --- | --- | --- |
| Mert | YZ **başkanı** / Sokul | `1.1–1.6`: Kart biçimi dönüştürücüsü, 20 kart prompt denemesi, torba kontrolü, 500 hikâye + 100 kart denemesi, filtre, büyük üretim | `Hz.3` Colab hız ölçümü; gece üretimi ve sayı takibi; genel koordinasyon |
| Sokul | YZ yardımcısı / Mert | `1.1–1.6`: Aynı üretim hattı, dönüştürücü/filtre testleri ve veri kontrolü | Mert ile Colab gece üretimi; oyun ekibine onaylı örnek kart JSON'u sağlama |
| Kara | Oyun **başkanı** / Muhammed | `1.7–1.13`: Godot ana sahne/paneller, göstergeler, yerel kart verisi, kart ekranı, kaydırma, açılış ve oyun akışı | `G0` erken Android denemesini Muhammed ile yürütme; kendi bilgisayarında 1.7 ve gerçek giriş kontrolleri |
| Muhammed | Oyun yardımcısı / Kara | `1.7–1.13`: Aynı oyun görevleri; mevcut sahne iskeletinden göstergelere ve kart döngüsüne ilerleme | `G0` Android araçları/cihaz denemesi ve test kanıtı; 1.7'nin yerel testlerini Kara'ya anlatma |

**Herkes:** `1.0a–1.0d` kart biçimini okuma, karakter kör tahmin testi, **28 Ekim teslim sorumlusu kurası**, karar günlüğüne katkı. Torbaların ilk hâli hafta sonuna kadar kontrol edilir. **Pazar hedefi:** Oyun tarafında ilk kart kaydırılabiliyor; YZ tarafında üretim hattı çalışıyor.

#### H2 · 2–8 Kasım

| Kişi | Alan · rol / eş | Atanan görevler | Pazar teslimi / ek sorumluluk |
| --- | --- | --- | --- |
| Mert | YZ **başkanı** / Sokul | `2.1–2.4`: Veri üretimi/filtre, lisansı uygun hazır veriler, tokenizer eğitimi, `train.bin`/`val.bin` hazırlama | **8 Kas:** `donem-1-yapay-zeka.md`; veri/tokenizer konumları, çalıştırma adımları ve devir paketi; koordinasyon |
| Sokul | YZ yardımcısı / Mert | `2.1–2.4`: Aynı veri/tokenizer işleri; eğitim verisi ve çıktı doğrulamaları | Mert ile gece üretimi; H3'te Kara'ya anlatacağı YZ hattını öğrenip devir notlarını hazırlama |
| Kara | Oyun **başkanı** / Muhammed | `2.5–2.9`: Sonuç ekranı, sekiz son, yıl sayacı, altın set yedek destesi, modelsiz tam oyun | **8 Kas:** `donem-1-oyun.md`; sekiz son testleri ve çalışır sürüm |
| Muhammed | Oyun yardımcısı / Kara | `2.5–2.9`: Aynı oyun görevleri; sonuç/son/yıl/deste akışını birlikte uygulama ve test etme | H3'te Mert'e anlatacağı oyun yapısını öğrenme; yarım işler ve test adımlarını Kara ile belgeleme |

**Pazar hedefi:** Yedek desteyle baştan sona oynanan oyun + tokenizer/veri hazırlığı. İki başkan kendi dönem raporunu teslim eder.

#### H3 · 9–15 Kasım

| Kişi | Alan · rol / eş | Atanan görevler | Hafta içi ek sorumluluk / çıktı |
| --- | --- | --- | --- |
| Mert | Oyun yardımcısı / Muhammed | `3.7–3.10`: Rol–konu tablosu, kazanma/rekor, kayıt sistemi, oynanabilir ilk APK | Pazartesiden itibaren Muhammed'den oyun devri alma; APK/yeniden açma testlerine katılma; koordinasyon |
| Sokul | YZ **başkanı** / Kara | `3.1–3.6`: Üretimi tamamlama, model kodu, eğitim döngüsü, 1M deneme, örnek üretimi, 10M eğitim | Pazartesiden itibaren Kara'ya YZ devri anlatma; Colab üretim/eğitim kayıtlarını takip etme |
| Kara | YZ yardımcısı / Sokul | `3.1–3.6`: Aynı model/eğitim işleri; kodu ve denemeleri birlikte yürütme | Sokul'dan devir alma; o dönemin YZ ekibi olarak Colab işlerine katılım |
| Muhammed | Oyun **başkanı** / Mert | `3.7–3.10`: Aynı rol–konu/kazanma/kayıt/APK işleri; entegrasyon ve kabul kontrollerini yönlendirme | Pazartesiden itibaren Mert'e oyun devri anlatma; G0'dan farklı olarak modelsiz **tam oyunun** APK'sını doğrulama |

**Pazar hedefi:** Gerçek telefonda oynanabilir ilk APK; YZ tarafında 1M doğrulaması ve 10M eğitim çıktıları. Hedef yıl henüz test ayarıdır; kalıcı değer H4'te kararlaştırılır.

#### H4 · 16–22 Kasım

| Kişi | Alan · rol / eş | Atanan görevler | Pazar teslimi / ek sorumluluk |
| --- | --- | --- | --- |
| Mert | Oyun yardımcısı / Muhammed | `4.7–4.9`: Yaklaşık 42 görsel, arayüz/ses, denge testi | Tarz/lisans notları ve cihaz testlerine katkı; denge raporunu Muhammed ile hazırlama; koordinasyon |
| Sokul | YZ **başkanı** / Kara | `4.1–4.6`: 30M eğitim, 200 kartlık sınav setini ayırma, sınav/öğretmen puanı/ezber ölçümü, 1M–10M–30M karşılaştırması | **22 Kas:** `donem-2-yapay-zeka.md`; H5 karar noktası için ölçüm tablosu/grafikleri |
| Kara | YZ yardımcısı / Sokul | `4.1–4.6`: Aynı eğitim/ölçüm işleri; sınav kartlarının eğitimden ayrı tutulması ve sonuç kontrolü | H5'te Muhammed'e aktaracağı YZ durumunu öğrenme; ölçüm ve devir notlarına katkı |
| Muhammed | Oyun **başkanı** / Mert | `4.7–4.9`: Aynı görsel/arayüz/ses/denge işleri; tam oyun entegrasyonu | **22 Kas:** `donem-2-oyun.md` ve `denge_testi.md`; oyun adı, hedef süre/yıl ve kart-yıl oranını toplantıya taşıma |

**Herkes:** En az 10 oyunluk ekip denge testine katkı verir. Etki kademelerinin değiştirilmesi iki ekibin onayını gerektirir. **Pazar hedefi:** Görselleri/arayüzü eklenmiş APK ve ölçülebilir denge raporu; YZ model karşılaştırması hazır.

#### H5 · 23–29 Kasım

| Kişi | Alan · rol / eş | Atanan görevler | Hafta içi ek sorumluluk / çıktı |
| --- | --- | --- | --- |
| Mert | Oyun **başkanı** / Sokul | `5.4–5.6`: Godot Python istemcisi, yedek desteye geçiş, bekleme görünümü | Pazartesiden itibaren Sokul'a oyun devri; YZ ekibiyle `5.2` API sözleşmesini onaylama; koordinasyon |
| Sokul | Oyun yardımcısı / Mert | `5.4–5.6`: Aynı bağlantı/fallback/bekleme işleri; sunucu yok/yavaş/bozuk cevap testleri | Mert'ten oyun devri alma; sahte sunucuyla istemci denemeleri |
| Kara | YZ **başkanı** / Muhammed | `5.1–5.3`: İnce ayar 1. tur, iki ekiple `api.md`, sahte sunucu | Pazartesiden itibaren Muhammed'e YZ devri; sınav kartlarını ince ayardan dışlama; sözleşme ve sahte cevapları oyun ekibine sunma |
| Muhammed | YZ yardımcısı / Kara | `5.1–5.3`: Aynı ince ayar/sözleşme/sahte sunucu işleri | Kara'dan devir alma; önceki oyun deneyimini API alanları ve hata/fallback senaryolarına aktarma |

**Herkes:** **25 Kasım karar noktası toplantısı** — ölçümlere göre mevcut model veya B planı değerlendirilir. **Pazar hedefi:** İki ekipçe onaylı sözleşme ve sahte sunucuyla çalışan, hata halinde yerel desteye dönen oyun.

#### H6 · 30 Kasım – 6 Aralık

| Kişi | Alan · rol / eş | Atanan görevler | Pazar teslimi / ek sorumluluk |
| --- | --- | --- | --- |
| Mert | Oyun **başkanı** / Sokul | `6.5–6.6`: Gerçek model sunucusuna bağlantı, model kartlarının uzun/kısa metin görünümü | **6 Ara:** `donem-3-oyun.md`; gerçek model + fallback test kanıtı; koordinasyon |
| Sokul | Oyun yardımcısı / Mert | `6.5–6.6`: Aynı entegrasyon/görünüm işleri; zaman aşımı, hatalı kart ve geç gelen cevap kontrolleri | H7'de Kara'ya anlatacağı oyun durumunu ve açık sorunları hazırlama |
| Kara | YZ **başkanı** / Muhammed | `6.1–6.4`: Kart üretici, doğrulayıcı ve testleri, kısıtlı üretim, gerçek sunucu | **6 Ara:** `donem-3-yapay-zeka.md`; API uyumu, 3 sn yanıt hedefi ve model/veri konumları |
| Muhammed | YZ yardımcısı / Kara | `6.1–6.4`: Aynı kart üretimi/doğrulama/sunucu işleri; oyun ekibiyle uçtan uca test | H7'de Mert'e anlatacağı YZ durumunu öğrenme; sunucu/model çalıştırma notlarına katkı |

**Pazar hedefi:** Dizüstünde gerçek model kartlarıyla oynanan, sunucu kapanınca takılmadan yedek desteye dönen oyun; iki alanın devir raporları hazır.

#### H7 · 7–13 Aralık

| Kişi | Alan · rol / eş | Atanan görevler | Hafta içi ek sorumluluk / çıktı |
| --- | --- | --- | --- |
| Mert | YZ yardımcısı / Muhammed | `7.0y–7.3y`: İnce ayar 2. tur, son ölçüm, ölçüme dayalı iyileştirme, modelin içine bakış görseli | Muhammed'den YZ devri alma; dış oyun testi için tanıdık getirme; koordinasyon |
| Sokul | Oyun **başkanı** / Kara | `7.5o–7.6o`: En az beş ekip dışı oyuncuyla test, önemli hataları düzeltme ve denge | Pazartesiden itibaren Kara'ya oyun devri; `oyun_testi.md` gözlemlerini derleme; tanıdık oyuncu getirme |
| Kara | Oyun yardımcısı / Sokul | `7.5o–7.6o`: Aynı dış test/hata/denge işleri; gözlem ve tekrar üretim adımları | Sokul'dan oyun devri alma; tanıdık oyuncu getirme ve düzeltmelerin tekrar testi |
| Muhammed | YZ **başkanı** / Mert | `7.0y–7.3y`: Aynı ikinci ince ayar/ölçüm/iyileştirme/iç bakış işleri | Pazartesiden itibaren Mert'e YZ devri; 200 sınav kartını ince ayardan dışlama; tanıdık oyuncu getirme |

**`7.4y` için Mert + Muhammed:** Yalnız zorunlu işler bitmişse ve ekip onaylarsa ayrı ONNX/Godot fizibilitesine bakılır. Eski Unity Sentis maddesi Godot'ta uygulanmış veya zorunlu sayılmaz. **Pazar hedefi:** Dış test raporu, kritik hata düzeltmeleri ve son model ölçümleri.

#### H8 · 14–20 Aralık

| Kişi | Alan · rol / eş | Atanan görevler | Pazar teslimi / ek sorumluluk |
| --- | --- | --- | --- |
| Mert | YZ yardımcısı / Muhammed | `8.3`, `8.4`: Dizüstü offline model kurulumu ve model sunumu; oyun ekibiyle entegrasyon | Model çalıştırma/yedek gösterim hazırlığı; `8.5b` tam prova; koordinasyon |
| Sokul | Oyun **başkanı** / Kara | `8.1`, `8.2`, `8.5`: Cila, son offline APK, oyun sunumu; `8.3` ortak dizüstü kurulumu | **20 Ara:** `donem-4-oyun.md`; teslim sorumlusuyla APK kontrolü ve `8.6` README'ye katkı |
| Kara | Oyun yardımcısı / Sokul | `8.1`, `8.2`, `8.5`: Aynı cila/APK/oyun sunumu işleri; `8.3` ortak kurulum | Gerçek cihazda offline tam oyun, APK tekrar testi ve `8.5b` tam prova |
| Muhammed | YZ **başkanı** / Mert | `8.3`, `8.4`: Aynı offline sunum kurulumu, 1M→30M/ölçüm/iç bakış sunum bölümü | **20 Ara:** `donem-4-yapay-zeka.md`; model bölümü teslimi ve `8.5b` tam prova |

**Teslim sorumlusu (28 Ekim kurasında belirlenecek):** Oyun ekibiyle `8.2` son APK kontrolü, `8.6` nihai README ve teslim paketi takibi. Bu belge bu rolü kura yapılmadan bir kişiye atamaz. **Son teslim:** 20 Aralık; çalışan APK, dizüstü gösterimi, sunum ve son dönem raporları.

### 4.2 Her hafta herkesin kişisel sorumluluğu

Aşağıdaki işler yukarıdaki teknik görevlere **ek olarak** her hafta sürer:

| Kişi | Altın set üretimi | Çapraz okuma | Sürekli rol |
| --- | --- | --- | --- |
| Mert | Günde ~11 / haftada ~78 **iyi kart** | Sokul'un kartlarını okur | Koordinatör; erişim/bağımlılık/karar takibini yürütür |
| Sokul | Günde ~11 / haftada ~78 **iyi kart** | Kara'nın kartlarını okur | O haftaki alanda eşli geliştirme ve PR incelemesi |
| Kara | Günde ~11 / haftada ~78 **iyi kart** | Muhammed'in kartlarını okur | O haftaki alanda eşli geliştirme ve PR incelemesi |
| Muhammed | Günde ~11 / haftada ~78 **iyi kart** | Mert'in kartlarını okur | O haftaki alanda eşli geliştirme ve PR incelemesi |

- Herkes **çarşamba öğle arası toplantısına** katılır; engel ve kararları paylaşır. Karar günlüğünü koordinatör günceller.
- Kartı üreten kişi kendi kartına nihai “iyi/at” onayı vermez. Hedef kabul edilmiş iyi karttır; elenenler için ek üretim gerekebilir.
- O haftanın YZ ikilisi Colab üretim/eğitim işlerini kendi hesaplarıyla yürütür; gece işleri oyun ekibine sessizce devredilmez.
- Herkes değişikliklerini PR ve test kanıtıyla paylaşır; kod yazan ile inceleyen aynı kişi olmaz. Tercihen diğer alandan biri inceler; inceleme gecikirse başkan/koordinatörle planlanır.
- Dönem sonunda raporu başkan yazar; yardımcı test/çalıştırma/devir notlarını sağlar. Merkezî `Docs/raporlar/` dosyaları o repoda güncellenir.

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

Ortak işler `1.0a–1.0d`: kart biçimini okuma, 23 rol için kör tahmin testine katılım, teslim sorumlusu kurası ve karar günlüğü. Merkezî `1.0b` kabul metni 25 Ekim tarihli **23 rol** kararıyla uyumlu tutulur.

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

**2 Kasım yerel durum:** 2.5–2.7 uygulandı, 2.9 örnek desteyle doğrulandı. 2.8'in kod bağlantısı hazır; çapraz okunmuş veri bekleniyor.
**7 Kasım:** 2 Kasım kararıyla çapraz okuma kaldırıldığı için 2.8'in veri şartı karşılandı: oyun, altın setten seçilen 600 kartlık desteyle açılır ([veri notu](../game/Veri/README.md)). 2.9 bu desteyle H1 testinde uçtan uca (açılış → sonuç → son → yeniden oyun) doğrulandı. Kutular PR incelemesinden sonra işaretlenir. Kullanıcı talebiyle H1/H2 main için birleştirildi; eksik veri ve bağımsız ekip/cihaz doğrulaması nedeniyle kutular açık kalır; [test/devir notuna](h2_durum.md) bakın.

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

Çekirdek ve sahne testleri Godot içinde `Testler/SahneIskeletiTesti.tscn` ve `Testler/H2OyunTesti.tscn` ile çalışır; komutları [README](../README.md) içindedir. H1 testi gösterge/kart/oturum mantığını ve viewport'a gönderilen sentetik fare/dokunma/Enter girdilerini, H2 testi sonuç/son/yıl/yeniden oyun ve hatalı desteyi kontrol eder. İkisi de üç pencere boyutunda denenir. Ayrı `dotnet test` projesi yoktur; bu komut test kanıtı sayılmaz. Fiziksel dokunmatik, ikinci bilgisayar ve APK ayrıca doğrulanmalıdır.

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
