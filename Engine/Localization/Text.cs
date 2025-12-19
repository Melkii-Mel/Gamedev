using System;
using System.Collections.Generic;
using System.Linq;
using Gamedev.Debugging;
using Utils.FileParsing;

namespace Gamedev.Localization;

public class LocalizationCore
{
    private static LocalizationCore? _instance;

    private static LocalizationCore Instance
    {
        get
        {
            Diagnostics.Assert(_instance != null,
                () => new DebugMessage(MessageType.Error,
                    "Localizations.Instance is null. Initialize Localizations using Localizations.Init static function. Localizations will be initialized with an empty default value"));
            return _instance ?? Init(new Dictionary<string, Dictionary<string, string>>(), "en");
        }
    }

    internal static event Action? OnUpdate;

    #region api

    public Dictionary<string, Dictionary<string, string>> Localization;
    public string CurrentLanguage;

    public LocalizationCore(Dictionary<string, Dictionary<string, string>> localization, string currentLanguage)
    {
        Localization = localization;
        CurrentLanguage = currentLanguage;
    }

    public static void InitFromFile(string csvLocation, string? defaultLanguage = null)
    {
        var localization = Diagnostics.Try(() => Csv.Load(csvLocation),
            (Exception e) => new DebugMessage(MessageType.Error,
                $"Failed to load localization file, localization will be initialized with an empty fallback value: {e.Message}"),
            () => DefaultLocalization);
        Init(localization, defaultLanguage ?? localization.Keys.FirstOrDefault() ?? "en");
    }

    private static readonly Dictionary<string, Dictionary<string, string>> DefaultLocalization =
        new()
        {
            ["en"] = new Dictionary<string, string>(),
        };

    public static LocalizationCore Init(Dictionary<string, Dictionary<string, string>> localization,
        string currentLanguage)
    {
        _instance = new LocalizationCore(localization, currentLanguage);
        return _instance;
    }

    public static Dictionary<string, string> CurrentLocalization()
    {
        return Instance.Localization[Instance.CurrentLanguage];
    }

    public static string GetLanguage()
    {
        return Instance.CurrentLanguage;
    }

    public static void ChangeLanguage(string newLanguage)
    {
        Instance.CurrentLanguage = newLanguage;
        OnUpdate?.Invoke();
    }

    public static void ChangeSingleSource(string language, Dictionary<string, string> source)
    {
        Instance.Localization[language] = source;
        OnUpdate?.Invoke();
    }

    public static void ChangeAllSources(Dictionary<string, Dictionary<string, string>> sources)
    {
        Instance.Localization = sources;
        OnUpdate?.Invoke();
    }

    #endregion
}

public class Text
{
    private readonly string _key;

    public Text(string key)
    {
        _key = key;
        LocalizationCore.OnUpdate += () => OnUpdate?.Invoke(GetValue());
    }

    public event Action<string>? OnUpdate;

    public string GetValue()
    {
        return LocalizationCore.CurrentLocalization().TryGetValue(_key, out var value)
            ? value
            : Diagnostics.Debug
                ? $"ERROR: translation for `{_key}` key not found."
                : _key;
    }

    public void Bind(Action<string> setter)
    {
        OnUpdate += setter;
        setter(GetValue());
    }
}
