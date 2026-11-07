using Godot;

public partial class AnaMenu : MenuSahnesi
{
    private const string YeniOyunSahnesi = "res://Sahneler/YeniOyun.tscn";
    private const string KayitliOyunlarSahnesi = "res://Sahneler/KayitliOyunlar.tscn";
    private const string AyarlarSahnesi = "res://Sahneler/Ayarlar.tscn";
    private const string CreditsSahnesi = "res://Sahneler/Credits.tscn";

    protected override string VarsayilanOdak => "YeniOyun";

    protected override void BaglantilariKur()
    {
        SahneGecisiBagla("YeniOyun", YeniOyunSahnesi);
        SahneGecisiBagla("KayitliOyun", KayitliOyunlarSahnesi);
        SahneGecisiBagla("Ayarlar", AyarlarSahnesi);
        SahneGecisiBagla("Credits", CreditsSahnesi);
        var cikis = DugmeBul("Cikis");
        if (cikis != null) cikis.Pressed += CikisYap;
    }

    private void SahneGecisiBagla(string dugmeAdi, string sahneYolu)
    {
        var dugme = DugmeBul(dugmeAdi);
        if (dugme == null)
        {
            GD.PushError($"Ana menü düğmesi bulunamadı: {dugmeAdi}");
            return;
        }

        dugme.Pressed += () => SahneyeGit(sahneYolu);
    }

    private void CikisYap() => GetTree().Quit();
}
