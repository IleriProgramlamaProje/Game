using Godot;

/// <summary>
/// Ses düzeyi ve tam ekran tercihini <c>user://ayarlar.cfg</c> içinde saklar ve uygular.
/// Oyun ilk menüyü açtığında bir kez yüklenir; Ayarlar ekranı her değişiklikte kaydeder.
/// </summary>
public static class OyunAyarlari
{
    public const string Dosya = "user://ayarlar.cfg";
    private const string Bolum = "ayarlar";
    private static bool _yuklendi;

    /// <summary>0–100 arası ana ses düzeyi.</summary>
    public static int SesDuzeyi { get; private set; } = 70;
    public static bool TamEkran { get; private set; }

    /// <summary>Tam ekran yalnız masaüstünde anlamlıdır; telefonda uygulama zaten tam ekrandır.</summary>
    public static bool TamEkranDesteklenir => !OS.HasFeature("mobile") && !OS.HasFeature("web") && DisplayServer.GetName() != "headless";

    public static void IlkKezUygula()
    {
        if (_yuklendi) return;
        _yuklendi = true;
        var dosya = new ConfigFile();
        if (dosya.Load(Dosya) == Error.Ok)
        {
            SesDuzeyi = Mathf.Clamp((int)dosya.GetValue(Bolum, "ses", SesDuzeyi), 0, 100);
            TamEkran = (bool)dosya.GetValue(Bolum, "tam_ekran", TamEkran);
        }
        SesiUygula();
        if (TamEkran) EkraniUygula();
    }

    public static void SesiAyarla(int duzey)
    {
        SesDuzeyi = Mathf.Clamp(duzey, 0, 100);
        SesiUygula();
        Kaydet();
    }

    public static void TamEkraniAyarla(bool tamEkran)
    {
        TamEkran = tamEkran;
        EkraniUygula();
        Kaydet();
    }

    private static void SesiUygula()
    {
        int ana = AudioServer.GetBusIndex("Master");
        AudioServer.SetBusMute(ana, SesDuzeyi == 0);
        AudioServer.SetBusVolumeDb(ana, Mathf.LinearToDb(SesDuzeyi / 100f));
    }

    private static void EkraniUygula()
    {
        if (!TamEkranDesteklenir) return;
        DisplayServer.WindowSetMode(TamEkran ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }

    private static void Kaydet()
    {
        var dosya = new ConfigFile();
        dosya.SetValue(Bolum, "ses", SesDuzeyi);
        dosya.SetValue(Bolum, "tam_ekran", TamEkran);
        var hata = dosya.Save(Dosya);
        if (hata != Error.Ok) GD.PushWarning($"Ayarlar kaydedilemedi: {hata}");
    }
}
