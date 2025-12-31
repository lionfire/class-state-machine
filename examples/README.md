# LionFire.StateMachines Examples

This directory contains standalone example projects demonstrating the LionFire.StateMachines library.

## Examples

### BasicOrder

A simple order processing state machine demonstrating:
- State and transition enum definitions
- The `[StateMachine]` attribute on a class
- Basic `On{Transition}` convention methods
- Using `TryTransition` to check if transitions are valid

Run it:
```bash
cd BasicOrder
dotnet run
```

### AdvancedWorkflow

A document review workflow demonstrating advanced features:
- **Guard properties** (`Can{Transition}`) - Conditionally allow/prevent transitions
- **Before/After hooks** - Run code before or after specific transitions
- **State change events** - Subscribe to `StateChanged` event
- **Complex workflows** - Approval, rejection, revision cycles

Run it:
```bash
cd AdvancedWorkflow
dotnet run
```

## Building All Examples

From the solution root:
```bash
dotnet build examples/BasicOrder/BasicOrder.csproj
dotnet build examples/AdvancedWorkflow/AdvancedWorkflow.csproj
```

## Convention Method Reference

| Pattern | Purpose | Example |
|---------|---------|---------|
| `On{Transition}()` | Called during transition | `OnSubmit()` |
| `Can{Transition}` | Guard property (bool) | `CanSubmit => HasContent` |
| `Before{Transition}()` | Called before transition | `BeforeApprove()` |
| `After{Transition}()` | Called after transition | `AfterApprove()` |
| `OnEnter{State}()` | Called when entering state | `OnEnterApproved()` |
| `OnLeave{State}()` | Called when leaving state | `OnLeaveDraft()` |
