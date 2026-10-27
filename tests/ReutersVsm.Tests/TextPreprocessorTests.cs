using ReutersVsm.Text;

namespace ReutersVsm.Tests;

public class TextPreprocessorTests
{
    private static readonly TextPreprocessor Preprocessor = new(new HashSet<string>(StringComparer.Ordinal) { "the", "its" });

    [Fact]
    public void ReplaceNonLettersWithSpaces_KeepsWordBoundaries() =>
        Assert.Equal("inc tuesday", TextPreprocessor.ReplaceNonLettersWithSpaces("inc.tuesday"));

    [Fact]
    public void Process_LowercasesStripsStopWordsShortTokensDigitsAndStems() =>
        Assert.Equal(
            ["internet", "network", "network"],
            Preprocessor.Process("The Internet's 55 NETWORKS,\n\"its\" network!"));

    [Fact]
    public void Process_ReturnsNothingForTextWithoutWords() =>
        Assert.Empty(Preprocessor.Process("1996-08-20 ... !!"));
}
