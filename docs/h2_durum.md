# H2 · Muhammed · 2–8 Kasım 2026

2 Kasım uygulama ve devir notudur. Oyun başkanı Kara (Ömer), yardımcı Muhammed. Muhammed’in H1/H2 uygulaması ve Ömer’in `kara` dalındaki altı H1 commit’i, kullanıcının doğrudan talebiyle `main` için birleştirildi. Bu belge Kara’nın dönem sonu raporunun veya bağımsız ekip incelemesinin yerine geçmez.

## Haftanın oyun işleri

| Görev | Yerel durum | Dosyalar / kanıt |
| --- | --- | --- |
| 2.5 Sonuç ekranı | Uygulandı | `OyunEkranlari.cs`: yalnız seçilen sonucun metni, hitap, işaretli sayılar ve Devam Et. Sınırdaki kararda da son ekranından önce sonuç gösterilir. |
| 2.6 Sekiz son ve Yeniden Dene | Uygulandı | `SonKontrolu.cs`, tema okuma ve son paneli: dört göstergenin 0/100 durumu, onaylı tema anlatımları, ortak geçici taç görseli, temiz oturum. |
| 2.7 Yıl sayacı | Uygulandı | `YilSayaci.cs`: `tema.yil.kac_kartta_bir_yil` okunur (şu an 5). Başlangıç tamamlanmış yıl 0; 5. kararda 1, 10. kararda 2. Önizleme, yinelenen giriş ve Devam Et zamanı ilerletmez. |
| 2.8 Onaylı altın set destesi | Kod bağlantısı hazır; veri bekliyor | Kullanıcı onayıyla on örnek kart kullanılıyor. `AI/data/altin_set/` içinde henüz onaylı deste yok. Örnek kartlar altın set diye etiketlenmedi. |
| 2.9 Modelsiz tam oyun | Örnek desteyle uygulandı | Açılış → karar → sonuç → Devam Et → son → yeniden oyun; yerel JSON ile ağsız çalışır. Nihai kabul için 2.8 verisi ve ekip incelemesi gerekir. |

İki H1 uygulaması ortaklaştırıldı: tek kart modeli/doğrulayıcı, tek oyun durumu, bileşenlere ayrılmış açılış/gösterge/kart sunumu ve oturum karakterleri kullanılır. Ömer’in örnek destesi ve altı test sahnesi korunup ortak arayüze uyarlandı. Tek kararda birden çok gösterge sınıra ulaşırsa ilgili sonların **tamamı** gösterilir; liste sırası yalnız sunum içindir. Hangi sonun öncelikli olacağına ilişkin ekip kararı alınmış sayılmaz.

## Doğrulama

- `dotnet build game/Game.csproj`: başarı; sıfır uyarı/hata.
- Headless sonuçları: H1 **231**, H2 **1076** kontrol başarılı. Xvfb ile gerçek grafik işleyicisinde H1 **249**, H2 **1127** kontrol başarılı; fark ekran görüntüsü kaydetme kontrolleridir.
- Ömer’in altı bileşen testi: oyun durumu, yedek deste, kart görünümü, kaydırma, açılış ve oyun akışı başarılı.
- `Testler/SahneIskeletiTesti.tscn`: H1 regresyonu; göstergeler, hitap, örnek deste, sentetik fare/dokunma/klavye ve üç ekran boyutu.
- `Testler/H2OyunTesti.tscn`: her sekiz son tek tek, eşzamanlı iki son, sonuçların seçime göre ayrılması, sonucun sondan önce gelmesi, işaretli etkiler, yıl oranı, temiz yeniden başlama, eski giriş ve hatalı desteden toparlanma.
- Boyutlar: 1152×648, 360×640, 640×360. Yatay ekranda yeni metin yerleşince odaklı düğmenin görünür alana kaydırılması düzeltildi.
- Gerçek telefon/APK ve ikinci bilgisayar henüz denenmedi. Yerel testler çapraz okuma veya PR incelemesi yerine geçmez.

Grafik testinin 51 H2 görüntüsü üretildi; mobil son, masaüstü eşzamanlı son ve yatay sonuç ekranları görsel olarak da incelendi. Taç simgesi bu aşamanın ortak yer tutucusudur; H4'ün sonlara özel görselleri tamamlanmış sayılmaz.

Çalıştırma ve test komutları [README](../README.md) içindedir. Grafik test çıktıları yerel `.godot/testler/` altında oluşur, Git'e girmez.

## Kara'ya / sonraki döneme aktarılacaklar

1. README komutlarıyla ana oyunu, H1/H2 testlerini ve altı bileşen testini çalıştır; birleşik H1/H2 uygulamasını incele. Ömer’in commit geçmişi merge ile korunur.
2. Çapraz okumadan geçmiş altın set gelince [veri notundaki](../game/Veri/README.md) adımlarla desteyi değiştir ve test et. Muhammed'in Mert kartlarını okuma ve haftalık ~78 kabul edilmiş kart hedefi, kartlar/insan incelemesi olmadığı için tamamlandı sayılmaz.
3. Bağımsız ekip incelemesi, Kara’nın bilgisayarı ve fiziksel cihaz doğrulamasını kaydet. 8 Kasım `Docs/raporlar/donem-1-oyun.md` raporunu Kara yazar; Muhammed bu uygulama/test notunu sağlar. Kullanıcı talebiyle doğrudan main birleştirmesi, ikinci bilgisayar/cihaz ve veri kabulü yerine geçmez; bu eksikleri olan görevlerin kutuları açık kalır.

H3'te Mert'e aktarılacak yapı: `Oyun` gösterge/akış, `Cekirdek` yıl/son hesabı; `Kartlar` veri/tema/oturum kimliği; `Arayuz` sahne bağlantıları ve giriş. Sonraki işler kazanma/rekor, kayıt, rol–konu seçimi ve gerçek cihazda tam oyun APK'sıdır.
