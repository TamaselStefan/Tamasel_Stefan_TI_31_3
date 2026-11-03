// Salveaza rezultatele in folderul output/.
static class FisiereRezultat
{
    // Scrie dictionarul (@attribute) si fiecare articol ca vector rar (@data).
    // Linie: nume # pozitie:frecventa ... # topicuri # Training/Testing/All
    public static void ScrieVectori(string fisier, SortedSet<string> vectorGlobal,
                                    Dictionary<string, int> pozitie, List<Articol> articole)
    {
        using var scriitor = new StreamWriter(fisier);

        foreach (string cuvant in vectorGlobal)
            scriitor.WriteLine("@attribute " + cuvant);

        scriitor.WriteLine("@data");

        foreach (Articol articol in articole)
        {
            // Vectorul rar: doar valorile diferite de 0, in ordinea pozitiilor.
            var perechi = new List<string>();
            foreach (var cuvant in articol.Frecvente.Keys.OrderBy(c => pozitie[c]))
                perechi.Add(pozitie[cuvant] + ":" + articol.Frecvente[cuvant]);

            scriitor.WriteLine(articol.Nume + " # " + string.Join(" ", perechi) +
                               " # " + string.Join(" ", articol.Topicuri) +
                               " # " + articol.Eticheta);
        }
    }

    // Vectorii locali, cu cuvintele scrise direct: nume # cuvant:frecventa ...
    public static void ScrieVectoriLocali(string fisier, List<Articol> articole)
    {
        using var scriitor = new StreamWriter(fisier);

        foreach (Articol articol in articole)
        {
            var perechi = new List<string>();
            foreach (var cuvant in articol.Frecvente.Keys.OrderBy(c => c, StringComparer.Ordinal))
                perechi.Add(cuvant + ":" + articol.Frecvente[cuvant]);

            scriitor.WriteLine(articol.Nume + " # " + string.Join(" ", perechi));
        }
    }

    // Fisierul .txt al unui articol: pe prima linie cuvintele dupa prelucrare,
    // apoi metadatele (clasa grupului, apoi codurile), cate una pe linie.
    // Programul doar scrie aceste fisiere, nu le citeste niciodata inapoi.
    public static void ScrieText(string folder, Articol articol)
    {
        var linii = new List<string> { string.Join(" ", articol.Cuvinte) };
        linii.AddRange(articol.Metadate);
        File.WriteAllLines(Path.Combine(folder, articol.Nume + ".txt"), linii);
    }
}
