using Godot;
using System;

public partial class KartGorunumuTesti : Node
{
	public override void _Ready()
	{
		try
		{
			var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
			var ekranlar = sahne.Instantiate<OyunEkranlari>();
			AddChild(ekranlar);
			ekranlar.EkraniGoster(OyunEkranlari.Ekran.Oyun);

			Dogrula(ekranlar.GetNode<Label>("%KartBasligi").Text == "Elçi Deniz", "Unvan ve oturum adı görünmeli.");
			Dogrula(ekranlar.GetNode<Label>("%KartMetni").Text.Contains("Beyim"), "{hitap} Beyim olarak yerleşmeli.");
			Dogrula(ekranlar.GetNode<Label>("%SolSecenek").Text.Contains("Elçiyi huzura kabul et"), "Sol seçenek görünmeli.");
			Dogrula(ekranlar.GetNode<Label>("%SagSecenek").Text.Contains("Elçiyi geri gönder"), "Sağ seçenek görünmeli.");

			GD.Print("KART GÖRÜNÜMÜ TESTİ BAŞARILI: başlık, hitap ve iki seçenek.");
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
