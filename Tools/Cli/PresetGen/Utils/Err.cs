using System.CommandLine.Parsing;
using System.Text;

namespace PresetGen.Utils;

public record Err(CommandResult? CommandResult = null, string? Expectation = null)
{
    public void Message(params string[] messages)
    {
        const string error = "error: ";
        const string indent = "\n       ";

        var sb = new StringBuilder();
        if (messages.Length > 0)
        {
            sb.AppendLine(error + string.Join(indent, messages));
            if (Expectation != null) sb.AppendLine(Expectation);
        }
        else
        {
            if (Expectation != null) sb.AppendLine(error + Expectation);
        }
#if DEBUG
        if (sb.Length == 0) throw new ArgumentException("No error message or expectation were provided.");
#endif
        if (CommandResult == null)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(sb.ToString());
            Console.ForegroundColor = prevColor;
            Environment.Exit(1);
        }
        else
        {
            CommandResult.AddError(sb.ToString());
        }
    }
}
