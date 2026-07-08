# .NET Framework 4.7.2 Elevator Fault Reader - SME Answers

## System Architecture Questions

**Q: Why is the current system on .NET Framework 4.7.2?**

A: This application was originally developed in 2018 when .NET Framework was the standard. The company has not had resources to modernize it to .NET Core/.NET 8. It works but is limited to Windows deployment and cannot leverage modern cloud-native patterns.

**Q: What are the pain points with .NET Framework 4.7.2?**

A: 
- Windows-only deployment (technicians increasingly use tablets/mobile devices)
- Cannot containerize easily for cloud deployment
- Missing modern C# language features
- Large framework installation footprint
- No trimming/self-contained deployment options
- Static methods make unit testing difficult

**Q: How does the current system connect to elevator controllers?**

A: Currently uses a text file to simulate controller output. In production, this would be replaced with serial communication (COM port) or network protocol. The pipe-delimited format (`timestamp|fault_code|description`) is standard across KONE controllers.

**Q: What fault codes are currently supported?**

A: The system recognizes 8 fault codes:
- DOOR_LOCK_FAILURE
- MOTOR_OVERCURRENT
- LEVELING_SENSOR_FAULT
- COMMUNICATION_TIMEOUT
- BRAKE_SWITCH_FAULT
- DOOR_REVERSAL
- POSITION_ERROR
- UNKNOWN_CODE

Any unmapped code returns a generic "consult documentation" message.

## Business Rules

**Q: What determines the recommendations shown to technicians?**

A: Each fault code maps to a specific technician recommendation based on:
- Common root causes for that fault type
- Standard troubleshooting procedures
- Safety inspection requirements

The mapping is hardcoded in `FaultCodeService.cs` using if/else chains. Modern version should use dictionary for better maintainability.

**Q: Are fault severity levels tracked?**

A: Not currently, but high-severity faults (MOTOR_OVERCURRENT, BRAKE_SWITCH_FAULT, POSITION_ERROR) should be flagged in the modernized version for prioritization.

**Q: Must the modernized version be case-sensitive for fault codes?**

A: No. The VB6 version used `UCase()` for case-insensitive matching. The legacy C# version is incorrectly case-sensitive, which has caused issues. The modern version MUST use case-insensitive comparison to match VB6 behavior.

## Technical Constraints

**Q: What platforms need to be supported in modern version?**

A: Primarily Windows for now, but cross-platform capability is required for future:
- Windows 10/11 (current technician laptops)
- Future: Linux (for cloud-based processing)
- Future: macOS (some field engineers use MacBooks)

**Q: Are there performance requirements?**

A: The system processes small fault datasets (typically 8-50 records). Performance is not a concern. Readability and maintainability are priorities.

**Q: What data format must be preserved?**

A: The pipe-delimited text file format must remain unchanged:
```
timestamp|fault_code|description
```

This ensures compatibility with existing test data and simulated controller output.

**Q: Should nullable reference types be enabled?**

A: Yes. The modern version should enable nullable reference types for better null safety.

## Deployment Requirements

**Q: How is the application currently deployed?**

A: Manual installation on technician laptops via MSI installer. The .NET Framework 4.7.2 runtime is pre-installed on Windows 10/11.

**Q: What deployment improvements are desired?**

A: 
- Self-contained deployment (no runtime installation required)
- Smaller deployment size through trimming
- Future: Docker container for cloud processing
- Future: Cross-platform deployment

## Integration Requirements

**Q: Does this integrate with any other systems?**

A: No current integrations. It's a standalone console application. Future integrations may include:
- Work order management systems
- Fault history database
- Mobile technician apps

**Q: Will this need to work offline?**

A: Yes, absolutely. Technicians often work in elevator shafts without network connectivity. Offline operation is critical.

## VB6 Legacy Code

**Q: Should the VB6 module be converted?**

A: The VB6 module (`FaultReader.bas`) is reference-only to show historical evolution. It does not need to be converted or maintained. Focus on the C# code only.

**Q: Why does VB6 code exist alongside C# code?**

A: The original system was VB6 (early 2000s). It was partially migrated to C# (.NET Framework) around 2018, but some teams still reference the VB6 module for business rule validation.

## Future Enhancements

**Q: What future features are planned?**

A: Potential future features:
- REST API for mobile app integration
- Real-time fault streaming via WebSockets
- Database persistence for fault history
- Machine learning for predictive maintenance
- Integration with KONE's ServiceHub platform

The modernized architecture should make these additions easier.

## Testing & Validation

**Q: How should correctness be validated?**

A: Run the modernized .NET 8 app and legacy .NET Framework 4.7.2 app side-by-side with the same input file. Output must be identical (except for formatting improvements like high-severity indicators).

**Q: Are there automated tests?**

A: No automated tests exist for the legacy code. The modernized version should enable unit testing through proper dependency injection and interface abstractions.
