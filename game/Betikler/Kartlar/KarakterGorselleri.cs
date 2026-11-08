using System.Collections.Generic;

namespace Krallik;

/// <summary>
/// Kartın üstündeki sahne: konuşanın portresi ve bulunduğu mekân.
/// Çizimler <c>Assets/karakterler/</c> ve <c>Assets/mekanlar/</c> altındadır; dosya adı rol anahtarından farklı olabilir.
/// </summary>
public static class KarakterGorselleri
{
    public const string KarakterKlasoru = "res://Assets/karakterler/";
    public const string MekanKlasoru = "res://Assets/mekanlar/";

    /// <summary>
    /// Portre anahtarı (ör. <c>hekim_kadin</c>) ya da rol anahtarı → çizim adı. Önce portre anahtarına bakılır,
    /// böylece "Çift" rollere kadın/erkek ayrı çizim eklenebilir. Listede olmayan rol portresiz gösterilir.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Portreler = new Dictionary<string, string>
    {
        ["vezir"] = "vezir",
        ["kadi"] = "kadi",
        ["elci"] = "elci",
        ["akinci_beyi"] = "akinci_beyi",
        ["kervan_basi"] = "kervan_basi",
        // Yakın karşılıklar: çizimin unvanı temadaki rolden farklı.
        ["saray_hatun"] = "valide_hatun",
        ["gozcu_basi"] = "casusbasi",
        ["koy_kethudasi"] = "koy_yaslisi",
        ["dervis"] = "kam_ata",
        ["hekim"] = "bas_hekim",
        ["hazinedar"] = "defterdar",
        ["ahi_ustasi"] = "ahi_babasi",
    };

    /// <summary>Her rolün kartta göründüğü mekân. Alanına göre seçilir; birkaç rolün kendi yeri vardır.</summary>
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

    /// <summary>Konuşanın mekân yolu; listede olmayan rol taht salonunda görünür.</summary>
    public static string MekanYolu(string rol) =>
        MekanKlasoru + (Mekanlar.TryGetValue(rol, out var ad) ? ad : VarsayilanMekan) + ".svg";
}
