using System.Collections.Generic;
using System.Linq;

namespace Krallik;

/// <summary>
/// Karakter ve mekân çizimlerinin yolları. Çizimler <c>Assets/karakterler/</c> ve <c>Assets/mekanlar/</c>
/// altındadır; dosya adı rol anahtarından farklı olabilir.
/// </summary>
public static class KarakterGorselleri
{
    public const string KarakterKlasoru = "res://Assets/karakterler/";
    public const string MekanKlasoru = "res://Assets/mekanlar/";
    public const string HaritaYolu = "res://Assets/harita/krallik_haritasi.svg";

    /// <summary>
    /// Portre anahtarı (ör. <c>hekim_kadin</c>) ya da rol anahtarı → çizim adı. Önce portre anahtarına bakılır,
    /// böylece "Çift" rollere kadın/erkek ayrı çizim eklenebilir. Bütün roller şimdilik tek çizimlidir.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Portreler = new Dictionary<string, string>
    {
        ["vezir"] = "vezir",
        ["kadi"] = "kadi",
        ["baskatip"] = "baskatip",
        ["elci"] = "elci",
        ["beylerbeyi"] = "beylerbeyi",
        ["soytari"] = "soytari",
        ["akinci_beyi"] = "akinci_beyi",
        ["kale_kumandani"] = "kale_kumandani",
        ["sinir_beyi"] = "sinir_beyi",
        ["ciftci"] = "ciftci",
        ["ozan"] = "ozan",
        ["coban"] = "coban",
        ["kervan_basi"] = "kervan_basi",
        ["demirci_usta"] = "demirci_usta",
        ["hanci"] = "hanci",
        ["tuccar"] = "tuccar",
        // Yakın karşılıklar: çizimin unvanı temadaki rolden farklı.
        ["saray_hatun"] = "valide_hatun",
        ["gozcu_basi"] = "casusbasi",
        ["koy_kethudasi"] = "koy_yaslisi",
        ["dervis"] = "kam_ata",
        ["hekim"] = "bas_hekim",
        ["hazinedar"] = "defterdar",
        ["ahi_ustasi"] = "ahi_babasi",
    };

    /// <summary>Her rolün bulunduğu mekân; mekânı gezerken orada duran karakterler buradan seçilir.</summary>
    public static readonly IReadOnlyDictionary<string, string> Mekanlar = new Dictionary<string, string>
    {
        // Yönetim
        ["vezir"] = "taht_salonu",
        ["kadi"] = "taht_salonu",
        ["baskatip"] = "taht_salonu",
        ["saray_hatun"] = "taht_salonu",
        ["elci"] = "taht_salonu",
        ["beylerbeyi"] = "taht_salonu",
        ["soytari"] = "taht_salonu",
        // Ordu
        ["akinci_beyi"] = "kale_surlari",
        ["kale_kumandani"] = "kale_surlari",
        ["gozcu_basi"] = "kale_surlari",
        ["sinir_beyi"] = "kale_surlari",
        // Halk
        ["koy_kethudasi"] = "koy_tarla",
        ["dervis"] = "koy_tarla",
        ["ciftci"] = "koy_tarla",
        ["hekim"] = "koy_tarla",
        ["ozan"] = "koy_tarla",
        ["coban"] = "koy_tarla",
        // Para
        ["hazinedar"] = "hazine_odasi",
        ["ahi_ustasi"] = "carsi",
        ["kervan_basi"] = "kervansaray",
        ["demirci_usta"] = "carsi",
        ["hanci"] = "kervansaray",
        ["tuccar"] = "carsi",
    };

    public const string VarsayilanMekan = "taht_salonu";

    /// <summary>Konuşanın portre yolu; çizimi yoksa <c>null</c>.</summary>
    public static string PortreYolu(string rol, string portreAnahtari = null) =>
        (portreAnahtari != null && Portreler.TryGetValue(portreAnahtari, out var ad)) || Portreler.TryGetValue(rol, out ad)
            ? KarakterKlasoru + ad + ".svg" : null;

    /// <summary>Rolün mekânının çizimi; listede olmayan rol taht salonundadır.</summary>
    public static string MekanYolu(string rol) =>
        MekanCizimi(Mekanlar.TryGetValue(rol, out var ad) ? ad : VarsayilanMekan);

    public static string MekanCizimi(string mekan) => MekanKlasoru + mekan + ".svg";

    /// <summary>Tahtta oturan hükümdar: Beyim ya da Hatunum.</summary>
    public static string HukumdarYolu(Hukumdar hukumdar) =>
        KarakterKlasoru + (hukumdar == Hukumdar.Hatun ? "hukumdar_hatun" : "hukumdar_bey") + ".svg";

    /// <summary>Mekânda duran roller: önce sözcü, sonra çizimi olan diğerleri, en fazla <paramref name="enFazla"/>.</summary>
    public static IReadOnlyList<string> Sakinler(string mekan, string sozcu, int enFazla = 3) =>
        new[] { sozcu }.Concat(Mekanlar.Where(m => m.Value == mekan && m.Key != sozcu).Select(m => m.Key))
            .Where(r => r != null && Portreler.ContainsKey(r)).Take(enFazla).ToArray();
}
