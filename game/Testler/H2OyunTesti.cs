using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

public partial class H2OyunTesti : Node
{
    private int _kontrol;
    private Tema _tema;
    private Kart _ornek;
    private string _geciciYol;
    private void Dogrula(bool kosul, string mesaj)
    {
        _kontrol++;
        if (!kosul) throw new InvalidOperationException(mesaj);
    }
    private void TemaHatasi(JsonNode json)
    {
        try { Tema.Oku(json.ToJsonString()); }
        catch (FormatException) { _kontrol++; return; }
        throw new InvalidOperationException("Eksik tema kabul edildi.");
    }
    private static string Oku(string yol) => Godot.FileAccess.GetFileAsString(yol);
    private YedekDeste Deste(Kart kart) => new(JsonSerializer.Serialize(new[] { kart }),
        _tema.Karakterler.Select(r => r.Anahtar).ToHashSet());
    private Kart EtkiliKart(Dictionary<string, int> etkiler) => _ornek with
    {
        Sol = _ornek.Sol with { Etki = etkiler, Sonuc = "{hitap}, sol kararın ardından köprü onarıldı." },
        Sag = _ornek.Sag with { Etki = new(etkiler), Sonuc = "{hitap}, sağ kararın ardından çarşı açıldı." }
    };
    private Kart SonKarti(string anahtar, int sinir)
    {
        var etkiler = new Dictionary<string, int> { [anahtar] = sinir == 0 ? -15 : 15 };
        if (sinir == 100) etkiler[anahtar == "para" ? "halk" : "para"] = -5;
        return EtkiliKart(etkiler);
    }
    public override async void _Ready()
    {
        _geciciYol = $"user://h2-test-{Guid.NewGuid():N}.json";
        try
        {
            _tema = Tema.Oku(Oku("res://Veri/tema.json"));
            _ornek = JsonSerializer.Deserialize<Kart[]>(Oku("res://Testler/Veri/ornek_deste.json"))[0];
            CekirdegiDene();
            await SahneleriDene();
            GD.Print($"H2 TESTLERİ BAŞARILI: {_kontrol} kontrol; sekiz son, yıl, sonuç, yeniden oyun, bozuk veri, üç boyut.");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
        finally
        {
            if (Godot.FileAccess.FileExists(_geciciYol))
                DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(_geciciYol));
        }
    }
    private void CekirdegiDene()
    {
        string temaJson = Oku("res://Veri/tema.json");
        foreach (string anahtar in Gostergeler.Siralama.SelectMany(k => new[] { k + "_0", k + "_100" }))
        {
            var bozuk = JsonNode.Parse(temaJson);
            bozuk["sonlar"].AsObject().Remove(anahtar);
            TemaHatasi(bozuk);
            bozuk = JsonNode.Parse(temaJson);
            bozuk["sonlar"][anahtar]["metin"] = " ";
            TemaHatasi(bozuk);
        }
        foreach (int sayi in new[] { 0, -1 })
        {
            var bozuk = JsonNode.Parse(temaJson);
            bozuk["yil"]["kac_kartta_bir_yil"] = sayi;
            TemaHatasi(bozuk);
        }
        foreach (string gosterge in Gostergeler.Siralama)
        foreach (int sinir in new[] { 0, 100 })
        {
            var oyun = new OyunYoneticisi(Deste(SonKarti(gosterge, sinir)));
            oyun.Baslat(Hukumdar.Bey);
            for (int i = 0; i < 4; i++)
            {
                long kimlik = oyun.KartKimligi;
                Dogrula(oyun.KararVer(false, kimlik), "Kontrollü karar kabul edilmeli.");
                Dogrula(!oyun.KararVer(true, kimlik), "Sonuçta ikinci karar reddedilmeli.");
                Dogrula(oyun.Asama == Asama.Sonuc && oyun.SonAnahtarlari.Count == 0, "Önce sonuç gösterilmeli.");
                oyun.DevamEt();
                Dogrula(oyun.Asama == (i == 3 ? Asama.Son : Asama.Kart), "Son yalnız sınırda açılmalı.");
            }
            Dogrula(oyun.SonAnahtarlari.SequenceEqual(new[] { $"{gosterge}_{sinir}" }), "Yanlış son seçildi.");
            Dogrula(!oyun.DevamEt() && !oyun.KararVer(false, oyun.KartKimligi), "Son ekranı girdi kabul etmemeli.");
            long eskiKimlik = oyun.KartKimligi;
            oyun.AcilisaDon();
            Dogrula(oyun.Asama == Asama.Acilis && oyun.Durum == null && oyun.AktifKart == null &&
                oyun.SonKarar == null && oyun.SonAnahtarlari.Count == 0, "Açılışa dönüş tüm oturumu temizlemeli.");
            oyun.Baslat(Hukumdar.Hatun);
            Dogrula(oyun.Durum.KararSayisi == 0 && oyun.Durum.Zaman.TamamlananYil == 0 &&
                oyun.Durum.Gostergeler.Degerler.Values.All(v => v == 50), "Yeni oyun tüm sayıları sıfırlamalı.");
            Dogrula(!oyun.KararVer(true, eskiKimlik), "Eski oturumun girdisi reddedilmeli.");
        }
        var ciftSon = new OyunYoneticisi(Deste(EtkiliKart(new() { ["para"] = -15, ["halk"] = 15 })));
        ciftSon.Baslat(Hukumdar.Bey);
        for (int i = 0; i < 4; i++) { ciftSon.KararVer(false, ciftSon.KartKimligi); ciftSon.DevamEt(); }
        Dogrula(ciftSon.SonAnahtarlari.SequenceEqual(new[] { "halk_100", "para_0" }), "Eşzamanlı sonlardan biri kayboldu.");

        var dengeli = EtkiliKart(new() { ["para"] = -5, ["halk"] = 5 });
        dengeli = dengeli with { Sag = dengeli.Sag with { Etki = new() { ["para"] = 5, ["halk"] = -5 } } };
        foreach (int oran in new[] { 5, 3 })
        {
            var oyun = new OyunYoneticisi(Deste(dengeli), oran);
            oyun.Baslat(Hukumdar.Bey);
            for (int i = 1; i <= 10; i++)
            {
                long kimlik = oyun.KartKimligi;
                oyun.KararVer(i % 2 == 0, kimlik);
                oyun.KararVer(false, kimlik);
                Dogrula(oyun.Durum.KararSayisi == i && oyun.Durum.Zaman.TamamlananYil == i / oran,
                    "Yıl yalnız onaylanan kararlarla ve tema oranıyla artmalı.");
                oyun.DevamEt();
                oyun.DevamEt();
                Dogrula(oyun.Durum.KararSayisi == i && oyun.Durum.Zaman.SonrakiYilaKalan == oran - i % oran,
                    "Devam Et yılı değiştirmemeli.");
            }
        }
    }
    private async Task Bekle()
    {
        for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    private void Yaz(string metin)
    {
        using var dosya = Godot.FileAccess.Open(_geciciYol, Godot.FileAccess.ModeFlags.Write);
        if (dosya == null) throw new InvalidOperationException("Test destesi yazılamadı.");
        dosya.StoreString(metin);
    }
    private async Task Tikla(OyunEkranlari ekran, string ad)
    {
        if (ad is "SolKarar" or "SagKarar")
        {
            var kart = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
            kart.Tamamla();
            float x = 200 + (ad == "SagKarar" ? 1 : -1) * kart.Esik * 1.5f;
            kart.SuruklemeyiBaslat(200);
            kart.SuruklemeyiGuncelle(x);
            kart.SuruklemeyiBitir(x);
        }
        else ekran.GetNode<Button>("%" + ad).EmitSignal(BaseButton.SignalName.Pressed);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    private async Task SahneleriDene()
    {
        var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
        foreach (var boyut in new[] { new Vector2I(1152, 648), new Vector2I(360, 640), new Vector2I(640, 360) })
        {
            GetWindow().Size = boyut;
            GetWindow().ContentScaleSize = boyut;
            GetWindow().ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
            GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Ignore;
            foreach (string gosterge in Gostergeler.Siralama)
            foreach (int sinir in new[] { 0, 100 })
            {
                Yaz(JsonSerializer.Serialize(new[] { SonKarti(gosterge, sinir) }));
                var ekran = sahne.Instantiate<OyunEkranlari>();
                ekran.Animasyonlar = false;
                ekran.DesteYolu = _geciciYol;
                AddChild(ekran);
                await Bekle();
                await Tikla(ekran, "Hatunum");
                string anahtar = $"{gosterge}_{sinir}";
                for (int i = 1; i <= 4; i++)
                {
                    await Tikla(ekran, "SagKarar");
                    Dogrula(ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu").TamMetin == "Hatunum, sağ kararın ardından çarşı açıldı.",
                        "Yalnız seçilen kararın sonucu ve doğru hitap görünmeli.");
                    var etkiler = ekran.GetNode<Label>("%EylemBilgisi").Text;
                    Dogrula(etkiler.Contains(Gostergeler.Ad(gosterge) + (sinir == 0 ? " −15" : " +15")), "İşaretli sayılar yanlış.");
                    Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Son kararda dahi önce sonuç görünmeli.");
                    await Tikla(ekran, "SolKarar");
                    Dogrula(ekran.Oyun.Durum.KararSayisi == i, "Sonuçta girdi sayacı artırmamalı.");
                    if (i == 4) await Goruntule(ekran, boyut, anahtar + "-Sonuc");
                    await Tikla(ekran, "Devam");
                }
                Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Son, "Son paneli açılmalı.");
                Dogrula(ekran.GetNode<Label>("%SonBaslik").Text == _tema.Sonlar[anahtar].Baslik &&
                    ekran.GetNode<Label>("%SonMetni").Text == _tema.Sonlar[anahtar].Metin, "Tema anlatımı birebir gösterilmeli.");
                Dogrula(ekran.GetNode<Label>("%SonOzet").Text == "0 yıl hüküm sürdün, 4 karar verdin.", "Son özeti yanlış.");
                string simge = ekran.GetNode<TextureRect>("%SonGorseli").Texture?.ResourcePath;
                Dogrula(simge == $"res://Assets/ui/icons/icon_0{2 + Gostergeler.Siralama.ToList().IndexOf(gosterge)}.png",
                    $"{anahtar}: sonda sebep olan göstergenin simgesi görünmeli.");
                Dogrula(ekran.GetNode<NinePatchRect>("Zemin").Texture?.ResourcePath == OyunEkranlari.SonZemini, "Son ekranı bordo zeminde olmalı.");
                await Goruntule(ekran, boyut, anahtar);
                await Tikla(ekran, "Yeniden");
                Dogrula(ekran.Oyun.Asama == Asama.Acilis && ekran.Oyun.Durum == null, "Yeniden Dene eski oyunu temizlemeli.");
                Dogrula(ekran.GetNode<NinePatchRect>("Zemin").Texture?.ResourcePath == OyunEkranlari.OyunZemini, "Yeni oyunda zemin laciverte dönmeli.");
                await Tikla(ekran, "Beyim");
                Dogrula(ekran.Oyun.Durum.KararSayisi == 0 && ekran.Oyun.Durum.Zaman.TamamlananYil == 0 &&
                    ekran.Oyun.Durum.Gostergeler.Degerler.Values.All(v => v == 50), "Arayüz yeni oyunu sıfırdan başlatmalı.");
                ekran.Free();
            }
            var dengeli = EtkiliKart(new() { ["para"] = -5, ["halk"] = 5 });
            dengeli = dengeli with { Sag = dengeli.Sag with { Etki = new() { ["para"] = 5, ["halk"] = -5 } } };
            Yaz(JsonSerializer.Serialize(new[] { dengeli }));
            var yilEkrani = sahne.Instantiate<OyunEkranlari>();
            yilEkrani.Animasyonlar = false;
            yilEkrani.DesteYolu = _geciciYol;
            AddChild(yilEkrani);
            await Bekle();
            await Tikla(yilEkrani, "Beyim");
            for (int i = 1; i <= 10; i++)
            {
                await Tikla(yilEkrani, i % 2 == 0 ? "SagKarar" : "SolKarar");
                Dogrula(yilEkrani.GetNode<Label>("%KararSayisi").Text == $"Tahtta geçen yıl: {i / 5} · Karar: {i}",
                    "Yıl etiketi her beş onaylı kararda ilerlemeli.");
                await Tikla(yilEkrani, "Devam");
            }
            yilEkrani.Free();

            Yaz(JsonSerializer.Serialize(new[] { EtkiliKart(new() { ["para"] = -15, ["halk"] = 15 }) }));
            var ciftEkran = sahne.Instantiate<OyunEkranlari>();
            ciftEkran.Animasyonlar = false;
            ciftEkran.DesteYolu = _geciciYol;
            AddChild(ciftEkran);
            await Bekle();
            await Tikla(ciftEkran, "Beyim");
            for (int i = 0; i < 4; i++) { await Tikla(ciftEkran, "SolKarar"); await Tikla(ciftEkran, "Devam"); }
            Dogrula(ciftEkran.GetNode<Label>("%SonMetni").Text.Contains(_tema.Sonlar["halk_100"].Metin) &&
                ciftEkran.GetNode<Label>("%SonMetni").Text.Contains(_tema.Sonlar["para_0"].Metin),
                "Eşzamanlı iki son da ekranda okunabilmeli.");
            Dogrula(ciftEkran.GetNode<TextureRect>("%SonGorseli").Texture?.ResourcePath == OyunEkranlari.GenelSonSimgesi,
                "Birden çok sebepte genel taç simgesi görünmeli.");
            await Goruntule(ciftEkran, boyut, "cift-son");
            ciftEkran.Free();

            // Bozuk destenin hata ekranından yeniden yüklemeyle toparlanması.
            Yaz("[]");
            var bozukEkran = sahne.Instantiate<OyunEkranlari>();
            bozukEkran.Animasyonlar = false;
            bozukEkran.DesteYolu = _geciciYol;
            AddChild(bozukEkran);
            await Bekle();
            Dogrula(bozukEkran.Oyun == null && bozukEkran.GetNode<Button>("%Beyim").Disabled &&
                bozukEkran.GetNode<Button>("%Hatunum").Disabled &&
                bozukEkran.GetNode<Control>("%HataAlani").Visible, "Bozuk deste oynanabilir görünmemeli.");
            Yaz(JsonSerializer.Serialize(new[] { _ornek }));
            await Tikla(bozukEkran, "TekrarYukle");
            Dogrula(bozukEkran.Oyun != null && !bozukEkran.GetNode<Control>("%HataAlani").Visible, "Düzeltilen deste tekrar yüklenebilmeli.");
            bozukEkran.Free();
        }
    }
    private async Task Goruntule(OyunEkranlari ekran, Vector2I boyut, string ad)
    {
        await Bekle();
        var scroll = ekran.GetNode<ScrollContainer>("%Icerik");
        Dogrula(scroll.GetHScrollBar().MaxValue <= scroll.Size.X + 1, "Sonuç/son ekranı yatay taşmamalı.");
        foreach (var e in Enum.GetValues<OyunEkranlari.Ekran>())
            Dogrula(ekran.GetNode<Control>("%" + OyunEkranlari.PanelAdi(e)).IsVisibleInTree() ==
                (OyunEkranlari.PanelAdi(e) == OyunEkranlari.PanelAdi(ekran.AktifEkran)), "Yalnız aktif panel görünmeli.");
        var dugme = ekran.GetNode<Button>(ekran.AktifEkran == OyunEkranlari.Ekran.Son ? "%Yeniden" : "%Devam");
        Dogrula(dugme.Size.Y >= 48 && dugme.HasFocus() && dugme.GlobalPosition.Y >= 0 &&
            dugme.GlobalPosition.Y + dugme.Size.Y <= boyut.Y,
            $"{boyut} {ad}: düğme görünür/odaklı olmalı (y={dugme.GlobalPosition.Y}, h={dugme.Size.Y}, odak={dugme.HasFocus()}).");
        if (DisplayServer.GetName() == "headless") return;
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute("res://.godot/testler/h2");
        var sonuc = GetViewport().GetTexture().GetImage().SavePng($"res://.godot/testler/h2/{boyut.X}x{boyut.Y}-{ad}.png");
        Dogrula(sonuc == Error.Ok, "H2 görüntüsü kaydedilemedi.");
    }
}
