using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum KartTarafi
{
	Sol,
	Sag
}

public partial class KaydirmaKontrolu : VBoxContainer
{
	private const float KararEsigi = 120.0f;

	private Label _onizleme = null;
	private Secenek _solSecenek = new();
	private Secenek _sagSecenek = new();
	private float _baslangicX;
	private float _kaymaX;
	private bool _surukleniyor;
	private bool _kararVerildi;

	public event Action<KartTarafi> KararOnaylandi;

	public override void _Ready()
	{
		_onizleme = GetNode<Label>("%KaydirmaOnizleme");
		GuiInput += GirisiIsle;
	}

	public void SecenekleriHazirla(Secenek sol, Secenek sag)
	{
		_solSecenek = sol;
		_sagSecenek = sag;
		_kararVerildi = false;
		OnizlemeyiTemizle();
	}

	public void SuruklemeyiBaslat(float baslangicX)
	{
		if (_kararVerildi)
			return;

		_surukleniyor = true;
		_baslangicX = baslangicX;
		_kaymaX = 0;
		OnizlemeyiTemizle();
	}

	public void SuruklemeyiGuncelle(float guncelX)
	{
		if (!_surukleniyor || _kararVerildi)
			return;

		_kaymaX = guncelX - _baslangicX;
		if (Mathf.Abs(_kaymaX) < KararEsigi)
		{
			OnizlemeyiTemizle();
			return;
		}

		var taraf = _kaymaX < 0 ? KartTarafi.Sol : KartTarafi.Sag;
		var secenek = taraf == KartTarafi.Sol ? _solSecenek : _sagSecenek;
		_onizleme.Text = $"{(taraf == KartTarafi.Sol ? "←" : "→")} {secenek.Etiket}\n{EtkiOklari(secenek.Etki)}";
	}

	public KartTarafi? SuruklemeyiBitir(float bitisX)
	{
		if (!_surukleniyor || _kararVerildi)
			return null;

		SuruklemeyiGuncelle(bitisX);
		_surukleniyor = false;
		if (Mathf.Abs(_kaymaX) < KararEsigi)
		{
			OnizlemeyiTemizle();
			return null;
		}

		_kararVerildi = true;
		var taraf = _kaymaX < 0 ? KartTarafi.Sol : KartTarafi.Sag;
		KararOnaylandi?.Invoke(taraf);
		return taraf;
	}

	private void GirisiIsle(InputEvent olay)
	{
		switch (olay)
		{
			case InputEventMouseButton fare when fare.ButtonIndex == MouseButton.Left:
				if (fare.Pressed)
					SuruklemeyiBaslat(fare.Position.X);
				else
					SuruklemeyiBitir(fare.Position.X);
				break;
			case InputEventMouseMotion fare when _surukleniyor:
				SuruklemeyiGuncelle(fare.Position.X);
				break;
			case InputEventScreenTouch dokunma:
				if (dokunma.Pressed)
					SuruklemeyiBaslat(dokunma.Position.X);
				else
					SuruklemeyiBitir(dokunma.Position.X);
				break;
			case InputEventScreenDrag dokunma when _surukleniyor:
				SuruklemeyiGuncelle(dokunma.Position.X);
				break;
		}
	}

	private void OnizlemeyiTemizle()
	{
		_onizleme.Text = "";
	}

	private static string EtkiOklari(IReadOnlyDictionary<string, int> etkiler)
	{
		return string.Join("  ", etkiler
			.OrderBy(etki => etki.Key, StringComparer.Ordinal)
			.Select(etki => $"{GostergeAdi(etki.Key)} {(etki.Value > 0 ? "▲" : "▼")}"));
	}

	private static string GostergeAdi(string anahtar) => anahtar switch
	{
		"yonetim" => "Yönetim",
		"halk" => "Halk",
		"para" => "Para",
		"ordu" => "Ordu",
		_ => anahtar
	};
}
