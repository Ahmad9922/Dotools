using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dotools
{
    /// <summary>
    /// Static utility class for reflection operations.
    /// Provides methods to dynamically read and write object properties using reflection.
    /// Commonly used for mapping between database results and object properties.
    /// </summary>
    public static class clsReflectionExecutor
    {
        /// <summary>
        /// Retrieves all property names from a given object as a list of strings.
        /// Useful for comparing object properties with database columns or parameters.
        /// </summary>
        /// <typeparam name="T">Type of the object (must be a reference type with parameterless constructor)</typeparam>
        /// <param name="Obj">Object instance to extract property names from</param>
        /// <returns>List of property names as strings</returns>
        /// <example>
        /// var user = new User();
        /// List<string> propertyNames = GetPropertiesNames(user); // Returns ["ID", "Name", "Email", etc.]
        /// </example>
        public static List<string> GetPropertiesNames<T>(T Obj) where T : class, new()
        {
            PropertyInfo[] properties = Obj.GetType().GetProperties();

            return properties.Select(Property => Property.Name).ToList();
        }

        /// <summary>
        /// Retrieves all properties and their current values from an object as a dictionary.
        /// Key represents property name, Value represents the property's current value.
        /// </summary>
        /// <typeparam name="T">Type of the object (must be a reference type with parameterless constructor)</typeparam>
        /// <param name="Obj">Object instance to extract properties and values from</param>
        /// <returns>Dictionary where keys are property names and values are property values</returns>
        /// <example>
        /// var user = new User { ID = 1, Name = "John" };
        /// var props = GetProperties(user); // Returns {"ID": 1, "Name": "John"}
        /// </example>
        public static Dictionary<string, object> GetProperties<T>(T Obj) where T : class, new()
        {
            PropertyInfo[] properties = Obj.GetType().GetProperties();

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

            foreach (PropertyInfo Property in properties)
            {
                keyValuePairs.Add(Property.Name, Property.GetValue(Obj));
            }

            return keyValuePairs;
        }

        /// <summary>
        /// Fills object properties from a dictionary of values.
        /// Property names in the dictionary must match object property names exactly.
        /// </summary>
        /// <typeparam name="T">Type of the object to fill</typeparam>
        /// <param name="Obj">Object instance whose properties will be set</param>
        /// <param name="Values">Dictionary containing property names as keys and values to set</param>
        /// <returns>Always returns true (could be improved to indicate success/failure)</returns>
        /// <remarks>
        /// WARNING: This method will throw KeyNotFoundException if a property exists in the object
        /// but not in the Values dictionary. Consider adding error handling.
        /// </remarks>
        public static bool FillProperties<T>(T Obj, Dictionary<string, object> Values) where T : class, new()
        {
            PropertyInfo[] properties = Obj.GetType().GetProperties();

            foreach (PropertyInfo Property in properties)
            {
                Property.SetValue(Obj, Values[Property.Name]);
            }

            return true;
        }

        /// <summary>
        /// Private helper method to extract all column names from a SqlDataReader.
        /// Used internally to match database columns with object properties.
        /// </summary>
        /// <param name="reader">SqlDataReader positioned at a valid record</param>
        /// <returns>List of column names in the current result set</returns>
        private static List<string> GetNames(SqlDataReader reader)
        {
            List<string> names = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                names.Add(reader.GetName(i));
            }

            return names;
        }

        /// <summary>
        /// Fills object properties from a SqlDataReader based on matching column names.
        /// Only properties that have matching column names in the reader will be set.
        /// Handles DBNull values by setting the property to null.
        /// </summary>
        /// <typeparam name="T">Type of the object to fill</typeparam>
        /// <param name="Obj">Object instance whose properties will be populated</param>
        /// <param name="reader">SqlDataReader containing the data (must be positioned at a valid record)</param>
        /// <returns>Always returns true (could be improved to indicate success/failure)</returns>
        /// <remarks>
        /// - Case-sensitive property/column name matching
        /// - Properties without matching columns are left unchanged
        /// - DBNull values are converted to null
        /// - No type conversion validation (relies on implicit conversion)
        /// </remarks>
        public static bool FillProperties<T>(T Obj, SqlDataReader reader) where T : class, new()
        {
            PropertyInfo[] properties = Obj.GetType().GetProperties();

            // Get all column names from the reader for efficient lookup
            List<string> Fieldsnames = GetNames(reader);

            foreach (PropertyInfo Property in properties)
            {
                // Only set property if a matching column exists in the reader
                if (Fieldsnames.Contains(Property.Name))
                {
                    // Handle NULL values from database
                    if (reader[Property.Name] == DBNull.Value)
                        Property.SetValue(Obj, null);
                    else
                        Property.SetValue(Obj, reader[Property.Name]);
                }
            }

            return true;
        }
    }
}
