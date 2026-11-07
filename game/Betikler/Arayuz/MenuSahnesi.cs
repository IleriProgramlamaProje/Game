using Godot;

/// <summary>
/// BUYRUK menülerinin ortak yaşam döngüsünü ve sahne geçişlerini yönetir.
/// Görünüm sahnelere atanmış ortak <c>krallik_tema.tres</c> temasından gelir;
/// her alt sınıf yalnızca kendi düğmelerini ve ekran davranışını tanımlar.
/// </summary>
public abstract partial class MenuSahnesi : Control
{
    /// <summary>Menü sütununun geniş ekrandaki azami genişliği.</summary>
    [Export] public float IcerikGenisligi { get; set; } = 420;

    /// <summary>Hazır olduğunda klavye odağını alacak düğmenin benzersiz adı.</summary>
    protected virtual string VarsayilanOdak => string.Empty;

    public sealed override void _Ready()
    {
        OyunAyarlari.IlkKezUygula();
        Resized += Yerlesim;
        Yerlesim();
        BaglantilariKur();
        VarsayilanOdagaGec();
    }

    protected abstract void BaglantilariKur();

    /// <summary>
    /// Atanırsa sahne değiştirmek yerine çağrılır. Testler menüyü kendi sahnesini
    /// kaybetmeden denemek için kullanır; oyunda boş kalır.
    /// </summary>
    public System.Action<string> SahneGecisi { get; set; }

    protected void SahneyeGit(string sahneYolu)
    {
        if (string.IsNullOrWhiteSpace(sahneYolu))
        {
            GD.PushWarning("Geçerli bir sahne yolu verilmedi.");
            return;
        }

        if (SahneGecisi != null) SahneGecisi(sahneYolu);
        else GetTree().ChangeSceneToFile(sahneYolu);
    }

    /// <summary>Atanırsa uygulamadan çıkmak yerine çağrılır; testler için.</summary>
    public System.Action CikisGecisi { get; set; }

    protected void UygulamadanCik()
    {
        if (CikisGecisi != null) CikisGecisi();
        else GetTree().Quit();
    }

    /// <summary>
    /// Android geri tuşu ya da geri hareketi. <c>quit_on_go_back</c> kapalı olduğundan
    /// uygulama kendiliğinden kapanmaz; her ekran kendi geri davranışını seçer.
    /// </summary>
    protected abstract void GeriTusu();

    public override void _Notification(int what)
    {
        if (what == NotificationWMGoBackRequest) GeriTusu();
    }

    // Projede nullable başvuru türleri etkin olmadığından bulunamama durumu null ile döner.
    protected Button DugmeBul(string benzersizAd) => GetNodeOrNull<Button>("%" + benzersizAd);

    // Dar telefonda iki yanda 16 birim kalır; geniş ekranda sütun okunur genişlikte durur.
    private void Yerlesim()
    {
        GuvenliAlan.Uygula(GetNodeOrNull<Control>("Kaydirma"));
        var icerik = GetNodeOrNull<Control>("%Icerik");
        if (icerik != null && Size.X > 0)
            icerik.CustomMinimumSize = new Vector2(Mathf.Min(IcerikGenisligi, Size.X - 32), 0);
    }

    private void VarsayilanOdagaGec()
    {
        if (!string.IsNullOrWhiteSpace(VarsayilanOdak))
            DugmeBul(VarsayilanOdak)?.GrabFocus(true);
    }
}
