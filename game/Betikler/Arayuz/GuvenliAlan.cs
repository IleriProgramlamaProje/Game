using Godot;

/// <summary>
/// Telefonun kamera deliği, çentiği ve sistem çubukları için güvenli kenar boşlukları.
/// Android oyunu tam ekran ve deliğin çevresine de çizer (<c>shortEdges</c>); menü ve oyun
/// içeriği bu boşlukların içinde kalır, zemin yine tam ekrandır.
/// G0: POCO X5 Pro'da (1080×2400) delik üst ortada 94 piksel, yani 360×800 arayüzde 32 birim.
/// </summary>
public static class GuvenliAlan
{
    /// <summary>
    /// Ekran pikselindeki güvenli alanı pencerenin arayüz birimine çevirir.
    /// Dönen değer: (sol, üst, sağ, alt) kenar boşlukları; güvenli alan bilinmiyorsa sıfır.
    /// </summary>
    public static Vector4 Kenarlar(Rect2I pencere, Rect2I guvenliAlan, Vector2 arayuz)
    {
        var guvenli = pencere.Intersection(guvenliAlan);
        if (!guvenli.HasArea() || !pencere.HasArea()) return Vector4.Zero;
        var olcek = arayuz / (Vector2)pencere.Size;
        var bas = (Vector2)(guvenli.Position - pencere.Position) * olcek;
        var son = (Vector2)(pencere.End - guvenli.End) * olcek;
        return new Vector4(Mathf.Ceil(bas.X), Mathf.Ceil(bas.Y), Mathf.Ceil(son.X), Mathf.Ceil(son.Y));
    }

    /// <summary>
    /// Yalnız telefonda uygulanır. Masaüstünde Godot ekranın görev çubuğu hariç alanını verir;
    /// bu pencereyle ilgili olmadığından orada boşluk eklenmez.
    /// </summary>
    public static Vector4 Kenarlar(Window pencere)
    {
        if (!OS.HasFeature("mobile")) return Vector4.Zero;
        return Kenarlar(new Rect2I(pencere.Position, pencere.Size), DisplayServer.GetDisplaySafeArea(), pencere.GetVisibleRect().Size);
    }
}
