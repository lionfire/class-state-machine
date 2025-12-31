using Microsoft.CodeAnalysis;

namespace LionFire.StateMachines.Class.Generation;

/// <summary>
/// Diagnostic descriptors for the StateMachine source generator.
/// </summary>
internal static class Diagnostics
{
    private const string Category = "LionFire.StateMachines";

    /// <summary>
    /// LFSM001: Class must be partial to use [StateMachine] attribute.
    /// </summary>
    public static readonly DiagnosticDescriptor ClassMustBePartial = new(
        id: "LFSM001",
        title: "Class must be partial",
        messageFormat: "Class '{0}' must be declared as partial to use the [StateMachine] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes decorated with [StateMachine] must be partial so the generator can add the state machine implementation.");

    /// <summary>
    /// LFSM002: State type must be an enum.
    /// </summary>
    public static readonly DiagnosticDescriptor StateTypeMustBeEnum = new(
        id: "LFSM002",
        title: "State type must be an enum",
        messageFormat: "State type '{0}' must be an enum",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The state type parameter of [StateMachine] must be an enum type.");

    /// <summary>
    /// LFSM003: Transition type must be an enum.
    /// </summary>
    public static readonly DiagnosticDescriptor TransitionTypeMustBeEnum = new(
        id: "LFSM003",
        title: "Transition type must be an enum",
        messageFormat: "Transition type '{0}' must be an enum",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The transition type parameter of [StateMachine] must be an enum type.");

    /// <summary>
    /// LFSM004: No transitions will be generated.
    /// </summary>
    public static readonly DiagnosticDescriptor NoTransitionsGenerated = new(
        id: "LFSM004",
        title: "No transitions generated",
        messageFormat: "No transition methods will be generated for '{0}' because no convention methods were found",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "No convention methods were found for any transitions. Add On{Transition}, Can{Transition}, After{Transition}, or Before{Transition} methods, or use DisablePruneUnusedTransitions flag.");

    /// <summary>
    /// LFSM005: State enum has no members.
    /// </summary>
    public static readonly DiagnosticDescriptor StateEnumEmpty = new(
        id: "LFSM005",
        title: "State enum is empty",
        messageFormat: "State enum '{0}' has no members",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The state enum should have at least one member to represent states.");

    /// <summary>
    /// LFSM006: Transition enum has no members.
    /// </summary>
    public static readonly DiagnosticDescriptor TransitionEnumEmpty = new(
        id: "LFSM006",
        title: "Transition enum is empty",
        messageFormat: "Transition enum '{0}' has no members",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The transition enum should have at least one member to represent transitions.");
}
