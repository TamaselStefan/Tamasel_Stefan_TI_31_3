// 2. Normalizarea vectorilor: pondere(d, a) = TF(d, a) * IDF(a).
// n(d, a) = de cate ori apare cuvantul "a" in documentul "d".
static class Normalizare
{
    public static readonly string[] Metode = { "Binar", "Logaritmic", "Nominal", "Suma 1" };

    // IDF(a) = log(|D| / |D_a|)
    // |D| = numarul de documente, |D_a| = numarul de documente care contin cuvantul "a".
    public static Dictionary<string, double> CalculeazaIdf(List<Articol> articole)
    {
        var documenteCuCuvant = new Dictionary<string, int>();
        foreach (Articol articol in articole)
        {
            foreach (string cuvant in articol.Frecvente.Keys)
            {
                if (documenteCuCuvant.ContainsKey(cuvant))
                    documenteCuCuvant[cuvant]++;
                else
                    documenteCuCuvant[cuvant] = 1;
            }
        }

        var idf = new Dictionary<string, double>();
        foreach (var (cuvant, numar) in documenteCuCuvant)
            idf[cuvant] = Math.Log((double)articole.Count / numar);
        return idf;
    }

    // Transforma frecventele unui document (sau ale interogarii) in ponderi TF * IDF.
    // Cuvintele care nu sunt in dictionarul global (nu au IDF) sunt ignorate.
    // Cuvintele care lipsesc din rezultat au valoarea 0.
    public static Dictionary<string, double> Normalizeaza(Dictionary<string, int> frecvente,
                                                          string metoda, Dictionary<string, double> idf)
    {
        var ponderi = new Dictionary<string, double>();
        if (frecvente.Count == 0)
            return ponderi;

        int maxim = frecvente.Values.Max();
        int suma = frecvente.Values.Sum();

        foreach (var (cuvant, n) in frecvente)
        {
            if (!idf.ContainsKey(cuvant))
                continue;

            double tf = metoda switch
            {
                "Binar" => 1,                                    // 1 daca apare, indiferent de cate ori
                "Logaritmic" => 1 + Math.Log(1 + Math.Log(n)),   // 1 + log(1 + log(n))
                "Nominal" => (double)n / maxim,                  // n / cel mai mare n din document
                "Suma 1" => (double)n / suma,                    // n / suma tuturor n din document
                _ => throw new ArgumentException("Metoda necunoscuta: " + metoda)
            };

            ponderi[cuvant] = tf * idf[cuvant];
        }

        return ponderi;
    }
}
