using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Krallik;

public sealed class Gostergeler
{
    public static readonly IReadOnlyList<string> Siralama = Array.AsReadOnly(new[] { "yonetim", "halk", "para", "ordu" });
    public static string Ad(string anahtar) => anahtar switch
    {
        "yonetim" => "Yönetim", "halk" => "Halk", "para" => "Para", "ordu" => "Ordu",
        _ => throw new ArgumentException("Bilinmeyen gösterge")
    };
    private readonly Dictionary<string, int> _degerler = Siralama.ToDictionary(x => x, _ => 50);
    public IReadOnlyDictionary<string, int> Degerler => new ReadOnlyDictionary<string, int>(_degerler);
    public event Action<string, int> GostergeDegisti;
    public bool Sinirda => _degerler.Values.Any(x => x == 0 || x == 100);

    public static void EtkiyiDogrula(IReadOnlyDictionary<string, int> etki)
    {
        if (etki == null || etki.Count < 1 || etki.Count > 3 || !etki.Values.Any(v => v < 0) ||
            etki.Any(p => !Siralama.Contains(p.Key) || p.Value is not (-15 or -10 or -5 or 5 or 10 or 15)))
            throw new FormatException("Karar 1–3 geçerli etki ve en az bir düşüş içermeli (±5/10/15).");
    }
    public void Uygula(IReadOnlyDictionary<string, int> etki)
    {
        EtkiyiDogrula(etki); // Tamamını doğrulamadan hiçbir değeri değiştirme.
        foreach (var (anahtar, fark) in etki) _degerler[anahtar] = Math.Clamp(_degerler[anahtar] + fark, 0, 100);
        foreach (var anahtar in etki.Keys) GostergeDegisti?.Invoke(anahtar, _degerler[anahtar]);
    }
}

public enum Asama { Acilis, Kart, Sonuc, Son }
public enum Hukumdar { Bey, Hatun }

public sealed class OyunDurumu
{
    public Gostergeler Gostergeler { get; } = new();
    public Hukumdar Cinsiyet { get; }
    public YilSayaci Zaman { get; }
    public int KararSayisi => Zaman.KararSayisi;
    public OyunDurumu(Hukumdar cinsiyet, int kacKarardaBirYil = 5)
    {
        Cinsiyet = cinsiyet;
        Zaman = new YilSayaci(kacKarardaBirYil);
    }
}

public sealed class OyunYoneticisi
{
    private readonly IKartKaynagi _kaynak;
    private readonly int _kacKarardaBirYil;
    private readonly List<KararKaydi> _kararlar = new();
    public Asama Asama { get; private set; } = Asama.Acilis;
    public OyunDurumu Durum { get; private set; }
    public Kart AktifKart { get; private set; }
    public Secenek SonKarar { get; private set; }
    public long KartKimligi { get; private set; }
    public IReadOnlyList<string> SonAnahtarlari { get; private set; } = Array.Empty<string>();
    public OyunYoneticisi(IKartKaynagi kaynak, int kacKarardaBirYil = 5)
    {
        _kaynak = kaynak ?? throw new ArgumentNullException(nameof(kaynak));
        if (kacKarardaBirYil <= 0) throw new ArgumentOutOfRangeException(nameof(kacKarardaBirYil));
        _kacKarardaBirYil = kacKarardaBirYil;
    }
    public void Baslat(Hukumdar cinsiyet)
    {
        if (!Enum.IsDefined(cinsiyet)) throw new ArgumentOutOfRangeException(nameof(cinsiyet));
        _kaynak.Sifirla();
        _kararlar.Clear();
        Durum = new OyunDurumu(cinsiyet, _kacKarardaBirYil);
        SonKarar = null;
        SonAnahtarlari = Array.Empty<string>();
        YeniKart();
    }
    public void AcilisaDon()
    {
        _kaynak.Sifirla();
        _kararlar.Clear();
        Durum = null;
        AktifKart = null;
        SonKarar = null;
        SonAnahtarlari = Array.Empty<string>();
        Asama = Asama.Acilis;
    }
    private void YeniKart()
    {
        AktifKart = _kaynak.Sonraki(_kararlar.Select(k => k.OlayId));
        KartKimligi++;
        Asama = Asama.Kart;
    }
    // Kimlik, eski karttan veya eski oturumdan gelen gecikmiş girdileri reddeder.
    public bool KararVer(bool sag, long kartKimligi)
    {
        if (Asama != Asama.Kart || kartKimligi != KartKimligi) return false;
        var secenek = sag ? AktifKart.Sag : AktifKart.Sol;
        Durum.Gostergeler.Uygula(secenek.Etki);
        _kararlar.Add(new KararKaydi { OlayId = AktifKart.OlayKimligi, Sag = sag, Etki = new(secenek.Etki) });
        SonKarar = secenek;
        Durum.Zaman.KarariSay();
        Asama = Asama.Sonuc;
        return true;
    }
    public bool DevamEt()
    {
        if (Asama != Asama.Sonuc) return false;
        SonAnahtarlari = SonKontrolu.Bul(Durum.Gostergeler);
        if (SonAnahtarlari.Count > 0) Asama = Asama.Son;
        else YeniKart();
        return true;
    }

    public OyunAnligi AnligiAl()
    {
        if (Durum == null) throw new InvalidOperationException("Henüz bir oturum başlamadı.");
        return new OyunAnligi
        {
            Hukumdar = Durum.Cinsiyet, Asama = Asama, AktifOlay = AktifKart.Kopyala(),
            KararSayisi = Durum.KararSayisi, Yil = Durum.Zaman.TamamlananYil,
            YilOrani = Durum.Zaman.KacKarardaBirYil, KaynakKonumu = _kaynak.Konum,
            Gostergeler = new(Durum.Gostergeler.Degerler),
            Kararlar = _kararlar.Select(k => k with { Etki = new(k.Etki) }).ToArray()
        };
    }

    public void GeriYukle(OyunAnligi kayit, IReadOnlySet<string> roller)
    {
        var durum = kayit.Dogrula(roller);
        _kaynak.KonumaGit(kayit.KaynakKonumu);
        Durum = durum;
        AktifKart = kayit.AktifOlay.Kopyala();
        Asama = kayit.Asama;
        KartKimligi++;
        _kararlar.Clear();
        _kararlar.AddRange(kayit.Kararlar.Select(k => k with { Etki = new(k.Etki) }));
        SonKarar = Asama is Asama.Sonuc or Asama.Son ? (_kararlar[^1].Sag ? AktifKart.Sag : AktifKart.Sol) : null;
        SonAnahtarlari = Asama == Asama.Son ? SonKontrolu.Bul(Durum.Gostergeler) : Array.Empty<string>();
    }
}
