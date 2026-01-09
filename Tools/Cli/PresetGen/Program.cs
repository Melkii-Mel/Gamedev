using System.CommandLine;
using System.CommandLine.Parsing;

namespace PresetGen;

internal class Program
{
    static int Main(string[] args)
    {
        var root = new RootCommand("C# Datatypes Serialized JSON Preset Generator")
        {
            Gen.Command()
        };
        root.TreatUnmatchedTokensAsErrors = false;
        return root.Parse(args).Invoke();
    }

    private static int ProcessErrors(ParseResult parseResult)
    {
        if (parseResult.Errors.Count == 0)
        {
            return 0;
        }
        foreach (ParseError parseError in parseResult.Errors)
        {
            Console.Error.WriteLine(parseError.Message);
        }
        return 1;
    }
}
