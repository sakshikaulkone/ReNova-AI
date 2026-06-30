using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace KstLce.Legacy
{
    /// <summary>
    /// FAKE LEGACY SAMPLE - FOR MODERNIZATION ANALYSIS ONLY.
    /// Old ADO.NET data access layer for persisting fault events to SQL Server.
    /// Uses SqlConnection, SqlCommand, SqlDataAdapter patterns.
    /// </summary>
    public class FaultDataAccess
    {
        private string _connectionString;

        public FaultDataAccess()
        {
            _connectionString = ConfigurationManager.AppSettings["FaultDbConnectionString"]
                ?? "Server=localhost;Database=KstLceFaults;Integrated Security=true;";
        }

        public DataTable GetFaultEvents(string assetId, int daysBack)
        {
            string sql = @"SELECT FaultCode, Severity, OccurredAt, ElevatorUnitId
                           FROM FaultEvents
                           WHERE AssetId = @AssetId
                           AND OccurredAt >= DATEADD(day, -@DaysBack, GETDATE())
                           ORDER BY OccurredAt DESC";

            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@AssetId", assetId);
                command.Parameters.AddWithValue("@DaysBack", daysBack);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
            }

            return table;
        }

        public void SaveControllerFaultEvents(DataTable faults)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (DataRow row in faults.Rows)
                {
                    string sql = @"INSERT INTO FaultEvents (FaultCode, Severity, OccurredAt, ElevatorUnitId)
                                   VALUES (@FaultCode, @Severity, @OccurredAt, @ElevatorUnitId)";

                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@FaultCode", row["FaultCode"]);
                    command.Parameters.AddWithValue("@Severity", row["Severity"]);
                    command.Parameters.AddWithValue("@OccurredAt", row["OccurredAt"]);
                    command.Parameters.AddWithValue("@ElevatorUnitId", row["ElevatorUnitId"]);

                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetRecurringFaultCount(string assetId, string faultCode, int daysBack)
        {
            string sql = @"SELECT COUNT(*) FROM FaultEvents
                           WHERE AssetId = @AssetId
                           AND FaultCode = @FaultCode
                           AND OccurredAt >= DATEADD(day, -@DaysBack, GETDATE())";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@AssetId", assetId);
                command.Parameters.AddWithValue("@FaultCode", faultCode);
                command.Parameters.AddWithValue("@DaysBack", daysBack);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }
    }
}
