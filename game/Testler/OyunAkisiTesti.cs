using Godot;
using System;

public partial class OyunAkisiTesti : Node
{
    public override async void _Ready()
    {
        try
        {
            var oyun = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
            AddChild(oyun);
            oyun.GetNode<Button>("%Beyim").EmitSignal(BaseButton.SignalName.Pressed);
            var kart = oyun.GetNode<KartGorunumu>("%KartAlani");
            long kimlik = oyun.Oyun.KartKimligi;
            kart.SuruklemeyiBaslat(200);
            kart.SuruklemeyiBitir(200 - kart.Esik - 5);
            await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
            if (oyun.AktifEkran != OyunEkranlari.Ekran.Sonuc || oyun.Oyun.Durum.Gostergeler.Degerler["para"] != 40)
                throw new Exception("Karar sonucu uygulanmadı.");
            kart.SuruklemeyiBitir(0);
            if (oyun.Oyun.Durum.Gostergeler.Degerler["para"] != 40) throw new Exception("Karar iki kez uygulandı.");
            oyun.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
            if (oyun.AktifEkran != OyunEkranlari.Ekran.Oyun || oyun.Oyun.KartKimligi <= kimlik)
                throw new Exception("Sonraki karta geçilmedi.");
            GD.Print("OYUN AKIŞI TESTİ BAŞARILI");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
