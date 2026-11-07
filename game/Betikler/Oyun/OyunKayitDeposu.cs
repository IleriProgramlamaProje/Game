using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krallik;

public sealed record OturumKaydi
{
    public const int GuncelSurum = 1;
    public required int Surum { get; init; }
    public required Guid Id { get; init; }
    public required DateTimeOffset KayitZamani { get; init; }
    public required OyunAnligi Oyun { get; init; }
    public required Dictionary<string, KarakterKimligi> Karakterler { get; init; }

    internal void Dogrula(Guid id, Tema tema)
    {
        if (Surum != GuncelSurum || Id == Guid.Empty || Id != id || KayitZamani == default || Oyun == null)
            throw new FormatException("Kayıt sürümü veya oturum kimliği geçersiz.");
        Oyun.Dogrula(tema.Karakterler.Select(r => r.Anahtar).ToHashSet(StringComparer.Ordinal));
        _ = new OturumKarakterleri(tema.Karakterler, Karakterler);
    }
}

public sealed class OyunKayitDeposu(string dizin)
{
    private static readonly JsonSerializerOptions Bicim = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public void Kaydet(OturumKaydi kayit)
    {
        if (kayit.Id == Guid.Empty || kayit.Surum != OturumKaydi.GuncelSurum)
            throw new ArgumentException("Kayıt kimliği veya sürümü geçersiz.", nameof(kayit));
        Directory.CreateDirectory(dizin);
        string hedef = DosyaYolu(kayit.Id);
        string gecici = hedef + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var akim = new FileStream(gecici, FileMode.CreateNew, System.IO.FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(akim, kayit, Bicim);
                akim.Flush(flushToDisk: true);
            }
            File.Move(gecici, hedef, overwrite: true);
        }
        finally
        {
            // Başarısız yazma önceki kaydı korur. Artık bir geçici dosya varsa listeye girmez.
            try { File.Delete(gecici); }
            catch (Exception hata) when (hata is IOException or UnauthorizedAccessException) { }
        }
    }

    public OturumKaydi Yukle(Guid id, Tema tema)
    {
        try
        {
            using var akim = File.OpenRead(DosyaYolu(id));
            var kayit = JsonSerializer.Deserialize<OturumKaydi>(akim, Bicim)
                ?? throw new FormatException("Kayıt boş.");
            kayit.Dogrula(id, tema);
            return kayit;
        }
        catch (JsonException hata) { throw new FormatException("Kayıt JSON biçimi okunamadı.", hata); }
    }

    public (IReadOnlyList<OturumKaydi> Kayitlar, int Bozuk) Listele(Tema tema)
    {
        var kayitlar = new List<OturumKaydi>();
        int bozuk = 0;
        if (Directory.Exists(dizin))
        {
            foreach (string yol in Directory.EnumerateFiles(dizin, "*.json"))
            {
                try
                {
                    if (!Guid.TryParseExact(Path.GetFileNameWithoutExtension(yol), "N", out var id))
                        throw new FormatException("Kayıt dosyasının adı geçersiz.");
                    kayitlar.Add(Yukle(id, tema));
                }
                catch (Exception hata) when (hata is FormatException or IOException or UnauthorizedAccessException)
                {
                    bozuk++;
                }
            }
        }
        return (kayitlar.OrderByDescending(k => k.KayitZamani).ToArray(), bozuk);
    }

    private string DosyaYolu(Guid id) => Path.Combine(dizin, id.ToString("N") + ".json");
}
