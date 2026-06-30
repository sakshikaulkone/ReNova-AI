-- ============================================================
-- FAKE LEGACY SAMPLE - FOR MODERNIZATION ANALYSIS ONLY
-- Reference data: fault codes and their severity/recommended actions.
-- ============================================================

CREATE TABLE FaultCodeSeverity (
    FaultCode NVARCHAR(50) NOT NULL PRIMARY KEY,
    Severity NVARCHAR(20) NOT NULL,
    Description NVARCHAR(200),
    RecommendedAction NVARCHAR(500)
);

INSERT INTO FaultCodeSeverity (FaultCode, Severity, Description, RecommendedAction)
VALUES
    ('DOOR_LOCK_FAILURE', 'HIGH',
     'Door lock mechanism failed to engage or release',
     'Urgent inspection required. Check door lock solenoid, wiring, and interlock switch.'),

    ('MOTOR_OVERCURRENT', 'HIGH',
     'Drive motor exceeded safe current threshold',
     'Urgent inspection required. Check motor windings, drive controller, and mechanical load.'),

    ('LEVELING_SENSOR_FAULT', 'MEDIUM',
     'Floor leveling sensor reporting inconsistent readings',
     'Schedule maintenance. Inspect leveling sensor alignment and calibration.'),

    ('COMMUNICATION_TIMEOUT', 'LOW',
     'Controller did not respond within expected time window',
     'Monitor. May indicate intermittent connection issue. Check cabling and dongle connection.');
