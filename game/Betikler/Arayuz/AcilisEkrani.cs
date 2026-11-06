using Godot;
using Krallik;
using System;

public partial class AcilisEkrani : VBoxContainer
{
    public event Action<Hukumdar> TahtaCikildi;

    public override void _Ready()
    {
        GetNode<Button>("%Beyim").Pressed += () => HukumdarSec(Hukumdar.Bey);
        GetNode<Button>("%Hatunum").Pressed += () => HukumdarSec(Hukumdar.Hatun);
    }

    public void EtkinligiAyarla(bool etkin)
    {
        GetNode<Button>("%Beyim").Disabled = !etkin;
        GetNode<Button>("%Hatunum").Disabled = !etkin;
    }

    private void HukumdarSec(Hukumdar hukumdar) => TahtaCikildi?.Invoke(hukumdar);
}
