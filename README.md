## Facebook -> Wordpress konverter

Freeblogos exportot konvertal Wordpesses xmlbe.

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

	Wp2Fb.exe <forras> <cel>

A `forras` azt a konyvtarat adja meg, ahova a freeblogos export ki van toomoritve. (Ebben a konyvtarban van a `comments.xml`, `categories.xml` es `entries.xml`.)

A `cel` mondja meg, hogy hova keruljon a wordpress fajl.

Pl.:

	fb2wp.exe "c:\Dokumentumok\export_20130319\" "c:\Dokumentumok\wp.xml"

