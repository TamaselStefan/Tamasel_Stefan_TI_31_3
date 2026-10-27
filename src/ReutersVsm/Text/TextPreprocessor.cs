namespace ReutersVsm.Text;

/// <summary>Turns raw article text into stemmed terms. Run it exactly once per article.</summary>
internal sealed class TextPreprocessor(IReadOnlySet<string> stopWords)
{
    public const int MinimumTokenLength = 3;

    public IReadOnlyList<string> Process(string text)
    {
        var lowercase = text.ToLowerInvariant();                 // 1. culture-invariant lowercase
        var lettersOnly = ReplaceNonLettersWithSpaces(lowercase); // 2. keep a-z and spaces only
        return Tokenize(lettersOnly)                              // 3. split, drop empty tokens
            .Where(token => !stopWords.Contains(token))           // 4. remove stop words
            .Where(token => token.Length >= MinimumTokenLength)   // 5. drop short tokens
            .Select(PorterStemmer.Stem)                           // 6. Porter stemming
            .ToList();
    }

    /// <summary>Every character other than a-z or space becomes a space, so word boundaries survive ("inc.tuesday" -> "inc tuesday").</summary>
    internal static string ReplaceNonLettersWithSpaces(string text) =>
        string.Create(text.Length, text, static (destination, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = IsKept(source[i]) ? source[i] : ' ';
            }
        });

    internal static string[] Tokenize(string text) => text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static bool IsKept(char character) => character is (>= 'a' and <= 'z') or ' ';
}
