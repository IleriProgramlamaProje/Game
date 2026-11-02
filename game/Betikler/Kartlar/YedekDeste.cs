using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krallik;

public sealed class YedekDeste : IKartKaynagi
{
    private readonly Kart[] _kartlar;
    private int _sira;
    public int Adet => _kartlar.Length;
    public YedekDeste(string json, IReadOnlySet<string> roller)
    {
        _kartlar = MetindenYukle(json, roller);
    }
    public static Kart[] MetindenYukle(string json, IReadOnlySet<string> roller)
    {
        try
        {
            using var belge = JsonDocument.Parse(json);
            var kok = belge.RootElement;
            // Ömer'in H1 destesinin kartlar kapsayıcısını da koru.
            if (kok.ValueKind == JsonValueKind.Object && kok.TryGetProperty("kartlar", out var kartlar)) kok = kartlar;
            var sonuc = JsonSerializer.Deserialize<Kart[]>(kok.GetRawText(), new JsonSerializerOptions
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            });
            if (sonuc == null || sonuc.Length == 0) throw new FormatException("Deste boş olamaz.");
            foreach (var kart in sonuc)
            {
                if (kart == null) throw new FormatException("Deste boş kart içeriyor.");
                kart.Dogrula(roller);
            }
            return sonuc;
        }
        catch (JsonException hata) { throw new FormatException("Deste JSON biçimi okunamadı.", hata); }
    }
    public IReadOnlyList<Kart> KartlariYukle() => Array.AsReadOnly(_kartlar);
    public Kart Sonraki()
    {
        var kart = _kartlar[_sira];
        _sira = (_sira + 1) % _kartlar.Length;
        return kart;
    }
    public void Sifirla() => _sira = 0;
}
