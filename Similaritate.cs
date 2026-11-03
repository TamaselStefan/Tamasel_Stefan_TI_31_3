// 3. Metode de calcul a similaritatii intre 2 vectori.
// Vectorii sunt rari: un cuvant care lipseste are valoarea 0.
static class Similaritate
{
    public static readonly string[] Metode = { "Euclid", "Manhattan", "Cosinus" };

    // Pentru Euclid si Manhattan (distante) mai mic = mai asemanator,
    // pentru cosinus mai mare = mai asemanator.
    public static bool MaiMicEsteMaiBine(string metoda) => metoda != "Cosinus";

    public static double Calculeaza(string metoda, Dictionary<string, double> x, Dictionary<string, double> y)
    {
        return metoda switch
        {
            "Euclid" => Euclid(x, y),
            "Manhattan" => Manhattan(x, y),
            "Cosinus" => Cosinus(x, y),
            _ => throw new ArgumentException("Metoda necunoscuta: " + metoda)
        };
    }

    // d(x, x') = radical( suma (x_i - x'_i)^2 )
    static double Euclid(Dictionary<string, double> x, Dictionary<string, double> y)
    {
        double suma = 0;
        foreach (string cuvant in x.Keys.Union(y.Keys))
        {
            double diferenta = Valoare(x, cuvant) - Valoare(y, cuvant);
            suma += diferenta * diferenta;
        }
        return Math.Sqrt(suma);
    }

    // d(x, x') = suma |x_i - x'_i|
    static double Manhattan(Dictionary<string, double> x, Dictionary<string, double> y)
    {
        double suma = 0;
        foreach (string cuvant in x.Keys.Union(y.Keys))
            suma += Math.Abs(Valoare(x, cuvant) - Valoare(y, cuvant));
        return suma;
    }

    // cos(x1, x2) = <x1, x2> / (||x1|| * ||x2||)
    static double Cosinus(Dictionary<string, double> x, Dictionary<string, double> y)
    {
        double produsScalar = 0;
        foreach (var (cuvant, valoare) in x)
            produsScalar += valoare * Valoare(y, cuvant);

        double lungimi = Lungime(x) * Lungime(y);
        return lungimi == 0 ? 0 : produsScalar / lungimi;
    }

    // ||x|| = radical(<x, x>)
    static double Lungime(Dictionary<string, double> x)
    {
        double suma = 0;
        foreach (double valoare in x.Values)
            suma += valoare * valoare;
        return Math.Sqrt(suma);
    }

    static double Valoare(Dictionary<string, double> vector, string cuvant)
    {
        return vector.TryGetValue(cuvant, out double valoare) ? valoare : 0;
    }
}
