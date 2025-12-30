# LionFire.StateMachines.Abstractions

Core attributes and interfaces for defining convention-based state machines in C#.

## Overview

This package provides the foundational types for describing state machines:

- `[State]` - Mark enum values as states
- `[Transition(from, to)]` - Define valid transitions between states
- `[StateMachine(stateType, transitionType)]` - Mark a class as a state machine
- `IStateMachine<TState, TTransition>` - Interface for state machine implementations

## Usage

Define your state and transition enums:

```csharp
public enum OrderState
{
    [State(isStart: true)]
    Pending,
    [State]
    Processing,
    [State]
    Completed
}

public enum OrderTransition
{
    [Transition(OrderState.Pending, OrderState.Processing)]
    Start,
    [Transition(OrderState.Processing, OrderState.Completed)]
    Complete
}
```

Then decorate your class:

```csharp
[StateMachine(typeof(OrderState), typeof(OrderTransition))]
public partial class Order
{
    // Convention methods are auto-discovered:
    // void OnStart() { }
    // bool CanComplete => true;
    // void OnEnterCompleted() { }
}
```

## Related Packages

- **LionFire.StateMachines** - Runtime state machine implementation
- **LionFire.StateMachines.Generation** - Roslyn source generator for auto-generating state machine code

## License

MIT
