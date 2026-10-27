# WordFrequency: extragere și vectori locali de frecvență (Reuters)

Aplicație console C# (.NET 10) pentru laboratorul de Sisteme de Recunoaștere a Formelor.
Citește articolele Reuters din XML, le salvează ca TXT, preprocesează textul și calculează
frecvența cuvintelor pentru fiecare articol.

## Pornire

Din directorul `WordFrequency`:

```
dotnet run
```

Sau din rădăcina proiectului: `dotnet run --project WordFrequency`. Merge și din Visual Studio (F5):
căile sunt relative la directorul `WordFrequency`, indiferent de unde pornește programul.

Date necesare (în rădăcina proiectului, lângă `WordFrequency/`):
`Reuters/Reuters_34/Training`, `Reuters/Reuters_34/Testing`, `Reuters/Reuters_7083`, `stopwords.txt`, `PorterStemmer.cs`.

## Ce face, în ordine

1. **Extragere** (`Extraction.cs`): pentru fiecare XML scrie un TXT cu același nume.
   Linia 1 = titlul + tot textul din `<text>`, pe o singură linie, cu spațiile normalizate.
   Liniile următoare = metadatele din `metadata/codes`: valoarea `class`, apoi fiecare `code`, câte unul pe linie, în ordinea din XML.
2. **Preprocesare** (`TextEdit.cs`): doar linia 1 se transformă: litere mici, orice nu e a-z devine spațiu,
   tokenizare, eliminare stop words, stemming Porter, tokenuri unite prin câte un spațiu. Metadatele rămân neschimbate.
3. **Vectori locali** (`FeatureExtraction.cs`): frecvențe brute ale cuvintelor din linia 1, câte un rând per articol.

TXT-urile se regenerează din XML la fiecare rulare, deci stemmingul nu se aplică niciodată de două ori.
Fișierele XML sursă nu sunt modificate.

## Ieșiri

```
WordFrequency/output/
  Reuters_34/Training/*.txt
  Reuters_34/Testing/*.txt
  Reuters_7083/*.txt
  vectors/Reuters_34_Training.txt
  vectors/Reuters_34_Testing.txt
  vectors/Reuters_7083.txt
```

## Format TXT (exemplu 2504NEWS.txt)

```
usa novel unveil intranet product novel tuesdai unveil intranetwar softwar ...
bip:countries:1.0
USA
bip:industries:1.0
I33020
...
bip:topics:1.0
C22
CCAT
```

## Format vectori

Un rând per articol, cuvintele sortate ordinal:

```
nume_articol # cuvant:frecventa cuvant:frecventa ...
2504NEWS # access:2 advantag:1 afternoon:1 base:1 ...
```

Numele este calea relativă la folderul procesat, fără extensie.

## Ce nu face încă

`CreateGlobalVector` (vocabularul global, sortat ordinal) există, dar nu e apelată în program.
Nu există încă indici globali, export `@attribute / @data`, TF-IDF sau expandarea contracțiilor
(aceasta din urmă ar trebui făcută înainte de tokenizare).
