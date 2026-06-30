# Modernization Request Report

## 1. Executive Summary

The KST-LCE (Legacy Controller Equipment) application is being modernized to replace the legacy hardware dongle with a mobile or web-based solution that allows technicians to connect to elevator controllers without relying on the old dongle. The primary goal is to improve technician mobility, device flexibility, and remote diagnostics capabilities.

## 2. Current State

The KST-LCE application currently depends on an old hardware dongle to connect to elevator controllers. Technicians must physically connect the dongle to a laptop, then use the legacy desktop application to communicate with the elevator controller. This workflow is hardware-dependent and limits technician mobility, device flexibility, and remote diagnostics.

## 3. Desired Future State

The modernized solution should allow technicians to connect to elevator controllers from a mobile device or web browser, without the need for the legacy hardware dongle. The new solution should be deployable on modern devices (phones, tablets, laptops) and support the same read/write capabilities as the current dongle-based workflow.

## 4. Business Goals

The key business goals driving this modernization effort are:

1. Improve technician mobility and flexibility by eliminating the hardware dongle dependency.
2. Enable remote diagnostics and support capabilities for elevator controllers.
3. Reduce the total cost of ownership by moving away from legacy hardware.
4. Ensure continued compatibility with existing elevator controllers.

## 5. User Personas

The primary users affected by this change are the field technicians responsible for maintaining and troubleshooting elevator controllers. The modernized solution should improve their ability to access and interact with the controllers from a wider range of devices and locations.

## 6. Technical Implications

The key technical challenges and considerations for this modernization effort include:

1. Determining the exact communication protocol between the legacy dongle and the elevator controller.
2. Identifying any existing API, SDK, or documented interface provided by the elevator controller.
3. Implementing a secure access model for connecting to the elevator controllers, including authentication, encryption, and audit logging.
4. Ensuring the modernized solution can operate in offline mode when internet connectivity is unavailable at the elevator site.
5. Replicating the full read/write capabilities of the current dongle-based workflow, while carefully considering any safety-critical write operations.
6. Ensuring the modernized solution meets any relevant elevator safety regulations or certifications.

## 7. Risks and Assumptions

Risks and Assumptions:

1. **Dongle/Controller Communication Protocol**: The exact communication protocol between the dongle and the elevator controller is not fully documented. This requires SME review before any protocol assumptions can be made.
2. **Controller API/SDK**: It is unknown if the elevator controller exposes any API, SDK, or documented interface. This needs to be confirmed with an SME.
3. **Cybersecurity Requirements**: The security model required for controller access, including authentication, encryption, and audit logging, is unclear and requires SME input.
4. **Offline Mode**: It is unknown if the technician needs offline access when the elevator site has no internet connectivity. This should be clarified with an SME.
5. **Read/Write Capabilities**: The specific data that can be read from and written to the controller is unclear, and any safety-critical write operations must be identified and reviewed by an SME.
6. **Regulatory Compliance**: The relevant elevator safety regulations or certifications that the modernized solution must satisfy are unknown and require SME review.

## 8. Human Review Questions

1. How does the legacy dongle physically and logically communicate with the elevator controller (e.g., serial, USB, Bluetooth, proprietary protocol)?
2. Does the elevator controller expose any API, SDK, or documented interface that can be used to integrate with the modernized solution?
3. What security model is required for accessing the elevator controllers, including authentication, encryption, and audit logging requirements?
4. Does the technician need offline access to the elevator controllers when the site has no internet connectivity?
5. What specific data can be read from and written to the elevator controllers? Are there any safety-critical write operations that require special consideration?
6. What elevator safety regulations or certifications must the modernized solution comply with?

## 9. Recommended Next Step

The recommended next step is to engage a Subject Matter Expert (SME) who can provide detailed answers to the questions listed in the "Human Review Questions" section. The SME's input is critical to understanding the existing communication protocols, controller capabilities, security requirements, and regulatory constraints before proceeding with the modernization design and implementation.
