# KST-LCE Modernization Request

## Application Name

KST-LCE (Legacy Controller Equipment)

## Current State

The KST-LCE application currently depends on an old hardware dongle to connect to elevator controllers. Technicians must physically connect the dongle to a laptop, then use the legacy desktop application to communicate with the elevator controller.

This workflow is hardware-dependent and limits technician mobility, device flexibility, and remote diagnostics.

## Modernization Goal

Replace the hardware-dependent dongle workflow with a mobile application or web application so that technicians can connect to elevator controllers without relying on the old dongle.

## Desired Outcomes

- Technicians can connect to elevator controllers from a mobile device or web browser.
- No dependency on the legacy hardware dongle for controller communication.
- The solution should be deployable on modern devices (phones, tablets, laptops).
- The solution should support the same read/write capabilities as the current dongle-based workflow.

## Known Constraints

- The exact communication protocol between the dongle and the elevator controller is not fully documented.
- A Subject Matter Expert (SME) review is required before any protocol assumptions are made.
- The modernized solution must not break existing controller compatibility.

## Unknowns Requiring SME Review

1. **Dongle/Controller Communication Protocol**: How does the dongle physically and logically communicate with the elevator controller? (Serial, USB, Bluetooth, proprietary protocol?)
2. **Controller API/SDK**: Does the elevator controller expose any API, SDK, or documented interface?
3. **Cybersecurity Requirements**: What security model is required for controller access? Authentication? Encryption? Audit logging?
4. **Offline Mode**: Does the technician need offline access when the elevator site has no internet connectivity?
5. **Read/Write Capabilities**: What data can be read from the controller? What data can be written? Are there safety-critical write operations?
6. **Regulatory Compliance**: Are there elevator safety regulations or certifications that the modernized solution must satisfy?

## Out of Scope for This Request

- Designing the final application architecture.
- Generating converted code.
- Building the mobile/web application.
- Replacing the elevator controller firmware.
