using Godot;
using System.Collections.Generic;

namespace Krallik.Arayuz;

public partial class Gostergeler : VBoxContainer
{
    private readonly Dictionary<string, ProgressBar> _cubuklar = new();
    private readonly Dictionary<string, Label> _etiketler = new();
    private OyunDurumu _durum;
    public override void _Ready()
    {
        foreach (string gosterge in Krallik.Gostergeler.Siralama)
        {
            _cubuklar.Add(gosterge, GetNode<ProgressBar>("%" + gosterge + "Cubuk"));
            _etiketler.Add(gosterge, GetNode<Label>("%" + gosterge + "Deger"));
        }
    }
    public void DurumuGoster(OyunDurumu durum)
    {
        if (_durum != null) _durum.Gostergeler.GostergeDegisti -= GostergeyiGuncelle;
        _durum = durum;
        _durum.Gostergeler.GostergeDegisti += GostergeyiGuncelle;
        foreach (var (gosterge, deger) in durum.Gostergeler.Degerler) GostergeyiGuncelle(gosterge, deger);
    }
    private void GostergeyiGuncelle(string gosterge, int deger)
    {
        _cubuklar[gosterge].Value = deger;
        _etiketler[gosterge].Text = $"{Krallik.Gostergeler.Ad(gosterge)}  {deger}";
    }
    public override void _ExitTree()
    {
        if (_durum != null) _durum.Gostergeler.GostergeDegisti -= GostergeyiGuncelle;
    }
}
