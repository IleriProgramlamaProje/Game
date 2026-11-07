using Godot;
using Krallik;
using System;
using System.Globalization;
using System.Linq;

public partial class DesteDogrulama : Node
{
    public override void _Ready()
    {
        try
        {
            var args = OS.GetCmdlineUserArgs();
            var tema = Tema.Oku(Oku("res://Veri/tema.json"));
            var roller = tema.Karakterler.Select(r => r.Anahtar).ToHashSet(StringComparer.Ordinal);
            if (args.Length is < 1 or > 2)
                throw new FormatException("Kullanım: DesteDogrulama.tscn -- <JSON yolu> [beklenen kart sayısı]");
            var deste = new YedekDeste(Oku(args[0]), roller);
            if (args.Length == 2 &&
                (!int.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out int adet) ||
                    adet <= 0 || deste.Adet != adet))
                throw new FormatException($"Beklenen kart sayısı {args[1]}, bulunan {deste.Adet}.");
            GD.Print($"DESTE DOĞRULANDI: {deste.Adet} kart, {deste.OlayGrubuSayisi} olay grubu.");
            GetTree().Quit();
        }
        catch (Exception hata) when (hata is FormatException or InvalidOperationException)
        {
            GD.PushError(hata.Message);
            GetTree().Quit(1);
        }
    }

    private static string Oku(string yol)
    {
        using var dosya = Godot.FileAccess.Open(yol, Godot.FileAccess.ModeFlags.Read);
        if (dosya == null) throw new InvalidOperationException($"JSON dosyası açılamadı: {yol}");
        return dosya.GetAsText();
    }
}
