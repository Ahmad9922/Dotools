using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Dotools
{
    /// <summary>
    /// Provides utility methods for retrieving database metadata, such as primary keys, parameters, tables, and columns.
    /// </summary>
    public static class clsDatabaseInfo
    {
        /// <summary>
        /// Retrieves primary key information for a specified table.
        /// </summary>
        /// <param name="TableName">The name of the table.</param>
        /// <returns>A DataTable containing primary key information.</returns>
        public static DataTable GetPrimaryKeyInfo(string TableName)
        {
            string spName = "sp_pkeys";

            return clsAdoExecutor.ExecuteStoredProcedure(Command => clsAdoExecutor.ExecuteReader(Command), spName, new SqlParameter("@table_name", TableName));
        }

        /// <summary>
        /// Retrieves the name of the primary key column for a specified table.
        /// </summary>
        /// <param name="TableName">The name of the table.</param>
        /// <returns>The name of the primary key column.</returns>
        public static string GetPrimaryKeyColumnName(string TableName)
        {
            DataTable Record = GetPrimaryKeyInfo(TableName);

            return Convert.ToString(Record.Rows[0]["COLUMN_NAME"]);
        }

        /// <summary>
        /// Retrieves information about output parameters for a specified stored procedure.
        /// </summary>
        /// <param name="Sp_Name">The name of the stored procedure.</param>
        /// <returns>A DataTable containing output parameter information.</returns>
        public static DataTable GetOutputParametersInfo(string Sp_Name)
        {
            string Query = "SELECT * FROM sys.parameters\r\nWHERE object_id = (SELECT object_id FROM sys.objects\r\nWHERE name = @Sp_Name) AND is_output = 1";

            return clsAdoExecutor.ExecuteQuery(Command => clsAdoExecutor.ExecuteReader(Command), Query, new SqlParameter("@Sp_Name", Sp_Name));
        }

        /// <summary>
        /// Retrieves information about the first output parameter for a specified stored procedure.
        /// </summary>
        /// <param name="Sp_Name">The name of the stored procedure.</param>
        /// <returns>A DataTable containing information about the first output parameter.</returns>
        public static DataTable GetFirstOutputParameterInfo(string Sp_Name)
        {
            string Query = "SELECT TOP 1 * FROM sys.parameters\r\nWHERE object_id = (SELECT object_id FROM sys.objects\r\nWHERE name = @Sp_Name) AND is_output = 1";

            return clsAdoExecutor.ExecuteQuery(Command => clsAdoExecutor.ExecuteReader(Command), Query, new SqlParameter("@Sp_Name", Sp_Name));
        }

        /// <summary>
        /// Retrieves the name of the first output parameter for a specified stored procedure.
        /// </summary>
        /// <param name="Sp_Name">The name of the stored procedure.</param>
        /// <returns>The name of the first output parameter, or an empty string if none exist.</returns>
        public static string GetFirstOutputParameterName(string Sp_Name)
        {
            DataTable Record = GetFirstOutputParameterInfo(Sp_Name);

            if (Record.Rows.Count > 0)
                return Convert.ToString(Record.Rows[0]["name"]);
            else
                return string.Empty;
        }

        /// <summary>
        /// Determines whether a stored procedure contains exactly one output parameter.
        /// </summary>
        /// <param name="Sp_Name">The name of the stored procedure.</param>
        /// <returns>True if the stored procedure contains one output parameter; otherwise, false.</returns>
        public static bool IsContainOneOutputParameter(string Sp_Name)
        {
            DataTable OutputParametersTable = GetOutputParametersInfo(Sp_Name);

            return OutputParametersTable.Rows.Count == 1;
        }

        /// <summary>
        /// Retrieves parameter information for a specified stored procedure.
        /// </summary>
        /// <param name="Sp_Name">The name of the stored procedure.</param>
        /// <returns>A DataTable containing parameter information.</returns>
        public static DataTable GetParametersInfo(string Sp_Name)
        {
            string spName = "sp_help";

            return clsAdoExecutor.ExecuteStoredProcedure(Command => clsAdoExecutor.ExecuteReader(Command, 1), spName, new SqlParameter("@objname", Sp_Name));
        }

        /// <summary>
        /// Retrieves the names of all parameters for a specified stored procedure.
        /// </summary>
        /// <param name="Sp_Name">The name of the stored procedure.</param>
        /// <returns>A list of parameter names.</returns>
        public static List<string> GetParametersNames(string Sp_Name)
        {
            DataTable ParametersTable = GetParametersInfo(Sp_Name);

            List<string> TablesNames = new List<string>();

            foreach (DataRow R in ParametersTable.Rows)
            {
                TablesNames.Add(Convert.ToString(R["Parameter_name"]));
            }

            return TablesNames;
        }

        /// <summary>
        /// Retrieves information about all databases.
        /// </summary>
        /// <returns>A DataTable containing database information.</returns>
        public static DataTable GetDatabasesInfo()
        {
            string spName = "sp_databases";

            return clsAdoExecutor.ExecuteStoredProcedure(Command => clsAdoExecutor.ExecuteReader(Command), spName);
        }

        /// <summary>
        /// Retrieves information about all tables.
        /// </summary>
        /// <returns>A DataTable containing table information.</returns>
        public static DataTable GetTablesInfo()
        {
            string spName = "sp_tables";

            return clsAdoExecutor.ExecuteStoredProcedure(Command => clsAdoExecutor.ExecuteReader(Command), spName,
                new SqlParameter[] { new SqlParameter("@table_name", DBNull.Value), new SqlParameter("@table_owner", "dbo") });
        }

        /// <summary>
        /// Retrieves the names of all tables.
        /// </summary>
        /// <returns>A list of table names.</returns>
        public static List<string> GetTablesNames()
        {
            DataTable Table = GetTablesInfo();

            List<string> TablesNames = new List<string>();

            foreach (DataRow R in Table.Rows)
            {
                TablesNames.Add(Convert.ToString(R["COLUMN_NAME"]));
            }

            return TablesNames;
        }

        /// <summary>
        /// Retrieves information about all columns in a specified table.
        /// </summary>
        /// <param name="TableName">The name of the table.</param>
        /// <returns>A DataTable containing column information.</returns>
        public static DataTable GetColumnsInfo(string TableName)
        {
            string spName = "sp_columns";

            return clsAdoExecutor.ExecuteStoredProcedure(Command => clsAdoExecutor.ExecuteReader(Command), spName, new SqlParameter("@table_name", TableName));
        }

        /// <summary>
        /// Retrieves the names of all columns in a specified table.
        /// </summary>
        /// <param name="TableName">The name of the table.</param>
        /// <returns>A list of column names.</returns>
        public static List<string> GetColumnsNames(string TableName)
        {
            DataTable Table = GetColumnsInfo(TableName);

            List<string> ColumnsNames = new List<string>();

            foreach (DataRow R in Table.Rows)
            {
                ColumnsNames.Add(Convert.ToString(R["COLUMN_NAME"]));
            }

            return ColumnsNames;
        }
    }
}
