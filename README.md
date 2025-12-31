# LionFire.StateMachines

Convention-based state machines for C# using Roslyn source generation.

Define your state machine with attributes and convention methods - code is generated at compile-time with full IntelliSense support.

## Why LionFire.StateMachines?

- **Less Boilerplate**: Convention methods reduce code by 50-70% vs fluent APIs
- **Compile-Time Safety**: Generated code catches errors at design-time
- **Discoverable**: IDE autocomplete shows convention method names
- **Type-Safe**: Strongly-typed states and transitions via enums
- **Modern**: Uses IIncrementalGenerator for optimal IDE performance

## Quick Start

### 1. Install Packages

```bash
dotnet add package LionFire.StateMachines.Abstractions
dotnet add package LionFire.StateMachines
dotnet add package LionFire.StateMachines.Generation
```

### 2. Define State and Transition Enums

```csharp
using LionFire.StateMachines;

public enum OrderState
{
    Pending,
    Processing,
    Shipped,
    Delivered
}

public enum OrderTransition
{
    [Transition(OrderState.Pending, OrderState.Processing)]
    Process,

    [Transition(OrderState.Processing, OrderState.Shipped)]
    Ship,

    [Transition(OrderState.Shipped, OrderState.Delivered)]
    Deliver
}
```

### 3. Create Your State Machine Class

```csharp
using LionFire.StateMachines.Class;

[StateMachine(typeof(OrderState), typeof(OrderTransition))]
public partial class Order
{
    public string OrderId { get; set; }

    // Convention methods - called automatically during transitions
    private void OnProcess() => Console.WriteLine($"Processing order {OrderId}");
    private void OnShip() => Console.WriteLine($"Shipping order {OrderId}");
    private void OnDeliver() => Console.WriteLine($"Delivered order {OrderId}");

    // Guard conditions - return false to prevent transition
    public bool CanProcess => !string.IsNullOrEmpty(OrderId);
    public bool CanShip => true;
}
```

### 4. Use It

```csharp
var order = new Order { OrderId = "12345" };

Console.WriteLine(order.StateMachine.CurrentState); // Pending

order.Process();  // Generated method - calls OnProcess()
Console.WriteLine(order.StateMachine.CurrentState); // Processing

order.Ship();     // Generated method - calls OnShip()
Console.WriteLine(order.StateMachine.CurrentState); // Shipped

order.Deliver();  // Generated method - calls OnDeliver()
Console.WriteLine(order.StateMachine.CurrentState); // Delivered
```

## Convention Methods

The source generator discovers these methods on your class and wires them automatically:

| Pattern | When Called | Example |
|---------|-------------|---------|
| `On{Transition}()` | During transition | `OnProcess()` |
| `Can{Transition}` | Guard check before transition | `bool CanProcess => true` |
| `OnEnter{State}()` | After entering state | `OnEnterProcessing()` |
| `OnLeave{State}()` | Before leaving state | `OnLeavePending()` |
| `After{State}()` | After entering state (alternative) | `AfterShipped()` |

All convention methods are optional. Only implement the ones you need.

## Generated Code

The source generator creates a partial class with:

```csharp
public partial class Order
{
    private StateMachineState<OrderState, OrderTransition, Order>? _stateMachine;

    public StateMachineState<OrderState, OrderTransition, Order> StateMachine
    {
        get => _stateMachine ??= StateMachine<OrderState, OrderTransition>.Create(this);
    }

    // Generated transition methods
    public void Process() => StateMachine.Transition(OrderTransition.Process);
    public void Ship() => StateMachine.Transition(OrderTransition.Ship);
    public void Deliver() => StateMachine.Transition(OrderTransition.Deliver);
}
```

## Advanced Usage

### TryTransition Pattern

```csharp
if (order.StateMachine.TryTransition(OrderTransition.Ship))
{
    Console.WriteLine("Order shipped!");
}
else
{
    Console.WriteLine("Cannot ship order in current state");
}
```

### Check Why Transition Failed

```csharp
var reasons = order.StateMachine.CannotChangeStateReasons(OrderTransition.Ship);
foreach (var reason in reasons)
{
    Console.WriteLine($"Cannot transition: {reason}");
}
```

### Events

```csharp
order.StateMachine.StateChanging += (sender, e) =>
    Console.WriteLine($"Changing from {e.From} to {e.To}");

order.StateMachine.StateChanged += (sender, e) =>
    Console.WriteLine($"Changed to {e.To}");

order.StateMachine.StateChangeAborted += (sender, e) =>
    Console.WriteLine($"Transition aborted: {e.Reason}");
```

## Packages

| Package | Purpose |
|---------|---------|
| [LionFire.StateMachines.Abstractions](https://www.nuget.org/packages/LionFire.StateMachines.Abstractions/) | Attributes and interfaces |
| [LionFire.StateMachines](https://www.nuget.org/packages/LionFire.StateMachines/) | Runtime implementation |
| [LionFire.StateMachines.Generation](https://www.nuget.org/packages/LionFire.StateMachines.Generation/) | Source generator |

## Compatibility

- **Target Framework**: netstandard2.0 (broad compatibility)
- **Tested On**: .NET 8.0, .NET 9.0, .NET 10.0
- **IDE Support**: Visual Studio, VS Code, Rider (any IDE with Roslyn support)

## Comparison to Stateless

| Feature | LionFire.StateMachines | Stateless |
|---------|------------------------|-----------|
| Configuration | Conventions + Attributes | Fluent API |
| Code Generation | Yes (compile-time) | No |
| Boilerplate | Minimal | More verbose |
| Learning Curve | Lower | Higher |
| Async Support | Not yet | Yes |
| Hierarchical States | Not yet | Yes |

Choose LionFire.StateMachines for simpler state machines where convention-based configuration reduces boilerplate. Choose Stateless for complex scenarios requiring async transitions or hierarchical states.

## Current Limitations

- No async/await support (planned)
- No hierarchical/nested states
- No state persistence (planned)
- No visual diagram generation (planned)

## Documentation

- [Architecture Overview](docs/analysis/architecture.md)
- [Development Roadmap](docs/roadmap.md)
- [Troubleshooting](docs/troubleshooting.md)

## Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT License - see [LICENSE](LICENSE) file.

## Author

Jared Thirsk - [LionFire](https://github.com/lionfire)
