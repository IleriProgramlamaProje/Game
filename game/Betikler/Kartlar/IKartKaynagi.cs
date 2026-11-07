using System.Collections.Generic;

namespace Krallik;

public interface IKartKaynagi
{
    Kart Sonraki();
    Kart Sonraki(IEnumerable<string> kullanilmisOlaylar) => Sonraki();
    void Sifirla();
    int Konum { get; }
    void KonumaGit(int konum);
}
