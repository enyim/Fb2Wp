## Facebook -> Wordpress konverter

Freeblogos exportot konvertal Wordpesses xmlbe.

Letoltes: [Fb2Wp.zip](https://www.dropbox.com/s/k0rkahszgy6zl6g/Fb2Wp.zip)

Notesz:

- ha nem mukodik, az nem az en hibam
- ha valami maskepp kene, h mukodjon: javitsd ki
- ha nem boldogulsz, kerdezz meg valakit, aki nem en vagyok

## Hasznalat

Telepitsd fel a 4.5-os .NET Frameworkot. Enelkul nem fog menni.

	http://www.microsoft.com/hu-hu/download/details.aspx?id=30653

Ha megvagy, akkor indits egy parancssort:

- `Start > Futtatas`, vagy `Zaszlo + R` a billentyuzeten
- ird be h `cmd` majd Enter

A feljovo ablakban navigalj el oda, ahova letoltotted a Fb2Wp.exe fajlt

	cd "konyvtar"

Ha ez megvan, akkor:

	fb2wp.exe <forras> <cel>

A `forras` azt a konyvtarat adja meg, ahova a freeblogos export ki van toomoritve. (Ebben a konyvtarban van a `comments.xml`, `categories.xml` es `entries.xml`.)

A `cel` mondja meg, hogy hova keruljon a wordpress fajl.

Pl.:

	fb2wp.exe "c:\Dokumentumok\export_20130319\" "c:\Dokumentumok\wp.xml"

## Tippek

Ha kesz a konvertalas akkor valamilyen szovegszerkesztoben (pl [Notepad++](http://notepad-plus-plus.org/)) javitsd ki a kep es egyeb fajl hivatkozasokat.

Keress ra arra, hogy "http://blogod.freeblog.hu/files/" es javitsd at az uj wordpresses urlekre.