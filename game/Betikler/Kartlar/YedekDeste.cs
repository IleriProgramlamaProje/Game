using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krallik;

public sealed class YedekDeste : IKartKaynagi
{
    private readonly Kart[] _kartlar;
    private readonly Kart[][] _gruplar;
    private int _sira;
    public int Adet => _kartlar.Length;
    public int OlayGrubuSayisi => _gruplar.Length;
    public int Konum => _sira;
    private static readonly JsonSerializerOptions Bicim = new()
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public YedekDeste(string json, IReadOnlySet<string> roller)
    {
        _kartlar = MetindenYukle(json, roller);
        _gruplar = _kartlar.GroupBy(k => k.OlayGrubu == null ? (false, k.OlayKimligi) : (true, k.OlayGrubu))
            .Select(g => g.ToArray()).ToArray();
    }
    public static Kart[] MetindenYukle(string json, IReadOnlySet<string> roller)
    {
        try
        {
            using var belge = JsonDocument.Parse(json);
            var kok = belge.RootElement;
            TekrarlananAlanlariDogrula(kok);
            // Ömer'in H1 destesinin kartlar kapsayıcısını da koru.
            if (kok.ValueKind == JsonValueKind.Object && kok.TryGetProperty("kartlar", out var kartlar)) kok = kartlar;
            var sonuc = kok.Deserialize<Kart[]>(Bicim);
            if (sonuc == null || sonuc.Length == 0) throw new FormatException("Deste boş olamaz.");
            var kimlikler = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < sonuc.Length; i++)
            {
                try
                {
                    var kart = sonuc[i] ?? throw new FormatException("Kart boş.");
                    kart.Dogrula(roller);
                    if (!kimlikler.Add(kart.OlayKimligi)) throw new FormatException($"Yinelenen kart kimliği: {kart.OlayKimligi}");
                }
                catch (FormatException hata)
                {
                    throw new FormatException($"Deste kartı {i + 1}: {hata.Message}", hata);
                }
            }
            return sonuc;
        }
        catch (JsonException hata) { throw new FormatException("Deste JSON biçimi okunamadı.", hata); }
    }

    public static Kart MetindenOlayYukle(string json, IReadOnlySet<string> roller)
    {
        try
        {
            using var belge = JsonDocument.Parse(json);
            TekrarlananAlanlariDogrula(belge.RootElement);
            var kart = belge.RootElement.Deserialize<Kart>(Bicim) ?? throw new FormatException("Olay boş.");
            kart.Dogrula(roller);
            return kart;
        }
        catch (JsonException hata) { throw new FormatException("Olay JSON biçimi okunamadı.", hata); }
    }

    internal static void TekrarlananAlanlariDogrula(JsonElement deger)
    {
        if (deger.ValueKind == JsonValueKind.Object)
        {
            var alanlar = new HashSet<string>(StringComparer.Ordinal);
            foreach (var alan in deger.EnumerateObject())
            {
                if (!alanlar.Add(alan.Name)) throw new FormatException($"Yinelenen JSON alanı: {alan.Name}");
                TekrarlananAlanlariDogrula(alan.Value);
            }
        }
        else if (deger.ValueKind == JsonValueKind.Array)
        {
            foreach (var oge in deger.EnumerateArray()) TekrarlananAlanlariDogrula(oge);
        }
    }

    public IReadOnlyList<Kart> KartlariYukle() => Array.AsReadOnly(_kartlar);
    public Kart Sonraki()
    {
        // Her olay grubundan tur başına bir kart. Sonraki tur aynı olayın başka varyantını seçer.
        var grup = _gruplar[_sira % _gruplar.Length];
        int varyant = (_sira / _gruplar.Length) % grup.Length;
        _sira = _sira == int.MaxValue ? 0 : _sira + 1;
        return grup[varyant];
    }
    public void Sifirla() => _sira = 0;
    public void KonumaGit(int konum)
    {
        if (konum < 0) throw new ArgumentOutOfRangeException(nameof(konum));
        _sira = konum;
    }
}
