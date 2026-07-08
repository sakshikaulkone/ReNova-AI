# Elevator Fault Reader - .NET Framework 4.7.2 Legacy Demo

This is a **fake legacy codebase** for demonstrating ReNova AI's modernization capabilities.

## ⚠️ Important Notes

- **Demo code only** - does not connect to real elevator controllers
- **Text file simulation** - uses pipe-delimited file instead of real hardware protocol
- **No secrets or credentials** - completely standalone for POC purposes
- **.NET Framework 4.7.2** - intentionally old framework to demonstrate modernization need
- **VB6 reference file** - shows historical codebase evolution (does not compile)

## What This Application Does

This legacy application helps elevator technicians:
1. Connect to an elevator controller (simulated via text file)
2. Read fault codes from the controller
3. Display fault information with technician recommendations

The simulated controller returns fault records in pipe-delimited format:
```
timestamp|fault_code|description
```

## Folder Structure

```
elevator-fault-dotnet-framework-demo/
├── legacy/
│   ├── vb6/
│   │   └── FaultReader.bas              # Old VB6 fault reader (reference only)
│   └── csharp/
│       ├── Program.cs                   # Console app entry point
│       ├── LegacyControllerClient.cs    # File reader (simulates controller)
│       ├── FaultCodeService.cs          # Fault code mapper with if/else chains
│       ├── LegacyElevatorFaultReader.csproj  # .NET Framework 4.7.2 project
│       ├── Properties/
│       │   └── AssemblyInfo.cs          # Assembly metadata
│       └── controller_fault_codes.txt   # Simulated controller data
│
└── README.md                            # This file
```

## Legacy Technology Stack

- **.NET Framework 4.7.2** (old, pre-.NET Core)
- **Visual Studio 2017/2019** project format (not SDK-style)
- **Static methods** with tight coupling
- **No dependency injection**
- **Hardcoded file paths**
- **No configuration management**
- **No structured logging**
- **String array handling** instead of typed objects

## Modernization Opportunities

This legacy codebase demonstrates typical issues requiring modernization:

### Architecture Issues
- Static methods everywhere (hard to test)
- Tight coupling between components
- No separation of concerns
- Hardcoded dependencies
- Console.WriteLine scattered throughout business logic

### Technology Issues
- .NET Framework 4.7.2 (not cross-platform, limited deployment options)
- Old project format (not SDK-style)
- No nullable reference types
- No modern C# features (records, pattern matching, etc.)
- Manual string parsing instead of structured data

### Maintainability Issues
- If/else chains instead of dictionaries
- No interfaces or abstractions
- Can't unit test without full file system
- Can't extend to support multiple data sources

## Running the Legacy App

### Prerequisites
- .NET Framework 4.7.2 or higher installed
- Windows operating system (required for .NET Framework)

### Option 1: Visual Studio
1. Open `LegacyElevatorFaultReader.csproj` in Visual Studio
2. Press F5 to build and run

### Option 2: MSBuild Command Line
```powershell
cd examples\elevator-fault-dotnet-framework-demo\legacy\csharp
msbuild LegacyElevatorFaultReader.csproj /t:Build /p:Configuration=Release
.\bin\Release\LegacyElevatorFaultReader.exe
```

### Option 3: Developer Command Prompt
```powershell
cd examples\elevator-fault-dotnet-framework-demo\legacy\csharp
csc /out:LegacyElevatorFaultReader.exe /target:exe Program.cs LegacyControllerClient.cs FaultCodeService.cs Properties\AssemblyInfo.cs
LegacyElevatorFaultReader.exe
```

## Expected Output

```
==========================================
Elevator Fault Reader - Legacy .NET Framework 4.7.2
==========================================

Connecting to elevator controller...
Reading from: controller_fault_codes.txt

Successfully read 8 fault records from controller.

Processing 8 fault records...

--------------------------------------------------
Fault Code: DOOR_LOCK_FAILURE
Timestamp: 2026-01-15T08:30:00
Description: Door lock circuit did not confirm closed state
Recommendation: Inspect door lock mechanism and wiring. Check for mechanical obstruction.
--------------------------------------------------

[... additional faults ...]

==========================================
Fault processing complete.
==========================================
```

## Sample Fault Codes

| Fault Code | Description |
|-----------|-------------|
| `DOOR_LOCK_FAILURE` | Door lock circuit error |
| `MOTOR_OVERCURRENT` | Motor current exceeded threshold |
| `LEVELING_SENSOR_FAULT` | Car leveling sensor issue |
| `COMMUNICATION_TIMEOUT` | Controller communication timeout |
| `BRAKE_SWITCH_FAULT` | Brake switch status error |
| `DOOR_REVERSAL` | Door reversal sensor triggered |
| `POSITION_ERROR` | Position encoder inconsistency |
| `UNKNOWN_CODE` | Unmapped fault code |

## VB6 Legacy File

The `legacy/vb6/FaultReader.bas` file demonstrates:
- Old VB6 module structure (`Attribute VB_Name`)
- `Option Explicit` directive
- Module-level functions and subs
- `Split()` for parsing
- `Select Case` with `UCase()` for case-insensitive matching
- `Debug.Print` for output

**Note:** This file is for reference only and does not need to compile in a real VB6 IDE.

## Modernization Goals for ReNova AI

This legacy codebase is designed for ReNova AI to:

1. **Analyze** - Identify legacy patterns and modernization opportunities
2. **Plan** - Design a modern .NET 8 architecture with:
   - Dependency injection
   - Interface-based abstractions
   - Domain models (records)
   - Configuration management
   - Structured logging
   - Separation of concerns

3. **Convert** - Generate modern C# code that:
   - Targets .NET 8 (cross-platform)
   - Uses SDK-style project format
   - Implements clean architecture patterns
   - Enables unit testing
   - Supports future extensibility (REST API, mobile apps)

4. **Preserve** - Maintain functional parity:
   - Same fault code mappings
   - Same recommendations
   - Same pipe-delimited format support
   - Same output for technicians

## Assumptions

1. .NET Framework 4.7.2 is the "legacy" technology to be modernized
2. No real elevator protocols - text file simulation is acceptable
3. No database, APIs, or external services required
4. VB6 file is historical reference only
5. Windows-only deployment is current limitation
6. No authentication or security required for POC

## NOT Included

This demo does **NOT** include:
- Real elevator controller protocols
- COM port or serial communication
- Network communication
- Database persistence
- Web APIs or REST endpoints
- Authentication or authorization
- Cloud deployment configurations
- Docker containerization
- Real credentials or secrets

## Use Case

This sample is designed for:
- **AI-assisted modernization demonstrations**
- **Legacy code analysis practice**
- **Before/after architecture comparisons**
- **Modernization ROI discussions**

---

**Created for ReNova AI demonstration purposes only.**
