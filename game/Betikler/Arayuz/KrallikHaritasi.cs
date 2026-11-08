using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Üstten bakışlı krallık haritası. Her mekânın adı haritada bir düğmedir: ok tuşları en yakın mekâna
/// atlar (Godot odak komşuluğu), Enter ya da dokunma içeri girer. Tehlikedeki göstergenin mekânı kırmızıdır.
/// </summary>
public partial class KrallikHaritasi : Control
{
    /// <summary>Çizimin birim boyutu (300×400).</summary>
    public static readonly Vector2 CizimBoyutu = new(300, 400);
    public event Action<Mekan> MekanSecildi;
    public event Action<Mekan> MekanOdaklandi;
    public IReadOnlyDictionary<string, Button> Isaretler => _isaretler;
    public Mekan Secili { get; private set; }

    private TextureRect _cizim;
    private readonly Dictionary<string, Button> _isaretler = new();
    private readonly Dictionary<string, Mekan> _mekanlar = new();
    private static readonly Color Lacivert = new(0.07f, 0.1f, 0.18f, 0.88f);
    private static readonly Color Altin = new(0.9f, 0.71f, 0.25f);
    private static readonly Color Bordo = new(0.5f, 0.1f, 0.12f, 0.92f);

    public override void _Ready()
    {
        _cizim = new TextureRect
        {
            Name = "Cizim", MouseFilter = MouseFilterEnum.Ignore, Texture = GD.Load<Texture2D>(KarakterGorselleri.HaritaYolu),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.Scale
        };
        AddChild(_cizim);
        MouseFilter = MouseFilterEnum.Ignore;
        Resized += Yerlestir;
    }

    public void Kur(IEnumerable<Mekan> mekanlar)
    {
        foreach (var d in _isaretler.Values) d.QueueFree();
        _isaretler.Clear();
        _mekanlar.Clear();
        foreach (var mekan in mekanlar)
        {
            var dugme = new Button
            {
                Name = "Isaret_" + mekan.Anahtar, Text = mekan.Ad, FocusMode = FocusModeEnum.All,
                CustomMinimumSize = new Vector2(0, 44), ClipText = false
            };
            Stil(dugme, false);
            dugme.Pressed += () => MekanSecildi?.Invoke(mekan);
            dugme.FocusEntered += () => { Secili = mekan; MekanOdaklandi?.Invoke(mekan); };
            dugme.MouseEntered += () => MekanOdaklandi?.Invoke(mekan);
            AddChild(dugme);
            _isaretler[mekan.Anahtar] = dugme;
            _mekanlar[mekan.Anahtar] = mekan;
        }
        Callable.From(Yerlestir).CallDeferred();
    }

    /// <summary>Göstergesi uçta olan mekânın adı kırmızı zemine geçer.</summary>
    public void DurumuGoster(Func<Mekan, bool> tehlikede)
    {
        foreach (var (anahtar, dugme) in _isaretler) Stil(dugme, tehlikede(_mekanlar[anahtar]));
    }

    public void Odakla(string anahtar)
    {
        if (_isaretler.TryGetValue(anahtar, out var dugme)) dugme.GrabFocus(true);
    }

    private static void Stil(Button dugme, bool tehlike)
    {
        StyleBoxFlat Kutu(Color zemin, Color kenar, int kalinlik) => new()
        {
            BgColor = zemin, BorderColor = kenar, CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, ContentMarginLeft = 10, ContentMarginRight = 10,
            ContentMarginTop = 4, ContentMarginBottom = 4,
            BorderWidthLeft = kalinlik, BorderWidthTop = kalinlik, BorderWidthRight = kalinlik, BorderWidthBottom = kalinlik
        };
        var zemin = tehlike ? Bordo : Lacivert;
        dugme.AddThemeStyleboxOverride("normal", Kutu(zemin, Altin, 2));
        dugme.AddThemeStyleboxOverride("hover", Kutu(zemin.Lightened(0.15f), Altin, 3));
        dugme.AddThemeStyleboxOverride("pressed", Kutu(Altin.Darkened(0.2f), Altin, 3));
        dugme.AddThemeStyleboxOverride("focus", Kutu(new Color(0, 0, 0, 0), new Color(1, 0.93f, 0.6f), 3));
        dugme.AddThemeColorOverride("font_color", new Color(1, 0.92f, 0.7f));
        dugme.AddThemeColorOverride("font_focus_color", new Color(1, 0.97f, 0.8f));
        dugme.AddThemeColorOverride("font_hover_color", Colors.White);
        dugme.TooltipText = tehlike ? "Bu mekânda durum kritik" : "";
    }

    /// <summary>Çizimin alana sığan dikdörtgeni; düğmeler bunun içinde, mekânın etiket noktasında ortalanır.</summary>
    public Rect2 CizimAlani { get; private set; }

    private void Yerlestir()
    {
        if (_cizim == null || Size.X <= 0 || Size.Y <= 0) return;
        float olcek = Mathf.Min(Size.X / CizimBoyutu.X, Size.Y / CizimBoyutu.Y);
        var boyut = CizimBoyutu * olcek;
        CizimAlani = new Rect2(((Size - boyut) / 2).Floor(), boyut);
        _cizim.Position = CizimAlani.Position;
        _cizim.Size = boyut;
        // Küçük haritada adlar da küçülür ki birbirine binmesin; telefonda 14, dar masaüstü penceresinde 11.
        int yazi = Mathf.Clamp(Mathf.RoundToInt(boyut.X / 24), 11, 15);
        foreach (var (anahtar, dugme) in _isaretler)
        {
            var e = _mekanlar[anahtar].Etiket;
            dugme.AddThemeFontSizeOverride("font_size", yazi);
            foreach (string durum in new[] { "normal", "hover", "pressed", "focus" })
                if (dugme.GetThemeStylebox(durum) is StyleBoxFlat kutu) kutu.ContentMarginLeft = kutu.ContentMarginRight = yazi < 13 ? 5 : 10;
            dugme.Size = Vector2.Zero;
            dugme.Size = dugme.GetCombinedMinimumSize();
            var merkez = CizimAlani.Position + new Vector2(e[0], e[1]) * boyut;
            // Düğme haritanın dışına taşmaz.
            dugme.Position = new Vector2(
                Mathf.Clamp(merkez.X - dugme.Size.X / 2, CizimAlani.Position.X + 2, CizimAlani.End.X - dugme.Size.X - 2),
                Mathf.Clamp(merkez.Y - dugme.Size.Y / 2, CizimAlani.Position.Y + 2, CizimAlani.End.Y - dugme.Size.Y - 2)).Floor();
        }
    }

    public IEnumerable<Mekan> Mekanlar => _mekanlar.Values;
}
