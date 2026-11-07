using Godot;

/// <summary>
/// Telefonun kamera deliği ya da çentiği için üst güvenli boşluk. Android oyunu tam ekran ve
/// deliğin çevresine de çizer (<c>shortEdges</c>); kaydırma alanları bu boşluk kadar aşağıdan
/// başlar, zemin yine tam ekrandır. G0: POCO X5 Pro'da delik üst ortada 94 piksel (≈31 birim).
/// </summary>
public static class GuvenliAlan
{
    /// <summary>Ekran pikselindeki güvenli alanın üst boşluğunu arayüz birimine çevirir.</summary>
    public static float UstBosluk(Rect2I guvenliAlan, Vector2I pencere, float arayuzYuksekligi)
    {
        if (pencere.Y <= 0 || guvenliAlan.Size.Y <= 0) return 0;
        return Mathf.Max(0, guvenliAlan.Position.Y) * arayuzYuksekligi / pencere.Y;
    }

    /// <summary>
    /// Yalnız telefonda uygulanır. Masaüstünde Godot ekranın görev çubuğu hariç alanını verir;
    /// bu pencereyle ilgili olmadığından orada boşluk eklenmez.
    /// </summary>
    public static float UstBosluk(Control dugum)
    {
        if (!OS.HasFeature("mobile")) return 0;
        return UstBosluk(DisplayServer.GetDisplaySafeArea(), DisplayServer.WindowGetSize(), dugum.GetViewportRect().Size.Y);
    }

    /// <summary>Tam ekrana yayılmış kaydırma alanının üst kenarını güvenli alana indirir.</summary>
    public static void Uygula(Control kaydirma)
    {
        if (kaydirma != null) kaydirma.OffsetTop = UstBosluk(kaydirma);
    }
}
