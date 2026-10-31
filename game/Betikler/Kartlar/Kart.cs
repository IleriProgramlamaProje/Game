using System.Collections.Generic;

public sealed class Kart
{
	public string Id { get; set; } = "";
	public string Konusan { get; set; } = "";
	public string Konu { get; set; } = "";
	public string Metin { get; set; } = "";
	public Secenek Sol { get; set; } = new();
	public Secenek Sag { get; set; } = new();
	public string Kaynak { get; set; } = "";
	public string Lisans { get; set; } = "";
}

public sealed class Secenek
{
	public string Etiket { get; set; } = "";
	public Dictionary<string, int> Etki { get; set; } = new();
	public string Sonuc { get; set; } = "";
}
