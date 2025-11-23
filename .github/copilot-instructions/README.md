# DurableTask Framework Modernization Guide

This directory contains comprehensive instructions for modernizing the DurableTask Framework codebase to .NET 8/10 with modern C# features.

## Overview

The modernization is split into **4 phases**, each designed to be completed in a single GitHub Copilot Agent session. Each phase builds on the previous one and can be completed independently.

**Total Scope:**
- 368 source files (~60K lines of code)
- 6 main library projects
- Multiple test and sample projects
- Current: .NET Framework 4.x, .NET 6, netstandard2.0, C# 9.0, Newtonsoft.Json
- Target: .NET 8 & 10, C# latest, System.Text.Json, Nullable enabled, Modern C# features

## Phases

### Phase 1: Upgrade to .NET 8 & .NET 10, Remove .NET Framework 4.x
**File:** [PHASE-1-upgrade-dotnet.md](./PHASE-1-upgrade-dotnet.md)

**Objective:** Upgrade all projects from .NET Framework 4.x (net462, net472, net451) and .NET 6 to .NET 8 and .NET 10.

**Changes:**
- Update all TargetFramework/TargetFrameworks to net8.0 and net10.0
- Remove .NET Framework specific code and dependencies
- Update package versions for .NET 8/10 compatibility
- Set LangVersion to "latest"

**Estimated Time:** 4-6 hours  
**Risk Level:** Medium (breaking changes, but straightforward)  
**Dependencies:** None  
**Output:** All projects target .NET 8 and .NET 10 only

---

### Phase 2: Replace Newtonsoft.Json with System.Text.Json
**File:** [PHASE-2-replace-newtonsoft-json.md](./PHASE-2-replace-newtonsoft-json.md)

**Objective:** Replace all Newtonsoft.Json functionality with System.Text.Json throughout the codebase.

**Changes:**
- Remove Newtonsoft.Json package references
- Replace JsonConvert, JObject, JToken with System.Text.Json equivalents
- Create common JsonSerializerOptions configuration
- Update custom converters
- Handle serialization compatibility

**Estimated Time:** 6-8 hours  
**Risk Level:** High (serialization behavior changes possible)  
**Dependencies:** Phase 1 must be completed  
**Output:** No Newtonsoft.Json dependencies, all code uses System.Text.Json

---

### Phase 3: Enable Nullable Reference Types
**File:** [PHASE-3-enable-nullable.md](./PHASE-3-enable-nullable.md)

**Objective:** Enable nullable reference types project-wide and update code to properly annotate nullability.

**Changes:**
- Enable `<Nullable>enable</Nullable>` in DurableTask.props
- Annotate all reference types with proper nullability
- Fix all nullable warnings (CS8600-CS8625)
- Initialize non-nullable properties
- Update public APIs to express nullability contracts

**Estimated Time:** 8-12 hours  
**Risk Level:** Medium (requires understanding of code semantics)  
**Dependencies:** Phase 1 and 2 must be completed  
**Output:** No nullable warnings, safer code with compile-time null checking

---

### Phase 4: Modernize C# Code with Latest Features
**File:** [PHASE-4-modernize-csharp.md](./PHASE-4-modernize-csharp.md)

**Objective:** Update codebase to use modern C# features (C# 10-13).

**Changes:**
- Convert to file-scoped namespaces
- Apply primary constructors where appropriate
- Use required properties for DTOs
- Convert immutable data to records
- Use collection expressions
- Modernize switch statements to expressions
- Add global usings
- Apply other modern patterns

**Estimated Time:** 6-10 hours  
**Risk Level:** Low (code style improvements, no behavior changes)  
**Dependencies:** Phase 1, 2, and 3 must be completed  
**Output:** Modern, readable C# code using latest language features

---

## Execution Instructions for GitHub Copilot Agent

### Prerequisites
- .NET 8 and .NET 10 SDKs must be installed
- Access to Azure Storage Emulator (for running tests)
- Git repository cloned and on a feature branch

### How to Use These Instructions

Each phase is designed to be completed in a **single Copilot Agent session**. Follow these steps:

#### 1. Start with Phase 1
```
@workspace Please follow the instructions in .github/copilot-instructions/PHASE-1-upgrade-dotnet.md 
to upgrade all projects to .NET 8 and .NET 10 and remove .NET Framework 4.x compatibility.
```

#### 2. Verify Phase 1 Completion
After Phase 1 is complete, verify:
- [ ] All projects build successfully
- [ ] Tests pass (or failures are documented as unrelated)
- [ ] No .NET Framework references remain
- [ ] All projects target net8.0 and net10.0

#### 3. Continue with Phase 2
```
@workspace Please follow the instructions in .github/copilot-instructions/PHASE-2-replace-newtonsoft-json.md
to replace Newtonsoft.Json with System.Text.Json throughout the codebase.
```

#### 4. Verify Phase 2 Completion
After Phase 2 is complete, verify:
- [ ] All projects build successfully  
- [ ] Tests pass
- [ ] No Newtonsoft.Json references remain
- [ ] Serialization works correctly

#### 5. Continue with Phase 3
```
@workspace Please follow the instructions in .github/copilot-instructions/PHASE-3-enable-nullable.md
to enable nullable reference types and update code to properly handle nullability.
```

#### 6. Verify Phase 3 Completion
After Phase 3 is complete, verify:
- [ ] All projects build without nullable warnings
- [ ] Tests pass
- [ ] Public APIs correctly express nullability

#### 7. Continue with Phase 4
```
@workspace Please follow the instructions in .github/copilot-instructions/PHASE-4-modernize-csharp.md
to modernize the C# code with latest language features.
```

#### 8. Verify Phase 4 Completion
After Phase 4 is complete, verify:
- [ ] All projects build successfully
- [ ] Tests pass
- [ ] Code uses modern C# patterns consistently
- [ ] No behavior changes introduced

### Alternative: Execute All Phases Together

If you want to attempt all phases in one go (not recommended due to complexity):

```
@workspace Please follow all four modernization phases in sequence:
1. PHASE-1-upgrade-dotnet.md
2. PHASE-2-replace-newtonsoft-json.md  
3. PHASE-3-enable-nullable.md
4. PHASE-4-modernize-csharp.md

Complete each phase fully before moving to the next, and verify builds/tests pass after each phase.
```

## Important Guidelines

### For All Phases

1. **Build Frequently**
   - Build after each significant change
   - Don't accumulate errors
   - Use `dotnet build --configuration Release`

2. **Test Frequently**
   - Run tests after each file or small group of files
   - Use `dotnet test --configuration Release`
   - Some tests may require Azure Storage Emulator

3. **Commit Frequently**
   - Commit after completing each major step
   - Use descriptive commit messages
   - Don't create one giant commit

4. **Make Minimal Changes**
   - Only change what's necessary for the phase
   - Don't fix unrelated issues
   - Don't add new features

5. **Handle Errors Properly**
   - Build errors must be fixed before proceeding
   - Document test failures if they're unrelated
   - Ask for help if stuck

### Phase-Specific Notes

**Phase 1 (Framework Upgrade)**
- ServiceFabric packages may need special attention
- Some packages may not have .NET 8/10 versions
- Azure Storage Emulator required for tests

**Phase 2 (JSON Replacement)**
- Test serialization thoroughly
- Watch for breaking changes in format
- Custom converters need careful rewriting

**Phase 3 (Nullable)**
- This takes the longest
- Don't over-use null-forgiving operator (!)
- Think about the contracts you're expressing

**Phase 4 (Modern C#)**
- Don't overuse new features
- Primary constructors aren't always better
- Keep code readable

## Rollback Strategy

If any phase encounters insurmountable issues:

1. **Stop the current phase**
2. **Document the blocking issue** clearly
3. **Revert changes** using Git:
   ```bash
   git reset --hard HEAD~n  # n = number of commits to undo
   ```
4. **Report the issue** with:
   - What was being attempted
   - What error occurred
   - What was tried to fix it
   - Suggestions for alternative approaches

## Testing Strategy

### Unit Tests
Run after each file or small group of files:
```bash
dotnet test test/DurableTask.Core.Tests --configuration Release
dotnet test test/DurableTask.AzureStorage.Tests --configuration Release
```

### Integration Tests
Run after completing major components:
```bash
dotnet test --configuration Release --filter "TestCategory=Integration"
```

### Full Test Suite
Run after completing each phase:
```bash
dotnet test --configuration Release
```

### Sample Applications
Test samples after Phases 1, 2, and 4:
```bash
cd samples/Correlation.Samples
dotnet run
# Verify expected behavior
```

## Success Criteria

### Phase 1 Success
- ✅ All projects target net8.0 and net10.0
- ✅ No net462, net472, net451, netstandard2.0 targets remain
- ✅ Solution builds clean
- ✅ Tests pass (or failures documented)

### Phase 2 Success
- ✅ No Newtonsoft.Json package references
- ✅ No `using Newtonsoft.Json` statements
- ✅ All serialization uses System.Text.Json
- ✅ Tests pass including serialization tests

### Phase 3 Success
- ✅ `<Nullable>enable</Nullable>` in DurableTask.props
- ✅ No CS8xxx nullable warnings
- ✅ Public APIs properly annotated
- ✅ Tests pass

### Phase 4 Success
- ✅ File-scoped namespaces throughout
- ✅ Appropriate use of modern features
- ✅ Code is more readable
- ✅ No behavior changes
- ✅ Tests pass

### Overall Success
- ✅ All 4 phases completed
- ✅ Solution builds without warnings
- ✅ All tests pass
- ✅ Samples work correctly
- ✅ Code is modern, safe, and maintainable
- ✅ Documentation updated

## Post-Modernization

After completing all phases:

1. **Update Documentation**
   - README.md with new requirements
   - API documentation
   - Migration guides for users

2. **Create Release Notes**
   - Document breaking changes
   - Highlight improvements
   - Provide upgrade guidance

3. **Performance Testing**
   - Benchmark critical paths
   - Compare with previous version
   - Document any improvements

4. **Security Review**
   - Verify no vulnerabilities introduced
   - Check dependency versions
   - Run security scanning tools

## Getting Help

If you encounter issues during modernization:

1. **Check the phase-specific file** for troubleshooting guidance
2. **Review common issues** section in each phase
3. **Check .NET upgrade guides** at https://docs.microsoft.com/dotnet
4. **Consult C# language docs** at https://docs.microsoft.com/dotnet/csharp
5. **Ask the repository maintainers** if stuck on architecture decisions

## Timeline

**Realistic Timeline for Full Modernization:**
- Phase 1: 4-6 hours
- Phase 2: 6-8 hours
- Phase 3: 8-12 hours
- Phase 4: 6-10 hours
- Testing & Verification: 4-6 hours
- Documentation: 2-3 hours

**Total: 30-45 hours** of focused work

Can be split across multiple days/weeks with each phase as a milestone.

## Contributing

When contributing improvements to these instructions:

1. Keep instructions clear and actionable
2. Provide concrete examples
3. Document risks and mitigations
4. Include verification steps
5. Update this README if adding new phases

## License

These instructions are part of the DurableTask Framework project and follow the same Apache 2.0 license.

---

**Last Updated:** 2024-11-23  
**Author:** GitHub Copilot Agent  
**Status:** Ready for execution
