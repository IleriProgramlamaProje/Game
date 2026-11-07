using Godot;

/// <summary>Ses düzeyi ve tam ekran seçeneklerini gerçek ayarlara bağlar.</summary>
public partial class AyarlarEkrani : AltEkran
{
    protected override void BaglantilariKur()
    {
        base.BaglantilariKur();
        var ses = GetNode<HSlider>("%Ses");
        var sesEtiketi = GetNode<Label>("%SesBaslik");
        ses.SetValueNoSignal(OyunAyarlari.SesDuzeyi);
        sesEtiketi.Text = SesMetni(OyunAyarlari.SesDuzeyi);
        ses.ValueChanged += deger =>
        {
            OyunAyarlari.SesiAyarla((int)deger);
            sesEtiketi.Text = SesMetni(OyunAyarlari.SesDuzeyi);
        };

        var tamEkran = GetNode<Button>("%TamEkran");
        tamEkran.Visible = OyunAyarlari.TamEkranDesteklenir;
        tamEkran.SetPressedNoSignal(OyunAyarlari.TamEkran);
        tamEkran.Text = TamEkranMetni(OyunAyarlari.TamEkran);
        tamEkran.Toggled += acik =>
        {
            OyunAyarlari.TamEkraniAyarla(acik);
            tamEkran.Text = TamEkranMetni(acik);
        };
    }

    public static string TamEkranMetni(bool acik) => acik ? "TAM EKRAN: AÇIK" : "TAM EKRAN: KAPALI";

    public static string SesMetni(int duzey) => duzey == 0 ? "Ses düzeyi: kapalı" : $"Ses düzeyi: %{duzey}";
}
