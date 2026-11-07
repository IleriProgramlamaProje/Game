using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Krallik;

/// <summary>Hazır bir dış yanıt varsa doğrular. Ağ işlemi yapmaz, yanıt beklemez.</summary>
public sealed class YedekliKartKaynagi : IKartKaynagi
{
    private readonly IKartKaynagi _yerel;
    private readonly Func<string> _hazirYanitiAl;
    private readonly HashSet<string> _roller;

    public YedekliKartKaynagi(IKartKaynagi yerel, Func<string> hazirYanitiAl, IReadOnlySet<string> roller)
    {
        _yerel = yerel ?? throw new ArgumentNullException(nameof(yerel));
        _hazirYanitiAl = hazirYanitiAl ?? throw new ArgumentNullException(nameof(hazirYanitiAl));
        _roller = new HashSet<string>(roller, StringComparer.Ordinal);
    }

    // Yerel sayaç yalnız yedeğe düşüldüğünde ilerler ve oturum kaydından geri alınabilir.
    public int Konum => _yerel.Konum;
    public void KonumaGit(int konum) => _yerel.KonumaGit(konum);
    public void Sifirla() => _yerel.Sifirla();
    public Kart Sonraki() => Sonraki(Array.Empty<string>());

    public Kart Sonraki(IEnumerable<string> kullanilmisOlaylar)
    {
        try
        {
            string json = _hazirYanitiAl();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var kart = YedekDeste.MetindenOlayYukle(json, _roller);
                if (kart.Kaynak == "model" && !kullanilmisOlaylar.Contains(kart.OlayKimligi, StringComparer.Ordinal))
                    return kart;
            }
        }
        catch (Exception hata) when (hata is FormatException or IOException or HttpRequestException or TimeoutException)
        {
            // Başarısız veya geçersiz dış olay, güvenilir yerel sırayı kesmez.
        }
        return _yerel.Sonraki(kullanilmisOlaylar);
    }
}
