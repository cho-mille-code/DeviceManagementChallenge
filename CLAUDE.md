# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**DeviceManagementChallenge** is an ASP.NET Core 10 minimal API challenge/exercise for managing device inventory. The scaffolding is in place but the implementation is intentionally incomplete — this is a coding challenge.

**Technology Stack**
- .NET 10.0, ASP.NET Core Web SDK (minimal API style in `Program.cs`)
- xUnit 2.9 with coverlet for coverage
- FluentValidation expected by tests (not yet installed)
- OpenAPI via `Microsoft.AspNetCore.OpenApi`

## Build and Test Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~SerialNumber_NotAGuid_FailsValidation"

# Run tests with verbose output
dotnet test --verbosity detailed

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run the API (HTTP :5224, HTTPS :7009)
dotnet run --project DeviceManagement
```

OpenAPI JSON is served at `/openapi/v1.json` in Development.

## Architecture

Two projects in one solution (`DeviceManagementChallenge.slnx`):
- `DeviceManagement/` — the web API (entry point: `Program.cs`)
- `DeviceManagement.Tests/` — xUnit tests referencing the main project

### What is implemented vs. what is missing

**Implemented:**
- `Device` class in `DeviceManagement/Models/Device.cs` with DataAnnotations (`[Required]`, `[EmailAddress]`)

**Not yet implemented (the challenge work):**
- `DeviceType` and `Status` are typed as `Enum` (the base class) — concrete enums need to be defined
- `Device` is a `class` but tests use the `with` keyword (requires `record`)
- `_validator` field in `UnitTest1` — a FluentValidation `AbstractValidator<Device>` must be created and wired up
- `ValidRequest()` helper method returning a valid `Device` fixture
- `ShouldHaveValidationErrorFor()` calls require `FluentValidation.TestHelper` NuGet package
- `FluentValidation` and `FluentValidation.TestHelper` packages are not yet added to the test project
- `Program.cs` still contains the auto-generated WeatherForecast placeholder; device endpoints need to be added

### Test expectations

`UnitTest1.cs` tests validate:
- `SerialNumber` must be a valid Guid
- `PrimaryUser` must be a valid email address
- `DeviceType` must be one of the valid enum values (valid values TBD; test checks "Tablet", "laptop", `""`, and `null` all fail)

The tests use FluentValidation's `AbstractValidator` pattern with `.Validate()` and `.ShouldHaveValidationErrorFor()` from `FluentValidation.TestHelper`.
