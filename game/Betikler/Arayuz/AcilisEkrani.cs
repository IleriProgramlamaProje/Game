using Godot;
using System;

public enum OyuncuCinsiyeti
{
	Erkek,
	Kadin
}

public partial class AcilisEkrani : VBoxContainer
{
	private Button _baslaDugmesi = null;
	private Control _cinsiyetSecimi = null;
	private Button _erkekDugmesi = null;
	private Button _kadinDugmesi = null;
	private Button _tahtaCikDugmesi = null;

	public OyuncuCinsiyeti? SecilenCinsiyet { get; private set; }
	public string Hitap => SecilenCinsiyet == OyuncuCinsiyeti.Kadin ? "Hatunum" : "Beyim";
	public event Action<string> TahtaCikildi;

	public override void _Ready()
	{
		_baslaDugmesi = GetNode<Button>("%BaslaDugmesi");
		_cinsiyetSecimi = GetNode<Control>("%CinsiyetSecimi");
		_erkekDugmesi = GetNode<Button>("%ErkekDugmesi");
		_kadinDugmesi = GetNode<Button>("%KadinDugmesi");
		_tahtaCikDugmesi = GetNode<Button>("%TahtaCikDugmesi");

		_baslaDugmesi.Pressed += CinsiyetSeciminiGoster;
		_erkekDugmesi.Pressed += () => CinsiyetSec(OyuncuCinsiyeti.Erkek);
		_kadinDugmesi.Pressed += () => CinsiyetSec(OyuncuCinsiyeti.Kadin);
		_tahtaCikDugmesi.Pressed += TahtaCik;
	}

	private void CinsiyetSeciminiGoster()
	{
		_baslaDugmesi.Visible = false;
		_cinsiyetSecimi.Visible = true;
	}

	private void CinsiyetSec(OyuncuCinsiyeti cinsiyet)
	{
		SecilenCinsiyet = cinsiyet;
		_erkekDugmesi.SetPressedNoSignal(cinsiyet == OyuncuCinsiyeti.Erkek);
		_kadinDugmesi.SetPressedNoSignal(cinsiyet == OyuncuCinsiyeti.Kadin);
		_tahtaCikDugmesi.Disabled = false;
	}

	private void TahtaCik()
	{
		if (SecilenCinsiyet == null)
			return;

		TahtaCikildi?.Invoke(Hitap);
	}
}
