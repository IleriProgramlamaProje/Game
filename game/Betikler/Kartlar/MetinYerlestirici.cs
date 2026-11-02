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
    private readonly Dictionary<string, KarakterKimligi> _kimlikler = new();
    private readonly Hukumdar _hukumdar;
    private static readonly string[] ErkekAdlari = ["Yakup", "Osman", "Turgut", "Ali", "Yunus", "Orhan", "Selim", "Murat", "Sinan", "Kemal", "İlyas", "Ömer"];
    private static readonly string[] KadinAdlari = ["Ayşe", "Elif", "Zeynep", "Selma", "Ayla", "İpek", "Suna", "Aslı", "Gül", "Dilek", "Esma", "Bahar"];
    public MetinYerlestirici(IEnumerable<Rol> roller, Hukumdar hukumdar, Random rastgele)
    {
        _hukumdar = hukumdar;
        foreach (var rol in roller)
        {
            string cinsiyet = rol.Cinsiyet == "Çift" ? (rastgele.Next(2) == 0 ? "Erkek" : "Kadın") : rol.Cinsiyet;
            var adlar = cinsiyet == "Erkek" ? ErkekAdlari : KadinAdlari;
            _kimlikler.Add(rol.Anahtar, new(rol.Unvan, adlar[rastgele.Next(adlar.Length)], cinsiyet,
                rol.Anahtar + (rol.Cinsiyet == "Çift" ? (cinsiyet == "Erkek" ? "_erkek" : "_kadin") : "")));
        }
    }
    public KarakterKimligi Kimlik(string rol) => _kimlikler[rol];
    public string Baslik(string rol) => $"{Kimlik(rol).Unvan} {Kimlik(rol).Isim}";
    public string Yerlestir(string metin) => metin.Replace("{hitap}", _hukumdar == Hukumdar.Bey ? "Beyim" : "Hatunum");
}
