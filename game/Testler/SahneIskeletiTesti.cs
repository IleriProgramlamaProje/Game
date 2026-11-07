using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public partial class SahneIskeletiTesti : Node
{
	private int _kontrol;
	private void Dogrula(bool kosul, string mesaj)
	{
		_kontrol++;
		if (!kosul) throw new InvalidOperationException(mesaj);
	}
	private void HataBekle(Action islem)
	{
		try { islem(); } catch (FormatException) { _kontrol++; return; }
		throw new InvalidOperationException("Bozuk veri reddedilmedi.");
	}
	private static string Oku(string yol) => Godot.FileAccess.GetFileAsString(yol);
	public override async void _Ready()
	{
		try
		{
			CekirdegiDene();
			await SahneyiDene();
			GD.Print($"H1 TESTLERİ BAŞARILI: {_kontrol} kontrol; çekirdek, deste, fare/dokunma/klavye, 3 ekran boyutu.");
			GetTree().Quit();
		}
		catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
	}
	private void CekirdegiDene()
	{
		var tema = Tema.Oku(Oku("res://Veri/tema.json"));
		var roller = tema.Karakterler.Select(r => r.Anahtar).ToHashSet();
		string json = Oku("res://Testler/Veri/ornek_deste.json");
		var deste = new YedekDeste(json, roller);
		Dogrula(deste.Adet == 10, "On örnek kart bulunmalı.");
		var ilk = deste.Sonraki();
		for (int i = 0; i < 9; i++) deste.Sonraki();
		Dogrula(deste.Sonraki() == ilk, "Deste sırayla başa dönmeli.");
		deste.Sifirla();
		Dogrula(deste.Sonraki() == ilk, "Yeni oyunda deste sıfırlanmalı.");
		foreach (string bozuk in new[] { "{", "[]", "null", "[null]", "[{}]", json.Replace("\"para\": -10", "\"para\": -20"),
					json.Replace("\"hazinedar\"", "\"yok\"") })
			HataBekle(() => new YedekDeste(bozuk, roller));
		HataBekle(() => Tema.Oku("{}"));
		var g = new Gostergeler();
		Dogrula(g.Degerler.Values.All(v => v == 50), "Başlangıç 50 olmalı.");
		g.Uygula(new Dictionary<string, int> { ["para"] = -15, ["halk"] = 10 });
		Dogrula(g.Degerler["para"] == 35 && g.Degerler["halk"] == 60 && g.Degerler["ordu"] == 50, "Etkiler yanlış.");
		HataBekle(() => g.Uygula(new Dictionary<string, int> { ["para"] = -5, ["yok"] = 5 }));
		Dogrula(g.Degerler["para"] == 35, "Geçersiz etki kısmen uygulanmamalı.");
		for (int i = 0; i < 5; i++) g.Uygula(new Dictionary<string, int> { ["para"] = -15, ["halk"] = 15 });
		Dogrula(g.Degerler["para"] == 0 && g.Degerler["halk"] == 100 && g.Sinirda, "İki sınır da tutulmalı.");
		var isimler = new MetinYerlestirici(tema.Karakterler, Hukumdar.Hatun, new Random(7));
		string baslik = isimler.Baslik("hazinedar");
		Dogrula(baslik == isimler.Baslik("hazinedar"), "İsim oturumda değişmemeli.");
		Dogrula(isimler.Yerlestir("{hitap}, hoş geldiniz.") == "Hatunum, hoş geldiniz.", "Hitap yanlış.");
		Dogrula(isimler.Yerlestir("Sade metin.") == "Sade metin.", "Metne zorla hitap eklenmemeli.");
		foreach (var rol in tema.Karakterler)
		{
			var kimlik = isimler.Kimlik(rol.Anahtar);
			Dogrula(rol.Cinsiyet == "Çift" || kimlik.Cinsiyet == rol.Cinsiyet, "Rol cinsiyeti yanlış.");
			Dogrula(!string.IsNullOrEmpty(kimlik.PortreAnahtari), "Portre eşlemesi oturumda hazır olmalı.");
		}
		var oyun = new OyunYoneticisi(deste);
		Dogrula(!oyun.KararVer(false, 0) && !oyun.DevamEt(), "Açılışta karar verilmemeli.");
		oyun.Baslat(Hukumdar.Bey);
		long id = oyun.KartKimligi;
		Dogrula(oyun.KararVer(false, id), "İlk karar uygulanmalı.");
		Dogrula(!oyun.KararVer(true, id) && oyun.Durum.KararSayisi == 1, "Çift giriş tek sayılmalı.");
		oyun.DevamEt();
		Dogrula(!oyun.KararVer(true, id), "Eski kartın girdisi reddedilmeli.");
		for (int i = 0; i < 100 && oyun.Asama != Asama.Son; i++)
		{ oyun.KararVer(false, oyun.KartKimligi); oyun.DevamEt(); }
		Dogrula(oyun.Asama == Asama.Son && !oyun.KararVer(true, oyun.KartKimligi), "Sınırda oyun durmalı.");
		oyun.Baslat(Hukumdar.Hatun);
		Dogrula(oyun.Durum.KararSayisi == 0 && oyun.Durum.Gostergeler.Degerler.Values.All(v => v == 50), "Yeni oyun sıfırlanmalı.");
		Dogrula(!oyun.KararVer(false, id), "Önceki oturumun girdisi reddedilmeli.");
	}
	private async Task Bekle()
	{
		for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}
	private static void GeriTusu(Node ekran) => ekran.PropagateNotification((int)Node.NotificationWMGoBackRequest);
	private void Girdi(InputEvent olay) => GetViewport().PushInput(olay, true);
	private void Fare(bool basili, Vector2 konum) => Girdi(new InputEventMouseButton
	{ ButtonIndex = MouseButton.Left, Pressed = basili, Position = konum, GlobalPosition = konum });
	private void Hareket(Vector2 konum) => Girdi(new InputEventMouseMotion
	{ Position = konum, GlobalPosition = konum, ButtonMask = MouseButtonMask.Left });

	private async Task SahneyiDene()
	{
		var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
		foreach (var boyut in new[] { new Vector2I(1152, 648), new Vector2I(360, 640), new Vector2I(640, 360) })
		{
			GetWindow().Size = boyut;
			GetWindow().ContentScaleSize = boyut;
			GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
			GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Ignore;
			var ekran = sahne.Instantiate<OyunEkranlari>();
			ekran.DesteYolu = "res://Testler/Veri/ornek_deste.json"; // Sayımları ve metinleri bilinen örnek deste.
			AddChild(ekran);
			await Bekle();
			Dogrula(ekran.Oyun != null && ekran.AktifEkran == OyunEkranlari.Ekran.Acilis, "Açılış/veri yükleme yanlış.");
			var gecisler = new List<string>();
			ekran.SahneGecisi = gecisler.Add;
			GeriTusu(ekran);
			Dogrula(gecisler.SequenceEqual(new[] { OyunEkranlari.AnaMenuYolu }), "Hükümdar seçiminde geri tuşu ana menüye dönmeli.");
			await YerlesimVeGoruntu(ekran, boyut);
			ekran.GetNode<Button>("%Hatunum").EmitSignal(BaseButton.SignalName.Pressed);
			await Bekle();
			Dogrula(ekran.Oyun.Durum.Cinsiyet == Hukumdar.Hatun && ekran.GetNode<Label>("%KartMetni").Text.Contains("Hatunum"), "Seçim karta yansımalı.");
			gecisler.Clear();
			GeriTusu(ekran);
			Dogrula(gecisler.Count == 0 && ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "Oyun ortasında geri tuşu oyunu bırakmamalı.");
			await YerlesimVeGoruntu(ekran, boyut);
			var kart = ekran.GetNode<KaydirmaKontrolu>("%KartAlani");
			for (int karar = 0; karar < 3; karar++)
			{
				kart.SuruklemeyiBaslat(200);
				kart.SuruklemeyiGuncelle(200 + kart.Esik * 1.5f);
				kart.SuruklemeyiBitir(200 + kart.Esik * 1.5f);
				await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
				Dogrula(ekran.Oyun.Durum.KararSayisi == karar + 1, "Çevirme bir karar vermeli.");
				GeriTusu(ekran);
				Dogrula(gecisler.Count == 0 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Sonuç ekranında geri tuşu oyunu bırakmamalı.");
				await YerlesimVeGoruntu(ekran, boyut);
				ekran.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
				await Bekle();
			}
			await GercekGirdileriDene(ekran, boyut);
			for (int i = 0; i < 100 && ekran.AktifEkran != OyunEkranlari.Ekran.Son; i++)
			{
				if (ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc) ekran.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
				else
				{
					kart.SuruklemeyiBaslat(200);
					kart.SuruklemeyiGuncelle(200 - kart.Esik * 1.5f);
					kart.SuruklemeyiBitir(200 - kart.Esik * 1.5f);
					await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
				}
			}
			Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Son, "Oyun sınırda sona ermeli.");
			await YerlesimVeGoruntu(ekran, boyut);
			GeriTusu(ekran);
			Dogrula(gecisler.SequenceEqual(new[] { OyunEkranlari.AnaMenuYolu }), "Sonda geri tuşu ana menüye dönmeli.");
			gecisler.Clear();
			ekran.GetNode<Button>("%AnaMenuyeDon").EmitSignal(BaseButton.SignalName.Pressed);
			Dogrula(gecisler.SequenceEqual(new[] { OyunEkranlari.AnaMenuYolu }), "Sonda Ana Menüye Dön ana menüye dönmeli.");
			ekran.GetNode<Button>("%Yeniden").EmitSignal(BaseButton.SignalName.Pressed);
			ekran.GetNode<Button>("%Beyim").EmitSignal(BaseButton.SignalName.Pressed);
			Dogrula(ekran.Oyun.Durum.KararSayisi == 0, "Yeniden başlama sayacı sıfırlamalı.");
			ekran.Free();
			await GercekDesteyiDene(sahne, boyut);
		}
	}
	// Kart eğik olabilir; öğe, kartın kendi koordinatlarında kartın sınırları içinde mi?
	private static bool KartinIcinde(Control kart, Control oge)
	{
		var konum = (kart.GetGlobalTransform().AffineInverse() * oge.GetGlobalTransform()).Origin;
		return new Rect2(Vector2.Zero, kart.Size).Grow(1).Encloses(new Rect2(konum, oge.Size));
	}
	/// <summary>Altın set destesinin en uzun metinli ve en uzun kararlı kartları kartın içine sığar.</summary>
	private async Task GercekDesteyiDene(PackedScene sahne, Vector2I boyut)
	{
		var ekran = sahne.Instantiate<OyunEkranlari>();
		AddChild(ekran);
		await Bekle();
		Dogrula(ekran.Oyun != null, "Altın set destesi oyunda yüklenmeli.");
		ekran.GetNode<Button>("%Hatunum").EmitSignal(BaseButton.SignalName.Pressed);
		await Bekle();
		var tema = Tema.Oku(Oku("res://Veri/tema.json"));
		var kartlar = new YedekDeste(Oku(ekran.DesteYolu), tema.Karakterler.Select(r => r.Anahtar).ToHashSet()).KartlariYukle();
		static int Kelime(string s) => s.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
		var zorlar = kartlar.OrderByDescending(k => Kelime(k.Metin)).Take(3)
			.Concat(kartlar.OrderByDescending(k => Math.Max(k.Sol.Etiket.Length, k.Sag.Etiket.Length) + k.Sol.Etki.Count + k.Sag.Etki.Count).Take(2));
		var metin = new MetinYerlestirici(tema.Karakterler, Hukumdar.Hatun, new Random(3));
		var kart = ekran.GetNode<KartGorunumu>("%KartAlani");
		foreach (var zor in zorlar)
		{
			kart.KartiGoster(zor, metin);
			await Bekle();
			foreach (int yon in new[] { -1, 1 })
			{
				kart.SuruklemeyiBaslat(200);
				kart.SuruklemeyiGuncelle(200 + yon * kart.Esik);
				await Bekle();
				foreach (string ad in new[] { "KartBasligi", "KartMetni", "KararOnizleme" })
					Dogrula(KartinIcinde(kart, ekran.GetNode<Control>("%" + ad)), $"{boyut}: {zor.Id} {ad} kartın dışına taşıyor.");
				kart.Iptal();
			}
		}
		ekran.Free();

		// 2.9: Altın set destesiyle açılış → karar → sonuç → Devam Et → son → yeniden oyun; ağ gerekmez.
		var oyun = sahne.Instantiate<OyunEkranlari>();
		AddChild(oyun);
		await Bekle();
		oyun.GetNode<Button>("%Beyim").EmitSignal(BaseButton.SignalName.Pressed);
		await Bekle();
		var oyunKarti = oyun.GetNode<KartGorunumu>("%KartAlani");
		int karar = 0;
		while (oyun.AktifEkran != OyunEkranlari.Ekran.Son && karar < 200)
		{
			Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Oyun, "Sonuçtan sonra yeni kart gelmeli.");
			oyunKarti.SuruklemeyiBaslat(200);
			oyunKarti.SuruklemeyiGuncelle(200 + oyunKarti.Esik * 1.5f);
			oyunKarti.SuruklemeyiBitir(200 + oyunKarti.Esik * 1.5f);
			await Animasyon();
			karar++;
			Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Sonuc && oyun.GetNode<Label>("%SonucMetni").Text.Length > 0,
				"Her karardan sonra sonuç gösterilmeli.");
			oyun.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
			await Bekle();
		}
		Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Son && oyun.GetNode<Label>("%SonMetni").Text.Length > 0 &&
			oyun.GetNode<Label>("%SonOzet").Text.Contains($"{karar} karar"), "Altın set destesiyle oyun bir sona ulaşmalı.");
		await YerlesimVeGoruntu(oyun, boyut, "-AltinSet");
		oyun.GetNode<Button>("%Yeniden").EmitSignal(BaseButton.SignalName.Pressed);
		Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Acilis, "Tekrar Oyna hükümdar seçimine dönmeli.");
		oyun.GetNode<Button>("%Beyim").EmitSignal(BaseButton.SignalName.Pressed);
		Dogrula(oyun.Oyun.Durum.KararSayisi == 0 && oyun.AktifEkran == OyunEkranlari.Ekran.Oyun, "Yeni oyun sıfırdan başlamalı.");
		oyun.Free();
	}
	private async Task Animasyon() => await ToSignal(GetTree().CreateTimer(0.35), SceneTreeTimer.SignalName.Timeout);
	private void Tus(Key tus)
	{
		Girdi(new InputEventKey { Keycode = tus, Pressed = true });
		Girdi(new InputEventKey { Keycode = tus, Pressed = false });
	}
	private async Task DevamEt(OyunEkranlari ekran)
	{
		ekran.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
		await Bekle();
	}
	// Kartın üst-orta noktasını görünür alana getirip viewport koordinatını döndürür.
	private async Task<Vector2> KartNoktasi(OyunEkranlari ekran, KaydirmaKontrolu kart, Vector2I boyut)
	{
		var scroll = ekran.GetNode<ScrollContainer>("%Icerik");
		var nokta = kart.GetGlobalTransformWithCanvas() * new Vector2(kart.Size.X / 2, 70);
		if (nokta.Y < 40 || nokta.Y > boyut.Y - 40)
		{
			scroll.ScrollVertical += (int)(nokta.Y - boyut.Y / 3f);
			await Bekle();
			nokta = kart.GetGlobalTransformWithCanvas() * new Vector2(kart.Size.X / 2, 70);
		}
		Dogrula(nokta.Y >= 0 && nokta.Y <= boyut.Y, $"{boyut}: kart dokunulabilir alanda değil.");
		return nokta;
	}
	/// <summary>Viewport'a gerçek fare, çoklu dokunma ve klavye girdisi gönderir.</summary>
	private async Task GercekGirdileriDene(OyunEkranlari ekran, Vector2I boyut)
	{
		var kart = ekran.GetNode<KartGorunumu>("%KartAlani");
		var onizleme = ekran.GetNode<Label>("%KararOnizleme");
		int sayi = ekran.Oyun.Durum.KararSayisi;
		Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "Gerçek girdi testi kart ekranında başlamalı.");
		// Kart içeriği kadar uzar; metin çerçevenin dışına taşmaz.
		Dogrula(kart.Size.Y <= Mathf.Max(260, kart.GetCombinedMinimumSize().Y) + 1, $"{boyut}: kart içeriğinden uzun.");
		foreach (string ad in new[] { "KartBasligi", "KartMetni", "KararOnizleme" })
			Dogrula(kart.GetGlobalRect().Grow(1).Encloses(ekran.GetNode<Control>("%" + ad).GetGlobalRect()), $"{boyut}: {ad} kartın dışına taşıyor.");

		// Fare: kısa sürükleme iptal, eşik üstü tek karar, ikinci bırakma etkisiz.
		var bas = await KartNoktasi(ekran, kart, boyut);
		Fare(true, bas); Hareket(bas + new Vector2(20, 0)); Fare(false, bas + new Vector2(20, 0));
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi && ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "Kısa sürükleme iptal olmalı.");
		Fare(true, bas); Hareket(bas + new Vector2(kart.Esik + 6, 0));
		Dogrula(onizleme.Text.Contains("▲") || onizleme.Text.Contains("▼"), "Önizlemede etki okları olmalı.");
		Dogrula(onizleme.Text.Contains("Seçmek için bırak"), "Eşik geçilince bırakma ipucu görünmeli.");
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi, "Önizleme karar uygulamamalı.");
		await YerlesimVeGoruntu(ekran, boyut, "-Onizleme");
		Fare(false, bas + new Vector2(kart.Esik + 6, 0));
		Fare(false, bas);
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 1 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Fare sürüklemesi tek karar vermeli.");
		await DevamEt(ekran);

		// Çoklu dokunma: ikinci parmak karar veremez, ilk parmak tek karar verir.
		bas = await KartNoktasi(ekran, kart, boyut);
		var sol = bas + new Vector2(-kart.Esik - 6, 0);
		Girdi(new InputEventScreenTouch { Index = 0, Pressed = true, Position = bas });
		Girdi(new InputEventScreenTouch { Index = 1, Pressed = true, Position = bas });
		Girdi(new InputEventScreenDrag { Index = 1, Position = sol });
		Girdi(new InputEventScreenTouch { Index = 1, Pressed = false, Position = sol });
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 1, "İkinci parmak karar verememeli.");
		Girdi(new InputEventScreenDrag { Index = 0, Position = sol });
		Girdi(new InputEventScreenTouch { Index = 0, Pressed = false, Position = sol });
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 2, "İlk parmak tek karar vermeli.");
		await DevamEt(ekran);

		// Klavye: ilk ok önizler, ters ok vazgeçer; ok + Enter ya da aynı ok iki kez onaylar.
		Dogrula(kart.HasFocus(), "Kart ekranında odak kartta olmalı.");
		Tus(Key.Left);
		await Animasyon();
		Dogrula(kart.KlavyeOnizlemesi && onizleme.Text.Contains("Enter") && ekran.Oyun.Durum.KararSayisi == sayi + 2,
			"Sol ok önizleme göstermeli, karar vermemeli.");
		await YerlesimVeGoruntu(ekran, boyut, "-Klavye");
		Tus(Key.Right);
		await Animasyon();
		Dogrula(!kart.KlavyeOnizlemesi && onizleme.Text == KartGorunumu.BeklemeMetni && ekran.Oyun.Durum.KararSayisi == sayi + 2,
			"Ters ok vazgeçmeli.");
		Tus(Key.Left); Tus(Key.Enter);
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 3 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Ok + Enter karar vermeli.");
		Tus(Key.Enter);
		await Bekle();
		Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "Sonuçta odaklı Devam Et, Enter ile ilerlemeli.");
		Tus(Key.Right); Tus(Key.Right);
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 4, "Aynı ok iki kez karar vermeli.");
		await DevamEt(ekran);
		await TelefonDokunmasiniDene(ekran, kart, boyut);
	}
	private void TaklitFare(bool basili, Vector2 konum) => Girdi(new InputEventMouseButton
	{ Device = KaydirmaKontrolu.TaklitAygit, ButtonIndex = MouseButton.Left, Pressed = basili, Position = konum, GlobalPosition = konum });
	private void TaklitHareket(Vector2 konum) => Girdi(new InputEventMouseMotion
	{ Device = KaydirmaKontrolu.TaklitAygit, Position = konum, GlobalPosition = konum, ButtonMask = MouseButtonMask.Left });
	/// <summary>
	/// G0 bulgusu: telefonda Godot her dokunmadan hemen önce taklit bir fare olayı gönderir; sayfa kaydırma ve
	/// ses kaydırıcısı bu taklide dayanır. Kart dokunmanın kendisini izlemeli: tek karar, iptal edilen dokunma kararsız.
	/// </summary>
	private async Task TelefonDokunmasiniDene(OyunEkranlari ekran, KaydirmaKontrolu kart, Vector2I boyut)
	{
		Dogrula((bool)ProjectSettings.GetSetting("input_devices/pointing/emulate_mouse_from_touch", true),
			"Telefonda sayfa kaydırma ve ses kaydırıcısı için dokunmadan fare taklidi açık olmalı.");
		Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "Telefon dokunması testi kart ekranında başlamalı.");
		int sayi = ekran.Oyun.Durum.KararSayisi;
		var bas = await KartNoktasi(ekran, kart, boyut);
		var sag = bas + new Vector2(kart.Esik + 6, 0);
		void Dokun(bool iptal)
		{
			TaklitFare(true, bas); Girdi(new InputEventScreenTouch { Index = 0, Pressed = true, Position = bas });
			TaklitHareket(sag); Girdi(new InputEventScreenDrag { Index = 0, Position = sag });
			TaklitFare(false, sag); Girdi(new InputEventScreenTouch { Index = 0, Pressed = false, Position = sag, Canceled = iptal });
		}
		Dokun(iptal: true); // Ör. sürüklerken bildirim perdesi çekildi.
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi && ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "İptal edilen dokunma karar vermemeli.");
		Dokun(iptal: false);
		await Animasyon();
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 1 && ekran.AktifEkran != OyunEkranlari.Ekran.Oyun, "Telefondaki dokunma tek karar vermeli.");
		if (ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc) await DevamEt(ekran);
	}
	private async Task YerlesimVeGoruntu(OyunEkranlari ekran, Vector2I boyut, string ek = "")
	{
		await Bekle();
		foreach (var e in Enum.GetValues<OyunEkranlari.Ekran>())
			Dogrula(ekran.GetNode<Control>("%" + e + "Paneli").IsVisibleInTree() == (e == ekran.AktifEkran), "Tek panel görünmeli.");
		var scroll = ekran.GetNode<ScrollContainer>("%Icerik");
		Dogrula(scroll.GetHScrollBar().MaxValue <= scroll.Size.X + 1, $"{boyut}: yatay taşma.");
		foreach (string ad in new[] { "Beyim", "Hatunum", "Devam", "Yeniden", "AnaMenuyeDon" })
		{
			var dugme = ekran.GetNode<Button>("%" + ad);
			if (dugme.IsVisibleInTree()) Dogrula(dugme.Size.Y >= 48, "Dokunma hedefi küçük.");
		}
		if (DisplayServer.GetName() != "headless")
		{
			await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
			DirAccess.MakeDirRecursiveAbsolute("res://.godot/testler");
			var hata = GetViewport().GetTexture().GetImage().SavePng($"res://.godot/testler/{boyut.X}x{boyut.Y}-{ekran.AktifEkran}{ek}.png");
			Dogrula(hata == Error.Ok, "Ekran görüntüsü kaydedilemedi.");
		}
	}
}
