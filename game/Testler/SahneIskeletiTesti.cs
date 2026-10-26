using Godot;
using System;
using System.Threading.Tasks;

public partial class SahneIskeletiTesti : Node
{
	public override async void _Ready()
	{
		try
		{
			await Calistir();
			GD.Print("SAHNE TESTİ BAŞARILI: 3 boyutta 4 panel, düğmeler ve başlangıç durumu.");
			GetTree().Quit();
		}
		catch (Exception hata)
		{
			GD.PushError(hata.ToString());
			GetTree().Quit(1);
		}
	}

	private async Task Calistir()
	{
		var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
		Vector2I[] boyutlar = [new(1152, 648), new(360, 640), new(640, 360)];

		foreach (var boyut in boyutlar)
		{
			GetWindow().Size = boyut;
			GetWindow().ContentScaleSize = boyut;
			GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
			GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Ignore;
			var ekranlar = sahne.Instantiate<OyunEkranlari>();
			AddChild(ekranlar);
			Dogrula(ekranlar.AktifEkran == OyunEkranlari.Ekran.Acilis, "İlk panel açılış olmalı.");

			foreach (var ekran in Enum.GetValues<OyunEkranlari.Ekran>())
			{
				var dugme = ekranlar.GetNode<Button>($"%{ekran}Dugmesi");
				dugme.EmitSignal(BaseButton.SignalName.Pressed);
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				Dogrula(ekranlar.AktifEkran == ekran, "Düğme doğru paneli açmalı.");

				foreach (var diger in Enum.GetValues<OyunEkranlari.Ekran>())
				{
					bool secili = diger == ekran;
					Dogrula(ekranlar.GetNode<Control>($"%{diger}Paneli").IsVisibleInTree() == secili,
						"Yalnız seçili panel görünmeli.");
					var kontrol = ekranlar.GetNode<Button>($"%{diger}Dugmesi");
					Dogrula(kontrol.ButtonPressed == secili, "Gezinti seçimi panelle eşleşmeli.");
					Dogrula(kontrol.Size.Y >= 48, "Düğme dokunma yüksekliğini korumalı.");
					Dogrula(kontrol.GlobalPosition.X >= 0 && kontrol.GlobalPosition.X + kontrol.Size.X <= boyut.X,
						$"{boyut}: gezinti yatay taşmamalı. Düğme: {kontrol.GlobalPosition}, {kontrol.Size}; kök: {ekranlar.Size}.");
				}

				var icerik = ekranlar.GetNode<ScrollContainer>("%Icerik");
				Dogrula(icerik.Size.Y > 0, "İçerik için görünür alan kalmalı.");
				Dogrula(icerik.GetHScrollBar().MaxValue <= icerik.Size.X + 1,
					$"{boyut}: içerik yatay taşmamalı.");

				if (DisplayServer.GetName() != "headless")
				{
					await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
					DirAccess.MakeDirRecursiveAbsolute("res://.godot/testler");
					var sonuc = GetViewport().GetTexture().GetImage().SavePng(
						$"res://.godot/testler/{boyut.X}x{boyut.Y}-{ekran}.png");
					Dogrula(sonuc == Error.Ok, "Ekran görüntüsü kaydedilmeli.");
				}
			}

			try
			{
				ekranlar.EkraniGoster((OyunEkranlari.Ekran)999);
				throw new InvalidOperationException("Geçersiz panel reddedilmeli.");
			}
			catch (ArgumentOutOfRangeException)
			{
				Dogrula(ekranlar.AktifEkran == OyunEkranlari.Ekran.Son, "Geçersiz seçim durumu bozmamalı.");
			}

			ekranlar.Free();
		}
	}

	private static void Dogrula(bool kosul, string mesaj)
	{
		if (!kosul)
			throw new InvalidOperationException(mesaj);
	}
}
