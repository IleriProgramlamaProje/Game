using Godot;
using System;
using System.Linq;

namespace Krallik.Arayuz;

/// <summary>
/// Tek bir oyun göstergesinin (etiket + ilerleme çubuğu) ortak Godot uygulaması.
/// </summary>
public partial class GostergeBileseni : VBoxContainer, IGostergeBileseni
{
    [Export] public string Anahtar { get; set; } = string.Empty;
    [Export] public NodePath EtiketYolu { get; set; }
    [Export] public NodePath CubukYolu { get; set; }

    private Label _etiket;
    private ProgressBar _cubuk;

    public override void _Ready()
    {
        if (!Krallik.Gostergeler.Siralama.Contains(Anahtar))
            throw new InvalidOperationException($"Bilinmeyen gösterge anahtarı: {Anahtar}");

        _etiket = GetNode<Label>(EtiketYolu);
        _cubuk = GetNode<ProgressBar>(CubukYolu);
    }

    public void DegeriGoster(int deger)
    {
        if (deger is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(deger), "Gösterge değeri 0 ile 100 arasında olmalı.");

        _cubuk.Value = deger;
        _etiket.Text = $"{Krallik.Gostergeler.Ad(Anahtar)}  {deger}";
    }
}
