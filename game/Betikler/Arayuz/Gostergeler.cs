using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Krallik.Arayuz;

public partial class Gostergeler : VBoxContainer
{
    private readonly Dictionary<string, IGostergeBileseni> _bilesenler = new();
    private OyunDurumu _durum;
    public override void _Ready()
    {
        // Bileşenler doğrudan bu panelin altında değil, Cubuklar ızgarasının içindedir.
        // Bu nedenle yalnızca ilk seviye yerine tüm alt düğümler taranır.
        var bilesenler = FindChildren("*", "", true, false).OfType<IGostergeBileseni>().ToArray();
        foreach (var bilesen in bilesenler)
            if (!_bilesenler.TryAdd(bilesen.Anahtar, bilesen))
                throw new InvalidOperationException($"Gösterge bileşeni iki kez tanımlandı: {bilesen.Anahtar}");

        var eksikAnahtarlar = Krallik.Gostergeler.Siralama.Except(_bilesenler.Keys).ToArray();
        if (eksikAnahtarlar.Length > 0)
            throw new InvalidOperationException($"Gösterge bileşeni eksik: {string.Join(", ", eksikAnahtarlar)}");
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
        if (!_bilesenler.TryGetValue(gosterge, out var bilesen))
        {
            GD.PushWarning($"Arayüzde tanımlı olmayan gösterge güncellenmek istendi: {gosterge}");
            return;
        }

        bilesen.DegeriGoster(deger);
    }
    public override void _ExitTree()
    {
        if (_durum != null) _durum.Gostergeler.GostergeDegisti -= GostergeyiGuncelle;
    }
}
