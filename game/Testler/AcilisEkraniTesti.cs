using Godot;
using System;

public partial class AcilisEkraniTesti : Node
{
	public override void _Ready()
	{
		try
		{
			var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
			var ekranlar = sahne.Instantiate<OyunEkranlari>();
			AddChild(ekranlar);
			var basla = ekranlar.GetNode<Button>("%BaslaDugmesi");
			var secim = ekranlar.GetNode<Control>("%CinsiyetSecimi");
			var kadin = ekranlar.GetNode<Button>("%KadinDugmesi");
			var tahtaCik = ekranlar.GetNode<Button>("%TahtaCikDugmesi");

			Dogrula(basla.Visible && !secim.Visible, "Açılış önce Başla düğmesini göstermeli.");
			basla.EmitSignal(BaseButton.SignalName.Pressed);
			Dogrula(!basla.Visible && secim.Visible && tahtaCik.Disabled, "Başla sonrası cinsiyet seçimi açılmalı.");
			kadin.EmitSignal(BaseButton.SignalName.Pressed);
			Dogrula(!tahtaCik.Disabled, "Cinsiyet seçilince Tahta Çık etkinleşmeli.");
			tahtaCik.EmitSignal(BaseButton.SignalName.Pressed);
			Dogrula(ekranlar.AktifEkran == OyunEkranlari.Ekran.Oyun, "Tahta Çık oyuna geçirmeli.");
			Dogrula(ekranlar.GetNode<Label>("%KartMetni").Text.Contains("Hatunum"), "Kadın seçimi Hitap metnine yerleşmeli.");

			GD.Print("AÇILIŞ TESTİ BAŞARILI: Başla, cinsiyet seçimi ve Tahta Çık akışı.");
			GetTree().Quit();
		}
		catch (Exception hata)
		{
			GD.PushError(hata.ToString());
			GetTree().Quit(1);
		}
	}

	private static void Dogrula(bool kosul, string mesaj)
	{
		if (!kosul)
			throw new InvalidOperationException(mesaj);
	}
}
