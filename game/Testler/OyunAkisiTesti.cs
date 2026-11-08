using Godot;
using System;

public partial class OyunAkisiTesti : Node
{
    public override async void _Ready()
    {
        try
        {
            var oyun = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
            oyun.DesteYolu = "res://Testler/Veri/ornek_deste.json"; // İlk kartın sol kararı Para −10 olan örnek deste.
            oyun.Animasyonlar = false;
            AddChild(oyun);
            oyun.GetNode<Button>("%Beyim").EmitSignal(BaseButton.SignalName.Pressed);
            for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            long kimlik = oyun.Oyun.KartKimligi;
            var kart = oyun.GetNode<DiyalogKutusu>("%DiyalogKutusu");
            kart.SuruklemeyiBaslat(200);
            kart.SuruklemeyiBitir(200 - kart.Esik - 5);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            if (oyun.AktifEkran != OyunEkranlari.Ekran.Sonuc || oyun.Oyun.Durum.Gostergeler.Degerler["para"] != 40)
                throw new Exception("Karar sonucu uygulanmadı.");
            kart.SuruklemeyiBaslat(200);
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
