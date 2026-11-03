// Etapa 1: extragerea trasaturilor din articolele Reuters.
// Etapa 2: interogarea setului de date.
// Rulare: dotnet run            -> Reuters_34 (Training + Testing)
//         dotnet run -- 7083    -> Reuters_7083 (eticheta All)
class Program
{
    static void Main(string[] args)
    {
        // Lucram din folderul proiectului, unde sunt stopwords.txt si Reuters/.
        Directory.SetCurrentDirectory(GasesteFolderulProiectului());

        // Ce foldere citim si cu ce eticheta.
        string set = args.Length > 0 ? args[0] : "34";
        var intrari = new List<(string Folder, string Eticheta)>();
        if (set == "34")
        {
            intrari.Add((Path.Combine("Reuters", "Reuters_34", "Training"), "Training"));
            intrari.Add((Path.Combine("Reuters", "Reuters_34", "Testing"), "Testing"));
        }
        else if (set == "7083")
        {
            intrari.Add((Path.Combine("Reuters", "Reuters_7083"), "All"));
        }
        else
        {
            Console.WriteLine("Set necunoscut: " + set + ". Foloseste 34 sau 7083.");
            return;
        }

        if (!File.Exists("stopwords.txt"))
        {
            Console.WriteLine("Nu gasesc stopwords.txt in " + Directory.GetCurrentDirectory());
            return;
        }
        PrelucrareText.CitesteStopwords("stopwords.txt");

        // Etapa 1: citim fiecare folder si salvam .txt-urile si vectorii locali.
        Directory.CreateDirectory(Path.Combine("output", "vectors"));
        var articole = new List<Articol>();
        foreach (var (folder, eticheta) in intrari)
        {
            // Reuters/Reuters_34/Training -> output/Reuters_34/Training
            string numeRelativ = Path.GetRelativePath("Reuters", folder);
            string folderText = Path.Combine("output", numeRelativ);
            Directory.CreateDirectory(folderText);

            List<Articol> articoleFolder = CitesteFolder(folder, eticheta, folderText);
            string fisierLocal = numeRelativ.Replace(Path.DirectorySeparatorChar, '_') + ".txt";
            FisiereRezultat.ScrieVectoriLocali(Path.Combine("output", "vectors", fisierLocal), articoleFolder);
            articole.AddRange(articoleFolder);
        }

        // Vectorul global (dictionarul): toate cuvintele din toate documentele.
        // SortedSet adauga un cuvant doar daca nu exista deja si le tine in ordine alfabetica.
        var vectorGlobal = new SortedSet<string>(StringComparer.Ordinal);
        foreach (Articol articol in articole)
        {
            foreach (string cuvant in articol.Frecvente.Keys)
                vectorGlobal.Add(cuvant);
        }

        // Pozitia fiecarui cuvant in vectorul global (0, 1, 2, ...).
        var pozitie = new Dictionary<string, int>();
        foreach (string cuvant in vectorGlobal)
            pozitie[cuvant] = pozitie.Count;

        string fisierVectori = Path.Combine("output", "Reuters_" + set + ".txt");
        FisiereRezultat.ScrieVectori(fisierVectori, vectorGlobal, pozitie, articole);

        Console.WriteLine($"Articole procesate: {articole.Count}");
        Console.WriteLine($"Cuvinte in vectorul global: {vectorGlobal.Count}");
        Console.WriteLine("Vectorii au fost salvati in " + fisierVectori);

        // Etapa 2: interogarea setului de date.
        Interogare.Porneste(articole);
    }

    // Citeste toate articolele dintr-un folder. Un fisier stricat nu opreste programul.
    static List<Articol> CitesteFolder(string folder, string eticheta, string folderText)
    {
        var optiuni = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        string[] fisiere = Directory.GetFiles(folder, "*.xml", optiuni);
        Array.Sort(fisiere, StringComparer.Ordinal);

        var articole = new List<Articol>();
        int erori = 0;
        foreach (string fisier in fisiere)
        {
            try
            {
                Articol articol = Articol.Citeste(fisier, eticheta);
                FisiereRezultat.ScrieText(folderText, articol);
                articole.Add(articol);
            }
            catch (Exception e)
            {
                Console.WriteLine("Eroare la " + Path.GetFileName(fisier) + ": " + e.Message);
                erori++;
            }
        }

        Console.WriteLine($"{eticheta}: {fisiere.Length} fisiere, {articole.Count} OK, {erori} erori");
        return articole;
    }

    // Programul poate fi pornit din bin\Debug\net10.0, asa ca urcam
    // din folder in folder pana gasim fisierul .csproj.
    static string GasesteFolderulProiectului()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder != null && folder.GetFiles("*.csproj").Length == 0)
            folder = folder.Parent;
        return folder != null ? folder.FullName : Directory.GetCurrentDirectory();
    }
}
