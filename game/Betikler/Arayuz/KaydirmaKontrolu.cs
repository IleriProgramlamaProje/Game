using Godot;
using System;

public enum KartTarafi { Sol, Sag }

public partial class KaydirmaKontrolu : PanelContainer
{
    public event Action<float> Onizleme;
    public event Action<KartTarafi> KararOnaylandi;
    public bool Etkin { get; set; }
    /// <summary>Önizleme klavyeyle açıldıysa doğrudur; metin "bırak" yerine "Enter" der.</summary>
    public bool KlavyeOnizlemesi => _klavyeTarafi != null;
    private int? _isaretci;
    private Vector2 _baslangic;
    private Vector2 _merkez;
    private bool _yatay;
    private Tween _geriDonus;
    private bool _onaylaniyor;
    private KartTarafi? _klavyeTarafi;
    private bool _klavyeOdagi;
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
        // Odak çizgisi yalnız klavyeyle oynarken görünür; fare/dokunmada süslü çerçeveyi kapatmaz.
        if (HasFocus() && _klavyeOdagi)
            DrawRect(new Rect2(new Vector2(6, 6), Size - new Vector2(12, 12)), new Color(1, 0.88f, 0.56f), false, 2);
    }
    public override void _ExitTree() => GetWindow().FocusExited -= Iptal;

    public override void _GuiInput(InputEvent olay)
    {
        if (olay is InputEventKey { Pressed: true }) KlavyeOdaginiAyarla(true);
        else if (olay is InputEventMouseButton or InputEventScreenTouch) KlavyeOdaginiAyarla(false);
        if (!Etkin || _onaylaniyor) return;
        if (olay is InputEventKey { Pressed: true })
        {
            if (KlavyeGirdisi(olay)) AcceptEvent();
            return;
        }
        if (_isaretci != null) return;
        if (olay is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } fare)
            Basla(-1, GetGlobalTransformWithCanvas() * fare.Position);
        else if (olay is InputEventScreenTouch { Pressed: true } dokunma)
            Basla(dokunma.Index, GetGlobalTransformWithCanvas() * dokunma.Position);
    }
    private void KlavyeOdaginiAyarla(bool klavye)
    {
        if (_klavyeOdagi == klavye) return;
        _klavyeOdagi = klavye;
        QueueRedraw();
    }
    private bool KlavyeGirdisi(InputEvent olay)
    {
        if (_isaretci != null) return false;
        if (olay.IsActionPressed("ui_left")) { KlavyeyleYonel(KartTarafi.Sol); return true; }
        if (olay.IsActionPressed("ui_right")) { KlavyeyleYonel(KartTarafi.Sag); return true; }
        if (_klavyeTarafi is { } taraf && olay.IsActionPressed("ui_accept")) { Onayla(taraf); return true; }
        if (_klavyeTarafi != null && olay.IsActionPressed("ui_cancel")) { Iptal(); return true; }
        return false;
    }
    /// <summary>İlk ok kartı o yöne yatırıp önizler; aynı ok onaylar, ters ok vazgeçer.</summary>
    public void KlavyeyleYonel(KartTarafi taraf)
    {
        if (!Etkin || _onaylaniyor || _isaretci != null) return;
        if (_klavyeTarafi == taraf) { Onayla(taraf); return; }
        if (_klavyeTarafi != null) { Iptal(); return; }
        _geriDonus?.Kill();
        PivotOffset = Size / 2;
        _klavyeTarafi = taraf;
        _geriDonus = CreateTween().SetParallel();
        _geriDonus.TweenProperty(this, "position", _merkez + KlavyeKaymasi(taraf), 0.12f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _geriDonus.TweenProperty(this, "rotation_degrees", KlavyeEgimi(taraf), 0.12f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        Onizleme?.Invoke(taraf == KartTarafi.Sag ? 1 : -1);
    }
    // Klavyede kart, metin dar ekranda da okunur kalsın diye sürüklemeden daha az yatar.
    private Vector2 KlavyeKaymasi(KartTarafi taraf) => new((taraf == KartTarafi.Sag ? 1 : -1) * Esik * 0.6f, 0);
    private static float KlavyeEgimi(KartTarafi taraf) => (taraf == KartTarafi.Sag ? 1 : -1) * AzamiEgim * 0.4f;
    /// <summary>Klavye önizlemesindeki kararı onaylar.</summary>
    public KartTarafi? KlavyeyleOnayla()
    {
        if (_klavyeTarafi is not { } taraf || !Etkin || _onaylaniyor) return null;
        Onayla(taraf);
        return taraf;
    }
    private void Basla(int kimlik, Vector2 konum)
    {
        _geriDonus?.Kill();
        _klavyeTarafi = null;
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
        GetViewport().SetInputAsHandled();
    }
    private KartTarafi? Bitir(Vector2 konum, bool iptal)
    {
        if (_isaretci == null || !Etkin) return null;
        float dx = konum.X - _baslangic.X;
        bool sec = !iptal && Mathf.Abs(dx) >= Esik && Mathf.Abs(dx) > Mathf.Abs(konum.Y - _baslangic.Y);
        GetViewport().SetInputAsHandled();
        if (!sec) { Iptal(); return null; }
        var taraf = dx > 0 ? KartTarafi.Sag : KartTarafi.Sol;
        Onayla(taraf);
        return taraf;
    }
    private void Onayla(KartTarafi taraf)
    {
        _isaretci = null;
        _yatay = false;
        _klavyeTarafi = null;
        _onaylaniyor = true;
        _geriDonus?.Kill();
        var yon = taraf == KartTarafi.Sag ? 1 : -1;
        _geriDonus = CreateTween().SetParallel();
        _geriDonus.TweenProperty(this, "position:x", Position.X + yon * GetViewportRect().Size.X, 0.2)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _geriDonus.TweenProperty(this, "rotation_degrees", yon * AzamiEgim * 2, 0.2);
        _geriDonus.Chain().TweenCallback(Callable.From(() =>
        {
            _onaylaniyor = false;
            if (Etkin) KararOnaylandi?.Invoke(taraf);
        }));
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
        _klavyeTarafi = null;
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

    /// <summary>
    /// Yerleşim değişince kartın dinlenme noktasını günceller. Süren onay animasyonunu,
    /// sürüklemeyi veya klavye önizlemesini bozmaz; aksi hâlde ertelenmiş bir yerleşim
    /// çıkış animasyonunu öldürür ve karar hiç uygulanmazdı.
    /// </summary>
    protected void MerkezeYerlestir(Vector2 merkez)
    {
        var kayma = merkez - _merkez;
        _merkez = merkez;
        if (_onaylaniyor) return;
        if (_isaretci != null)
        {
            Position += kayma;
            return;
        }
        _geriDonus?.Kill();
        if (_klavyeTarafi is { } taraf)
        {
            Position = merkez + KlavyeKaymasi(taraf);
            RotationDegrees = KlavyeEgimi(taraf);
            return;
        }
        Position = merkez;
        RotationDegrees = 0;
    }
}
