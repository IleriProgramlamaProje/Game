using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krallik;

/// <summary>Haritadaki bir mekân. Taht salonu dışındaki mekânlar bir göstergenin durumunu sözcüsünün ağzından anlatır.</summary>
public sealed record Mekan
{
    public const string TahtSalonuAnahtari = "taht_salonu";
    /// <summary>Durum cümlelerinin aralıkları: 0–15, 16–35, 36–64, 65–84, 85–100.</summary>
    public static readonly IReadOnlyList<int> AralikUstSinirlari = new[] { 15, 35, 64, 84, 100 };

    [JsonPropertyName("anahtar")] public string Anahtar { get; init; }
    [JsonPropertyName("ad")] public string Ad { get; init; }
    /// <summary>Çizimdeki yeri (0–1, sol üstten).</summary>
    [JsonPropertyName("harita")] public float[] Harita { get; init; }
    /// <summary>Haritadaki ad düğmesinin ortası (0–1).</summary>
    [JsonPropertyName("etiket")] public float[] Etiket { get; init; }
    [JsonPropertyName("gosterge")] public string Gosterge { get; init; }
    [JsonPropertyName("sozcu")] public string Sozcu { get; init; }
    [JsonPropertyName("durumlar")] public string[] Durumlar { get; init; }

    public bool TahtSalonu => Anahtar == TahtSalonuAnahtari;

    /// <summary>Gösterge değerinin aralığı: 0 çöküş eşiği, 2 olağan, 4 taşkın; iki uç da sonu yaklaştırır.</summary>
    public static int Aralik(int deger)
    {
        deger = Math.Clamp(deger, 0, 100);
        for (int i = 0; i < AralikUstSinirlari.Count; i++)
            if (deger <= AralikUstSinirlari[i]) return i;
        return AralikUstSinirlari.Count - 1;
    }
    public static bool Tehlikede(int deger) => Aralik(deger) is 0 or 4;
    public string Durum(int deger) => Durumlar[Aralik(deger)];
}

public static class MekanListesi
{
    private sealed record Dosya
    {
        [JsonPropertyName("mekanlar")] public Mekan[] Mekanlar { get; init; }
    }

    public static Mekan[] Oku(string json, IReadOnlySet<string> roller)
    {
        Dosya dosya;
        try { dosya = JsonSerializer.Deserialize<Dosya>(json); }
        catch (JsonException hata) { throw new FormatException("Mekân verisi okunamadı.", hata); }
        var mekanlar = dosya?.Mekanlar;
        if (mekanlar == null || mekanlar.Length == 0) throw new FormatException("Mekân listesi boş.");
        foreach (var mekan in mekanlar) Dogrula(mekan, roller);
        if (mekanlar.Count(m => m.TahtSalonu) != 1) throw new FormatException("Haritada tek bir taht salonu olmalı.");
        if (mekanlar.Select(m => m.Anahtar).Distinct().Count() != mekanlar.Length) throw new FormatException("Mekân anahtarı iki kez yazılmış.");
        return mekanlar;
    }

    private static bool Konum(float[] k) => k is { Length: 2 } && k.All(v => v is >= 0 and <= 1);

    private static void Dogrula(Mekan m, IReadOnlySet<string> roller)
    {
        if (m == null || string.IsNullOrWhiteSpace(m.Anahtar) || string.IsNullOrWhiteSpace(m.Ad) || !Konum(m.Harita) || !Konum(m.Etiket))
            throw new FormatException($"Mekân eksik: {m?.Anahtar}");
        if (m.TahtSalonu) return;
        if (!Gostergeler.Siralama.Contains(m.Gosterge)) throw new FormatException($"{m.Anahtar}: gösterge geçersiz.");
        if (m.Sozcu == null || !roller.Contains(m.Sozcu)) throw new FormatException($"{m.Anahtar}: sözcü temada yok.");
        if (m.Durumlar is not { Length: 5 } || m.Durumlar.Any(string.IsNullOrWhiteSpace))
            throw new FormatException($"{m.Anahtar}: beş durum cümlesi gerekir.");
    }
}
