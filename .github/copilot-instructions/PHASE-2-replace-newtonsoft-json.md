# Phase 2: Replace Newtonsoft.Json with System.Text.Json

## Objective
Replace all Newtonsoft.Json functionality with System.Text.Json throughout the codebase. This includes serialization, deserialization, and JSON manipulation operations.

## Prerequisites
- Phase 1 must be completed (.NET 8/10 upgrade)
- Solution must build successfully
- System.Text.Json package must be available (included in .NET 8+)

## Important Notes
- System.Text.Json has different default behaviors than Newtonsoft.Json
- Pay attention to serialization settings (camelCase, null handling, etc.)
- Some Newtonsoft.Json features may require custom converters in System.Text.Json
- This is a BREAKING CHANGE for any code that relies on serialization format details
- Test thoroughly after each file conversion

## Known Usage Locations (42 files in src/)

Key files to update based on grep results:
- `src/DurableTask.ServiceBus/Common/ServiceBusUtils.cs`
- `src/DurableTask.ServiceBus/Tracking/AzureTableOrchestrationHistoryEventEntity.cs`
- `src/DurableTask.AzureStorage/Partitioning/AppLeaseManager.cs`
- `src/DurableTask.AzureStorage/Partitioning/BlobPartitionLease.cs`
- Multiple files in DurableTask.Core

## Step-by-Step Instructions

### 1. Update Package References

#### 1.1 Update `Directory.Packages.props`
```xml
<!-- Keep System.Text.Json and update to latest -->
<PackageVersion Include="System.Text.Json" Version="8.0.5" />

<!-- Mark Newtonsoft.Json for removal - but don't remove yet to track usage -->
<!-- Will be removed at the end -->
```

### 2. Understand System.Text.Json Differences

Key behavioral differences to account for:
- **Property naming:** Newtonsoft defaults to PascalCase, System.Text.Json defaults to exact match
- **Null handling:** Different default behaviors
- **Enums:** Newtonsoft serializes as strings by default, System.Text.Json as numbers
- **Dynamic objects:** JObject/JToken don't exist - use JsonDocument/JsonElement instead
- **Converters:** Need to rewrite custom JsonConverter implementations

### 3. Create Common Serialization Configuration

#### 3.1 Create JsonSerializerOptions Helper
Create a new file `src/DurableTask.Core/Serialization/JsonSettings.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DurableTask.Core.Serialization
{
    /// <summary>
    /// Provides consistent JSON serialization settings across DurableTask.
    /// </summary>
    public static class JsonSettings
    {
        private static readonly Lazy<JsonSerializerOptions> _defaultOptions = new(() =>
        {
            var options = new JsonSerializerOptions
            {
                // Match Newtonsoft.Json defaults where possible for compatibility
                PropertyNamingPolicy = null, // Use PascalCase (same as Newtonsoft default)
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                PropertyNameCaseInsensitive = true, // More forgiving than Newtonsoft
                Converters =
                {
                    new JsonStringEnumConverter() // Serialize enums as strings (Newtonsoft default)
                }
            };
            
            return options;
        });

        /// <summary>
        /// Gets the default JSON serialization options for DurableTask.
        /// </summary>
        public static JsonSerializerOptions Default => _defaultOptions.Value;
        
        /// <summary>
        /// Gets JSON serialization options with indented formatting.
        /// </summary>
        public static JsonSerializerOptions Indented => new(Default) { WriteIndented = true };
    }
}
```

### 4. Replace Newtonsoft.Json Usage Patterns

#### 4.1 Simple Serialization/Deserialization

**Pattern 1: JsonConvert.SerializeObject**
```csharp
// Old (Newtonsoft.Json)
using Newtonsoft.Json;
string json = JsonConvert.SerializeObject(obj);

// New (System.Text.Json)
using System.Text.Json;
using DurableTask.Core.Serialization;
string json = JsonSerializer.Serialize(obj, JsonSettings.Default);
```

**Pattern 2: JsonConvert.DeserializeObject**
```csharp
// Old
var obj = JsonConvert.DeserializeObject<MyType>(json);

// New
var obj = JsonSerializer.Deserialize<MyType>(json, JsonSettings.Default);
```

**Pattern 3: With JsonSerializerSettings**
```csharp
// Old
var settings = new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All,
    Formatting = Formatting.Indented
};
string json = JsonConvert.SerializeObject(obj, settings);

// New - Create custom options or add to JsonSettings
var options = new JsonSerializerOptions(JsonSettings.Default)
{
    WriteIndented = true
    // Note: TypeNameHandling.All requires custom implementation
};
string json = JsonSerializer.Serialize(obj, options);
```

#### 4.2 JObject/JToken Replacement

**Pattern 1: Parse JSON**
```csharp
// Old
using Newtonsoft.Json.Linq;
JObject obj = JObject.Parse(json);
string value = obj["property"].ToString();

// New
using System.Text.Json;
using JsonDocument doc = JsonDocument.Parse(json);
JsonElement root = doc.RootElement;
string value = root.GetProperty("property").GetString();
```

**Pattern 2: Create JSON dynamically**
```csharp
// Old
var obj = new JObject
{
    ["name"] = "value",
    ["count"] = 42
};
string json = obj.ToString();

// New
using System.Text.Json.Nodes;
var obj = new JsonObject
{
    ["name"] = "value",
    ["count"] = 42
};
string json = obj.ToJsonString();
```

**Pattern 3: Query JSON**
```csharp
// Old
JToken token = JToken.Parse(json);
string value = token.SelectToken("$.path.to.property")?.ToString();

// New
using JsonDocument doc = JsonDocument.Parse(json);
// Use GetProperty() chain or implement JSONPath helper
JsonElement element = doc.RootElement.GetProperty("path").GetProperty("to").GetProperty("property");
string value = element.GetString();
```

#### 4.3 Custom JsonConverter

**Pattern: Custom Type Converter**
```csharp
// Old (Newtonsoft.Json)
public class MyConverter : Newtonsoft.Json.JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Write logic
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, 
        object existingValue, JsonSerializer serializer)
    {
        // Read logic
        return result;
    }
    
    public override bool CanConvert(Type objectType) => objectType == typeof(MyType);
}

// New (System.Text.Json)
public class MyConverter : System.Text.Json.Serialization.JsonConverter<MyType>
{
    public override void Write(Utf8JsonWriter writer, MyType value, JsonSerializerOptions options)
    {
        // Write logic
    }
    
    public override MyType Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        // Read logic
        return result;
    }
}
```

### 5. Update Key Files

For each file with Newtonsoft.Json usage:

#### 5.1 Update `using` statements
```csharp
// Remove
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

// Add
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes; // For JsonObject/JsonArray
using DurableTask.Core.Serialization; // For JsonSettings
```

#### 5.2 Process Each File Systematically

1. **Identify usage patterns** in the file
2. **Apply appropriate replacements** from patterns above
3. **Build the project** to catch compile errors
4. **Run tests** for the component
5. **Fix any runtime issues** related to serialization

#### 5.3 Special Attention Files

Files that likely need careful conversion:

**`src/DurableTask.Core/` - Core serialization**
- Look for DataConverter implementations
- Check for polymorphic serialization (TypeNameHandling)
- Verify backward compatibility requirements

**`src/DurableTask.AzureStorage/Partitioning/`**
- Lease serialization
- Partition management state

**`src/DurableTask.ServiceBus/`**
- Message serialization
- History event serialization

### 6. Handle Special Cases

#### 6.1 TypeNameHandling (Polymorphic Serialization)
Newtonsoft's TypeNameHandling.All/Auto doesn't have a direct equivalent. Options:
- Use JsonDerivedType attribute (C# 11+)
- Implement custom type discriminator
- Use source generation with known types

Example with JsonDerivedType:
```csharp
[JsonDerivedType(typeof(DerivedClass1), "type1")]
[JsonDerivedType(typeof(DerivedClass2), "type2")]
public abstract class BaseClass { }
```

#### 6.2 DateTime Handling
```csharp
// Newtonsoft often uses ISO 8601 format
// System.Text.Json does too by default, but verify format matches

// If custom format needed:
public class DateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";
    
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);
    }
    
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
```

#### 6.3 Null Handling
```csharp
// To ignore null values during serialization:
var options = new JsonSerializerOptions(JsonSettings.Default)
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

### 7. Update Tests

#### 7.1 Test Files to Update
All test files that reference Newtonsoft.Json:
- `test/DurableTask.AzureStorage.Tests/`
- `test/DurableTask.Core.Tests/`
- `test/DurableTask.ServiceBus.Tests/`

#### 7.2 Update Test Assertions
```csharp
// Old
var obj = JsonConvert.DeserializeObject<MyType>(json);
Assert.AreEqual(expected, obj.Property);

// New
var obj = JsonSerializer.Deserialize<MyType>(json, JsonSettings.Default);
Assert.AreEqual(expected, obj.Property);
```

### 8. Update Sample Projects

Update all samples to use System.Text.Json:
- `samples/Correlation.Samples/`
- `samples/DurableTask.Samples/`
- Other sample projects

### 9. Remove Newtonsoft.Json References

#### 9.1 Remove from Directory.Packages.props
```xml
<!-- Remove this line -->
<PackageVersion Include="Newtonsoft.Json" Version="13.0.1" />
```

#### 9.2 Remove from Project Files
Remove any `<PackageReference Include="Newtonsoft.Json" />` from:
- `src/DurableTask.Core/DurableTask.Core.csproj`
- Any other project files

### 10. Build and Test

#### 10.1 Verify No Newtonsoft References Remain
```bash
grep -r "Newtonsoft.Json" --include="*.cs" --include="*.csproj" .
# Should return no results
```

#### 10.2 Full Build
```bash
dotnet clean
dotnet restore
dotnet build --configuration Release
```

#### 10.3 Run All Tests
```bash
dotnet test --configuration Release --no-build
```

#### 10.4 Run Sample Applications
Test each sample to ensure they work correctly:
```bash
cd samples/Correlation.Samples
dotnet run
# Verify output matches expected behavior
```

### 11. Verify Serialization Compatibility

#### 11.1 Create Compatibility Tests
If backward compatibility with existing serialized data is required, create tests:

```csharp
[TestMethod]
public void SystemTextJson_CanDeserialize_NewtonsoftJson_Format()
{
    // JSON serialized with Newtonsoft.Json
    string newtonsoftJson = @"{""Property"":""Value""}";
    
    // Should deserialize with System.Text.Json
    var obj = JsonSerializer.Deserialize<MyType>(newtonsoftJson, JsonSettings.Default);
    
    Assert.IsNotNull(obj);
    Assert.AreEqual("Value", obj.Property);
}
```

### 12. Update Documentation

Update any documentation that mentions JSON serialization:
- README.md
- API documentation
- Migration guides

### 13. Performance Testing (Optional but Recommended)

System.Text.Json is generally faster than Newtonsoft.Json, but verify:
- Serialization performance hasn't regressed
- Memory usage is acceptable
- Large payload handling works correctly

## Common Issues and Solutions

### Issue 1: Case Sensitivity
**Problem:** Properties not deserializing
**Solution:** Use `PropertyNameCaseInsensitive = true` in options

### Issue 2: Missing Properties
**Problem:** Optional properties cause deserialization to fail
**Solution:** Use nullable types and `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`

### Issue 3: Circular References
**Problem:** Object graph has circular references
**Solution:** Use `ReferenceHandler.Preserve` in JsonSerializerOptions

### Issue 4: Dynamic/Unknown Types
**Problem:** Need to handle unknown types at runtime
**Solution:** Use JsonDocument for dynamic parsing, or implement custom factory pattern

## Expected Outcome

After completing this phase:
- [ ] No Newtonsoft.Json references remain in code or project files
- [ ] All serialization uses System.Text.Json
- [ ] All tests pass
- [ ] Sample applications work correctly
- [ ] Serialization format is compatible (or breaking changes are documented)
- [ ] Performance is acceptable
- [ ] Ready for Phase 3 (Nullable reference types)

## Rollback Plan

If critical issues are found:
- Revert changes using Git
- Document specific incompatibilities
- Consider hybrid approach (both libraries temporarily)
- Evaluate if certain components need to keep Newtonsoft.Json

## Notes for Agent

- This phase is complex and high-risk - test thoroughly
- Start with simple files and build confidence
- Create the JsonSettings helper class first
- Convert and test one file at a time if issues arise
- Pay special attention to storage formats that may have persisted data
- Document any serialization format changes
- Consider creating a compatibility layer if needed
