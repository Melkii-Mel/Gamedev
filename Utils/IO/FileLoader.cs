using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utils.IO;

public static class FileLoader
{
    public enum LoadingPriority
    {
        External,

        // ReSharper disable once UnusedMember.Global
        Embedded,
    }

    private const string Assets = "Assets";

    private static Dictionary<string, string>? _assemblyFilesMap;

    private static Assembly? _assembly;

    private static Dictionary<string, string> AssemblyFilesMap
    {
        get
        {
            if (_assemblyFilesMap is not null) return _assemblyFilesMap;
            _assemblyFilesMap = ListAssemblyFiles().ToDictionary(k => k, k =>
            {
                var path = k.Replace('.', '/');
                var assetsIndex = path.IndexOf(Assets + "/", StringComparison.InvariantCulture);
                if (assetsIndex >= 0) path = path.Substring(assetsIndex + Assets.Length + 1);

                var last = path.LastIndexOf('/');
                if (last >= 0) path = path.Substring(0, last) + '.' + path.Substring(last + 1);

                return path;
            });
            return _assemblyFilesMap;
        }
    }

    private static Assembly Assembly
    {
        get
        {
            _assembly ??= Assembly.GetExecutingAssembly();
            return _assembly;
        }
    }

    public static string? LoadTextFile(string path, LoadingPriority priority = LoadingPriority.External)
    {
        return TryDecodeBytesOption(LoadAuto(path, priority));
    }

    public static byte[]? LoadByteFile(string path, LoadingPriority priority = LoadingPriority.External)
    {
        return LoadAuto(path, priority);
    }

    private static byte[]? LoadAuto(string path, LoadingPriority priority = LoadingPriority.External)
    {
        return priority == LoadingPriority.External
            ? LoadFile(path) ?? LoadEmbeddedResource(path)
            : LoadEmbeddedResource(path) ?? LoadFile(path);
    }

    private static byte[]? LoadFile(string path)
    {
        path = Path.Combine(AppContext.BaseDirectory, Assets, path);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    private static byte[]? LoadEmbeddedResource(string fileName)
    {
        if (!AssemblyFilesMap.TryGetValue(fileName, out fileName)) return null;
        using var stream = Assembly.GetManifestResourceStream(fileName);
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static string? TryDecodeBytesOption(byte[]? bytes)
    {
        return bytes != null ? DecodeBytes(bytes) : null;
    }

    private static string DecodeBytes(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    private static string[] ListAssemblyFiles()
    {
        return Assembly.GetManifestResourceNames();
    }
}
