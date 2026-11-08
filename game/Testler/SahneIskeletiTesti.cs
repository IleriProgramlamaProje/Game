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
			GD.Print($"H1 TESTLERİ BAŞARILI: {_kontrol} kontrol; çekirdek, Altın Set, taht salonu, konuşma, harita, fare/dokunma/klavye, geri tuşu, 5 ekran boyutu.");
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
	private async Task Sure(double saniye) => await ToSignal(GetTree().CreateTimer(saniye), SceneTreeTimer.SignalName.Timeout);
	private static void GeriTusu(Node ekran) => ekran.PropagateNotification((int)Node.NotificationWMGoBackRequest);
	private static void Bas(Node ekran, string ad) => ekran.GetNode<BaseButton>("%" + ad).EmitSignal(BaseButton.SignalName.Pressed);
	private void Girdi(InputEvent olay) => GetViewport().PushInput(olay, true);
	private void Fare(bool basili, Vector2 konum, int aygit = 0) => Girdi(new InputEventMouseButton
	{ Device = aygit, ButtonIndex = MouseButton.Left, Pressed = basili, Position = konum, GlobalPosition = konum });
	private void Tikla(Vector2 konum) { Fare(true, konum); Fare(false, konum); }
	private void Tus(Key tus)
	{
		Girdi(new InputEventKey { Keycode = tus, Pressed = true });
		Girdi(new InputEventKey { Keycode = tus, Pressed = false });
	}
	/// <summary>Öğeyi görünür alana alıp ortasının viewport koordinatını döndürür.</summary>
	private async Task<Vector2> Orta(OyunEkranlari ekran, Control oge, Vector2I boyut)
	{
		ekran.GetNode<ScrollContainer>("%Icerik").EnsureControlVisible(oge);
		await Bekle();
		var nokta = oge.GetGlobalTransformWithCanvas() * (oge.Size / 2);
		Dogrula(nokta.Y >= 0 && nokta.Y <= boyut.Y && nokta.X >= 0 && nokta.X <= boyut.X, $"{boyut}: {oge.Name} dokunulabilir alanda değil.");
		return nokta;
	}
	/// <summary>Karar kartını sağa ya da sola kaydırır (metin önce tamamlanır).</summary>
	private static void Kaydir(OyunEkranlari ekran, bool sag)
	{
		var kart = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
		kart.Tamamla();
		float x = 200 + (sag ? 1 : -1) * kart.Esik * 1.5f;
		kart.SuruklemeyiBaslat(200);
		kart.SuruklemeyiGuncelle(x);
		kart.SuruklemeyiBitir(x);
	}
	private static string Hitap(OyunEkranlari ekran) => ekran.Oyun.Durum.Cinsiyet == Hukumdar.Bey ? "Beyim" : "Hatunum";

	private async Task SahneyiDene()
	{
		var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
		var tema = Tema.Oku(Oku("res://Veri/tema.json"));
		string Unvan(string rol) => tema.Karakterler.Single(r => r.Anahtar == rol).Unvan;
		// 360×800: dikey telefonun mantıksal ekranı (G0, POCO X5 Pro 1080×2400).
		foreach (var boyut in new[] { new Vector2I(1152, 648), new Vector2I(923, 520), new Vector2I(360, 640), new Vector2I(360, 800), new Vector2I(640, 360) })
		{
			GetWindow().Size = boyut;
			GetWindow().ContentScaleSize = boyut;
			GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
			GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Ignore;
			var ekran = sahne.Instantiate<OyunEkranlari>();
			ekran.DesteYolu = "res://Testler/Veri/ornek_deste.json"; // Sayımları ve metinleri bilinen örnek deste.
			ekran.Animasyonlar = false;
			AddChild(ekran);
			await Bekle();
			Dogrula(ekran.Oyun != null && ekran.AktifEkran == OyunEkranlari.Ekran.Acilis, "Açılış/veri yükleme yanlış.");
			var gecisler = new List<string>();
			ekran.SahneGecisi = gecisler.Add;
			GeriTusu(ekran);
			Dogrula(gecisler.SequenceEqual(new[] { OyunEkranlari.AnaMenuYolu }), "Hükümdar seçiminde geri tuşu ana menüye dönmeli.");
			await YerlesimVeGoruntu(ekran, boyut);
			Bas(ekran, "Hatunum");
			await Bekle();
			var diyalog = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
			var salon = ekran.GetNode<SahneGorunumu>("%Sahne");
			var kart = ekran.Oyun.AktifKart;
			Dogrula(ekran.Oyun.Durum.Cinsiyet == Hukumdar.Hatun && diyalog.TamMetin.Contains("Hatunum"), "Seçim konuşmaya yansımalı.");
			Dogrula(ekran.GetNode<Label>("%KonusanAdi").Text.StartsWith(Unvan(kart.Konusan) + " "), "Konuşanın unvanı ve adı kutuda görünmeli.");
			Dogrula(salon.Zemin?.ResourcePath == KarakterGorselleri.MekanCizimi(Mekan.TahtSalonuAnahtari) &&
				salon.Hukumdar?.ResourcePath == KarakterGorselleri.HukumdarYolu(Hukumdar.Hatun),
				"Taht salonunda Hatun tahtta oturmalı.");
			Dogrula(salon.Ziyaretci?.ResourcePath == KarakterGorselleri.PortreYolu(kart.Konusan), "Kartın konuşanı taht salonuna gelmeli.");
			gecisler.Clear();
			GeriTusu(ekran);
			Dogrula(gecisler.Count == 0 && ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "Kayıtsız oyunda geri tuşu oyunu bırakmamalı.");
			Dogrula(ekran.GetNode<ScrollContainer>("%Icerik").ScrollVertical == 0, $"{boyut}: yeni kart göstergeleri gizlememeli.");
			await YerlesimVeGoruntu(ekran, boyut);
			await UzunMetinYerlesimi(ekran, boyut);
			for (int karar = 0; karar < 3; karar++)
			{
				Kaydir(ekran, sag: true);
				await Bekle();
				Dogrula(ekran.Oyun.Durum.KararSayisi == karar + 1 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Kartı kaydırmak bir karar vermeli.");
				Dogrula(diyalog.TamMetin == ekran.Oyun.SonKarar.Sonuc.Replace("{hitap}", "Hatunum") &&
					ekran.GetNode<Label>("%EylemBilgisi").Text.Length > 0, "Sonuç ve etkileri kutuda anlatılmalı.");
				GeriTusu(ekran);
				Dogrula(gecisler.Count == 0 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Sonuçta geri tuşu oyunu bırakmamalı.");
				await YerlesimVeGoruntu(ekran, boyut);
				Bas(ekran, "Devam");
				await Bekle();
			}
			await IstatistikleriDene(ekran, boyut);
			await HaritayiDene(ekran, boyut, tema);
			await GercekGirdileriDene(ekran, boyut);
			for (int i = 0; i < 100 && ekran.AktifEkran != OyunEkranlari.Ekran.Son; i++)
				if (ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc) Bas(ekran, "Devam");
				else Kaydir(ekran, sag: false);
			Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Son, "Oyun sınırda sona ermeli.");
			Bas(ekran, "HaritaDugmesi");
			Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Son, "Sonda harita açılmamalı.");
			await YerlesimVeGoruntu(ekran, boyut);
			GeriTusu(ekran);
			Dogrula(gecisler.SequenceEqual(new[] { OyunEkranlari.AnaMenuYolu }), "Sonda geri tuşu ana menüye dönmeli.");
			gecisler.Clear();
			Bas(ekran, "AnaMenuyeDon");
			Dogrula(gecisler.SequenceEqual(new[] { OyunEkranlari.AnaMenuYolu }), "Sonda Ana Menüye Dön ana menüye dönmeli.");
			Bas(ekran, "Yeniden");
			Bas(ekran, "Beyim");
			Dogrula(ekran.Oyun.Durum.KararSayisi == 0 && salon.Hukumdar?.ResourcePath == KarakterGorselleri.HukumdarYolu(Hukumdar.Bey),
				"Yeniden başlama sayacı sıfırlamalı, tahtta Bey oturmalı.");
			ekran.Free();
			await GercekDesteyiDene(sahne, boyut);
		}
	}

	private static bool KartinIcinde(Control kart, Control oge)
	{
		var konum = (kart.GetGlobalTransform().AffineInverse() * oge.GetGlobalTransform()).Origin;
		return new Rect2(Vector2.Zero, kart.Size).Grow(1).Encloses(new Rect2(konum, oge.Size));
	}

	/// <summary>İstatistik çubuğu özeti gösterir; dokununca ayrıntılı çubuklar açılır, yine dokununca kapanır.</summary>
	private async Task IstatistikleriDene(OyunEkranlari ekran, Vector2I boyut)
	{
		var cubuk = ekran.GetNode<Button>("%IstatistikCubugu");
		var gostergeler = ekran.GetNode<Control>("%Gostergeler");
		var salon = ekran.GetNode<SahneGorunumu>("%Sahne");
		Dogrula(cubuk.IsVisibleInTree() && !gostergeler.IsVisibleInTree() && !ekran.IstatistiklerAcik, "Oyunda istatistikler kapalı başlamalı.");
		foreach (var (anahtar, deger) in ekran.Oyun.Durum.Gostergeler.Degerler)
			Dogrula(ekran.GetNode<Label>("%OzetDeger_" + anahtar).Text == deger.ToString(), $"Özet {anahtar} değeri yanlış.");
		Dogrula(ekran.GetNode<Label>("%OzetYil").Text == $"Yıl {ekran.Oyun.Durum.Zaman.TamamlananYil}", "Özette yıl olmalı.");
		var cizim = salon.CizimAlani;
		var nokta = await Orta(ekran, cubuk, boyut);
		Tikla(nokta);
		await Bekle();
		Dogrula(gostergeler.IsVisibleInTree() && ekran.IstatistiklerAcik && ekran.GetNode<Label>("%OzetOk").Text == "▴",
			$"Çubuğa dokununca ayrıntılı göstergeler açılmalı (açık={ekran.IstatistiklerAcik}, kutu={ekran.GetNode<Control>("%GostergeKutusu").Visible}, ağaçta={gostergeler.IsVisibleInTree()}, ok={ekran.GetNode<Label>("%OzetOk").Text}, nokta={nokta}).");
		Dogrula(salon.CizimAlani.IsEqualApprox(cizim), $"{boyut}: istatistikler açılınca sahne büyüyüp küçülmemeli.");
		await YerlesimVeGoruntu(ekran, boyut, "-Istatistik");
		Tikla(await Orta(ekran, cubuk, boyut));
		await Bekle();
		Dogrula(!gostergeler.IsVisibleInTree() && !ekran.IstatistiklerAcik, "İkinci dokunuş göstergeleri kapatmalı.");
		Dogrula(salon.CizimAlani.IsEqualApprox(cizim), $"{boyut}: istatistikler kapanınca sahne aynı kalmalı.");
	}

	/// <summary>Sığmayan metin sayfalara bölünür; her sayfa kutunun içinde kalır, sayfalar metni eksiksiz taşır.</summary>
	private async Task UzunMetinYerlesimi(OyunEkranlari ekran, Vector2I boyut)
	{
		var diyalog = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
		var etiket = ekran.GetNode<Label>("%DiyalogMetni");
		string uzun = string.Join(" ", Enumerable.Repeat("Hükümdarlığımızın", 60));
		diyalog.Goster("Vezir Ali", uzun);
		await Bekle();
		Dogrula(diyalog.SayfaSayisi > 1 && string.Join(" ", diyalog.Sayfalar) == uzun, $"{boyut}: uzun metin eksiksiz sayfalara bölünmeli.");
		Dogrula(!diyalog.Etkin, "Metin bitmeden kart kaydırılamamalı.");
		for (int sayfa = 0; sayfa < diyalog.SayfaSayisi; sayfa++)
		{
			Dogrula(diyalog.Sayfa == sayfa && etiket.GetLineCount() <= diyalog.SatirSiniri &&
				diyalog.GetGlobalRect().Grow(1).Encloses(etiket.GetGlobalRect()), $"{boyut}: {sayfa}. sayfa kutuya sığmalı.");
			diyalog.Ilerle();
			await Bekle();
		}
		Dogrula(diyalog.Bitti && diyalog.Etkin, "Son sayfadan sonra kart kaydırılabilmeli.");
		ekran.KartiGoster(ekran.Oyun.AktifKart);
		await Bekle();
	}

	/// <summary>Harita: altı mekân çizimin içinde, üst üste binmeden; ok tuşu atlar, Enter girer; geri tuşu ve Esc döner.</summary>
	private async Task HaritayiDene(OyunEkranlari ekran, Vector2I boyut, Tema tema)
	{
		var onceki = ekran.AktifEkran;
		long kart = ekran.Oyun.KartKimligi;
		Bas(ekran, "HaritaDugmesi");
		await Bekle();
		Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Harita, "Harita düğmesi haritayı açmalı.");
		var harita = ekran.GetNode<KrallikHaritasi>("%Harita");
		Dogrula(harita.Isaretler.Count == ekran.Mekanlar.Count && ekran.Mekanlar.Count == 6, "Altı mekân haritada olmalı.");
		var alanlar = harita.Isaretler.Values.Select(d => new Rect2(d.Position, d.Size)).ToArray();
		for (int i = 0; i < alanlar.Length; i++)
		{
			Dogrula(harita.CizimAlani.Grow(1).Encloses(alanlar[i]), $"{boyut}: mekân adı haritanın dışına taşıyor.");
			for (int j = i + 1; j < alanlar.Length; j++)
				Dogrula(!alanlar[i].Intersects(alanlar[j]), $"{boyut}: mekân adları üst üste biniyor: {alanlar[i]} / {alanlar[j]}, harita {harita.CizimAlani}.");
		}
		Dogrula(harita.Isaretler[Mekan.TahtSalonuAnahtari].HasFocus(), "Harita taht salonu seçili açılmalı.");
		await YerlesimVeGoruntu(ekran, boyut);
		Tus(Key.Down);
		await Bekle();
		var secili = harita.Secili;
		Dogrula(secili != null && !secili.TahtSalonu && harita.Isaretler[secili.Anahtar].HasFocus() &&
			ekran.GetNode<Label>("%HaritaBilgisi").Text.StartsWith(secili.Ad), "Ok tuşu başka bir mekâna atlamalı.");
		Tus(Key.Enter);
		await Bekle();
		Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Mekan && ekran.AktifMekan == secili, "Enter mekâna girmeli.");
		var diyalog = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
		var salon = ekran.GetNode<SahneGorunumu>("%Sahne");
		int deger = ekran.Oyun.Durum.Gostergeler.Degerler[secili.Gosterge];
		string sozcu = tema.Karakterler.Single(r => r.Anahtar == secili.Sozcu).Unvan;
		Dogrula(diyalog.TamMetin == secili.Durum(deger).Replace("{hitap}", Hitap(ekran)) &&
			ekran.GetNode<Label>("%KonusanAdi").Text.StartsWith(sozcu + " "), "Mekânın sözcüsü durumu anlatmalı.");
		Dogrula(salon.Zemin?.ResourcePath == KarakterGorselleri.MekanCizimi(secili.Anahtar) && salon.Sakinler.Count >= 1 &&
			salon.Sakinler[0].ResourcePath == KarakterGorselleri.PortreYolu(secili.Sozcu) && salon.Hukumdar == null && salon.Ziyaretci == null,
			"Mekânda hükümdar yok, sözcü ve sakinler durmalı.");
		Dogrula(ekran.GetNode<Button>("%HaritayaDon").HasFocus(), "Mekân sözü bitince odak Haritaya Dön'de olmalı.");
		await YerlesimVeGoruntu(ekran, boyut, "-" + secili.Anahtar);
		GeriTusu(ekran);
		Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Harita, "Mekânda geri tuşu haritaya dönmeli.");
		Tus(Key.Escape);
		await Bekle();
		Dogrula(ekran.AktifEkran == onceki && ekran.Oyun.KartKimligi == kart, "Haritada Esc taht salonuna, bekleyen karta dönmeli.");
		Bas(ekran, "HaritaDugmesi");
		harita.Isaretler[Mekan.TahtSalonuAnahtari].EmitSignal(BaseButton.SignalName.Pressed);
		Dogrula(ekran.AktifEkran == onceki && ekran.Oyun.KartKimligi == kart, "Haritada taht salonu bekleyen karta dönmeli.");
		await Bekle();
	}

	/// <summary>Altın set destesinin en uzun metinli ve en uzun seçenekli kartları telefonda kaydırmadan okunur.</summary>
	private async Task GercekDesteyiDene(PackedScene sahne, Vector2I boyut)
	{
		var ekran = sahne.Instantiate<OyunEkranlari>();
		ekran.Animasyonlar = false;
		AddChild(ekran);
		await Bekle();
		Dogrula(ekran.Oyun != null, "Altın set destesi oyunda yüklenmeli.");
		Bas(ekran, "Hatunum");
		await Bekle();
		var tema = Tema.Oku(Oku("res://Veri/tema.json"));
		var kartlar = new YedekDeste(Oku(ekran.DesteYolu), tema.Karakterler.Select(r => r.Anahtar).ToHashSet()).KartlariYukle();
		static int Kelime(string s) => s.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
		var zorlar = kartlar.OrderByDescending(k => Kelime(k.Metin)).Take(3)
			.Concat(kartlar.OrderByDescending(k => Math.Max(k.Sol.Etiket.Length, k.Sag.Etiket.Length) + k.Sol.Etki.Count + k.Sag.Etki.Count).Take(2));
		var diyalog = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
		var etiket = ekran.GetNode<Label>("%DiyalogMetni");
		var salon = ekran.GetNode<SahneGorunumu>("%Sahne");
		var yuva = ekran.GetNode<Control>("%KartYuvasi");
		await Bekle();
		var cizim = salon.CizimAlani;
		float kartBoyu = yuva.Size.Y;
		foreach (var zor in zorlar)
		{
			ekran.KartiGoster(zor);
			await Bekle();
			Dogrula(salon.CizimAlani.IsEqualApprox(cizim) && Mathf.IsEqualApprox(yuva.Size.Y, kartBoyu),
				$"{boyut}: {zor.Id} kart değişince sahne ve kart boyu aynı kalmalı (çizim {cizim} → {salon.CizimAlani}, kart {kartBoyu} → {yuva.Size.Y}, sayfa {diyalog.SayfaSayisi}).");
			for (int sayfa = 0; sayfa < diyalog.SayfaSayisi; sayfa++, diyalog.Ilerle())
				Dogrula(etiket.GetLineCount() <= diyalog.SatirSiniri && diyalog.GetGlobalRect().Grow(1).Encloses(etiket.GetGlobalRect()),
					$"{boyut}: {zor.Id} metni kutuya sığmalı.");
			await Bekle();
			foreach (int yon in new[] { -1, 1 })
			{
				diyalog.SuruklemeyiBaslat(200);
				diyalog.SuruklemeyiGuncelle(200 + yon * diyalog.Esik);
				await Bekle();
				var onizleme = ekran.GetNode<Label>("%KararOnizleme");
				Dogrula(onizleme.Text.Contains(yon > 0 ? "→" : "←") &&
					KartinIcinde(diyalog, onizleme), $"{boyut}: {zor.Id} önizleme kartın içinde kalmalı.");
				diyalog.Iptal();
			}
			if (boyut.X <= 400 && boyut.Y >= 800)
				Dogrula(ekran.GetNode<ScrollContainer>("%Icerik").ScrollVertical == 0 &&
					ekran.GetNode<Control>("%Gostergeler").GetGlobalRect().Position.Y >= 0 && diyalog.GetGlobalRect().End.Y <= boyut.Y,
					$"{boyut}: {zor.Id} dikey telefonda göstergeler, sahne ve konuşma kaydırmadan birlikte görünmeli.");
		}
		if (boyut.X <= 400 && boyut.Y >= 800)
		{
			// Telefonda destedeki her söz ve her sonuç tek sayfadır: söz yarım kalmış gibi görünmez.
			var tumu = kartlar.SelectMany(k => new[] { (k.Id, k.Metin), (k.Id + " sol", k.Sol.Sonuc), (k.Id + " sağ", k.Sag.Sonuc) });
			var metinci = new MetinYerlestirici(tema.Karakterler, Hukumdar.Hatun, new Random(5));
			var tasan = tumu.Where(t => { diyalog.Goster("Vezir Ali", metinci.Yerlestir(t.Item2)); return diyalog.SayfaSayisi > 1; })
				.Select(t => t.Item1).ToArray();
			Dogrula(tasan.Length == 0, $"{boyut}: {tasan.Length} söz telefonda tek sayfaya sığmıyor: {string.Join(", ", tasan.Take(5))}");
			await Bekle();
			Dogrula(salon.CizimAlani.IsEqualApprox(cizim) && Mathf.IsEqualApprox(yuva.Size.Y, kartBoyu),
				$"{boyut}: sözler değişirken sahne ve kart boyu aynı kalmalı.");
		}
		ekran.Free();

		// 2.9: Altın set destesiyle açılış → karar → sonuç → Devam Et → son → yeniden oyun; ağ gerekmez.
		// Geçici kayıt deposuyla: son ekranı telefondaki gibi rekor satırını da gösterir.
		string kayitDizini = ProjectSettings.GlobalizePath("user://test-h1-" + Guid.NewGuid().ToString("N"));
		var oyun = sahne.Instantiate<OyunEkranlari>();
		oyun.Animasyonlar = false;
		oyun.KayitDeposu = new OyunKayitDeposu(kayitDizini);
		AddChild(oyun);
		await Bekle();
		Bas(oyun, "Beyim");
		await Bekle();
		int karar = 0;
		while (oyun.AktifEkran != OyunEkranlari.Ekran.Son && karar < 200)
		{
			Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Oyun, "Sonuçtan sonra yeni konuşan gelmeli.");
			Kaydir(oyun, sag: true);
			karar++;
			Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Sonuc && oyun.GetNode<DiyalogKutusu>("%DiyalogKutusu").TamMetin.Length > 0,
				"Her karardan sonra sonuç anlatılmalı.");
			Bas(oyun, "Devam");
		}
		await Bekle();
		Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Son && oyun.GetNode<Label>("%SonMetni").Text.Length > 0 &&
			oyun.GetNode<Label>("%SonOzet").Text.Contains($"{karar} karar"), "Altın set destesiyle oyun bir sona ulaşmalı.");
		await YerlesimVeGoruntu(oyun, boyut, "-AltinSet");
		Dogrula(oyun.GetNode<Label>("%SonRekor").Visible && oyun.GetNode<Label>("%SonRekor").Text == "Yeni rekor!",
			"İlk tamamlanan hükümdarlık sonda yeni rekor olarak görünmeli.");
		if (boyut.X <= 400 && boyut.Y >= 800)
			foreach (string ad in new[] { "SonBaslik", "Yeniden", "AnaMenuyeDon" })
			{
				var oge = oyun.GetNode<Control>("%" + ad).GetGlobalRect();
				Dogrula(oge.Position.Y >= 0 && oge.End.Y <= boyut.Y, $"{boyut}: sonda {ad} ekranda görünmeli (rekor satırıyla).");
			}
		Bas(oyun, "Yeniden");
		Dogrula(oyun.AktifEkran == OyunEkranlari.Ekran.Acilis, "Tekrar Oyna hükümdar seçimine dönmeli.");
		Bas(oyun, "Beyim");
		Dogrula(oyun.Oyun.Durum.KararSayisi == 0 && oyun.AktifEkran == OyunEkranlari.Ekran.Oyun, "Yeni oyun sıfırdan başlamalı.");
		oyun.Free();
		if (System.IO.Directory.Exists(kayitDizini)) System.IO.Directory.Delete(kayitDizini, true);
	}

	/// <summary>
	/// Gerçek girdi (animasyonlar açık): söz harf harf akar, karta dokunmak tamamlar; söz bitmeden kart kaymaz.
	/// Fare: kısa sürükleme iptal, eşik üstü tek karar. Çoklu dokunma: ikinci parmak karar veremez. Klavye: ok
	/// önizler, ters ok vazgeçer, ok + Enter ya da aynı ok iki kez onaylar; sonuçta odak Devam Et'e geçer.
	/// Yeni konuşan yürüyerek gelir. Telefon: taklit fare + dokunma tek karar verir, iptal edilen dokunma vermez.
	/// </summary>
	private async Task GercekGirdileriDene(OyunEkranlari ekran, Vector2I boyut)
	{
		if (ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc) Bas(ekran, "Devam");
		ekran.Animasyonlar = true;
		var kart = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
		var salon = ekran.GetNode<SahneGorunumu>("%Sahne");
		var onizleme = ekran.GetNode<Label>("%KararOnizleme");
		int sayi = ekran.Oyun.Durum.KararSayisi;
		ekran.KartiGoster(ekran.Oyun.AktifKart);
		await Bekle();
		Dogrula(kart.Yaziyor && !kart.Etkin && ekran.GetNode<Label>("%DiyalogMetni").VisibleCharacters < kart.Sayfalar[0].Length,
			"Söz harf harf akmalı, bitmeden kart kaydırılamamalı.");
		var bas = await Orta(ekran, kart, boyut);
		Fare(true, bas); Fare(false, bas + new Vector2(kart.Esik * 2, 0));
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi, "Söz akarken kart karar vermemeli.");
		for (int i = 0; i < 10 && !kart.Bitti; i++) Tikla(bas);
		Dogrula(kart.Bitti && kart.Etkin && onizleme.Text == OyunEkranlari.BeklemeMetni && ekran.Oyun.Durum.KararSayisi == sayi,
			"Karta dokunmak sözü bitirmeli; sonra kaydırma ipucu görünmeli.");
		var cizim = salon.CizimAlani;

		// Fare: kısa sürükleme iptal, eşik üstü önizleme ve tek karar.
		Fare(true, bas); Girdi(new InputEventMouseMotion { Position = bas + new Vector2(20, 0), GlobalPosition = bas + new Vector2(20, 0), ButtonMask = MouseButtonMask.Left });
		Fare(false, bas + new Vector2(20, 0));
		await Sure(0.3);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi && ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "Kısa sürükleme iptal olmalı.");
		var sag = bas + new Vector2(kart.Esik + 6, 0);
		Fare(true, bas); Girdi(new InputEventMouseMotion { Position = sag, GlobalPosition = sag, ButtonMask = MouseButtonMask.Left });
		Dogrula((onizleme.Text.Contains("▲") || onizleme.Text.Contains("▼")) && onizleme.Text.Contains("Seçmek için bırak"),
			"Eşik geçilince seçenek, etki okları ve bırakma ipucu görünmeli.");
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi, "Önizleme karar uygulamamalı.");
		Dogrula(salon.CizimAlani.IsEqualApprox(cizim), $"{boyut}: kart kaydırılırken sahne büyüyüp küçülmemeli.");
		await YerlesimVeGoruntu(ekran, boyut, "-Onizleme");
		Fare(false, sag);
		Fare(false, bas);
		await Sure(0.35);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 1 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Fare sürüklemesi tek karar vermeli.");

		// Sonuç da akar; Enter tamamlar, odak Devam Et'e geçer, Enter devam eder; yeni konuşan yürüyerek gelir.
		Dogrula(kart.HasFocus() && kart.Yaziyor && !kart.Etkin, "Sonuç da harf harf akmalı, kaydırılmamalı.");
		Dogrula(salon.CizimAlani.IsEqualApprox(cizim), $"{boyut}: sonuç kartında sahne aynı kalmalı.");
		for (int i = 0; i < 10 && !kart.Bitti; i++) Tus(Key.Enter);
		Dogrula(ekran.GetNode<Button>("%Devam").HasFocus(), "Sonuç bitince odak Devam Et'e geçmeli.");
		Tus(Key.Enter);
		await Bekle();
		Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Oyun && salon.Ziyaretci != null &&
			(salon.Yuruyor || salon.ZiyaretciKonumu > SahneGorunumu.ZiyaretciHedefi), "Yeni konuşan yürüyerek gelmeli.");
		await Sure(1.4);
		Dogrula(!salon.Yuruyor && Mathf.IsEqualApprox(salon.ZiyaretciKonumu, SahneGorunumu.ZiyaretciHedefi), "Konuşan tahtın önünde durmalı.");

		// Çoklu dokunma: ikinci parmak karar veremez, ilk parmak tek karar verir.
		kart.Tamamla();
		bas = await Orta(ekran, kart, boyut);
		var sol = bas + new Vector2(-kart.Esik - 6, 0);
		Girdi(new InputEventScreenTouch { Index = 0, Pressed = true, Position = bas });
		Girdi(new InputEventScreenTouch { Index = 1, Pressed = true, Position = bas });
		Girdi(new InputEventScreenDrag { Index = 1, Position = sol });
		Girdi(new InputEventScreenTouch { Index = 1, Pressed = false, Position = sol });
		await Sure(0.35);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 1, "İkinci parmak karar verememeli.");
		Girdi(new InputEventScreenDrag { Index = 0, Position = sol });
		Girdi(new InputEventScreenTouch { Index = 0, Pressed = false, Position = sol });
		await Sure(0.35);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 2, "İlk parmak tek karar vermeli.");
		Bas(ekran, "Devam");
		await Bekle();

		// Klavye: ilk ok önizler, ters ok vazgeçer; ok + Enter ya da aynı ok iki kez onaylar.
		kart.Tamamla();
		kart.GrabFocus();
		Tus(Key.Left);
		await Sure(0.2);
		Dogrula(kart.KlavyeOnizlemesi && onizleme.Text.Contains("Enter") && ekran.Oyun.Durum.KararSayisi == sayi + 2,
			"Sol ok önizleme göstermeli, karar vermemeli.");
		await YerlesimVeGoruntu(ekran, boyut, "-Klavye");
		Tus(Key.Right);
		await Sure(0.25);
		Dogrula(!kart.KlavyeOnizlemesi && onizleme.Text == OyunEkranlari.BeklemeMetni, "Ters ok vazgeçmeli.");
		Tus(Key.Left); Tus(Key.Enter);
		await Sure(0.35);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 3 && ReferenceEquals(ekran.Oyun.SonKarar, ekran.Oyun.AktifKart.Sol),
			"Ok + Enter sol kararı vermeli.");
		Bas(ekran, "Devam");
		await Bekle();
		kart.Tamamla();
		kart.GrabFocus();
		Tus(Key.Right); Tus(Key.Right);
		await Sure(0.35);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 4, "Aynı ok iki kez karar vermeli.");
		Bas(ekran, "Devam");
		await Bekle();

		// G0: telefonda dokunma önce taklit fare olayı, sonra dokunma olayı olarak gelir; kart dokunmayı izler.
		Dogrula((bool)ProjectSettings.GetSetting("input_devices/pointing/emulate_mouse_from_touch", true),
			"Telefonda sayfa kaydırma ve ses kaydırıcısı için dokunmadan fare taklidi açık olmalı.");
		kart.Tamamla();
		bas = await Orta(ekran, kart, boyut);
		sag = bas + new Vector2(kart.Esik + 6, 0);
		void Dokun(bool iptal)
		{
			Fare(true, bas, KaydirmaKontrolu.TaklitAygit); Girdi(new InputEventScreenTouch { Index = 0, Pressed = true, Position = bas });
			Girdi(new InputEventMouseMotion { Device = KaydirmaKontrolu.TaklitAygit, Position = sag, GlobalPosition = sag, ButtonMask = MouseButtonMask.Left });
			Girdi(new InputEventScreenDrag { Index = 0, Position = sag });
			Fare(false, sag, KaydirmaKontrolu.TaklitAygit);
			Girdi(new InputEventScreenTouch { Index = 0, Pressed = false, Position = sag, Canceled = iptal });
		}
		Dokun(iptal: true); // Ör. sürüklerken bildirim perdesi çekildi.
		await Sure(0.35);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 4 && ekran.AktifEkran == OyunEkranlari.Ekran.Oyun, "İptal edilen dokunma karar vermemeli.");
		Dokun(iptal: false);
		await Sure(0.35);
		Dogrula(ekran.Oyun.Durum.KararSayisi == sayi + 5 && ekran.AktifEkran != OyunEkranlari.Ekran.Oyun, "Telefondaki dokunma tek karar vermeli.");
		if (ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc) Bas(ekran, "Devam");
		ekran.Animasyonlar = false;
		await Bekle();
	}

	private async Task YerlesimVeGoruntu(OyunEkranlari ekran, Vector2I boyut, string ek = "")
	{
		await Bekle();
		foreach (var e in Enum.GetValues<OyunEkranlari.Ekran>())
			Dogrula(ekran.GetNode<Control>("%" + OyunEkranlari.PanelAdi(e)).IsVisibleInTree() ==
				(OyunEkranlari.PanelAdi(e) == OyunEkranlari.PanelAdi(ekran.AktifEkran)), "Tek panel görünmeli.");
		var scroll = ekran.GetNode<ScrollContainer>("%Icerik");
		Dogrula(scroll.GetHScrollBar().MaxValue <= scroll.Size.X + 1, $"{boyut}: yatay taşma.");
		var baslik = ekran.GetNode<Label>("%OyunAdi");
		Dogrula(baslik.Visible == (ekran.AktifEkran == OyunEkranlari.Ekran.Acilis),
			"Başlık bandı yalnız hükümdar seçiminde görünmeli; oyunda yer sahneye ve konuşmaya kalır.");
		if (baslik.Visible)
			Dogrula(baslik.GetGlobalRect().Position.Y >= 0 && baslik.GetGlobalRect().End.Y <= scroll.GetGlobalRect().Position.Y + 1,
				$"{boyut}: başlık kaydırma alanı üstünde görünür kalmalı.");
		foreach (string ad in new[] { "Beyim", "Hatunum", "IstatistikCubugu", "Devam", "HaritaDugmesi", "KaydetVeDon",
					"HaritayaDon", "TahtaDon", "Yeniden", "AnaMenuyeDon" })
		{
			var dugme = ekran.GetNode<Button>("%" + ad);
			if (dugme.IsVisibleInTree()) Dogrula(dugme.Size.Y >= 48, $"{ad}: dokunma hedefi küçük.");
		}
		if (ekran.AktifEkran is OyunEkranlari.Ekran.Oyun or OyunEkranlari.Ekran.Sonuc or OyunEkranlari.Ekran.Mekan)
		{
			var kutu = ekran.GetNode<Control>("%DiyalogKutusu").GetGlobalRect();
			Dogrula(kutu.Grow(1).Encloses(ekran.GetNode<Control>("%DiyalogMetni").GetGlobalRect()) &&
				ekran.GetNode<SahneGorunumu>("%Sahne").Size.Y >= 129, $"{boyut}: sahne ve konuşma kutusu yerleşimi bozuk.");
			if (boyut.X <= 400 && boyut.Y >= 800)
				Dogrula(scroll.ScrollVertical == 0 && kutu.End.Y <= boyut.Y, $"{boyut}: telefonda konuşma kutusu kaydırmadan görünmeli.");
			// Sahne tam ekrandır; dikey ekranda çizim yanlarda boşluk bırakmaz, karakterler çubuk ile kart arasında görünür.
			var salon = ekran.GetNode<SahneGorunumu>("%Sahne");
			float cubukAlti = ekran.GetNode<Control>("%IstatistikCubugu").GetGlobalRect().End.Y;
			float kartUstu = ekran.GetNode<Control>("%KartYuvasi").GetGlobalRect().Position.Y;
			Dogrula(salon.GetGlobalRect().IsEqualApprox(new Rect2(Vector2.Zero, boyut)), $"{boyut}: sahne tam ekran olmalı.");
			if (boyut.Y > boyut.X)
				Dogrula(salon.CizimAlani.Position.X <= 0.5f && salon.CizimAlani.End.X >= boyut.X - 0.5f, $"{boyut}: çizim ekranın enini kaplamalı.");
			foreach (var alan in ekran.AktifEkran == OyunEkranlari.Ekran.Mekan || salon.Yuruyor || scroll.ScrollVertical > 0 ? Array.Empty<Rect2>() : new[] { salon.HukumdarAlani, salon.ZiyaretciAlani })
				Dogrula(alan.GetCenter().Y > cubukAlti && alan.GetCenter().Y < kartUstu && alan.Position.X >= -1 && alan.End.X <= boyut.X + 1,
					$"{boyut}: hükümdar ve konuşan çubuk ile kart arasında görünmeli ({alan}, {cubukAlti}–{kartUstu}).");
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
