using Godot;
using System;

public partial class OyunDurumuTesti : Node
{
	public override void _Ready()
	{
		try
		{
			var durum = new OyunDurumu();
			foreach (Gosterge gosterge in Enum.GetValues<Gosterge>())
				Dogrula(durum.DegeriniAl(gosterge) == 50, "Her gösterge 50 başlamalı.");

			durum.EtkiUygula(Gosterge.Para, -15);
			Dogrula(durum.DegeriniAl(Gosterge.Para) == 35, "Etki doğru göstergeye uygulanmalı.");
			durum.EtkiUygula(Gosterge.Para, -100);
			Dogrula(durum.DegeriniAl(Gosterge.Para) == 0, "Değer 0 altına inmemeli.");
			durum.EtkiUygula(Gosterge.Ordu, 100);
			Dogrula(durum.DegeriniAl(Gosterge.Ordu) == 100, "Değer 100 üstüne çıkmamalı.");

			GD.Print("OYUN DURUMU TESTİ BAŞARILI: başlangıç, etki ve 0–100 sınırları.");
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
