using System.Xml.Linq;

// Un articol citit din XML, dupa prelucrare.
// Cuvinte    = textul (titlu + paragrafe) dupa prelucrare, in ordinea din articol.
// Metadate   = pentru fiecare grup <codes>: clasa, apoi codurile lui, cate unul pe linie.
// Frecvente  = vectorul local: cuvant -> numar de aparitii.
record Articol(string Nume, string Eticheta, List<string> Topicuri, List<string> Metadate,
               List<string> Cuvinte, Dictionary<string, int> Frecvente)
{
    // Citeste un articol: <title> + fiecare <p> din <text> sunt continutul,
    // <codes class="bip:topics:1.0"> sunt categoriile.
    public static Articol Citeste(string fisier, string eticheta)
    {
        XElement radacina = XDocument.Load(fisier).Root;

        // Titlul si paragrafele, separate prin spatiu, ca sa nu se lipeasca cuvintele.
        string text = radacina.Element("title").Value;
        foreach (XElement paragraf in radacina.Element("text").Elements("p"))
            text += " " + paragraf.Value;

        var topicuri = new List<string>();
        var metadate = new List<string>();
        XElement metadata = radacina.Element("metadata");
        if (metadata != null)
        {
            foreach (XElement grup in metadata.Elements("codes"))
            {
                string clasa = (string)grup.Attribute("class");
                if (string.IsNullOrWhiteSpace(clasa))
                    continue;

                metadate.Add(clasa);
                foreach (XElement cod in grup.Elements("code"))
                {
                    string valoare = (string)cod.Attribute("code");
                    if (string.IsNullOrWhiteSpace(valoare))
                        continue;

                    metadate.Add(valoare);
                    if (clasa == "bip:topics:1.0")
                        topicuri.Add(valoare);
                }
            }
        }

        string nume = Path.GetFileNameWithoutExtension(fisier);
        List<string> cuvinte = PrelucrareText.ExtrageCuvinte(text);

        return new Articol(nume, eticheta, topicuri, metadate, cuvinte, PrelucrareText.NumaraCuvinte(cuvinte));
    }
}
