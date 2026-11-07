using Godot;
using Krallik;
using System;

public partial class KartGorunumuTesti : Node
{
    public override void _Ready()
    {
        try
        {
            var ekranlar = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
            ekranlar.DesteYolu = "res://Testler/Veri/ornek_deste.json"; // İlk kartı hazinedar olan örnek deste.
            AddChild(ekranlar);
            ekranlar.GetNode<Button>("%Beyim").EmitSignal(BaseButton.SignalName.Pressed);
            var kart = ekranlar.Oyun.AktifKart;
            string baslik = ekranlar.GetNode<Label>("%KartBasligi").Text;
            if (!baslik.StartsWith("Hazinedar ") || !ekranlar.GetNode<Label>("%KartMetni").Text.Contains("Beyim"))
                throw new Exception("Unvan, oturum adı ve hitap görünmeli.");
            var cevrilenKart = ekranlar.GetNode<KartGorunumu>("%KartAlani");
            cevrilenKart.SuruklemeyiBaslat(200);
            cevrilenKart.SuruklemeyiGuncelle(200 - cevrilenKart.Esik);
            if (!ekranlar.GetNode<Label>("%KararOnizleme").Text.Contains(kart.Sol.Etiket))
                throw new Exception("Sola çevirirken sol seçenek görünmeli.");
            cevrilenKart.Iptal();
            var metin = new MetinYerlestirici(Tema.Oku(FileAccess.GetFileAsString("res://Veri/tema.json")).Karakterler, Hukumdar.Hatun, new Random(7));
            var gorunum = ekranlar.GetNode<KartGorunumu>("%KartAlani");
            gorunum.KartiGoster(kart, metin);
            string hatunBasligi = ekranlar.GetNode<Label>("%KartBasligi").Text;
            gorunum.KartiGoster(kart, metin);
            if (ekranlar.GetNode<Label>("%KartBasligi").Text != hatunBasligi ||
                !ekranlar.GetNode<Label>("%KartMetni").Text.Contains("Hatunum"))
                throw new Exception("Tekrar gösterimde oturum kimliği ve yeni hitap korunmalı.");
            GD.Print("KART GÖRÜNÜMÜ TESTİ BAŞARILI: başlık, hitap ve iki seçenek.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
