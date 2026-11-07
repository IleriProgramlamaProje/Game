# Dönem 1 · BUYRUK Oyun · Başkan: Ömer (Kara) · Yardımcı: Muhammed

**Dönem:** 26 Ekim–8 Kasım 2026 · **Rapor kapsamı:** Hafta 2 (2–8 Kasım) · **Durum tarihi:** 7 Kasım 2026

Hafta 2 tesliminde BUYRUK, 600 Altın Set kartıyla açılıştan oyun sonuna kadar çevrimdışı oynanıyor. İlk birleşik referans `657dbf4` örnek desteyle çalışıyordu. Teslim sürümünde Muhammed'in **2.8–2.9** çalışması `39f7883` ve Android düzeltmesi `2a8bd4f` temel alındı. Ömer'in çalışma kopyasındaki kayıt/yükleme, rekor ve yerleşim değişiklikleri bu temel üzerine eklendi. [2.8–2.9 commit'i](https://github.com/IleriProgramlamaProje/Game/commit/39f7883), [Android düzeltmesi](https://github.com/IleriProgramlamaProje/Game/commit/2a8bd4f)

## Hafta 2'de yapılanlar

| Görev | Durum | Uygulama |
| --- | --- | --- |
| **2.5 — Karar sonucu** | Tamamlandı | Seçilen kararın hikâye sonucu ve işaretli gösterge etkileri gösteriliyor. **Devam Et** sonraki karta veya sona götürüyor. Sınırda biten kararda önce karar sonucu gösteriliyor. `game/Sahneler/Oyun.tscn`, `game/Betikler/Arayuz/OyunEkranlari.cs` |
| **2.6 — Sekiz son ve yeniden oynama** | Tamamlandı | Yönetim, Halk, Para ve Ordu'nun 0 ve 100 değerleri sekiz ayrı sona karşılık geliyor. Aynı kararda birden çok sınır tetiklenirse ilgili sonların tamamı gösteriliyor. **Tekrar Oyna** temiz hükümdar seçimine dönüyor. `game/Betikler/Cekirdek/SonKontrolu.cs`, `game/Veri/tema.json` |
| **2.7 — Yıl sayacı** | Tamamlandı | 5 kabul edilmiş karar 1 yıl. Önizleme ve **Devam Et** karar sayılmıyor. `game/Betikler/Cekirdek/YilSayaci.cs` |
| **2.8 — Altın Set yedek destesi** | Tamamlandı | Muhammed'in `game/Veri/yedek_deste.json` dosyasındaki 600 kart, aynı içerik, sıra ve kimliklerle varsayılan deste olarak korundu. 23 rolün tamamı var. `kaynak: altin_set` ve `lisans: sadece_ders` alanları korunuyor. |
| **2.9 — Modelsiz tam oyun** | Tamamlandı | Muhammed'in varsayılan desteyle açılış, karar, sonuç, son ve yeniden başlama testi birleşik sürümde de çalıştırıldı. `game/Testler/SahneIskeletiTesti.tscn` |

Altın Set kaynağı, AI deposunun `22a71a2` revizyonundaki `data/altin_set/ham/altin_set.json`. 1.448 kartın 1.345'i oyun doğrulamasını geçti. Muhammed'in `araclar/deste_olustur.py` betiği 600 kartı hedef metin uzunluğu ve rol dengesiyle seçiyor. 2 Kasım proje kararı kartları çapraz okuma yerine otomatik kontrolle kabul ediyor. Önceki 10 örnek kart regresyon testleri için `game/Testler/Veri/ornek_deste.json` altında. [AI kaynak notu](https://github.com/IleriProgramlamaProje/AI/blob/22a71a2828f2e4815f0befb1d916aefe328e50e5/data/altin_set/README.md), [proje karar kaydı](https://github.com/IleriProgramlamaProje/Docs/blob/9769f17cdd3d4d8bdccee5927f175713e7fdc2b6/kararlar.md)

## Birleşik sürümün test sonuçları

| Kontrol | Gözlenen sonuç |
| --- | --- |
| Windows ve Android C# derlemesi | Her iki hedefte 0 uyarı, 0 hata. Android komutu `dotnet build game/Game.csproj -p:GodotTargetPlatform=android`. |
| Varsayılan deste | 600 kart ve 600 olay grubu doğrulandı. Dosya, üretim betiği ve örnek test verisi Muhammed'in dalıyla aynı. |
| Başsız testler | 12/12 geçti. H1 652, H2 1.076, Menü 132 ve oturum kaydı 14 kontrol. |
| Grafik testleri | Menü 153 kontrolle, H1 tekrar turu 688 kontrolle geçti. İlk Windows grafik turunda bir kart kararı zamanında tamamlanmadı. Tekrarda sorun görülmedi, ilk hatanın nedeni kesinleşmedi. |
| Fiziksel Android | Bu birleşik sürümde APK kurulumu veya fiziksel cihaz testi yapılmadı. Muhammed'in APK paket içeriği doğrulaması `2a8bd4f` commit notunda kayıtlı. |

H1, gerçek destenin uzun anlatı ve karar metinleriyle yerleşimi ve çevrimdışı oynanışı 1152×648, 923×520, 360×640 ve 640×360 boyutlarında denetler. H2, sekiz sonu ve eşzamanlı sınırları kontrollü kartlarla doğrular. H2'nin üç boş deste uyarısı beklenen hata senaryosudur.

## H3'e devir ve kalan işler

- **Kayıt/yükleme ve rekor:** Her yeni hükümdarlık ayrı kayıt alıyor. Kayıtlı Oyunlar ekranından bekleyen karta veya karar sonucuna dönülebiliyor. Bozuk kayıt sağlam kayıtları engellemiyor. Tamamlanan hükümdarlıklar geçmiş ve rekor bölümüne giriyor. `game/Betikler/Oyun/OyunKayitDeposu.cs`, `HukumdarlikGecmisi.cs`, `game/Testler/OturumKaydiTesti.tscn`.
- **Arayüz:** Üst başlık sabit. Kart ilk açıldığında göstergeler görünüyor. Dar ekranda seçenek önizlemesi gerektiğinde görünür alana kaydırılıyor. Ortak tema ve PNG kaynakları korundu. `game/Sahneler/Oyun.tscn`, `game/Betikler/Arayuz/GuvenliAlan.cs`.
- **Kalan:** Ekip incelemesi ve `main` birleşimi, hedef yıl/kazanma kuralı, onaylı rol–konu eşleşmeleri ve gerçek Android cihazda kurulum/dokunma/kayıt testi. Karakter portreleri ve sonlara özel illüstrasyonlar sonraki görsel çalışmaya ait.

## Nasıl çalıştırılır?

`Game` kökünden Godot **4.7.2 .NET** ve `global.json` ile uyumlu .NET SDK kullanın. `godot` yerine gerekirse kurulu çalıştırıcının yolunu yazın.

```sh
git lfs pull
dotnet build game/Game.csproj
godot --headless --path game --editor --import
godot --path game
godot --headless --path game res://Araclar/DesteDogrulama.tscn -- res://Veri/yedek_deste.json 600
godot --headless --path game res://Testler/SahneIskeletiTesti.tscn
godot --headless --path game res://Testler/H2OyunTesti.tscn
godot --headless --path game res://Testler/OturumKaydiTesti.tscn
```

Oynama yolu **Yeni Oyun → Başla → Beyim/Hatunum**. PNG kaynakları `game/Assets/ui/` altında Git LFS ile saklanıyor. Tema ve oyun destesi `game/Veri/` altında. Oynamak için model ağırlığı veya ağ bağlantısı gerekmiyor.

Yeni gelen ekip için ilk üç adım:

1. `kara` sürümünü açın. Varsayılan desteyle bir oyunu baştan sona tamamlayın ve H1/H2 testlerini çalıştırın.
2. Bir kararın ardından oyunu kapatın. **Kayıtlı Oyunlar** üzerinden aynı sonuç ve göstergelerle devam edildiğini doğrulayın.
3. Android APK'yı gerçek cihazda sınayın. Kazanma hedefini ve rol–konu eşleşmelerini ekipçe karara bağlayın.
