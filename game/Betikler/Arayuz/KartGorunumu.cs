using Godot;

public partial class KartGorunumu : VBoxContainer
{
	private Label _baslik = null;
	private Label _metin = null;
	private Label _solSecenek = null;
	private Label _sagSecenek = null;
	private readonly OturumKarakterleri _karakterler = new();

	public override void _Ready()
	{
		_baslik = GetNode<Label>("%KartBasligi");
		_metin = GetNode<Label>("%KartMetni");
		_solSecenek = GetNode<Label>("%SolSecenek");
		_sagSecenek = GetNode<Label>("%SagSecenek");
	}

	public void KartiGoster(Kart kart, string hitap)
	{
		_baslik.Text = _karakterler.BasligiAl(kart.Konusan);
		_metin.Text = MetinYerlestirici.Yerlestir(kart.Metin, hitap);
		_solSecenek.Text = $"Sol: {kart.Sol.Etiket}";
		_sagSecenek.Text = $"Sağ: {kart.Sag.Etiket}";
	}
}
