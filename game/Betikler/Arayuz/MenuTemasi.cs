using Godot;

public static class MenuTemasi
{
    public static readonly Color Gece = new("120f14");
    public static readonly Color Koyu = new("241b22");
    public static readonly Color Altin = new("c99a4a");
    public static readonly Color Fildisi = new("f4e6c8");
    public static readonly Color Soluk = new("bcae9a");

    public static void Dugme(Button dugme, bool ikincil = false)
    {
        var normal = new StyleBoxFlat
        {
            BgColor = ikincil ? new Color("342832") : Koyu,
            BorderColor = Altin,
            BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5,
            ContentMarginLeft = 18, ContentMarginRight = 18, ContentMarginTop = 12, ContentMarginBottom = 12
        };
        var hover = (StyleBoxFlat)normal.Duplicate();
        hover.BgColor = new Color("553a2f");
        dugme.AddThemeStyleboxOverride("normal", normal);
        dugme.AddThemeStyleboxOverride("hover", hover);
        dugme.AddThemeStyleboxOverride("pressed", hover);
        dugme.AddThemeColorOverride("font_color", Fildisi);
        dugme.AddThemeFontSizeOverride("font_size", 18);
    }

    public static void Uygula(Control ekran)
    {
        ekran.GetNode<ColorRect>("Zemin").Color = Gece;
        UygulaDugmeTemasi(ekran);
        UygulaMetinTemasi(ekran);
    }

    private static void UygulaDugmeTemasi(Control ekran)
    {
        foreach (var dugme in ekran.FindChildren("*", "Button", true, false))
            if (dugme.IsInGroup("menu_dugmesi")) Dugme((Button)dugme);
    }

    private static void UygulaMetinTemasi(Control ekran)
    {
        foreach (var etiket in ekran.FindChildren("*", "Label", true, false))
            if (etiket.IsInGroup("menu_metin")) ((Label)etiket).AddThemeColorOverride("font_color", Fildisi);
    }
}
