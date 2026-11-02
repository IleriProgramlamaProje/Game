using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public partial class SahneIskeletiTesti : Node
{
    private int _kontrol;
    private void Dogrula(bool kosul, string mesaj)
    {
        _kontrol++;
        if (!kosul) throw new InvalidOperationException(mesaj);
    }
    private void HataBekle(Action islem)
    {
        try { islem(); } catch (FormatException) { _kontrol++; return; }
        throw new InvalidOperationException("Bozuk veri reddedilmedi.");
    }
    private static string Oku(string yol) => Godot.FileAccess.GetFileAsString(yol);
    public override async void _Ready()
    {
        try
        {
            CekirdegiDene();
            await SahneyiDene();
            GD.Print($"H1 TESTLERİ BAŞARILI: {_kontrol} kontrol; çekirdek, deste, fare/dokunma, 3 ekran boyutu.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
    private void CekirdegiDene()
    {
        var tema = Tema.Oku(Oku("res://Veri/tema.json"));
        var roller = tema.Karakterler.Select(r => r.Anahtar).ToHashSet();
        string json = Oku("res://Veri/yedek_deste.json");
        var deste = new YedekDeste(json, roller);
        Dogrula(deste.Adet == 10, "On örnek kart bulunmalı.");
        var ilk = deste.Sonraki();
        for (int i = 0; i < 9; i++) deste.Sonraki();
        Dogrula(deste.Sonraki() == ilk, "Deste sırayla başa dönmeli.");
        deste.Sifirla();
        Dogrula(deste.Sonraki() == ilk, "Yeni oyunda deste sıfırlanmalı.");
        foreach (string bozuk in new[] { "{", "[]", "null", "[null]", "[{}]", json.Replace("\"para\": -10", "\"para\": -20"),
                    json.Replace("\"hazinedar\"", "\"yok\"") })
            HataBekle(() => new YedekDeste(bozuk, roller));
        HataBekle(() => Tema.Oku("{}"));
        var g = new Gostergeler();
        Dogrula(g.Degerler.Values.All(v => v == 50), "Başlangıç 50 olmalı.");
        g.Uygula(new Dictionary<string, int> { ["para"] = -15, ["halk"] = 10 });
        Dogrula(g.Degerler["para"] == 35 && g.Degerler["halk"] == 60 && g.Degerler["ordu"] == 50, "Etkiler yanlış.");
        HataBekle(() => g.Uygula(new Dictionary<string, int> { ["para"] = -5, ["yok"] = 5 }));
        Dogrula(g.Degerler["para"] == 35, "Geçersiz etki kısmen uygulanmamalı.");
        for (int i = 0; i < 5; i++) g.Uygula(new Dictionary<string, int> { ["para"] = -15, ["halk"] = 15 });
        Dogrula(g.Degerler["para"] == 0 && g.Degerler["halk"] == 100 && g.Sinirda, "İki sınır da tutulmalı.");
        var isimler = new MetinYerlestirici(tema.Karakterler, Hukumdar.Hatun, new Random(7));
        string baslik = isimler.Baslik("hazinedar");
        Dogrula(baslik == isimler.Baslik("hazinedar"), "İsim oturumda değişmemeli.");
        Dogrula(isimler.Yerlestir("{hitap}, hoş geldiniz.") == "Hatunum, hoş geldiniz.", "Hitap yanlış.");
        Dogrula(isimler.Yerlestir("Sade metin.") == "Sade metin.", "Metne zorla hitap eklenmemeli.");
        foreach (var rol in tema.Karakterler)
        {
            var kimlik = isimler.Kimlik(rol.Anahtar);
            Dogrula(rol.Cinsiyet == "Çift" || kimlik.Cinsiyet == rol.Cinsiyet, "Rol cinsiyeti yanlış.");
            Dogrula(!string.IsNullOrEmpty(kimlik.PortreAnahtari), "Portre eşlemesi oturumda hazır olmalı.");
        }
        var oyun = new OyunYoneticisi(deste);
        Dogrula(!oyun.KararVer(false, 0) && !oyun.DevamEt(), "Açılışta karar verilmemeli.");
        oyun.Baslat(Hukumdar.Bey);
        long id = oyun.KartKimligi;
        Dogrula(oyun.KararVer(false, id), "İlk karar uygulanmalı.");
        Dogrula(!oyun.KararVer(true, id) && oyun.Durum.KararSayisi == 1, "Çift giriş tek sayılmalı.");
        oyun.DevamEt();
        Dogrula(!oyun.KararVer(true, id), "Eski kartın girdisi reddedilmeli.");
        for (int i = 0; i < 100 && oyun.Asama != Asama.Son; i++)
        { oyun.KararVer(false, oyun.KartKimligi); oyun.DevamEt(); }
        Dogrula(oyun.Asama == Asama.Son && !oyun.KararVer(true, oyun.KartKimligi), "Sınırda oyun durmalı.");
        oyun.Baslat(Hukumdar.Hatun);
        Dogrula(oyun.Durum.KararSayisi == 0 && oyun.Durum.Gostergeler.Degerler.Values.All(v => v == 50), "Yeni oyun sıfırlanmalı.");
        Dogrula(!oyun.KararVer(false, id), "Önceki oturumun girdisi reddedilmeli.");
    }
    private async Task Bekle()
    {
        for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    private void Girdi(InputEvent olay) => GetViewport().PushInput(olay, true);
    private void Fare(bool basili, Vector2 konum) => Girdi(new InputEventMouseButton
    { ButtonIndex = MouseButton.Left, Pressed = basili, Position = konum, GlobalPosition = konum });
    private void Hareket(Vector2 konum) => Girdi(new InputEventMouseMotion
    { Position = konum, GlobalPosition = konum, ButtonMask = MouseButtonMask.Left });

    private async Task SahneyiDene()
    {
        var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
        foreach (var boyut in new[] { new Vector2I(1152, 648), new Vector2I(360, 640), new Vector2I(640, 360) })
        {
            GetWindow().Size = boyut;
            GetWindow().ContentScaleSize = boyut;
            GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
            GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Ignore;
            var ekran = sahne.Instantiate<OyunEkranlari>();
            AddChild(ekran);
            await Bekle();
            Dogrula(ekran.Oyun != null && ekran.AktifEkran == OyunEkranlari.Ekran.Acilis, "Açılış/veri yükleme yanlış.");
            await YerlesimVeGoruntu(ekran, boyut);
            ekran.GetNode<OptionButton>("%Cinsiyet").Select(1);
            ekran.GetNode<Button>("%Baslat").EmitSignal(BaseButton.SignalName.Pressed);
            await Bekle();
            Dogrula(ekran.Oyun.Durum.Cinsiyet == Hukumdar.Hatun && ekran.GetNode<Label>("%KartMetni").Text.Contains("Hatunum"), "Seçim karta yansımalı.");
            await YerlesimVeGoruntu(ekran, boyut);
            // Kart dar/yatay ekranda kaydırma alanı içine alınır.
            var scroll = ekran.GetNode<ScrollContainer>("%Icerik");
            var kart = ekran.GetNode<KaydirmaKontrolu>("%KartAlani");
            scroll.EnsureControlVisible(kart);
            await Bekle();
            Vector2 bas = kart.GlobalPosition + new Vector2(kart.Size.X / 2, 35);
            Fare(true, bas); Hareket(bas + new Vector2(20, 0));
            Dogrula(ekran.Oyun.Durum.KararSayisi == 0, "Önizleme karar uygulamamalı.");
            Fare(false, bas + new Vector2(20, 0));
            Dogrula(ekran.Oyun.Asama == Asama.Kart, "Kısa sürükleme iptal olmalı.");
            Fare(true, bas); Hareket(bas + new Vector2(kart.Esik + 3, 0));
            Dogrula(ekran.GetNode<Label>("%KartMetni").Text.Contains("▲") || ekran.GetNode<Label>("%KartMetni").Text.Contains("▼"), "Önizlemede etki okları olmalı.");
            await YerlesimVeGoruntu(ekran, boyut, "-Onizleme");
            var onizleme = ekran.GetNode<Label>("%KartMetni");
            Dogrula(onizleme.GlobalPosition.Y >= 0 && onizleme.GlobalPosition.Y + onizleme.Size.Y <= boyut.Y,
                "Kaydırma önizlemesi tamamen görünür olmalı.");
            Fare(false, bas + new Vector2(kart.Esik + 3, 0));
            await Bekle();
            Dogrula(ekran.Oyun.Durum.KararSayisi == 1 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Fare sürüklemesi bir karar vermeli.");
            Fare(false, bas);
            Dogrula(ekran.Oyun.Durum.KararSayisi == 1, "İkinci bırakma etkisiz olmalı.");
            await YerlesimVeGoruntu(ekran, boyut);
            ekran.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
            await Bekle();
            scroll.EnsureControlVisible(kart);
            await Bekle();
            bas = kart.GlobalPosition + new Vector2(kart.Size.X / 2, 35);
            Girdi(new InputEventScreenTouch { Index = 0, Pressed = true, Position = bas });
            Girdi(new InputEventScreenTouch { Index = 1, Pressed = true, Position = bas });
            Girdi(new InputEventScreenDrag { Index = 1, Position = bas + new Vector2(-kart.Esik - 5, 0) });
            Girdi(new InputEventScreenTouch { Index = 1, Pressed = false, Position = bas + new Vector2(-kart.Esik - 5, 0) });
            Dogrula(ekran.Oyun.Durum.KararSayisi == 1, "İkinci parmak karar verememeli.");
            Girdi(new InputEventScreenDrag { Index = 0, Position = bas + new Vector2(-kart.Esik - 5, 0) });
            Girdi(new InputEventScreenTouch { Index = 0, Pressed = false, Position = bas + new Vector2(-kart.Esik - 5, 0) });
            await Bekle();
            Dogrula(ekran.Oyun.Durum.KararSayisi == 2, "İlk parmak tek karar vermeli.");
            ekran.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
            await Bekle();
            // Klavye erişimi: odaklı düğmeye Enter.
            ekran.GetNode<Button>("%SolKarar").GrabFocus();
            await Bekle();
            var odak = ekran.GetNode<Button>("%SolKarar");
            Dogrula(odak.GlobalPosition.Y >= 0 && odak.GlobalPosition.Y + odak.Size.Y <= boyut.Y,
                "Odaklı karar düğmesi görünür olmalı.");
            await YerlesimVeGoruntu(ekran, boyut, "-Klavye");
            Girdi(new InputEventKey { Keycode = Key.Enter, Pressed = true });
            Girdi(new InputEventKey { Keycode = Key.Enter, Pressed = false });
            await Bekle();
            Dogrula(ekran.Oyun.Durum.KararSayisi == 3, "Enter ile karar verilebilmeli.");
            for (int i = 0; i < 100 && ekran.AktifEkran != OyunEkranlari.Ekran.Son; i++)
            {
                if (ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc) ekran.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
                else ekran.GetNode<Button>("%SolKarar").EmitSignal(BaseButton.SignalName.Pressed);
            }
            Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Son, "Oyun sınırda sona ermeli.");
            await YerlesimVeGoruntu(ekran, boyut);
            ekran.GetNode<Button>("%Yeniden").EmitSignal(BaseButton.SignalName.Pressed);
            ekran.GetNode<Button>("%Baslat").EmitSignal(BaseButton.SignalName.Pressed);
            Dogrula(ekran.Oyun.Durum.KararSayisi == 0, "Yeniden başlama sayacı sıfırlamalı.");
            ekran.Free();
        }
    }
    private async Task YerlesimVeGoruntu(OyunEkranlari ekran, Vector2I boyut, string ek = "")
    {
        await Bekle();
        foreach (var e in Enum.GetValues<OyunEkranlari.Ekran>())
            Dogrula(ekran.GetNode<Control>("%" + e + "Paneli").IsVisibleInTree() == (e == ekran.AktifEkran), "Tek panel görünmeli.");
        var scroll = ekran.GetNode<ScrollContainer>("%Icerik");
        Dogrula(scroll.GetHScrollBar().MaxValue <= scroll.Size.X + 1, $"{boyut}: yatay taşma.");
        foreach (string ad in new[] { "Baslat", "SolKarar", "SagKarar", "Devam", "Yeniden" })
        {
            var dugme = ekran.GetNode<Button>("%" + ad);
            if (dugme.IsVisibleInTree()) Dogrula(dugme.Size.Y >= 48, "Dokunma hedefi küçük.");
        }
        if (DisplayServer.GetName() != "headless")
        {
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            DirAccess.MakeDirRecursiveAbsolute("res://.godot/testler");
            var hata = GetViewport().GetTexture().GetImage().SavePng($"res://.godot/testler/{boyut.X}x{boyut.Y}-{ekran.AktifEkran}{ek}.png");
            Dogrula(hata == Error.Ok, "Ekran görüntüsü kaydedilemedi.");
        }
    }
}
