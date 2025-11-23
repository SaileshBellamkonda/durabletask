# Quick Start Guide - DurableTask Modernization

This is a quick reference for executing the modernization phases. For detailed instructions, see [README.md](./README.md).

## Phase Order (Must be executed sequentially)

```
Phase 1 → Phase 2 → Phase 3 → Phase 4
```

## Phase 1: .NET 8/10 Upgrade

**Command for Copilot Agent:**
```
@workspace Follow the instructions in .github/copilot-instructions/PHASE-1-upgrade-dotnet.md 
to upgrade all projects to .NET 8 and .NET 10 and remove .NET Framework 4.x compatibility.
```

**Verification:**
```bash
dotnet build --configuration Release
dotnet test --configuration Release
grep -r "net462\|net472\|net451\|netstandard2.0" --include="*.csproj" .
```

**Expected:** No .NET Framework targets, clean build, tests pass

---

## Phase 2: System.Text.Json Migration

**Command for Copilot Agent:**
```
@workspace Follow the instructions in .github/copilot-instructions/PHASE-2-replace-newtonsoft-json.md
to replace Newtonsoft.Json with System.Text.Json throughout the codebase.
```

**Verification:**
```bash
dotnet build --configuration Release
dotnet test --configuration Release
grep -r "Newtonsoft.Json" --include="*.cs" --include="*.csproj" .
```

**Expected:** No Newtonsoft.Json references, clean build, tests pass

---

## Phase 3: Nullable Reference Types

**Command for Copilot Agent:**
```
@workspace Follow the instructions in .github/copilot-instructions/PHASE-3-enable-nullable.md
to enable nullable reference types and update code to properly handle nullability.
```

**Verification:**
```bash
dotnet build --configuration Release 2>&1 | grep -i "CS8"
dotnet test --configuration Release
```

**Expected:** No CS8xxx warnings, clean build, tests pass

---

## Phase 4: Modern C# Features

**Command for Copilot Agent:**
```
@workspace Follow the instructions in .github/copilot-instructions/PHASE-4-modernize-csharp.md
to modernize the C# code with latest language features.
```

**Verification:**
```bash
dotnet build --configuration Release
dotnet test --configuration Release
```

**Expected:** Modern code patterns, clean build, tests pass

---

## One-Shot Command (Not Recommended)

If you want to attempt all phases at once:

```
@workspace Execute all four modernization phases in sequence:
1. .github/copilot-instructions/PHASE-1-upgrade-dotnet.md
2. .github/copilot-instructions/PHASE-2-replace-newtonsoft-json.md
3. .github/copilot-instructions/PHASE-3-enable-nullable.md
4. .github/copilot-instructions/PHASE-4-modernize-csharp.md

Complete each phase fully, verify builds/tests pass, and commit after each phase before proceeding to the next.
```

**Warning:** This is complex and may exceed a single session's capabilities. Phases 1-3 are better done separately.

---

## Key Files to Check

After each phase, verify these key files:

### Configuration Files
- `Directory.Packages.props` - Package versions
- `tools/DurableTask.props` - Common build properties
- `src/*/DurableTask.*.csproj` - Project files

### Source Projects
- `src/DurableTask.Core/` - Core functionality
- `src/DurableTask.AzureStorage/` - Azure Storage provider
- `src/DurableTask.ServiceBus/` - Service Bus provider
- `src/DurableTask.Emulator/` - Emulator
- `src/DurableTask.ApplicationInsights/` - App Insights integration
- `src/DurableTask.AzureServiceFabric/` - Service Fabric provider

### Test Projects
- `test/DurableTask.Core.Tests/`
- `test/DurableTask.AzureStorage.Tests/`
- `test/DurableTask.ServiceBus.Tests/`

---

## Common Commands

### Build
```bash
dotnet clean
dotnet restore
dotnet build --configuration Release
```

### Test
```bash
dotnet test --configuration Release --no-build
```

### Find Issues
```bash
# Find remaining .NET Framework targets
grep -r "net462\|net472" --include="*.csproj" .

# Find Newtonsoft.Json usage
grep -r "Newtonsoft.Json" --include="*.cs" .

# Find nullable warnings
dotnet build 2>&1 | grep "CS8"

# Count lines changed
git diff --stat
```

---

## Rollback

If a phase fails:

```bash
# See what changed
git status
git diff

# Undo uncommitted changes
git checkout .

# Undo last N commits (preserve changes)
git reset --soft HEAD~N

# Undo last N commits (discard changes)
git reset --hard HEAD~N
```

---

## Success Checklist

After all phases:

- [ ] All projects target net8.0 and net10.0
- [ ] No Newtonsoft.Json dependencies
- [ ] Nullable enabled, no warnings
- [ ] Modern C# features used throughout
- [ ] `dotnet build --configuration Release` succeeds
- [ ] `dotnet test --configuration Release` succeeds
- [ ] Sample applications run correctly
- [ ] Documentation updated

---

## Estimated Timeline

- **Phase 1:** 4-6 hours
- **Phase 2:** 6-8 hours  
- **Phase 3:** 8-12 hours
- **Phase 4:** 6-10 hours
- **Total:** 24-36 hours + testing time

---

## Getting Help

1. Check the detailed phase instructions
2. Review the main [README.md](./README.md)
3. Search for specific error messages
4. Consult .NET and C# documentation
5. Ask repository maintainers

---

**Quick Links:**
- [Master README](./README.md)
- [Phase 1 Details](./PHASE-1-upgrade-dotnet.md)
- [Phase 2 Details](./PHASE-2-replace-newtonsoft-json.md)
- [Phase 3 Details](./PHASE-3-enable-nullable.md)
- [Phase 4 Details](./PHASE-4-modernize-csharp.md)
