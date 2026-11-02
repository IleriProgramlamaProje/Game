using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krallik;

public sealed record Secenek
{
    [JsonPropertyName("etiket")] public string Etiket { get; init; }
    [JsonPropertyName("etki")] public Dictionary<string, int> Etki { get; init; }
    [JsonPropertyName("sonuc")] public string Sonuc { get; init; }
}

public sealed record Kart
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Id { get; init; }
    [JsonPropertyName("konusan")] public string Konusan { get; init; }
    [JsonPropertyName("konu")] public string Konu { get; init; }
    [JsonPropertyName("metin")] public string Metin { get; init; }
    [JsonPropertyName("sol")] public Secenek Sol { get; init; }
    [JsonPropertyName("sag")] public Secenek Sag { get; init; }
    [JsonPropertyName("kaynak")] public string Kaynak { get; init; }
    [JsonPropertyName("lisans")] public string Lisans { get; init; }

    public void Dogrula(IReadOnlySet<string> roller)
    {
        if (string.IsNullOrWhiteSpace(Konusan) || !roller.Contains(Konusan) ||
            string.IsNullOrWhiteSpace(Konu) || string.IsNullOrWhiteSpace(Metin) ||
            string.IsNullOrWhiteSpace(Lisans) || !new[] { "model", "altin_set", "ogretmen" }.Contains(Kaynak))
            throw new FormatException("Kartın rol, konu, metin, kaynak veya lisans alanı geçersiz.");
        int kelime = Metin.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
        if (kelime < 20 || kelime > 50)
            throw new FormatException("Kart metni 20–50 kelime olmalı.");
        foreach (var secenek in new[] { Sol, Sag })
        {
            if (secenek == null || string.IsNullOrWhiteSpace(secenek.Etiket) || string.IsNullOrWhiteSpace(secenek.Sonuc))
                throw new FormatException("Kartın iki kararı ve sonuç cümleleri dolu olmalı.");
            int uzunluk = secenek.Etiket.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
            if (uzunluk < 2 || uzunluk > 5) throw new FormatException("Kararlar 2–5 kelime olmalı.");
            Gostergeler.EtkiyiDogrula(secenek.Etki);
        }
    }
}
