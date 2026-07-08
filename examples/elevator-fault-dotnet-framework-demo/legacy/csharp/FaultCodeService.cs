using System;

namespace LegacyElevatorFaultReader
{
    /// <summary>
    /// Legacy static service for parsing and interpreting elevator fault codes.
    /// Uses static methods with no dependency injection.
    /// Tightly coupled to console output.
    /// </summary>
    public static class FaultCodeService
    {
        /// <summary>
        /// Parses a pipe-delimited fault line and displays fault details with recommendation.
        /// Format: timestamp|fault_code|description
        /// </summary>
        /// <param name="faultLine">Raw fault line from controller</param>
        public static void ParseAndDisplayFault(string faultLine)
        {
            // Parse pipe-delimited format
            string[] parts = faultLine.Split('|');

            if (parts.Length < 3)
            {
                Console.WriteLine("ERROR: Invalid fault line format (expected 3 fields)");
                Console.WriteLine("Line: " + faultLine);
                Console.WriteLine();
                return;
            }

            // Extract fields with whitespace trimming
            string timestamp = parts[0].Trim();
            string faultCode = parts[1].Trim();
            string description = parts[2].Trim();

            // Get technician recommendation
            string recommendation = GetTechnicianRecommendation(faultCode);

            // Display fault details (tightly coupled to Console)
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("Fault Code: " + faultCode);
            Console.WriteLine("Timestamp: " + timestamp);
            Console.WriteLine("Description: " + description);
            Console.WriteLine("Recommendation: " + recommendation);
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine();
        }

        /// <summary>
        /// Maps fault codes to technician recommendations.
        /// Uses if/else chain instead of dictionary (old style).
        /// </summary>
        /// <param name="faultCode">Fault code identifier</param>
        /// <returns>Technician recommendation text</returns>
        public static string GetTechnicianRecommendation(string faultCode)
        {
            // Note: This is case-sensitive (unlike VB6 UCase version)
            // Could cause issues if controller sends mixed-case codes
            if (faultCode == "DOOR_LOCK_FAILURE")
            {
                return "Inspect door lock mechanism and wiring. Check for mechanical obstruction.";
            }
            else if (faultCode == "MOTOR_OVERCURRENT")
            {
                return "Check motor windings and bearings. Verify load conditions. Inspect motor contactor.";
            }
            else if (faultCode == "LEVELING_SENSOR_FAULT")
            {
                return "Clean leveling sensors. Check sensor alignment and wiring connections.";
            }
            else if (faultCode == "COMMUNICATION_TIMEOUT")
            {
                return "Verify controller communication cable connections. Check for electromagnetic interference.";
            }
            else if (faultCode == "BRAKE_SWITCH_FAULT")
            {
                return "Inspect brake switch operation. Verify brake coil voltage and mechanical linkage.";
            }
            else if (faultCode == "DOOR_REVERSAL")
            {
                return "Check door reversal sensor and safety edge. Verify door track alignment.";
            }
            else if (faultCode == "POSITION_ERROR")
            {
                return "Inspect position encoder and mounting. Check for mechanical wear or misalignment.";
            }
            else if (faultCode == "UNKNOWN_CODE")
            {
                return "Refer to controller technical manual. Contact manufacturer support if needed.";
            }
            else
            {
                return "Unknown fault code. Consult technical documentation.";
            }
        }
    }
}
