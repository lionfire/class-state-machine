# Architecture Review

## Structure Overview

### Project Structure

```
StateMachines/
├── src/
│   ├── LionFire.StateMachines.Abstractions/     # Attributes, interfaces, exceptions
│   ├── LionFire.StateMachines/                  # Runtime implementation
│   └── LionFire.StateMachines.Generation/       # Roslyn source generator
├── test/
│   ├── LionFire.StateMachines.Tests/            # Unit tests
│   ├── LionFire.StateMachines.Tests.Model/      # Test model classes
│   └── LionFire.StateMachines.Tests.ExternalModel/  # Separate enum definitions
└── docs/                                         # Documentation (minimal)
```

**Assessment**: Clean three-layer architecture with proper separation of concerns. Test organization is logical with model separation.

### Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    User Application                          │
│  [StateMachine(typeof(State), typeof(Transition))]          │
│  public partial class MyClass {                             │
│    public void OnReady() { ... }                            │
│    public bool CanInitialize => true;                       │
│  }                                                           │
└───────────────┬─────────────────────────────────────────────┘
                │
                │ (Design-time)
                ▼
┌─────────────────────────────────────────────────────────────┐
│        LionFire.StateMachines.Generation                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ StateMachineGenerator : ISourceGenerator              │  │
│  │  - Execute()                                          │  │
│  │  - Discovers [StateMachine] attributes                │  │
│  │  - Scans for convention methods                       │  │
│  │  - Generates partial class with:                      │  │
│  │    * StateMachine property                            │  │
│  │    * Transition methods (Initialize(), Start(), etc.) │  │
│  └───────────────────────────────────────────────────────┘  │
└───────────────┬─────────────────────────────────────────────┘
                │ generates
                │
                ▼ (Compile-time artifact)
┌─────────────────────────────────────────────────────────────┐
│                 Generated Partial Class                      │
│  public partial class MyClass {                             │
│    public void Initialize() =>                              │
│      StateMachine.Transition(Transition.Initialize);        │
│    public StateMachineState<State, Transition, MyClass>     │
│      StateMachine { get { ... } }                           │
│  }                                                           │
└───────────────┬─────────────────────────────────────────────┘
                │
                │ (Runtime)
                ▼
┌─────────────────────────────────────────────────────────────┐
│         LionFire.StateMachines (Runtime)                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ StateMachineState<TState, TTransition, TOwner>        │  │
│  │  - CurrentState                                        │  │
│  │  - Transition(transition)                             │  │
│  │  - TryTransition(transition)                          │  │
│  │  - Events: StateChanging, StateChanged, etc.          │  │
│  └───────────────┬───────────────────────────────────────┘  │
│                  │                                           │
│                  │ uses                                      │
│                  ▼                                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ BindingProvider<TState, TTransition, TOwner>          │  │
│  │  - GetStateBinding()                                  │  │
│  │  - GetTransitionBinding()                             │  │
│  │  - Uses reflection to discover:                       │  │
│  │    * On{TransitionName}()                             │  │
│  │    * Can{TransitionName}                              │  │
│  │    * On{StateName}(), After{StateName}()              │  │
│  │    * CanLeave{StateName}, CanEnter{StateName}         │  │
│  └───────────────────────────────────────────────────────┘  │
│                  │                                           │
│                  │ queries                                   │
│                  ▼                                           │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ StateMachine<TState, TTransition>                     │  │
│  │  - Static registry of transition info                 │  │
│  │  - Reads [Transition(from, to)] attributes            │  │
│  │  - StartingState, EndingState                         │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  │ references
                  ▼
┌─────────────────────────────────────────────────────────────┐
│      LionFire.StateMachines.Abstractions                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ [StateMachine], [Transition], [State]                │  │
│  │ IStateMachine<TState, TTransition>                    │  │
│  │ StateMachineException hierarchy                       │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Architectural Analysis

### 1. Layered Architecture Assessment

#### Abstractions Layer
**Purpose**: Define contracts, attributes, and exceptions

**Files**:
- `StateMachineAttribute.cs` - Marks classes for code generation
- `TransitionAttribute.cs` - Defines state transitions on enum fields
- `StateAttribute.cs` - Metadata for states
- Exception types (6 files)
- `IStateMachine<TState, TTransition>` interface

**Evaluation**:
- **Strengths**: Minimal dependencies, clear contracts, reusable across implementations
- **Weaknesses**: `StateMachineAttribute` combines concerns (both metadata AND code generation trigger). Should be split per TODO comment on line 7-8.

#### Runtime Layer
**Purpose**: State machine execution engine

**Key Classes**:
- `StateMachine<TState, TTransition>` - Static registry, factory
- `StateMachineState<TState, TTransition, TOwner>` - Per-instance state and transition logic
- `BindingProvider<TState, TTransition, TOwner>` - Convention discovery via reflection
- Binding classes (`StateBinding`, `TransitionBinding`) - Hold discovered methods

**Evaluation**:
- **Strengths**:
  - Separation of static metadata (StateMachine) from instance state (StateMachineState)
  - Thread-safe transition logic with proper locking
  - Flexible binding system supporting multiple convention prefixes

- **Weaknesses**:
  - Heavy reflection usage impacts performance (MethodInfo.Invoke per transition)
  - No caching of compiled delegates
  - `StateMachineState.currentState` field exists but is unused (line 54) - dead code

#### Generation Layer
**Purpose**: Compile-time code generation

**Key Classes**:
- `StateMachineGenerator : ISourceGenerator`
- `MySyntaxReceiver : ISyntaxReceiver`

**Evaluation**:
- **Strengths**: Uses modern Roslyn ISourceGenerator (incremental compilation compatible)
- **Weaknesses**:
  - God class anti-pattern (930 lines, 10+ responsibilities)
  - No diagnostic reporting to IDE
  - Extensive debug/logging code mixed with production logic
  - Preprocessor directives make code hard to follow

### 2. Separation of Concerns

#### Design-Time vs Runtime Separation
**Score**: 7/10

**Good**:
- Clean boundary between code generation (design-time) and execution (runtime)
- Generator references runtime types only for reading attributes, not executing them
- Generated code is partial classes, allowing manual code alongside generated code

**Issues**:
- Generator instantiates and configures `StateMachineAttribute` at compile time (lines 419-440), blurring the line between metadata and execution
- Generator calls `StateMachine.GetTransitionInfo()` which is a runtime API (line 524)

#### State vs Transition Logic Separation
**Score**: 8/10

**Good**:
- State logic (`StateBinding`) separate from transition logic (`TransitionBinding`)
- Clear ownership: states own enter/leave, transitions own the action
- Events cleanly separated (StateChanging, StateChanged, StateChangeAborted)

**Issues**:
- State change validation logic spread across three places: guards, binding checks, and transition execution

### 3. Dependency Analysis

#### Dependency Graph

```
Abstractions (no dependencies)
    ↑
    │
    ├── Runtime (depends on Abstractions)
    │       ↑
    │       │
    │       └── Generation (depends on both)
    │               ↑
    │               │
    └───────────────┘
```

#### External Dependencies

**Abstractions**: None (pure .NET)

**Runtime**:
- `System.Reflection.TypeExtensions` 4.7.0

**Generation**:
- `Microsoft.CodeAnalysis.CSharp` 4.4.0 (OUTDATED - current is 4.12+)
- `Microsoft.CodeAnalysis.Analyzers` 3.3.3 (OUTDATED - current is 3.11+)
- `Microsoft.CodeAnalysis.CSharp.Workspaces` 4.4.0 (OUTDATED)
- `Validation` 2.5.51

**Issues**:
1. CodeAnalysis packages are 2+ years old (from late 2022)
2. Using obsolete patterns (e.g., ISourceGenerator without incremental generation)
3. Validation package is not widely used - could be replaced with standard checks

#### Circular Dependencies
**Found**: None. Dependency direction is clean.

#### Tight Coupling Issues

1. **Generated Code → Runtime API**: Generated code calls `StateMachine<TState, TTransition>.Transition()` which is tightly coupled to implementation. However, this is acceptable for generated code.

2. **BindingProvider → Reflection**: Tight coupling to reflection APIs makes it hard to optimize or replace. Should introduce abstraction layer.

3. **Generator → Runtime Types**: Generator instantiates `StateMachineAttribute` (line 420-439) creating compile-time dependency on runtime behavior.

### 4. Extensibility Assessment

#### Extension Points Identified

1. **Convention Configuration** (`StateMachineConventions`)
   ```csharp
   public static StateMachineConventions Conventions
   {
       get => conventions ?? StateMachineConventions.DefaultConventions;
       set => conventions = value;
   }
   ```
   **Assessment**: Good. Allows customizing method name prefixes.

2. **Binding Flags Configuration**
   ```csharp
   public static BindingFlags MethodBindingFlags { get; set; } = ...;
   public static BindingFlags PropertyBindingFlags { get; set; } = ...;
   ```
   **Assessment**: Allows controlling what methods are discoverable (public/private/etc.)

3. **Events**
   ```csharp
   public event StateChangeEventHandler<TState, TTransition> StateChanging;
   public event StateChangeEventHandler<TState, TTransition> StateChanged;
   public event StateChangeEventHandler<TState, TTransition> StateChangeAborted;
   ```
   **Assessment**: Good observability hooks. Missing: global/static hooks for cross-cutting concerns.

#### Missing Extension Points

1. **Custom Code Generation**: No plugin system for customizing generated code
2. **Transition Pipeline**: No middleware/interceptor pattern for cross-cutting concerns (logging, metrics, authorization)
3. **State Persistence**: No hooks for saving/loading state machine state
4. **Async Extensions**: `AsyncStateMachineState.cs` exists but is incomplete/unused
5. **Custom Binding Resolution**: `BindingProvider` is not easily replaceable

#### Proposed Extension Architecture

```csharp
// Allow custom transition pipeline
public interface ITransitionInterceptor<TState, TTransition>
{
    Task<bool> BeforeTransition(StateChange<TState, TTransition> change);
    Task AfterTransition(StateChange<TState, TTransition> change);
}

// Allow custom state persistence
public interface IStatePersistence<TState>
{
    Task SaveState(TState state);
    Task<TState> LoadState();
}
```

### 5. API Design Review

#### Public API Surface

**Core API**:
```csharp
// User-facing (generated)
void Initialize();  // Generated transition methods
void Start();

// State machine instance
public StateMachineState<TState, TTransition, TOwner> StateMachine { get; }

// State machine operations
public void Transition(TTransition transition, object context = null)
public bool TryTransition(TTransition transition, object context = null)
public bool CanChangeState(TTransition transition, object context = null)
public IEnumerable<object> CannotChangeStateReasons(TTransition transition, object context = null)

// State access
public TState CurrentState { get; }
public IStateChange<TState, TTransition> LastStateChange { get; }
```

**API Quality Assessment**:

**Strengths**:
1. **Clear Naming**: Methods are self-explanatory (`TryTransition`, `CanChangeState`)
2. **Try Pattern**: `TryTransition` returns bool instead of throwing
3. **Diagnostic API**: `CannotChangeStateReasons` allows querying why transition would fail
4. **Immutable State Access**: `CurrentState` is read-only, mutations only through `Transition()`

**Weaknesses**:
1. **Inconsistent Context Parameter**:
   - `context` parameter is `object` type (line 101, 86, 81, 68)
   - No generic constraint or strongly-typed alternative
   - Documentation doesn't explain what context should contain

2. **No Async Support**: All methods are synchronous
   - Modern state machines often need async transitions (API calls, DB operations)
   - `AsyncStateMachineState.cs` exists but appears unused

3. **Limited State Query API**:
   - Can only check current state
   - No `IsInState(TState)` helper
   - No `GetAllowedTransitions()` method

4. **Event API Limitations**:
   - Events don't provide deferral mechanism
   - Can't asynchronously validate transitions via events
   - No priority ordering for event handlers

#### Consistency Issues

1. **Property vs Method Naming**:
   ```csharp
   public TState CurrentState { get; }  // Property
   public IStateChange<TState, TTransition> LastStateChange => lastStateChange;  // Property

   // But convention methods are:
   bool CanInitialize { get; }  // Property (detected by reflection)
   void OnReady() // Method
   ```
   Mixing properties and methods for similar concepts.

2. **Null vs Empty**:
   - `CannotChangeStateReasons` returns empty enumerable for success (line 160)
   - But some methods return null for "not found" (line 161 in StateMachine.cs)

### 6. Scalability Considerations

#### Performance Characteristics

**Bottlenecks Identified**:

1. **Reflection on Every Transition** (Critical)
   ```csharp
   // BindingProvider.cs line 200
   return (o, sc) => mi.Invoke(o, Array.Empty<object>());
   ```
   **Impact**: 10-100x slower than direct method calls
   **Mitigation**: Use compiled expressions or source generation for bindings

2. **Lock Contention** (Medium)
   ```csharp
   // StateMachineState.cs line 89, 104, 123
   lock (lockObject) { ... }
   ```
   **Impact**: All transitions serialized per instance. High-frequency state changes will bottleneck.
   **Mitigation**: Consider lock-free designs or finer-grained locking

3. **Enum String Conversion** (Low)
   ```csharp
   // Line 59 in BindingProvider.cs
   var fi = typeof(TState).GetField(state.ToString());
   ```
   **Impact**: ToString() + GetField() on every binding lookup
   **Mitigation**: Cache field lookups in dictionary

#### Resource Management

**Memory**:
- **Good**: Bindings cached per type (singleton BindingProvider per generic instantiation)
- **Issue**: No cache eviction strategy - cached bindings never released
- **Issue**: Generator creates lists that accumulate (line 215, 359-360)

**Threads**:
- **Good**: No thread creation, uses caller's thread
- **Issue**: No async support means blocking on I/O-bound operations

**Handles/Resources**:
- **Good**: No unmanaged resources
- **Issue**: Event handlers could leak if not unsubscribed

#### Concurrency Patterns

**Current Implementation**:
```csharp
private object lockObject = new object();

public void Transition(TTransition transition, object context = null)
{
    lock (lockObject)  // Coarse-grained locking
    {
        // All validation and execution serialized
    }
}
```

**Assessment**: Safe but not scalable for high-concurrency scenarios.

**Recommendations**:
1. Use `SemaphoreSlim` for async-compatible locking
2. Consider optimistic concurrency (compare-and-swap on state)
3. Allow read-only operations (like `CurrentState`) without locking
4. Implement transition queuing for async scenarios

### 7. Caching Strategy

**Current Caching**:

1. **Binding Cache** (Good)
   ```csharp
   private Dictionary<TTransition, TransitionBinding<...>> transitions = new ...();
   private Dictionary<TState, StateBinding<...>> states = new ...();
   ```
   - Lazy initialized on first access
   - Never evicted
   - One cache per `BindingProvider<TState, TTransition, TOwner>` instance

2. **Transition Info Cache** (Good)
   ```csharp
   static Dictionary<TTransition, StateTransitionInfo<TState, TTransition>> transitions = ...
   ```
   - Populated in static constructor
   - Read-only after initialization
   - Shared across all instances of same type

3. **Intermediate Cache** (Questionable)
   ```csharp
   Dictionary<string, MethodInfo> methods;
   Dictionary<string, PropertyInfo> properties;

   public void ClearIntermediateCache()
   {
       methods = null;
       properties = null;
   }
   ```
   - Cleared frequently (line 132, suspicious comment: `methods = null;` *inside* method that populates it!)
   - Purpose unclear - appears to be a bug

**Cache Effectiveness Issues**:
- No metrics or telemetry to measure cache hit rate
- No lazy compilation of method invocations (still using `MethodInfo.Invoke`)
- Generator doesn't cache parsed syntax between incremental builds

## Architecture Strengths

1. **Clean Layering**: Abstractions → Runtime → Generation is textbook architecture
2. **Convention over Configuration**: Reduces boilerplate dramatically compared to competitors
3. **Compile-Time Validation**: Source generator provides early error detection
4. **Type Safety**: Generics ensure state and transition enums are strongly typed
5. **Testability**: Core logic is separated from code generation, enabling unit testing

## Architecture Weaknesses

1. **No Async Story**: Critical gap for modern applications
2. **Reflection Performance**: Acceptable for low-frequency transitions, problematic for high-throughput
3. **Limited Extensibility**: Hard to add cross-cutting concerns (logging, metrics, etc.)
4. **Generator Complexity**: God class makes maintenance difficult
5. **Missing Observability**: No built-in logging, metrics, or diagnostics

## Recommended Architectural Improvements

### Priority 1: Async Support
**Effort**: 3 weeks
```csharp
public interface IAsyncStateMachine<TState, TTransition>
{
    Task TransitionAsync(TTransition transition, CancellationToken ct = default);
    ValueTask<bool> TryTransitionAsync(TTransition transition, CancellationToken ct = default);
}

// Generated:
public async Task InitializeAsync() =>
    await StateMachine.TransitionAsync(Transition.Initialize);
```

### Priority 2: Refactor Generator
**Effort**: 1 week
```
StateMachineGenerator
├── SyntaxAnalyzer (finds [StateMachine] classes)
├── SemanticAnalyzer (resolves types, reads attributes)
├── ConventionScanner (discovers convention methods)
├── CodeEmitter (generates partial class code)
└── DiagnosticReporter (emits errors/warnings to IDE)
```

### Priority 3: Optimize Binding Performance
**Effort**: 1 week

Replace reflection invocation with compiled expressions:
```csharp
// Instead of:
return (o, sc) => mi.Invoke(o, Array.Empty<object>());

// Use:
var ownerParam = Expression.Parameter(typeof(TOwner), "owner");
var scParam = Expression.Parameter(typeof(IStateChange<TState, TTransition>), "sc");
var call = Expression.Call(ownerParam, mi);
var lambda = Expression.Lambda<Action<TOwner, IStateChange<TState, TTransition>>>(call, ownerParam, scParam);
return lambda.Compile();
```

### Priority 4: Add Interceptor Pipeline
**Effort**: 1 week
```csharp
public class StateMachineInterceptorPipeline<TState, TTransition>
{
    public void Use(ITransitionInterceptor<TState, TTransition> interceptor);
    internal async Task<bool> ExecutePipeline(StateChange<TState, TTransition> change);
}
```

## Overall Architecture Score: 7/10

### Breakdown
- **Structure**: 9/10 (Excellent layering and separation)
- **Dependency Management**: 8/10 (Clean, but outdated packages)
- **Extensibility**: 5/10 (Limited extension points)
- **API Design**: 7/10 (Clear but inconsistent, no async)
- **Scalability**: 5/10 (Reflection bottleneck, coarse locking)
- **Maintainability**: 6/10 (Generator is complex, runtime is clean)

The architecture is fundamentally sound with excellent separation of concerns and innovative use of conventions. The main gaps are lack of async support, reflection performance, and extensibility mechanisms. Addressing these would elevate this to a production-ready, competitive state machine library.
