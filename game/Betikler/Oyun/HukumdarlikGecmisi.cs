using System.Collections.Generic;
using System.Linq;

namespace Krallik;

public sealed class HukumdarlikGecmisi
{
    public IReadOnlyList<OturumKaydi> Tamamlananlar { get; }
    public OturumKaydi Rekor { get; }

    public HukumdarlikGecmisi(IEnumerable<OturumKaydi> kayitlar)
    {
        Tamamlananlar = kayitlar.Where(k => k.Oyun.Asama == Asama.Son)
            .OrderByDescending(k => k.KayitZamani).ThenBy(k => k.Id).ToArray();
        Rekor = Tamamlananlar.OrderByDescending(k => k.Oyun.Yil)
            .ThenByDescending(k => k.Oyun.KararSayisi)
            .ThenBy(k => k.KayitZamani).ThenBy(k => k.Id).FirstOrDefault();
    }
}
