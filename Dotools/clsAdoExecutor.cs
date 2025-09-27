using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dotools.clsDataTypes;
using System.Threading;
using Microsoft.Win32;

namespace Dotools
{
    /// <summary>
    /// Provides static methods for executing ADO.NET operations such as queries, stored procedures, and data mapping.
    /// </summary>
    public static class clsAdoExecutor
    {
        /// <summary>
        /// Gets the database connection string from application settings.
        /// </summary>
        static internal string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["ConnectionString"];
            }
        }

        /// <summary>
        /// Executes a SqlCommand and returns a list of values from the specified column index.
        /// </summary>
        /// <typeparam name="T">Type of the values to return.</typeparam>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <param name="ColumnIndex">The column index to read values from.</param>
        /// <returns>List of values of type T.</returns>
        public static List<T> ExecuteReader<T>(SqlCommand Command, int ColumnIndex)
        {
            List<T> list = new List<T>();

            using (SqlDataReader reader = Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add((T)reader[ColumnIndex]);
                }
            }

            return list;
        }

        /// <summary>
        /// Fills the properties of an object with values from a SqlDataReader.
        /// </summary>
        /// <typeparam name="T">Type of the object to fill.</typeparam>
        /// <param name="reader">The SqlDataReader to read from.</param>
        /// <param name="obj">The object to fill.</param>
        private static void FillObject<T>(SqlDataReader reader, T obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (reader[property.Name] == DBNull.Value)
                    property.SetValue(obj, null);
                else
                    property.SetValue(obj, reader[property.Name]);
            }
        }

        /// <summary>
        /// Fills the properties of an object with values from a SqlDataReader.
        /// </summary>
        /// <param name="Reader">The SqlDataReader to read from.</param>
        /// <param name="ObjectData">The object to fill.</param>
        private static void FillObjectData(SqlDataReader Reader, object ObjectData)
        {
            PropertyInfo[] Properties = ObjectData.GetType().GetProperties();

            foreach (PropertyInfo Property in Properties)
            {
                if (Reader[Property.Name] == DBNull.Value)
                    Property.SetValue(ObjectData, null);
                else
                    Property.SetValue(ObjectData, Reader[Property.Name]);
            }
        }

        /// <summary>
        /// Executes a SqlCommand and maps the first result to an object of type T.
        /// </summary>
        /// <typeparam name="T">Type of the object to return.</typeparam>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <returns>An object of type T with mapped properties.</returns>
        public static T ExecuteReaderWithOBJ<T>(SqlCommand Command) where T : class, new()
        {
            T obj = new T();

            using (SqlDataReader reader = Command.ExecuteReader())
            {
                if (reader.Read())
                {
                    FillObject(reader, obj);
                }
            }

            return obj;
        }

        /// <summary>
        /// Executes a SqlCommand and maps each result row to an object of type T.
        /// </summary>
        /// <typeparam name="T">Type of the objects to return.</typeparam>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <returns>List of objects of type T.</returns>
        public static List<T> ExecuteReader<T>(SqlCommand Command)
        {
            List <T> list = new List<T>();

            object Data = null;

            using (SqlDataReader reader = Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Data = Activator.CreateInstance(typeof(T));

                    FillObjectData(reader, Data);

                    list.Add((T)Data);
                }
            }

            return list;
        }

        /// <summary>
        /// Executes a SqlCommand and fills the provided object with the first result row.
        /// </summary>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <param name="ObjectData">The object to fill.</param>
        /// <returns>True if a row was read and filled; otherwise, false.</returns>
        public static bool ExecuteReader(SqlCommand Command, object ObjectData)
        {
            using (SqlDataReader reader = Command.ExecuteReader())
            {
                if (reader.Read())
                {
                    FillObjectData(reader, ObjectData);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Executes a SqlCommand and returns a DataTable from the specified result set index.
        /// </summary>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <param name="ResultSetIndex">The result set index to load.</param>
        /// <returns>DataTable containing the result set.</returns>
        public static DataTable ExecuteReader(SqlCommand Command, int ResultSetIndex)
        {
            DataTable dataTable = new DataTable();

            int Counter = 0;

            using (SqlDataReader reader = Command.ExecuteReader())
            {
                if (ResultSetIndex > 0)
                {
                    while (reader.NextResult())
                    {
                        Counter++;

                        if (Counter == ResultSetIndex)
                        {
                            break;
                        }
                    }
                }    

                if (reader.HasRows)
                {
                    dataTable.Load(reader);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Executes a SqlCommand and returns a DataTable containing the result.
        /// </summary>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <returns>DataTable containing the result.</returns>
        public static DataTable ExecuteReader(SqlCommand Command)
        {
            DataTable dataTable = new DataTable();

            using (SqlDataReader reader = Command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    dataTable.Load(reader);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Modifies the command text and parameters based on the provided filter data.
        /// </summary>
        /// <param name="Command">The SqlCommand to modify.</param>
        /// <param name="FilterData">The filter data to apply.</param>
        private static void GetQueryByFilterData(SqlCommand Command, clsFilterData FilterData)
        {
            using (SqlDataReader reader = Command.ExecuteReader())
            {
                string FieldName = string.Empty;

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    FieldName = reader.GetName(i);

                    if (FieldName == FilterData.FieldName.Trim())
                    {
                        string Condition = string.Empty;

                        switch (FilterData.FilterStyle)
                        {
                            case clsFilterData.enFilterStyle.Contains:
                                Condition = $" WHERE [{FieldName}] Like '%' + @Value + '%' ";
                                break;

                            case clsFilterData.enFilterStyle.Equals:
                                Condition = $" WHERE [{FieldName}] = @Value ";
                                break;
                        }

                        Command.CommandText += Condition;
                        Command.Parameters.AddWithValue("@Value", FilterData.Value);

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Executes a SqlCommand with filter data and returns a DataTable containing the result.
        /// </summary>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <param name="FilterData">The filter data to apply.</param>
        /// <returns>DataTable containing the filtered result.</returns>
        public static DataTable ExecuteReader(SqlCommand Command, clsFilterData FilterData)
        {
            if (!string.IsNullOrEmpty(FilterData?.Value))
            {
                GetQueryByFilterData(Command, FilterData);
            }

            DataTable dataTable = new DataTable();

            using (SqlDataReader reader = Command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    dataTable.Load(reader);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Executes a SqlCommand and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public static object ExecuteScalar(SqlCommand Command)
        {
            return Command.ExecuteScalar();
        }

        /// <summary>
        /// Executes a SqlCommand and returns the number of rows affected.
        /// </summary>
        /// <param name="Command">The SqlCommand to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteNonQuery(SqlCommand Command)
        {
            return Command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a command using the provided executor function and manages connection and error logging.
        /// </summary>
        /// <typeparam name="RType">The type of the result returned by the executor function.</typeparam>
        /// <param name="command">The SqlCommand to execute.</param>
        /// <param name="Executor">The function that executes the command.</param>
        /// <returns>The result returned by the executor function.</returns>
        private static RType ExecuteCommand<RType>(SqlCommand command, Func<SqlCommand, RType> Executor)
        {
            RType T = default(RType);

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (command)
                    {
                        command.Connection = connection;
                        
                        T = Executor(command);
                    }
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, EventLogEntryType.Error);
            }

            return T;
        }

        /// <summary>
        /// Executes a stored procedure using the specified executor function.
        /// </summary>
        /// <typeparam name="RType">The type of the result returned by the executor function.</typeparam>
        /// <param name="Executor">A delegate that defines how to execute the SqlCommand (e.g., ExecuteReader, ExecuteScalar).</param>
        /// <param name="Sp_Name">The name of the stored procedure to execute.</param>
        /// <returns>The result returned by the executor function.</returns>
        public static RType ExecuteStoredProcedure<RType>(Func<SqlCommand, RType> Executor, string Sp_Name)
        {
            SqlCommand command = new SqlCommand(Sp_Name);
            command.CommandType = CommandType.StoredProcedure;

            return ExecuteCommand(command, Executor);
        }

        /// <summary>
        /// Executes a stored procedure using the specified executor function and a single SQL parameter.
        /// </summary>
        /// <typeparam name="RType">The type of the result returned by the executor function.</typeparam>
        /// <param name="Executor">A delegate that defines how to execute the SqlCommand (e.g., ExecuteReader, ExecuteScalar).</param>
        /// <param name="Sp_Name">The name of the stored procedure to execute.</param>
        /// <param name="parameter">The SQL parameter to pass to the stored procedure.</param>
        /// <returns>The result returned by the executor function.</returns>
        public static RType ExecuteStoredProcedure<RType>(Func<SqlCommand, RType> Executor, string Sp_Name, SqlParameter parameter)
        {
            SqlCommand command = new SqlCommand(Sp_Name);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(parameter);

            return ExecuteCommand(command, Executor);
        }

        /// <summary>
        /// Executes a stored procedure with the specified SQL parameters using a custom executor delegate.
        /// </summary>
        /// <typeparam name="RType">The return type expected from the executor function.</typeparam>
        /// <param name="Executor">A function that defines how the SqlCommand should be executed (e.g., ExecuteReader, ExecuteScalar).</param>
        /// <param name="Sp_Name">The name of the stored procedure to execute.</param>
        /// <param name="Parameters">An array of SQL parameters to pass to the stored procedure.</param>
        /// <returns>The result of executing the stored procedure as defined by the executor function.</returns>
        public static RType ExecuteStoredProcedure<RType>(Func<SqlCommand, RType> Executor, string Sp_Name, SqlParameter[] Parameters)
        {
            SqlCommand command = new SqlCommand(Sp_Name);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(Parameters);

            return ExecuteCommand(command, Executor);
        }

        /// <summary>
        /// Executes a SQL query using the specified executor function.
        /// </summary>
        /// <typeparam name="RType">The type of the result returned by the executor function.</typeparam>
        /// <param name="Executor">A delegate that defines how to execute the SqlCommand.</param>
        /// <param name="Query">The SQL query to execute.</param>
        /// <returns>The result returned by the executor function.</returns>
        public static RType ExecuteQuery<RType>(Func<SqlCommand, RType> Executor, string Query)
        {
            RType T = default(RType);

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand Command = new SqlCommand(Query, connection))
                    {
                        T = Executor(Command);
                    }
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, EventLogEntryType.Error);
            }

            return T;
        }

        /// <summary>
        /// Sets SQL parameters for a command based on the properties of the provided object.
        /// </summary>
        /// <param name="Query">The SQL query containing parameter names.</param>
        /// <param name="Command">The SqlCommand to set parameters for.</param>
        /// <param name="ObjectData">The object containing parameter values.</param>
        private static void _SetParametersWithObjectData(string Query, SqlCommand Command, object ObjectData)
        {
            PropertyInfo[] Properties = ObjectData.GetType().GetProperties();

            foreach (PropertyInfo Property in Properties)
            {
                object obj = Property.GetValue(ObjectData);

                if (Query.Contains("@" + Property.Name))
                {
                    if (obj != null && !string.IsNullOrEmpty(Convert.ToString(obj)))
                    {
                        Command.Parameters.AddWithValue("@" + Property.Name, obj);
                    }
                    else
                    {
                        Command.Parameters.AddWithValue("@" + Property.Name, DBNull.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Executes a SQL query using the specified executor function and object data for parameters.
        /// </summary>
        /// <typeparam name="RType">The type of the result returned by the executor function.</typeparam>
        /// <param name="Executor">A delegate that defines how to execute the SqlCommand.</param>
        /// <param name="Query">The SQL query to execute.</param>
        /// <param name="ObjectData">An object whose property names match the parameter names in the query.</param>
        /// <returns>The result returned by the executor function.</returns>
        public static RType ExecuteQuery<RType>(Func<SqlCommand, RType> Executor, string Query, object ObjectData)
        {
            RType T = default(RType);

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand Command = new SqlCommand(Query, connection))
                    {
                        _SetParametersWithObjectData(Query, Command, ObjectData);

                        T = Executor(Command);
                    }
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, EventLogEntryType.Error);
            }

            return T;
        }

        /// <summary>
        /// Executes a SQL query using the specified executor function and a single SQL parameter.
        /// </summary>
        /// <typeparam name="RType">The type of the result returned by the executor function.</typeparam>
        /// <param name="Executor">A delegate that defines how to execute the SqlCommand.</param>
        /// <param name="Query">The SQL query to execute.</param>
        /// <param name="Parameters">The SQL parameter to pass to the query.</param>
        /// <returns>The result returned by the executor function.</returns>
        public static RType ExecuteQuery<RType>(Func<SqlCommand, RType> Executor, string Query, SqlParameter Parameters)
        {
            RType T = default(RType);

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand Command = new SqlCommand(Query, connection))
                    {
                        Command.Parameters.Add(Parameters);

                        T = Executor(Command);
                    }
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, EventLogEntryType.Error);
            }

            return T;
        }

        /// <summary>
        /// Executes a SQL query using the specified executor function and an array of SQL parameters.
        /// </summary>
        /// <typeparam name="RType">The type of the result returned by the executor function.</typeparam>
        /// <param name="Executor">A delegate that defines how to execute the SqlCommand.</param>
        /// <param name="Query">The SQL query to execute.</param>
        /// <param name="Parameters">An array of SQL parameters to pass to the query.</param>
        /// <returns>The result returned by the executor function.</returns>
        public static RType ExecuteQuery<RType>(Func<SqlCommand, RType> Executor, string Query, SqlParameter[] Parameters)
        {
            RType T = default(RType);

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand Command = new SqlCommand(Query, connection))
                    {
                        Command.Parameters.AddRange(Parameters);

                        T = Executor(Command);
                    }
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, EventLogEntryType.Error);
            }

            return T;
        }
    }
}
