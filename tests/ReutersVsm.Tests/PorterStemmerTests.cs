using ReutersVsm.Text;

namespace ReutersVsm.Tests;

public class PorterStemmerTests
{
    [Theory]
    // Sanity examples from the lab statement.
    [InlineData("caresses", "caress")]
    [InlineData("ponies", "poni")]
    [InlineData("cats", "cat")]
    [InlineData("agreed", "agre")]
    [InlineData("meeting", "meet")]
    [InlineData("connections", "connect")]
    [InlineData("relational", "relat")]
    [InlineData("happiness", "happi")]
    // Further examples from Porter's 1980 paper (full algorithm applied).
    [InlineData("feed", "feed")]
    [InlineData("hopping", "hop")]
    [InlineData("falling", "fall")]
    [InlineData("filing", "file")]
    [InlineData("sky", "sky")]
    [InlineData("rational", "ration")]
    [InlineData("conditional", "condit")]
    [InlineData("generalizations", "gener")]
    [InlineData("oscillators", "oscil")]
    [InlineData("controll", "control")]
    [InlineData("roll", "roll")]
    public void Stem_ProducesPorterStem(string word, string expected) =>
        Assert.Equal(expected, PorterStemmer.Stem(word));

    [Theory]
    [InlineData("is")]
    [InlineData("as")]
    [InlineData("a")]
    public void Stem_LeavesWordsOfLengthTwoOrLessUnchanged(string word) =>
        Assert.Equal(word, PorterStemmer.Stem(word));

    [Theory]
    [InlineData("tr", 0)]
    [InlineData("tree", 0)]
    [InlineData("trouble", 1)]
    [InlineData("oats", 1)]
    [InlineData("troubles", 2)]
    [InlineData("private", 2)]
    public void Measure_CountsVowelConsonantSequences(string stem, int expected) =>
        Assert.Equal(expected, PorterStemmer.Measure(stem));
}
