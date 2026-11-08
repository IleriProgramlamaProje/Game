using Godot;
using Krallik;
using System.Linq;

public partial class KartGorunumu : KaydirmaKontrolu
{
    public const string BeklemeMetni = "← Sola veya sağa sürükle →";
    // Parşömen üzerindeki mürekkep ve eşik geçildiğinde vurgulanan bordo.
    private static readonly Color Murekkep = new(0.17f, 0.105f, 0.055f);
    private static readonly Color Vurgu = new(0.5f, 0.11f, 0.06f);
    private Label _baslik;
    private Label _metin;
    private Label _onizleme;
    private TextureRect _mekan;
    private TextureRect _portre;
    private Kart _kart;
    private MetinYerlestirici _yerlestirici;
    private Control _yuva;
    private ScrollContainer _kaydirma;
    private Panel[] _arkaKartlar;
    /// <summary>Kart sığmak için en fazla bu orana küçülür; daha uzun kartta sayfa kayar.</summary>
    public const float EnAzOlcek = 0.75f;
    /// <summary>Portre/mekân sahnesinin olağan yüksekliği ve gizlenmeden önce inebileceği en az yükseklik.</summary>
    public const float SahneYuksekligi = 112, SahneEnAz = 56;
    private Control _sahne;
    public override void _Ready()
    {
        base._Ready();
        _baslik = GetNode<Label>("%KartBasligi");
        _metin = GetNode<Label>("%KartMetni");
        _onizleme = GetNode<Label>("%KararOnizleme");
        _sahne = GetNode<Control>("%KartSahnesi");
        _mekan = GetNode<TextureRect>("%KartMekani");
        _portre = GetNode<TextureRect>("%KartPortresi");
        _yuva = GetParent<Control>();
        // Deste hissi veren iki sabit kart sahnede tanımlıdır; girdi almaz ve gerçek kart verisi değildir.
        _arkaKartlar = new[] { _yuva.GetNode<Panel>("DesteKarti0"), _yuva.GetNode<Panel>("DesteKarti1") };
        for (var ata = _yuva.GetParent(); ata != null && _kaydirma == null; ata = ata.GetParent()) _kaydirma = ata as ScrollContainer;
        // Konum değişimi de dinlenir: üstteki göstergeler görününce kartın sığacağı yükseklik değişir.
        _yuva.ItemRectChanged += Yerlesim;
        // Başlık bandı gizlenip kaydırma alanı büyüyünce de kart yeniden sığdırılır.
        if (_kaydirma != null) _kaydirma.Resized += Yerlesim;
        // Etiketler yeni genişliğe göre ancak bir sonraki sıralamada yeniden sarılır. Bu yüzden yükseklik,
        // asgari boyut her değiştiğinde yeniden uyarlanır; aksi hâlde eski dar genişliğin dev yüksekliği kalır.
        MinimumSizeChanged += () => Callable.From(BoyutuUyarla).CallDeferred();
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
        SahneyiGoster(kart.Konusan, metin.Kimlik(kart.Konusan).PortreAnahtari);
        BeklemeyiGoster();
        Yerlesim();
        Callable.From(Yerlesim).CallDeferred();
    }

    /// <summary>Konuşanı kendi mekânının önünde gösterir; çizimi olmayan rolde yalnız mekân görünür.</summary>
    private void SahneyiGoster(string rol, string portreAnahtari)
    {
        _mekan.Texture = GD.Load<Texture2D>(KarakterGorselleri.MekanYolu(rol));
        string portre = KarakterGorselleri.PortreYolu(rol, portreAnahtari);
        _portre.Texture = portre == null ? null : GD.Load<Texture2D>(portre);
    }

    private void BeklemeyiGoster()
    {
        _onizleme.Text = BeklemeMetni;
        _onizleme.AddThemeColorOverride("font_color", Murekkep);
    }

    private void KarariGoster(float oran)
    {
        if (_kart == null) return;
        if (Mathf.IsZeroApprox(oran))
        {
            BeklemeyiGoster();
            return;
        }
        var karar = oran > 0 ? _kart.Sag : _kart.Sol;
        string etkiler = string.Join("   ", Krallik.Gostergeler.Siralama.Where(karar.Etki.ContainsKey)
            .Select(k => $"{Krallik.Gostergeler.Ad(k)} {(karar.Etki[k] > 0 ? "▲" : "▼")}"));
        bool hazir = Mathf.Abs(oran) >= 1;
        string ipucu = KlavyeOnizlemesi ? "Enter: onayla · ters ok: vazgeç"
            : hazir ? "Seçmek için bırak" : "Seçmek için biraz daha sürükle";
        string etiket = _yerlestirici.Yerlestir(karar.Etiket);
        _onizleme.AddThemeColorOverride("font_color", hazir ? Vurgu : Murekkep);
        _onizleme.Text = (oran > 0 ? etiket + "  →" : "←  " + etiket) + "\n" + etkiler + "\n" + ipucu;
    }

    private float _genislik;

    private void Yerlesim()
    {
        if (_yuva.Size.X <= 0) return;
        // Dar ekranda kart neredeyse tüm genişliği kullanır; geniş ekranda okunur bir sütunda kalır.
        // Kenarlarda arka kartlar ve hafif eğim için birkaç birim boşluk bırakılır.
        _genislik = Mathf.Clamp(_yuva.Size.X - 28, 220, 480);
        BoyutuUyarla();
    }

    private void BoyutuUyarla()
    {
        if (_genislik <= 0 || !IsInsideTree()) return;
        Size = new Vector2(_genislik, 0);
        SahneyiSigdir();
        float yukseklik = Mathf.Max(260, GetCombinedMinimumSize().Y);
        Size = new Vector2(_genislik, yukseklik);
        float olcek = Mathf.Min(1, (_yuva.Size.X - 8) / _genislik);
        // Telefonda göstergeler ve kart kaydırmadan birlikte görünsün; okunurluk için EnAzOlcek'in altına inilmez.
        olcek = Mathf.Min(olcek, Mathf.Max(EnAzOlcek, SiganYukseklik() / yukseklik));
        Scale = Vector2.One * olcek;
        _yuva.CustomMinimumSize = new Vector2(0, yukseklik * olcek + 30);
        // Ölçek kartın ortasına göre uygulanır; küçülen kartın görünen üst kenarı yine yuvanın 6 birim altında kalır.
        var merkez = new Vector2((_yuva.Size.X - _genislik) / 2, 6 - (1 - olcek) * yukseklik / 2);
        MerkezeYerlestir(merkez);
        PivotOffset = Size / 2;
        for (int i = 0; i < _arkaKartlar.Length; i++)
        {
            var arka = _arkaKartlar[i];
            arka.Size = Size;
            arka.Scale = Scale * (i == 0 ? 0.96f : 0.98f);
            arka.PivotOffset = PivotOffset;
            arka.Position = merkez + new Vector2(i == 0 ? -6 : 6, i == 0 ? 18 : 9);
            arka.RotationDegrees = i == 0 ? -3 : 2;
        }
    }

    /// <summary>
    /// Kart en küçük ölçekte bile ekrana sığmıyorsa metin daha da küçülmez; önce portre/mekân sahnesi kısalır,
    /// <see cref="SahneEnAz"/> altına inecekse gizlenir. Sonuç kartın kendi yüksekliğine bağlı değildir, döngü kurmaz.
    /// </summary>
    private void SahneyiSigdir()
    {
        float ayrim = _sahne.GetParent<Container>().GetThemeConstant("separation");
        float sahnesiz = GetCombinedMinimumSize().Y - (_sahne.Visible ? _sahne.CustomMinimumSize.Y + ayrim : 0);
        float sahne = Mathf.Min(SahneYuksekligi, Mathf.Floor(SiganYukseklik() / EnAzOlcek - sahnesiz - ayrim));
        bool goster = sahne >= SahneEnAz;
        if (goster) _sahne.CustomMinimumSize = new Vector2(0, sahne);
        _sahne.Visible = goster;
    }

    /// <summary>Kaydırma alanında, yuvanın üstünden ekranın altına kadar kart için kalan yükseklik.</summary>
    private float SiganYukseklik()
    {
        if (_kaydirma == null || _kaydirma.GetChildCount() == 0 || _kaydirma.Size.Y <= 0) return float.PositiveInfinity;
        // Kaydırmadan bağımsız olsun diye içeriğin üstüne göre ölçülür; 30 birim arka kartlara kalır.
        float ust = _yuva.GlobalPosition.Y - _kaydirma.GetChild<Control>(0).GlobalPosition.Y;
        return Mathf.Max(1, _kaydirma.Size.Y - ust - 30);
    }
}
