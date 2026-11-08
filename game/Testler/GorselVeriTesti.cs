using Godot;
using Krallik;
using System;
using System.Linq;

/// <summary>Her rolün portresi ve mekânı var; harita verisi geçerli; bozuk mekân verisi reddedilir.</summary>
public partial class GorselVeriTesti : Node
{
    private int _kontrol;
    private void Dogrula(bool kosul, string mesaj)
    {
        _kontrol++;
        if (!kosul) throw new InvalidOperationException(mesaj);
    }
    private void HataBekle(string json, System.Collections.Generic.IReadOnlySet<string> roller)
    {
        try { MekanListesi.Oku(json, roller); }
        catch (FormatException) { _kontrol++; return; }
        throw new InvalidOperationException("Bozuk mekân verisi kabul edildi.");
    }
    public override void _Ready()
    {
        try
        {
            var tema = Tema.Oku(FileAccess.GetFileAsString("res://Veri/tema.json"));
            var roller = tema.Karakterler.Select(r => r.Anahtar).ToHashSet();
            foreach (var rol in roller)
            {
                Dogrula(ResourceLoader.Exists(KarakterGorselleri.PortreYolu(rol)), $"{rol} rolünün portresi yok.");
                Dogrula(KarakterGorselleri.Mekanlar.ContainsKey(rol) && ResourceLoader.Exists(KarakterGorselleri.MekanYolu(rol)), $"{rol} rolünün mekânı yok.");
            }
            Dogrula(KarakterGorselleri.Portreler.Keys.All(roller.Contains), "Portre eşlemesinde temada olmayan rol var.");
            foreach (var h in Enum.GetValues<Hukumdar>())
                Dogrula(ResourceLoader.Exists(KarakterGorselleri.HukumdarYolu(h)), $"{h} tahtta oturan çizimi yok.");
            Dogrula(ResourceLoader.Exists(KarakterGorselleri.HaritaYolu), "Harita çizimi yok.");

            string json = FileAccess.GetFileAsString("res://Veri/mekanlar.json");
            var mekanlar = MekanListesi.Oku(json, roller);
            var yasak = System.Text.Json.Nodes.JsonNode.Parse(FileAccess.GetFileAsString("res://Veri/tema.json"))["yasak_kelimeler"]
                .AsArray().Select(k => (string)k).ToArray();
            Dogrula(mekanlar.Length == 6 && mekanlar.Count(m => m.TahtSalonu) == 1, "Haritada altı mekân, bir taht salonu olmalı.");
            foreach (var m in mekanlar)
            {
                Dogrula(ResourceLoader.Exists(KarakterGorselleri.MekanCizimi(m.Anahtar)), $"{m.Anahtar} çizimi yok.");
                if (m.TahtSalonu) continue;
                Dogrula(KarakterGorselleri.Sakinler(m.Anahtar, m.Sozcu)[0] == m.Sozcu, $"{m.Anahtar}: sözcü önde durmalı.");
                Dogrula(m.Durumlar.All(d => !yasak.Any(y => d.Contains(y, StringComparison.OrdinalIgnoreCase))),
                    $"{m.Anahtar}: durum cümlesinde yasak kelime var.");
            }
            Dogrula(Gostergeler.Siralama.All(g => mekanlar.Any(m => m.Gosterge == g)),
                "Her gösterge en az bir mekânda anlatılmalı.");
            Dogrula(new[] { 0, 15, 16, 35, 36, 64, 65, 84, 85, 100 }.Select(Mekan.Aralik).SequenceEqual(new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4 }) &&
                Mekan.Aralik(-5) == 0 && Mekan.Aralik(130) == 4, "Durum aralıkları yanlış.");
            Dogrula(Mekan.Tehlikede(10) && Mekan.Tehlikede(90) && !Mekan.Tehlikede(50), "Uç değerler tehlikeli sayılmalı.");

            HataBekle("{", roller);
            HataBekle("{\"mekanlar\": []}", roller);
            HataBekle(json.Replace("\"sozcu\": \"hazinedar\"", "\"sozcu\": \"yok\""), roller);
            HataBekle(json.Replace("\"gosterge\": \"ordu\"", "\"gosterge\": \"sans\""), roller);
            HataBekle(json.Replace("\"anahtar\": \"taht_salonu\"", "\"anahtar\": \"carsi\""), roller);
            HataBekle(json.Replace("[0.5, 0.25]", "[0.5, 1.25]"), roller);
            GD.Print($"GÖRSEL VE VERİ TESTİ BAŞARILI: {_kontrol} kontrol; 23 portre, 2 hükümdar, harita ve mekân verisi.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
