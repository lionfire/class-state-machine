# LionFire.StateMachines

Runtime implementation for convention-based state machines in C#.

## Overview

This package provides the core state machine runtime:

- `StateMachine<TState, TTransition>` - Static factory and transition registry
- `StateMachineState<TState, TTransition, TOwner>` - Per-instance state tracking
- Automatic discovery of convention methods via reflection

## Features

- **Convention-based**: Define behavior through naming conventions, not configuration
- **Type-safe**: Strongly typed states and transitions via enums
- **Lightweight**: Minimal runtime overhead
- **Event-driven**: `StateChanged` and `StateChanging` events

## Convention Methods

The runtime automatically discovers and invokes these methods on your class:

| Method Pattern | When Called |
|----------------|-------------|
| `On{Transition}()` | During transition execution |
| `Can{Transition}` (property) | Guard condition check |
| `OnEnter{State}()` / `On{State}()` | After entering a state |
| `OnLeave{State}()` / `Leave{State}()` | Before leaving a state |
| `After{State}()` | After state entry completes |

## Usage

```csharp
[StateMachine(typeof(OrderState), typeof(OrderTransition))]
public partial class Order
{
    private void OnStart()
    {
        Console.WriteLine("Order started processing");
    }

    private bool CanComplete => Items.Count > 0;

    private void OnEnterCompleted()
    {
        SendConfirmationEmail();
    }
}
```

## Related Packages

- **LionFire.StateMachines.Abstractions** - Core attributes and interfaces
- **LionFire.StateMachines.Generation** - Roslyn source generator (recommended)

## License

MIT
