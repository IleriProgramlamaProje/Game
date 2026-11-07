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
            var yeni = new YedekDeste(FileAccess.GetFileAsString("res://Testler/Veri/ornek_deste.json"), roller);
            if (yeni.Adet != 10) throw new Exception("JSON dizi destesi de okunmalı.");
            GercekDesteyiDene(roller);
            bool reddedildi = false;
            try { YedekDeste.MetindenYukle("{ bozuk json }", roller); }
            catch (FormatException) { reddedildi = true; }
            if (!reddedildi) throw new Exception("Bozuk JSON reddedilmeli.");
            GD.Print("YEDEK DESTE TESTİ BAŞARILI: iki deste biçimi, kimlik, etkiler, bozuk JSON ve altın set destesi.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }

    /// <summary>2.8: Oyunun varsayılan destesi altın setten gelir ve oyun gerçekten onu kullanır.</summary>
    private static void GercekDesteyiDene(System.Collections.Generic.IReadOnlySet<string> roller)
    {
        const string yol = "res://Veri/yedek_deste.json";
        var oyun = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
        string varsayilan = oyun.DesteYolu;
        oyun.Free();
        if (varsayilan != yol) throw new Exception("Oyun varsayılan olarak altın set destesini açmalı.");
        // Tek geçersiz kart bütün desteyi reddettirir; yükleme başarısı tüm kartların geçerli olduğunu gösterir.
        var kartlar = new YedekDeste(FileAccess.GetFileAsString(yol), roller).KartlariYukle();
        if (kartlar.Count < 100) throw new Exception("Yedek deste en az 100 altın set kartı içermeli.");
        if (kartlar.Any(k => k.Kaynak != "altin_set" || k.Lisans != "sadece_ders"))
            throw new Exception("Kaynak ve lisans etiketleri altın setteki gibi korunmalı.");
        if (kartlar.Any(k => k.Id?.StartsWith("altin-") != true) || kartlar.Select(k => k.Id).Distinct().Count() != kartlar.Count)
            throw new Exception("Her kart altın setteki tekil kimliğini taşımalı.");
        if (!roller.All(r => kartlar.Any(k => k.Konusan == r)))
            throw new Exception("Temadaki her rolün destede kartı olmalı.");
        if (Enumerable.Range(1, kartlar.Count - 1).Any(i => kartlar[i].Konusan == kartlar[i - 1].Konusan))
            throw new Exception("Aynı konuşmacı art arda gelmemeli.");
    }
}
