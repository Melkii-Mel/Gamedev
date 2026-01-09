using System.CommandLine.Parsing;

namespace PresetGen.Utils;

internal class ParamValidator
{
    public static bool Validate<T>(CommandResult commandResult, string expectation, T? value, string paramName, params Func<T, (bool condition, string message)>[] validators)
    {
        var err = new Err(commandResult, expectation);
        if (value == null)
        {
            err.Message($"argument '{paramName}' is required");
            return false;
        }
        foreach (var validator in validators)
        {
            var validatorResult = validator(value);
            if (validatorResult.condition)
            {
                err.Message(validatorResult.message);
                return false;
            }
        }
        return true;
    }
}
