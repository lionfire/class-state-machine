# LionFire.StateMachines - Development Roadmap

**Last Updated**: 2025-12-30
**Version**: 8.0.0-preview (IIncrementalGenerator refactor complete)

This roadmap outlines prioritized improvements to elevate LionFire.StateMachines from an experimental library to a production-ready, competitive state machine framework for C#.

## Executive Summary

The project requires approximately 12-16 weeks of focused development to reach production readiness. Critical priorities are documentation, dependency updates, and async support. The unique convention-based approach provides strong differentiation from competitors like Stateless, but only if execution quality matches the innovative design.

## Roadmap Phases

### Phase 1: Foundation & Stability (Weeks 1-4)
**Goal**: Make the library usable and trustworthy

### Phase 2: Feature Parity (Weeks 5-9)
**Goal**: Match competitor features for common use cases

### Phase 3: Differentiation & Polish (Weeks 10-12)
**Goal**: Leverage unique strengths, build community

### Phase 4: Growth & Ecosystem (Weeks 13-16+)
**Goal**: Expand adoption, integrations, tooling

---

## Phase 1: Foundation & Stability

### 1.1 Documentation Overhaul ✅ MOSTLY COMPLETED
**Effort**: 1.5 weeks | **Impact**: Critical

#### Tasks:
- [x] **Rewrite README.md** (4 hours) ✅
  - Added clear value proposition vs Stateless
  - Included quick start with working example
  - Fixed package name inconsistencies
  - Added NuGet badge links
  - Removed Travis CI references
  - Added "Limitations" section

- [x] **Create /examples/ directory** (8 hours) ✅
  - `BasicOrder/` - Simple order processing state machine
  - `AdvancedWorkflow/` - Guards, Before/After hooks, events
  - Each with README explaining patterns

- [ ] **Add XML documentation to all public APIs** (16 hours)
  - StateMachine<TState, TTransition>
  - StateMachineState<TState, TTransition, TOwner>
  - BindingProvider and configuration classes
  - All attributes (StateMachineAttribute, TransitionAttribute, StateAttribute)
  - All exception types with usage examples

- [x] **Create architecture documentation** (8 hours) ✅
  - Component diagram (projects and dependencies)
  - Explanation of source generation process
  - File: `docs/architecture.md`

- [x] **Create troubleshooting guide** (4 hours) ✅
  - Source generator not running
  - Convention methods not discovered
  - Performance issues
  - Common mistakes
  - File: `docs/troubleshooting.md`

- [x] **Create CONTRIBUTING.md** (2 hours) ✅
  - How to build locally
  - How to run tests
  - Code style guidelines
  - PR process

**Deliverables**:
- ✅ Comprehensive README
- ✅ 2 working examples (BasicOrder, AdvancedWorkflow)
- XML documentation still needed
- ✅ Architecture guide
- ✅ Troubleshooting guide
- ✅ Contributing guide

**Success Metrics**:
- ✅ New user can get started in <10 minutes
- ✅ Zero unanswered questions in README
- API docs in IntelliSense still needed

---

### 1.2 Dependency Updates & Compatibility ✅ COMPLETED
**Effort**: 3 days | **Impact**: Critical

#### Tasks:
- [x] **Update Roslyn packages** (4 hours) ✅
  - Upgraded Microsoft.CodeAnalysis.CSharp to 4.14.0
  - Upgraded Microsoft.CodeAnalysis.Analyzers to 3.11.0
  - Removed Microsoft.CodeAnalysis.CSharp.Workspaces (not needed for incremental generator)

- [x] **Resolve netstandard2.0 issues** (8 hours) ✅
  - Libraries target netstandard2.0 for compatibility
  - Tests run on net8.0, net9.0, net10.0
  - Generator is self-contained (no runtime assembly loading issues)

- [x] **Update all other dependencies** (2 hours) ✅
  - Removed Validation package (no longer needed)
  - Generator has minimal dependencies

- [ ] **Test compatibility** (4 hours)
  - Tests pass on .NET 8, 9, 10
  - Need to test on .NET Framework 4.8
  - Need to create compatibility matrix in README

**Deliverables**:
- ✅ All Roslyn dependencies current (4.14.0)
- ✅ netstandard2.0 compatibility maintained
- Compatibility matrix still needed

**Success Metrics**:
- ✅ No security warnings from outdated packages
- ✅ Library works on .NET 8+ without issues

---

### 1.3 Code Quality Improvements ✅ MOSTLY COMPLETED
**Effort**: 1 week | **Impact**: High

#### Tasks:
- [x] **Remove commented-out code** (4 hours) ✅
  - Deleted Execution-TEMPTEST folder
  - Removed StateMachineOptions.cs (unused)
  - Removed SyntaxNodeHelper.cs (unused)
  - Preprocessor directives eliminated in new generator

- [x] **Refactor StateMachineGenerator** (12 hours) ✅ COMPLETE REWRITE
  - Converted from ISourceGenerator to **IIncrementalGenerator**
  - Reduced from ~930 lines to ~260 lines (70% reduction!)
  - Self-contained: no project references needed at runtime
  - Uses ForAttributeWithMetadataName for efficient attribute detection
  - Clean separation: StateMachineInfo struct for data, pure functions for logic
  - EnforceExtendedAnalyzerRules enabled (was disabled before)

- [x] **Add Roslyn diagnostics** (8 hours) ✅
  - LFSM001: Class must be partial
  - LFSM002: State type must be an enum
  - LFSM003: Transition type must be an enum
  - LFSM004: No transitions generated
  - LFSM005: State enum is empty
  - LFSM006: Transition enum is empty
  - Implemented in Diagnostics.cs

- [ ] **Add .editorconfig** (1 hour)
  - Define code style rules
  - Enable analyzers (CA* rules)
  - Configure severity levels

- [ ] **Fix null handling inconsistencies** (4 hours)
  - Either validate all method parameters or document null behavior
  - Consider enabling nullable reference types

**Deliverables**:
- ✅ Clean, maintainable codebase
- ✅ Refactored generator (single clean class, not god class)
- ✅ Roslyn diagnostics (LFSM001-LFSM006)
- EditorConfig still needed

**Success Metrics**:
- ✅ Zero compiler warnings (build passes clean)
- ✅ Generator complexity reduced by 70%
- ✅ User error diagnostics with clear messages

---

### 1.4 Expand Test Coverage (MEDIUM PRIORITY)
**Effort**: 1 week | **Impact**: High

#### Tasks:
- [ ] **Add edge case tests** (8 hours)
  - Null owner
  - Concurrent transitions
  - Invalid enum values
  - Missing TransitionAttribute
  - Transition to same state
  - Cyclic state graphs

- [ ] **Add error path tests** (8 hours)
  - StateChangeAborted event
  - Exception from convention methods
  - Malformed enums
  - Reflection failures

- [ ] **Add generator tests** (12 hours)
  - Unit tests for SyntaxAnalyzer
  - Unit tests for SemanticAnalyzer
  - Unit tests for ConventionScanner
  - Unit tests for CodeEmitter
  - Integration tests for full generation

- [ ] **Add performance benchmarks** (4 hours)
  - Measure transition overhead
  - Compare to Stateless
  - Measure generator performance
  - Add BenchmarkDotNet project

- [ ] **Achieve 80% code coverage** (8 hours)
  - Measure current coverage
  - Identify gaps
  - Write tests to fill gaps

**Deliverables**:
- Comprehensive test suite
- Performance benchmarks published
- 80%+ code coverage

**Success Metrics**:
- All edge cases covered
- Benchmarks show competitive performance
- Code coverage >80%

---

## Phase 2: Feature Parity

### 2.1 Async/Await Support (HIGH PRIORITY)
**Effort**: 3 weeks | **Impact**: Critical

#### Tasks:
- [ ] **Design async API** (8 hours)
  ```csharp
  public interface IAsyncStateMachine<TState, TTransition>
  {
      Task TransitionAsync(TTransition transition, CancellationToken ct = default);
      ValueTask<bool> TryTransitionAsync(TTransition transition, CancellationToken ct = default);
  }
  ```
  - Document async vs sync trade-offs
  - Design async convention methods (OnInitializeAsync)

- [ ] **Implement AsyncStateMachineState** (16 hours)
  - Complete existing AsyncStateMachineState.cs
  - Support async guards (Task<bool> CanInitializeAsync)
  - Support async transitions (Task OnInitializeAsync)
  - Support async state handlers (Task OnEnterReadyAsync)
  - Use SemaphoreSlim for async-compatible locking

- [ ] **Update source generator** (12 hours)
  - Detect async convention methods
  - Generate async transition methods
  - Generate async/sync hybrid support

- [ ] **Add cancellation support** (8 hours)
  - CancellationToken parameter on async methods
  - Propagate cancellation through transition pipeline
  - Add tests for cancellation scenarios

- [ ] **Document async patterns** (4 hours)
  - When to use async vs sync
  - Async convention method examples
  - Cancellation best practices
  - Performance implications

**Deliverables**:
- Full async/await support
- Async examples
- Documentation

**Success Metrics**:
- Can call APIs in state transitions
- Competitive with Automatonymous async support

---

### 2.2 Visual Diagram Generation (MEDIUM PRIORITY)
**Effort**: 1 week | **Impact**: Medium

#### Tasks:
- [ ] **Design export API** (4 hours)
  ```csharp
  public static class StateMachineDiagram
  {
      public static string ToMermaid<TState, TTransition>();
      public static string ToDot<TState, TTransition>();
  }
  ```

- [ ] **Implement Mermaid export** (8 hours)
  - Read TransitionAttribute data
  - Generate Mermaid state diagram syntax
  - Include guard conditions as notes

- [ ] **Implement DOT export** (8 hours)
  - Generate GraphViz DOT format
  - Compatible with Stateless diagrams
  - Support styling options

- [ ] **Add Roslyn analyzer for diagrams** (8 hours)
  - Analyzer generates .mmd file alongside source
  - Updates when state machine changes
  - Configurable via MSBuild properties

- [ ] **Document diagram generation** (2 hours)
  - How to generate diagrams
  - How to render (VS Code extensions, online tools)
  - Examples of generated diagrams

**Deliverables**:
- Mermaid and DOT export
- Auto-generated diagrams
- Documentation with examples

**Success Metrics**:
- Visual diagram generated automatically
- Diagrams viewable in VS Code/GitHub

---

### 2.3 Performance Optimization (MEDIUM PRIORITY)
**Effort**: 1 week | **Impact**: Medium

#### Tasks:
- [ ] **Replace MethodInfo.Invoke with compiled expressions** (16 hours)
  ```csharp
  // Current (slow):
  return (o, sc) => mi.Invoke(o, Array.Empty<object>());

  // Optimized (fast):
  var ownerParam = Expression.Parameter(typeof(TOwner));
  var call = Expression.Call(ownerParam, mi);
  var lambda = Expression.Lambda<Action<TOwner>>(call, ownerParam);
  return lambda.Compile();
  ```
  - Cache compiled delegates
  - Measure performance improvement (expect 10-100x)

- [ ] **Optimize binding lookup** (4 hours)
  - Cache GetField() calls for enums
  - Eliminate redundant ToString() calls
  - Benchmark improvements

- [ ] **Add performance tests** (4 hours)
  - Benchmark transition throughput
  - Benchmark concurrent access
  - Compare to Stateless, LiquidState
  - Publish results in docs/performance.md

- [ ] **Optimize generator** (4 hours)
  - Reduce allocations
  - Cache semantic model queries
  - Implement incremental generation properly

**Deliverables**:
- 10-100x faster transitions (via compiled expressions)
- Published benchmarks
- Optimized generator

**Success Metrics**:
- Transition overhead <100ns
- Competitive with LiquidState

---

## Phase 3: Differentiation & Polish

### 3.1 Enhanced Developer Experience (HIGH PRIORITY)
**Effort**: 2 weeks | **Impact**: High

#### Tasks:
- [ ] **Improve error messages** (8 hours)
  - Add error codes to all exceptions
  - Include suggested remediations
  - Add context (current state, attempted transition)

- [ ] **Add IntelliSense hints** (8 hours)
  - Use Roslyn completion providers to suggest convention method names
  - Show available transitions for current state
  - Warn about unused convention methods

- [ ] **Create Visual Studio extension** (16 hours)
  - State machine visualizer tool window
  - Live view of current state (during debugging)
  - Navigation to convention methods

- [ ] **Add code snippets** (4 hours)
  - Snippet for state enum
  - Snippet for transition enum
  - Snippet for StateMachine class
  - Install via NuGet or VS extension

- [ ] **Create project templates** (8 hours)
  - "State Machine Library" project template
  - Includes pre-configured structure
  - Example state machine included

**Deliverables**:
- Better error messages
- IntelliSense improvements
- VS extension (optional but valuable)
- Code snippets and templates

**Success Metrics**:
- Developer can create state machine without leaving IDE
- Errors are actionable

---

### 3.2 State Persistence (MEDIUM PRIORITY)
**Effort**: 1.5 weeks | **Impact**: Medium

#### Tasks:
- [ ] **Design persistence API** (8 hours)
  ```csharp
  public interface IStatePersistence<TState>
  {
      Task SaveStateAsync(TState state);
      Task<TState> LoadStateAsync();
  }
  ```

- [ ] **Implement JSON persistence** (8 hours)
  - Serialize state to JSON
  - Deserialize and restore
  - Handle version mismatches

- [ ] **Implement Entity Framework persistence** (12 hours)
  - Create separate NuGet package: LionFire.StateMachines.EntityFramework
  - Store state in DB column
  - Handle concurrency conflicts

- [ ] **Add persistence examples** (4 hours)
  - Example with JSON files
  - Example with EF Core
  - Example with custom persistence

**Deliverables**:
- Persistence abstraction
- JSON and EF implementations
- Examples and docs

**Success Metrics**:
- Can save/restore state across app restarts
- Competitive with Automatonymous persistence

---

### 3.3 Observability & Diagnostics (MEDIUM PRIORITY)
**Effort**: 1 week | **Impact**: Medium

#### Tasks:
- [ ] **Add structured logging** (8 hours)
  - ILogger integration
  - Log state transitions with metadata
  - Configurable log levels

- [ ] **Add metrics/telemetry** (8 hours)
  - Count transitions
  - Measure transition duration
  - Track guard failures
  - Integration with System.Diagnostics.Metrics

- [ ] **Add runtime diagnostics API** (8 hours)
  ```csharp
  public class StateMachineDiagnostics<TState, TTransition>
  {
      public IReadOnlyList<TTransition> GetAllowedTransitions(TState currentState);
      public IReadOnlyDictionary<TTransition, StateTransitionInfo<TState, TTransition>> AllTransitions;
      public string GenerateDiagram();
  }
  ```

- [ ] **Document observability features** (4 hours)
  - How to enable logging
  - How to collect metrics
  - How to troubleshoot in production

**Deliverables**:
- Logging integration
- Metrics support
- Diagnostics API
- Documentation

**Success Metrics**:
- Can troubleshoot production issues via logs
- Can monitor state machine health

---

## Phase 4: Growth & Ecosystem

### 4.1 Framework Integrations (MEDIUM PRIORITY)
**Effort**: 2 weeks | **Impact**: Medium

#### Tasks:
- [ ] **ASP.NET Core integration** (8 hours)
  - Middleware for state validation
  - Model binding for state/transition
  - Example API with state machine
  - Package: LionFire.StateMachines.AspNetCore

- [ ] **Entity Framework integration** (already covered in 3.2)

- [ ] **MediatR integration** (8 hours)
  - Publish domain events on state change
  - Example with CQRS pattern
  - Package: LionFire.StateMachines.MediatR

- [ ] **SignalR integration** (8 hours)
  - Notify clients of state changes
  - Example real-time dashboard
  - Package: LionFire.StateMachines.SignalR

- [ ] **Document integration patterns** (8 hours)
  - Integration guides for each framework
  - Sample projects in examples/

**Deliverables**:
- 4 integration packages
- Integration examples
- Documentation

**Success Metrics**:
- Works seamlessly with popular frameworks
- Developers find integration examples

---

### 4.2 Community & Marketing (HIGH PRIORITY)
**Effort**: Ongoing | **Impact**: High

#### Tasks:
- [ ] **Create comparison page vs Stateless** (4 hours)
  - Side-by-side code examples
  - Feature comparison table
  - Performance benchmarks
  - When to use each

- [ ] **Write blog post series** (16 hours)
  - "Convention-Based State Machines in C#"
  - "From Stateless to LionFire.StateMachines"
  - "State Machines in Domain-Driven Design"
  - "Building Workflows with Source Generators"

- [ ] **Create video tutorials** (16 hours)
  - 10-minute quick start
  - 30-minute deep dive
  - Real-world example walkthrough
  - Publish on YouTube

- [ ] **Set up community infrastructure** (4 hours)
  - Enable GitHub Discussions
  - Create Discord/Slack channel
  - Set up issue templates
  - Create PR templates

- [ ] **Publish case studies** (8 hours)
  - Find 2-3 early adopters
  - Document their use cases
  - Publish on website/blog

- [ ] **Present at conferences/meetups** (variable)
  - Submit talks to .NET conferences
  - Present at local user groups
  - Create slides and demos

**Deliverables**:
- Comparison page
- Blog posts
- Video tutorials
- Active community channels
- Case studies

**Success Metrics**:
- 1,000+ NuGet downloads/month
- Active GitHub issues/discussions
- Mentioned alongside Stateless in state machine discussions

---

### 4.3 Advanced Features (LOW PRIORITY)
**Effort**: 3+ weeks | **Impact**: Low-Medium

These are "nice to have" but not critical for initial adoption:

- [ ] **Hierarchical (nested) states** (20 hours)
  - Compete with Stateless and Appccelerate
  - Significant design complexity

- [ ] **Parallel states** (16 hours)
  - Multiple active states simultaneously
  - Orthogonal regions

- [ ] **History states** (8 hours)
  - Remember last active substate
  - Return to previous state

- [ ] **Trigger parameters** (12 hours)
  - Pass data with transitions
  - Type-safe parameter binding

- [ ] **Transition middleware** (12 hours)
  - Interceptor pipeline
  - Cross-cutting concerns (auth, logging, etc.)

**Deliverables**: Advanced state machine features

**Success Metrics**: Feature parity with Stateless

---

## Timeline Summary

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| **Phase 1: Foundation** | 4 weeks | Documentation, dependencies, code quality, tests |
| **Phase 2: Feature Parity** | 5 weeks | Async support, diagrams, performance |
| **Phase 3: Differentiation** | 5 weeks | DX improvements, persistence, observability |
| **Phase 4: Growth** | Ongoing | Integrations, community, marketing |

**Total to Production-Ready**: 12-14 weeks

## Success Criteria

### Minimum Viable Product (End of Phase 1)
- [ ] Complete, accurate documentation
- [ ] All dependencies up to date
- [ ] No known critical bugs
- [ ] 80% test coverage
- [ ] Clean codebase (no commented code, god classes refactored)

### Competitive Product (End of Phase 2)
- [ ] Async/await support
- [ ] Visual diagrams
- [ ] Performance competitive with Stateless
- [ ] 10+ production users

### Market Leader in Niche (End of Phase 3)
- [ ] Superior DX vs Stateless for simple cases
- [ ] Persistence support
- [ ] Strong observability
- [ ] 100+ production users
- [ ] Featured in blogs/talks

## Risks & Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Stateless adds source generation | Low | High | Focus on convention-based DX as differentiator |
| Community doesn't adopt | Medium | High | Invest heavily in documentation and marketing |
| Performance issues discovered | Medium | Medium | Address in Phase 2, benchmark early |
| Async design is complex | High | Medium | Start simple, iterate based on feedback |
| Maintainer burnout | Medium | High | Seek contributors early, automate where possible |

## Resource Requirements

**Minimum**:
- 1 experienced .NET developer (full-time for 12 weeks)
- Part-time technical writer for documentation
- Community manager for marketing (part-time)

**Optimal**:
- 2 developers (one on core, one on tooling/integrations)
- Technical writer
- Community manager
- Budget for hosting, domain, marketing

## Next Steps (Immediate)

**Week 1**:
1. Rewrite README with quick start
2. Fix NuGet package references
3. Update dependencies
4. Create first 2 examples

**Week 2**:
5. Add XML docs to top 10 most-used APIs
6. Refactor StateMachineGenerator (extract SyntaxAnalyzer)
7. Add Roslyn diagnostics for common errors
8. Create architecture doc

**Week 3**:
9. Complete API documentation
10. Create troubleshooting guide
11. Expand test suite (edge cases)
12. Add performance benchmarks

**Week 4**:
13. Verify netstandard2.0 or migrate to net6.0
14. Achieve 80% test coverage
15. Create CONTRIBUTING.md
16. Set up GitHub Discussions

---

## Long-Term Vision (1-2 Years)

- Industry-standard library for convention-based state machines
- 10,000+ monthly NuGet downloads
- Active community of contributors
- Featured in Microsoft documentation
- Conference talks at .NET Conf, NDC, etc.
- Visual designer tool for state machines
- Integration with popular ORMs, message buses, frameworks
- Commercial support option

## Conclusion

This roadmap provides a clear path from experimental library to production-ready framework. The key is execution: completing Phase 1 (Foundation) is absolutely critical before attempting later phases. Documentation and stability must come before advanced features.

The convention-based approach is genuinely innovative and provides a strong competitive moat. With proper execution, LionFire.StateMachines can carve out a significant niche in the .NET state machine ecosystem.
