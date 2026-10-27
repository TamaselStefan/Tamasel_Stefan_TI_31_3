using ReutersVsm.Cli;

namespace ReutersVsm.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_DefaultsToReuters34TrainingAndTesting()
    {
        var options = CommandLineParser.Parse([]);

        Assert.Equal(
            [new InputFolder("Reuters/Reuters_34/Training", "Training"), new InputFolder("Reuters/Reuters_34/Testing", "Testing")],
            options.InputFolders);
        Assert.Equal("output/reuters_34.txt", options.OutputPath);
        Assert.Equal("stopwords.txt", options.StopWordsPath);
    }

    [Fact]
    public void Parse_Preset7083UsesLabelAll()
    {
        var options = CommandLineParser.Parse(["--preset", "7083"]);

        Assert.Equal([new InputFolder("Reuters/Reuters_7083", "All")], options.InputFolders);
        Assert.Equal("output/reuters_7083.txt", options.OutputPath);
    }

    [Fact]
    public void Parse_ExplicitInputsReplacePresetInputs()
    {
        var options = CommandLineParser.Parse(["--input", "data/news=All", "--output", "out.txt"]);

        Assert.Equal([new InputFolder("data/news", "All")], options.InputFolders);
        Assert.Equal("out.txt", options.OutputPath);
    }

    [Theory]
    [InlineData("--bogus")]
    [InlineData("--output")]
    [InlineData("--input", "noLabel")]
    [InlineData("--preset", "99")]
    public void Parse_RejectsInvalidArguments(params string[] args) =>
        Assert.Throws<ArgumentException>(() => CommandLineParser.Parse(args));
}
