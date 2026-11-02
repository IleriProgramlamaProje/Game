using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OyunEkranlari : Control
{
	[Export(PropertyHint.File, "*.json")]
	public string DesteYolu { get; set; } = "res://Veri/yedek_deste.json";
	public enum Ekran { Acilis, Oyun, Sonuc, Son }
	public Ekran AktifEkran { get; private set; }
	public OyunYoneticisi Oyun { get; private set; }
	private Tema _tema;
	private MetinYerlestirici _metin;
	private long _gosterilenKart;
	private KaydirmaKontrolu _kart;
	private readonly Dictionary<string, ProgressBar> _cubuklar = new();
	private T Bul<T>(string ad) where T : Node => GetNode<T>("%" + ad);

	public override void _Ready()
	{
		_kart = Bul<KaydirmaKontrolu>("KartAlani");
		_kart.Onizleme += Onizle;
		_kart.Secildi += KararVer;
		Bul<Button>("Baslat").Pressed += Baslat;
		Bul<Button>("SolKarar").Pressed += () => KararVer(false);
		Bul<Button>("SagKarar").Pressed += () => KararVer(true);
		Bul<Button>("Devam").Pressed += () => { if (Oyun?.DevamEt() == true) Guncelle(); };
		Bul<Button>("Yeniden").Pressed += () =>
		{
			if (AktifEkran != Ekran.Son) return;
			Oyun?.AcilisaDon();
			_metin = null;
			_gosterilenKart = 0;
			Bul<OptionButton>("Cinsiyet").Select(0);
			EkraniGoster(Ekran.Acilis);
		};
		Bul<Button>("TekrarYukle").Pressed += Yukle;
		foreach (string anahtar in Gostergeler.Siralama)
			_cubuklar.Add(anahtar, Bul<ProgressBar>(anahtar + "Cubuk"));
		Resized += Yerlesim;
		Yerlesim();
		Yukle();
	}
	private void Yerlesim()
	{
		int kenar = (int)Mathf.Max(16, (Size.X - 640) / 2);
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
			Bul<Button>("Baslat").Disabled = false;
			EkraniGoster(Ekran.Acilis);
		}
		catch (Exception hata) when (hata is FormatException or InvalidOperationException)
		{
			GD.PushWarning(hata.Message);
			Oyun = null;
			Bul<Button>("Baslat").Disabled = true;
			EkraniGoster(Ekran.Acilis);
			Bul<Control>("HataAlani").Show();
			Bul<Label>("HataMetni").Text = "Oyun verisi okunamadı. Tema ve deste dosyalarını kontrol edip yeniden deneyin.";
		}
	}
	private void Baslat()
	{
		if (Oyun == null || AktifEkran != Ekran.Acilis) return;
		var hukumdar = Bul<OptionButton>("Cinsiyet").Selected == 0 ? Hukumdar.Bey : Hukumdar.Hatun;
		_metin = new MetinYerlestirici(_tema.Karakterler, hukumdar, Random.Shared);
		Oyun.Baslat(hukumdar);
		Guncelle();
	}
	private void KararVer(bool sag)
	{
		if (AktifEkran != Ekran.Oyun || Oyun == null) return;
		if (Oyun.KararVer(sag, _gosterilenKart)) Guncelle();
	}
	private void Onizle(float oran)
	{
		if (Oyun?.AktifKart == null || _metin == null) return;
		var metin = Bul<Label>("KartMetni");
		if (AktifEkran != Ekran.Oyun || oran == 0)
		{
			_kart.CustomMinimumSize = new Vector2(0, 200);
			metin.AddThemeFontSizeOverride("font_size", 20);
			metin.Text = _metin.Yerlestir(Oyun.AktifKart.Metin);
			return;
		}
		// Önizleme kartın içinde kalır: yatay telefonda aşağıda kaybolmaz.
		_kart.CustomMinimumSize = new Vector2(0, _kart.Size.Y);
		var karar = oran > 0 ? Oyun.AktifKart.Sag : Oyun.AktifKart.Sol;
		string etkiler = string.Join("   ", Gostergeler.Siralama.Where(karar.Etki.ContainsKey)
			.Select(k => $"{Gostergeler.Ad(k)} {(karar.Etki[k] > 0 ? "▲" : "▼")}"));
		metin.AddThemeFontSizeOverride("font_size", 16);
		metin.Text = $"{_metin.Yerlestir(karar.Etiket)}\n\n{etkiler}\n\n" +
			(Mathf.Abs(oran) >= 1 ? "Karar vermek için bırak." : "Vazgeçmek için ortaya geri getir.");
	}
	private void Guncelle()
	{
		foreach (var (anahtar, cubuk) in _cubuklar)
		{
			int deger = Oyun.Durum.Gostergeler.Degerler[anahtar];
			cubuk.Value = deger;
			Bul<Label>(anahtar + "Deger").Text = $"{Gostergeler.Ad(anahtar)}  {deger}";
		}
		Bul<Label>("KararSayisi").Text = $"Tahtta geçen yıl: {Oyun.Durum.Zaman.TamamlananYil} · Karar: {Oyun.Durum.KararSayisi}";
		switch (Oyun.Asama)
		{
			case Asama.Kart:
				_gosterilenKart = Oyun.KartKimligi;
				Bul<Label>("KartBasligi").Text = _metin.Baslik(Oyun.AktifKart.Konusan);
				Bul<Label>("KartMetni").Text = _metin.Yerlestir(Oyun.AktifKart.Metin);
				Bul<Button>("SolKarar").Text = _metin.Yerlestir(Oyun.AktifKart.Sol.Etiket);
				Bul<Button>("SagKarar").Text = _metin.Yerlestir(Oyun.AktifKart.Sag.Etiket);
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
		_kart.Etkin = ekran == Ekran.Oyun;
		_kart.Iptal();
		Bul<Button>("SolKarar").Disabled = ekran != Ekran.Oyun;
		Bul<Button>("SagKarar").Disabled = ekran != Ekran.Oyun;
		Bul<Button>("Devam").Disabled = ekran != Ekran.Sonuc;
		Bul<Button>("Yeniden").Disabled = ekran != Ekran.Son;
		Bul<ScrollContainer>("Icerik").ScrollVertical = 0;
		string odak = ekran switch { Ekran.Acilis => "Cinsiyet", Ekran.Oyun => "KartAlani", Ekran.Sonuc => "Devam", _ => "Yeniden" };
		Bul<Control>(odak).GrabFocus();
		OdagaKaydir(Bul<Control>(odak), ekran);
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
