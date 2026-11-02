# H1 arayüz uygulama notları

Bu belge, H1 aşamasındaki Godot yerel kontrollerini tarihsel olarak kaydeder. **2 Kasım Assets entegrasyonuyla görsel değerler değişti; güncel tema ve yerleşim için [Assets arayüz notuna](arayuz_assets.md) bakın.** Kaynaklar `game/Sahneler/Oyun.tscn`, `game/Betikler/Arayuz/OyunEkranlari.cs` ve `game/Betikler/Arayuz/KaydirmaKontrolu.cs` dosyalarıdır. Görsel kimlik veya yeni bir tasarım sistemi tanımlamaz; değerlerin asıl kaynağı uygulamadır.

## Yerleşim ve metin

- İçerik genişliği 640 birim hedefiyle ortalanır; yan boşluklar en az 16 birimdir. Hesap `max(16, (genişlik - 640) / 2)` sonucunu tam sayıya çevirir. Üst ve alt boşluk 24 birimdir.
- Ana dikey gruplarda 20 birim ayrım vardır. Kart iç boşlukları 20, kart içeriği ayrımı 16 birimdir; kartın başlangıç minimum yüksekliği 200 birimdir.
- Dört gösterge iki sütunda yer alır. Sütun ayrımı 20, satır ayrımı 12, gösterge çubuklarının minimum yüksekliği 12 birimdir. Sayısal değerler ayrıca metinle gösterilir.
- Düğmelerin ve hükümdar seçicisinin minimum yüksekliği 48 birimdir; metin gerektiğinde satıra sarılır.
- Yazı boyutları oyun adında 28, ekran/kart başlıklarında 24, kart anlatısında 20, açıklama/sonuç/hata metinlerinde 18, göstergeler/yardım/kart önizlemesinde 16 birimdir.
- Özel font veya kontrol teması atanmaz; Godot varsayılanları kullanılır. Sahne zemin rengi `Color(0.09, 0.105, 0.12, 1)` değerindedir. Bunlar H4 görsel kimliği için verilmiş kararlar değildir.

## Etkileşim ve erişim

- İçerik dikey kayar; yatay kaydırma kapalıdır. `ScrollContainer.follow_focus` açıktır. Ekran değişirken kaydırma sıfırlanır ve odak açılışta hükümdar seçicisine, oyunda karta, sonuçta devam düğmesine, sonda yeniden başlama düğmesine verilir.
- Kart klavye odağı alabilir; odakta 1 birim kalınlıklı `Color(0.8, 0.82, 0.85)` çerçeve çizilir. Karar düğmeleri kaydırma hareketine alternatif sunar.
- Fare ve dokunma aynı karar akışını kullanır. Karar eşiği `max(48, kart genişliği × 0.22)` değeridir. Dikey hareket 12 birimi geçip yatay hareketten baskın olursa kart hareketi iptal edilerek sayfanın kaydırılmasına izin verilir.
- Önizleme, kartın anlatı alanında karar metnini, etkilerin yönünü ve bırakma/geri dönme yönergesini gösterir. Aktif sürüklemede kartın mevcut yüksekliği minimum olarak tutulur; iptalde anlatı, 20 birim yazı ve 200 birim minimum yükseklik geri gelir. Yatay telefonda önizlemenin kart altına kaybolmasını önlemek amaçlanır.
- Eşik aşılmış ve yatay hareket baskınsa bırakma kararı uygular. Pencere odağının kaybı, dokunma iptali veya ekran değişimi hareketi iptal eder.

## H4'e kalan kapsam

[Geliştirme planının](oyun_gelistirme_plani.md) 4.7–4.9 işleri geçerlidir: yaklaşık 42 görsel ve rol/cinsiyet eşleşmeleri; görsel arayüz ve ses; küçük/büyük ekran, Türkçe karakter, uzun metin ve dokunma hedefi denemeleri; kaynak/lisans notları ve ekip denge raporu. Bu H1 kayıtları bu işlerin tamamlandığını göstermez. Oyun adı, süre/hedef yıl ve kart-yıl oranının son kararı da H4 kapsamındadır.
