// Etapa 2: interogarea setului de date (pasii din sectiunea 4 a indrumarului).
static class Interogare
{
    const int NumarRezultate = 10;

    // Metodele folosite. Se pot schimba aici:
    // normalizare: "Binar", "Logaritmic", "Nominal", "Suma 1"
    // similaritate: "Euclid", "Manhattan", "Cosinus"
    const string normalizare = "Nominal";
    const string similaritate = "Cosinus";

    public static void Porneste(List<Articol> articole)
    {
        // Pasul 2: normalizam fiecare vector de document separat.
        Dictionary<string, double> idf = Normalizare.CalculeazaIdf(articole);
        var vectori = new List<Dictionary<string, double>>();
        foreach (Articol articol in articole)
            vectori.Add(Normalizare.Normalizeaza(articol.Frecvente, normalizare, idf));

        while (true)
        {
            // Pasul 3: citim interogarea de la tastatura.
            Console.Write("\nInterogare (Enter fara text pentru iesire): ");
            string interogare = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(interogare))
                break;

            // Pasii 4-5: aceeasi prelucrare ca la articole, apoi vector in dictionarul global.
            var frecvente = PrelucrareText.NumaraCuvinte(PrelucrareText.ExtrageCuvinte(interogare));
            var vectorInterogare = Normalizare.Normalizeaza(frecvente, normalizare, idf);

            var ignorate = frecvente.Keys.Where(c => !idf.ContainsKey(c));
            Console.WriteLine("Cuvinte folosite: " + string.Join(" ", vectorInterogare.Keys));
            if (ignorate.Any())
                Console.WriteLine("Cuvinte care nu sunt in dictionar (ignorate): " + string.Join(" ", ignorate));
            if (vectorInterogare.Count == 0)
            {
                Console.WriteLine("Niciun cuvant din interogare nu este in dictionar.");
                continue;
            }

            // Pasul 6: similaritatea dintre interogare si fiecare document.
            var rezultate = new List<(Articol Articol, double Valoare)>();
            for (int i = 0; i < articole.Count; i++)
                rezultate.Add((articole[i], Similaritate.Calculeaza(similaritate, vectorInterogare, vectori[i])));

            // Pasul 7: cele mai asemanatoare documente primele.
            var ordonate = Similaritate.MaiMicEsteMaiBine(similaritate)
                ? rezultate.OrderBy(r => r.Valoare)
                : rezultate.OrderByDescending(r => r.Valoare);

            // Pasul 8: afisam numele documentelor si valoarea similaritatii.
            Console.WriteLine($"Cele mai asemanatoare {NumarRezultate} documente ({normalizare}, {similaritate}):");
            int loc = 1;
            foreach (var (articol, valoare) in ordonate.Take(NumarRezultate))
            {
                Console.WriteLine($"{loc,3}. {articol.Nume,-10} {valoare,10:F4}   {articol.Eticheta}  {string.Join(" ", articol.Topicuri)}");
                loc++;
            }
        }
    }
}
