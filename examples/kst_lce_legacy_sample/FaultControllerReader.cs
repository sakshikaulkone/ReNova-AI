using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;

namespace KstLce.Legacy
{
    /// <summary>
    /// FAKE LEGACY SAMPLE - FOR MODERNIZATION ANALYSIS ONLY.
    /// Reads fault data from an elevator controller after dongle/connection is established.
    /// Parses raw pipe-delimited fault buffer into structured records.
    /// </summary>
    public class FaultControllerReader
    {
        private ControllerConnection _connection;

        public FaultControllerReader(ControllerConnection connection)
        {
            _connection = connection;
        }

        public DataTable ReadFaultsFromController(string elevatorUnitId)
        {
            if (!_connection.IsConnected())
            {
                throw new InvalidOperationException(
                    "Cannot read faults: controller is not connected. " +
                    "Validate dongle and connect first."
                );
            }

            string rawBuffer = ReadRawFaultBuffer(elevatorUnitId);
            DataTable faults = ParseRawFaultBuffer(rawBuffer);

            return faults;
        }

        public DataTable ParseRawFaultBuffer(string rawBuffer)
        {
            // Raw buffer format from controller:
            // FAULT_CODE|SEVERITY|OCCURRED_AT
            // Each line is one fault record.

            DataTable table = new DataTable("ControllerFaults");
            table.Columns.Add("FaultCode", typeof(string));
            table.Columns.Add("Severity", typeof(string));
            table.Columns.Add("OccurredAt", typeof(DateTime));
            table.Columns.Add("ElevatorUnitId", typeof(string));

            if (string.IsNullOrEmpty(rawBuffer))
            {
                return table;
            }

            string[] lines = rawBuffer.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (string line in lines)
            {
                string[] parts = line.Split('|');

                if (parts.Length >= 3)
                {
                    DataRow row = table.NewRow();
                    row["FaultCode"] = parts[0].Trim();
                    row["Severity"] = parts[1].Trim();
                    row["OccurredAt"] = DateTime.Parse(parts[2].Trim());
                    row["ElevatorUnitId"] = _connection.IsConnected() ? "ACTIVE_UNIT" : "UNKNOWN";
                    table.Rows.Add(row);
                }
            }

            return table;
        }

        public List<string> GetDistinctFaultCodes(DataTable faults)
        {
            List<string> codes = new List<string>();

            foreach (DataRow row in faults.Rows)
            {
                string code = row["FaultCode"].ToString();
                if (!codes.Contains(code))
                {
                    codes.Add(code);
                }
            }

            return codes;
        }

        public int CountHighSeverityFaults(DataTable faults)
        {
            int count = 0;
            int threshold = int.Parse(
                ConfigurationManager.AppSettings["HighSeverityThreshold"] ?? "2"
            );

            foreach (DataRow row in faults.Rows)
            {
                if (row["Severity"].ToString() == "HIGH")
                {
                    count++;
                }
            }

            return count;
        }

        private string ReadRawFaultBuffer(string elevatorUnitId)
        {
            // Simulates reading raw fault buffer from controller hardware
            // In reality, this sends a serial command and reads the response
            return "DOOR_LOCK_FAILURE|HIGH|2024-01-15 08:30:00\n" +
                   "MOTOR_OVERCURRENT|HIGH|2024-01-14 14:22:00\n" +
                   "LEVELING_SENSOR_FAULT|MEDIUM|2024-01-10 09:15:00\n" +
                   "COMMUNICATION_TIMEOUT|LOW|2024-01-08 16:45:00";
        }
    }
}
