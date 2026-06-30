# Legacy Understanding Report

## 1. Executive Summary

The legacy KST-LCE (Legacy Controller Equipment) application is responsible for connecting to and reading fault data from elevator controllers. It relies on a legacy hardware dongle for communication and has a desktop-based user interface. The application has several key components, including dongle validation, controller connection management, fault reading and analysis, and data persistence. While the core functionality appears to be intact, the legacy architecture and technology stack pose significant modernization risks that require careful review and planning before proceeding with a rewrite.

## 2. Files Analyzed

- `ControllerDongle.bas`: Handles legacy dongle validation and controller communication logic.
- `FaultTrend.bas`: Implements business rules for analyzing fault trends and generating recommendations.
- `ControllerConnection.cs`: Manages the connection to the elevator controller, including dongle validation.
- `FaultControllerReader.cs`: Reads and parses raw fault data from the connected elevator controller.
- `FaultDataAccess.cs`: Provides data access layer for persisting fault events to a SQL Server database.
- `app.config`: Stores various configuration settings used by the application.
- `FaultCodeSeverity.sql`: Reference data for fault codes, their severity, and recommended actions.
- `LegacyKstLce.csproj`: Visual Studio project file for the legacy .NET Framework application.

## 3. Dongle and Controller Connection Logic

The legacy application relies on a hardware dongle for communication with the elevator controllers. The `ControllerDongle.bas` and `ControllerConnection.cs` files handle the dongle validation and controller connection logic:

- The `ValidateDongle()` function in `ControllerDongle.bas` checks if the physical dongle is present and connected to the system. It attempts to read the dongle's hardware key and compares it to a hardcoded vendor ID. If the dongle is not present or the hardware key does not match, the function returns `False`.
- The `ControllerConnection.cs` class encapsulates the logic for opening a serial/COM port connection to the elevator controller. It first checks if the dongle has been validated (or if the dongle requirement is bypassed) before attempting to establish the connection. If the connection is successful, it sets the `_isConnected` flag and logs the event.

## 4. Fault Reading Logic

The `FaultControllerReader.cs` class is responsible for reading fault data from the connected elevator controller:

- The `ReadFaultsFromController()` method first checks if a controller connection is active. If not, it throws an `InvalidOperationException`.
- The `ReadRawFaultBuffer()` method (a private helper) simulates reading the raw fault buffer from the controller. In a real implementation, this would likely send a command to the controller and read the response.
- The `ParseRawFaultBuffer()` method takes the raw fault buffer string and parses it into a `DataTable` structure, with each row representing a fault record.
- The `GetDistinctFaultCodes()` and `CountHighSeverityFaults()` methods provide utility functions for analyzing the fault data.

## 5. Fault Trend and Recommendation Rules

The `FaultTrend.bas` module contains the business logic for analyzing fault trends and generating recommendations:

- The `CalculateFaultTrend()` function takes an asset ID and a number of days to look back, and returns a recommendation string based on the fault history.
- The `GetRecurringFaultCount()` and `GetHighSeverityFaultCount()` functions retrieve the number of recurring faults and high-severity faults, respectively, for the given asset and time period.
- The `GetRecommendationForFault()` function encapsulates the business rules for generating the recommendation. It checks if the recurring fault count exceeds a threshold, or if the high-severity fault count exceeds a threshold, and returns the appropriate recommendation.

## 6. Data Access and Persistence Logic

The `FaultDataAccess.cs` class provides the data access layer for storing and retrieving fault event data:

- The `GetFaultEvents()` method retrieves a `DataTable` of fault events for a given asset ID and number of days to look back, using an SQL query.
- The `SaveControllerFaultEvents()` method persists a `DataTable` of fault events to the database, using a loop and individual SQL `INSERT` statements.
- The `GetRecurringFaultCount()` method retrieves the count of a specific fault code for a given asset ID and time period, using an SQL query.

The connection string and other database-related configuration values are stored in the `app.config` file.

## 7. Configuration Dependencies

The legacy application relies on the following configuration values in the `app.config` file:

- `DongleRequired`: Determines if the hardware dongle is required for controller access.
- `ControllerComPort` and `ControllerBaudRate`: Specify the serial port and baud rate for connecting to the elevator controller.
- `ControllerReadTimeoutSeconds`: Sets the timeout for reading data from the controller.
- `FaultThreshold` and `HighSeverityThreshold`: Thresholds used in the fault trend analysis business rules.
- `DefaultDaysBack`: The default number of days to look back when analyzing fault history.
- `FaultDbConnectionString`: The connection string for the SQL Server database used to store fault events.

## 8. Modernization Risks

The key modernization risks identified in the legacy codebase include:

1. **Dongle Dependency**: The application is tightly coupled to the legacy hardware dongle, which limits technician mobility and device flexibility. Removing this dependency will require a significant architectural change.
2. **Undocumented Controller Protocol**: The exact communication protocol between the dongle and the elevator controller is not documented, which could make it challenging to replicate the existing functionality without the dongle.
3. **Lack of Documented Controller API**: It is unclear if the elevator controller exposes any documented API or SDK that could be used to integrate with a modernized solution. This may require reverse-engineering the protocol or negotiating with the controller vendor.
4. **Offline Mode Requirement**: The modernized solution may need to support offline access to the elevator controllers, which could add complexity to the design.
5. **Safety-Critical Write Operations**: The legacy application appears to have read-only access to the controller, but any safety-critical write operations must be carefully reviewed and validated before being included in the modernized solution.
6. **Regulatory Compliance**: The modernized solution may need to comply with specific elevator safety regulations or certifications, which should be identified and addressed.

## 9. Logic That Must Be Preserved

The following core behaviors and business rules from the legacy codebase must be preserved in the modernized solution:

1. **Dongle Validation**: The ability to validate the presence and authenticity of the hardware dongle, or an equivalent security mechanism, must be maintained.
2. **Controller Communication Protocol**: The modernized solution must replicate the existing protocol for communicating with the elevator controller, either by reverse-engineering the legacy protocol or integrating with a documented API/SDK.
3. **Fault Reading and Parsing**: The ability to read the raw fault buffer from the controller and parse it into a structured data format must be preserved.
4. **Fault Trend Analysis and Recommendations**: The business rules for analyzing fault trends and generating recommendations must be ported to the modernized solution.
5. **Data Persistence**: The functionality for storing and retrieving fault event data from the database must be maintained.

## 10. Unknowns and Human Review Questions

To proceed with the modernization effort, the following questions should be answered by a Subject Matter Expert (SME):

1. What is the exact communication protocol (serial, USB, Bluetooth, proprietary) used between the legacy dongle and the elevator controller? Is there any documented information or SDK provided by the controller vendor?
2. Does the elevator controller expose any API or documented interface that can be used to integrate with a modernized solution, or will the protocol need to be reverse-engineered?
3. What are the specific security requirements for accessing the elevator controllers, including authentication, encryption, and audit logging needs?
4. Is there a requirement for the modernized solution to support offline access to the elevator controllers when internet connectivity is unavailable at the site?
5. What specific data can be read from and written to the elevator controllers? Are there any safety-critical write operations that require special consideration?
6. What elevator safety regulations or certifications must the modernized solution comply with?

## 11. Recommended Next Step

The recommended next step is to engage a Subject Matter Expert (SME) who can provide detailed answers to the questions listed in the "Unknowns and Human Review Questions" section. The SME's input is critical to understanding the existing communication protocols, controller capabilities, security requirements, and regulatory constraints before proceeding with the modernization design and implementation.

Once the SME has provided the necessary information, the modernization team can begin designing a solution that addresses the identified risks and preserves the core functionality of the legacy application. This may involve exploring alternative communication mechanisms, integrating with documented controller APIs, and implementing a secure and flexible architecture that meets the business and technical requirements.
