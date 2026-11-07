# Yerel oyun verisi

`tema.json`, Docs deposunun `80d6afa10efc32e1f7de28d4f7b1f47458e74bf5` revizyonundaki dosyanın birebir kopyasıdır (2 Kasım 2026). Onaylı açılış, 23 rol, sekiz sonun başlık/metni ve yıl oranı buradan okunur. Eksik son veya sıfır/negatif yıl oranı veri hatasıdır. `hedef_yil: null` korunur; kazanma H3 işidir.

`yedek_deste.json` oyunun varsayılan destesidir: altın setten seçilmiş **600 kart** (7 Kasım, görev 2.8). Kaynak, AI deposunun `22a71a2` revizyonundaki `data/altin_set/ham/altin_set.json` dosyasıdır (3 Kasım'da eklenen 1.448 kart). 2 Kasım kararıyla altın set elle okunmaz; kartlar sayfanın otomatik kontrolünden geçmiştir. Her kart altın setteki kimliğini (`altin-0000001` …) `id` alanında taşır; `kaynak: altin_set`, `lisans: sadece_ders` değiştirilmez.

Deste `araclar/deste_olustur.py` ile üretilir (Game repo kökünden `python3 araclar/deste_olustur.py`; `--adet`, `--tohum` seçenekleri var). Betik:

- oyunun C# doğrulayıcısının kurallarını birebir uygular; geçmeyen kart desteye girmez (1.448 karttan 1.345'i uygun; 87'sinin metni 20–50, 16'sının kararı 2–5 kelime dışında),
- "en iyi 600"ü kuralla seçer: önce hedef uzunluktaki (20–35 kelime) metinler, roller dengeli (rol başına 26–27),
- aynı konuşmacıyı art arda getirmez; sıra sabit tohumla karışıktır.

Altın set büyüdüğünde AI deposunu güncelleyip betiği yeniden çalıştırın, ardından testleri koşun.

Önceki **10 kartlık örnek deste** test verisi olarak `Testler/Veri/ornek_deste.json` dosyasına taşındı (`kaynak: model`). İçeriği bilinen kartlara bağlı testler (`DesteYolu` ile) onu kullanır; dışa aktarılan oyuna girmez.

`yedek_desta.json`, Ömer’in `bccec30` revizyonundaki on kartlık H1 destesidir; özgün dosya adı, `id`, `kaynak: altin_set` ve `lisans: sadece_ders` etiketleri korundu. Etiketler tek başına çapraz okuma kanıtı sayılmaz. Bu deste bileşen testinde okunur; oyunun varsayılanı yukarıdaki `yedek_deste.json` dosyasıdır.

## Deste sözleşmesi ve değiştirme

- Varsayılan kapsayıcı boş olmayan bir **JSON dizisi**dir. Ömer’in H1 biçimi olan `{ "kartlar": [...] }` da aynı doğrulayıcıyla okunur; isteğe bağlı `id` alanı korunur. Kart alanları `konusan`, `konu`, `metin`, `sol`, `sag`, `kaynak`, `lisans`tır. Seçenekler `etiket`, `etki`, `sonuc` içerir. Biçim Docs/kart_formati.md bölüm 3 ile aynıdır.
- Rol tema listesinde bulunmalı; metin 20–50, karar 2–5 kelime olmalı. Her karar 1–3 göstergeyi ±5/10/15 etkiler ve en az bir negatif etki içerir. Boş sonuç, tanınmayan alan ve bozuk veri reddedilir.
- Kartlar dosya sırasıyla dolaşır; son karttan sonra ilk karta dönülür. Yeni oyun ilk karttan başlar. Bu H1'den gelen deneme davranışıdır; ekipçe onaylanmış nihai tekrar politikası değildir.
- Deste yukarıdaki betikle yenilenir; kaynak/lisans alanlarını elle değiştirmeyin. `Oyun` kökünün Inspector'daki `DesteYolu` alanı alternatif bir paketlenmiş JSON dosyasını da seçebilir.
- Değişim sonrası derleme ve tüm testleri çalıştırın. `YedekDesteTesti` desteyi bütünüyle doğrular (en az 100 kart, her rol, kimlik, kaynak/lisans, ardışık konuşmacı); H1, en uzun kartların üç ekran boyutunda kartın içine sığdığını ve desteyle oyunun bir sona kadar oynandığını denetler. H2 son testleri kendi kontrollü destelerini kullanır.
- Bozuk/eksik destede başlatma kapatılır ve yeniden yükleme sunulur. Dosya düzeltildikten sonra uygulamayı kapatmadan **Veriyi yeniden yükle** ile toparlanır.

Oyun yalnız kendi `res://Veri/` dosyalarını okur; kardeş AI/Docs repolarına veya bir sunucuya çalışma zamanında ihtiyaç duymaz. Deste JSON'unun paketlenen oyunda bulunduğu ayrıca export üzerinde doğrulanmalıdır.
