# Phase 1: Upgrade to .NET 8 & .NET 10, Remove .NET Framework 4.x Compatibility

## Objective
Upgrade all projects from .NET Framework 4.x (net462, net472, net451) and .NET 6 to .NET 8 and .NET 10. Remove all .NET Framework compatibility.

## Pre-requisites
- Verify .NET 8 and .NET 10 SDKs are installed: `dotnet --list-sdks`
- The environment should have .NET 8.0.x and 10.0.x SDKs available

## Important Notes
- This is a BREAKING CHANGE that removes .NET Framework support
- All projects will target .NET 8 and .NET 10 only
- Make minimal changes - only modify what's necessary for the framework upgrade
- Build and test after each major step
- Some projects may only need to target .NET 10 (samples that don't need multi-targeting)

## Step-by-Step Instructions

### 1. Update Global Configuration Files

#### 1.1 Update `tools/DurableTask.props`
```xml
<!-- Change this line -->
<LangVersion>9.0</LangVersion>

<!-- To this -->
<LangVersion>latest</LangVersion>
```

#### 1.2 Update `Directory.Packages.props`
- Remove all conditional package versions for `net462`, `net472`, and `net451`
- Remove `net462` and `net472` specific ItemGroups entirely
- Update package versions that have .NET Framework-specific versions to their latest .NET 8+ compatible versions
- Key packages to update/remove:
  - Remove: `ImpromptuInterface` net462/net472 conditions
  - Remove: `Microsoft.Azure.KeyVault.Core` (net462 only)
  - Remove: `Microsoft.Data.Edm`, `Microsoft.Data.OData`, `Microsoft.WindowsAzure.ConfigurationManager`, `System.Spatial` (net462 only)
  - Remove: `WindowsAzure.ServiceBus` (net462 only)
  - Remove: `WindowsAzure.Storage` (all versions)
  - Remove: `CommandLineParser` net462 version condition
  - Remove: `EnterpriseLibrary.SemanticLogging.TextFile` (net462 only)
  - Remove: `Microsoft.Tpl.Dataflow` (net462 only)
  - Update: `Microsoft.Extensions.Logging*` packages to version 8.0.0 for net8.0 and 10.0.0 for net10.0
  - Update: `System.Text.Json` to version 8.0.0 or later

### 2. Update Source Projects (`src/` directory)

#### 2.1 `src/DurableTask.Core/DurableTask.Core.csproj`
**Current:** `<TargetFramework>netstandard2.0</TargetFramework>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

#### 2.2 `src/DurableTask.AzureStorage/DurableTask.AzureStorage.csproj`
**Current:** `<TargetFramework>netstandard2.0</TargetFramework>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

#### 2.3 `src/DurableTask.ServiceBus/DurableTask.ServiceBus.csproj`
**Current:** `<TargetFrameworks>netstandard2.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```
- Remove all `Condition="'$(TargetFramework)' == 'net462'"` ItemGroups
- Remove all `Condition="'$(TargetFramework)' == 'netstandard2.0'"` conditions

#### 2.4 `src/DurableTask.Emulator/DurableTask.Emulator.csproj`
**Current:** `<TargetFrameworks>netstandard2.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

#### 2.5 `src/DurableTask.ApplicationInsights/DurableTask.ApplicationInsights.csproj`
**Current:** `<TargetFrameworks>netstandard2.0</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

#### 2.6 `src/DurableTask.AzureServiceFabric/DurableTask.AzureServiceFabric.csproj`
**Current:** `<TargetFrameworks>net462;net472</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```
- Remove all `Condition="'$(TargetFramework)' == 'net462'"` and `net472` ItemGroups
- Review ServiceFabric package compatibility with .NET 8/10 - may need package updates

### 3. Update Test Projects (`test/` directory)

#### 3.1 `test/DurableTask.Core.Tests/DurableTask.Core.Tests.csproj`
**Current:** `<TargetFrameworks>net6.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

#### 3.2 `test/DurableTask.AzureStorage.Tests/DurableTask.AzureStorage.Tests.csproj`
**Current:** `<TargetFrameworks>net6.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```
- Remove all .NET Framework conditional ItemGroups

#### 3.3 `test/DurableTask.ServiceBus.Tests/DurableTask.ServiceBus.Tests.csproj`
**Current:** `<TargetFrameworks>net6.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```
- Remove all .NET Framework conditional blocks

#### 3.4 `test/DurableTask.Emulator.Tests/DurableTask.Emulator.Tests.csproj`
**Current:** `<TargetFrameworks>net6.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

#### 3.5 `test/DurableTask.Stress.Tests/DurableTask.Stress.Tests.csproj`
**Current:** `<TargetFrameworks>net6.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```
- Remove all .NET Framework conditional blocks

#### 3.6 `test/DurableTask.Test.Orchestrations/DurableTask.Test.Orchestrations.csproj`
**Current:** `<TargetFrameworks>netstandard2.0;net462</TargetFrameworks>`
**Change to:**
```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

#### 3.7 `test/DurableTask.Samples.Tests/DurableTask.Samples.Tests.csproj`
**Current:** `<TargetFramework>net451</TargetFramework>`
**Action:** Either delete this project or upgrade to `net10.0` if still needed

#### 3.8 ServiceFabric Test Projects
- `test/DurableTask.AzureServiceFabric.Tests/DurableTask.AzureServiceFabric.Tests.csproj`
- `test/DurableTask.AzureServiceFabric.Integration.Tests/DurableTask.AzureServiceFabric.Integration.Tests.csproj`
- `test/TestFabricApplication/` projects

**Action:** These require Service Fabric compatibility. Either:
- Remove these projects if Service Fabric is no longer supported
- OR upgrade to net8.0/net10.0 and update Service Fabric packages to compatible versions

### 4. Update Sample Projects (`samples/` directory)

#### 4.1 `samples/Correlation.Samples/Correlation.Samples.csproj`
**Current:** `<TargetFramework>net6.0</TargetFramework>`
**Change to:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

#### 4.2 `samples/DistributedTraceSample/ApplicationInsights/ApplicationInsightsSample.csproj`
**Current:** `<TargetFramework>net6.0</TargetFramework>`
**Change to:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

#### 4.3 `samples/DistributedTraceSample/OpenTelemetry/OpenTelemetrySample.csproj`
**Current:** `<TargetFramework>net6.0</TargetFramework>`
**Change to:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

#### 4.4 `samples/DurableTask.Samples/DurableTask.Samples.csproj`
**Current:** `<TargetFramework>net462</TargetFramework>`
**Change to:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

#### 4.5 ManagedIdentity Samples
Review and update both v1.x and v2.x samples to target net10.0

### 5. Build and Verify

#### 5.1 Clean Build
```bash
dotnet clean
rm -rf build_output/
```

#### 5.2 Restore Packages
```bash
dotnet restore
```

#### 5.3 Build Solution
```bash
dotnet build --configuration Release
```

#### 5.4 Run Tests
```bash
dotnet test --configuration Release --no-build
```

### 6. Handle Build Errors

Common issues to fix:
- **API compatibility:** Some .NET Framework-specific APIs may not exist in .NET 8/10
  - Check for `System.Configuration` usage - replace with `Microsoft.Extensions.Configuration`
  - Check for `System.Web` usage - remove or find alternatives
  - Check for `AppDomain` usage - may need refactoring
  
- **Package compatibility:** Some packages may not support .NET 8/10
  - Update to latest versions
  - Find alternative packages if needed
  
- **Service Fabric:** May need special handling
  - Check Microsoft.ServiceFabric package compatibility
  - May need to use preview packages or community alternatives

### 7. Update Documentation

Update `README.md` to reflect:
- Minimum requirements: .NET 8 or .NET 10
- Remove .NET Framework references
- Update build instructions

### 8. Final Verification

- [ ] All projects build successfully
- [ ] All tests pass (or failing tests are unrelated to framework upgrade)
- [ ] NuGet packages can be generated successfully
- [ ] Sample projects run successfully
- [ ] No .NET Framework references remain in any .csproj files
- [ ] No .NET Framework conditional compilation remains

## Expected Outcome

After completing this phase:
- All projects target .NET 8 and .NET 10 only
- No .NET Framework 4.x support remains
- All projects use C# latest language version
- Solution builds and tests pass on .NET 8 and .NET 10
- Ready for Phase 2 (Newtonsoft.Json replacement)

## Rollback Plan

If issues are found:
- Revert changes using Git
- Document blocking issues
- Consider a phased approach targeting .NET 8 first, then .NET 10

## Notes for Agent

- Take your time with this phase - it's foundational for all other phases
- Build frequently to catch issues early
- Some tests may fail due to missing dependencies (Azure Storage Emulator, Service Bus) - this is expected
- Document any packages that cannot be upgraded and why
- If Service Fabric support is critical, consult with maintainers before removing
