using System.Collections.Generic;
using System.Linq;

namespace Krallik;

public static class SonKontrolu
{
    // Ekip bir öncelik belirleyene kadar eşzamanlı sonların tümünü koru.
    // Sıra yalnız sunum içindir; hiçbir gösterge diğerini geçersiz kılmaz.
    public static IReadOnlyList<string> Bul(Gostergeler gostergeler) =>
        Gostergeler.Siralama.Where(k => gostergeler.Degerler[k] is 0 or 100)
            .Select(k => $"{k}_{gostergeler.Degerler[k]}").ToArray();
}
