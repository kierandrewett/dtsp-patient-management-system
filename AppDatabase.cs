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

            Debug.WriteLine($"(Database ORM Reflection): Currently processing: {property.Name}: {property.PropertyType}");

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

        public static OleDbDataReader ExecuteQuery(OleDbConnection conn, string query, string[] args)
        {
            OleDbCommand command = new OleDbCommand(query, conn);

            foreach (string arg in args)
            {
                command.Parameters.AddWithValue("?", arg);
            }

            return command.ExecuteReader();
        }

        public static T[]? QueryAll<T>(string query, string[] args) where T : class, new()
        {
            OleDbConnection conn = CreateConnection();
            OleDbDataReader reader = ExecuteQuery(conn, query, args);

            List<T> queryResponse = new();

            try
            {
                while (reader.Read())
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

                    queryResponse.Add(objInstance);
                }

                return [..queryResponse];
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

        public static T? QueryFirst<T>(string query, string[] args) where T : class, new()
        {
            return QueryAll<T>(query, args)?.FirstOrDefault();
        }

        public static int QueryCount<T>(string query, string[] args) where T : class, new()
        {
            return QueryAll<T>(query, args)?.Length ?? 0;
        }

        public static int? Update(string query, string[] args)
        {
            OleDbConnection conn = AppDatabase.CreateConnection();
            OleDbDataReader reader;

            try
            {
                reader = AppDatabase.ExecuteQuery(
                    conn,
                    query,
                    args
                );

                int result = reader.RecordsAffected;
                reader.Close();

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error querying database: {ex.Message}");

                return null;
            }
            finally
            {
                conn?.Close();
            }
        }
    }
}
