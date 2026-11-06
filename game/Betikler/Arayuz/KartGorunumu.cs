using Godot;
using Krallik;
using System.Linq;

public partial class KartGorunumu : KaydirmaKontrolu
{
    private Label _baslik;
    private Label _metin;
    private Label _onizleme;
    private Kart _kart;
    private MetinYerlestirici _yerlestirici;
    private Control _yuva;
    private readonly Panel[] _arkaKartlar = new Panel[2];
    public override void _Ready()
    {
        base._Ready();
        _baslik = GetNode<Label>("%KartBasligi");
        _metin = GetNode<Label>("%KartMetni");
        _onizleme = GetNode<Label>("%KararOnizleme");
        _yuva = GetParent<Control>();
        DesteyiOlustur();
        _yuva.Resized += Yerlesim;
        Onizleme += KarariGoster;
        Yerlesim();
    }
    public void KartiGoster(Kart kart, MetinYerlestirici metin)
    {
        Iptal();
        RotationDegrees = 0;
        _kart = kart;
        _yerlestirici = metin;
        _baslik.Text = metin.Baslik(kart.Konusan);
        _metin.Text = metin.Yerlestir(kart.Metin);
        _onizleme.Text = "← Sola veya sağa sürükle →";
        Yerlesim();
        Callable.From(Yerlesim).CallDeferred();
    }

    private void KarariGoster(float oran)
    {
        if (_kart == null) return;
        if (Mathf.IsZeroApprox(oran))
        {
            _onizleme.Text = "← Sola veya sağa sürükle →";
            _onizleme.AddThemeColorOverride("font_color", new Color("211a17"));
            return;
        }
        var karar = oran > 0 ? _kart.Sag : _kart.Sol;
        string etkiler = string.Join("   ", Krallik.Gostergeler.Siralama.Where(karar.Etki.ContainsKey)
            .Select(k => $"{Krallik.Gostergeler.Ad(k)} {(karar.Etki[k] > 0 ? "▲" : "▼")}"));
        _onizleme.AddThemeColorOverride("font_color", new Color(Mathf.Abs(oran) >= 1 ? "713a12" : "211a17"));
        _onizleme.Text = _yerlestirici.Yerlestir(karar.Etiket) + "\n" + etkiler + "\n" +
            (Mathf.Abs(oran) >= 1 ? "Seçmek için bırak" : "Seçmek için biraz daha sürükle");
    }

    private void Yerlesim()
    {
        // Hafif eğilen üst kart ve sabit deste için kenarlarda boşluk bırak.
        if (_yuva.Size.X <= 0) return;
        float genislik = Mathf.Clamp(_yuva.Size.X * 0.75f, 280, 480);
        Size = new Vector2(genislik, 240);
        float yukseklik = Mathf.Max(240, GetCombinedMinimumSize().Y);
        Size = new Vector2(genislik, yukseklik);
        float aci = Mathf.DegToRad(AzamiEgim);
        float olcek = Mathf.Min(1, Mathf.Max(1, _yuva.Size.X - 40) /
            (genislik * Mathf.Cos(aci) + yukseklik * Mathf.Sin(aci)));
        Scale = Vector2.One * olcek;
        _yuva.CustomMinimumSize = new Vector2(0,
            (genislik * Mathf.Sin(aci) + yukseklik * Mathf.Cos(aci)) * olcek + 48);
        MerkezeYerlestir((_yuva.Size - Size) / 2);
        PivotOffset = Size / 2;
        for (int i = 0; i < _arkaKartlar.Length; i++)
        {
            var arka = _arkaKartlar[i];
            arka.Size = Size;
            arka.Scale = Scale * (i == 0 ? 0.96f : 0.98f);
            arka.PivotOffset = PivotOffset;
            arka.Position = Position + new Vector2(i == 0 ? -6 : 6, i == 0 ? 18 : 9);
            arka.RotationDegrees = i == 0 ? -3 : 2;
        }
    }

    private void DesteyiOlustur()
    {
        for (int i = 0; i < _arkaKartlar.Length; i++)
        {
            var arka = new Panel { Name = "DesteKarti" + i, MouseFilter = MouseFilterEnum.Ignore };
            arka.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(i == 0 ? "80694e" : "b69a73"),
                BorderColor = new Color("a78a61"),
                BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
                ShadowColor = new Color(0, 0, 0, 0.3f), ShadowSize = 6
            });
            _yuva.AddChild(arka);
            _yuva.MoveChild(arka, i);
            _arkaKartlar[i] = arka;
        }
    }
}
