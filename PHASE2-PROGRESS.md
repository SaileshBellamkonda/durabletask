# Phase 2 Progress: System.Text.Json Migration

## Summary

**Status:** IN PROGRESS - 19% Complete (7/37 files)
**Core Library:** 37% Complete (7/19 files)

## Completed Work

### Step 1: Package References ✅
- System.Text.Json 8.0.5 already available in Directory.Packages.props

### Step 2: JsonSettings Helper ✅
- Created `src/DurableTask.Core/Serialization/JsonSettings.cs`
- Provides `JsonSettings.Default` and `JsonSettings.Indented`
- Configured to match Newtonsoft.Json defaults:
  - PascalCase property naming
  - Enums as strings
  - Case-insensitive deserialization

### Step 3-4: Simple File Conversions ✅

**Files Converted (7):**

1. **FailureDetails.cs**
   - Changed: using Newtonsoft.Json → System.Text.Json.Serialization
   - Kept: [JsonConstructor] attribute (compatible with both)

2. **OrchestratorExecutionResult.cs**
   - Changed: [JsonProperty] → [JsonPropertyName]
   - Changed: using statement

3. **Tracing/DistributedTraceActivity.cs**
   - Removed: Unused Newtonsoft.Json using statement

4. **Tracing/TraceHelper.cs**
   - Removed: Unused Newtonsoft.Json using statement

5. **Entities/Serializer.cs**
   - Changed: JsonSerializer/JsonSerializerSettings → JsonSerializerOptions
   - Created: InternalSerializerOptions (Lazy<JsonSerializerOptions>)

6. **Entities/EntityMessageEvent.cs**
   - Changed: JsonConvert.SerializeObject → JsonSerializer.Serialize
   - Updated: Uses Serializer.InternalSerializerOptions.Value

7. **Entities/ClientEntityHelpers.cs**
   - Changed: JsonConvert.DeserializeObject → JsonSerializer.Deserialize
   - Updated: Uses Serializer.InternalSerializerOptions.Value

## Remaining Work

### DurableTask.Core Files (12 remaining)

#### High Priority - Complex Patterns

**1. TaskEntityDispatcher.cs** (6 Newtonsoft usages)
- 2x JsonConvert.SerializeObject → straightforward
- 3x JsonConvert.PopulateObject → needs workaround
- Challenge: PopulateObject doesn't have direct System.Text.Json equivalent
- Solution: Deserialize to temp, manually copy OR use JsonDocument

**2. OrchestrationEntityContext.cs** (1 usage)
- 1x JsonConvert.PopulateObject → needs workaround
- Same challenge as above

**3. Command/OrchestratorActionConverter.cs**
- Custom JsonConverter extending JsonCreationConverter<T>
- Uses JObject for type discrimination
- Challenge: Complete rewrite needed for System.Text.Json.Serialization.JsonConverter<T>
- Needs: Read type discriminator, create appropriate derived type

**4. Entities/OperationFormat/OperationActionConverter.cs**
- Same pattern as OrchestratorActionConverter
- Custom converter with JObject usage

**5. Serializing/JsonCreationConverter.cs**
- Base class for custom converters
- Uses JObject
- Challenge: Abstract base class, affects multiple converters
- Solution: Rewrite as JsonConverter<T> base class using JsonDocument

**6. Serializing/JsonDataConverter.cs** ⚠️ **MOST COMPLEX**
- Core serialization class used by entire framework
- Uses TypeNameHandling.Objects for polymorphic serialization
- Uses JsonSerializer/JsonTextWriter directly
- Challenge: No direct TypeNameHandling equivalent in System.Text.Json
- Solution Options:
  a) JsonDerivedType attributes (C# 11+)
  b) Custom type discriminator implementation
  c) Source generation with known types
- **CRITICAL:** This affects serialization format compatibility

**7. Serializing/PackageUpgradeSerializationBinder.cs**
- Extends DefaultSerializationBinder from Newtonsoft
- Used for backward compatibility with v1.0/v2.0 serialization
- Challenge: No SerializationBinder concept in System.Text.Json
- Solution: May need custom TypeInfoResolver or keep hybrid approach

#### Medium Priority - JObject/JToken Usage

**8. Common/Utils.cs**
- 3x JObject usage
- Solution: Replace with JsonDocument/JsonElement or JsonObject

**9. ReflectionBasedTaskActivity.cs**
- 4x JObject usage  
- Solution: Replace with JsonDocument/JsonElement

**10. TraceContextBase.cs**
- 1x JObject usage
- Solution: Replace with JsonDocument/JsonElement

**11. TaskActivity.cs**
- 2x JObject usage
- Solution: Replace with JsonDocument/JsonElement

**12. Command/OrchestratorAction.cs**
- Uses [JsonConverter(typeof(OrchestrationActionConverter))]
- Depends on OrchestratorActionConverter conversion

### Other Projects (18 files)

Not yet analyzed:
- DurableTask.Emulator
- DurableTask.ApplicationInsights
- DurableTask.AzureServiceFabric
- Test projects
- Sample projects

## Technical Challenges

### 1. JsonConvert.PopulateObject
**Problem:** No direct equivalent in System.Text.Json for .NET 8+

**Workarounds:**
```csharp
// Option A: Deserialize and assign
var obj = JsonSerializer.Deserialize<T>(json, options) ?? new T();

// Option B: Manual population using JsonDocument
using var doc = JsonDocument.Parse(json);
foreach (var prop in doc.RootElement.EnumerateObject()) {
    // Set property via reflection or property access
}
```

### 2. JObject/JToken
**Problem:** Dynamic JSON manipulation

**Solution:**
```csharp
// Read-only: JsonDocument/JsonElement
using var doc = JsonDocument.Parse(json);
var value = doc.RootElement.GetProperty("name").GetString();

// Mutable: JsonObject/JsonNode (.NET 6+)
var obj = JsonNode.Parse(json) as JsonObject;
obj["name"] = "newValue";
```

### 3. Custom JsonConverter
**Problem:** Different API in System.Text.Json

**Conversion Pattern:**
```csharp
// Old (Newtonsoft)
public class MyConverter : JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) { }
    public override bool CanConvert(Type objectType) { }
}

// New (System.Text.Json)
public class MyConverter : JsonConverter<MyType> {
    public override void Write(Utf8JsonWriter writer, MyType value, JsonSerializerOptions options) { }
    public override MyType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) { }
}
```

### 4. TypeNameHandling.Objects
**Problem:** Polymorphic serialization - no built-in equivalent

**Solution Options:**

**A. JsonDerivedType Attributes (C# 11+, .NET 7+):**
```csharp
[JsonDerivedType(typeof(DerivedClass1), "type1")]
[JsonDerivedType(typeof(DerivedClass2), "type2")]
public abstract class BaseClass { }
```

**B. Custom Type Discriminator:**
```csharp
public class PolymorphicConverter<T> : JsonConverter<T> {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using var doc = JsonDocument.ParseValue(ref reader);
        var typeDiscriminator = doc.RootElement.GetProperty("$type").GetString();
        var actualType = Type.GetType(typeDiscriminator);
        return (T)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), actualType, options);
    }
}
```

## Next Steps

### Immediate (Continue Phase 2)
1. Convert PopulateObject usages (TaskEntityDispatcher, OrchestrationEntityContext)
2. Convert simple JObject usage (Utils, TaskActivity, etc.)
3. Rewrite JsonCreationConverter base class
4. Rewrite specific converters (OrchestratorActionConverter, OperationActionConverter)
5. Tackle JsonDataConverter (most complex)
6. Convert other projects (Emulator, ApplicationInsights, AzureServiceFabric)
7. Update tests
8. Remove Newtonsoft.Json package references
9. Full build and test

### Risks & Considerations

**High Risk:**
- JsonDataConverter change affects ALL serialization
- Backward compatibility with persisted data
- TypeNameHandling removal may break existing workflows

**Medium Risk:**
- Custom converter changes may affect performance
- Subtle serialization format differences

**Low Risk:**
- Simple conversions (already done)
- Using statement updates

## Build Status

**Current:** DurableTask.Core has errors due to incomplete conversion
**Expected:** Build will pass after TaskEntityDispatcher and converters are fixed
**Goal:** All 4 successfully building projects continue to build

## Estimated Remaining Time

Based on Phase 2 instructions (6-8 hours total):
- Completed: ~2 hours (simple conversions)
- Remaining: ~4-6 hours
  - PopulateObject conversions: 30 min
  - JObject/JToken conversions: 1-2 hours
  - Custom converter rewrites: 2-3 hours
  - JsonDataConverter: 1-2 hours (most complex)
  - Other projects: 30 min
  - Testing and fixes: 30 min

## Compatibility Notes

**Breaking Changes:**
- Serialization format MAY change for:
  - Enum handling (should be compatible - both use strings with our settings)
  - DateTime format (should be compatible - both use ISO 8601)
  - Null handling (configured to match)
  - Property casing (configured to match)
  - Polymorphic types (TypeNameHandling.Objects → needs implementation)

**Non-Breaking:**
- [JsonConstructor] attribute works with both
- [JsonPropertyName] is equivalent to [JsonProperty]
- Case-insensitive deserialization provides more compatibility

## Testing Strategy

After conversion:
1. Unit tests for serialization/deserialization
2. Integration tests for workflow execution
3. Compatibility tests with old serialized data
4. Performance benchmarks (System.Text.Json should be faster)

## Documentation Needed

- Update README with System.Text.Json usage
- Document serialization format changes (if any)
- Migration guide for custom converters (if public API)
- Release notes with breaking changes
