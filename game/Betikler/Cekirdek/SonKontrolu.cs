using System.Collections.Generic;
using System.Linq;

namespace Krallik;

public static class SonKontrolu
{
    // Ekip bir öncelik belirleyene kadar eşzamanlı sonların tümünü koru.
    // Sıra yalnız sunum içindir; hiçbir gösterge diğerini geçersiz kılmaz.
    public static IReadOnlyList<string> Bul(Gostergeler gostergeler) =>
        Bul(gostergeler.Degerler);

    public static IReadOnlyList<string> Bul(IReadOnlyDictionary<string, int> degerler) =>
        Gostergeler.Siralama.Where(k => degerler[k] is 0 or 100)
            .Select(k => $"{k}_{degerler[k]}").ToArray();
}
