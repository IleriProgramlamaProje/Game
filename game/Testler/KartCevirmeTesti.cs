using Godot;
using System;
using System.Threading.Tasks;

public partial class KartCevirmeTesti : Node
{
    private void Dogrula(bool kosul, string mesaj)
    {
        if (!kosul) throw new Exception(mesaj);
    }
    private async Task Bekle() => await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);

    public override async void _Ready()
    {
        try
        {
            foreach (var boyut in new[] { new Vector2I(1152, 648), new Vector2I(360, 640), new Vector2I(640, 360) })
            foreach (int yon in new[] { -1, 1 })
            {
                GetWindow().Size = boyut;
                var ekran = GD.Load<PackedScene>("res://Sahneler/Oyun.tscn").Instantiate<OyunEkranlari>();
                ekran.DesteYolu = "res://Testler/Veri/ornek_deste.json";
                ekran.Animasyonlar = false;
                AddChild(ekran);
                ekran.GetNode<Button>("%Beyim").EmitSignal(BaseButton.SignalName.Pressed);
                await Bekle();
                var kart = ekran.GetNode<DiyalogKutusu>("%DiyalogKutusu");
                var secenek = yon > 0 ? ekran.Oyun.AktifKart.Sag : ekran.Oyun.AktifKart.Sol;
                var once = new System.Collections.Generic.Dictionary<string, int>(ekran.Oyun.Durum.Gostergeler.Degerler);
                string metin = ekran.GetNode<Label>("%DiyalogMetni").Text;
                Vector2 merkez = kart.Position;
                kart.SuruklemeyiBaslat(200);
                kart.SuruklemeyiGuncelle(200 + yon * kart.Esik / 2);
                Dogrula(ekran.Oyun.Durum.KararSayisi == 0, "Önizleme karar uyguladı.");
                kart.SuruklemeyiBitir(200 + yon * kart.Esik / 2);
                await Bekle();
                Dogrula(kart.Position.IsEqualApprox(merkez), "İptal kartı merkeze döndürmedi.");
                kart.SuruklemeyiBaslat(200);
                kart.SuruklemeyiGuncelle(200 + yon * kart.Esik * 2);
                Dogrula(Mathf.IsEqualApprox(kart.RotationDegrees, yon * KaydirmaKontrolu.AzamiEgim), "Kartın hafif eğimi yanlış.");
                Dogrula(Mathf.IsEqualApprox(Mathf.Abs(kart.Position.X - merkez.X), kart.Esik * 1.5f), "Azami kaydırma mesafesi yanlış.");
                Dogrula(Mathf.Sign(kart.Position.X - merkez.X) == yon && Mathf.IsEqualApprox(kart.Position.Y, merkez.Y), "Kart doğru yöne yatay hareket etmedi.");
                Dogrula(ekran.GetNode<Label>("%DiyalogMetni").Text == metin, "Ana metin önizlemede değişti.");
                Dogrula(ekran.GetNode<Label>("%KararOnizleme").Text.Contains("Seçmek için bırak"), "Onay eşiği görünmüyor.");
                kart.SuruklemeyiBitir(200 + yon * kart.Esik * 2);
                kart.SuruklemeyiBaslat(200);
                kart.SuruklemeyiBitir(0);
                await Bekle();
                Dogrula(ekran.Oyun.Durum.KararSayisi == 1 && ekran.AktifEkran == OyunEkranlari.Ekran.Sonuc, "Bırakma tek karar üretmedi.");
                foreach (var etki in secenek.Etki)
                    Dogrula(ekran.Oyun.Durum.Gostergeler.Degerler[etki.Key] == Math.Clamp(once[etki.Key] + etki.Value, 0, 100), "Yanlış tarafın etkisi uygulandı.");
                ekran.GetNode<Button>("%Devam").EmitSignal(BaseButton.SignalName.Pressed);
                await Bekle();
                Dogrula(Mathf.IsZeroApprox(kart.RotationDegrees) && kart.Position.X >= 0, "Yeni kart merkezde değil.");
                ekran.Free();
            }
            GD.Print("KART ÇEVİRME TESTLERİ BAŞARILI");
            GetTree().Quit();
        }
        catch (Exception hata) { GD.PushError(hata.ToString()); GetTree().Quit(1); }
    }
}
