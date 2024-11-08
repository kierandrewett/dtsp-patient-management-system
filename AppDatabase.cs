using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PMS
{
    public class AppDatabase
    {
        private static string CONNECTION_STRING = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=PMS.accdb";

        public static OleDbConnection CreateConnection()
        {
            OleDbConnection conn = new(CONNECTION_STRING);
            conn.Open();

            return conn;
        }

        private static void ReflectValue<T>(T objInstance, PropertyInfo property, object value)
        {
            object newValue;

            Debug.WriteLine("(Database ORM Reflection): Currently processing: " + property.PropertyType);

            // Special case for enums to handle the conversion
            if (property.PropertyType.IsEnum)
            {
                newValue = Enum.ToObject(property.PropertyType, value);
            } else
            {
                newValue = Convert.ChangeType(value, property.PropertyType);
            }

            property.SetValue(
                objInstance,
                newValue
            );
        }

        public static T? Query<T>(string query, string[] args) where T : class, new()
        {
            OleDbConnection conn = CreateConnection();
            OleDbDataReader reader;

            OleDbCommand command = new OleDbCommand(query, conn);

            foreach (string arg in args)
            {
                command.Parameters.AddWithValue("?", arg);
            }

            reader = command.ExecuteReader();

            try
            {
                if (reader.Read())
                {
                    var objInstance = new T();

                    for (int i = 0; i < reader.VisibleFieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        var value = reader.GetValue(i);

                        var property = typeof(T).GetProperty(fieldName);
                        if (property != null && value != DBNull.Value)
                        {
                            ReflectValue(objInstance, property, value);
                        }
                    }

                    return objInstance;
                } else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error querying database: {ex.Message}");

                return null;
            }
            finally
            {
                reader?.Close();
                conn?.Close();
            }

        }
    }
}
