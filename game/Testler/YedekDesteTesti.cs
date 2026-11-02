using Godot;
using Krallik;
using System;
using System.Linq;

public partial class YedekDesteTesti : Node
{
    public override void _Ready()
    {
        try
        {
            var roller = Tema.Oku(FileAccess.GetFileAsString("res://Veri/tema.json")).Karakterler.Select(r => r.Anahtar).ToHashSet();
            var kartlar = new YedekDeste(FileAccess.GetFileAsString("res://Veri/yedek_desta.json"), roller).KartlariYukle();
            if (kartlar.Count != 10 || kartlar[0].Id != "O001" || kartlar[2].Sol.Etki["ordu"] != 10)
                throw new Exception("Ömer'in kart kimliği ve etkileri korunmalı.");
            var yeni = new YedekDeste(FileAccess.GetFileAsString("res://Veri/yedek_deste.json"), roller);
            if (yeni.Adet != 10) throw new Exception("JSON dizi destesi de okunmalı.");
            bool reddedildi = false;
            try { YedekDeste.MetindenYukle("{ bozuk json }", roller); }
            catch (FormatException) { reddedildi = true; }
            if (!reddedildi) throw new Exception("Bozuk JSON reddedilmeli.");
            GD.Print("YEDEK DESTE TESTİ BAŞARILI: iki deste biçimi, kimlik, etkiler ve bozuk JSON.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
