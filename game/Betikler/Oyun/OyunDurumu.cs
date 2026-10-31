using Godot;
using System;
using System.Collections.Generic;

public enum Gosterge
{
	Yonetim,
	Halk,
	Para,
	Ordu
}

public sealed class OyunDurumu
{
	public const int BaslangicDegeri = 50;
	public const int EnDusukDeger = 0;
	public const int EnYuksekDeger = 100;

	private readonly Dictionary<Gosterge, int> _degerler = new();

	public event Action<Gosterge, int> GostergeDegisti;

	public OyunDurumu()
	{
		Sifirla();
	}

	public int DegeriniAl(Gosterge gosterge) => _degerler[gosterge];

	public void EtkiUygula(Gosterge gosterge, int etki)
	{
		int yeniDeger = Mathf.Clamp(DegeriniAl(gosterge) + etki, EnDusukDeger, EnYuksekDeger);
		if (yeniDeger == DegeriniAl(gosterge))
			return;

		_degerler[gosterge] = yeniDeger;
		GostergeDegisti?.Invoke(gosterge, yeniDeger);
	}

	public void Sifirla()
	{
		foreach (Gosterge gosterge in Enum.GetValues<Gosterge>())
			_degerler[gosterge] = BaslangicDegeri;
	}
}
