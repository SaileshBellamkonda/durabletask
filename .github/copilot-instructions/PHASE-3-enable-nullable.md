# Phase 3: Enable Nullable Reference Types

## Objective
Enable nullable reference types at the project level for all projects and update code to properly annotate nullability, eliminating null reference exceptions through compile-time checking.

## Prerequisites
- Phase 1 must be completed (.NET 8/10 upgrade)
- Phase 2 must be completed (System.Text.Json migration)
- Solution must build and tests must pass
- Using C# latest (which supports nullable reference types)

## Important Notes
- This is a MAJOR code quality improvement
- Will generate many warnings initially - this is expected
- Some files already have `#nullable enable` - these are done
- Work incrementally, one project at a time
- Focus on correctness over speed
- This phase may take the most time of all phases

## Current State
Files already with nullable enabled (partial list):
- `test/DurableTask.AzureStorage.Tests/MessageManagerTests.cs`
- `test/DurableTask.AzureStorage.Tests/TestTablePartitionManager.cs`
- `test/DurableTask.AzureStorage.Tests/TestHelpers.cs`
- `test/DurableTask.Core.Tests/DispatcherMiddlewareTests.cs`
- `src/DurableTask.AzureStorage/OrchestrationSessionManager.cs`
- Several other files in test and src

## Step-by-Step Instructions

### 1. Enable Nullable Reference Types Globally

#### 1.1 Update `tools/DurableTask.props`
Add nullable configuration:
```xml
<PropertyGroup>
  <!-- Existing properties... -->
  <LangVersion>latest</LangVersion>
  <Nullable>enable</Nullable>
  <TreatWarningsAsErrors>True</TreatWarningsAsErrors>
</PropertyGroup>
```

This enables nullable for all projects that import this props file.

### 2. Understanding Nullable Annotations

Key nullable syntax:
- `string?` - nullable reference type (can be null)
- `string` - non-nullable reference type (should never be null)
- `!` operator - null-forgiving operator (tells compiler "I know this isn't null")
- `?? throw` - throw if null
- `?.` - null conditional operator
- `??` - null coalescing operator

### 3. Systematic Approach per Project

For each project, follow this process:

#### 3.1 Build and Collect Warnings
```bash
dotnet build src/ProjectName/ProjectName.csproj > warnings.txt 2>&1
```

#### 3.2 Common Warning Types and Fixes

**Warning CS8600: Converting null literal or possible null value to non-nullable type**
```csharp
// Problem
string name = GetName(); // GetName() might return null

// Fix Option 1: Make variable nullable
string? name = GetName();

// Fix Option 2: Provide default
string name = GetName() ?? string.Empty;

// Fix Option 3: Assert not null
string name = GetName() ?? throw new ArgumentNullException(nameof(name));
```

**Warning CS8601: Possible null reference assignment**
```csharp
// Problem
string name = nullableString;

// Fix Option 1: Check for null
if (nullableString != null)
{
    string name = nullableString;
}

// Fix Option 2: Use null-forgiving if you know it's not null
string name = nullableString!;

// Fix Option 3: Make target nullable
string? name = nullableString;
```

**Warning CS8602: Dereference of a possibly null reference**
```csharp
// Problem
int length = name.Length; // name might be null

// Fix Option 1: Null check
if (name != null)
{
    int length = name.Length;
}

// Fix Option 2: Null-conditional
int? length = name?.Length;

// Fix Option 3: Assert not null with null-forgiving
int length = name!.Length;
```

**Warning CS8603: Possible null reference return**
```csharp
// Problem
public string GetName() // declared as non-nullable
{
    return _name; // but _name might be null
}

// Fix Option 1: Make return type nullable
public string? GetName()
{
    return _name;
}

// Fix Option 2: Ensure never null
public string GetName()
{
    return _name ?? string.Empty;
}
```

**Warning CS8604: Possible null reference argument**
```csharp
// Problem
ProcessName(name); // name might be null

// Fix Option 1: Check before calling
if (name != null)
{
    ProcessName(name);
}

// Fix Option 2: Change method to accept nullable
void ProcessName(string? name)
{
    if (name == null) return;
    // process name
}
```

**Warning CS8618: Non-nullable field/property must contain a non-null value when exiting constructor**
```csharp
// Problem
public class MyClass
{
    public string Name { get; set; } // Never initialized
    
    public MyClass() { }
}

// Fix Option 1: Initialize in constructor
public class MyClass
{
    public string Name { get; set; }
    
    public MyClass()
    {
        Name = string.Empty;
    }
}

// Fix Option 2: Make nullable if it can be null
public class MyClass
{
    public string? Name { get; set; }
}

// Fix Option 3: Use default literal
public class MyClass
{
    public string Name { get; set; } = string.Empty;
}

// Fix Option 4: Use null-forgiving (use sparingly, only if initialized elsewhere)
public class MyClass
{
    public string Name { get; set; } = null!;
}
```

**Warning CS8625: Cannot convert null literal to non-nullable reference type**
```csharp
// Problem
string name = null;

// Fix Option 1: Use nullable type
string? name = null;

// Fix Option 2: Don't initialize to null
string name = string.Empty;
```

### 4. Project-Specific Guidelines

#### 4.1 Core Library (DurableTask.Core)

Priority areas to fix:
- Public API surface (classes, methods, properties)
- Task orchestration interfaces
- History and tracking entities
- Settings and configuration classes

Key considerations:
- Be conservative with nullable - prefer non-nullable for required parameters
- Use nullable for optional parameters and return values that can be null
- Add `[return: NotNull]` attributes where compiler needs help

Example patterns:
```csharp
// Public API - be explicit
public Task<OrchestrationState?> GetOrchestrationStateAsync(string instanceId);

public void ScheduleTask<T>(
    string name,
    string? version = null, // optional parameter
    T? input = default) where T : class;

// Internal implementation - be thorough
private string GetNameOrThrow(string? input)
{
    return input ?? throw new ArgumentNullException(nameof(input));
}
```

#### 4.2 Storage Providers (AzureStorage, ServiceBus)

Priority areas:
- Entity classes that map to storage
- Message serialization/deserialization
- Partition management
- Client APIs

Key considerations:
- Storage entities often have required properties - mark as non-nullable
- Use nullable for optional metadata
- Partition leases may have conditional properties

Example:
```csharp
public class OrchestrationStateEntity
{
    public string InstanceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; } // Optional
    public string? Input { get; set; } // May not be set
    public OrchestrationStatus Status { get; set; }
}
```

#### 4.3 Test Projects

For tests:
- Test methods can be more lenient with `!` operator if you know values aren't null
- Test helper methods should be properly annotated
- Mock setups should match nullability of real implementations

Example:
```csharp
[TestMethod]
public async Task TestOrchestration_Success()
{
    // Setup
    var client = CreateTestClient();
    var instance = await client.CreateOrchestrationInstanceAsync(
        typeof(TestOrchestration),
        "testInput");
    
    // Can use ! if we know the test setup ensures non-null
    Assert.IsNotNull(instance);
    var state = await client.WaitForOrchestrationAsync(instance.InstanceId, TimeSpan.FromSeconds(30));
    Assert.AreEqual(OrchestrationStatus.Completed, state!.OrchestrationStatus);
}
```

### 5. Special Patterns and Techniques

#### 5.1 Generic Constraints
```csharp
// Make generic constraints nullable-aware
public T? GetValue<T>() where T : class;
public T GetValue<T>() where T : struct;
public T GetValueOrDefault<T>() where T : class?;
```

#### 5.2 Collection Initialization
```csharp
// Old
public List<string> Items { get; set; }

// New - always initialize collections
public List<string> Items { get; set; } = new List<string>();
// Or
public List<string> Items { get; set; } = new();
// Or with C# 12+
public List<string> Items { get; set; } = [];
```

#### 5.3 Async/Await and Task<T?>
```csharp
// Return nullable from async method
public async Task<OrchestrationState?> GetStateAsync(string instanceId)
{
    var result = await FetchFromStorageAsync(instanceId);
    return result; // can be null
}

// Consume
var state = await GetStateAsync(instanceId);
if (state != null)
{
    // Use state
}
```

#### 5.4 Using Attributes for Better Analysis

Add these attributes where needed:
```csharp
using System.Diagnostics.CodeAnalysis;

// Assert that method always returns non-null
[return: NotNull]
public string? GetValueOrDefault(string? input)
{
    return input ?? "default";
}

// Assert that after this method, parameter is not null
public bool TryGetValue([NotNullWhen(true)] out string? value)
{
    // ...
}

// Assert parameter is not null when method returns
public void Initialize([NotNull] ref string? config)
{
    config ??= LoadDefaultConfig();
}
```

### 6. Files Already with #nullable enable

These files already have nullable enabled. Verify they're correctly annotated and remove the `#nullable enable` directive (since it's now project-wide):

Test files:
- `test/DurableTask.AzureStorage.Tests/MessageManagerTests.cs`
- `test/DurableTask.AzureStorage.Tests/TestTablePartitionManager.cs`
- `test/DurableTask.AzureStorage.Tests/TestHelpers.cs`
- `test/DurableTask.AzureStorage.Tests/Net/UriPathTests.cs`
- `test/DurableTask.Core.Tests/DispatcherMiddlewareTests.cs`
- `test/DurableTask.Core.Tests/TaskHubClientTests.cs`
- `test/DurableTask.Core.Tests/ExceptionHandlingIntegrationTests.cs`

Source files:
- `src/DurableTask.AzureStorage/OrchestrationSessionManager.cs`
- `src/DurableTask.AzureStorage/TrackingServiceClientProvider.cs`
- `src/DurableTask.AzureStorage/Http/MonitoringHttpPipelinePolicy.cs`

### 7. Incremental Approach (Recommended)

If enabling nullable project-wide causes too many errors, use incremental approach:

#### 7.1 Disable Globally, Enable Per-File
```xml
<!-- In DurableTask.props -->
<Nullable>disable</Nullable>
```

Then in each file:
```csharp
#nullable enable
namespace DurableTask.Core
{
    // File content
}
#nullable restore
```

#### 7.2 Progressive Rollout Order
1. Test helper classes
2. Test classes
3. Data model classes (entities, DTOs)
4. Internal implementation classes
5. Public API classes (most critical)

### 8. Build and Verify

#### 8.1 Clean Build
```bash
dotnet clean
dotnet build --configuration Release
```

#### 8.2 Address Warnings Systematically
For each project:
```bash
dotnet build src/DurableTask.Core/DurableTask.Core.csproj | grep CS8
```

Fix warnings in batches:
- First: CS8618 (uninitialized non-nullable fields)
- Second: CS8600, CS8601, CS8625 (null assignments)
- Third: CS8602, CS8604 (dereferences and arguments)
- Fourth: CS8603 (returns)
- Finally: Any remaining warnings

#### 8.3 Run Tests
```bash
dotnet test --configuration Release
```

#### 8.4 Verify API Correctness
Ensure public APIs make sense:
- Required parameters are non-nullable
- Optional values are nullable
- Return values correctly express nullability

### 9. Documentation Updates

#### 9.1 Update XML Documentation
Add nullability info to XML docs:
```csharp
/// <summary>
/// Gets the orchestration state for the specified instance.
/// </summary>
/// <param name="instanceId">The instance identifier. Cannot be null.</param>
/// <returns>
/// The orchestration state, or <c>null</c> if the instance is not found.
/// </returns>
public Task<OrchestrationState?> GetOrchestrationStateAsync(string instanceId);
```

#### 9.2 Update README.md
Mention that the project uses nullable reference types and requires C# latest.

### 10. Final Verification

- [ ] All projects build without nullable warnings
- [ ] All tests pass
- [ ] Public API surfaces correctly express nullability
- [ ] No excessive use of `!` operator (indicates poor null handling)
- [ ] Collections are initialized
- [ ] No `#nullable enable` directives remain (since it's global)
- [ ] Code is cleaner and safer from null reference exceptions

## Common Pitfalls to Avoid

### Pitfall 1: Over-using Null-Forgiving Operator
```csharp
// BAD - hiding potential issues
string name = GetName()!;
ProcessName(name!);

// GOOD - proper null handling
string? name = GetName();
if (name != null)
{
    ProcessName(name);
}
```

### Pitfall 2: Making Everything Nullable
```csharp
// BAD - too permissive
public void Process(string? input)
{
    // Now have to null-check everywhere
}

// GOOD - be specific about requirements
public void Process(string input)
{
    ArgumentNullException.ThrowIfNull(input);
    // No null checks needed
}
```

### Pitfall 3: Not Initializing Collections
```csharp
// BAD - null collection
public List<string>? Items { get; set; }

// GOOD - empty collection
public List<string> Items { get; set; } = new();
```

### Pitfall 4: Ignoring Generic Nullability
```csharp
// BAD - unclear what T can be
public T GetValue<T>();

// GOOD - explicit constraints
public T? GetValue<T>() where T : class;
```

## Expected Outcome

After completing this phase:
- [ ] Nullable reference types enabled project-wide
- [ ] All null reference warnings resolved
- [ ] Code is significantly safer from null reference exceptions
- [ ] Public APIs clearly express nullability contracts
- [ ] Tests pass and coverage maintains or improves
- [ ] Ready for Phase 4 (Modern C# features)

## Rollback Plan

If insurmountable issues are found:
- Revert to `<Nullable>disable</Nullable>`
- Keep per-file `#nullable enable` where already done
- Gradually expand nullable-enabled files
- Document blocking issues

## Notes for Agent

- This phase requires careful thought and code understanding
- Don't rush - nullability bugs are subtle
- When in doubt, prefer non-nullable for required values
- Use nullable for genuinely optional values
- Build and test frequently - catch issues early
- The compiler is your friend - fix warnings properly
- Consider the contract: "What should this API accept/return?"
- Document decisions in code comments where nullability is non-obvious
