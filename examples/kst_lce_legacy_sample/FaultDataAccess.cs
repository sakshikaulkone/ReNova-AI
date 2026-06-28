using System;
using System.Data;
using System.Data.SqlClient;

namespace KstLce.Legacy
{
    public class FaultDataAccess
    {
        public DataTable GetFaultEvents(string assetId, int daysBack)
        {
            string sql = "SELECT * FROM FaultEvents WHERE AssetId = @AssetId";

            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection("legacy-connection-string"))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@AssetId", assetId);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
            }

            return table;
        }
    }
}