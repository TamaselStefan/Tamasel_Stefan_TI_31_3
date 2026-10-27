namespace ReutersVsm.Text;

/// <summary>
/// The Porter stemming algorithm as originally published:
/// M.F. Porter, "An algorithm for suffix stripping", Program 14(3), 130-137, 1980.
/// Input must be lowercase a-z. Words of length 2 or less are returned unchanged.
/// </summary>
/// <remarks>
/// Notation from the paper: m = number of vowel-consonant sequences in the stem,
/// *v* = stem contains a vowel, *d = stem ends with a double consonant,
/// *o = stem ends consonant-vowel-consonant where the last consonant is not w, x or y.
/// Within a step, only the longest matching suffix is considered; if its condition fails, the step does nothing.
/// </remarks>
internal static class PorterStemmer
{
    private sealed record SuffixRule(string Suffix, string Replacement, Func<string, bool> StemCondition);

    private static readonly SuffixRule[] Step1aRules = LongestFirst(
        Rules(_ => true, ("sses", "ss"), ("ies", "i"), ("ss", "ss"), ("s", "")));

    private static readonly SuffixRule[] Step2Rules = LongestFirst(
        Rules(stem => Measure(stem) > 0,
            ("ational", "ate"), ("tional", "tion"), ("enci", "ence"), ("anci", "ance"), ("izer", "ize"),
            ("abli", "able"), ("alli", "al"), ("entli", "ent"), ("eli", "e"), ("ousli", "ous"),
            ("ization", "ize"), ("ation", "ate"), ("ator", "ate"), ("alism", "al"), ("iveness", "ive"),
            ("fulness", "ful"), ("ousness", "ous"), ("aliti", "al"), ("iviti", "ive"), ("biliti", "ble")));

    private static readonly SuffixRule[] Step3Rules = LongestFirst(
        Rules(stem => Measure(stem) > 0,
            ("icate", "ic"), ("ative", ""), ("alize", "al"), ("iciti", "ic"), ("ical", "ic"), ("ful", ""), ("ness", "")));

    private static readonly SuffixRule[] Step4Rules = LongestFirst(
        Rules(stem => Measure(stem) > 1,
                ("al", ""), ("ance", ""), ("ence", ""), ("er", ""), ("ic", ""), ("able", ""), ("ible", ""),
                ("ant", ""), ("ement", ""), ("ment", ""), ("ent", ""), ("ou", ""), ("ism", ""), ("ate", ""),
                ("iti", ""), ("ous", ""), ("ive", ""), ("ize", ""))
            .Append(new SuffixRule("ion", "", stem => Measure(stem) > 1 && (stem.EndsWith('s') || stem.EndsWith('t')))));

    private static readonly string[] Step1bSuffixesNeedingVowel = ["ed", "ing"];

    public static string Stem(string word)
    {
        if (word.Length <= 2)
        {
            return word;
        }

        word = ApplyLongestMatchingRule(word, Step1aRules);
        word = Step1b(word);
        word = Step1c(word);
        word = ApplyLongestMatchingRule(word, Step2Rules);
        word = ApplyLongestMatchingRule(word, Step3Rules);
        word = ApplyLongestMatchingRule(word, Step4Rules);
        word = Step5a(word);
        return Step5b(word);
    }

    /// <summary>(m&gt;0) EED -&gt; EE; (*v*) ED -&gt; ; (*v*) ING -&gt; ; then tidy up the stem if ED or ING was removed.</summary>
    private static string Step1b(string word)
    {
        if (HasSuffix(word, "eed"))
        {
            var stem = word[..^3];
            return Measure(stem) > 0 ? stem + "ee" : word;
        }

        foreach (var suffix in Step1bSuffixesNeedingVowel)
        {
            if (!HasSuffix(word, suffix))
            {
                continue;
            }

            var stem = word[..^suffix.Length];
            return ContainsVowel(stem) ? TidyUpAfterStep1b(stem) : word;
        }

        return word;
    }

    /// <summary>AT -&gt; ATE, BL -&gt; BLE, IZ -&gt; IZE; (*d and not (*L or *S or *Z)) -&gt; single letter; (m=1 and *o) -&gt; E.</summary>
    private static string TidyUpAfterStep1b(string stem)
    {
        if (HasSuffix(stem, "at") || HasSuffix(stem, "bl") || HasSuffix(stem, "iz"))
        {
            return stem + "e";
        }

        if (EndsWithDoubleConsonant(stem) && stem[^1] is not ('l' or 's' or 'z'))
        {
            return stem[..^1];
        }

        return Measure(stem) == 1 && EndsWithCvc(stem) ? stem + "e" : stem;
    }

    /// <summary>(*v*) Y -&gt; I.</summary>
    private static string Step1c(string word) =>
        word.EndsWith('y') && ContainsVowel(word[..^1]) ? word[..^1] + "i" : word;

    /// <summary>(m&gt;1) E -&gt; ; (m=1 and not *o) E -&gt; .</summary>
    private static string Step5a(string word)
    {
        if (!word.EndsWith('e'))
        {
            return word;
        }

        var stem = word[..^1];
        var measure = Measure(stem);
        return measure > 1 || (measure == 1 && !EndsWithCvc(stem)) ? stem : word;
    }

    /// <summary>(m&gt;1 and *d and *L) -&gt; single letter.</summary>
    private static string Step5b(string word) =>
        HasSuffix(word, "ll") && Measure(word) > 1 ? word[..^1] : word;

    private static string ApplyLongestMatchingRule(string word, SuffixRule[] rulesLongestFirst)
    {
        foreach (var rule in rulesLongestFirst)
        {
            if (!HasSuffix(word, rule.Suffix))
            {
                continue;
            }

            var stem = word[..^rule.Suffix.Length];
            return rule.StemCondition(stem) ? stem + rule.Replacement : word;
        }

        return word;
    }

    /// <summary>m in the paper: the number of vowel-to-consonant transitions, i.e. n in [C](VC)^n[V].</summary>
    internal static int Measure(string stem)
    {
        var measure = 0;
        var previousIsVowel = false;
        for (var i = 0; i < stem.Length; i++)
        {
            var isConsonant = IsConsonant(stem, i);
            if (isConsonant && previousIsVowel)
            {
                measure++;
            }

            previousIsVowel = !isConsonant;
        }

        return measure;
    }

    /// <summary>A consonant is a letter other than a, e, i, o, u, and other than y preceded by a consonant.</summary>
    private static bool IsConsonant(string word, int index) => word[index] switch
    {
        'a' or 'e' or 'i' or 'o' or 'u' => false,
        'y' => index == 0 || !IsConsonant(word, index - 1),
        _ => true,
    };

    private static bool ContainsVowel(string stem)
    {
        for (var i = 0; i < stem.Length; i++)
        {
            if (!IsConsonant(stem, i))
            {
                return true;
            }
        }

        return false;
    }

    private static bool EndsWithDoubleConsonant(string word) =>
        word.Length >= 2 && word[^1] == word[^2] && IsConsonant(word, word.Length - 1);

    private static bool EndsWithCvc(string word)
    {
        var n = word.Length;
        return n >= 3
            && IsConsonant(word, n - 3)
            && !IsConsonant(word, n - 2)
            && IsConsonant(word, n - 1)
            && word[^1] is not ('w' or 'x' or 'y');
    }

    private static bool HasSuffix(string word, string suffix) => word.EndsWith(suffix, StringComparison.Ordinal);

    private static IEnumerable<SuffixRule> Rules(Func<string, bool> condition, params (string Suffix, string Replacement)[] rules) =>
        rules.Select(rule => new SuffixRule(rule.Suffix, rule.Replacement, condition));

    private static SuffixRule[] LongestFirst(IEnumerable<SuffixRule> rules) =>
        rules.OrderByDescending(rule => rule.Suffix.Length).ToArray();
}
