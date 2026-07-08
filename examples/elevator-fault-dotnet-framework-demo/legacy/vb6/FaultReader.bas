Attribute VB_Name = "FaultReader"
Option Explicit

' ============================================================================
' Legacy VB6 Elevator Fault Code Reader Module
' ============================================================================
' This is old VB6-style code for reference only.
' Does not need to compile in a real VB6 IDE.
' Demonstrates old module-based structure and coding patterns.
' ============================================================================

' Public function to process a single controller fault line
' Expected format: timestamp|fault_code|description
Public Function ProcessControllerFault(ByVal faultLine As String) As String
    Dim parts() As String
    Dim timestamp As String
    Dim faultCode As String
    Dim description As String
    Dim recommendation As String
    Dim outputMsg As String

    ' Parse pipe-delimited fault line
    parts = Split(faultLine, "|")

    ' Validate minimum field count
    If UBound(parts) < 2 Then
        ProcessControllerFault = "ERROR: Invalid fault line format"
        Exit Function
    End If

    ' Extract fields with trimming
    timestamp = Trim(parts(0))
    faultCode = Trim(parts(1))
    description = Trim(parts(2))

    ' Get technician recommendation for this fault code
    recommendation = GetTechnicianRecommendation(faultCode)

    ' Format output message
    outputMsg = "Fault Code: " & faultCode & vbCrLf
    outputMsg = outputMsg & "Timestamp: " & timestamp & vbCrLf
    outputMsg = outputMsg & "Description: " & description & vbCrLf
    outputMsg = outputMsg & "Recommendation: " & recommendation

    ProcessControllerFault = outputMsg
End Function

' Maps fault codes to technician recommendations
' Uses Select Case with UCase for case-insensitive matching
Private Function GetTechnicianRecommendation(ByVal faultCode As String) As String
    Dim recommendation As String

    ' Case-insensitive fault code matching
    Select Case UCase(faultCode)
        Case "DOOR_LOCK_FAILURE"
            recommendation = "Inspect door lock mechanism and wiring. Check for mechanical obstruction."

        Case "MOTOR_OVERCURRENT"
            recommendation = "Check motor windings and bearings. Verify load conditions. Inspect motor contactor."

        Case "LEVELING_SENSOR_FAULT"
            recommendation = "Clean leveling sensors. Check sensor alignment and wiring connections."

        Case "COMMUNICATION_TIMEOUT"
            recommendation = "Verify controller communication cable connections. Check for electromagnetic interference."

        Case "BRAKE_SWITCH_FAULT"
            recommendation = "Inspect brake switch operation. Verify brake coil voltage and mechanical linkage."

        Case "DOOR_REVERSAL"
            recommendation = "Check door reversal sensor and safety edge. Verify door track alignment."

        Case "POSITION_ERROR"
            recommendation = "Inspect position encoder and mounting. Check for mechanical wear or misalignment."

        Case "UNKNOWN_CODE"
            recommendation = "Refer to controller technical manual. Contact manufacturer support if needed."

        Case Else
            recommendation = "Unknown fault code. Consult technical documentation."
    End Select

    GetTechnicianRecommendation = recommendation
End Function

' Demo sub that simulates reading and processing faults
' Uses hardcoded sample data (not real controller communication)
Public Sub ReadAndProcessFaults()
    Dim sampleFaults(1 To 3) As String
    Dim i As Integer
    Dim result As String

    ' Simulated controller fault data
    sampleFaults(1) = "2026-01-15T08:30:00|DOOR_LOCK_FAILURE|Door lock circuit did not confirm closed state"
    sampleFaults(2) = "2026-01-15T09:12:00|MOTOR_OVERCURRENT|Motor current exceeded configured threshold"
    sampleFaults(3) = "2026-01-15T10:05:00|LEVELING_SENSOR_FAULT|Car leveling sensor returned inconsistent signal"

    Debug.Print "=========================================="
    Debug.Print "VB6 Fault Reader Demo"
    Debug.Print "=========================================="
    Debug.Print ""

    ' Process each fault
    For i = 1 To UBound(sampleFaults)
        result = ProcessControllerFault(sampleFaults(i))
        Debug.Print result
        Debug.Print "------------------------------------------"
        Debug.Print ""
    Next i

    Debug.Print "Fault processing complete."
End Sub

' Note: In real code, this would read from hardware controller via COM port or network.
' This demo version uses text file simulation handled by the C# console app.
