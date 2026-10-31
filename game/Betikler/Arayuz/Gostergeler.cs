using Godot;
using System;
using System.Collections.Generic;

public partial class Gostergeler : HBoxContainer
{
	private readonly Dictionary<Gosterge, ProgressBar> _cubuklar = new();
	private readonly Dictionary<Gosterge, Label> _etiketler = new();
	private OyunDurumu _durum;

	public override void _Ready()
	{
		foreach (Gosterge gosterge in Enum.GetValues<Gosterge>())
		{
			_cubuklar[gosterge] = GetNode<ProgressBar>($"%{gosterge}Cubugu");
			_etiketler[gosterge] = GetNode<Label>($"%{gosterge}Etiketi");
		}
	}

	public void DurumuGoster(OyunDurumu durum)
	{
		if (_durum != null)
			_durum.GostergeDegisti -= GostergeyiGuncelle;

		_durum = durum;
		_durum.GostergeDegisti += GostergeyiGuncelle;

		foreach (Gosterge gosterge in Enum.GetValues<Gosterge>())
			GostergeyiGuncelle(gosterge, durum.DegeriniAl(gosterge));
	}

	private void GostergeyiGuncelle(Gosterge gosterge, int deger)
	{
		_cubuklar[gosterge].Value = deger;
		_etiketler[gosterge].Text = $"{GostergeAdi(gosterge)}: {deger}";
	}

	private static string GostergeAdi(Gosterge gosterge) => gosterge switch
	{
		Gosterge.Yonetim => "Yönetim",
		Gosterge.Halk => "Halk",
		Gosterge.Para => "Para",
		Gosterge.Ordu => "Ordu",
		_ => throw new ArgumentOutOfRangeException(nameof(gosterge))
	};
}
