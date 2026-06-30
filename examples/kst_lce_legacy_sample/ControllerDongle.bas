Attribute VB_Name = "ControllerDongle"

' ============================================================
' FAKE LEGACY SAMPLE - FOR MODERNIZATION ANALYSIS ONLY
' This is NOT real production code. It simulates old VB6-style
' dongle validation and controller communication logic.
' ============================================================

Option Explicit

Private Const DONGLE_VENDOR_ID As String = "KST-HW-2003"
Private Const DONGLE_CHECK_RETRIES As Integer = 3

Public Function ValidateDongle(technicianId As String) As Boolean
    ' Legacy dongle validation.
    ' The hardware key must be physically connected via USB/serial
    ' before the technician can access any controller functions.

    Dim retryCount As Integer
    Dim hardwareKey As String

    For retryCount = 1 To DONGLE_CHECK_RETRIES
        hardwareKey = ReadDongleHardwareKey()

        If hardwareKey = DONGLE_VENDOR_ID Then
            LogDongleAccess technicianId, "VALIDATED"
            ValidateDongle = True
            Exit Function
        End If

        Call Sleep(500)
    Next retryCount

    LogDongleAccess technicianId, "FAILED"
    ValidateDongle = False
End Function


Public Function OpenControllerSession(elevatorUnitId As String, comPort As String, baudRate As Long) As Boolean
    ' Opens a serial/COM port connection to the elevator controller.
    ' The dongle must be validated before this function is called.

    Dim sessionHandle As Long

    If Not IsDonglePresent() Then
        MsgBox "Hardware dongle not detected. Cannot open controller session.", vbCritical
        OpenControllerSession = False
        Exit Function
    End If

    sessionHandle = OpenComPort(comPort, baudRate)

    If sessionHandle <= 0 Then
        LogControllerEvent elevatorUnitId, "CONNECTION_FAILED", "COM port open failed"
        OpenControllerSession = False
        Exit Function
    End If

    ' Send handshake to controller
    Call SendControllerHandshake(sessionHandle, elevatorUnitId)

    LogControllerEvent elevatorUnitId, "SESSION_OPENED", "COM=" & comPort & " BAUD=" & CStr(baudRate)
    OpenControllerSession = True
End Function


Public Function ReadControllerFaultBuffer(elevatorUnitId As String) As String
    ' Reads raw fault buffer from the elevator controller.
    ' Returns a pipe-delimited string of fault records.
    ' Format: FAULT_CODE|SEVERITY|OCCURRED_AT

    Dim rawBuffer As String
    Dim bufferLength As Long

    If Not IsControllerSessionActive() Then
        ReadControllerFaultBuffer = ""
        Exit Function
    End If

    rawBuffer = SendControllerCommand("READ_FAULT_BUFFER", elevatorUnitId)
    bufferLength = Len(rawBuffer)

    If bufferLength = 0 Then
        LogControllerEvent elevatorUnitId, "FAULT_READ", "Empty buffer"
    Else
        LogControllerEvent elevatorUnitId, "FAULT_READ", "Buffer length=" & CStr(bufferLength)
    End If

    ReadControllerFaultBuffer = rawBuffer
End Function


' --- Private helper stubs ---

Private Function ReadDongleHardwareKey() As String
    ' Simulates reading vendor ID from physical dongle hardware
    ReadDongleHardwareKey = "KST-HW-2003"
End Function

Private Function IsDonglePresent() As Boolean
    IsDonglePresent = True
End Function

Private Function IsControllerSessionActive() As Boolean
    IsControllerSessionActive = True
End Function

Private Function OpenComPort(comPort As String, baudRate As Long) As Long
    ' Simulates opening a serial COM port
    OpenComPort = 1
End Function

Private Sub SendControllerHandshake(sessionHandle As Long, elevatorUnitId As String)
    ' Simulates protocol handshake with controller
End Sub

Private Function SendControllerCommand(command As String, elevatorUnitId As String) As String
    ' Simulates sending a command and receiving response from controller
    SendControllerCommand = "DOOR_LOCK_FAILURE|HIGH|2024-01-15 08:30:00" & vbCrLf & _
                            "MOTOR_OVERCURRENT|HIGH|2024-01-14 14:22:00" & vbCrLf & _
                            "LEVELING_SENSOR_FAULT|MEDIUM|2024-01-10 09:15:00"
End Function

Private Sub LogDongleAccess(technicianId As String, status As String)
    ' Log dongle validation attempt
End Sub

Private Sub LogControllerEvent(elevatorUnitId As String, eventType As String, details As String)
    ' Log controller communication event
End Sub
