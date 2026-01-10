using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Kernel;
using PresetGen.Utils;

namespace PresetGen;

internal static class Gen
{
    private const string PathArgument = "assembly-or-project-path";
    private const string TypeArgument = "type-full-name";
    private const string PathExpectation = "expected a path to .dll, .csproj, or project directory";
    private const string TypeExpectation = "expected a full target type name, including namespace";
    private static readonly string[] ValidExtensions = [".csproj", ".dll"];

    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification = "<Pending>")]
    public static Command Command()
    {
        var pathArg = new Argument<FileSystemInfo>(PathArgument)
        {
            Description = ".dll or .csproj path",
        };
        var typeArg = new Argument<string>(TypeArgument)
        {
            Description = "Full name of the target type, including namespace",
        };
        var genCommand = new Command("gen", "Generate JSON based on a type signature")
        {
            pathArg,
            typeArg,
        };
        genCommand.Validators.Add(result =>
        {
            var path = result.GetValue(pathArg);
            ParamValidator.Validate(result, PathExpectation, path, PathArgument,
                p => (!p.Exists, $"path '{p.FullName}' doesn't exist"),
                p =>
                (
                    !p.Attributes.HasFlag(FileAttributes.Directory) && !ValidExtensions.Contains(p.Extension),
                    $"path '{p.FullName}' must be a .dll file, a .csproj file, or a directory"
                )
            );
            var type = result.GetValue(typeArg);
            ParamValidator.Validate(result, TypeExpectation, type, TypeArgument);
        });
        genCommand.SetAction(parserResult =>
        {
            var path = parserResult.GetRequiredValue(pathArg);
            var typeName = parserResult.GetRequiredValue(typeArg);
            var type = LoadType(path, typeName);
            if (type is null)
            {
                new Err().Message($"loaded assembly doesn't contain {typeName} type",
                    "ensure the full name is specified");
                return;
            }

            var fixture = new Fixture();
            fixture.Register(() => 0);
            fixture.Register(() => 0f);
            fixture.Register(() => 0d);
            fixture.Register(() => "");
            fixture.Register(() => false);
            fixture.RepeatCount = 1;
            var instance = fixture.Create(type, new SpecimenContext(fixture));
            Console.WriteLine(JsonSerializer.Serialize(instance, instance.GetType(), new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
            }));

            return;

            static Type? LoadType(FileSystemInfo? path, string type)
            {
                switch (path)
                {
                    case FileInfo file:
                        return file.Extension switch
                        {
                            ".dll" => LoadTypeFromAssembly(file.FullName, type),
                            ".csproj" => LoadTypeFromProjectName(file.FullName, type),
                            _ => throw new NotSupportedException(),
                        };
                    case DirectoryInfo directory:
                        var projectFiles = Directory.GetFiles(directory.FullName, "*.csproj",
                            SearchOption.TopDirectoryOnly);
                        var err = new Err(Expectation: PathExpectation);
                        switch (projectFiles.Length)
                        {
                            case 0:
                                err.Message("path doesn't contain .csproj file");
                                return null;
                            case > 1:
                                err.Message("path can't contain multiple .csproj files");
                                return null;
                            default:
                                return LoadTypeFromProjectName(projectFiles[0], type);
                        }

                    default:
                        throw new NotSupportedException();
                }
            }
        });
        return genCommand;
    }

    private static Type? LoadTypeFromProjectName(string projName, string type)
    {
        var dllPath = BuildProject(projName);
        return LoadTypeFromAssembly(dllPath, type);

        static string BuildProject(string projName)
        {
            Console.WriteLine(projName);
            return ProcessSpawner.Spawn("dotnet", "msbuild",
                    $"\"{projName}\" -t:Build -getProperty:TargetPath -nologo -v:q -p:CopyLocalLockFileAssemblies=true",
                    true, (code, @out, err) => { new Err().Message(@out, err, $"build failed with exit code {code}"); })
                .Trim();
        }
    }

    private static Type? LoadTypeFromAssembly(string assemblyFile, string typeName)
    {
        Assembly assembly;
        try
        {
            assembly = Assembly.LoadFrom(assemblyFile);
        }
        catch (Exception e)
        {
            new Err().Message("Could not load assembly for the following reason: " + e.Message);
            return null;
        }

        return assembly.GetType(typeName);
    }
}
