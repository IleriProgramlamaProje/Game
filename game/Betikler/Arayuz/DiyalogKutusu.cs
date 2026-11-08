using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Sahnenin altındaki konuşma kartı: konuşanın adı, harf harf yazılan metin ve sığmayan metin için sayfalar.
/// Enter, boşluk ya da karta dokunmak yazıyı tamamlar, sonra sonraki sayfaya geçer. Son sayfa bitince
/// alttaki eylem alanı (kaydırma ipucu, Devam Et, Haritaya Dön) belirir. <see cref="Kaydirilabilir"/> ise
/// kart o anda sağa/sola kaydırılarak karar verilir (<see cref="KaydirmaKontrolu"/>). Kart yuvasının
/// genişliğini alır, boyu içeriği kadardır; yuva bir kapsayıcı değildir ki kaydırılan kartın yeri bozulmasın.
/// </summary>
public partial class DiyalogKutusu : KaydirmaKontrolu
{
    /// <summary>
    /// Kartın metin alanı hep bu kadar satırdır: kart her sözde aynı boydadır, arkadaki sahne kıpırdamaz.
    /// Destedeki sözlerin hepsi telefonda tek sayfaya sığar; sığmayan söz eşit sayfalara bölünür.
    /// </summary>
    [Export] public int EnFazlaSatir { get; set; } = 8;
    /// <summary>Telefon kartının metin genişliği (birim); daha geniş kartta aynı söz daha az satır tutar.</summary>
    private const float TelefonGenisligi = 290;
    /// <summary>Bu genişlikte sayfa başına satır: telefonda <see cref="EnFazlaSatir"/>, geniş ekranda en az 4.</summary>
    public int SatirSiniri { get; private set; }
    private readonly Dictionary<int, float> _satirYukseklikleri = new();
    /// <summary>Saniyede yazılan harf.</summary>
    [Export] public float HarfHizi { get; set; } = 55f;
    /// <summary>Testler ve ekran görüntüleri için: metin beklemeden tam görünür.</summary>
    public bool Aninda { get; set; }
    /// <summary>Son sayfa tamamen yazıldı, eylemler açıldı.</summary>
    public event Action MetinBitti;

    public string TamMetin { get; private set; } = "";
    public int Sayfa { get; private set; }
    public int SayfaSayisi => _sayfalar.Count;
    public IReadOnlyList<string> Sayfalar => _sayfalar;
    public bool Bitti { get; private set; }
    public bool Yaziyor => !_sayfaTamam;
    /// <summary>Metin bitince kart sağa/sola kaydırılabilir (karar kartı). Sonuç ve mekân kartında kapalıdır.</summary>
    public bool Kaydirilabilir { get; set; }
    protected override bool HemenOnayla => Aninda;

    private Control _isimPlakasi;
    private Label _isim;
    private Label _metin;
    private Label _isaret;
    private Control _eylemler;
    private readonly List<string> _sayfalar = new() { "" };
    private float _gorunen;
    private bool _sayfaTamam = true;
    private float _sayfalananGenislik;

    private Control _yuva;

    public override void _Ready()
    {
        base._Ready();
        _yuva = GetParent<Control>();
        _yuva.Resized += Yerlesim;
        MinimumSizeChanged += () => Callable.From(Yerlesim).CallDeferred();
        _isimPlakasi = GetNode<Control>("%IsimPlakasi");
        _isim = GetNode<Label>("%KonusanAdi");
        _metin = GetNode<Label>("%DiyalogMetni");
        _isaret = GetNode<Label>("%DevamIsareti");
        _eylemler = GetNode<Control>("%Eylemler");
        // Harfler yerleşmiş satırların üstünde belirir; kelimeler yazılırken satır değiştirmez.
        _metin.VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping;
        SatirSiniriniAyarla();
        _metin.Resized += () => Callable.From(GenislikDegisti).CallDeferred();
        SetProcess(false);
        EylemleriAc(false);
    }

    public void Goster(string isim, string metin)
    {
        _isim.Text = isim ?? "";
        _isimPlakasi.Visible = !string.IsNullOrEmpty(isim);
        TamMetin = metin ?? "";
        Sayfa = 0;
        Bitti = false;
        Etkin = false;
        Sifirla();
        EylemleriAc(false);
        // Yazı, kart genişliğini aldıktan sonra başlar: sayfalar o genişlikle bölünür. Kart bu karede görünür
        // olduysa (ör. hükümdar seçilince ilk kart) genişlik yerleşimle gelir; başlangıç o zamana kalır.
        _sayfaTamam = true;
        SetProcess(false);
        _isaret.Visible = false;
        _metin.Text = "";
        _baslamayiBekliyor = true;
        if (!BaslamayiDene()) Callable.From(() => BaslamayiDene()).CallDeferred();
    }

    private bool _baslamayiBekliyor;

    /// <summary>Genişlik hazırsa sayfaları böler ve yazmaya başlar; değilse genişlik gelince yeniden denenir.</summary>
    private bool BaslamayiDene()
    {
        if (!_baslamayiBekliyor) return true;
        if (!IsInsideTree() || _metin.Size.X < 40) return false;
        _baslamayiBekliyor = false;
        Sayfala();
        SayfayiBaslat();
        return true;
    }

    /// <summary>Enter ya da dokunma: yazılan sayfayı tamamlar; tamamsa sonraki sayfaya geçer.</summary>
    public void Ilerle()
    {
        if (_baslamayiBekliyor) return;
        if (!_sayfaTamam) SayfayiTamamla();
        else if (Sayfa < _sayfalar.Count - 1)
        {
            Sayfa++;
            SayfayiBaslat();
        }
    }

    /// <summary>Bütün sayfaları atlayıp son sayfayı tam gösterir.</summary>
    public void Tamamla()
    {
        if (_baslamayiBekliyor)
        {
            // Genişlik gelmeden tamamlanırsa metin tek sayfa kalır; genişlik gelince son sayfası gösterilir.
            _baslamayiBekliyor = false;
            Sayfala();
        }
        Sayfa = _sayfalar.Count - 1;
        _metin.Text = _sayfalar[Sayfa];
        SayfayiTamamla();
    }

    public override void _Process(double delta)
    {
        if (_sayfaTamam) { SetProcess(false); return; }
        _gorunen += (float)delta * HarfHizi;
        // Etiket henüz şekillenmemişse karakter sayısı 0 dönebilir; ölçü metnin kendisidir.
        if (_gorunen >= _metin.Text.Length) SayfayiTamamla();
        else _metin.VisibleCharacters = (int)_gorunen;
    }

    public override void _GuiInput(InputEvent olay)
    {
        // Karar kartı metin bitince kaydırılır: sürükleme, ← → önizleme ve Enter onayı üst sınıfta.
        if (Etkin) { base._GuiInput(olay); return; }
        // Telefonda dokunma Godot'ta fare tıklaması olarak da gelir; yalnız tıklama dinlenir, iki kez ilerlemez.
        if (olay is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } || olay.IsActionPressed("ui_accept"))
        {
            Ilerle();
            AcceptEvent();
        }
    }

    /// <summary>Kart yuvanın genişliğini alır, boyu içeriği kadardır; yuva da kart kadar yer ayırır.</summary>
    private void Yerlesim()
    {
        if (_yuva == null || _yuva.Size.X <= 0 || !IsInsideTree()) return;
        Size = new Vector2(_yuva.Size.X, 0);
        Size = new Vector2(_yuva.Size.X, GetCombinedMinimumSize().Y);
        PivotOffset = Size / 2;
        _yuva.CustomMinimumSize = new Vector2(0, Size.Y);
        MerkezeYerlestir(Vector2.Zero);
    }

    private void SayfayiBaslat()
    {
        _metin.Text = _sayfalar[Sayfa];
        _isaret.Visible = false;
        if (Aninda) { SayfayiTamamla(); return; }
        _gorunen = 0;
        _metin.VisibleCharacters = 0;
        _sayfaTamam = false;
        SetProcess(true);
    }

    private void SayfayiTamamla()
    {
        _metin.VisibleCharacters = -1;
        _sayfaTamam = true;
        SetProcess(false);
        bool son = Sayfa >= _sayfalar.Count - 1;
        _isaret.Visible = !son;
        if (!son || Bitti) return;
        Bitti = true;
        Etkin = Kaydirilabilir;
        EylemleriAc(true);
        MetinBitti?.Invoke();
    }

    /// <summary>
    /// Metni kelime kelime ekleyip satır sayısını etiketin kendisiyle ölçer; <see cref="EnFazlaSatir"/>
    /// aşılınca yeni sayfa başlar. Genişlik henüz yoksa tek sayfa kalır, yerleşince yeniden bölünür.
    /// </summary>
    private void Sayfala()
    {
        _sayfalar.Clear();
        _sayfalananGenislik = _metin.Size.X;
        if (_sayfalananGenislik < 40) _sayfalar.Add(TamMetin);
        else
        {
            SatirSiniriniAyarla();
            _sayfalar.AddRange(Bol(SatirSiniri));
            if (_sayfalar.Count > 1)
            {
                // Sığmayan söz eşit bölünür: son sayfada birkaç kelime kalmaz.
                _metin.Text = TamMetin;
                int hedef = (int)Math.Ceiling(_metin.GetLineCount() / (double)_sayfalar.Count);
                var dengeli = Bol(Math.Max(1, hedef));
                if (dengeli.Count == _sayfalar.Count)
                {
                    _sayfalar.Clear();
                    _sayfalar.AddRange(dengeli);
                }
            }
        }
        Sayfa = Math.Clamp(Sayfa, 0, _sayfalar.Count - 1);
        _metin.Text = _sayfalar[Sayfa];
    }

    /// <summary>
    /// Metin alanı tam <see cref="SatirSiniri"/> satır boyundadır (satır aralığı dahil; ölçü gerçek bir metinle
    /// alınır). Sınır yalnız genişliğe bağlıdır: kart her sözde aynı boydadır, arkadaki sahne kıpırdamaz.
    /// </summary>
    private void SatirSiniriniAyarla()
    {
        float genislik = _metin.Size.X;
        int satir = genislik < 40 ? EnFazlaSatir
            : Math.Clamp((int)Math.Ceiling(EnFazlaSatir * TelefonGenisligi / genislik), Math.Min(4, EnFazlaSatir), EnFazlaSatir);
        if (satir == SatirSiniri) return;
        SatirSiniri = satir;
        if (!_satirYukseklikleri.TryGetValue(satir, out float yukseklik))
        {
            string metin = _metin.Text;
            var kaydir = _metin.AutowrapMode;
            _metin.AutowrapMode = TextServer.AutowrapMode.Off;
            _metin.Text = string.Join("\n", Enumerable.Repeat("Ağ", satir));
            yukseklik = _satirYukseklikleri[satir] = Mathf.Ceil(_metin.GetMinimumSize().Y);
            _metin.AutowrapMode = kaydir;
            _metin.Text = metin;
        }
        _metin.CustomMinimumSize = new Vector2(0, yukseklik);
    }

    /// <summary>Kelimeleri sırayla ekleyip satır sayısını etiketin kendisiyle ölçer; sınır aşılınca yeni sayfa.</summary>
    private List<string> Bol(int satirSiniri)
    {
        var sayfalar = new List<string>();
        string sayfa = "";
        foreach (string kelime in TamMetin.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string aday = sayfa.Length == 0 ? kelime : sayfa + " " + kelime;
            _metin.Text = aday;
            if (sayfa.Length > 0 && _metin.GetLineCount() > satirSiniri)
            {
                sayfalar.Add(sayfa);
                sayfa = kelime;
            }
            else sayfa = aday;
        }
        sayfalar.Add(sayfa);
        return sayfalar;
    }

    private void GenislikDegisti()
    {
        if (_baslamayiBekliyor) { BaslamayiDene(); return; }
        if (!IsInsideTree() || Mathf.IsEqualApprox(_metin.Size.X, _sayfalananGenislik)) return;
        bool yaziyordu = !_sayfaTamam;
        Sayfala();
        if (Bitti) Tamamla();
        else if (!yaziyordu) SayfayiTamamla();
        else _metin.VisibleCharacters = Mathf.Min((int)_gorunen, _metin.Text.Length);
    }

    /// <summary>Eylemler yer kaplamaya devam eder (kutu boyu sıçramaz); metin bitene kadar saydam ve basılamaz.</summary>
    private void EylemleriAc(bool acik)
    {
        _eylemler.Modulate = acik ? Colors.White : new Color(1, 1, 1, 0);
        foreach (var dugme in _eylemler.FindChildren("*", "BaseButton", true, false).Cast<BaseButton>())
        {
            dugme.Disabled = !acik;
            dugme.FocusMode = acik ? FocusModeEnum.All : FocusModeEnum.None;
        }
    }
}
