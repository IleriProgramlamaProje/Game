using System;
using System.Collections.Generic;

public sealed class OturumKarakterleri
{
	private readonly Dictionary<string, string> _basliklar = new(StringComparer.Ordinal)
	{
		["elci"] = "Elçi Deniz",
		["sinir_beyi"] = "Sınır Beyi Turgut",
		["kale_kumandani"] = "Kale Kumandanı Arslan",
		["hekim"] = "Hekim Derya",
		["ciftci"] = "Çiftçi Ayşe",
		["koy_kethudasi"] = "Köy Kethüdası Hasan",
		["kervan_basi"] = "Kervan Başı Selçuk",
		["ahi_ustasi"] = "Ahi Ustası Yusuf",
		["tuccar"] = "Tüccar Zeynep",
		["hazinedar"] = "Hazinedar Yakup"
	};

	public string BasligiAl(string konusan)
	{
		if (_basliklar.TryGetValue(konusan, out var baslik))
			return baslik;

		throw new InvalidOperationException($"Konuşan için oturum adı tanımlı değil: {konusan}");
	}
}
