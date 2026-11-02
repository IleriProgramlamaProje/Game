using Godot;
using System;

public partial class KaydirmaKontrolu : PanelContainer
{
    public event Action<float> Onizleme;
    public event Action<bool> Secildi;
    public bool Etkin { get; set; }
    private int? _isaretci;
    private Vector2 _baslangic;
    private bool _yatay;
    public float Esik => Mathf.Max(48, Size.X * 0.22f);

    public override void _Ready()
    {
        GetWindow().FocusExited += Iptal;
        FocusEntered += QueueRedraw;
        FocusExited += QueueRedraw;
    }
    public override void _Draw()
    {
        if (HasFocus()) DrawRect(new Rect2(Vector2.Zero, Size), new Color(0.8f, 0.82f, 0.85f), false, 1);
    }
    public override void _ExitTree() => GetWindow().FocusExited -= Iptal;

    public override void _GuiInput(InputEvent olay)
    {
        if (!Etkin || _isaretci != null) return;
        if (olay is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } fare)
            Basla(-1, GetGlobalTransformWithCanvas() * fare.Position);
        else if (olay is InputEventScreenTouch { Pressed: true } dokunma)
            Basla(dokunma.Index, GetGlobalTransformWithCanvas() * dokunma.Position);
    }
    private void Basla(int kimlik, Vector2 konum)
    {
        _isaretci = kimlik;
        _baslangic = konum;
        _yatay = false;
    }
    public override void _Input(InputEvent olay)
    {
        if (_isaretci == null) return;
        if (!Etkin) { Iptal(); return; }
        if (_isaretci == -1)
        {
            if (olay is InputEventMouseMotion fare) Hareket(fare.Position);
            else if (olay is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } birak)
                Bitir(birak.Position, false);
        }
        else if (olay is InputEventScreenDrag surukle && surukle.Index == _isaretci)
            Hareket(surukle.Position);
        else if (olay is InputEventScreenTouch dokunma && dokunma.Index == _isaretci && !dokunma.Pressed)
            Bitir(dokunma.Position, dokunma.Canceled);
    }
    private void Hareket(Vector2 konum)
    {
        var fark = konum - _baslangic;
        if (!_yatay && Mathf.Abs(fark.Y) > 12 && Mathf.Abs(fark.Y) > Mathf.Abs(fark.X)) { Iptal(); return; }
        if (Mathf.Abs(fark.X) > 12) _yatay = true;
        Onizleme?.Invoke(Mathf.Clamp(fark.X / Esik, -1, 1));
        if (_yatay) GetViewport().SetInputAsHandled();
    }
    private void Bitir(Vector2 konum, bool iptal)
    {
        float dx = konum.X - _baslangic.X;
        bool sec = !iptal && Mathf.Abs(dx) >= Esik && Mathf.Abs(dx) > Mathf.Abs(konum.Y - _baslangic.Y);
        Iptal(); // Çağrı ekranı değiştirirse eski parmak/fare durumu kalmasın.
        GetViewport().SetInputAsHandled();
        if (sec) Secildi?.Invoke(dx > 0);
    }
    public void Iptal()
    {
        _isaretci = null;
        _yatay = false;
        Onizleme?.Invoke(0);
    }
}
