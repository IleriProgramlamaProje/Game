using Godot;

/// <summary>
/// BUYRUK menülerinin ortak yaşam döngüsünü ve sahne geçişlerini yönetir.
/// Her alt sınıf yalnızca kendi düğmelerini ve ekran davranışını tanımlar.
/// </summary>
public abstract partial class MenuSahnesi : Control
{
    /// <summary>Hazır olduğunda klavye odağını alacak düğmenin benzersiz adı.</summary>
    protected virtual string VarsayilanOdak => string.Empty;

    public sealed override void _Ready()
    {
        MenuTemasi.Uygula(this);
        BaglantilariKur();
        VarsayilanOdagaGec();
    }

    protected abstract void BaglantilariKur();

    protected void SahneyeGit(string sahneYolu)
    {
        if (string.IsNullOrWhiteSpace(sahneYolu))
        {
            GD.PushWarning("Geçerli bir sahne yolu verilmedi.");
            return;
        }

        GetTree().ChangeSceneToFile(sahneYolu);
    }

    // Projede nullable başvuru türleri etkin olmadığından bulunamama durumu null ile döner.
    protected Button DugmeBul(string benzersizAd) => GetNodeOrNull<Button>("%" + benzersizAd);

    private void VarsayilanOdagaGec()
    {
        if (!string.IsNullOrWhiteSpace(VarsayilanOdak))
            DugmeBul(VarsayilanOdak)?.GrabFocus();
    }
}
