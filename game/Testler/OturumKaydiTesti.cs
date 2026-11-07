using Godot;
using Krallik;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class OturumKaydiTesti : Node
{
    private int _kontrol;

    public override async void _Ready()
    {
        string dizin = ProjectSettings.GlobalizePath("user://test-oturum-" + Guid.NewGuid().ToString("N"));
        try
        {
            await Dene(dizin);
            GD.Print($"OTURUM KAYDI TESTİ BAŞARILI: {_kontrol} kontrol.");
            GetTree().Quit();
        }
        catch (Exception hata)
        {
            GD.PushError(hata.ToString());
            GetTree().Quit(1);
        }
        finally
        {
            if (Directory.Exists(dizin))
            {
                foreach (string dosya in Directory.EnumerateFiles(dizin)) File.Delete(dosya);
                Directory.Delete(dizin);
            }
        }
    }

    private async Task Dene(string dizin)
    {
        var depo = new OyunKayitDeposu(dizin);
        var tema = Tema.Oku(Godot.FileAccess.GetFileAsString("res://Veri/tema.json"));
        var oyunSahnesi = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn");
        var ekran = oyunSahnesi.Instantiate<OyunEkranlari>();
        ekran.KayitDeposu = depo;
        AddChild(ekran);
        await Bekle();
        Bas(ekran, "%Hatunum");
        var ilk = depo.Listele(tema).Kayitlar.Single();
        Dogrula(ilk.Oyun.Asama == Asama.Kart && ilk.Oyun.KararSayisi == 0 && ilk.Oyun.Hukumdar == Hukumdar.Hatun,
            "İlk karar öncesi otomatik kayıt eksik.");
        string ilkBaslik = ekran.GetNode<Label>("%KartBasligi").Text;
        ekran.Free();

        ekran = oyunSahnesi.Instantiate<OyunEkranlari>();
        ekran.KayitDeposu = depo;
        ekran.YuklenecekKayit = depo.Yukle(ilk.Id, tema);
        AddChild(ekran);
        await Bekle();
        Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Oyun &&
            ekran.GetNode<Label>("%KartBasligi").Text == ilkBaslik &&
            ekran.Oyun.AktifKart.OlayKimligi == ilk.Oyun.AktifOlay.OlayKimligi, "Kart veya karakter adı geri gelmedi.");

        var kart = ekran.GetNode<KaydirmaKontrolu>("%KartAlani");
        kart.SuruklemeyiBaslat(200);
        kart.SuruklemeyiGuncelle(200 + kart.Esik * 1.5f);
        kart.SuruklemeyiBitir(200 + kart.Esik * 1.5f);
        await ToSignal(GetTree().CreateTimer(0.35), SceneTreeTimer.SignalName.Timeout);
        var sonuc = depo.Yukle(ilk.Id, tema);
        Dogrula(sonuc.Oyun.Asama == Asama.Sonuc && sonuc.Oyun.Kararlar.Single().Sag &&
            sonuc.Oyun.Kararlar[0].OlayId == ilk.Oyun.AktifOlay.OlayKimligi, "Karar geçmişi veya bekleyen sonuç kayboldu.");
        Dogrula(sonuc.Oyun.Gostergeler.Any(p => p.Value != 50), "Değişen göstergeler yazılmadı.");
        ekran.Free();

        Guid bozuk = Guid.NewGuid();
        File.WriteAllText(Path.Combine(dizin, bozuk.ToString("N") + ".json"), "{");
        depo.Kaydet(sonuc with
        {
            Id = Guid.NewGuid(),
            Oyun = sonuc.Oyun with { KararSayisi = sonuc.Oyun.KararSayisi + 1 }
        });
        Dogrula(depo.Listele(tema).Bozuk == 2 && depo.Listele(tema).Kayitlar.Count == 1,
            "Bozuk veya tutarsız kayıt diğer kayıtları engellememeli.");

        var menu = GD.Load<PackedScene>("res://Sahneler/KayitliOyunlar.tscn").Instantiate<KayitliOyunlarEkrani>();
        menu.KayitDeposu = depo;
        GetTree().Root.AddChild(menu);
        GetTree().CurrentScene = menu;
        await Bekle();
        var liste = menu.GetNode<VBoxContainer>("%KayitListesi");
        Dogrula(liste.GetChildCount() == 1 && menu.GetNode<Label>("%BosDurum").Text.Contains("2"),
            "Kayıt menüsü geçerli kaydı ve bozuk kayıt uyarısını göstermeli.");
        liste.GetChild<Button>(0).EmitSignal(BaseButton.SignalName.Pressed);
        await Bekle();
        ekran = GetTree().CurrentScene as OyunEkranlari;
        Dogrula(ekran?.AktifEkran == OyunEkranlari.Ekran.Sonuc && ekran.Oyun.Durum.KararSayisi == 1,
            "Kayıt menüsünden bekleyen sonuca dönülemedi.");
        Dogrula(ekran.Oyun.Durum.Gostergeler.Degerler.All(p => sonuc.Oyun.Gostergeler[p.Key] == p.Value),
            "Kayıtlı göstergeler geri yüklenmedi.");
        Bas(ekran, "%Devam");
        Dogrula(ekran.AktifEkran == OyunEkranlari.Ekran.Oyun && ekran.Oyun.Durum.KararSayisi == 1 &&
            ekran.Oyun.AktifKart.OlayKimligi != ilk.Oyun.AktifOlay.OlayKimligi, "Sonraki kart veya karar sayısı yanlış.");
        // Telefonun geri tuşu oyun ortasında "Kaydet ve Ana Menüye Dön" ile aynı işi yapar (G0 sonrası).
        Dogrula(ekran.GetNode<Button>("%KaydetVeDon").Visible, "Kayıt açıkken oyunda Kaydet ve Ana Menüye Dön görünmeli.");
        var gecisler = new List<string>();
        ekran.SahneGecisi = gecisler.Add;
        var onceki = depo.Yukle(ilk.Id, tema).KayitZamani;
        await ToSignal(GetTree().CreateTimer(0.02), SceneTreeTimer.SignalName.Timeout);
        ekran.PropagateNotification((int)Node.NotificationWMGoBackRequest);
        var geriKaydi = depo.Yukle(ilk.Id, tema);
        Dogrula(gecisler.SequenceEqual(new[] { OyunEkranlari.AnaMenuYolu }) && geriKaydi.KayitZamani > onceki &&
            geriKaydi.Oyun.Asama == Asama.Kart && geriKaydi.Oyun.KararSayisi == 1,
            "Oyun ortasında geri tuşu ilerlemeyi kaydedip ana menüye dönmeli.");
        ekran.SahneGecisi = null;

        var yeni = oyunSahnesi.Instantiate<OyunEkranlari>();
        yeni.KayitDeposu = depo;
        yeni.DesteYolu = "res://Testler/Veri/ornek_deste.json";
        AddChild(yeni);
        await Bekle();
        Bas(yeni, "%Beyim");
        Dogrula(depo.Listele(tema).Kayitlar.Count == 2 && depo.Yukle(ilk.Id, tema).Oyun.KararSayisi == 1,
            "Yeni oyun önceki hükümdarlığı korumalı.");
        Dogrula(!Directory.EnumerateFiles(dizin, "*.tmp").Any(), "Geçici kayıt dosyası kalmamalı.");
        for (int i = 0; i < 30 && yeni.AktifEkran != OyunEkranlari.Ekran.Son; i++)
        {
            kart = yeni.GetNode<KaydirmaKontrolu>("%KartAlani");
            kart.SuruklemeyiBaslat(200);
            kart.SuruklemeyiGuncelle(200 - kart.Esik * 1.5f);
            kart.SuruklemeyiBitir(200 - kart.Esik * 1.5f);
            await ToSignal(GetTree().CreateTimer(0.28), SceneTreeTimer.SignalName.Timeout);
            Bas(yeni, "%Devam");
        }
        Dogrula(yeni.AktifEkran == OyunEkranlari.Ekran.Son, "Tamamlanan hükümdarlık son ekranına gelmeli.");
        var sonRekor = yeni.GetNode<Label>("%SonRekor");
        Dogrula(sonRekor.Visible && sonRekor.Text.StartsWith("Yeni rekor"), "İlk tamamlanan hükümdarlık son ekranında yeni rekor olarak görünmeli.");
        var tekrarAcilanDepo = new OyunKayitDeposu(dizin);
        var gecmis = new HukumdarlikGecmisi(tekrarAcilanDepo.Listele(tema).Kayitlar);
        Dogrula(gecmis.Tamamlananlar.Count == 1 && gecmis.Rekor.Oyun.KararSayisi == yeni.Oyun.Durum.KararSayisi &&
            gecmis.Rekor.Oyun.Yil == yeni.Oyun.Durum.Zaman.TamamlananYil, "Rekor yeniden açılan depoda korunmalı.");
        menu = GD.Load<PackedScene>("res://Sahneler/KayitliOyunlar.tscn").Instantiate<KayitliOyunlarEkrani>();
        menu.KayitDeposu = tekrarAcilanDepo;
        AddChild(menu);
        await Bekle();
        Dogrula(menu.GetNode<Label>("%RekorMetni").Text.Contains($"{gecmis.Rekor.Oyun.KararSayisi} karar") &&
            menu.GetNode<VBoxContainer>("%GecmisListesi").GetChildCount() == 3 &&
            menu.GetNode<VBoxContainer>("%KayitListesi").GetChildCount() == 1,
            "Tamamlanan oyun rekor ve geçmişe girip devam listesinden çıkmalı.");
        yeni.Free();
        GetTree().CurrentScene = this;
        ekran.Free();
        await EkranGoruntusu(menu, "KayitliOyunlar");
        menu.Free();
        await RastgeleBaslangiciDene(depo, tema, oyunSahnesi);
    }

    /// <summary>Her hükümdarlık destenin başka bir olayından başlar; konum kayda yazılır.</summary>
    private async Task RastgeleBaslangiciDene(OyunKayitDeposu depo, Tema tema, PackedScene oyunSahnesi)
    {
        const string ornek = "res://Testler/Veri/ornek_deste.json";
        var roller = tema.Karakterler.Select(r => r.Anahtar).ToHashSet();
        var deste = new YedekDeste(Godot.FileAccess.GetFileAsString(ornek), roller).KartlariYukle();
        var cekirdek = new OyunYoneticisi(new YedekDeste(Godot.FileAccess.GetFileAsString(ornek), roller));
        for (int k = 0; k < deste.Count; k++)
        {
            cekirdek.Baslat(Hukumdar.Bey, k);
            Dogrula(cekirdek.AktifKart.OlayKimligi == deste[k].OlayKimligi, $"Başlangıç konumu {k} yanlış kartı açtı.");
        }
        int beklenen = new Random(5).Next(deste.Count);
        var ekran = oyunSahnesi.Instantiate<OyunEkranlari>();
        ekran.KayitDeposu = depo;
        ekran.DesteYolu = ornek;
        ekran.BaslangicRastgele = new Random(5);
        AddChild(ekran);
        await Bekle();
        Bas(ekran, "%Beyim");
        var kayit = depo.Listele(tema).Kayitlar[0];
        Dogrula(beklenen != 0 && ekran.Oyun.AktifKart.OlayKimligi == deste[beklenen].OlayKimligi &&
            kayit.Oyun.AktifOlay.OlayKimligi == deste[beklenen].OlayKimligi && kayit.Oyun.KaynakKonumu == beklenen + 1,
            "Yeni hükümdarlık rastgele seçilen olaydan başlamalı ve konum kayda yazılmalı.");
        ekran.Free();
    }

    /// <summary>Grafik çalıştırmada dolu kayıt ekranını dikey telefon boyutunda kaydeder.</summary>
    private async Task EkranGoruntusu(Control ekran, string ad)
    {
        if (DisplayServer.GetName() == "headless") return;
        var boyut = new Vector2I(360, 800);
        GetWindow().Size = boyut;
        GetWindow().ContentScaleSize = boyut;
        GetWindow().ContentScaleAspect = Window.ContentScaleAspectEnum.Ignore;
        ekran.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        await Bekle();
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        DirAccess.MakeDirRecursiveAbsolute("res://.godot/testler");
        GetViewport().GetTexture().GetImage().SavePng($"res://.godot/testler/{boyut.X}x{boyut.Y}-{ad}.png");
    }

    private static void Bas(Node ekran, string dugme) =>
        ekran.GetNode<Button>(dugme).EmitSignal(BaseButton.SignalName.Pressed);

    private async Task Bekle()
    {
        for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private void Dogrula(bool kosul, string mesaj)
    {
        if (!kosul) throw new InvalidOperationException(mesaj);
        _kontrol++;
    }
}
