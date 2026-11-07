using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krallik;

public sealed record Rol
{
    [JsonPropertyName("anahtar")] public string Anahtar { get; init; }
    [JsonPropertyName("unvan")] public string Unvan { get; init; }
    [JsonPropertyName("cinsiyet")] public string Cinsiyet { get; init; }
}
public sealed record Tema
{
    [JsonPropertyName("acilis")] public string Acilis { get; init; }
    [JsonPropertyName("karakterler")] public Rol[] Karakterler { get; init; }
    [JsonPropertyName("sonlar")] public Dictionary<string, SonAnlatimi> Sonlar { get; init; }
    [JsonPropertyName("yil")] public YilAyarlari Yil { get; init; }
    public static Tema Oku(string json)
    {
        try
        {
            var tema = JsonSerializer.Deserialize<Tema>(json);
            if (tema == null || string.IsNullOrWhiteSpace(tema.Acilis) || tema.Karakterler == null || tema.Karakterler.Length == 0 ||
                tema.Karakterler.Any(r => r == null || string.IsNullOrWhiteSpace(r.Anahtar) || string.IsNullOrWhiteSpace(r.Unvan) ||
                    r.Cinsiyet is not ("Erkek" or "Kadın" or "Çift")) ||
                tema.Karakterler.Select(r => r.Anahtar).Distinct().Count() != tema.Karakterler.Length)
                throw new FormatException("Tema açılışı veya karakter listesi geçersiz.");
            if (tema.Yil == null || tema.Yil.KacKarttaBirYil <= 0)
                throw new FormatException("Tema yıl ayarı pozitif bir karar sayısı olmalı.");
            foreach (string anahtar in Gostergeler.Siralama.SelectMany(k => new[] { k + "_0", k + "_100" }))
                if (tema.Sonlar == null || !tema.Sonlar.TryGetValue(anahtar, out var son) || son == null ||
                    string.IsNullOrWhiteSpace(son.Baslik) || string.IsNullOrWhiteSpace(son.Metin))
                    throw new FormatException($"Tema son anlatımı eksik veya boş: {anahtar}");
            return tema;
        }
        catch (JsonException hata) { throw new FormatException("Tema JSON biçimi okunamadı.", hata); }
    }
}
public sealed record SonAnlatimi
{
    [JsonPropertyName("baslik")] public string Baslik { get; init; }
    [JsonPropertyName("metin")] public string Metin { get; init; }
}
public sealed record YilAyarlari
{
    [JsonPropertyName("kac_kartta_bir_yil")] public int KacKarttaBirYil { get; init; }
}
public sealed record KarakterKimligi(string Unvan, string Isim, string Cinsiyet, string PortreAnahtari);

public sealed class MetinYerlestirici
{
    private readonly OturumKarakterleri _karakterler;
    private readonly Hukumdar _hukumdar;
    public MetinYerlestirici(IEnumerable<Rol> roller, Hukumdar hukumdar, Random rastgele)
    {
        _karakterler = new OturumKarakterleri(roller, rastgele);
        _hukumdar = hukumdar;
    }
    public MetinYerlestirici(IEnumerable<Rol> roller, Hukumdar hukumdar,
        IReadOnlyDictionary<string, KarakterKimligi> kimlikler)
    {
        _karakterler = new OturumKarakterleri(roller, kimlikler);
        _hukumdar = hukumdar;
    }
    public Dictionary<string, KarakterKimligi> KarakterKaydi() => _karakterler.KaydiAl();
    public KarakterKimligi Kimlik(string rol) => _karakterler.Kimlik(rol);
    public string Baslik(string rol) => _karakterler.Baslik(rol);
    public static string Yerlestir(string metin, string hitap) => metin.Replace("{hitap}", hitap);
    public string Yerlestir(string metin) => Yerlestir(metin, _hukumdar == Hukumdar.Bey ? "Beyim" : "Hatunum");
}
