using System;
using System.Configuration;

namespace KstLce.Legacy
{
    /// <summary>
    /// FAKE LEGACY SAMPLE - FOR MODERNIZATION ANALYSIS ONLY.
    /// Simulates old .NET Framework controller connection logic.
    /// The dongle must be validated before connecting to a controller.
    /// </summary>
    public class ControllerConnection
    {
        public string ComPort { get; private set; }
        public int BaudRate { get; private set; }
        public bool DongleRequired { get; private set; }
        public string LastConnectionStatus { get; private set; }

        private bool _isConnected;
        private string _currentElevatorUnitId;

        public ControllerConnection()
        {
            ComPort = ConfigurationManager.AppSettings["ControllerComPort"] ?? "COM3";
            BaudRate = int.Parse(ConfigurationManager.AppSettings["ControllerBaudRate"] ?? "9600");
            DongleRequired = bool.Parse(ConfigurationManager.AppSettings["DongleRequired"] ?? "true");
            LastConnectionStatus = "DISCONNECTED";
            _isConnected = false;
        }

        public bool ValidateDongle(string technicianId)
        {
            if (!DongleRequired)
            {
                LastConnectionStatus = "DONGLE_BYPASSED";
                return true;
            }

            bool donglePresent = CheckHardwareDongle();

            if (!donglePresent)
            {
                LastConnectionStatus = "DONGLE_NOT_FOUND";
                LogEvent(technicianId, "DONGLE_VALIDATION_FAILED");
                return false;
            }

            LastConnectionStatus = "DONGLE_VALIDATED";
            LogEvent(technicianId, "DONGLE_VALIDATED");
            return true;
        }

        public bool ConnectToController(string elevatorUnitId)
        {
            if (LastConnectionStatus != "DONGLE_VALIDATED" && LastConnectionStatus != "DONGLE_BYPASSED")
            {
                LastConnectionStatus = "ERROR_DONGLE_NOT_VALIDATED";
                return false;
            }

            int timeoutSeconds = int.Parse(
                ConfigurationManager.AppSettings["ControllerReadTimeoutSeconds"] ?? "10"
            );

            bool success = OpenSerialConnection(ComPort, BaudRate, timeoutSeconds);

            if (success)
            {
                _isConnected = true;
                _currentElevatorUnitId = elevatorUnitId;
                LastConnectionStatus = "CONNECTED";
                LogEvent(elevatorUnitId, "CONTROLLER_CONNECTED");
            }
            else
            {
                LastConnectionStatus = "CONNECTION_FAILED";
                LogEvent(elevatorUnitId, "CONTROLLER_CONNECTION_FAILED");
            }

            return success;
        }

        public void Disconnect()
        {
            if (_isConnected)
            {
                CloseSerialConnection();
                LogEvent(_currentElevatorUnitId, "CONTROLLER_DISCONNECTED");
                _isConnected = false;
                _currentElevatorUnitId = null;
                LastConnectionStatus = "DISCONNECTED";
            }
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        private bool CheckHardwareDongle()
        {
            // Simulates checking for physical hardware dongle on USB/serial
            return true;
        }

        private bool OpenSerialConnection(string port, int baud, int timeoutSeconds)
        {
            // Simulates opening a COM/serial port to the elevator controller
            return true;
        }

        private void CloseSerialConnection()
        {
            // Simulates closing serial connection
        }

        private void LogEvent(string entityId, string eventType)
        {
            // Legacy event logging
            Console.WriteLine($"[KST-LCE] {DateTime.Now}: {eventType} - {entityId}");
        }
    }
}
