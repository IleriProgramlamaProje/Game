using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DengeTesti : Node
{
    private const int UstKarar = 500;
    private const int RastgeleOyunSayisi = 1000;
    private record Sonuc(int Karar, int Yil, bool Bitis, bool Dongu);

    public override void _Ready()
    {
        try
        {
            var tema = Tema.Oku(FileAccess.GetFileAsString("res://Veri/tema.json"));
            var deste = new YedekDeste(FileAccess.GetFileAsString("res://Veri/yedek_deste.json"),
                tema.Karakterler.Select(r => r.Anahtar).ToHashSet());
            var oyun = new OyunYoneticisi(deste, tema.Yil.KacKarttaBirYil);
            Yaz("Sürekli sol", Simule(oyun, _ => false, true));
            Yaz("Sürekli sağ", Simule(oyun, _ => true, true));
            Yaz("Merkeze yakın", Simule(oyun,
                o => Risk(o, o.AktifKart.Sag) < Risk(o, o.AktifKart.Sol), true));
            var rastgele = new Random(657);
            var kosular = Enumerable.Range(0, RastgeleOyunSayisi)
                .Select(_ => Simule(oyun, _ => rastgele.Next(2) == 1, false))
                .OrderBy(s => s.Karar).ToArray();
            int limit = kosular.Count(s => !s.Bitis);
            int medyan = (kosular[499].Karar + kosular[500].Karar) / 2;
            GD.Print($"Rastgele {RastgeleOyunSayisi} oyun: min={kosular[0].Karar}, medyan={medyan}, " +
                $"p90={kosular[899].Karar}, max={kosular[^1].Karar} karar, {limit} koşu {UstKarar} karar sınırında.");
            GD.Print("DENGE TESTİ BAŞARILI: 0–100, yıl, sekiz son anahtarı ve tekrarlanabilir deste simülasyonu.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }

    private static Sonuc Simule(OyunYoneticisi oyun, Func<OyunYoneticisi, bool> sec, bool donguAra)
    {
        oyun.Baslat(Hukumdar.Bey);
        var gorulenler = new HashSet<(string Olay, int Yonetim, int Halk, int Para, int Ordu)>();
        for (int i = 0; i < UstKarar; i++)
        {
            var g = oyun.Durum.Gostergeler.Degerler;
            if (donguAra && !gorulenler.Add((oyun.AktifKart.OlayKimligi, g["yonetim"], g["halk"], g["para"], g["ordu"])))
                return new(oyun.Durum.KararSayisi, oyun.Durum.Zaman.TamamlananYil, false, true);
            if (!oyun.KararVer(sec(oyun), oyun.KartKimligi) || !oyun.DevamEt())
                throw new InvalidOperationException("Simülasyonda geçerli karar reddedildi.");
            if (g.Values.Any(v => v < 0 || v > 100) || oyun.Durum.KararSayisi != i + 1 ||
                oyun.Durum.Zaman.TamamlananYil != (i + 1) / oyun.Durum.Zaman.KacKarardaBirYil)
                throw new InvalidOperationException("Gösterge sınırı, karar sayacı veya yıl oranı bozuldu.");
            var sonlar = g.Where(p => p.Value is 0 or 100).Select(p => $"{p.Key}_{p.Value}").ToHashSet();
            if (!sonlar.SetEquals(oyun.SonAnahtarlari) || (oyun.Asama == Asama.Son) != (sonlar.Count > 0))
                throw new InvalidOperationException("Simülasyonda sınır sonu atlandı veya erken tetiklendi.");
            if (oyun.Asama == Asama.Son)
                return new(oyun.Durum.KararSayisi, oyun.Durum.Zaman.TamamlananYil, true, false);
        }
        return new(oyun.Durum.KararSayisi, oyun.Durum.Zaman.TamamlananYil, false, false);
    }

    private static int Risk(OyunYoneticisi oyun, Secenek secenek) =>
        oyun.Durum.Gostergeler.Degerler.Sum(g =>
        {
            int fark = Math.Clamp(g.Value + secenek.Etki.GetValueOrDefault(g.Key), 0, 100) - 50;
            return fark * fark;
        });

    private static void Yaz(string ad, Sonuc sonuc) =>
        GD.Print($"{ad}: {sonuc.Karar} karar, {sonuc.Yil} yıl; son={sonuc.Bitis}, periyodik döngü={sonuc.Dongu}.");
}
