using Godot;

public partial class AltEkran : MenuSahnesi
{
    [Export] public string GeriDonusSahnesi = "res://Sahneler/AnaMenu.tscn";
    [Export] public string DevamSahnesi = "";

    protected override string VarsayilanOdak => "Geri";

    protected override void BaglantilariKur()
    {
        var geri = DugmeBul("Geri");
        if (geri != null) geri.Pressed += GeriDon;
        var devam = DugmeBul("Devam");
        if (devam != null && !string.IsNullOrWhiteSpace(DevamSahnesi))
            devam.Pressed += DevamEt;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GeriDon();
            GetViewport().SetInputAsHandled();
        }
    }

    private void GeriDon() => SahneyeGit(GeriDonusSahnesi);
    private void DevamEt() => SahneyeGit(DevamSahnesi);
}
