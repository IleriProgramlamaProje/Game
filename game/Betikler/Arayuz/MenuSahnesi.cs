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
        GuvenliAlan.MobilOlcegiAyarla(GetWindow());
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

    // Projede nullable başvuru türleri etkin olmadığından bulunamama durumu null ile döner.
    protected Button DugmeBul(string benzersizAd) => GetNodeOrNull<Button>("%" + benzersizAd);

    // Dar telefonda iki yanda 16 birim kalır; geniş ekranda sütun okunur genişlikte durur.
    private void Yerlesim()
    {
        var icerik = GetNodeOrNull<Control>("%Icerik");
        var guvenli = GuvenliAlan.Kenarlar(GetWindow());
        if (GetNodeOrNull<ScrollContainer>("Kaydirma") is { } kaydirma)
        {
            kaydirma.OffsetLeft = guvenli.X;
            kaydirma.OffsetTop = guvenli.Y;
            kaydirma.OffsetRight = -guvenli.Z;
            kaydirma.OffsetBottom = -guvenli.W;
        }
        if (icerik != null && Size.X > 0)
            icerik.CustomMinimumSize = new Vector2(Mathf.Clamp(Size.X - guvenli.X - guvenli.Z - 32, 0, IcerikGenisligi), 0);
    }

    protected virtual void VarsayilanOdagaGec()
    {
        if (!string.IsNullOrWhiteSpace(VarsayilanOdak))
            DugmeBul(VarsayilanOdak)?.GrabFocus(true);
    }
}
