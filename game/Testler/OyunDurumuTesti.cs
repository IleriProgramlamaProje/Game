using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OyunDurumuTesti : Node
{
    public override void _Ready()
    {
        try
        {
            var durum = new OyunDurumu(Hukumdar.Bey);
            var gostergeler = durum.Gostergeler;
            if (!gostergeler.Degerler.Values.All(v => v == 50)) throw new Exception("Her gösterge 50 başlamalı.");
            int bildirilen = 0;
            gostergeler.GostergeDegisti += (ad, deger) => { if (ad == "para") bildirilen = deger; };
            gostergeler.Uygula(new Dictionary<string, int> { ["para"] = -15 });
            if (gostergeler.Degerler["para"] != 35 || bildirilen != 35) throw new Exception("Etki/bildirim doğru göstergeye uygulanmalı.");
            for (int i = 0; i < 5; i++) gostergeler.Uygula(new Dictionary<string, int> { ["para"] = -15, ["ordu"] = 15 });
            if (gostergeler.Degerler["para"] != 0 || gostergeler.Degerler["ordu"] != 100) throw new Exception("0–100 sınırları korunmalı.");
            GD.Print("OYUN DURUMU TESTİ BAŞARILI: başlangıç, etki, bildirim ve 0–100 sınırları.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
