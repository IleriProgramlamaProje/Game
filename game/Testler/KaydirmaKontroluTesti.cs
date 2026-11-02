using Godot;
using System;

public partial class KaydirmaKontroluTesti : Node
{
    public override void _Ready()
    {
        try
        {
            var ekranlar = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
            AddChild(ekranlar);
            ekranlar.GetNode<Button>("%Baslat").EmitSignal(BaseButton.SignalName.Pressed);
            var kart = ekranlar.GetNode<KartGorunumu>("%KartAlani");
            var metin = ekranlar.GetNode<Label>("%KartMetni");
            string anlatim = metin.Text;
            kart.SuruklemeyiBaslat(200);
            kart.SuruklemeyiGuncelle(200 - kart.Esik / 2);
            if (ekranlar.Oyun.Durum.KararSayisi != 0) throw new Exception("Önizleme durum değiştirmemeli.");
            if (kart.SuruklemeyiBitir(200 - kart.Esik / 2) != null || metin.Text != anlatim)
                throw new Exception("Eşik altı hareket iptal olmalı.");
            kart.SuruklemeyiBaslat(200);
            kart.SuruklemeyiGuncelle(200 - kart.Esik - 5);
            if (!metin.Text.Contains("▼") || !metin.Text.Contains("▲") || metin.Text.Contains("Para −10"))
                throw new Exception("Önizleme sayılar yerine etki oklarını göstermeli.");
            if (ekranlar.Oyun.Durum.Gostergeler.Degerler["para"] != 50) throw new Exception("Önizleme değerleri değiştirmemeli.");
            if (kart.SuruklemeyiBitir(200 - kart.Esik - 5) != KartTarafi.Sol || ekranlar.Oyun.Durum.KararSayisi != 1)
                throw new Exception("Eşik üstü hareket tek karar üretmeli.");
            if (kart.SuruklemeyiBitir(500) != null || ekranlar.Oyun.Durum.KararSayisi != 1)
                throw new Exception("İkinci bırakma etkisiz olmalı.");
            GD.Print("KAYDIRMA TESTİ BAŞARILI: eşik, önizleme ve tek karar.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
