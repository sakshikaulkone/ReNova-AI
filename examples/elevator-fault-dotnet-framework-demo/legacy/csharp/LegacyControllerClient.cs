using System;
using System.IO;

namespace LegacyElevatorFaultReader
{
    /// <summary>
    /// Legacy static class for simulating elevator controller connection.
    /// In real deployment, this would communicate via COM port or network.
    /// For this demo, reads from a plain text file.
    /// </summary>
    public static class LegacyControllerClient
    {
        /// <summary>
        /// Simulates connecting to elevator controller and reading fault data.
        /// Returns array of fault lines from the controller output file.
        /// </summary>
        /// <param name="filePath">Path to controller data file</param>
        /// <returns>Array of fault lines</returns>
        public static string[] ReadControllerFaultLines(string filePath)
        {
            Console.WriteLine("Connecting to elevator controller...");
            Console.WriteLine("Reading from: " + filePath);
            Console.WriteLine();

            // Check if file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Controller data file not found: " + filePath);
            }

            // Read all fault lines from file
            string[] faultLines = File.ReadAllLines(filePath);

            Console.WriteLine("Successfully read {0} fault records from controller.", faultLines.Length);
            Console.WriteLine();

            return faultLines;
        }
    }
}
