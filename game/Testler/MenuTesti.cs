using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// BUYRUK menüleri: ortak tema, üç ekran boyutunda yerleşim, düğme bağlantıları,
/// Escape ve Android geri tuşuyla geri dönüş, telefonda dikey ekran ve kalıcı ses ayarı.
/// </summary>
public partial class MenuTesti : Node
{
    private const string AnaMenuYolu = "res://Sahneler/AnaMenu.tscn";
    private static readonly string[] Menuler = { "AnaMenu", "YeniOyun", "KayitliOyunlar", "Ayarlar", "Credits" };
    private int _kontrol;
    private void Dogrula(bool kosul, string mesaj)
    {
        _kontrol++;
        if (!kosul) throw new InvalidOperationException(mesaj);
    }
    private async Task Bekle()
    {
        for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    public override async void _Ready()
    {
        // Ayar dosyası kullanıcının gerçek dosyasıdır; test sonunda eski hâline döndürülür.
        bool ayarVardi = FileAccess.FileExists(OyunAyarlari.Dosya);
        string eskiAyar = ayarVardi ? FileAccess.GetFileAsString(OyunAyarlari.Dosya) : null;
        try
        {
            foreach (var boyut in new[] { new Vector2I(1152, 648), new Vector2I(360, 640), new Vector2I(360, 800), new Vector2I(640, 360) })
            {
                GetWindow().Size = boyut;
                GetWindow().ContentScaleSize = boyut;
                GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
                GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Ignore;
                foreach (string ad in Menuler) await YerlesimiDene(ad, boyut);
            }
            TelefonAyarlariniDene();
            await GecisleriDene();
            await AyarlariDene();
            GD.Print($"MENÜ TESTLERİ BAŞARILI: {_kontrol} kontrol; tema, 4 ekran boyutu, dikey telefon, geçişler, Escape, geri tuşu, ses ayarı.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
        finally
        {
            if (ayarVardi) { using var f = FileAccess.Open(OyunAyarlari.Dosya, FileAccess.ModeFlags.Write); f?.StoreString(eskiAyar); }
            else if (FileAccess.FileExists(OyunAyarlari.Dosya)) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(OyunAyarlari.Dosya));
        }
    }

    private MenuSahnesi Ac(string ad, List<string> gecisler = null)
    {
        var ekran = GD.Load<PackedScene>($"res://Sahneler/{ad}.tscn").Instantiate<MenuSahnesi>();
        if (gecisler != null) ekran.SahneGecisi = gecisler.Add;
        AddChild(ekran);
        return ekran;
    }

    private async Task YerlesimiDene(string ad, Vector2I boyut)
    {
        var ekran = Ac(ad);
        await Bekle();
        Dogrula(ekran.Theme?.ResourcePath == "res://Assets/ui/krallik_tema.tres", $"{ad}: ortak tema atanmalı.");
        Dogrula(ekran.GetNode<Control>("Kaydirma").OffsetTop == 0, $"{ad}: masaüstünde güvenli alan boşluğu eklenmemeli.");
        var kaydirma = ekran.GetNode<ScrollContainer>("Kaydirma");
        Dogrula(kaydirma.GetHScrollBar().MaxValue <= kaydirma.Size.X + 1, $"{ad} {boyut}: yatay taşma.");
        var icerik = ekran.GetNode<Control>("%Icerik").GetGlobalRect();
        Dogrula(icerik.Position.X >= 15 && icerik.End.X <= boyut.X - 15, $"{ad} {boyut}: menü sütunu kenarlara değmemeli.");
        foreach (var dugum in ekran.FindChildren("*", "BaseButton", true, false))
        {
            var dugme = (Control)dugum;
            if (!dugme.IsVisibleInTree()) continue;
            Dogrula(dugme.Size.Y >= 48, $"{ad} {boyut}: {dugme.Name} dokunma hedefi küçük.");
            Dogrula(dugme.GetGlobalRect().End.X <= boyut.X, $"{ad} {boyut}: {dugme.Name} ekrandan taşıyor.");
        }
        if (DisplayServer.GetName() != "headless")
        {
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            DirAccess.MakeDirRecursiveAbsolute("res://.godot/testler");
            var hata = GetViewport().GetTexture().GetImage().SavePng($"res://.godot/testler/menu-{boyut.X}x{boyut.Y}-{ad}.png");
            Dogrula(hata == Error.Ok, "Ekran görüntüsü kaydedilemedi.");
        }
        ekran.Free();
    }

    private static void GeriTusu(Node ekran) => ekran.PropagateNotification((int)Node.NotificationWMGoBackRequest);

    /// <summary>G0 (7 Kasım, POCO X5 Pro) bulguları: oyun telefonda dikey açılır, geri tuşu uygulamayı kapatmaz.</summary>
    private void TelefonAyarlariniDene()
    {
        Dogrula((int)ProjectSettings.GetSetting("display/window/handheld/orientation") == (int)DisplayServer.ScreenOrientation.Portrait,
            "Oyun telefonda dikey açılmalı.");
        Dogrula((int)ProjectSettings.GetSetting("display/window/size/viewport_width") == 360 &&
            (int)ProjectSettings.GetSetting("display/window/size/viewport_height") == 640,
            "Temel boyut 360×640 olmalı; arayüz birimleri telefonda dp gibi okunur.");
        Dogrula(!(bool)ProjectSettings.GetSetting("application/config/quit_on_go_back", true),
            "Android geri tuşu uygulamayı doğrudan kapatmamalı; ekranlar kendisi karar verir.");
        // POCO X5 Pro: 1080×2400 ekranda kamera deliği üstte 94 piksel; arayüz 360×800 birim.
        Dogrula(Mathf.IsEqualApprox(GuvenliAlan.UstBosluk(new Rect2I(0, 94, 1080, 2306), new Vector2I(1080, 2400), 800), 94f / 3),
            "Kamera deliğinin üst boşluğu arayüz birimine çevrilmeli.");
        Dogrula(GuvenliAlan.UstBosluk(new Rect2I(0, 0, 1080, 2400), new Vector2I(1080, 2400), 800) == 0 &&
            GuvenliAlan.UstBosluk(new Rect2I(), Vector2I.Zero, 800) == 0, "Deliksiz ekranda ya da bilinmeyen boyutta boşluk eklenmemeli.");
    }

    private void Bas(MenuSahnesi ekran, string dugme) =>
        ekran.GetNode<Button>("%" + dugme).EmitSignal(BaseButton.SignalName.Pressed);

    private async Task GecisleriDene()
    {
        var gecisler = new List<string>();
        var ana = Ac("AnaMenu", gecisler);
        await Bekle();
        Dogrula(GetViewport().GuiGetFocusOwner() == ana.GetNode<Control>("%YeniOyun"), "Ana menüde odak Yeni Oyun'da başlamalı.");
        var beklenen = new Dictionary<string, string>
        {
            ["YeniOyun"] = "res://Sahneler/YeniOyun.tscn",
            ["KayitliOyun"] = "res://Sahneler/KayitliOyunlar.tscn",
            ["Ayarlar"] = "res://Sahneler/Ayarlar.tscn",
            ["Credits"] = "res://Sahneler/Credits.tscn",
        };
        foreach (var (dugme, yol) in beklenen)
        {
            gecisler.Clear();
            Bas(ana, dugme);
            Dogrula(gecisler.Count == 1 && gecisler[0] == yol, $"{dugme} doğru sahneyi açmalı.");
            Dogrula(ResourceLoader.Exists(yol), $"{yol} bulunmalı.");
        }
        int cikis = 0;
        ana.CikisGecisi = () => cikis++;
        gecisler.Clear();
        Bas(ana, "Cikis");
        Dogrula(cikis == 1 && gecisler.Count == 0, "Çıkış düğmesi uygulamadan çıkmalı.");
        GeriTusu(ana);
        Dogrula(cikis == 2 && gecisler.Count == 0, "Ana menüde Android geri tuşu uygulamadan çıkmalı.");
        ana.Free();

        foreach (string ad in new[] { "YeniOyun", "KayitliOyunlar", "Ayarlar", "Credits" })
        {
            var alt = Ac(ad, gecisler);
            await Bekle();
            gecisler.Clear();
            Bas(alt, "Geri");
            Dogrula(gecisler.Count == 1 && gecisler[0] == AnaMenuYolu, $"{ad}: geri düğmesi ana menüye dönmeli.");
            gecisler.Clear();
            GetViewport().PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = true }, true);
            GetViewport().PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = false }, true);
            await Bekle();
            Dogrula(gecisler.Count == 1 && gecisler[0] == AnaMenuYolu, $"{ad}: Escape ana menüye dönmeli.");
            gecisler.Clear();
            GeriTusu(alt);
            Dogrula(gecisler.Count == 1 && gecisler[0] == AnaMenuYolu, $"{ad}: Android geri tuşu ana menüye dönmeli.");
            if (ad == "YeniOyun")
            {
                gecisler.Clear();
                Bas(alt, "Devam");
                Dogrula(gecisler.Count == 1 && gecisler[0] == "res://Sahneler/Oyun.tscn", "Yeni Oyun, oyun sahnesini açmalı.");
            }
            alt.Free();
        }
        Dogrula(GD.Load<PackedScene>("res://Sahneler/Oyun.tscn") != null, "Oyun sahnesi yüklenmeli.");
        Dogrula((string)ProjectSettings.GetSetting("application/run/main_scene") == AnaMenuYolu, "Oyun ana menüyle açılmalı.");
    }

    private async Task AyarlariDene()
    {
        var ekran = Ac("Ayarlar", new List<string>());
        await Bekle();
        var ses = ekran.GetNode<HSlider>("%Ses");
        var etiket = ekran.GetNode<Label>("%SesBaslik");
        Dogrula((int)ses.Value == OyunAyarlari.SesDuzeyi, "Kaydırıcı kayıtlı ses düzeyini göstermeli.");
        Dogrula(ekran.GetNode<Button>("%TamEkran").Visible == OyunAyarlari.TamEkranDesteklenir,
            "Tam ekran seçeneği yalnız desteklenen ortamda görünmeli.");
        int ana = AudioServer.GetBusIndex("Master");
        ses.Value = 30;
        Dogrula(OyunAyarlari.SesDuzeyi == 30 && etiket.Text == "Ses düzeyi: %30", "Ses ayarı etikete yansımalı.");
        Dogrula(Mathf.IsEqualApprox(AudioServer.GetBusVolumeDb(ana), Mathf.LinearToDb(0.3f)) && !AudioServer.IsBusMute(ana),
            "Ses ayarı ana kanala uygulanmalı.");
        var dosya = new ConfigFile();
        Dogrula(dosya.Load(OyunAyarlari.Dosya) == Error.Ok && (int)dosya.GetValue("ayarlar", "ses", -1) == 30, "Ses ayarı kaydedilmeli.");
        ses.Value = 0;
        Dogrula(AudioServer.IsBusMute(ana) && etiket.Text == "Ses düzeyi: kapalı", "Sıfır ses kanalı susturmalı.");
        ses.Value = 70;
        Dogrula(!AudioServer.IsBusMute(ana), "Ses geri açılmalı.");
        ekran.Free();
    }
}
