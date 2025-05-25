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
    public static class clsAdoQueryExecutor
    {
        static internal string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["ConnectionString"];
            }
        }

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

        public static object ExecuteScalar(SqlCommand Command)
        {
            return Command.ExecuteScalar();
        }

        public static int ExecuteNonQuery(SqlCommand Command)
        {
            return Command.ExecuteNonQuery();
        }

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

        public static async Task<RType> ExecuteQueryAsync<RType>(Func<SqlCommand, Task<RType>> Executor, string Query)
        {
            RType T = default(RType);

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand Command = new SqlCommand(Query, connection))
                    {
                        T = await Executor(Command);
                    }
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.WriteEntryInApplicationLog(ex.Message, EventLogEntryType.Error);
            }

            return T;
        }

        private static void _SetParametersWithObjectData(string Query, SqlCommand Command, object ObjectData)
        {
            PropertyInfo[] Properties = ObjectData.GetType().GetProperties();

            foreach (PropertyInfo Property in Properties)
            {
                if (Query.Contains("@" + Property.Name))
                    Command.Parameters.AddWithValue("@" + Property.Name, Property.GetValue(ObjectData) ?? DBNull.Value);
            }
        }

        /// <param name="ObjectData">
        /// When the SQL query includes parameters, pass an object whose property names match the parameter names used in the query.
        /// </param>
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
