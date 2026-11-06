using Godot;
using System;

public enum KartTarafi { Sol, Sag }

public partial class KaydirmaKontrolu : PanelContainer
{
    public event Action<float> Onizleme;
    public event Action<KartTarafi> KararOnaylandi;
    public bool Etkin { get; set; }
    private int? _isaretci;
    private Vector2 _baslangic;
    private Vector2 _merkez;
    private bool _yatay;
    private Tween _geriDonus;
    private bool _onaylaniyor;
    public const float AzamiEgim = 5;
    public float Esik => Mathf.Max(24, Size.X * 0.11f);

    public override void _Ready()
    {
        _merkez = Position;
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
        if (!Etkin || _onaylaniyor || _isaretci != null) return;
        if (olay is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } fare)
            Basla(-1, GetGlobalTransformWithCanvas() * fare.Position);
        else if (olay is InputEventScreenTouch { Pressed: true } dokunma)
            Basla(dokunma.Index, GetGlobalTransformWithCanvas() * dokunma.Position);
    }
    private void Basla(int kimlik, Vector2 konum)
    {
        _geriDonus?.Kill();
        PivotOffset = Size / 2;
        Position = _merkez;
        RotationDegrees = 0;
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
        if (!_yatay) return;
        float mesafe = Mathf.Clamp(fark.X, -Esik * 1.5f, Esik * 1.5f);
        Position = _merkez + new Vector2(mesafe, 0);
        float oran = mesafe / Esik;
        RotationDegrees = oran / 1.5f * AzamiEgim;
        Onizleme?.Invoke(oran);
        if (_yatay) GetViewport().SetInputAsHandled();
    }
    private KartTarafi? Bitir(Vector2 konum, bool iptal)
    {
        if (_isaretci == null || !Etkin) return null;
        float dx = konum.X - _baslangic.X;
        bool sec = !iptal && Mathf.Abs(dx) >= Esik && Mathf.Abs(dx) > Mathf.Abs(konum.Y - _baslangic.Y);
        GetViewport().SetInputAsHandled();
        if (!sec) { Iptal(); return null; }
        var taraf = dx > 0 ? KartTarafi.Sag : KartTarafi.Sol;
        _isaretci = null;
        _yatay = false;
        _onaylaniyor = true;
        _geriDonus?.Kill();
        var yon = dx > 0 ? 1 : -1;
        _geriDonus = CreateTween();
        _geriDonus.TweenProperty(this, "position:x", Position.X + yon * GetViewportRect().Size.X, 0.2);
        _geriDonus.Chain().TweenCallback(Callable.From(() =>
        {
            _onaylaniyor = false;
            if (Etkin) KararOnaylandi?.Invoke(taraf);
        }));
        return taraf;
    }
    // Programatik kullanım ve bileşen testleri de gerçek girişle aynı yolu izler.
    public void SuruklemeyiBaslat(float x)
    {
        if (Etkin && !_onaylaniyor && _isaretci == null) Basla(-1, new Vector2(x, 0));
    }
    public void SuruklemeyiGuncelle(float x)
    {
        if (Etkin && _isaretci != null) Hareket(new Vector2(x, 0));
    }
    public KartTarafi? SuruklemeyiBitir(float x) => Bitir(new Vector2(x, 0), false);
    public void Iptal()
    {
        _isaretci = null;
        _yatay = false;
        _onaylaniyor = false;
        _geriDonus?.Kill();
        if (IsInsideTree() && (!Position.IsEqualApprox(_merkez) || !Mathf.IsZeroApprox(RotationDegrees)))
        {
            _geriDonus = CreateTween().SetParallel();
            _geriDonus.TweenProperty(this, "position", _merkez, 0.18f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            _geriDonus.TweenProperty(this, "rotation_degrees", 0.0f, 0.18f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }
        Onizleme?.Invoke(0);
    }

    protected void MerkezeYerlestir(Vector2 merkez)
    {
        Iptal();
        _geriDonus?.Kill();
        _merkez = merkez;
        Position = merkez;
        RotationDegrees = 0;
    }
}
