using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace LionFire.StateMachines.Class.Generation;

// Local copy of flags to avoid loading external assembly at generator runtime
[Flags]
internal enum GenerateStateMachineFlags
{
    None = 0,
    DisablePruneUnusedStates = 1 << 0,
    DisablePruneUnusedTransitions = 1 << 1,
    DisableGeneration = 1 << 2,
    NoLock = 1 << 3,
}

/// <summary>
/// Incremental source generator that generates state machine code for classes
/// decorated with [StateMachine] attribute.
/// </summary>
[Generator]
public class StateMachineGenerator : IIncrementalGenerator
{
    private const string StateMachineAttributeFullName = "LionFire.StateMachines.Class.StateMachineAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all class declarations with [StateMachine] attribute
        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                StateMachineAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (context, ct) => GetGeneratorResult(context, ct));

        // Generate source and report diagnostics for each state machine class
        context.RegisterSourceOutput(classDeclarations, static (spc, result) =>
        {
            // Report any diagnostics
            foreach (var diagnostic in result.Diagnostics)
            {
                spc.ReportDiagnostic(diagnostic);
            }

            // Generate source if we have valid info
            if (result.Info is not null)
            {
                var source = GenerateStateMachineCode(result.Info.Value);
                spc.AddSource($"{result.Info.Value.ClassName}.StateMachine.g.cs", source);
            }
        });
    }

    private static GeneratorResult GetGeneratorResult(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
            return new GeneratorResult(null, diagnostics.ToImmutable());

        var classSymbol = context.TargetSymbol as INamedTypeSymbol;
        if (classSymbol is null)
            return new GeneratorResult(null, diagnostics.ToImmutable());

        // Check if class is partial
        var isPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        if (!isPartial)
        {
            diagnostics.Add(Diagnostic.Create(
                Diagnostics.ClassMustBePartial,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name));
            return new GeneratorResult(null, diagnostics.ToImmutable());
        }

        // Get the [StateMachine] attribute data
        var attributeData = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.GetFullMetadataName() == StateMachineAttributeFullName);

        if (attributeData is null)
            return new GeneratorResult(null, diagnostics.ToImmutable());

        // Extract state and transition types from attribute constructor arguments
        if (attributeData.ConstructorArguments.Length < 2)
            return new GeneratorResult(null, diagnostics.ToImmutable());

        var stateTypeArg = attributeData.ConstructorArguments[0];
        var transitionTypeArg = attributeData.ConstructorArguments[1];

        var stateTypeSymbol = stateTypeArg.Value as INamedTypeSymbol;
        var transitionTypeSymbol = transitionTypeArg.Value as INamedTypeSymbol;

        // Validate state type is an enum
        if (stateTypeSymbol is null || stateTypeSymbol.TypeKind != TypeKind.Enum)
        {
            var typeName = stateTypeSymbol?.Name ?? stateTypeArg.Value?.ToString() ?? "null";
            diagnostics.Add(Diagnostic.Create(
                Diagnostics.StateTypeMustBeEnum,
                attributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation(),
                typeName));
            return new GeneratorResult(null, diagnostics.ToImmutable());
        }

        // Validate transition type is an enum
        if (transitionTypeSymbol is null || transitionTypeSymbol.TypeKind != TypeKind.Enum)
        {
            var typeName = transitionTypeSymbol?.Name ?? transitionTypeArg.Value?.ToString() ?? "null";
            diagnostics.Add(Diagnostic.Create(
                Diagnostics.TransitionTypeMustBeEnum,
                attributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation(),
                typeName));
            return new GeneratorResult(null, diagnostics.ToImmutable());
        }

        // Get flags if provided
        var flags = GenerateStateMachineFlags.None;
        if (attributeData.ConstructorArguments.Length > 2 &&
            attributeData.ConstructorArguments[2].Value is int flagsValue)
        {
            flags = (GenerateStateMachineFlags)flagsValue;
        }

        if (flags.HasFlag(GenerateStateMachineFlags.DisableGeneration))
            return new GeneratorResult(null, diagnostics.ToImmutable());

        // Get namespace
        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : classSymbol.ContainingNamespace.ToDisplayString();

        // Get all state enum members
        var stateMembers = stateTypeSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasConstantValue)
            .Select(f => f.Name)
            .ToImmutableArray();

        // Get all transition enum members
        var transitionMembers = transitionTypeSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.HasConstantValue)
            .Select(f => f.Name)
            .ToImmutableArray();

        // Warn if enums are empty
        if (stateMembers.Length == 0)
        {
            diagnostics.Add(Diagnostic.Create(
                Diagnostics.StateEnumEmpty,
                attributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation(),
                stateTypeSymbol.Name));
        }

        if (transitionMembers.Length == 0)
        {
            diagnostics.Add(Diagnostic.Create(
                Diagnostics.TransitionEnumEmpty,
                attributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation(),
                transitionTypeSymbol.Name));
        }

        // Get methods defined on the class to determine which transitions are used
        var classMethods = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Select(m => m.Identifier.Text)
            .ToImmutableHashSet();

        var classProperties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(p => p.Identifier.Text)
            .ToImmutableHashSet();

        // Determine which transitions have convention methods
        var usedTransitions = flags.HasFlag(GenerateStateMachineFlags.DisablePruneUnusedTransitions)
            ? transitionMembers
            : transitionMembers
                .Where(t => HasConventionMethod(t, classMethods, classProperties))
                .ToImmutableArray();

        // Warn if no transitions will be generated
        if (usedTransitions.Length == 0 && transitionMembers.Length > 0)
        {
            diagnostics.Add(Diagnostic.Create(
                Diagnostics.NoTransitionsGenerated,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name));
        }

        var info = new StateMachineInfo(
            classSymbol.Name,
            namespaceName,
            stateTypeSymbol.GetFullMetadataName(),
            transitionTypeSymbol.GetFullMetadataName(),
            usedTransitions,
            flags);

        return new GeneratorResult(info, diagnostics.ToImmutable());
    }

    private static bool HasConventionMethod(string transition, ImmutableHashSet<string> methods, ImmutableHashSet<string> properties)
    {
        // Check for transition handler methods
        if (methods.Contains($"On{transition}")) return true;
        if (methods.Contains(transition)) return true;
        if (methods.Contains($"After{transition}")) return true;
        if (methods.Contains($"Before{transition}")) return true;

        // Check for Can{Transition} guard property
        if (properties.Contains($"Can{transition}")) return true;

        return false;
    }

    private static string GenerateStateMachineCode(StateMachineInfo info)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using LionFire.StateMachines.Class;");
        sb.AppendLine();

        if (info.Namespace is not null)
        {
            sb.AppendLine($"namespace {info.Namespace}");
            sb.AppendLine("{");
        }

        var indent = info.Namespace is not null ? "    " : "";

        sb.AppendLine($"{indent}public partial class {info.ClassName}");
        sb.AppendLine($"{indent}{{");

        // Generate StateMachine property
        var stateType = info.StateTypeFullName;
        var transitionType = info.TransitionTypeFullName;
        var stateMachineStateType = $"StateMachineState<{stateType}, {transitionType}, {info.ClassName}>";

        sb.AppendLine($"{indent}    private {stateMachineStateType}? _stateMachine;");
        sb.AppendLine();
        sb.AppendLine($"{indent}    public {stateMachineStateType} StateMachine");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        get");
        sb.AppendLine($"{indent}        {{");
        sb.AppendLine($"{indent}            if (_stateMachine == null)");
        sb.AppendLine($"{indent}            {{");
        sb.AppendLine($"{indent}                _stateMachine = StateMachine<{stateType}, {transitionType}>.Create(this);");
        sb.AppendLine($"{indent}            }}");
        sb.AppendLine($"{indent}            return _stateMachine;");
        sb.AppendLine($"{indent}        }}");
        sb.AppendLine($"{indent}    }}");

        // Generate transition methods
        foreach (var transition in info.Transitions)
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}    public void {transition}() => StateMachine.Transition({transitionType}.{transition});");
        }

        sb.AppendLine($"{indent}}}");

        if (info.Namespace is not null)
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Result from analyzing a class with [StateMachine] attribute.
    /// Contains either valid info for generation or diagnostics to report.
    /// </summary>
    private readonly struct GeneratorResult : IEquatable<GeneratorResult>
    {
        public StateMachineInfo? Info { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public GeneratorResult(StateMachineInfo? info, ImmutableArray<Diagnostic> diagnostics)
        {
            Info = info;
            Diagnostics = diagnostics;
        }

        public bool Equals(GeneratorResult other)
        {
            if (!EqualityComparer<StateMachineInfo?>.Default.Equals(Info, other.Info))
                return false;

            if (Diagnostics.Length != other.Diagnostics.Length)
                return false;

            // Compare diagnostic IDs for equality (not full diagnostic comparison)
            for (int i = 0; i < Diagnostics.Length; i++)
            {
                if (Diagnostics[i].Id != other.Diagnostics[i].Id)
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj) => obj is GeneratorResult other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Info?.GetHashCode() ?? 0;
                hash = hash * 31 + Diagnostics.Length;
                return hash;
            }
        }
    }

    private readonly struct StateMachineInfo : IEquatable<StateMachineInfo>
    {
        public string ClassName { get; }
        public string Namespace { get; }
        public string StateTypeFullName { get; }
        public string TransitionTypeFullName { get; }
        public ImmutableArray<string> Transitions { get; }
        public GenerateStateMachineFlags Flags { get; }

        public StateMachineInfo(
            string className,
            string namespaceName,
            string stateTypeFullName,
            string transitionTypeFullName,
            ImmutableArray<string> transitions,
            GenerateStateMachineFlags flags)
        {
            ClassName = className;
            Namespace = namespaceName;
            StateTypeFullName = stateTypeFullName;
            TransitionTypeFullName = transitionTypeFullName;
            Transitions = transitions;
            Flags = flags;
        }

        public bool Equals(StateMachineInfo other)
        {
            return ClassName == other.ClassName
                && Namespace == other.Namespace
                && StateTypeFullName == other.StateTypeFullName
                && TransitionTypeFullName == other.TransitionTypeFullName
                && Transitions.SequenceEqual(other.Transitions)
                && Flags == other.Flags;
        }

        public override bool Equals(object obj) => obj is StateMachineInfo other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + ClassName.GetHashCode();
                hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
                hash = hash * 31 + StateTypeFullName.GetHashCode();
                hash = hash * 31 + TransitionTypeFullName.GetHashCode();
                hash = hash * 31 + Transitions.Length;
                hash = hash * 31 + (int)Flags;
                return hash;
            }
        }
    }
}
