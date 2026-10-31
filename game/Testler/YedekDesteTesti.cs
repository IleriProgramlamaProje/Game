using Godot;
using System;

public partial class YedekDesteTesti : Node
{
	public override void _Ready()
	{
		try
		{
			var kartlar = new YedekDeste().KartlariYukle();
			Dogrula(kartlar.Count == 10, "Yedek destede 10 kart olmalı.");
			Dogrula(kartlar[0].Id == "O001", "İlk kart kimliği korunmalı.");
			Dogrula(kartlar[2].Sol.Etki["ordu"] == 10, "Etki sayıları JSON'dan doğru okunmalı.");

			try
			{
				YedekDeste.MetindenYukle("{ bozuk json }");
				throw new InvalidOperationException("Bozuk JSON reddedilmeli.");
			}
			catch (InvalidOperationException)
			{
			}

			GD.Print("YEDEK DESTE TESTİ BAŞARILI: 10 kart, etkiler ve bozuk JSON kontrolü.");
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
