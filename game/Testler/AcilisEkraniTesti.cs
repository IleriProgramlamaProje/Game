using Godot;
using Krallik;
using System;

public partial class AcilisEkraniTesti : Node
{
    public override async void _Ready()
    {
        try
        {
            foreach (int secim in new[] { 0, 1 })
            {
                var ekranlar = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
                ekranlar.DesteYolu = "res://Testler/Veri/ornek_deste.json"; // İlk kartın hitap içerdiği bilinen örnek deste.
                AddChild(ekranlar);
                if (ekranlar.AktifEkran != OyunEkranlari.Ekran.Acilis) throw new Exception("Önce açılış görünmeli.");
                ekranlar.GetNode<Button>(secim == 0 ? "%Beyim" : "%Hatunum").EmitSignal(BaseButton.SignalName.Pressed);
                var cinsiyet = secim == 0 ? Hukumdar.Bey : Hukumdar.Hatun;
                if (ekranlar.AktifEkran != OyunEkranlari.Ekran.Oyun || ekranlar.Oyun.Durum.Cinsiyet != cinsiyet ||
                    !ekranlar.GetNode<DiyalogKutusu>("%DiyalogKutusu").TamMetin.Contains(secim == 0 ? "Beyim" : "Hatunum"))
                    throw new Exception("Hükümdar butonu ve hitabı oyuna aktarmalı.");
                // Telefonda bulunan hata: ilk kart, kutu henüz genişlik almadan açılır. Söz yine baştan,
                // harf harf akmalı; bitmiş sayılıp son sayfaya atlamamalı.
                var kart = ekranlar.GetNode<DiyalogKutusu>("%DiyalogKutusu");
                for (int i = 0; i < 6; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                if (!kart.Yaziyor || kart.Bitti || kart.Etkin || kart.Sayfa != 0 ||
                    ekranlar.GetNode<Label>("%DiyalogMetni").VisibleCharacters >= kart.Sayfalar[0].Length)
                    throw new Exception($"İlk kartın sözü baştan harf harf akmalı (yazıyor={kart.Yaziyor}, bitti={kart.Bitti}, sayfa={kart.Sayfa}/{kart.SayfaSayisi}).");
                ekranlar.Free();
            }
            GD.Print("AÇILIŞ TESTİ BAŞARILI: iki hükümdar butonu, oyun akışı ve ilk kartın harf harf akması.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
