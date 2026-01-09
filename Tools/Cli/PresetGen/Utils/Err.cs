using System.CommandLine.Parsing;
using System.Text;

namespace PresetGen.Utils;

public record Err(CommandResult? CommandResult = null, string? Expectation = null)
{
    public void Message(params string[] messages)
    {
        const string Error = "error: ";
        const string Indent = "\n       ";

        var sb = new StringBuilder();
        if (messages.Length > 0)
        {
            sb.AppendLine(Error + string.Join(Indent, messages));
            if (Expectation != null)
            {
                sb.AppendLine(Expectation);
            }
        }
        else
        {
            if (Expectation != null)
            {
                sb.AppendLine(Error + Expectation);
            }
        }
#if DEBUG
        if (sb.Length == 0)
        {
            throw new ArgumentException("No error message or expectation were provided.");
        }
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