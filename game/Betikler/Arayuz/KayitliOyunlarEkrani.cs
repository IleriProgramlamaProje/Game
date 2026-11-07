using Godot;
using Krallik;
using System;
using System.IO;
using System.Linq;

public partial class KayitliOyunlarEkrani : AltEkran
{
    public OyunKayitDeposu KayitDeposu { get; set; }
    private Tema _tema;
    // Kart önizlemesindeki bordo vurguyla aynı renk; rekor parşömen üzerinde öne çıkar.
    private static readonly Color Vurgu = new(0.5f, 0.11f, 0.06f);

    protected override void VarsayilanOdagaGec()
    {
        var liste = GetNode<VBoxContainer>("%KayitListesi");
        var odak = liste.GetChildCount() > 0 ? liste.GetChild<Button>(0) : GetNode<Control>("%KayitBasligi");
        odak.GrabFocus(true);
    }

    protected override void BaglantilariKur()
    {
        base.BaglantilariKur();
        if (GetParent() == GetTree().Root)
            KayitDeposu ??= new OyunKayitDeposu(ProjectSettings.GlobalizePath(OyunEkranlari.KayitDizini));
        if (KayitDeposu == null) return;

        var durum = GetNode<Label>("%BosDurum");
        try
        {
            _tema = Tema.Oku(Godot.FileAccess.GetFileAsString("res://Veri/tema.json"));
            var (kayitlar, bozuk) = KayitDeposu.Listele(_tema);
            var devamEden = kayitlar.Where(k => k.Oyun.Asama != Asama.Son).ToArray();
            durum.Text = devamEden.Length == 0
                ? "Henüz devam eden bir hükümdarlık yok."
                : "Devam etmek istediğin hükümdarlığı seç. Yeni oyun eski kayıtları korur.";
            if (bozuk > 0) durum.Text += $"\nOkunamayan kayıt: {bozuk}.";
            var liste = GetNode<VBoxContainer>("%KayitListesi");
            var devamIkonu = GD.Load<Texture2D>("res://Assets/ui/icons/icon_13.png");
            foreach (var kayit in devamEden)
            {
                // En yeni kayıt üstte; ikon "devam et" anlamındaki Yeni Oyun ikonudur.
                var dugme = new Button
                {
                    Text = $"{(kayit.Oyun.Hukumdar == Hukumdar.Bey ? "Beyim" : "Hatunum")} · {kayit.Oyun.Yil} yıl · {kayit.Oyun.KararSayisi} karar\n{kayit.KayitZamani.ToLocalTime():dd.MM.yyyy · HH:mm}",
                    Icon = devamIkonu,
                    CustomMinimumSize = new Vector2(0, 56),
                    AutowrapMode = TextServer.AutowrapMode.WordSmart
                };
                dugme.Pressed += () => KaydiAc(kayit.Id);
                liste.AddChild(dugme);
            }
            GecmisiGoster(new HukumdarlikGecmisi(kayitlar));
        }
        catch (Exception hata) when (hata is FormatException or IOException or UnauthorizedAccessException)
        {
            GD.PushWarning(hata.Message);
            durum.Text = "Kayıtlar okunamadı.";
        }
    }

    private void GecmisiGoster(HukumdarlikGecmisi gecmis)
    {
        var panel = GetNode<Control>("%GecmisPanosu");
        panel.Show();
        var rekor = GetNode<Label>("%RekorMetni");
        // Dar telefonda kötü bölünmesin diye rekor iki satırdır.
        rekor.Text = gecmis.Rekor is { } enIyi
            ? $"En iyi hükümdarlık\n{enIyi.Oyun.Yil} yıl · {enIyi.Oyun.KararSayisi} karar"
            : "Henüz tamamlanmış hükümdarlık yok.";
        if (gecmis.Rekor != null) rekor.AddThemeColorOverride("font_color", Vurgu);
        var liste = GetNode<VBoxContainer>("%GecmisListesi");
        foreach (var kayit in gecmis.Tamamlananlar)
        {
            string sonlar = string.Join(" / ", SonKontrolu.Bul(kayit.Oyun.Gostergeler).Select(k => _tema.Sonlar[k].Baslik));
            liste.AddChild(new Label
            {
                Text = $"{sonlar}\n{kayit.KayitZamani.ToLocalTime():dd.MM.yyyy} · {kayit.Oyun.Yil} yıl · {kayit.Oyun.KararSayisi} karar",
                ThemeTypeVariation = "ParchmentLabel",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });
        }
    }

    private void KaydiAc(Guid id)
    {
        try
        {
            var kayit = KayitDeposu.Yukle(id, _tema);
            var sahne = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
            sahne.KayitDeposu = KayitDeposu;
            sahne.YuklenecekKayit = kayit;
            if (GetTree().ChangeSceneToNode(sahne) != Error.Ok)
            {
                sahne.Free();
                GetNode<Label>("%BosDurum").Text = "Oyun ekranı açılamadı.";
            }
        }
        catch (Exception hata) when (hata is FormatException or IOException or UnauthorizedAccessException)
        {
            GD.PushWarning(hata.Message);
            GetNode<Label>("%BosDurum").Text = "Bu kayıt artık okunamıyor. Başka bir kayıt seçebilirsin.";
        }
    }
}
