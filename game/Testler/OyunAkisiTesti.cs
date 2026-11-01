using Godot;
using System;

public partial class OyunAkisiTesti : Node
{
	public override void _Ready()
	{
		try
		{
			var oyun = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
			AddChild(oyun);
			oyun.GetNode<Button>("%BaslaDugmesi").EmitSignal(BaseButton.SignalName.Pressed);
			oyun.GetNode<Button>("%ErkekDugmesi").EmitSignal(BaseButton.SignalName.Pressed);
			oyun.GetNode<Button>("%TahtaCikDugmesi").EmitSignal(BaseButton.SignalName.Pressed);
			var kart = oyun.GetNode<KartGorunumu>("%KartGorunumu");
			kart.SuruklemeyiBaslat(200);
			kart.SuruklemeyiBitir(50);
			if (oyun.AktifEkran != OyunEkranlari.Ekran.Sonuc || oyun.Durum.DegeriniAl(Gosterge.Para) != 55) throw new Exception("Karar sonucu uygulanmadı.");
			kart.SuruklemeyiBitir(50);
			if (oyun.Durum.DegeriniAl(Gosterge.Para) != 55) throw new Exception("Karar iki kez uygulandı.");
			oyun.GetNode<Button>("%DevamDugmesi").EmitSignal(BaseButton.SignalName.Pressed);
			if (oyun.AktifEkran != OyunEkranlari.Ekran.Oyun || !oyun.GetNode<Label>("%KartBasligi").Text.Contains("Sınır Beyi")) throw new Exception("Sonraki karta geçilmedi.");
			GD.Print("OYUN AKIŞI TESTİ BAŞARILI");
			GetTree().Quit();
		}
		catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
	}
}
