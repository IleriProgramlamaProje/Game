using Godot;

internal static class GuvenliAlan
{
    public static void MobilOlcegiAyarla(Window pencere)
    {
        if (!OS.HasFeature("mobile")) return;
        pencere.ContentScaleSize = new Vector2I(360, 640);
        pencere.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
    }

    public static Vector4 Kenarlar(Window pencere)
    {
        if (!OS.HasFeature("mobile")) return Vector4.Zero;
        var fiziksel = new Rect2I(pencere.Position, pencere.Size);
        var guvenli = fiziksel.Intersection(DisplayServer.GetDisplaySafeArea());
        if (!guvenli.HasArea() || !fiziksel.HasArea()) return Vector4.Zero;
        var olcek = pencere.GetVisibleRect().Size / (Vector2)fiziksel.Size;
        var bas = (Vector2)(guvenli.Position - fiziksel.Position) * olcek;
        var son = (Vector2)(fiziksel.End - guvenli.End) * olcek;
        return new Vector4(Mathf.Ceil(bas.X), Mathf.Ceil(bas.Y), Mathf.Ceil(son.X), Mathf.Ceil(son.Y));
    }
}
