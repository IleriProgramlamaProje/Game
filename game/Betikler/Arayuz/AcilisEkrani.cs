using Godot;
using Krallik;
using System;

public partial class AcilisEkrani : VBoxContainer
{
    private OptionButton _cinsiyet;
    public event Action<Hukumdar> TahtaCikildi;
    public Hukumdar SecilenCinsiyet => _cinsiyet.Selected == 0 ? Hukumdar.Bey : Hukumdar.Hatun;
    public override void _Ready()
    {
        _cinsiyet = GetNode<OptionButton>("%Cinsiyet");
        GetNode<Button>("%Baslat").Pressed += () => TahtaCikildi?.Invoke(SecilenCinsiyet);
    }
    public void Sifirla() => _cinsiyet.Select(0);
}
