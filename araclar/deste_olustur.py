#!/usr/bin/env python3
"""Altın setten oyunun yedek destesini (game/Veri/yedek_deste.json) üretir.

Game repo kökünden:

    python3 araclar/deste_olustur.py            # varsayılan: 600 kart
    python3 araclar/deste_olustur.py --adet 300

Kaynak, kardeş AI deposundaki altın settir (``../AI/data/altin_set/ham/``).
Yalnız standart kütüphane kullanılır.

Seçim kuralları (Docs/yol_haritasi.md 7.1: yedek deste = "en iyi 600"):

1. Kart, oyunun C# doğrulayıcısının (Kart.Dogrula, Gostergeler.EtkiyiDogrula)
   kurallarından birebir geçmelidir; geçemeyen kart desteye hiç girmez.
   Oyun tek bir geçersiz kartta bütün desteyi reddettiği için bu zorunludur.
2. 2 Kasım kararıyla altın set elle okunmadığından "en iyi" kuralla seçilir:
   önce ürün kuralındaki hedef uzunlukta (20–35 kelime) metinler tercih edilir.
3. Roller dengelenir: kartlar rollerden sırayla birer birer alınır.
4. Aynı konuşmacı art arda gelmez; sıra sabit tohumla karışıktır.

Kart alanları ve kaynak/lisans etiketleri değiştirilmez; altın setteki kimlik
(``altin-0000001``) ``id`` alanına yazılır.
"""
from __future__ import annotations

import argparse
import json
import random
import sys
from collections import defaultdict
from pathlib import Path

GOSTERGELER = {"yonetim", "halk", "para", "ordu"}
KADEMELER = {-15, -10, -5, 5, 10, 15}
KAYNAKLAR = {"model", "altin_set", "ogretmen"}
ALANLAR = ("konusan", "konu", "metin", "sol", "sag", "kaynak", "lisans")
SECENEK_ALANLARI = {"etiket", "etki", "sonuc"}
HEDEF_UZUNLUK = range(20, 36)


def kelime(metin: str) -> int:
    # C# string.Split(null, RemoveEmptyEntries) ile aynı: tüm boşluk karakterleri ayırır.
    return len(metin.split())


def dolu(deger) -> bool:
    return isinstance(deger, str) and deger.strip() != ""


def gecersizlik(kart: dict, roller: set[str]) -> str | None:
    """Kart oyuna giremiyorsa nedenini, girebiliyorsa None döndürür."""
    if set(kart) - set(ALANLAR):
        return "tanınmayan alan"
    if kart.get("konusan") not in roller:
        return "rol tema listesinde yok"
    if not all(dolu(kart.get(a)) for a in ("konu", "metin", "lisans")) or kart.get("kaynak") not in KAYNAKLAR:
        return "konu/metin/kaynak/lisans geçersiz"
    if not 20 <= kelime(kart["metin"]) <= 50:
        return "metin 20–50 kelime değil"
    for taraf in ("sol", "sag"):
        secenek = kart.get(taraf)
        if not isinstance(secenek, dict) or set(secenek) - SECENEK_ALANLARI:
            return "seçenek biçimi geçersiz"
        if not dolu(secenek.get("etiket")) or not dolu(secenek.get("sonuc")):
            return "karar veya sonuç boş"
        if not 2 <= kelime(secenek["etiket"]) <= 5:
            return "karar 2–5 kelime değil"
        etki = secenek.get("etki")
        if (not isinstance(etki, dict) or not 1 <= len(etki) <= 3 or not any(v < 0 for v in etki.values())
                or any(k not in GOSTERGELER or v not in KADEMELER for k, v in etki.items())):
            return "etki kuralı"
    return None


def kimlikleri_oku(ham_yolu: Path, kartlar: list[dict]) -> list[str]:
    """Ham JSONL'deki kimlikleri aynı sıradaki kartlarla eşleştirir; sıra bozuksa durur."""
    ham = [json.loads(satir) for satir in ham_yolu.read_text(encoding="utf-8").splitlines() if satir.strip()]
    if len(ham) != len(kartlar):
        sys.exit(f"Ham kayıt ({len(ham)}) ile kart ({len(kartlar)}) sayısı farklı; kimlikler eşleştirilemez.")
    kimlikler = []
    for sira, (kayit, kart) in enumerate(zip(ham, kartlar)):
        ilk_satir = kayit.get("cikti", "").split("\n", 1)[0].removeprefix("Metin:").strip()
        if ilk_satir.split()[:6] != kart["metin"].split()[:6]:
            sys.exit(f"{sira}. kayıt ile kart metni uyuşmuyor; ham ve JSON dosyaları aynı indirmeden olmalı.")
        kimlikler.append(kayit["id"])
    return kimlikler


def sec(adaylar: dict[str, list[dict]], adet: int, rastgele: random.Random) -> list[dict]:
    """Rollerden sırayla birer kart alır; ardışık iki kartın konuşmacısı farklıdır."""
    for liste in adaylar.values():
        rastgele.shuffle(liste)
        # Hedef uzunluktakiler öne; kendi içlerinde tohumlu sıra korunur.
        liste.sort(key=lambda k: kelime(k["metin"]) not in HEDEF_UZUNLUK)
    secilen: list[dict] = []
    while len(secilen) < adet and any(adaylar.values()):
        tur = [rol for rol, liste in adaylar.items() if liste]
        rastgele.shuffle(tur)
        if secilen and len(tur) > 1 and tur[0] == secilen[-1]["konusan"]:
            tur[0], tur[1] = tur[1], tur[0]
        for rol in tur:
            if len(secilen) == adet:
                break
            secilen.append(adaylar[rol].pop(0))
    return secilen


def main() -> None:
    kok = Path(__file__).resolve().parent.parent
    ap = argparse.ArgumentParser(description=__doc__.split("\n")[0])
    ap.add_argument("--altin-set", type=Path, default=kok.parent / "AI/data/altin_set/ham/altin_set.json")
    ap.add_argument("--ham", type=Path, default=kok.parent / "AI/data/altin_set/ham/altin_set_ham.jsonl")
    ap.add_argument("--tema", type=Path, default=kok / "game/Veri/tema.json")
    ap.add_argument("--cikti", type=Path, default=kok / "game/Veri/yedek_deste.json")
    ap.add_argument("--adet", type=int, default=600)
    ap.add_argument("--tohum", type=int, default=2026)
    arg = ap.parse_args()

    kartlar = json.loads(arg.altin_set.read_text(encoding="utf-8"))
    roller = {r["anahtar"] for r in json.loads(arg.tema.read_text(encoding="utf-8"))["karakterler"]}
    kimlikler = kimlikleri_oku(arg.ham, kartlar)

    nedenler: dict[str, int] = defaultdict(int)
    adaylar: dict[str, list[dict]] = defaultdict(list)
    gorulen_metin: set[str] = set()
    for kimlik, kart in zip(kimlikler, kartlar):
        neden = gecersizlik(kart, roller)
        if neden is None and kart["metin"] in gorulen_metin:
            neden = "aynı metin tekrarı"
        if neden:
            nedenler[neden] += 1
            continue
        gorulen_metin.add(kart["metin"])
        adaylar[kart["konusan"]].append({"id": kimlik, **{a: kart[a] for a in ALANLAR}})

    uygun = sum(len(v) for v in adaylar.values())
    eksik_rol = sorted(roller - set(adaylar))
    secilen = sec(adaylar, arg.adet, random.Random(arg.tohum))
    if not secilen:
        sys.exit("Desteye girebilecek kart yok.")
    arg.cikti.write_text(json.dumps(secilen, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    rol_sayisi = defaultdict(int)
    for kart in secilen:
        rol_sayisi[kart["konusan"]] += 1
    hedefte = sum(kelime(k["metin"]) in HEDEF_UZUNLUK for k in secilen)
    print(f"Altın set: {len(kartlar)} kart · oyuna uygun: {uygun} · elenen: {dict(nedenler)}")
    print(f"Deste: {len(secilen)} kart → {arg.cikti}")
    print(f"Rol başına: {min(rol_sayisi.values())}–{max(rol_sayisi.values())} · hedef uzunlukta (20–35): {hedefte}")
    if eksik_rol:
        print(f"UYARI: altın sette kartı olmayan roller: {', '.join(eksik_rol)}")


if __name__ == "__main__":
    main()
