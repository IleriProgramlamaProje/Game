using Godot;
using System;

public partial class KaydirmaKontroluTesti : Node
{
	public override void _Ready()
	{
		try
		{
			var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
			var ekranlar = sahne.Instantiate<OyunEkranlari>();
			AddChild(ekranlar);
			var kart = ekranlar.GetNode<KartGorunumu>("%KartGorunumu");
			var onizleme = ekranlar.GetNode<Label>("%KaydirmaOnizleme");

			kart.SuruklemeyiBaslat(100);
			kart.SuruklemeyiGuncelle(160);
			Dogrula(onizleme.Text == "", "Eşik altındaki sürükleme önizleme göstermemeli.");
			Dogrula(kart.SuruklemeyiBitir(160) == null, "Eşik altındaki sürükleme iptal olmalı.");

			kart.SuruklemeyiBaslat(200);
			kart.SuruklemeyiGuncelle(70);
			Dogrula(onizleme.Text.Contains("← Elçiyi huzura kabul et"), "Sol seçenek önizlenmeli.");
			Dogrula(onizleme.Text.Contains("Para ▲") && onizleme.Text.Contains("Yönetim ▼"), "Önizlemede yalnız etki okları görünmeli.");
			Dogrula(kart.SuruklemeyiBitir(70) == KartTarafi.Sol, "Eşik üstü sola sürükleme sol kararı seçmeli.");
			Dogrula(kart.SuruklemeyiBitir(330) == null, "Aynı kartta ikinci karar üretilmemeli.");
			Dogrula(ekranlar.Durum.DegeriniAl(Gosterge.Para) == 50, "Önizleme oyun durumunu değiştirmemeli.");

			GD.Print("KAYDIRMA TESTİ BAŞARILI: eşik, önizleme ve tek karar.");
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
