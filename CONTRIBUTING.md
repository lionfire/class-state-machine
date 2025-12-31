# Contributing to LionFire.StateMachines

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider
- Git

### Building Locally

```bash
# Clone the repository
git clone https://github.com/lionfire/Core.git
cd Core/StateMachines

# Restore dependencies
dotnet restore StateMachines.sln

# Build
dotnet build StateMachines.sln

# Run tests
dotnet test StateMachines.sln
```

### Project Structure

```
StateMachines/
├── src/
│   ├── LionFire.StateMachines.Abstractions/  # Attributes, interfaces, exceptions
│   ├── LionFire.StateMachines/               # Runtime implementation
│   └── LionFire.StateMachines.Generation/    # Roslyn source generator
├── test/
│   ├── LionFire.StateMachines.Tests/         # Unit tests
│   ├── LionFire.StateMachines.Tests.Model/   # Test model classes
│   └── LionFire.StateMachines.Tests.ExternalModel/  # Separate enum definitions
└── docs/                                      # Documentation
```

## How to Contribute

### Reporting Issues

- Search existing issues before creating a new one
- Use a clear, descriptive title
- Include reproduction steps
- Include your environment (OS, .NET version, IDE)

### Suggesting Features

- Open an issue with `[Feature Request]` prefix
- Describe the use case
- Explain why existing features don't meet your needs

### Submitting Pull Requests

1. **Fork the repository**

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow the code style (see below)
   - Add tests for new functionality
   - Update documentation as needed

4. **Test your changes**
   ```bash
   dotnet test StateMachines.sln
   ```

5. **Commit with a clear message**
   ```bash
   git commit -m "feat: add async transition support"
   ```

   Use conventional commits:
   - `feat:` - New feature
   - `fix:` - Bug fix
   - `docs:` - Documentation
   - `refactor:` - Code refactoring
   - `test:` - Adding tests
   - `chore:` - Maintenance

6. **Push and create PR**
   ```bash
   git push origin feature/your-feature-name
   ```

## Code Style

### General Guidelines

- Use C# 12 features where appropriate
- Prefer `var` when type is obvious
- Use expression-bodied members for simple methods
- Keep methods short and focused

### Naming Conventions

- **Classes/Interfaces**: PascalCase (`StateMachine`, `ITransitionHandler`)
- **Methods**: PascalCase (`Transition`, `TryTransition`)
- **Properties**: PascalCase (`CurrentState`, `LastStateChange`)
- **Private fields**: camelCase (`_stateMachine`, `lockObject`)
- **Parameters**: camelCase (`transition`, `stateChange`)
- **Constants**: PascalCase (`DefaultTimeout`)

### Documentation

- Add XML comments to all public APIs
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` where applicable
- Update README.md for user-facing changes

### Source Generator Guidelines

The generator uses `IIncrementalGenerator`. When modifying:

- Keep it self-contained (no runtime project references)
- Use pure functions where possible
- Implement proper equality for caching
- Follow Roslyn analyzer best practices

## Testing

### Running Tests

```bash
# Run all tests
dotnet test StateMachines.sln

# Run with coverage
dotnet test StateMachines.sln --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~TransitionPrereq"
```

### Writing Tests

- Add tests for new features
- Test both success and failure cases
- Test edge cases (null inputs, invalid states)
- Use descriptive test names

```csharp
[Fact]
public void Transition_WhenGuardReturnsFalse_ShouldNotChangeState()
{
    // Arrange
    var sut = new GeneratedExecutable();
    sut.InitializePrereq = false;

    // Act
    sut.StateMachine.TryTransition(ExecutionTransition.Initialize);

    // Assert
    Assert.Equal(ExecutionState.Uninitialized, sut.StateMachine.CurrentState);
}
```

## Areas Needing Help

We especially welcome contributions in these areas:

1. **Async Support** - Implementing async/await for transitions
2. **Roslyn Diagnostics** - Adding IDE warnings/errors for common mistakes
3. **Documentation** - Improving examples and explanations
4. **Performance** - Replacing reflection with compiled expressions
5. **Tests** - Expanding edge case coverage

## Questions?

- Open a GitHub Discussion for general questions
- Tag maintainers in issues if blocked

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
