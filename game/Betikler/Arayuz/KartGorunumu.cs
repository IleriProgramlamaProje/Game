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
    private Kart _kart;
    private MetinYerlestirici _yerlestirici;
    private Control _yuva;
    private Panel[] _arkaKartlar;
    public override void _Ready()
    {
        base._Ready();
        _baslik = GetNode<Label>("%KartBasligi");
        _metin = GetNode<Label>("%KartMetni");
        _onizleme = GetNode<Label>("%KararOnizleme");
        _yuva = GetParent<Control>();
        // Deste hissi veren iki sabit kart sahnede tanımlıdır; girdi almaz ve gerçek kart verisi değildir.
        _arkaKartlar = new[] { _yuva.GetNode<Panel>("DesteKarti0"), _yuva.GetNode<Panel>("DesteKarti1") };
        _yuva.Resized += Yerlesim;
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
        BeklemeyiGoster();
        Yerlesim();
        Callable.From(Yerlesim).CallDeferred();
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
        float yukseklik = Mathf.Max(260, GetCombinedMinimumSize().Y);
        Size = new Vector2(_genislik, yukseklik);
        float olcek = Mathf.Min(1, (_yuva.Size.X - 8) / _genislik);
        Scale = Vector2.One * olcek;
        _yuva.CustomMinimumSize = new Vector2(0, yukseklik * olcek + 30);
        var merkez = new Vector2((_yuva.Size.X - _genislik) / 2, 6);
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
}
