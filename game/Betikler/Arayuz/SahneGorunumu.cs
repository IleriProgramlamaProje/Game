using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Tam ekran sahne: mekân çizimi ve önündeki karakterler bütün ekranın arkasında durur; istatistik çubuğu
/// üstte, konuşma kartı altta sahnenin üstüne biner. Çizimler 400×300 birimlik yataydır; telefonun dikey
/// ekranında çizimin önemli bölgesi (taht salonunda tahtın tepesinden konuşanın ayağına) çubuk ile kart
/// arasındaki görünür alana sığacak kadar büyütülür, çizimin taşmadığı yerler çizimin kendi üst ve alt
/// renkleriyle doldurulur. Karakterler çizim birimlerinde, ayaklarının bastığı noktaya göre yerleşir.
/// </summary>
public partial class SahneGorunumu : Control
{
    public static readonly Vector2 CizimBoyutu = new(400, 300);
    /// <summary>Karakter çizimlerinin en/boy oranı (80×112 birim).</summary>
    private const float KarakterOrani = 80f / 112f;
    private static readonly Vector2 TahtNoktasi = new(200, 207);
    private const float HukumdarBoyu = 106;
    private const float ZiyaretciZemini = 272, ZiyaretciBoyu = 124;
    /// <summary>Konuşanın tahtın sağ önünde durduğu yer (çizim birimi).</summary>
    public const float ZiyaretciHedefi = 262;
    /// <summary>Görünür kalması gereken bölgeler (çizim birimi).</summary>
    private static readonly Rect2 TahtBolgesi = new(118, 62, 188, 214);
    /// <summary>Mekânda sakin sayısına göre: tek kişi yakından, üç kişi geniş açıdan.</summary>
    private static readonly Rect2[] MekanBolgeleri = { new(110, 40, 180, 246), new(92, 40, 216, 246), new(56, 40, 288, 246) };
    private const float YurumeSuresi = 0.7f;
    private static readonly Vector2[][] SakinYerleri =
    {
        new[] { new Vector2(200, 274) },
        new[] { new Vector2(150, 274), new Vector2(262, 274) },
        new[] { new Vector2(112, 272), new Vector2(204, 278), new Vector2(296, 272) },
    };
    private const float SakinBoyu = 118;

    /// <summary>Testler ve ekran görüntüleri için: yürüyüş beklemeden biter.</summary>
    public bool Aninda { get; set; }
    public Texture2D Ziyaretci => _ziyaretci.Visible ? _ziyaretci.Texture : null;
    public Texture2D Hukumdar => _hukumdar.Visible ? _hukumdar.Texture : null;
    public Texture2D Zemin => _zemin.Texture;
    public float ZiyaretciKonumu { get; private set; } = 9999;
    public bool Yuruyor => _yuruyus != null && _yuruyus.IsValid() && _yuruyus.IsRunning();
    public IReadOnlyList<Texture2D> Sakinler => _sakinDokulari;
    /// <summary>Çubuk ile kart arasında sahnenin göründüğü alan (bu düğümün koordinatlarında).</summary>
    public Rect2 GorunurAlan { get; private set; }

    private Control _cizim;
    private ColorRect _ustDolgu, _altDolgu;
    private TextureRect _zemin, _hukumdar, _ziyaretci;
    private readonly List<TextureRect> _sakinler = new();
    private readonly List<Texture2D> _sakinDokulari = new();
    private static readonly Dictionary<Texture2D, (Color ust, Color alt)> KenarRenkleri = new();
    private Tween _yuruyus;
    private float _olcek = 1;
    private double _adim;
    private bool _mekanModu;

    public override void _Ready()
    {
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Ignore;
        _ustDolgu = new ColorRect { Name = "UstDolgu", MouseFilter = MouseFilterEnum.Ignore };
        _altDolgu = new ColorRect { Name = "AltDolgu", MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_ustDolgu);
        AddChild(_altDolgu);
        _cizim = new Control { Name = "Cizim", MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_cizim);
        _zemin = Resim("Zemin");
        _zemin.StretchMode = TextureRect.StretchModeEnum.Scale;
        _hukumdar = Resim("Hukumdar");
        for (int i = 0; i < 3; i++) _sakinler.Add(Resim("Sakin" + i));
        _ziyaretci = Resim("Ziyaretci");
        _hukumdar.Visible = _ziyaretci.Visible = false;
        foreach (var s in _sakinler) s.Visible = false;
        Resized += Yerlestir;
        VisibilityChanged += () => { if (IsVisibleInTree() && _alan != null) AlaniAyarla(_alan()); };
        Yerlestir();
    }

    private TextureRect Resim(string ad)
    {
        var resim = new TextureRect
        {
            Name = ad, MouseFilter = MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
        _cizim.AddChild(resim);
        return resim;
    }

    private Func<Rect2> _alan;
    /// <summary>
    /// Sahnenin göründüğü alanı (ekran koordinatı) veren işlev. Her karede bakılır, yalnız gerçekten değişince
    /// (ekran boyu, güvenli alan) çizim yeniden yerleşir; oyun içindeki hareketler sahneyi oynatmaz.
    /// </summary>
    public void Takip(Func<Rect2> alan)
    {
        _alan = alan;
        SetProcess(true);
    }

    /// <summary>Sahnenin görüneceği alanı (ekran koordinatı) bildirir; çizim bu alana göre büyütülüp kaydırılır.</summary>
    public void AlaniAyarla(Rect2 ekranAlani)
    {
        GorunurAlan = new Rect2(ekranAlani.Position - GetGlobalRect().Position, ekranAlani.Size);
        Yerlestir();
    }

    /// <summary>Taht salonu: hükümdar tahtta, sakinler gizli. Ziyaretçi olduğu yerde kalır.</summary>
    public void TahtSalonu(Texture2D zemin, Texture2D hukumdar)
    {
        _mekanModu = false;
        ZeminiKur(zemin, Colors.White);
        _hukumdar.Texture = hukumdar;
        _hukumdar.Visible = hukumdar != null;
        SakinleriGoster(Array.Empty<Texture2D>());
        Yerlestir();
    }

    /// <summary>Gezilen mekân: hükümdar ve ziyaretçi yok, mekânın sakinleri durur. Ton krallığın durumunu yansıtır.</summary>
    public void Mekan(Texture2D zemin, IReadOnlyList<Texture2D> sakinler, Color ton)
    {
        _yuruyus?.Kill();
        _mekanModu = true;
        ZeminiKur(zemin, ton);
        _hukumdar.Visible = false;
        _ziyaretci.Visible = false;
        ZiyaretciKonumu = GirisKonumu;
        SakinleriGoster(sakinler);
        Yerlestir();
    }

    private void ZeminiKur(Texture2D zemin, Color ton)
    {
        _zemin.Texture = zemin;
        _zemin.SelfModulate = ton;
        var (ust, alt) = Kenarlar(zemin);
        _ustDolgu.Color = ust * ton;
        _altDolgu.Color = alt * ton;
    }

    /// <summary>Çizimin üst kenarının ortası (duvar/gök) ve alt kenarının solu (zemin) dolgu rengi olur.</summary>
    private static (Color ust, Color alt) Kenarlar(Texture2D doku)
    {
        if (doku == null) return (Colors.Black, Colors.Black);
        if (KenarRenkleri.TryGetValue(doku, out var renkler)) return renkler;
        var resim = doku.GetImage();
        if (resim == null) return (Colors.Black, Colors.Black);
        if (resim.IsCompressed()) resim.Decompress();
        renkler = (resim.GetPixel(resim.GetWidth() / 2, 1), resim.GetPixel(resim.GetWidth() / 25, resim.GetHeight() - 2));
        renkler.ust.A = renkler.alt.A = 1;
        KenarRenkleri[doku] = renkler;
        return renkler;
    }

    private void SakinleriGoster(IReadOnlyList<Texture2D> dokular)
    {
        _sakinDokulari.Clear();
        _sakinDokulari.AddRange(dokular);
        for (int i = 0; i < _sakinler.Count; i++)
        {
            _sakinler[i].Visible = i < dokular.Count;
            _sakinler[i].Texture = i < dokular.Count ? dokular[i] : null;
        }
    }

    /// <summary>Ekranın sağ kenarının hemen dışı (çizim birimi): konuşan buradan yürüyerek girer.</summary>
    public float GirisKonumu => (Size.X - _cizim.Position.X) / _olcek + ZiyaretciBoyu * KarakterOrani / 2 + 4;

    /// <summary>
    /// Yeni konuşan sağdan yürüyerek gelir. Önceki biri duruyorsa önce o çıkar. Aynı kişi zaten
    /// yerindeyse kımıldamaz (ör. haritadan dönüşte).
    /// </summary>
    public void ZiyaretciGelsin(Texture2D portre)
    {
        if (_ziyaretci.Visible && _ziyaretci.Texture == portre && !Yuruyor && Mathf.IsEqualApprox(ZiyaretciKonumu, ZiyaretciHedefi)) return;
        _yuruyus?.Kill();
        bool biriVar = _ziyaretci.Visible && ZiyaretciKonumu < GirisKonumu && _ziyaretci.Texture != portre;
        if (Aninda)
        {
            _ziyaretci.Texture = portre;
            _ziyaretci.Visible = portre != null;
            Konumla(ZiyaretciHedefi);
            return;
        }
        _yuruyus = CreateTween();
        if (biriVar) _yuruyus.TweenMethod(Callable.From<float>(Konumla), ZiyaretciKonumu, GirisKonumu, YurumeSuresi * 0.6f);
        _yuruyus.TweenCallback(Callable.From(() =>
        {
            _ziyaretci.Texture = portre;
            _ziyaretci.Visible = portre != null;
            Konumla(GirisKonumu);
        }));
        _yuruyus.TweenMethod(Callable.From<float>(Konumla), GirisKonumu, ZiyaretciHedefi, YurumeSuresi)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        _yuruyus.Finished += YuruyusBitti;
    }

    private void YuruyusBitti() => Konumla(ZiyaretciKonumu);

    /// <summary>Konuşan sağa doğru çıkar (ör. son ekranına geçerken).</summary>
    public void ZiyaretciGitsin()
    {
        _yuruyus?.Kill();
        if (Aninda || !_ziyaretci.Visible)
        {
            _ziyaretci.Visible = false;
            Konumla(GirisKonumu);
            return;
        }
        _yuruyus = CreateTween();
        _yuruyus.TweenMethod(Callable.From<float>(Konumla), ZiyaretciKonumu, GirisKonumu, YurumeSuresi * 0.6f);
        _yuruyus.TweenCallback(Callable.From(() => _ziyaretci.Visible = false));
        _yuruyus.Finished += YuruyusBitti;
    }

    public override void _Process(double delta)
    {
        if (_alan != null && IsVisibleInTree())
        {
            var ekranAlani = _alan();
            var alan = new Rect2(ekranAlani.Position - GetGlobalRect().Position, ekranAlani.Size);
            if (!alan.IsEqualApprox(GorunurAlan)) AlaniAyarla(ekranAlani);
        }
        // Yürürken hafif sekme; durunca yere oturur.
        if (!Yuruyor) return;
        _adim += delta;
        Konumla(ZiyaretciKonumu);
    }

    private void Konumla(float x)
    {
        ZiyaretciKonumu = x;
        float sekme = Yuruyor ? Mathf.Abs(Mathf.Sin((float)_adim * 13f)) * 3f : 0f;
        Karakter(_ziyaretci, new Vector2(x, ZiyaretciZemini - sekme), ZiyaretciBoyu);
    }

    private void Yerlestir()
    {
        if (_cizim == null || Size.X <= 0 || Size.Y <= 0) return;
        var bolge = _mekanModu ? MekanBolgeleri[Math.Clamp(_sakinDokulari.Count, 1, 3) - 1] : TahtBolgesi;
        var alan = GorunurAlan.Size.Y > 0 ? GorunurAlan : new Rect2(Vector2.Zero, Size);
        // Önemli bölge hem ekran genişliğine hem görünür alanın yüksekliğine sığar.
        _olcek = Mathf.Max(0.05f, Mathf.Min(Size.X / bolge.Size.X, alan.Size.Y / bolge.Size.Y));
        // Dikey ekranda sahne her zaman ekranın enini doldurur; kısa telefonda bölgenin kenarları çubuğun ve kartın altına girer.
        if (Size.Y >= Size.X) _olcek = Mathf.Max(_olcek, Size.X / CizimBoyutu.X);
        var boyut = CizimBoyutu * _olcek;
        float x = Size.X / 2 - bolge.GetCenter().X * _olcek;
        // Çizim ekrandan genişse kenar boşluk bırakmaz; dar (yatay ekran) ise ortalanır.
        if (boyut.X >= Size.X) x = Mathf.Clamp(x, Size.X - boyut.X, 0);
        float y = alan.GetCenter().Y - bolge.GetCenter().Y * _olcek;
        _cizim.Position = new Vector2(x, y).Floor();
        _cizim.Size = boyut;
        _zemin.Position = Vector2.Zero;
        _zemin.Size = boyut;
        // Çizimin dışında kalan ekran: üstü duvar/gök rengi, çizimin alt kenarından aşağısı zemin rengi.
        _ustDolgu.Position = Vector2.Zero;
        _ustDolgu.Size = Size;
        float alt = _cizim.Position.Y + boyut.Y - 1;
        _altDolgu.Position = new Vector2(0, alt);
        _altDolgu.Size = new Vector2(Size.X, Mathf.Max(0, Size.Y - alt));
        Karakter(_hukumdar, TahtNoktasi, HukumdarBoyu);
        Konumla(ZiyaretciKonumu);
        var yerler = SakinYerleri[Math.Clamp(_sakinDokulari.Count, 1, 3) - 1];
        for (int i = 0; i < _sakinler.Count && i < yerler.Length; i++) Karakter(_sakinler[i], yerler[i], SakinBoyu);
    }

    /// <summary>Karakteri ayak noktası ve boyuyla (çizim birimi) yerleştirir.</summary>
    private void Karakter(TextureRect resim, Vector2 ayak, float boy)
    {
        var boyut = new Vector2(boy * KarakterOrani, boy) * _olcek;
        resim.Size = boyut;
        resim.Position = ayak * _olcek - new Vector2(boyut.X / 2, boyut.Y);
    }

    /// <summary>Ekran koordinatında dikdörtgenler (testler için).</summary>
    public Rect2 ZiyaretciAlani => _ziyaretci.GetGlobalRect();
    public Rect2 HukumdarAlani => _hukumdar.GetGlobalRect();
    public Rect2 CizimAlani => _cizim.GetGlobalRect();
}
