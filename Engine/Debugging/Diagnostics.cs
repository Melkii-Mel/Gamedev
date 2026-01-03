using System;

namespace Gamedev.Debugging;

public enum MessageType
{
    Info,
    Warning,
    Error,
}

public record DebugMessage(MessageType Type, string Message);

public static class Diagnostics
{
    public static bool Debug => EngineInstance.E.EssentialSettings.Debug;
    public static event Action<DebugMessage>? MessageTriggered;

    public static TValue Try<TValue, TException>(Func<TValue> expression, Func<TException, DebugMessage> errorHandler,
        Func<TValue> fallback) where TException : Exception
    {
        try
        {
            return expression();
        }
        catch (TException e)
        {
            if (Debug) MessageTriggered?.Invoke(errorHandler(e));
            return fallback();
        }
    }

    public static void Assert(bool condition, Func<DebugMessage> message)
    {
        if (!condition && Debug) MessageTriggered?.Invoke(message());
    }
}

public interface IDebugOutput
{
    void Out(DebugMessage message);
}

public class DebugOutput(IDebugOutput debugOutput)
{
    public void Info(string text)
    {
        debugOutput.Out(new DebugMessage(MessageType.Info, text));
    }

    public void Warning(string text)
    {
        debugOutput.Out(new DebugMessage(MessageType.Warning, text));
    }

    public void Error(string text)
    {
        debugOutput.Out(new DebugMessage(MessageType.Error, text));
    }
}
