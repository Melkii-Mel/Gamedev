namespace Utils.Tests;

public static class DataAccess
{
    private const string Back = "..";

    public static string Read(params string[] paths)
    {
        var path = Path.Combine(AppContext.BaseDirectory, Back, Back, Back, Path.Combine(paths));
        return File.ReadAllText(path);
    }

    public static string ReadTestData(params string[] paths)
    {
        return Read(Prepend(paths, "TestData"));
    }

    public static string ReadTestDataInput(params string[] paths)
    {
        return ReadTestData(Prepend(paths, "Inputs"));
    }

    public static string ReadTestDataSnapshots(params string[] paths)
    {
        return ReadTestData(Prepend(paths, "Snapshots"));
    }

    private static string[] Prepend(string[] a, params string[] b)
    {
        var result = new string[a.Length + b.Length];
        b.CopyTo(result, 0);
        a.CopyTo(result, b.Length);
        return result;
    }
}
