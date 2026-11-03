using System.Text.RegularExpressions;
using WordFrequency;

// Transforma textul unui articol in cuvinte numarate.
static class PrelucrareText
{
    static HashSet<string> stopwords;
    static PorterStemmer stemmer = new PorterStemmer();

    // 3.2 Cuvinte de legatura: cate un cuvant pe linie in stopwords.txt.
    public static void CitesteStopwords(string fisier)
    {
        stopwords = new HashSet<string>();
        foreach (string linie in File.ReadAllLines(fisier))
        {
            if (linie.Trim() != "")
                stopwords.Add(linie.Trim().ToLowerInvariant());
        }
    }

    // 3. Extragerea cuvintelor, exact in aceasta ordine:
    // litere mici -> doar a-z -> impartire in cuvinte -> fara stopwords (3.2)
    // -> radacina cuvantului (3.3).
    // Aceeasi functie se foloseste si pentru interogarea utilizatorului.
    public static List<string> ExtrageCuvinte(string text)
    {
        // Orice nu e litera sau spatiu (punctuatie, cifre, linii noi) devine spatiu,
        // ca "inc.tuesday" sa devina "inc tuesday", nu "inctuesday".
        string doarLitere = Regex.Replace(text.ToLowerInvariant(), "[^a-z ]", " ");

        var rezultat = new List<string>();
        foreach (string cuvant in doarLitere.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (stopwords.Contains(cuvant))
                continue;

            rezultat.Add(stemmer.StemWord(cuvant));
        }

        return rezultat;
    }

    // 3.1 Vectorul local al documentului: cuvant -> numar de aparitii.
    public static Dictionary<string, int> NumaraCuvinte(List<string> cuvinte)
    {
        var frecvente = new Dictionary<string, int>();
        foreach (string cuvant in cuvinte)
        {
            if (frecvente.ContainsKey(cuvant))
                frecvente[cuvant]++;
            else
                frecvente[cuvant] = 1;
        }
        return frecvente;
    }
}
