# Yerel oyun verisi

`tema.json`, Docs deposunun `80d6afa10efc32e1f7de28d4f7b1f47458e74bf5` revizyonundaki dosyanın birebir kopyasıdır (2 Kasım 2026). Onaylı açılış, 23 rol, sekiz sonun başlık/metni ve yıl oranı buradan okunur. Eksik son veya sıfır/negatif yıl oranı veri hatasıdır. `hedef_yil: null` korunur; kazanma H3 işidir.

`yedek_deste.json` şu anda **10 yerel örnek kart** içerir. Çapraz okuma yapılmış altın set değildir; `kaynak: model`, `lisans: sadece_ders` alanları değiştirilmedi. 2 Kasım'da kullanıcının örnek desteyle ilerleme tercihiyle kullanılır. `2.8` görevinin onaylı veri şartı hâlâ bekler.

`yedek_desta.json`, Ömer’in `bccec30` revizyonundaki on kartlık H1 destesidir; özgün dosya adı, `id`, `kaynak: altin_set` ve `lisans: sadece_ders` etiketleri korundu. Etiketler tek başına çapraz okuma kanıtı sayılmaz. Bu deste bileşen testinde okunur; oyunun varsayılanı yukarıdaki `yedek_deste.json` dosyasıdır.

## Deste sözleşmesi ve değiştirme

- Varsayılan kapsayıcı boş olmayan bir **JSON dizisi**dir. Ömer’in H1 biçimi olan `{ "kartlar": [...] }` da aynı doğrulayıcıyla okunur; isteğe bağlı `id` alanı korunur. Kart alanları `konusan`, `konu`, `metin`, `sol`, `sag`, `kaynak`, `lisans`tır. Seçenekler `etiket`, `etki`, `sonuc` içerir. Biçim Docs/kart_formati.md bölüm 3 ile aynıdır.
- Rol tema listesinde bulunmalı; metin 20–50, karar 2–5 kelime olmalı. Her karar 1–3 göstergeyi ±5/10/15 etkiler ve en az bir negatif etki içerir. Boş sonuç, tanınmayan alan ve bozuk veri reddedilir.
- Kartlar dosya sırasıyla dolaşır; son karttan sonra ilk karta dönülür. Yeni oyun ilk karttan başlar. Bu H1'den gelen deneme davranışıdır; ekipçe onaylanmış nihai tekrar politikası değildir.
- Onaylı deste geldiğinde `AI/data/altin_set/yedek_deste.json` dosyasını buraya kopyalayın; kaynak/lisans alanlarını değiştirmeyin. `Oyun` kökünün Inspector'daki `DesteYolu` alanı alternatif bir paketlenmiş JSON dosyasını da seçebilir.
- Değişim sonrası derleme ve H1/H2 testlerini çalıştırın. Kart sayısı ondan farklıysa H1 testinin örnek desteye özel sayım/başa dönüş senaryosu da yeni desteye uyarlanmalıdır. H2 son testleri kendi kontrollü destelerini kullanır.
- Bozuk/eksik destede başlatma kapatılır ve yeniden yükleme sunulur. Dosya düzeltildikten sonra uygulamayı kapatmadan **Veriyi yeniden yükle** ile toparlanır.

Oyun yalnız kendi `res://Veri/` dosyalarını okur; kardeş AI/Docs repolarına veya bir sunucuya çalışma zamanında ihtiyaç duymaz. Deste JSON'unun paketlenen oyunda bulunduğu ayrıca export üzerinde doğrulanmalıdır.
