# Troubleshooting Guide

Common issues and solutions when using LionFire.StateMachines.

## Source Generator Not Running

### Symptoms
- No `StateMachine` property on your class
- No generated transition methods (e.g., `Initialize()`, `Start()`)
- Build succeeds but class is incomplete

### Solutions

1. **Check the `partial` keyword**
   ```csharp
   // Wrong - missing partial
   [StateMachine(typeof(MyState), typeof(MyTransition))]
   public class MyClass { }

   // Correct
   [StateMachine(typeof(MyState), typeof(MyTransition))]
   public partial class MyClass { }
   ```

2. **Verify package references**

   Your project needs all three packages:
   ```xml
   <PackageReference Include="LionFire.StateMachines.Abstractions" Version="8.0.0" />
   <PackageReference Include="LionFire.StateMachines" Version="8.0.0" />
   <PackageReference Include="LionFire.StateMachines.Generation" Version="8.0.0" />
   ```

3. **Check generator is loaded as analyzer**

   If using project reference instead of NuGet:
   ```xml
   <ProjectReference Include="...\LionFire.StateMachines.Generation.csproj"
                     OutputItemType="Analyzer"
                     ReferenceOutputAssembly="false" />
   ```

4. **Restart IDE**

   Sometimes source generators need an IDE restart to load properly. Try:
   - Close and reopen Visual Studio/VS Code/Rider
   - Run `dotnet build` from command line to verify

5. **Check build output**

   Look for generator warnings in build output:
   ```bash
   dotnet build -v detailed 2>&1 | grep -i "StateMachineGenerator"
   ```

## Convention Methods Not Being Called

### Symptoms
- Transition succeeds but your `On{Transition}()` method isn't called
- Guard property (`Can{Transition}`) is ignored

### Solutions

1. **Verify method naming**

   Method names must match exactly (case-sensitive):
   ```csharp
   // Wrong
   private void onInitialize() { }  // lowercase 'o'
   private void OnINITIALIZE() { }  // wrong case

   // Correct
   private void OnInitialize() { }
   ```

2. **Check method signature**

   Convention methods must have specific signatures:
   ```csharp
   // Transition handlers - void, no parameters
   private void OnStart() { }

   // Guard properties - bool getter
   public bool CanStart => true;

   // State handlers - void, no parameters
   private void OnEnterRunning() { }
   private void OnLeaveReady() { }
   ```

3. **Verify transition name matches enum**

   The method name suffix must match the enum field name exactly:
   ```csharp
   public enum MyTransition
   {
       [Transition(State.A, State.B)]
       StartProcess  // Method should be OnStartProcess, not OnStart
   }
   ```

## Transition Fails Silently

### Symptoms
- Calling a transition method doesn't change state
- No exception thrown

### Solutions

1. **Use TryTransition to check result**
   ```csharp
   if (!myObject.StateMachine.TryTransition(MyTransition.Start))
   {
       var reasons = myObject.StateMachine.CannotChangeStateReasons(MyTransition.Start);
       foreach (var reason in reasons)
       {
           Console.WriteLine($"Failed: {reason}");
       }
   }
   ```

2. **Check current state**
   ```csharp
   Console.WriteLine($"Current state: {myObject.StateMachine.CurrentState}");
   // Verify you're in the correct "from" state for the transition
   ```

3. **Verify guard conditions**
   ```csharp
   // If CanStart returns false, transition will fail
   public bool CanStart => someCondition;
   ```

## "Type not found" Errors

### Symptoms
- Build error: "The type or namespace 'StateMachineAttribute' could not be found"
- Missing using directives

### Solutions

1. **Add required using statements**
   ```csharp
   using LionFire.StateMachines;      // For [Transition] attribute
   using LionFire.StateMachines.Class; // For [StateMachine] attribute
   ```

2. **Check package is installed**
   ```bash
   dotnet list package | grep StateMachines
   ```

## Performance Issues

### Symptoms
- State transitions are slow
- High CPU usage during transitions

### Understanding
- LionFire.StateMachines uses reflection to discover and call convention methods
- This is slower than direct method calls but provides flexibility
- First transition per type has initialization overhead (binding discovery)

### Recommendations

1. **Reuse state machine instances**
   ```csharp
   // Good - reuse the object
   var order = new Order();
   order.Process();
   order.Ship();

   // Avoid - creating many short-lived instances
   for (int i = 0; i < 1000; i++)
   {
       new Order().Process();  // Binding rediscovered each time
   }
   ```

2. **For high-frequency transitions**

   If you need thousands of transitions per second, consider:
   - Using the static `StateMachine<TState, TTransition>` API directly
   - Using a different library (Stateless) for performance-critical paths

## Thread Safety Issues

### Symptoms
- Inconsistent state
- Race conditions during concurrent transitions

### Understanding
- State transitions are thread-safe (protected by lock)
- But reading `CurrentState` while another thread transitions may give stale value

### Recommendations

1. **Use atomic check-and-transition**
   ```csharp
   // Thread-safe - check and transition atomically
   if (myObject.StateMachine.TryTransition(MyTransition.Start))
   {
       // Transition succeeded
   }

   // NOT thread-safe - state could change between check and transition
   if (myObject.StateMachine.CurrentState == MyState.Ready)
   {
       myObject.Start();  // Another thread could have changed state!
   }
   ```

## Still Having Issues?

1. **Check the tests** - The test project shows working examples
2. **Enable detailed logging** - Build with `-v detailed`
3. **File an issue** - [GitHub Issues](https://github.com/lionfire/Core/issues)

## Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `InvalidFromStateException` | Transition requires different starting state | Check `CurrentState` before transitioning |
| `CannotChangeStateException` | Guard returned false or state mismatch | Check guard conditions and current state |
| `TransitionNotAllowedException` | No `[Transition]` attribute for this transition | Add `[Transition(from, to)]` to enum field |
| `StateChangeAbortedException` | StateChanging event handler aborted | Check event handlers for abort logic |
