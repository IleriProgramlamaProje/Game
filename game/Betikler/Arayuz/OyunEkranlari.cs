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
	private Kart _ilkKart = null;

	public override void _Ready()
	{
		Durum = new OyunDurumu();
		GetNode<Gostergeler>("%Gostergeler").DurumuGoster(Durum);
		_ilkKart = new YedekDeste().KartlariYukle()[0];
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
		GetNode<KartGorunumu>("%KartGorunumu").KartiGoster(_ilkKart, hitap);
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
