# BUYRUK geliştirme raporu

**H2 deste takibi.** Bu rapor ilk geliştirme turunu kaydeder. Teslim sürümünde Muhammed’in dalındaki 600 kartlık Altın Set destesi ve testleri esas alındı. [Güncel deste notu](../game/Veri/README.md).

7 Kasım 2026 · Referans `657dbf4` · Çalışma dalı `codex/buyruk-gelistirme`

## A — Başlangıç durumu

Godot 4.7.2 .NET ve C# ile çalışan sürüm incelendi. `AnaMenu.tscn` menü ve alt ekran geçişlerini, `Oyun.tscn` hükümdar seçimi, kart, sonuç ve son ekranlarını içeriyordu. `OyunYoneticisi` akışı, `IKartKaynagi` yerel kart sırasını, `Tema` 23 rolü ve son metinlerini yönetiyordu. Dört gösterge 50'den başlıyor, 0 ve 100 sekiz kaybetme sonunu tetikliyor, beş karar bir yıl sayılıyordu. Giriş ve animasyonlar, sonuç sunumu, karakter isimleri, ses ve tam ekran ayarları ile 104 PNG'lik ortak tema hazırdı.

Eksikler arasında gerçek kayıt/yükleme, rekor, kalıcı olay kimlikleri ve dış olay doğrulaması vardı. `yedek_deste.json` 10 örnek kart, `yedek_desta.json` ikinci biçimde 10 H1 kartı içeriyordu. Onaylanmış 200 kartlık bir kaynak bu revizyonda bulunmadı.

Kod değiştirilmeden önce Windows'ta derleme 0 uyarı ve 0 hatayla tamamlandı. Mevcut 10 başsız test sahnesinin tamamı 0 koduyla çıktı. H1 294, H2 1076, Menü 132 kontrol bildirdi. H2'nin üç boş deste uyarısı, beklenen hata senaryosuna aitti. `kara` çalışma kopyasındaki değişmiş `game/Testler/YedekDesteTesti.cs` ve izlenmeyen `docs/Docs/` olduğu gibi bırakıldı. Geliştirme referans commit'ten oluşturulan `BUYRUK-gelistirme` çalışma kopyasında yapıldı.

## B — Arayüz

Başlık, kart odağı kaydırmayı tetiklediğinde içerikle birlikte yukarı çıkıyordu. Altın başlık bandı kaydırılan gövdenin üstüne alındı. Kart ilk açıldığında göstergeler sayfanın üstünde görünür. Seçenek önizlemesi başladığında gerektiğinde görünür alana kaydırılır. Sonuç ve son ekranları klavye odağıyla görünür düğmeye ulaşmayı sürdürüyor. Uzun Türkçe kart metni satıra sarılıyor, ekran boyundan uzun içerik dikey kaydırılabiliyor.

Mobilde dikey yön, 360×640 tasarım referansı ve `Expand` ölçekleme kullanılıyor. Menü ve oyun içeriği, Android/iOS'un bildirdiği güvenli alan piksellerinden hesaplanan kenar boşluklarına uyuyor. Masaüstü ölçekleme, tema ve PNG kaynakları korundu. [Godot güvenli ekran alanı](https://docs.godotengine.org/en/stable/classes/class_displayserver.html#class-displayserver-method-get-display-safe-area), [Godot pencere ayarları](https://docs.godotengine.org/en/stable/classes/class_projectsettings.html#class-projectsettings-property-display-window-handheld-orientation).

## C — Kayıt ve yükleme

- Her yeni hükümdarlık ayrı bir oturum kimliği alır. Önceki kayıtlar listede kalır.
- `user://hukumdarliklar/<oturum-id>.json` altında sürüm 1 kaydı tutulur. Hükümdar, dört değer, yıl/oran, karar sayısı, aktif kart, kart/sonuç/son aşaması, kaynak konumu, 23 karakter kimliği ve karar geçmişi kaydedilir. Geçmiş her kabul edilen olay kimliğini, seçilen yönü ve uygulanan etkiyi tutar.
- Tahta çıkınca, karar tamamlanınca ve **Devam Et** ile aşama değişince otomatik kayıt alınır. **Kaydet ve Ana Menüye Dön** ayrıca son kararlı durumu kaydeder. Yarım sürükleme karar sayılmaz. Sonuç ekranından yükleme, etkiyi ikinci defa uygulamadan aynı sonuca döner.
- JSON önce aynı klasörde benzersiz bir geçici dosyaya yazılıp diske boşaltılır. Tam yazım sonrası hedef dosya değiştirilir. Yazma hatasında kullanıcıya mesaj gösterilir. Artık geçici dosyalar listelenmez.
- Yüklemede şema ve sürüm, dosya/oturum kimliği, aktif kart, karakterler, aşama ve karar geçmişinden tekrar hesaplanan göstergeler/yıl denetlenir. Bozuk kayıtlar sayılır ve sağlam kayıtlar açılabilir. Kayıt aktif kartın kopyasını saklar. Deste daha sonra değişirse kaydedilmiş kart/sonuç aynen açılır, sonraki çekim yeni havuzda kayıtlı kaynak konumundan devam eder.

## D — Deste ve veri aktarımı

JSON dizisi ve `{ "kartlar": [...] }` kapsayıcısı korunuyor. Rol, iki farklı seçenek, etki aralığı, sonuç metni, tekrarlı JSON alanı ve benzersiz olay kimliği denetleniyor. Kimliksiz eski kartlara metin ve seçeneklerden kararlı SHA-256 tabanlı kimlik üretiliyor. Yeni veri için açık ve kalıcı `id` öneriliyor. İsteğe bağlı `olay_grubu`, farklı kimlikli varyantları grupluyor. Her turda olay başına bir kart geliyor. Havuz tükenince kontrollü olarak yeni tura geçiliyor ve grup varyantı sırayla değişiyor. Gruplanmamış örnek deste eski sırada döngüye devam ediyor.

Gerekli girdi, ekipçe çapraz okunup onaylanan **200 kartın JSON dışa aktarımı**. Veri notundaki beklenen kaynak yolu `AI/data/altin_set/yedek_deste.json`. Dosyayı `DesteDogrulama.tscn` ile 200 kart beklentisiyle doğrulayın. Ardından `game/Veri/yedek_deste.json` yoluna alın veya `OyunEkranlari.DesteYolu` üzerinden paketlenmiş alternatifi seçin. Sayı ve şema denetimi içerik onayı yerine geçmez. [Veri biçimi ve kaynak notu](../game/Veri/README.md).

## E — Rekor ve denge

Kayıtlı Oyunlar ekranı devam eden oyunları, tamamlanan hükümdarlıkları, yıl/karar sayılarını ve tetiklenen sonları gösteriyor. Rekor tamamlanan oyunlardan hesaplanıyor. Önce yıl, sonra karar sayısı, eşitlikte daha erken kayıt öne çıkıyor. Rekor aynı kalıcı kayıtlardan yeniden hesaplandığı için ikinci bir kayıt dosyasına bağımlı değil. Sekiz kaybetme koşulu ve etki kademeleri korundu. Kazanma kuralı ekip kararına açık.

| Strateji | 10 örnek kartla gözlenen sonuç |
| --- | --- |
| Sürekli sol | 9 karar, 1 yıl |
| Sürekli sağ | 32 karar, 6 yıl |
| Göstergeleri 50'ye yakın tutan seçim | 89 karar, 17 yıl |
| 1.000 eşit olasılıklı rastgele koşu | En az 5, medyan 13, %90 yüzdeliği 22, en çok 37 karar |

Deterministik stratejilerde aynı oyun durumuna ve kart konumuna dönen bir döngü görülmedi. Rastgele koşulardan hiçbiri 500 karar sınırına ulaşmadı. Sol/sağ farkı, tekrarlanan bu küçük destedeki yön asimetrisini gösteriyor. Sonuçlar onaylı destenin veya oyuncu davranışının ölçümü değildir.

Ekip için iki seçenek var. Açık uçlu rekor modu, hayatta kalmayı hedef yapar ve yeni deste ile tekrar ölçülebilir. Süreli **Altın Çağ** hedefi bir bitiş noktası sağlar. Örnek destede 5 yıl 25 karar gerektirir ve rastgele koşuların %90 yüzdeliğinin üzerindedir. 10 yıl 50 karar gerektirir ve yalnız daha dikkatli stratejilere erişilebilir görünüyor. Hedef seçilirse onaylı destede ve ekip oyun testinde yeniden ölçülmeli.

## F — AI bağlantı noktaları

`IKartKaynagi` kaynak türünden bağımsız kart çekimini ve geri yüklenebilir yerel konumu sağlar. `YedekliKartKaynagi`, ileride asenkron bir istemcinin önceden hazırlayacağı tek kart JSON'unu alabilir. Yerel kartlarla aynı alan, rol ve etki doğrulaması uygulanır. Boş, geçersiz, kullanılmış kimlikli veya uygun olmayan kaynak etiketli yanıt yerel sıraya düşer. Yerel sayaç yalnız yerel kart çekilince ilerler. `OyunYoneticisi.AnligiAl()` serileştirilebilir durum ve karar geçmişi verir. Geçerli kart seçenekleri ancak oyuncu karar verdiğinde oyun çekirdeği üzerinden uygulanır. Sağlayıcı göstergelere, yıl oranına veya son kurallarına erişmez.

Canlı ağ istemcisi ve LLM bu aşamanın kapsamı dışında. Gelecekteki istemci hazır JSON'u kuyruklamalı, ağ beklemesini oyun döngüsünden ayrı tutmalı ve Android internet iznini o aşamada değerlendirmeli.

## G — Test ve Android

Komutlar geliştirme çalışma kopyasının kökünden, kurulu Godot 4.7.2 .NET çalıştırıcısıyla verildi. Windows'ta GUI çalıştırıcısı beklemeli olarak başlatılıp her testin çıkış kodu ve günlüğü ayrı alındı.

```sh
dotnet build game/Game.csproj --nologo
godot --headless --path game --editor --import
godot --headless --path game res://Testler/<TestAdi>.tscn
godot --path game res://Testler/SahneIskeletiTesti.tscn
godot --path game res://Testler/MenuTesti.tscn
godot --headless --path game res://Araclar/DesteDogrulama.tscn -- res://Veri/yedek_deste.json 10
godot --headless --path game res://Araclar/DesteDogrulama.tscn -- res://Veri/yedek_deste.json 200
```

| Kontrol | Gözlenen sonuç |
| --- | --- |
| Windows derlemesi ve Godot içe aktarımı | Derleme 0 uyarı, 0 hata. İçe aktarım 0. |
| H1 ve H2 | H1 412 başsız, 444 grafik kontrol ile dört boyutta fare/dokunma/klavye, sabit başlık, göstergeler ve uzun Türkçe kart. H2 1076 kontrol ile sekiz son, eşzamanlı sonlar, yıl ve bozuk deste. |
| Menü ve oturum kaydı | Menü 132 başsız, 153 grafik kontrol. Oturum 14 kontrol ile ilk kart, sonuçtan devam, bozuk kayıt, eski kayıtların korunması ve rekor. |
| Bileşen ve denge | Açılış, kart çevirme/görünüm, kaydırma, oyun akışı, oyun durumu, yedek deste ve denge sahneleri geçti. |
| Deste doğrulama aracı | 10 kart ve 10 grup kabul edildi, çıkış 0. Aynı örnek deste 200 beklentisinde reddedildi, çıkış 1. |
| Grafik H1 tekrar kontrolü | İlk Windows grafik turunda sol ok önizleme doğrulaması başarısız oldu. Aynı koşulda referans 318, geliştirme tekrarı 440 ve son tur 444 kontrolle geçti. İlk aralıklı başarısızlığın nedeni kesinleşmedi. |

Son kabul turunda 12 başsız sahne ve 2 grafik çalıştırması 0 koduyla çıktı. Günlüklerde motor veya C# hatası görülmedi. H2'nin beklenen boş deste uyarıları korunuyor. Yerleşim boyutları 1152×648, 923×520, 360×640 ve 640×360. Grafik çıktıları ve günlükler yerel `game/.godot/` altında.

Mevcut `Android H1` profili ARM64, Compatibility işleyicisi ve paketlenen `Veri/*.json` ile çalışacak şekilde tanımlı. Test ve doğrulama sahneleri dışa aktarım dışında. Android C# hedefi `net9.0`, masaüstü `net10.0`. `res://` paket verisine, `user://` yazılabilir uygulama verisine ayrıldı. Windows'ta sentetik dokunma ve 360×640 yerleşimi sınandı. Fiziksel cihaz, ekran kesiti, sistem hareketleri ve APK kurulumu sınanmadı. Bu makinede Java 21 görüldü, standart SDK yolu, `adb` ve varsayılan Godot export template klasörü doğrulanamadı. Profil eski APK dosya adını ve paket kimliğini koruyor.

APK için Godot sürümüyle eşleşen **.NET Android export templates**, Godot editöründe JDK ve SDK yolları, OpenJDK 17 önerisi, Android platform 35, Platform Tools ≥35.0.0, Build Tools 35.0.1, command-line tools, NDK r28b ve CMake 3.10.2.4988404 gerekiyor. Önce debug ARM64 APK'yı gerçek cihazda açıp uzun metin, çentik, yatay sürükleme, dikey kaydırma ve yeniden açılışta kayıt yüklemeyi doğrulayın. [Godot Android dışa aktarma kılavuzu](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html).

## Dosyalar ve devir

| Alan | Değişen veya eklenen dosyalar |
| --- | --- |
| Yerleşim ve kayıt ekranı | `game/project.godot`, `game/Sahneler/Oyun.tscn`, `KayitliOyunlar.tscn`, `YeniOyun.tscn`, `game/Betikler/Arayuz/GuvenliAlan.cs`, `MenuSahnesi.cs`, `OyunEkranlari.cs`, `KayitliOyunlarEkrani.cs` |
| Kayıt, oturum, rekor | `game/Betikler/Oyun/OyunDurumu.cs`, `OyunAnligi.cs`, `OyunKayitDeposu.cs`, `HukumdarlikGecmisi.cs`, `game/Betikler/Cekirdek/SonKontrolu.cs`, `game/Betikler/Kartlar/OturumKarakterleri.cs`, `MetinYerlestirici.cs` |
| Deste ve dış olay | `game/Betikler/Kartlar/Kart.cs`, `YedekDeste.cs`, `IKartKaynagi.cs`, `YedekliKartKaynagi.cs`, `game/Araclar/DesteDogrulama.cs` ve `.tscn` |
| Test ve paket | `game/Testler/SahneIskeletiTesti.cs`, `YedekDesteTesti.cs`, `OturumKaydiTesti.cs`, `DengeTesti.cs`, iki yeni test sahnesi, yeni Godot `.cs.uid` eşleri ve `game/export_presets.cfg` |
| Belgeler | `README.md`, `game/Veri/README.md`, bu rapor |

Ekipten beklenenler, onaylı deste teslimi ve tekrar politikası, hedef yıl seçimi, örnek koşulardaki yön asimetrisinin gerçek kartlarla ölçümü ve fiziksel Android kabulü. Karakter portreleri, sona özel illüstrasyonlar, müzik ve ses varlıkları bu geliştirme kapsamında ele alınmadı. Çok uzun oturumlarda tam geçmişi her kararda yazmanın maliyeti ve beklenmedik güç kaybı ayrıca cihazda ölçülmeli.
