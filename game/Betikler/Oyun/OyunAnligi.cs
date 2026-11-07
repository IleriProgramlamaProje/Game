using System;
using System.Collections.Generic;
using System.Linq;

namespace Krallik;

public sealed record KararKaydi
{
    public required string OlayId { get; init; }
    public required bool Sag { get; init; }
    public required Dictionary<string, int> Etki { get; init; }
}

public sealed record OyunAnligi
{
    public required Hukumdar Hukumdar { get; init; }
    public required Asama Asama { get; init; }
    public required int KararSayisi { get; init; }
    public required int Yil { get; init; }
    public required int YilOrani { get; init; }
    public required Dictionary<string, int> Gostergeler { get; init; }
    public required Kart AktifOlay { get; init; }
    public required int KaynakKonumu { get; init; }
    public required KararKaydi[] Kararlar { get; init; }

    internal OyunDurumu Dogrula(IReadOnlySet<string> roller)
    {
        if (!Enum.IsDefined(Hukumdar) || Asama is not (Asama.Kart or Asama.Sonuc or Asama.Son) ||
            Kararlar == null || Kararlar.Length != KararSayisi || YilOrani <= 0 ||
            Yil != KararSayisi / YilOrani || KaynakKonumu < 0 || AktifOlay == null ||
            Gostergeler == null || Gostergeler.Count != Krallik.Gostergeler.Siralama.Count)
            throw new FormatException("Oturum kaydı eksik veya tutarsız.");

        AktifOlay.Dogrula(roller);
        var durum = new OyunDurumu(Hukumdar, YilOrani);
        foreach (var karar in Kararlar)
        {
            if (karar == null || !Kart.GecerliKimlik(karar.OlayId) || durum.Gostergeler.Sinirda)
                throw new FormatException("Karar geçmişi geçersiz.");
            durum.Gostergeler.Uygula(karar.Etki);
            durum.Zaman.KarariSay();
        }
        if (durum.Gostergeler.Degerler.Any(p => !Gostergeler.TryGetValue(p.Key, out var deger) || p.Value != deger) ||
            (Asama == Asama.Kart && durum.Gostergeler.Sinirda) ||
            (Asama == Asama.Son && !durum.Gostergeler.Sinirda))
            throw new FormatException("Kayıttaki gösterge veya oyun aşaması geçmişle uyuşmuyor.");

        if (Asama is Asama.Sonuc or Asama.Son)
        {
            var son = Kararlar.LastOrDefault();
            var secenek = son?.Sag == true ? AktifOlay.Sag : AktifOlay.Sol;
            if (son == null || son.OlayId != AktifOlay.OlayKimligi || son.Etki.Count != secenek.Etki.Count ||
                son.Etki.Any(p => !secenek.Etki.TryGetValue(p.Key, out int etki) || p.Value != etki))
                throw new FormatException("Kayıttaki bekleyen sonuç son kararla uyuşmuyor.");
        }
        return durum;
    }
}
