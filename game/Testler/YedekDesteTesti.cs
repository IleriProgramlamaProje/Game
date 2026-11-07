using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
            var ilk = kartlar[0] with { OlayGrubu = "elcilik" };
            var varyant = ilk with { Id = "O001B" };
            var diger = kartlar[1];
            HataBekle(() => new YedekDeste(JsonSerializer.Serialize(new[] { ilk, ilk }), roller));
            HataBekle(() => new YedekDeste(JsonSerializer.Serialize(new[] { ilk with { Id = " " } }), roller));
            string tekrarAlan = JsonSerializer.Serialize(new[] { ilk }).Replace("\"etki\":{", "\"etki\":{\"para\":5,");
            HataBekle(() => new YedekDeste(tekrarAlan, roller));
            var gruplu = new YedekDeste(JsonSerializer.Serialize(new[] { ilk, varyant, diger }), roller);
            string[] sira = Enumerable.Range(0, 5).Select(_ => gruplu.Sonraki().Id).ToArray();
            if (gruplu.OlayGrubuSayisi != 2 || !sira.SequenceEqual(new[] { ilk.Id, diger.Id, varyant.Id, diger.Id, ilk.Id }))
                throw new Exception("Bir turda aynı olay grubundan tek kart gelmeli; tükenince varyant dönmeli.");
            gruplu.KonumaGit(2);
            if (gruplu.Sonraki().Id != varyant.Id) throw new Exception("Kayıtlı kaynak konumu varyantı korumalı.");
            if (yeni.KartlariYukle().Any(k => !k.OlayKimligi.StartsWith("yerel:", StringComparison.Ordinal)))
                throw new Exception("Kimliksiz eski kartlara kararlı kimlik atanmalı.");
            DisOlaylariDene(yeni, roller);
            GD.Print("YEDEK DESTE TESTİ BAŞARILI: iki biçim, Altın Set, kimlik, bozuk JSON, varyant ve dış olay yedeği.");
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
        var deste = new YedekDeste(FileAccess.GetFileAsString(yol), roller);
        foreach (var kart in kartlar)
            if (deste.Sonraki().Id != kart.Id) throw new Exception("Altın Set sırası değişti.");
        if (deste.Sonraki().Id != kartlar[0].Id) throw new Exception("Altın Set tükendiğinde yeni tur başlamalı.");
    }

    private static void HataBekle(Action islem)
    {
        try { islem(); }
        catch (FormatException) { return; }
        throw new Exception("Geçersiz deste reddedilmedi.");
    }

    private static void DisOlaylariDene(YedekDeste yerel, IReadOnlySet<string> roller)
    {
        var dis = yerel.KartlariYukle()[0] with { Id = "dis-001" };
        string yanit = JsonSerializer.Serialize(dis);
        var gecersiz = dis with { Sag = dis.Sag with { Etki = new() { ["ordu"] = 999 } } };
        var cevaplar = new Queue<string>(new[]
        {
            yanit, yanit, JsonSerializer.Serialize(gecersiz),
            yanit.Replace("\"id\":", "\"gostergeler\":{\"para\":100},\"id\":")
        });
        var kaynak = new YedekliKartKaynagi(yerel, () => cevaplar.TryDequeue(out var json) ? json : null, roller);
        var oyun = new OyunYoneticisi(kaynak);
        oyun.Baslat(Hukumdar.Bey);
        if (oyun.AktifKart.Id != dis.Id || kaynak.Konum != 0 || oyun.Durum.Gostergeler.Degerler.Values.Any(v => v != 50))
            throw new Exception("Dış olay yalnızca kart sağlamalı ve yerel sırayı tüketmemeli.");
        var anlik = oyun.AnligiAl();
        anlik.AktifOlay.Sag.Etki.Clear();
        if (oyun.AktifKart.Sag.Etki.Count == 0) throw new Exception("Dışa aktarılan durum canlı kartı değiştirmemeli.");
        for (int i = 0; i < 4; i++)
        {
            oyun.KararVer(true, oyun.KartKimligi);
            oyun.DevamEt();
            if (oyun.AktifKart.OlayKimligi != yerel.KartlariYukle()[i].OlayKimligi || kaynak.Konum != i + 1)
                throw new Exception("Tekrarlı, geçersiz, alan enjekte eden veya boş yanıt yerel sıraya dönmeli.");
        }
    }
}
