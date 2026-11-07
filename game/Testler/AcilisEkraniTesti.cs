using Godot;
using Krallik;
using System;

public partial class AcilisEkraniTesti : Node
{
    public override void _Ready()
    {
        try
        {
            foreach (int secim in new[] { 0, 1 })
            {
                var ekranlar = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
                AddChild(ekranlar);
                if (ekranlar.AktifEkran != OyunEkranlari.Ekran.Acilis) throw new Exception("Önce açılış görünmeli.");
                ekranlar.GetNode<Button>(secim == 0 ? "%Beyim" : "%Hatunum").EmitSignal(BaseButton.SignalName.Pressed);
                var cinsiyet = secim == 0 ? Hukumdar.Bey : Hukumdar.Hatun;
                if (ekranlar.AktifEkran != OyunEkranlari.Ekran.Oyun || ekranlar.Oyun.Durum.Cinsiyet != cinsiyet ||
                    !ekranlar.GetNode<Label>("%KartMetni").Text.Contains(secim == 0 ? "Beyim" : "Hatunum"))
                    throw new Exception("Hükümdar butonu ve hitabı oyuna aktarmalı.");
                ekranlar.Free();
            }
            GD.Print("AÇILIŞ TESTİ BAŞARILI: iki hükümdar butonu ve oyun akışı.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
