using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public sealed class YedekDeste : IKartKaynagi
{
	public const string VarsayilanYol = "res://Veri/yedek_desta.json";

	private static readonly HashSet<string> GecerliGostergeler =
		new(StringComparer.Ordinal) { "yonetim", "halk", "para", "ordu" };

	private readonly string _yol;

	public YedekDeste(string yol = VarsayilanYol)
	{
		_yol = yol;
	}

	public IReadOnlyList<Kart> KartlariYukle()
	{
		if (!FileAccess.FileExists(_yol))
			throw new InvalidOperationException($"Yedek deste bulunamadı: {_yol}");

		return MetindenYukle(FileAccess.GetFileAsString(_yol));
	}

	public static IReadOnlyList<Kart> MetindenYukle(string json)
	{
		try
		{
			var deste = JsonSerializer.Deserialize<DesteVerisi>(json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});
			if (deste?.Kartlar == null)
				throw new InvalidOperationException("Yedek deste 'kartlar' listesini içermiyor.");

			if (deste.Kartlar.Count < 10)
				throw new InvalidOperationException("Yedek destede en az 10 kart bulunmalı.");

			foreach (var kart in deste.Kartlar)
				KartıDogrula(kart);

			return deste.Kartlar;
		}
		catch (JsonException hata)
		{
			throw new InvalidOperationException("Yedek deste JSON biçimi bozuk.", hata);
		}
	}

	private static void KartıDogrula(Kart kart)
	{
		if (string.IsNullOrWhiteSpace(kart.Id) || string.IsNullOrWhiteSpace(kart.Konusan) ||
			string.IsNullOrWhiteSpace(kart.Konu) || string.IsNullOrWhiteSpace(kart.Metin))
			throw new InvalidOperationException("Kartın kimliği, konuşanı, konusu ve metni zorunludur.");

		int kelimeSayisi = kart.Metin.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
		if (kelimeSayisi < 20 || kelimeSayisi > 50)
			throw new InvalidOperationException($"{kart.Id}: kart metni 20–50 kelime olmalı.");

		SecenegiDogrula(kart.Id, kart.Sol);
		SecenegiDogrula(kart.Id, kart.Sag);
	}

	private static void SecenegiDogrula(string kartId, Secenek secenek)
	{
		int etiketKelimeSayisi = secenek.Etiket.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
		if (etiketKelimeSayisi < 2 || etiketKelimeSayisi > 5)
			throw new InvalidOperationException($"{kartId}: seçenek etiketi 2–5 kelime olmalı.");
		if (string.IsNullOrWhiteSpace(secenek.Sonuc))
			throw new InvalidOperationException($"{kartId}: seçenek sonucu boş olamaz.");
		if (secenek.Etki.Count < 1 || secenek.Etki.Count > 3 || !secenek.Etki.Values.Any(etki => etki < 0))
			throw new InvalidOperationException($"{kartId}: seçenek 1–3 etki içermeli ve en az biri negatif olmalı.");

		foreach (var (gosterge, etki) in secenek.Etki)
		{
			if (!GecerliGostergeler.Contains(gosterge) || Math.Abs(etki) is not (5 or 10 or 15))
				throw new InvalidOperationException($"{kartId}: geçersiz gösterge veya etki değeri.");
		}
	}

	private sealed class DesteVerisi
	{
		public List<Kart> Kartlar { get; set; } = new();
	}
}
