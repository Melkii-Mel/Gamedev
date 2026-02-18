using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelegateImplementation.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Exception = System.Exception;

namespace DelegateImplementation;

[Generator]
public sealed class DelegateImplementationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();

        var classes = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
            static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
        );

        context.RegisterSourceOutput(
            classes.Combine(context.CompilationProvider),
            static (spc, pair) => { Execute(spc, pair.Right, pair.Left); });
    }

    private static void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ClassDeclarationSyntax classDeclaration
    )
    {
        var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        if (model.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol) return;
        var attrSymbol = compilation.GetTypeByMetadataName("Attributes.DelegateImplementationAttribute");
        if (attrSymbol is null) return;
        var ignoredIndexers = AttributeProcessor
            .FindMultiple(compilation, classSymbol, "Attributes.IgnoredIndexerAttribute")
            .Select(ap => ap.GetArgs<INamedTypeSymbol>().ToArray()).ToArray();
        foreach (var attributeProcessor in AttributeProcessor.FindMultiple(compilation, classSymbol,
                     "Attributes.DelegateImplementationAttribute"))
        {
            try
            {
                var openInterfaceSymbol = attributeProcessor.GetArg<INamedTypeSymbol>();
                INamedTypeSymbol? interfaceSymbol;
                if (openInterfaceSymbol.Arity == 0)
                {
                    interfaceSymbol = openInterfaceSymbol;
                }
                else
                {
                    interfaceSymbol = classSymbol.AllInterfaces.FirstOrDefault(i =>
                        SymbolEqualityComparer.Default.Equals(i.OriginalDefinition.ConstructUnboundGenericType(),
                            openInterfaceSymbol));
                    if (interfaceSymbol == null)
                    {
                        // TODO: Create helper class for more convenient diagnostic
                        context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(id: "DIG001",
                                title: "Interface Not Specified for Delegation",
                                messageFormat: "An error occurred: {0}",
                                category: nameof(DelegateImplementationGenerator), DiagnosticSeverity.Error,
                                isEnabledByDefault: true), classDeclaration.GetLocation(),
                            "Specify the interface implementation in the class definition to delegate a generic interface."));
                        continue;
                    }
                }

                // TODO: Improve error handling
                var targetProperty = attributeProcessor.GetArg<string>();
                var implicitDelegation = (int)(attributeProcessor.GetArg<object?>(() => null) ?? 0) == 0;
                var ignoredMembers = attributeProcessor.GetArgs<string>(() => "").ToArray();
                GenerateDelegation(
                    context,
                    classSymbol,
                    interfaceSymbol,
                    targetProperty,
                    implicitDelegation,
                    ignoredMembers,
                    ignoredIndexers
                );
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(id: "DIG002",
                    title: "Delegate implementation generator failed", messageFormat: "An error occurred: {0}. StackTrace: {1}",
                    category: nameof(DelegateImplementationGenerator), DiagnosticSeverity.Error,
                    isEnabledByDefault: true), classDeclaration.GetLocation(), ex.Message, ex.StackTrace));
            }
        }
    }

    private static void GenerateDelegation(SourceProductionContext context,
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol interfaceSymbol,
        string targetPropertyName,
        bool implicitDelegation,
        string[] ignoredMembers,
        INamedTypeSymbol[][] ignoredIndexers)
    {
        var ns = classSymbol.ContainingNamespace;
        var className = classSymbol.Name;
        var interfaceName = interfaceSymbol.ToDisplayString();
        var interfaceMembers = ExtractInterfaceMembers();
        var methods = interfaceMembers.Methods;
        var properties = interfaceMembers.Properties;

        var source = GenSource();
        context.AddSource(
            $"{className}_{interfaceSymbol.Name}_Delegation.g.cs",
            SourceText.From(source, Encoding.UTF8)
        );

        return;

        string GenSource()
        {
            return
                $"// <auto-generated />\n" +
                $"#nullable enable\n" +
                $"{GenUsings()}\n\n" +
                ns.IsGlobalNamespace switch
                {
                    true => GenClassDef(),
                    false => $"namespace {ns.ToDisplayString()}\n{{\n{GenClassDef()}\n}}",
                };
        }

        string GenUsings()
        {
            var namespaces = new HashSet<INamespaceSymbol>(SymbolEqualityComparer.Default);
            foreach (var method in methods)
            {
                CollectNamespaces(method.Symbol.ReturnType, namespaces);
            }

            foreach (var property in properties)
            {
                CollectNamespaces(property.Symbol.Type, namespaces);
            }

            return string.Join("\n", namespaces.Select(n => $"using {n.ToDisplayString()};"));

            void CollectNamespaces(ITypeSymbol? type, HashSet<INamespaceSymbol> set)
            {
                while (true)
                {
                    if (type is null) return;
                    if (type.SpecialType == SpecialType.System_Void) return;
                    if (!type.ContainingNamespace.IsGlobalNamespace) set.Add(type.ContainingNamespace);
                    switch (type)
                    {
                        case INamedTypeSymbol named:
                            foreach (var arg in named.TypeArguments) CollectNamespaces(arg, set);
                            break;
                        case IArrayTypeSymbol array:
                            type = array.ElementType;
                            continue;
                        case IPointerTypeSymbol pointer:
                            type = pointer.PointedAtType;
                            continue;
                    }

                    break;
                }
            }
        }

        string GenClassDef()
        {
            var interfaceImplementation = interfaceSymbol.Arity == 0 ? $": {interfaceName} " : "";
            var generics = classSymbol.Arity == 0
                ? ""
                : $"<{string.Join(", ", classSymbol.TypeParameters.Select(tp => tp.Name))}>";
            return $"public partial class {className}{generics} {interfaceImplementation}{{ {GenClassContent()} }}";
        }

        string GenClassContent()
        {
            var sb = new StringBuilder();
            foreach (var method in methods) sb.AppendLine(GenMethodDelegation(method));
            foreach (var property in properties) sb.AppendLine(GenPropertyDelegation(property));
            return sb.ToString();
        }

        string GenMethodDelegation(InterfaceMembers.Method method)
        {
            var symbol = method.Symbol;
            var name = symbol.Name;
            var parameters = string.Join(", ",
                symbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            var args = string.Join(", ", symbol.Parameters.Select(p => p.Name));
            return
                $"{GenMemberSignatureName(symbol.ReturnType, symbol.ContainingSymbol, method.MustBeExplicit)}{name}({parameters}) => {targetPropertyName}.{name}({args});";
        }

        string GenPropertyDelegation(InterfaceMembers.Property property)
        {
            var symbol = property.Symbol;
            (string Type, string Name)[] parameters =
                symbol.Parameters.Select(p => (p.Type.ToDisplayString(), p.Name)).ToArray();

            var isIndexer = symbol.IsIndexer;
            var propertyName = symbol.Name;
            var index = targetPropertyName + (isIndexer ? $"[{ArgList()}]" : $".{propertyName}");
            var name = isIndexer ? $"this[{ParamList()}]" : propertyName;
            var body =
                (symbol.SetMethod != null ? $"set => {index} = value;" : "") +
                (symbol.GetMethod != null ? $"get => {index};" : "");

            var sb = new StringBuilder();
            sb.AppendLine(
                $"{GenMemberSignatureName(symbol.Type, symbol.ContainingSymbol, property.MustBeExplicit)}{name} {{ {body} }}");

            return sb.ToString();

            string ArgList() => string.Join(", ", parameters.Select(p => p.Name));
            string ParamList() => string.Join(", ", parameters.Select(p => $"{p.Type} {p.Name}"));
        }

        string GenMemberSignatureName(ITypeSymbol symbol, ISymbol memberContainer, bool mustBeExplicit)
        {
            return implicitDelegation && !mustBeExplicit
                ? $"public {symbol.ToDisplayString()} "
                : $"{symbol.ToDisplayString()} {memberContainer.ToDisplayString()}.";
        }

        InterfaceMembers ExtractInterfaceMembers()
        {
            var methodSymbols = new List<IMethodSymbol>();
            var propertySymbols = new List<IPropertySymbol>();

            IEnumerable<INamedTypeSymbol> interfaces =
                [interfaceSymbol, .. interfaceSymbol.AllInterfaces];

            foreach (var iface in interfaces)
            {
                foreach (var member in iface.GetMembers().Where(m =>
                             !ignoredMembers.Contains(m.Name) && !(m is IPropertySymbol { IsIndexer: true } ps &&
                                                                   ignoredIndexers.Any(ii =>
                                                                       ii.SequenceEqual(
                                                                           ps.Parameters.Select(p => p.Type),
                                                                           SymbolEqualityComparer.Default)))))
                {
                    switch (member)
                    {
                        case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false } m:
                            methodSymbols.Add(m);
                            break;

                        case IPropertySymbol p:
                            propertySymbols.Add(p);
                            break;
                    }
                }
            }

            return new InterfaceMembers(methodSymbols, propertySymbols);
        }
    }
}

public class InterfaceMembers
{
    private readonly List<Method> _methods;
    private readonly List<Property> _properties;

    public InterfaceMembers(List<IMethodSymbol> methodSymbols, List<IPropertySymbol> propertySymbols)
    {
        _methods = Init(methodSymbols, s => s.Parameters.Length, (s, b) => new Method(s, b));
        _properties = Init(propertySymbols, s => s.Parameters.Length, (s, b) => new Property(s, b));
    }

    private static List<TOut> Init<TIn, TOut>(IEnumerable<TIn> symbols, Func<TIn, int> getParametersCount,
        Func<TIn, bool, TOut> createOut) where TIn : ISymbol
    {
        var orderedArray = Order(symbols).ToArray();
        var signatureSet = new HashSet<(string, int)>();
        var result = new List<TOut>(orderedArray.Length);
        result.AddRange(orderedArray.Select(symbol => !signatureSet.Add((symbol.Name, getParametersCount(symbol)))
            ? createOut(symbol, true)
            : createOut(symbol, false)));

        return result;
    }

    public Method[] Methods => _methods.ToArray();

    public Property[] Properties => _properties.ToArray();

    private static IEnumerable<T> Order<T>(IEnumerable<T> symbols) where T : ISymbol
    {
        return symbols.OrderByDescending(s => s.ContainingType.IsGenericType)
            .ThenByDescending(s => s.ContainingType.Arity)
            .ThenByDescending(s =>
                s.ContainingType.AllInterfaces.Length)
            .ThenBy(s => s.ContainingType.Name);
    }

    public record Method(IMethodSymbol Symbol, bool MustBeExplicit);

    public record Property(IPropertySymbol Symbol, bool MustBeExplicit);
}
