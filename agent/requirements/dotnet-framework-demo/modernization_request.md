# .NET Framework 4.7.2 to .NET 8 Modernization Request

## Project Overview

Modernize the legacy elevator fault code reader application from .NET Framework 4.7.2 to .NET 8 with modern architecture patterns.

## Current System

**Technology Stack:**
- .NET Framework 4.7.2 (Windows-only, old framework)
- Old-style project format (verbose XML)
- Static methods with tight coupling
- No dependency injection
- Hardcoded file paths
- If/else chains for business logic
- Direct Console.WriteLine in business logic

**Components:**
- VB6 module (historical reference)
- Legacy C# console application
- Text file simulation of controller output
- Static LegacyControllerClient class
- Static FaultCodeService class

## Modernization Goals

### Primary Objectives

1. **Cross-Platform Support**
   - Migrate from Windows-only .NET Framework 4.7.2 to cross-platform .NET 8
   - Enable deployment on Windows, Linux, and macOS
   - Use modern SDK-style project format

2. **Modern Architecture**
   - Replace static methods with dependency injection
   - Implement interface-based abstractions
   - Separate concerns (data access, business logic, presentation)
   - Use modern C# language features (records, pattern matching, nullable types)

3. **Maintainability & Testability**
   - Enable unit testing through proper separation
   - Use Options pattern for configuration
   - Implement structured logging
   - Create strongly-typed domain models

4. **Future Extensibility**
   - Prepare for REST API or gRPC services
   - Abstract controller communication for multiple protocols
   - Support different output formats (console, JSON, etc.)

### Scope

**In Scope:**
- Convert .NET Framework 4.7.2 → .NET 8
- Refactor static methods → dependency injection
- Create domain models (records)
- Implement configuration management (Options pattern)
- Add structured logging
- Maintain backward compatibility with pipe-delimited format

**Out of Scope:**
- Real elevator controller integration (keep text file simulation)
- Web API or mobile UI development (architecture only)
- Database persistence
- Authentication and authorization
- Cloud deployment

## Technical Requirements

1. **Framework Migration**
   - Target .NET 8 (latest LTS)
   - Use SDK-style project format
   - Enable nullable reference types
   - Use modern C# 12 features

2. **Architecture Improvements**
   - Dependency injection with Microsoft.Extensions.DependencyInjection
   - Interface abstractions (IFaultDataReader, IFaultAnalyzer)
   - Configuration management with IOptions<T>
   - Structured logging with Microsoft.Extensions.Logging

3. **Preserve Business Rules**
   - All 8 fault code mappings must remain identical
   - Pipe-delimited format parsing logic preserved
   - Same technician recommendations
   - Case-insensitive fault code matching (VB6 behavior)

## Success Criteria

- Modern .NET 8 app produces identical output to .NET Framework 4.7.2 app
- Cross-platform deployment capability (Windows, Linux, macOS)
- Fully testable architecture with interface abstractions
- Configuration externalized to appsettings.json
- All business rules preserved exactly

## Timeline

This is a proof-of-concept modernization to demonstrate ReNova AI's capabilities for .NET Framework → .NET 8 migrations.
