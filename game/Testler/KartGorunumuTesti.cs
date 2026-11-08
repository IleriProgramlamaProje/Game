using Godot;
using Krallik;
using System;
using System.Linq;

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
            var mekan = ekranlar.GetNode<TextureRect>("%KartMekani");
            var portre = ekranlar.GetNode<TextureRect>("%KartPortresi");
            if (mekan.Texture?.ResourcePath != "res://Assets/mekanlar/hazine_odasi.svg" ||
                portre.Texture?.ResourcePath != "res://Assets/karakterler/defterdar.svg")
                throw new Exception("Hazinedar kartında hazine odası ve defterdar çizimi görünmeli.");
            GorselleriDogrula();
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
            // Çizimi olmayan rolde önceki konuşanın portresi kalmamalı; mekân yine görünür.
            gorunum.KartiGoster(kart with { Konusan = "soytari" }, metin);
            if (portre.Texture != null || mekan.Texture?.ResourcePath != "res://Assets/mekanlar/taht_salonu.svg")
                throw new Exception("Portresiz rol taht salonunda, portresiz görünmeli.");
            GD.Print("KART GÖRÜNÜMÜ TESTİ BAŞARILI: başlık, hitap, portre/mekân ve iki seçenek.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }

    /// <summary>Her rolün mekânı vardır; eşlemedeki her çizim dosyası projede bulunur ve geçerli bir role aittir.</summary>
    private static void GorselleriDogrula()
    {
        var roller = Tema.Oku(FileAccess.GetFileAsString("res://Veri/tema.json")).Karakterler;
        var anahtarlar = roller.Select(r => r.Anahtar).ToHashSet();
        foreach (var rol in roller)
            if (!KarakterGorselleri.Mekanlar.ContainsKey(rol.Anahtar) || !ResourceLoader.Exists(KarakterGorselleri.MekanYolu(rol.Anahtar)))
                throw new Exception($"{rol.Anahtar} rolünün mekân çizimi yok.");
        foreach (var (anahtar, _) in KarakterGorselleri.Portreler)
        {
            string rol = anahtar.EndsWith("_erkek") || anahtar.EndsWith("_kadin") ? anahtar[..anahtar.LastIndexOf('_')] : anahtar;
            if (!anahtarlar.Contains(rol) || !ResourceLoader.Exists(KarakterGorselleri.PortreYolu(rol, anahtar)))
                throw new Exception($"Portre eşlemesi geçersiz: {anahtar}");
        }
    }
}
