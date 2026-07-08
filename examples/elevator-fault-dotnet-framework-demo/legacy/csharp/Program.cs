using System;

namespace LegacyElevatorFaultReader
{
    /// <summary>
    /// Legacy console application entry point.
    /// Uses hardcoded file path and static method calls.
    /// No dependency injection or configuration management.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Hardcoded controller data file path
            string controllerDataFile = "controller_fault_codes.txt";

            Console.WriteLine("==========================================");
            Console.WriteLine("Elevator Fault Reader - Legacy .NET Framework 4.7.2");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            try
            {
                // Read fault data from controller (simulated via text file)
                string[] faultLines = LegacyControllerClient.ReadControllerFaultLines(controllerDataFile);

                Console.WriteLine("Processing {0} fault records...", faultLines.Length);
                Console.WriteLine();

                // Process each fault line
                foreach (string faultLine in faultLines)
                {
                    FaultCodeService.ParseAndDisplayFault(faultLine);
                }

                Console.WriteLine("==========================================");
                Console.WriteLine("Fault processing complete.");
                Console.WriteLine("==========================================");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex.Message);
                Console.ResetColor();
            }

            // Keep console open if running from Visual Studio
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
