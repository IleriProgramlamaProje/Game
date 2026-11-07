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
	private T Bul<T>(string ad) where T : Node => GetNode<T>("%" + ad);

	public override void _Ready()
	{
		if (GetParent() == GetTree().Root)
			KayitDeposu ??= new OyunKayitDeposu(ProjectSettings.GlobalizePath(KayitDizini));
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
		GuvenliAlan.MobilOlcegiAyarla(GetWindow());
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
		Oyun.Baslat(hukumdar);
		Kaydet();
		Guncelle();
	}
	private void KararVer(bool sag)
	{
		if (AktifEkran != Ekran.Oyun || Oyun == null) return;
		if (Oyun.KararVer(sag, _gosterilenKart)) { Kaydet(); Guncelle(); }
	}
	private void AnaMenuyeDon()
	{
		if (AktifEkran == Ekran.Son && Kaydet()) MenuyuAc();
	}
	private void MenuyuAc() => GetTree().ChangeSceneToFile("res://Sahneler/AnaMenu.tscn");

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
				Bul<Label>("SonOzet").Text = $"{Oyun.Durum.Zaman.TamamlananYil} yıl hüküm sürdün, {Oyun.Durum.KararSayisi} karar verdin.";
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
		if (GodotObject.IsInstanceValid(this) && IsInsideTree() && AktifEkran == ekran && odak.HasFocus())
			Bul<ScrollContainer>("Icerik").EnsureControlVisible(odak);
	}
}
