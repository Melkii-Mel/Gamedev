using System.Diagnostics;
using PresetGen.Utils;

namespace PresetGen;

public static class ProcessSpawner
{
    public static string Spawn(string processName, string commandName, string arguments, bool critical, Action<int, string, string> errorExitCodeHandler)
    {
        var psi = new ProcessStartInfo
        {
            FileName = processName,
            Arguments = commandName + " " + arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var process = Process.Start(psi);
        if (process == null)
        {
            new Err().Message($"Couldn't start `{processName + " " + commandName}` process" + (critical ? ", cannot continue" : ""));
            throw new NotSupportedException();
        }
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            errorExitCodeHandler(process.ExitCode, stdout, stderr);
        }
        return stdout;
    }
}
