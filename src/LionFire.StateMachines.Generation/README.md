# LionFire.StateMachines.Generation

Roslyn source generator that auto-generates state machine code from your class definitions.

## Overview

This package analyzes classes decorated with `[StateMachine]` and generates:

- A `StateMachine` property for state management
- Public methods for each transition (e.g., `Start()`, `Complete()`)
- Wiring to your convention methods

## Installation

Reference this package as an analyzer in your project:

```xml
<PackageReference Include="LionFire.StateMachines.Generation" Version="8.0.0"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

Or via project reference:

```xml
<ProjectReference Include="...\LionFire.StateMachines.Generation.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

## How It Works

1. Define state and transition enums with attributes
2. Mark your class with `[StateMachine]` and make it `partial`
3. The generator creates a partial class with:
   - State machine infrastructure
   - Transition methods that invoke your convention handlers

## Example

Your code:
```csharp
[StateMachine(typeof(OrderState), typeof(OrderTransition))]
public partial class Order
{
    private void OnStart() => Console.WriteLine("Starting");
    private bool CanComplete => true;
}
```

Generated code adds:
```csharp
public partial class Order
{
    public StateMachineState<OrderState, OrderTransition, Order> StateMachine { get; }

    public void Start() => StateMachine.Transition(OrderTransition.Start);
    public void Complete() => StateMachine.Transition(OrderTransition.Complete);
}
```

## Related Packages

- **LionFire.StateMachines.Abstractions** - Core attributes and interfaces
- **LionFire.StateMachines** - Runtime implementation

## License

MIT
