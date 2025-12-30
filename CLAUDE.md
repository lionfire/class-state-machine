# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build all projects (use dotnet_win on Windows filesystem)
dotnet_win build C:\src\StateMachines\StateMachines.sln

# Build specific project
dotnet_win build C:\src\StateMachines\src\LionFire.StateMachines\LionFire.StateMachines.csproj

# Run tests
dotnet_win test C:\src\StateMachines\test\LionFire.StateMachines.Tests\LionFire.StateMachines.Tests.csproj

# Restore packages
dotnet_win restore C:\src\StateMachines\StateMachines.sln
```

## Architecture Overview

This is a convention-oriented state machine library for C# that uses Roslyn source generation to auto-generate state machine code at design-time.

### Project Structure

- **LionFire.StateMachines.Abstractions** - Core attributes and interfaces (`StateAttribute`, `TransitionAttribute`, `StateMachineAttribute`, `IStateMachine`)
- **LionFire.StateMachines** - Runtime state machine implementation (`StateMachine<TState, TTransition>`, `StateMachineState<TState, TTransition, TOwner>`)
- **LionFire.StateMachines.Generation** - Roslyn source generator (`StateMachineGenerator : ISourceGenerator`)

### How It Works

1. Define a **state enum** (e.g., `ExecutionState`) with your states
2. Define a **transition enum** with `[Transition(from, to)]` attributes on each field
3. Decorate your class with `[StateMachine(typeof(StateEnum), typeof(TransitionEnum))]`
4. Implement convention-based methods (the generator detects these):
   - `On{TransitionName}()` - called during transition
   - `Can{TransitionName}` property - returns bool for transition guards
   - `On{StateName}()` / `OnEnter{StateName}()` - entering state
   - `Leave{StateName}()` / `OnLeave{StateName}()` - leaving state
   - `After{StateName}()` - after entering state

The source generator creates:
- A `StateMachine` property returning `StateMachineState<TState, TTransition, TOwner>`
- Public methods for each transition (e.g., `Initialize()`, `Start()`)

### Key Classes

- `StateMachine<TState, TTransition>` - Static factory and transition info registry
- `StateMachineState<TState, TTransition, TOwner>` - Per-instance state machine state, calls convention methods via reflection
- `StateTransitionInfo<TState, TTransition>` - Defines From/To states for a transition

### Source Generator Integration

The generator is referenced as an analyzer in consuming projects:
```xml
<ProjectReference Include="...\LionFire.StateMachines.Generation.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

## Target Framework

All libraries target **netstandard2.0** for broad compatibility. Tests target **net6.0**.
