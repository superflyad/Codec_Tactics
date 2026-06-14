using CodecTactics.Core;

var tests = new (string Name, Action Test)[]
{
    ("project name is stable", () => AssertEqual("Codec_Tactics", ProjectInfo.Name)),
    ("foundation milestone is zero", () => AssertEqual(0, ProjectInfo.FoundationMilestone)),
    ("current focus documents repo setup", () => AssertContains("foundation", ProjectInfo.CurrentFocus)),
};

var failed = 0;

foreach (var (name, test) in tests)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Console.Error.WriteLine($"{failed} test(s) failed.");
    return 1;
}

Console.WriteLine($"{tests.Length} test(s) passed.");
return 0;

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

static void AssertContains(string expected, string actual)
{
    if (actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) < 0)
    {
        throw new InvalidOperationException($"Expected '{actual}' to contain '{expected}'.");
    }
}
