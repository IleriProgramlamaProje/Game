using System;

namespace Krallik;

public sealed class YilSayaci
{
    public int KacKarardaBirYil { get; }
    public int KararSayisi { get; private set; }
    public int TamamlananYil => KararSayisi / KacKarardaBirYil;
    public int SonrakiYilaKalan => KacKarardaBirYil - KararSayisi % KacKarardaBirYil;

    public YilSayaci(int kacKarardaBirYil = 5)
    {
        if (kacKarardaBirYil <= 0) throw new ArgumentOutOfRangeException(nameof(kacKarardaBirYil));
        KacKarardaBirYil = kacKarardaBirYil;
    }

    // Yalnız kabul edilen kararlar zamanı ilerletir.
    internal void KarariSay() => KararSayisi++;
}
