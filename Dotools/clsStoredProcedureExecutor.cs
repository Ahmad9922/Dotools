using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dotools
{
    /// <summary>
    /// Static class for executing stored procedures in a generic and unified way.
    /// Uses reflection to automatically bind data between objects and stored procedures.
    /// </summary>
    public static class clsStoredProcedureExecutor
    {
        /// <summary>
        /// Private method to read data from SqlDataReader and populate the properties of the passed object.
        /// Uses clsReflectionExecutor to automatically fill properties based on column names.
        /// </summary>
        /// <typeparam name="T">Type of object whose properties need to be filled</typeparam>
        /// <param name="Command">SQL command prepared for execution</param>
        /// <param name="obj">Object whose properties will be populated with data</param>
        /// <returns>Always returns true (could be improved to return false when no data found)</returns>
        private static bool ExecuteReader<T>(SqlCommand Command, T obj) where T : class, new()
        {
            using (SqlDataReader reader = Command.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Use reflection to fill object properties from the reader
                    clsReflectionExecutor.FillProperties(obj, reader);
                }
            }

            return true;
        }

        /// <summary>
        /// Reads a single record from the database using a stored procedure with one parameter.
        /// </summary>
        /// <typeparam name="T">Type of object to be populated with data</typeparam>
        /// <param name="SpName">Name of the stored procedure</param>
        /// <param name="obj">Object that will be populated with retrieved data</param>
        /// <param name="parameter">Single SQL parameter (e.g., @ID)</param>
        /// <returns>true if execution was successful</returns>
        public static bool Read<T>(string SpName, T obj, SqlParameter parameter) where T : class, new()
        {
            return clsAdoExecutor.ExecuteStoredProcedure(command => ExecuteReader(command, obj), SpName, parameter);
        }

        /// <summary>
        /// Reads a single record from the database using a stored procedure with multiple parameters.
        /// </summary>
        /// <typeparam name="T">Type of object to be populated with data</typeparam>
        /// <param name="SpName">Name of the stored procedure</param>
        /// <param name="obj">Object that will be populated with retrieved data</param>
        /// <param name="parameters">Array of SQL parameters</param>
        /// <returns>true if execution was successful</returns>
        public static bool Read<T>(string SpName, T obj, params SqlParameter[] parameters) where T : class, new()
        {
            return clsAdoExecutor.ExecuteStoredProcedure(command => ExecuteReader(command, obj), SpName, parameters);
        }

        /// <summary>
        /// Private method to verify that object properties match stored procedure parameters.
        /// Automatically creates SQL parameters from object properties.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="SpName">Name of the stored procedure</param>
        /// <param name="obj">Object containing the data</param>
        /// <param name="sqlParameters">Created parameters (output parameter)</param>
        /// <returns>true if all parameters exist in the object, false otherwise</returns>
        private static bool AreParametersInObject<T>(string SpName, T obj, out SqlParameter[] sqlParameters) where T : class, new()
        {
            // Get parameter names from the stored procedure in the database
            List<string> Parameters = clsDatabaseInfo.GetParametersNames(SpName);

            // Get object properties as a dictionary (property name, value)
            Dictionary<string, object> Properties = clsReflectionExecutor.GetProperties(obj);

            List<SqlParameter> SqlParametersTemp = new List<SqlParameter>();

            // Get the name of the first output parameter if exists
            string OutputParamName = clsDatabaseInfo.GetFirstOutputParameterName(SpName);

            foreach (var Param in Parameters)
            {
                // Check if property exists in object (remove @ from parameter name)
                if (!Properties.ContainsKey(Param.Substring(1)))
                {
                    sqlParameters = null;
                    return false;
                }

                // Create SQL parameter from object property
                SqlParametersTemp.Add(new SqlParameter(Param, Properties[Param.Substring(1)]));

                // Mark output parameter if found
                if (OutputParamName == Param)
                {
                    SqlParameter P = SqlParametersTemp.Last();
                    P.Direction = ParameterDirection.Output;
                    P.DbType = DbType.Int32; // Assumes output parameter is Int32
                }
            }

            sqlParameters = SqlParametersTemp.ToArray();
            return true;
        }

        /// <summary>
        /// Creates a new record in the database using a stored procedure.
        /// Expects the stored procedure to have one output parameter to return the new record ID.
        /// </summary>
        /// <typeparam name="T">Type of object containing the data</typeparam>
        /// <param name="Sp_Name">Name of the stored procedure</param>
        /// <param name="obj">Object containing data to be inserted</param>
        /// <returns>ID of the newly created record</returns>
        /// <exception cref="Exception">Thrown if procedure doesn't have output parameter or properties don't match</exception>
        public static int? Create<T>(string Sp_Name, T obj) where T : class, new()
        {
            // Verify that stored procedure has at least one output parameter
            if (!clsDatabaseInfo.IsContainOneOutputParameter(Sp_Name))
                throw new Exception("The stored procedure must have one output parameter to return the ID.");

            SqlParameter[] SqlParameters;

            // Verify that object properties match stored procedure parameters
            if (!AreParametersInObject(Sp_Name, obj, out SqlParameters))
                throw new Exception("The object must contain all parameters defined in the stored procedure.");

            // Execute the stored procedure
            clsAdoExecutor.ExecuteStoredProcedure(command => command.ExecuteNonQuery(), Sp_Name, SqlParameters);

            object OutputValue = SqlParameters.First(p => p.Direction == ParameterDirection.Output).Value;

            if (OutputValue != null && OutputValue != DBNull.Value)
            {
                // Return the output parameter value (new record ID)
                return Convert.ToInt32(OutputValue);
            }

            return null;
        }

        /// <summary>
        /// Updates an existing record in the database using a stored procedure.
        /// Parameters are automatically extracted from the passed object's properties.
        /// </summary>
        /// <typeparam name="T">Type of object containing the data</typeparam>
        /// <param name="Sp_Name">Name of the stored procedure</param>
        /// <param name="obj">Object containing updated data</param>
        /// <returns>true if at least one record was updated, false otherwise</returns>
        /// <exception cref="Exception">Thrown if object properties don't match procedure parameters</exception>
        public static bool Update<T>(string Sp_Name, T obj) where T : class, new()
        {
            SqlParameter[] SqlParameters;

            // Verify that object properties match stored procedure parameters
            if (!AreParametersInObject(Sp_Name, obj, out SqlParameters))
                throw new Exception("The object must contain all parameters defined in the stored procedure.");

            // Execute procedure and check if at least one row was affected
            return clsAdoExecutor.ExecuteStoredProcedure(command => command.ExecuteNonQuery(), Sp_Name, SqlParameters) > 0;
        }

        /// <summary>
        /// Deletes a record from the database using a stored procedure.
        /// Typically used with a single parameter (e.g., @ID).
        /// </summary>
        /// <param name="Sp_Name">Name of the stored procedure</param>
        /// <param name="parameter">SQL parameter (usually the ID of record to delete)</param>
        /// <returns>true if at least one record was deleted, false otherwise</returns>
        public static bool Delete(string Sp_Name, SqlParameter parameter)
        {
            return clsAdoExecutor.ExecuteStoredProcedure(command => command.ExecuteNonQuery(), Sp_Name, parameter) > 0;
        }
    }
}
