namespace ElevatorFaultReader.Domain;

/// <summary>
/// Enumeration of known fault codes.
/// Used for type-safe fault code handling.
/// </summary>
public static class FaultCode
{
    public const string DoorLockFailure = "DOOR_LOCK_FAILURE";
    public const string MotorOvercurrent = "MOTOR_OVERCURRENT";
    public const string LevelingSensorFault = "LEVELING_SENSOR_FAULT";
    public const string CommunicationTimeout = "COMMUNICATION_TIMEOUT";
    public const string BrakeSwitchFault = "BRAKE_SWITCH_FAULT";
    public const string DoorReversal = "DOOR_REVERSAL";
    public const string PositionError = "POSITION_ERROR";
    public const string UnknownCode = "UNKNOWN_CODE";
}

/// <summary>
/// Fault severity levels for prioritization.
/// High-severity faults: MOTOR_OVERCURRENT, BRAKE_SWITCH_FAULT, POSITION_ERROR
/// </summary>
public enum FaultSeverity
{
    Low = 1,
    Medium = 2,
    High = 3
}