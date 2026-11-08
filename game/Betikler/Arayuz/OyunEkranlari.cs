using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Oyun sahnesi. Hükümdar seçildikten sonra taht salonu bütün ekranı kaplar: hükümdar tahtta oturur, kartın
/// konuşanı yürüyerek gelir ve sözü alttaki kartta harf harf akar; söz bitince kart sağa ya da sola kaydırılarak
/// karar verilir. Sonuç aynı kartta anlatılır. Üstteki istatistik çubuğu dört göstergeyi özetler, dokununca
/// ayrıntılı çubuklar açılır. Harita ile üstten bakışlı haritaya çıkılır; diğer mekânlarda o mekânın sözcüsü
/// krallığın o alandaki durumunu anlatır. Kartlar yalnız taht salonunda gelir.
/// </summary>
public partial class OyunEkranlari : Control
{
	[Export(PropertyHint.File, "*.json")]
	public string DesteYolu { get; set; } = "res://Veri/yedek_deste.json";
	public const string MekanlarYolu = "res://Veri/mekanlar.json";
	public enum Ekran { Acilis, Oyun, Sonuc, Son, Harita, Mekan }
	public Ekran AktifEkran { get; private set; }
	public OyunYoneticisi Oyun { get; private set; }
	public OyunKayitDeposu KayitDeposu { get; set; }
	public OturumKaydi YuklenecekKayit { get; set; }
	/// <summary>Kapalıyken metin ve yürüyüş beklemeden tamamlanır (testler, ekran görüntüleri).</summary>
	public bool Animasyonlar
	{
		get => _animasyonlar;
		set
		{
			_animasyonlar = value;
			if (_diyalog != null) _diyalog.Aninda = _sahne.Aninda = !value;
		}
	}
	private bool _animasyonlar = true;
	/// <summary>Mekân ekranında gösterilen mekân.</summary>
	public Mekan AktifMekan { get; private set; }
	public IReadOnlyList<Mekan> Mekanlar => _mekanlar;
	internal const string KayitDizini = "user://hukumdarliklar";
	private Guid _oturumId;
	private Tema _tema;
	private MetinYerlestirici _metin;
	private long _gosterilenKart;
	private int _olaySayisi;
	private Mekan[] _mekanlar = Array.Empty<Mekan>();
	private DiyalogKutusu _diyalog;
	private SahneGorunumu _sahne;
	private KrallikHaritasi _harita;
	private string _sonMekan = Mekan.TahtSalonuAnahtari;
	/// <summary>
	/// Atanırsa her yeni hükümdarlık destenin rastgele bir olayından başlar. Gerçek oyunda
	/// <see cref="Random.Shared"/>; testlerde boş kalır ve deste baştan, belirli sırayla açılır.
	/// </summary>
	public Random BaslangicRastgele { get; set; }
	private T Bul<T>(string ad) where T : Node => GetNode<T>("%" + ad);

	/// <summary>Oyun boyunca lacivert fırçalı zemin; hükümdarlık bitince bordo kadife.</summary>
	public const string OyunZemini = "res://Assets/ui/backgrounds/bg_navy_brushed.png";
	public const string SonZemini = "res://Assets/ui/backgrounds/bg_crimson_cloth.png";
	/// <summary>Tek sebepli sonda o göstergenin simgesi görünür; birden çok sebepte taç kalır.</summary>
	private static readonly Dictionary<string, string> SonSimgeleri = new()
	{
		["yonetim"] = "res://Assets/ui/icons/icon_02.png",
		["halk"] = "res://Assets/ui/icons/icon_03.png",
		["para"] = "res://Assets/ui/icons/icon_04.png",
		["ordu"] = "res://Assets/ui/icons/icon_05.png",
	};
	public const string GenelSonSimgesi = "res://Assets/ui/icons/icon_01.png";
	internal static string SonSimgesi(IReadOnlyList<string> sonAnahtarlari) =>
		sonAnahtarlari.Count == 1 && SonSimgeleri.TryGetValue(sonAnahtarlari[0].Split('_')[0], out var yol) ? yol : GenelSonSimgesi;
	/// <summary>Mekânın tonu: çöküşte soluk, olağanda doğal, taşkınlıkta sıcak.</summary>
	private static readonly Color[] MekanTonlari =
	{
		new(0.6f, 0.6f, 0.68f), new(0.82f, 0.8f, 0.84f), Colors.White, Colors.White, new(1f, 0.88f, 0.72f)
	};

	/// <summary>Ekranın paneli: taht salonu ve mekânlar aynı sahne panelini kullanır.</summary>
	public const string BeklemeMetni = "← Sola veya sağa kaydır →";
	// Parşömen üzerindeki mürekkep ve eşik geçildiğinde vurgulanan bordo.
	private static readonly Color Murekkep = new(0.17f, 0.105f, 0.055f);
	private static readonly Color Vurgu = new(0.5f, 0.11f, 0.06f);
	private bool _istatistikAcik;
	/// <summary>İstatistik çubuğu açık mı (ayrıntılı gösterge çubukları görünür).</summary>
	public bool IstatistiklerAcik => _istatistikAcik;

	public static string PanelAdi(Ekran ekran) => ekran switch
	{
		Ekran.Oyun or Ekran.Sonuc or Ekran.Mekan => "SahnePaneli",
		_ => ekran + "Paneli"
	};
	private static readonly string[] Paneller = { "AcilisPaneli", "SahnePaneli", "HaritaPaneli", "SonPaneli" };

	public override void _Ready()
	{
		if (GetParent() == GetTree().Root)
		{
			KayitDeposu ??= new OyunKayitDeposu(ProjectSettings.GlobalizePath(KayitDizini));
			BaslangicRastgele ??= Random.Shared;
		}
		_diyalog = Bul<DiyalogKutusu>("DiyalogKutusu");
		_sahne = Bul<SahneGorunumu>("Sahne");
		_harita = Bul<KrallikHaritasi>("Harita");
		_diyalog.Aninda = _sahne.Aninda = !Animasyonlar;
		_diyalog.MetinBitti += MetinBitti;
		_diyalog.KararOnaylandi += taraf => KararVer(taraf == KartTarafi.Sag);
		_diyalog.Onizleme += KarariOnizle;
		Bul<Button>("IstatistikCubugu").Toggled += acik => { _istatistikAcik = acik; IstatistikleriGoster(); };
		_sahne.Takip(SahneAlani);
		Bul<AcilisEkrani>("AcilisPaneli").TahtaCikildi += Baslat;
		Bul<Button>("Devam").Pressed += () => { if (AktifEkran == Ekran.Sonuc && Oyun?.DevamEt() == true) { Kaydet(); Guncelle(); } };
		Bul<Button>("Yeniden").Pressed += () =>
		{
			if (AktifEkran != Ekran.Son) return;
			Oyun?.AcilisaDon();
			_metin = null;
			_gosterilenKart = 0;
			EkraniGoster(Ekran.Acilis);
		};
		Bul<Button>("AnaMenuyeDon").Pressed += AnaMenuyeDon;
		Bul<Button>("KaydetVeDon").Pressed += () => { if (Kaydet()) MenuyuAc(); };
		Bul<Button>("TekrarYukle").Pressed += Yukle;
		Bul<Button>("HaritaDugmesi").Pressed += HaritayiAc;
		Bul<Button>("HaritayaDon").Pressed += HaritayiAc;
		Bul<Button>("TahtaDon").Pressed += TahtaDon;
		_harita.MekanSecildi += MekanaGir;
		_harita.MekanOdaklandi += m => Bul<Label>("HaritaBilgisi").Text = HaritaBilgisi(m);
		Resized += Yerlesim;
		Bul<MarginContainer>("UstAlan").MinimumSizeChanged += () => Callable.From(Yerlesim).CallDeferred();
		Yerlesim();
		Yukle();
	}
	/// <summary>
	/// Sahnenin göründüğü alan: istatistik çubuğunun altından kartın üstüne kadar. Yalnız ekran boyuna ve
	/// kartın (sabit) boyuna bağlıdır; söz, kaydırma, sayfa ya da açılan istatistikler sahneyi büyütüp küçültmez.
	/// </summary>
	private Rect2 SahneAlani()
	{
		var icerik = Bul<ScrollContainer>("Icerik").GetGlobalRect();
		var bosluk = Bul<MarginContainer>("Bosluk");
		float ust = icerik.Position.Y + bosluk.GetThemeConstant("margin_top") + Bul<Control>("IstatistikCubugu").Size.Y + 8;
		float alt = icerik.End.Y - bosluk.GetThemeConstant("margin_bottom") - Bul<Control>("KartYuvasi").Size.Y - 8;
		return new Rect2(icerik.Position.X, ust, icerik.Size.X, Mathf.Max(80, alt - ust));
	}
	private void Yerlesim()
	{
		if (!GodotObject.IsInstanceValid(this) || !IsInsideTree()) return;
		var guvenli = GuvenliAlan.Kenarlar(GetWindow());
		int kenar = (int)Mathf.Max(16, (Size.X - guvenli.X - guvenli.Z - 640) / 2);
		var ust = Bul<MarginContainer>("UstAlan");
		ust.OffsetLeft = guvenli.X;
		ust.OffsetRight = -guvenli.Z;
		ust.OffsetTop = guvenli.Y + 12;
		ust.OffsetBottom = ust.OffsetTop + ust.GetCombinedMinimumSize().Y;
		ust.AddThemeConstantOverride("margin_left", kenar);
		ust.AddThemeConstantOverride("margin_right", kenar);
		var icerik = Bul<ScrollContainer>("Icerik");
		icerik.OffsetLeft = guvenli.X;
		icerik.OffsetRight = -guvenli.Z;
		icerik.OffsetTop = ust.OffsetBottom;
		icerik.OffsetBottom = -guvenli.W;
		var bosluk = Bul<MarginContainer>("Bosluk");
		bosluk.AddThemeConstantOverride("margin_left", kenar);
		bosluk.AddThemeConstantOverride("margin_right", kenar);
	}
	private static string Oku(string yol)
	{
		using var dosya = Godot.FileAccess.Open(yol, Godot.FileAccess.ModeFlags.Read);
		if (dosya == null) throw new InvalidOperationException($"Veri dosyası açılamadı: {yol} ({Godot.FileAccess.GetOpenError()})");
		return dosya.GetAsText();
	}
	public void Yukle()
	{
		try
		{
			_tema = Tema.Oku(Oku("res://Veri/tema.json"));
			var roller = _tema.Karakterler.Select(r => r.Anahtar).ToHashSet();
			var deste = new YedekDeste(Oku(DesteYolu), roller);
			_mekanlar = MekanListesi.Oku(Oku(MekanlarYolu), roller);
			_harita.Kur(_mekanlar);
			Oyun = new OyunYoneticisi(deste, _tema.Yil.KacKarttaBirYil);
			_olaySayisi = deste.OlayGrubuSayisi;
			Bul<Label>("AcilisMetni").Text = _tema.Acilis;
			Bul<Control>("HataAlani").Hide();
			Bul<AcilisEkrani>("AcilisPaneli").EtkinligiAyarla(true);
			EkraniGoster(Ekran.Acilis);
			KayittanBaslat();
		}
		catch (Exception hata) when (hata is FormatException or InvalidOperationException)
		{
			GD.PushWarning(hata.Message);
			Oyun = null;
			Bul<AcilisEkrani>("AcilisPaneli").EtkinligiAyarla(false);
			EkraniGoster(Ekran.Acilis);
			Bul<Control>("HataAlani").Show();
			Bul<Label>("HataMetni").Text = "Oyun verisi okunamadı. Tema ve deste dosyalarını kontrol edip yeniden deneyin.";
		}
	}
	private void Baslat(Hukumdar hukumdar)
	{
		if (Oyun == null || AktifEkran != Ekran.Acilis) return;
		_metin = new MetinYerlestirici(_tema.Karakterler, hukumdar, Random.Shared);
		_oturumId = Guid.NewGuid();
		Oyun.Baslat(hukumdar, BaslangicRastgele?.Next(_olaySayisi) ?? 0);
		Kaydet();
		Guncelle();
	}
	private void KararVer(bool sag)
	{
		if (AktifEkran != Ekran.Oyun || Oyun == null) return;
		if (Oyun.KararVer(sag, _gosterilenKart)) { Kaydet(); Guncelle(); }
	}
	public const string AnaMenuYolu = "res://Sahneler/AnaMenu.tscn";
	/// <summary>Atanırsa sahne değiştirmek yerine çağrılır; testler için.</summary>
	public Action<string> SahneGecisi { get; set; }
	private void AnaMenuyeDon()
	{
		if (AktifEkran == Ekran.Son && Kaydet()) MenuyuAc();
	}
	private void MenuyuAc()
	{
		if (SahneGecisi != null) SahneGecisi(AnaMenuYolu);
		else GetTree().ChangeSceneToFile(AnaMenuYolu);
	}
	/// <summary>
	/// Android geri tuşu, ekrandaki düğmelerle aynı işi yapar: hükümdar seçiminde ana menü; mekânda haritaya,
	/// haritada taht salonuna dönüş; taht salonunda "Kaydet ve Çık"; sonda "Ana Menüye Dön". Kayıt yoksa taht
	/// salonunda etkisizdir, yanlışlıkla yapılan geri hareketi ilerlemeyi kaybettirmez.
	/// </summary>
	public override void _Notification(int what)
	{
		if (what != NotificationWMGoBackRequest) return;
		Geri();
	}
	private void Geri()
	{
		switch (AktifEkran)
		{
			case Ekran.Acilis: MenuyuAc(); break;
			case Ekran.Son: AnaMenuyeDon(); break;
			case Ekran.Mekan: HaritayiAc(); break;
			case Ekran.Harita: TahtaDon(); break;
			default: if (KayitDeposu != null && Kaydet()) MenuyuAc(); break;
		}
	}
	public override void _UnhandledInput(InputEvent olay)
	{
		// Masaüstünde Esc: haritadan ve mekândan geri döner (taht salonunda oyunu bırakmaz).
		if (olay.IsActionPressed("ui_cancel") && AktifEkran is Ekran.Harita or Ekran.Mekan)
		{
			Geri();
			GetViewport().SetInputAsHandled();
		}
	}

	private void KayittanBaslat()
	{
		if (YuklenecekKayit is not { } kayit) return;
		YuklenecekKayit = null;
		try
		{
			kayit.Dogrula(kayit.Id, _tema);
			_metin = new MetinYerlestirici(_tema.Karakterler, kayit.Oyun.Hukumdar, kayit.Karakterler);
			Oyun.GeriYukle(kayit.Oyun, _tema.Karakterler.Select(r => r.Anahtar).ToHashSet());
			_oturumId = kayit.Id;
			Guncelle();
		}
		catch (FormatException hata)
		{
			GD.PushWarning(hata.Message);
			KayitMesaji("Bu kayıt yüklenemedi. Yeni bir hükümdarlık başlatabilirsin.");
		}
	}

	private bool Kaydet()
	{
		if (KayitDeposu == null || Oyun?.Durum == null || _oturumId == Guid.Empty) return true;
		try
		{
			KayitDeposu.Kaydet(new OturumKaydi
			{
				Surum = OturumKaydi.GuncelSurum, Id = _oturumId, KayitZamani = DateTimeOffset.UtcNow,
				Oyun = Oyun.AnligiAl(), Karakterler = _metin.KarakterKaydi()
			});
			KayitMesaji("");
			return true;
		}
		catch (Exception hata) when (hata is IOException or UnauthorizedAccessException)
		{
			GD.PushWarning(hata.Message);
			KayitMesaji("İlerleme kaydedilemedi. Depolama alanını kontrol edip tekrar dene.");
			return false;
		}
	}

	/// <summary>Son ekranında bu hükümdarlığı kayıtlı rekorla karşılaştırır (3.8). Kayıt yoksa gizlidir.</summary>
	private void RekoruGoster()
	{
		var etiket = Bul<Label>("SonRekor");
		etiket.Visible = false;
		if (KayitDeposu == null || _oturumId == Guid.Empty) return;
		try
		{
			var rekor = new HukumdarlikGecmisi(KayitDeposu.Listele(_tema).Kayitlar).Rekor;
			if (rekor == null) return;
			// Tek satır: dar telefonda son ekranının düğmeleri aşağı itilmesin.
			etiket.Text = rekor.Id == _oturumId
				? "Yeni rekor!"
				: $"Rekorun: {rekor.Oyun.Yil} yıl · {rekor.Oyun.KararSayisi} karar";
			etiket.Visible = true;
		}
		catch (Exception hata) when (hata is IOException or UnauthorizedAccessException or FormatException)
		{
			GD.PushWarning(hata.Message);
		}
	}

	private void KayitMesaji(string mesaj)
	{
		var etiket = Bul<Label>("KayitMesaji");
		etiket.Text = mesaj;
		etiket.Visible = mesaj.Length > 0;
	}

	private static Texture2D Doku(string yol) => yol == null ? null : GD.Load<Texture2D>(yol);
	private Texture2D Portre(string rol) => Doku(KarakterGorselleri.PortreYolu(rol, _metin.Kimlik(rol).PortreAnahtari));

	/// <summary>
	/// Kart kaydırılırken o yöndeki seçeneğin etiketi ve etkilediği göstergeler (yalnız yön okuyla) görünür;
	/// eşik geçilince bırakma ipucu bordoya döner. Kart ortadaysa kaydırma ipucu yazar.
	/// </summary>
	private void KarariOnizle(float oran)
	{
		var etiket = Bul<Label>("KararOnizleme");
		if (Mathf.IsZeroApprox(oran) || AktifEkran != Ekran.Oyun || Oyun?.AktifKart == null)
		{
			etiket.Text = BeklemeMetni;
			etiket.AddThemeColorOverride("font_color", Murekkep);
			return;
		}
		var karar = oran > 0 ? Oyun.AktifKart.Sag : Oyun.AktifKart.Sol;
		string etkiler = string.Join("   ", Gostergeler.Siralama.Where(karar.Etki.ContainsKey)
			.Select(k => $"{Gostergeler.Ad(k)} {(karar.Etki[k] > 0 ? "▲" : "▼")}"));
		bool hazir = Mathf.Abs(oran) >= 1;
		string ipucu = _diyalog.KlavyeOnizlemesi ? "Enter: onayla · ters ok: vazgeç"
			: hazir ? "Seçmek için bırak" : "Seçmek için biraz daha kaydır";
		string ad = _metin.Yerlestir(karar.Etiket);
		etiket.AddThemeColorOverride("font_color", hazir ? Vurgu : Murekkep);
		etiket.Text = (oran > 0 ? ad + "  →" : "←  " + ad) + "\n" + etkiler + "\n" + ipucu;
	}

	/// <summary>Kapalı çubukta dört göstergenin sayısı ve yıl; uçtaki gösterge kırmızı yazar.</summary>
	private void OzetiGuncelle()
	{
		if (Oyun?.Durum == null) return;
		foreach (var (anahtar, deger) in Oyun.Durum.Gostergeler.Degerler)
		{
			var etiket = Bul<Label>("OzetDeger_" + anahtar);
			etiket.Text = deger.ToString();
			etiket.AddThemeColorOverride("font_color", Krallik.Mekan.Tehlikede(deger) ? new Color(1, 0.45f, 0.4f) : new Color(1, 0.92f, 0.72f));
		}
		Bul<Label>("OzetYil").Text = $"Yıl {Oyun.Durum.Zaman.TamamlananYil}";
	}

	/// <summary>Oyun içinde çubuk görünür; ayrıntılı göstergeler çubuk açıksa (sonda her zaman) görünür.</summary>
	private void IstatistikleriGoster()
	{
		bool oyunda = AktifEkran is Ekran.Oyun or Ekran.Sonuc or Ekran.Mekan or Ekran.Harita;
		var cubuk = Bul<Button>("IstatistikCubugu");
		cubuk.Visible = oyunda;
		cubuk.SetPressedNoSignal(_istatistikAcik);
		Bul<Label>("OzetOk").Text = _istatistikAcik ? "▴" : "▾";
		// Ayrıntılı göstergeler yarı saydam kutuda sahnenin üstüne açılır; sahne yerinden oynamaz.
		Bul<Control>("GostergeKutusu").Visible = AktifEkran == Ekran.Son || (oyunda && _istatistikAcik);
	}

	/// <summary>Oyunun aşamasını taht salonunda (ya da sonda) gösterir.</summary>
	private void Guncelle()
	{
		Bul<Krallik.Arayuz.Gostergeler>("Gostergeler").DurumuGoster(Oyun.Durum);
		Bul<Label>("KararSayisi").Text = $"Tahtta geçen yıl: {Oyun.Durum.Zaman.TamamlananYil} · Karar: {Oyun.Durum.KararSayisi}";
		OzetiGuncelle();
		switch (Oyun.Asama)
		{
			case Asama.Kart:
				_gosterilenKart = Oyun.KartKimligi;
				KartiGoster(Oyun.AktifKart);
				break;
			case Asama.Sonuc:
				TahtSalonunuKur();
				_sahne.ZiyaretciGelsin(Portre(Oyun.AktifKart.Konusan));
				Bul<Label>("EylemBilgisi").Text = string.Join("   ", Gostergeler.Siralama.Where(Oyun.SonKarar.Etki.ContainsKey)
					.Select(k => $"{Gostergeler.Ad(k)} {(Oyun.SonKarar.Etki[k] > 0 ? "+" : "−")}{Math.Abs(Oyun.SonKarar.Etki[k])}"));
				EkraniGoster(Ekran.Sonuc);
				_diyalog.Goster("Kararın ardından", _metin.Yerlestir(Oyun.SonKarar.Sonuc));
				break;
			case Asama.Son:
				bool tekSon = Oyun.SonAnahtarlari.Count == 1;
				Bul<Label>("SonBaslik").Text = tekSon ? _tema.Sonlar[Oyun.SonAnahtarlari[0]].Baslik : "Hükümdarlığın sonu";
				Bul<Label>("SonMetni").Text = string.Join("\n\n", Oyun.SonAnahtarlari.Select(k =>
				{
					var son = _tema.Sonlar[k];
					return (tekSon ? "" : son.Baslik + "\n") + _metin.Yerlestir(son.Metin);
				}));
				Bul<TextureRect>("SonGorseli").Texture = GD.Load<Texture2D>(SonSimgesi(Oyun.SonAnahtarlari));
				Bul<Label>("SonOzet").Text = $"{Oyun.Durum.Zaman.TamamlananYil} yıl hüküm sürdün, {Oyun.Durum.KararSayisi} karar verdin.";
				RekoruGoster();
				_sahne.ZiyaretciGitsin();
				EkraniGoster(Ekran.Son);
				break;
		}
	}

	/// <summary>Kartın konuşanı taht salonuna gelir, sözü kutuda akar; seçenekler metin bitince açılır.</summary>
	internal void KartiGoster(Kart kart)
	{
		TahtSalonunuKur();
		_sahne.ZiyaretciGelsin(Portre(kart.Konusan));
		EkraniGoster(Ekran.Oyun);
		KarariOnizle(0);
		_diyalog.Goster(_metin.Baslik(kart.Konusan), _metin.Yerlestir(kart.Metin));
	}

	private void TahtSalonunuKur() =>
		_sahne.TahtSalonu(Doku(KarakterGorselleri.MekanCizimi(Mekan.TahtSalonuAnahtari)), Doku(KarakterGorselleri.HukumdarYolu(Oyun.Durum.Cinsiyet)));

	private bool OyunSuruyor => Oyun?.Durum != null && Oyun.Asama is Asama.Kart or Asama.Sonuc;

	/// <summary>Üstten bakışlı harita; son gezilen mekân seçili gelir. Yalnız hükümdarlık sürerken açılır.</summary>
	public void HaritayiAc()
	{
		if (!OyunSuruyor) return;
		_harita.DurumuGoster(m => !m.TahtSalonu && Krallik.Mekan.Tehlikede(Oyun.Durum.Gostergeler.Degerler[m.Gosterge]));
		EkraniGoster(Ekran.Harita);
		var secili = _mekanlar.FirstOrDefault(m => m.Anahtar == _sonMekan) ?? _mekanlar[0];
		Bul<Label>("HaritaBilgisi").Text = HaritaBilgisi(secili);
		_harita.Odakla(secili.Anahtar);
	}

	private string HaritaBilgisi(Mekan mekan)
	{
		if (mekan.TahtSalonu) return $"{mekan.Ad} · kararlar burada verilir";
		int deger = Oyun?.Durum?.Gostergeler.Degerler[mekan.Gosterge] ?? 0;
		return $"{mekan.Ad} · {Gostergeler.Ad(mekan.Gosterge)} {deger}" + (Krallik.Mekan.Tehlikede(deger) ? " · durum kritik" : "");
	}

	/// <summary>Haritadan bir mekâna girer; taht salonu seçilirse bekleyen karta dönülür.</summary>
	public void MekanaGir(Mekan mekan)
	{
		if (!OyunSuruyor || mekan == null) return;
		_sonMekan = mekan.Anahtar;
		if (mekan.TahtSalonu) { TahtaDon(); return; }
		AktifMekan = mekan;
		int deger = Oyun.Durum.Gostergeler.Degerler[mekan.Gosterge];
		var sakinler = KarakterGorselleri.Sakinler(mekan.Anahtar, mekan.Sozcu).Select(Portre).ToArray();
		_sahne.Mekan(Doku(KarakterGorselleri.MekanCizimi(mekan.Anahtar)), sakinler, MekanTonlari[Krallik.Mekan.Aralik(deger)]);
		Bul<Label>("EylemBilgisi").Text = $"{Gostergeler.Ad(mekan.Gosterge)}: {deger}";
		EkraniGoster(Ekran.Mekan);
		_diyalog.Goster(_metin.Baslik(mekan.Sozcu), _metin.Yerlestir(mekan.Durum(deger)));
	}

	/// <summary>Haritadan taht salonuna: bekleyen kart ya da sonuç yeniden anlatılır.</summary>
	public void TahtaDon()
	{
		if (!OyunSuruyor) return;
		_sonMekan = Mekan.TahtSalonuAnahtari;
		AktifMekan = null;
		Guncelle();
	}

	/// <summary>Metin bitince klavye odağı ilgili düğmeye geçer; seçimde odak kutuda kalır, ok tuşları seçer.</summary>
	private void MetinBitti()
	{
		var dugme = AktifEkran switch { Ekran.Sonuc => Bul<Button>("Devam"), Ekran.Mekan => Bul<Button>("HaritayaDon"), _ => null };
		if (dugme == null) return;
		dugme.GrabFocus(true);
		// Düğme bu karede görünür oldu; yerleşim oturunca kısa ekranda görünür alana kaydırılır.
		OdagaKaydir(dugme, AktifEkran);
	}

	public void EkraniGoster(Ekran ekran)
	{
		if (!Enum.IsDefined(ekran)) throw new ArgumentOutOfRangeException(nameof(ekran));
		AktifEkran = ekran;
		string panel = PanelAdi(ekran);
		foreach (string ad in Paneller) Bul<Control>(ad).Visible = ad == panel;
		IstatistikleriGoster();
		// Başlık bandı yalnız hükümdar seçimindedir: dikey telefonda göstergeler, sahne ve konuşma birlikte sığar.
		Bul<Control>("OyunAdi").Visible = ekran == Ekran.Acilis;
		GetNode<NinePatchRect>("Zemin").Texture = GD.Load<Texture2D>(ekran == Ekran.Son ? SonZemini : OyunZemini);
		bool taht = ekran is Ekran.Oyun or Ekran.Sonuc;
		_sahne.Visible = taht || ekran == Ekran.Mekan;
		Bul<Control>("AracCubugu").Visible = taht;
		Bul<Button>("KaydetVeDon").Visible = KayitDeposu != null;
		// Yalnız karar kartı kaydırılır; başka ekrana geçerken süren sürükleme bırakılır.
		_diyalog.Kaydirilabilir = ekran == Ekran.Oyun;
		if (ekran != Ekran.Oyun && _diyalog.Etkin) { _diyalog.Etkin = false; _diyalog.Iptal(); }
		Bul<Control>("KararOnizleme").Visible = ekran == Ekran.Oyun;
		Bul<Control>("EylemBilgisi").Visible = ekran is Ekran.Sonuc or Ekran.Mekan;
		Bul<Control>("Devam").Visible = ekran == Ekran.Sonuc;
		Bul<Control>("HaritayaDon").Visible = ekran == Ekran.Mekan;
		Bul<Button>("Yeniden").Disabled = ekran != Ekran.Son;
		Bul<Button>("AnaMenuyeDon").Disabled = ekran != Ekran.Son;
		var kaydirma = Bul<ScrollContainer>("Icerik");
		kaydirma.ScrollVertical = 0;
		Control odak = ekran switch
		{
			Ekran.Acilis => Bul<Control>("Beyim"),
			Ekran.Son => Bul<Control>("Yeniden"),
			Ekran.Harita => null,
			_ => _diyalog,
		};
		// Odak klavye için hazırdır; çerçevesi yalnız klavyeyle gezinince görünür.
		odak?.GrabFocus(true);
		if (odak != null && ekran is Ekran.Acilis or Ekran.Son) OdagaKaydir(odak, ekran);
	}

	private async void OdagaKaydir(Control odak, Ekran ekran)
	{
		// Yeni metnin ve panelin boyutları yerleşince odağı görünür alana al.
		var agac = GetTree();
		await ToSignal(agac, SceneTree.SignalName.ProcessFrame);
		await ToSignal(agac, SceneTree.SignalName.ProcessFrame);
		if (!GodotObject.IsInstanceValid(this) || !IsInsideTree() || AktifEkran != ekran || !odak.HasFocus()) return;
		var kaydirma = Bul<ScrollContainer>("Icerik");
		// Sonda iki düğme de görünsün: önce alttaki "Ana Menüye Dön", sonra odaklı düğme görünür alana alınır.
		if (ekran == Ekran.Son) kaydirma.EnsureControlVisible(Bul<Control>("AnaMenuyeDon"));
		kaydirma.EnsureControlVisible(odak);
	}
}
