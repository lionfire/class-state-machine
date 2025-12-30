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
                transform: static (context, ct) => GetStateMachineInfo(context, ct))
            .Where(static info => info is not null);

        // Generate source for each state machine class
        context.RegisterSourceOutput(classDeclarations, static (spc, info) =>
        {
            if (info is null) return;

            var source = GenerateStateMachineCode(info.Value);
            spc.AddSource($"{info.Value.ClassName}.StateMachine.g.cs", source);
        });
    }

    private static StateMachineInfo? GetStateMachineInfo(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
            return null;

        var classSymbol = context.TargetSymbol as INamedTypeSymbol;
        if (classSymbol is null)
            return null;

        // Get the [StateMachine] attribute data
        var attributeData = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.GetFullMetadataName() == StateMachineAttributeFullName);

        if (attributeData is null)
            return null;

        // Extract state and transition types from attribute constructor arguments
        if (attributeData.ConstructorArguments.Length < 2)
            return null;

        var stateTypeArg = attributeData.ConstructorArguments[0];
        var transitionTypeArg = attributeData.ConstructorArguments[1];

        if (stateTypeArg.Value is not INamedTypeSymbol stateTypeSymbol ||
            transitionTypeArg.Value is not INamedTypeSymbol transitionTypeSymbol)
            return null;

        // Get flags if provided
        var flags = GenerateStateMachineFlags.None;
        if (attributeData.ConstructorArguments.Length > 2 &&
            attributeData.ConstructorArguments[2].Value is int flagsValue)
        {
            flags = (GenerateStateMachineFlags)flagsValue;
        }

        if (flags.HasFlag(GenerateStateMachineFlags.DisableGeneration))
            return null;

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

        return new StateMachineInfo(
            classSymbol.Name,
            namespaceName,
            stateTypeSymbol.GetFullMetadataName(),
            transitionTypeSymbol.GetFullMetadataName(),
            usedTransitions,
            flags);
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
