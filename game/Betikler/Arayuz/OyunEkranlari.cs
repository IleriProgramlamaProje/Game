using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class OyunEkranlari : Control
{
	[Export(PropertyHint.File, "*.json")]
	public string DesteYolu { get; set; } = "res://Veri/yedek_deste.json";
	public enum Ekran { Acilis, Oyun, Sonuc, Son }
	public Ekran AktifEkran { get; private set; }
	public OyunYoneticisi Oyun { get; private set; }
	public OyunKayitDeposu KayitDeposu { get; set; }
	public OturumKaydi YuklenecekKayit { get; set; }
	internal const string KayitDizini = "user://hukumdarliklar";
	private Guid _oturumId;
	private Tema _tema;
	private MetinYerlestirici _metin;
	private long _gosterilenKart;
	private KartGorunumu _kart;
	private bool _onizlemeAcik;
	private int _olaySayisi;
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

	public override void _Ready()
	{
		if (GetParent() == GetTree().Root)
		{
			KayitDeposu ??= new OyunKayitDeposu(ProjectSettings.GlobalizePath(KayitDizini));
			BaslangicRastgele ??= Random.Shared;
		}
		_kart = Bul<KartGorunumu>("KartAlani");
		_kart.KararOnaylandi += taraf => KararVer(taraf == KartTarafi.Sag);
		_kart.Onizleme += OnizlemeyiGorunurYap;
		Bul<AcilisEkrani>("AcilisPaneli").TahtaCikildi += Baslat;
		Bul<Button>("Devam").Pressed += () => { if (Oyun?.DevamEt() == true) { Kaydet(); Guncelle(); } };
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
		// Telefonda kartın üstündeki "← Sola veya sağa sürükle →" yeterlidir; klavye ipucu anlamsızdır.
		Bul<Control>("Yardim").Visible = !OS.HasFeature("mobile");
		Resized += Yerlesim;
		Bul<MarginContainer>("UstAlan").MinimumSizeChanged += () => Callable.From(Yerlesim).CallDeferred();
		Yerlesim();
		Yukle();
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
			var deste = new YedekDeste(Oku(DesteYolu), _tema.Karakterler.Select(r => r.Anahtar).ToHashSet());
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
	/// Android geri tuşu, ekrandaki düğmelerle aynı işi yapar: hükümdar seçiminde ana menü; oyun ortasında
	/// "Kaydet ve Ana Menüye Dön"; sonda "Ana Menüye Dön". Kayıt yoksa oyun ortasında etkisizdir,
	/// yanlışlıkla yapılan geri hareketi ilerlemeyi kaybettirmez.
	/// </summary>
	public override void _Notification(int what)
	{
		if (what != NotificationWMGoBackRequest) return;
		if (AktifEkran == Ekran.Acilis) MenuyuAc();
		else if (AktifEkran == Ekran.Son) AnaMenuyeDon();
		else if (KayitDeposu != null && Kaydet()) MenuyuAc();
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
	private void Guncelle()
	{
		Bul<Krallik.Arayuz.Gostergeler>("Gostergeler").DurumuGoster(Oyun.Durum);
		Bul<Label>("KararSayisi").Text = $"Tahtta geçen yıl: {Oyun.Durum.Zaman.TamamlananYil} · Karar: {Oyun.Durum.KararSayisi}";
		switch (Oyun.Asama)
		{
			case Asama.Kart:
				_gosterilenKart = Oyun.KartKimligi;
				_kart.KartiGoster(Oyun.AktifKart, _metin);
				EkraniGoster(Ekran.Oyun);
				break;
			case Asama.Sonuc:
				Bul<Label>("SonucMetni").Text = _metin.Yerlestir(Oyun.SonKarar.Sonuc);
				Bul<Label>("SonucEtkileri").Text = string.Join("\n", Gostergeler.Siralama.Where(Oyun.SonKarar.Etki.ContainsKey)
					.Select(k => $"{Gostergeler.Ad(k)} {(Oyun.SonKarar.Etki[k] > 0 ? "+" : "−")}{Math.Abs(Oyun.SonKarar.Etki[k])}"));
				EkraniGoster(Ekran.Sonuc);
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
				EkraniGoster(Ekran.Son);
				break;
		}
	}
	public void EkraniGoster(Ekran ekran)
	{
		if (!Enum.IsDefined(ekran)) throw new ArgumentOutOfRangeException(nameof(ekran));
		AktifEkran = ekran;
		foreach (var e in Enum.GetValues<Ekran>()) Bul<Control>(e + "Paneli").Visible = e == ekran;
		Bul<Control>("Gostergeler").Visible = ekran != Ekran.Acilis;
		// Başlık bandı yalnız hükümdar seçimindedir: dikey telefonda göstergeler ile kart, sonda da iki düğme birlikte sığar.
		Bul<Control>("OyunAdi").Visible = ekran == Ekran.Acilis;
		GetNode<NinePatchRect>("Zemin").Texture = GD.Load<Texture2D>(ekran == Ekran.Son ? SonZemini : OyunZemini);
		_kart.Iptal();
		_kart.Etkin = ekran == Ekran.Oyun;
		Bul<Button>("Devam").Disabled = ekran != Ekran.Sonuc;
		Bul<Button>("Yeniden").Disabled = ekran != Ekran.Son;
		Bul<Button>("AnaMenuyeDon").Disabled = ekran != Ekran.Son;
		Bul<Button>("KaydetVeDon").Visible = KayitDeposu != null && ekran is Ekran.Oyun or Ekran.Sonuc;
		var kaydirma = Bul<ScrollContainer>("Icerik");
		kaydirma.FollowFocus = ekran != Ekran.Oyun;
		kaydirma.ScrollVertical = 0;
		string odak = ekran switch { Ekran.Acilis => "Beyim", Ekran.Oyun => "KartAlani", Ekran.Sonuc => "Devam", _ => "Yeniden" };
		// Odak klavye için hazırdır; çerçevesi yalnız klavyeyle gezinince görünür.
		Bul<Control>(odak).GrabFocus(true);
		if (ekran != Ekran.Oyun) OdagaKaydir(Bul<Control>(odak), ekran);
		else Callable.From(() =>
		{
			// İlk kart odağı sayfayı atlatmasın; sonraki Tab hareketleri kaydırılabilsin.
			if (GodotObject.IsInstanceValid(this) && IsInsideTree() && AktifEkran == Ekran.Oyun)
				kaydirma.FollowFocus = true;
		}).CallDeferred();
	}

	private async void OnizlemeyiGorunurYap(float oran)
	{
		if (Mathf.IsZeroApprox(oran)) { _onizlemeAcik = false; return; }
		if (_onizlemeAcik) return;
		_onizlemeAcik = true;
		long kart = _gosterilenKart;
		var agac = GetTree();
		await ToSignal(agac, SceneTree.SignalName.ProcessFrame);
		await ToSignal(agac, SceneTree.SignalName.ProcessFrame);
		if (GodotObject.IsInstanceValid(this) && IsInsideTree() && AktifEkran == Ekran.Oyun &&
			_onizlemeAcik && kart == _gosterilenKart)
			Bul<ScrollContainer>("Icerik").EnsureControlVisible(Bul<Label>("KararOnizleme"));
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
