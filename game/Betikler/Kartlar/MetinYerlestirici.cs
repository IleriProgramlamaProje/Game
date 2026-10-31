public static class MetinYerlestirici
{
	public static string Yerlestir(string metin, string hitap)
	{
		return metin.Replace("{hitap}", hitap);
	}
}
