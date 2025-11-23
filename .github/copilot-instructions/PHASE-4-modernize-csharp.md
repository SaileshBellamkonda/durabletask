# Phase 4: Modernize C# Code with Latest Features

## Objective
Update the codebase to use modern C# features including primary constructors, required properties, records, file-scoped namespaces, collection expressions, and other C# 10-13 features.

## Prerequisites
- Phase 1 completed (.NET 8/10 upgrade)
- Phase 2 completed (System.Text.Json migration)
- Phase 3 completed (Nullable reference types enabled)
- All tests passing
- Using C# latest (set in Phase 1)

## Important Notes
- This phase improves code readability and maintainability
- Changes should not alter behavior, only improve code style
- Focus on consistent patterns across the codebase
- Some features are optional - use where they add value
- This is the final modernization phase

## Available C# Features by Version

### C# 10 (.NET 6+)
- File-scoped namespaces
- Global using directives
- Extended property patterns
- Lambda improvements
- Constant interpolated strings

### C# 11 (.NET 7+)
- Required members
- Raw string literals
- List patterns
- File-local types
- UTF-8 string literals

### C# 12 (.NET 8+)
- Primary constructors
- Collection expressions
- Inline arrays
- Lambda default parameters
- Alias any type

### C# 13 (.NET 9+)
- Params collections
- Partial properties
- Lock object improvements

## Step-by-Step Instructions

### 1. File-Scoped Namespaces (C# 10)

Convert all files to file-scoped namespaces for cleaner code.

#### Pattern
```csharp
// Old
namespace DurableTask.Core
{
    public class MyClass
    {
        // Implementation
    }
}

// New
namespace DurableTask.Core;

public class MyClass
{
    // Implementation
}
```

#### Approach
For each .cs file:
1. If it has a single namespace block (most do)
2. Convert to file-scoped namespace
3. Remove one level of indentation

#### Automated Tool (Optional)
```bash
# Use dotnet format if available
dotnet format --include-generated
```

### 2. Primary Constructors (C# 12)

Use primary constructors for simple classes with constructor parameters.

#### Pattern: Dependency Injection Classes
```csharp
// Old
public class OrchestrationService
{
    private readonly ILogger _logger;
    private readonly OrchestrationSettings _settings;
    
    public OrchestrationService(ILogger logger, OrchestrationSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }
    
    public void Execute()
    {
        _logger.LogInformation("Executing");
    }
}

// New
public class OrchestrationService(ILogger logger, OrchestrationSettings settings)
{
    public void Execute()
    {
        logger.LogInformation("Executing");
    }
}
```

#### Pattern: Simple DTOs with Validation
```csharp
// Old
public class OrchestrationRequest
{
    public string Name { get; }
    public string InstanceId { get; }
    
    public OrchestrationRequest(string name, string instanceId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        InstanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
    }
}

// New
public class OrchestrationRequest(string name, string instanceId)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string InstanceId { get; } = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
}
```

#### When to Use Primary Constructors
✅ Use when:
- Simple dependency injection
- Constructor parameters map directly to fields/properties
- No complex initialization logic

❌ Don't use when:
- Multiple constructors needed
- Complex initialization logic
- Constructor performs operations beyond assignment
- Need to capture parameters as private fields with different names

### 3. Required Properties (C# 11)

Mark properties that must be initialized as `required`.

#### Pattern: Configuration Classes
```csharp
// Old
public class AzureStorageSettings
{
    public string ConnectionString { get; set; } = null!;
    public string TaskHubName { get; set; } = null!;
    public int MaxConcurrentTaskActivityWorkItems { get; set; }
    
    public AzureStorageSettings()
    {
        // Properties set via object initializer
    }
}

// New
public class AzureStorageSettings
{
    public required string ConnectionString { get; set; }
    public required string TaskHubName { get; set; }
    public int MaxConcurrentTaskActivityWorkItems { get; set; }
}

// Usage enforces initialization
var settings = new AzureStorageSettings 
{ 
    ConnectionString = "...",  // Required
    TaskHubName = "..."         // Required
};
```

#### Pattern: Entity Classes
```csharp
// Old
public class OrchestrationInstance
{
    public string InstanceId { get; set; } = string.Empty;
    public string ExecutionId { get; set; } = string.Empty;
}

// New
public class OrchestrationInstance
{
    public required string InstanceId { get; set; }
    public required string ExecutionId { get; set; }
}
```

#### When to Use Required
✅ Use when:
- Property must be set for object to be valid
- Using object initializer syntax
- No constructor initialization

❌ Don't use when:
- Property has a sensible default value
- Property is optional/nullable
- Using constructor-based initialization

### 4. Records (C# 9+)

Convert immutable data classes to records.

#### Pattern: Value Objects
```csharp
// Old
public class OrchestrationState
{
    public string InstanceId { get; init; }
    public OrchestrationStatus Status { get; init; }
    public DateTime CreatedTime { get; init; }
    
    public override bool Equals(object? obj)
    {
        // Equality implementation
    }
    
    public override int GetHashCode()
    {
        // Hash code implementation
    }
}

// New
public record OrchestrationState(
    string InstanceId,
    OrchestrationStatus Status,
    DateTime CreatedTime);

// Or with properties
public record OrchestrationState
{
    public required string InstanceId { get; init; }
    public required OrchestrationStatus Status { get; init; }
    public required DateTime CreatedTime { get; init; }
}
```

#### Pattern: DTOs
```csharp
// Old
public class TaskMessage
{
    public int TaskId { get; set; }
    public string Payload { get; set; } = string.Empty;
}

// New - if immutable
public record TaskMessage(int TaskId, string Payload);

// Or if mutable properties needed
public record TaskMessage
{
    public int TaskId { get; set; }
    public required string Payload { get; set; }
}
```

#### Pattern: Record Structs for Small Value Types
```csharp
// For small, frequently-created types
public readonly record struct PartitionKey(string Value);
public readonly record struct Timestamp(DateTimeOffset Value);
```

#### When to Use Records
✅ Use when:
- Immutable data transfer objects
- Value-based equality needed
- Simple data containers
- Event/message types

❌ Don't use when:
- Complex mutable state
- Entity with behavior-heavy logic
- Need explicit control over equality

### 5. Collection Expressions (C# 12)

Simplify collection initialization with collection expressions.

#### Pattern: Array Creation
```csharp
// Old
var items = new[] { "a", "b", "c" };
var empty = new string[] { };

// New
var items = ["a", "b", "c"];
var empty = string[];
```

#### Pattern: List Creation
```csharp
// Old
var list = new List<string> { "a", "b", "c" };
var empty = new List<string>();

// New
var list = ["a", "b", "c"];
List<string> typed = ["a", "b", "c"];
var empty = List<string> [];
```

#### Pattern: Spreading Collections
```csharp
// Old
var combined = new List<string>();
combined.AddRange(first);
combined.AddRange(second);

// New
var combined = [..first, ..second];

// With additional items
var extended = [..existing, "new1", "new2"];
```

#### Pattern: Property Initialization
```csharp
// Old
public class MyClass
{
    public List<string> Tags { get; set; } = new List<string>();
}

// New
public class MyClass
{
    public List<string> Tags { get; set; } = [];
}
```

### 6. Switch Expressions and Patterns

Modernize switch statements to switch expressions where appropriate.

#### Pattern: Simple Mapping
```csharp
// Old
OrchestrationStatus GetStatus(string status)
{
    switch (status)
    {
        case "Running":
            return OrchestrationStatus.Running;
        case "Completed":
            return OrchestrationStatus.Completed;
        case "Failed":
            return OrchestrationStatus.Failed;
        default:
            throw new ArgumentException($"Unknown status: {status}");
    }
}

// New
OrchestrationStatus GetStatus(string status) => status switch
{
    "Running" => OrchestrationStatus.Running,
    "Completed" => OrchestrationStatus.Completed,
    "Failed" => OrchestrationStatus.Failed,
    _ => throw new ArgumentException($"Unknown status: {status}")
};
```

#### Pattern: Property Patterns
```csharp
// Old
decimal GetPrice(Product product)
{
    if (product.Category == "Electronics" && product.InStock)
        return product.Price * 0.9m;
    else if (product.Category == "Books")
        return product.Price * 0.8m;
    else
        return product.Price;
}

// New
decimal GetPrice(Product product) => product switch
{
    { Category: "Electronics", InStock: true } => product.Price * 0.9m,
    { Category: "Books" } => product.Price * 0.8m,
    _ => product.Price
};
```

### 7. Target-Typed New (C# 9)

Use target-typed `new` expressions for cleaner code.

#### Pattern
```csharp
// Old
Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
MyClass obj = new MyClass();

// New
Dictionary<string, List<string>> map = new();
MyClass obj = new();
```

### 8. Lambda Improvements (C# 10-12)

Use modern lambda syntax.

#### Pattern: Natural Type
```csharp
// Old
Func<int, int> square = (int x) => x * x;

// New - compiler infers type
var square = (int x) => x * x;
```

#### Pattern: Attributes on Lambdas
```csharp
// New in C# 10
var handler = [Description("Handles requests")] (string request) => Process(request);
```

### 9. Raw String Literals (C# 11)

Use raw string literals for multi-line strings and strings with quotes.

#### Pattern: JSON/XML Literals
```csharp
// Old
var json = @"{
    ""name"": ""test"",
    ""value"": 42
}";

// New
var json = """
    {
        "name": "test",
        "value": 42
    }
    """;
```

#### Pattern: Interpolated Raw Strings
```csharp
// New
var json = $$"""
    {
        "instanceId": "{{instanceId}}",
        "status": "{{status}}"
    }
    """;
```

### 10. Global Usings

Create a global usings file to reduce using statements.

#### Create `GlobalUsings.cs` in each project
```csharp
// src/DurableTask.Core/GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Text.Json;
global using Microsoft.Extensions.Logging;
```

Then remove these usings from individual files.

### 11. String Interpolation Improvements

Use modern string interpolation features.

#### Pattern: Alignment and Formatting
```csharp
// Old
string.Format("{0,-10} {1:N2}", name, value);

// New
$"{name,-10} {value:N2}"
```

#### Pattern: Constant Interpolated Strings (C# 10)
```csharp
// New - if all parts are constant
const string LogPrefix = $"{nameof(DurableTask)}.{nameof(Core)}";
```

### 12. Async Improvements

Use modern async patterns.

#### Pattern: ConfigureAwait(false) is Less Needed
```csharp
// Old - required in libraries
await DoSomethingAsync().ConfigureAwait(false);

// New - .NET Core+ doesn't need it as much, but keep for library code
await DoSomethingAsync();
// Keep ConfigureAwait(false) for library code to avoid sync context issues
```

#### Pattern: ValueTask for Hot Paths
```csharp
// When method often completes synchronously
public ValueTask<int> GetCachedValueAsync(string key)
{
    if (_cache.TryGetValue(key, out int value))
        return new ValueTask<int>(value);
    
    return LoadValueAsync(key);
}
```

### 13. Pattern Matching Improvements

Use enhanced pattern matching.

#### Pattern: List Patterns (C# 11)
```csharp
// Check list structure
bool IsValid(string[] args) => args switch
{
    [] => false,                          // Empty
    [_] => false,                         // One element
    [var first, ..] => first.Length > 0,  // At least one, check first
    _ => true
};
```

#### Pattern: Property Pattern with Extended
```csharp
// Old
if (state != null && state.Status == OrchestrationStatus.Running && state.Input != null)

// New
if (state is { Status: OrchestrationStatus.Running, Input: not null })
```

### 14. Systematic Conversion Approach

#### Order of Refactoring
1. **File-scoped namespaces** - Low risk, high impact
2. **Collection expressions** - Low risk, good readability
3. **Target-typed new** - Low risk, cleaner code
4. **Required properties** - Medium risk, good safety
5. **Primary constructors** - Medium risk, review carefully
6. **Records** - Higher risk, changes semantics
7. **Switch expressions** - Review logic carefully
8. **Global usings** - Do last, easier to review

#### Per-Project Process
For each project:
1. Apply file-scoped namespaces to all files
2. Add GlobalUsings.cs
3. Convert simple collections to collection expressions
4. Apply target-typed new where appropriate
5. Identify candidates for primary constructors
6. Mark required properties
7. Identify DTOs that should be records
8. Modernize switch statements
9. Build and test
10. Review and refine

### 15. Build and Verify

#### After Each Change Category
```bash
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```

#### Code Review Checklist
- [ ] File-scoped namespaces applied consistently
- [ ] Collection expressions used where appropriate
- [ ] Primary constructors used appropriately (not overused)
- [ ] Required properties marked where needed
- [ ] Immutable data as records
- [ ] Switch expressions for simple mappings
- [ ] Global usings reduce repetition
- [ ] No behavior changes from refactoring

### 16. Documentation

#### Update Code Comments
Modernize XML documentation to reflect new patterns:
```csharp
/// <summary>
/// Represents an orchestration instance with required identification.
/// </summary>
/// <param name="InstanceId">The unique instance identifier.</param>
/// <param name="ExecutionId">The execution identifier.</param>
public record OrchestrationInstance(string InstanceId, string ExecutionId);
```

#### Update README.md
- Note that code uses modern C# features
- Specify C# language version requirements
- Update sample code to show modern syntax

### 17. Areas to Focus

Priority files for modernization:

#### High Priority (Public APIs)
- `src/DurableTask.Core/` - Core interfaces and types
- Configuration classes in all projects
- Public DTOs and messages

#### Medium Priority (Internal Implementation)
- Service implementations
- Storage providers
- Internal utilities

#### Lower Priority
- Test helpers (but still do them)
- Sample code (make sure samples show modern patterns)

### 18. Specific Project Guidance

#### DurableTask.Core
- Interfaces: Consider adding default implementations
- TaskOrchestration: Keep as class (has state)
- OrchestrationInstance: Good candidate for record
- Settings classes: Use required properties

#### DurableTask.AzureStorage
- Entity classes: Consider records for immutable ones
- Partition classes: Primary constructors for DI
- Manager classes: Evaluate primary constructors

#### Test Projects
- Test helpers: Primary constructors
- Test data builders: Collection expressions
- Assert helpers: File-scoped namespaces

## Expected Outcome

After completing this phase:
- [ ] All files use file-scoped namespaces
- [ ] Collection expressions used throughout
- [ ] Appropriate use of primary constructors
- [ ] Required properties marked on DTOs/settings
- [ ] Immutable data uses records
- [ ] Switch expressions for simple mappings
- [ ] Global usings reduce boilerplate
- [ ] Code is more readable and maintainable
- [ ] All tests still pass
- [ ] No behavior changes

## What NOT to Change

Don't change:
- Serialization formats (already done in Phase 2)
- Public API contracts (breaking changes)
- Complex classes to records (if they have significant behavior)
- Constructor patterns that need multiple constructors
- Anything that would break backward compatibility

## Common Issues

### Issue 1: Primary Constructor Parameter Capture
```csharp
// Problem - parameter not captured
public class Service(ILogger logger)
{
    public void Log() => logger.LogInformation("test"); // Error if not in scope
}

// Fix - explicitly capture
public class Service(ILogger logger)
{
    private readonly ILogger _logger = logger;
    public void Log() => _logger.LogInformation("test");
}
```

### Issue 2: Record Value Semantics
Records have value-based equality:
```csharp
var a = new MyRecord(1);
var b = new MyRecord(1);
Console.WriteLine(a == b); // True for records, would be false for classes
```

Only use records where this is desired behavior.

## Final Verification

- [ ] Solution builds without warnings
- [ ] All tests pass
- [ ] Code follows consistent modern patterns
- [ ] No unintended behavior changes
- [ ] Samples demonstrate modern features
- [ ] Documentation updated

## Notes for Agent

- This phase improves code quality without changing behavior
- Be conservative with primary constructors - not everything needs them
- Records are great for DTOs but not for entities with behavior
- File-scoped namespaces are safe and should be done first
- Build and test frequently
- When in doubt, prefer readability over using every new feature
- Some features (like raw string literals) are optional - use where they add value
- Keep the code consistent - if using a pattern, use it throughout
- Review each change - automated refactoring can introduce subtle issues

## Success Criteria

The modernization is successful when:
1. Code uses modern C# idioms consistently
2. Readability is improved
3. No bugs introduced
4. All tests pass
5. Builds are clean (no warnings)
6. Team can easily understand the patterns used
7. Future development benefits from better language features

Congratulations on completing the full modernization! 🎉
