using Godot;
using System;
using System.Collections.Generic;

public partial class OyunEkranlari : Control
{
	public enum Ekran
	{
		Acilis,
		Oyun,
		Sonuc,
		Son
	}

	public Ekran AktifEkran { get; private set; }
	public OyunDurumu Durum { get; private set; }

	private readonly Dictionary<Ekran, (Control Panel, Button Dugme)> _ekranlar = new();
	private IReadOnlyList<Kart> _kartlar;
	private int _kartSirasi;
	private string _hitap;

	public override void _Ready()
	{
		Durum = new OyunDurumu();
		GetNode<Gostergeler>("%Gostergeler").DurumuGoster(Durum);
		_kartlar = new YedekDeste().KartlariYukle();
		GetNode<KartGorunumu>("%KartGorunumu").KararOnaylandi += KarariUygula;
		GetNode<Button>("%DevamDugmesi").Pressed += SonrakiKart;
		GetNode<AcilisEkrani>("%AcilisPaneli").TahtaCikildi += OyunuBaslat;

		foreach (Ekran ekran in Enum.GetValues<Ekran>())
		{
			var panel = GetNode<Control>($"%{ekran}Paneli");
			var dugme = GetNode<Button>($"%{ekran}Dugmesi");
			_ekranlar.Add(ekran, (panel, dugme));
			dugme.Pressed += () => EkraniGoster(ekran);
		}

		EkraniGoster(Ekran.Acilis);
	}

	private void OyunuBaslat(string hitap)
	{
		_hitap = hitap;
		_kartSirasi = 0;
		GetNode<KartGorunumu>("%KartGorunumu").KartiGoster(_kartlar[_kartSirasi], _hitap);
		EkraniGoster(Ekran.Oyun);
	}

	private void KarariUygula(KartTarafi taraf)
	{
		if (AktifEkran != Ekran.Oyun) return;
		var kart = _kartlar[_kartSirasi];
		var secenek = taraf == KartTarafi.Sol ? kart.Sol : kart.Sag;
		foreach (var (ad, etki) in secenek.Etki)
		{
			var gosterge = ad switch { "yonetim" => Gosterge.Yonetim, "halk" => Gosterge.Halk, "para" => Gosterge.Para, "ordu" => Gosterge.Ordu, _ => throw new InvalidOperationException(ad) };
			Durum.EtkiUygula(gosterge, etki);
		}
		GetNode<Label>("%SonucMetni").Text = secenek.Sonuc;
		EkraniGoster(Ekran.Sonuc);
	}

	private void SonrakiKart()
	{
		if (AktifEkran != Ekran.Sonuc) return;
		_kartSirasi = (_kartSirasi + 1) % _kartlar.Count;
		GetNode<KartGorunumu>("%KartGorunumu").KartiGoster(_kartlar[_kartSirasi], _hitap);
		EkraniGoster(Ekran.Oyun);
	}

	// Bu gezinti yalnızca 1.7 ekran iskeletini incelemek içindir; oyun kuralı uygulamaz.
	public void EkraniGoster(Ekran ekran)
	{
		if (!_ekranlar.ContainsKey(ekran))
			throw new ArgumentOutOfRangeException(nameof(ekran));

		foreach (var (anahtar, parcalar) in _ekranlar)
		{
			bool secili = anahtar == ekran;
			parcalar.Panel.Visible = secili;
			parcalar.Dugme.SetPressedNoSignal(secili);
		}

		AktifEkran = ekran;
	}
}
