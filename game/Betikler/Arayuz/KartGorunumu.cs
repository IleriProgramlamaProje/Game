using Godot;
using Krallik;

public partial class KartGorunumu : KaydirmaKontrolu
{
    private Label _baslik;
    private Label _metin;
    private Button _sol;
    private Button _sag;
    public override void _Ready()
    {
        base._Ready();
        _baslik = GetNode<Label>("%KartBasligi");
        _metin = GetNode<Label>("%KartMetni");
        _sol = GetNode<Button>("%SolKarar");
        _sag = GetNode<Button>("%SagKarar");
    }
    public void KartiGoster(Kart kart, MetinYerlestirici metin)
    {
        Iptal();
        _baslik.Text = metin.Baslik(kart.Konusan);
        _metin.Text = metin.Yerlestir(kart.Metin);
        _sol.Text = metin.Yerlestir(kart.Sol.Etiket);
        _sag.Text = metin.Yerlestir(kart.Sag.Etiket);
    }
}
