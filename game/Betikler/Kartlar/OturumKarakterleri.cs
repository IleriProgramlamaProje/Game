using System;
using System.Collections.Generic;

namespace Krallik;

public sealed class OturumKarakterleri
{
    private readonly Dictionary<string, KarakterKimligi> _kimlikler = new();
    private static readonly string[] ErkekAdlari = ["Yakup", "Osman", "Turgut", "Ali", "Yunus", "Orhan", "Selim", "Murat", "Sinan", "Kemal", "İlyas", "Ömer"];
    private static readonly string[] KadinAdlari = ["Ayşe", "Elif", "Zeynep", "Selma", "Ayla", "İpek", "Suna", "Aslı", "Gül", "Dilek", "Esma", "Bahar"];
    public OturumKarakterleri(IEnumerable<Rol> roller, Random rastgele)
    {
        foreach (var rol in roller)
        {
            string cinsiyet = rol.Cinsiyet == "Çift" ? (rastgele.Next(2) == 0 ? "Erkek" : "Kadın") : rol.Cinsiyet;
            var adlar = cinsiyet == "Erkek" ? ErkekAdlari : KadinAdlari;
            _kimlikler.Add(rol.Anahtar, new(rol.Unvan, adlar[rastgele.Next(adlar.Length)], cinsiyet,
                rol.Anahtar + (rol.Cinsiyet == "Çift" ? (cinsiyet == "Erkek" ? "_erkek" : "_kadin") : "")));
        }
    }
    public KarakterKimligi Kimlik(string rol) => _kimlikler[rol];
    public string Baslik(string rol) => $"{Kimlik(rol).Unvan} {Kimlik(rol).Isim}";
}
