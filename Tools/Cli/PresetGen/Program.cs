using System.CommandLine;

namespace PresetGen;

internal class Program
{
    private static int Main(string[] args)
    {
        var root = new RootCommand("C# Datatypes Serialized JSON Preset Generator")
        {
            Gen.Command(),
        };
        root.TreatUnmatchedTokensAsErrors = false;
        return root.Parse(args).Invoke();
    }
}
