using Microsoft.IdentityModel.Tokens;
using PMS.Models;
using System;
using System.Collections;
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

        private static object SerialiseValue(PropertyInfo property, object value)
        {
            Debug.WriteLine($"(Database ORM Reflection): Currently processing: {property.Name}: {property.PropertyType}");

            // Special case for enums to handle the conversion
            if (property.PropertyType.IsEnum)
            {
                // Convert enum to its integer representation
                return Convert.ChangeType(value, Enum.GetUnderlyingType(property.PropertyType));
            }
            else
            {
                return Convert.ChangeType(value, property.PropertyType);
            }
        }

        private static object DeserialiseValue(PropertyInfo property, object value)
        {
            Debug.WriteLine($"(Database ORM Reflection): Currently processing: {property.Name}: {property.PropertyType}");

            // Special case for enums to handle the conversion
            if (property.PropertyType.IsEnum)
            {
                // Convert the value integer back to an enum
                return Enum.ToObject(property.PropertyType, value);
            }
            else
            {
                return Convert.ChangeType(value, property.PropertyType);
            }
        }

        private static void ReflectValue<T>(T objInstance, PropertyInfo property, object value)
        {
            property.SetValue(
                objInstance,
                DeserialiseValue(property, value)
            );
        }

        public static OleDbDataReader ExecuteQuery(OleDbConnection conn, string query, string[] args)
        {
            OleDbCommand command = new OleDbCommand(query, conn);

            foreach (string arg in args)
            {
                command.Parameters.AddWithValue("?", arg);
            }

            Debug.WriteLine($"(Database): Executing query '{query}'...");

            return command.ExecuteReader();
        }

        public static string[]? QueryColumnNames(string table)
        {
            OleDbConnection conn = CreateConnection();
            OleDbDataReader reader = ExecuteQuery(conn, $"SELECT * FROM {table}", []);

            List<string> columns = [];

            try
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.VisibleFieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);

                        columns.Add(fieldName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error querying database: {ex.ToString()}");

                return null;
            }
            finally
            {
                reader?.Close();
                conn?.Close();
            }

            return [.. columns];
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
                Debug.WriteLine($"Error querying database: {ex.ToString()}");

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
                Debug.WriteLine($"Error querying database: {ex.ToString()}");

                return null;
            }
            finally
            {
                conn?.Close();
            }
        }

        public static int? WriteModelUpdate(BaseModel model)
        {
            string modelName = model.GetType().Name;
            string modelTable = model.ORM_TABLE;
            string modelPrimaryKey = model.ORM_PRIMARY_KEY;

            Debug.WriteLine($"(Database ORM): Attempting to write model object '{modelName}' to database...");

            if (
                modelTable == null ||
                modelPrimaryKey == null
            )
            {
                throw new Exception($"Missing required ORM model information on '{modelName}'.");
            }

            string[]? columnNames = AppDatabase.QueryColumnNames(modelTable);

            if (columnNames == null || columnNames.Length <= 0)
            {
                throw new Exception($"No column schema found for table '{modelTable}'.");
            }

            PropertyInfo? primaryKeyProp = model
                .GetType()
                .GetProperty(modelPrimaryKey);

            if (primaryKeyProp == null)
            {
                throw new Exception($"No primary key with name '{modelPrimaryKey}' found on '{modelName}'.");
            }

            object? primaryKeyValue = primaryKeyProp.GetValue(model);

            if (primaryKeyValue == null)
            {
                throw new Exception($"Primary key with name '{modelPrimaryKey}' on model '{modelName}' is null or empty.");
            }

            string primaryKeyValueStr = primaryKeyValue.ToString()!;

            int matches = AppDatabase.QueryCount<BaseModel>(
                $"SELECT * FROM {modelTable} WHERE {modelPrimaryKey} = ?",
                [primaryKeyValueStr]
            );

            bool isExistingRecord = matches > 0;

            Dictionary<string, string> columnValues = [];

            List<BaseModel> delegatedChildModels = [];

            foreach (PropertyInfo prop in model.GetType().GetProperties())
            {
                Debug.WriteLine(prop.Name);

                object? propValue = prop.GetValue(model);

                // We can assume any child models are not going to be in the table schema
                // check this before we continue to the skip props check.
                if (propValue is BaseModel childModel)
                {
                    Debug.WriteLine($"PROP VALUE {prop.Name} IS A CHILD MODEL");
                    delegatedChildModels.Add(childModel);
                    continue;
                }

                // Skip props that aren't in the table schema
                if (!columnNames.Contains(prop.Name))
                {
                    continue;
                }

                if (propValue != null)
                {
                    object castedValue = SerialiseValue(prop, propValue);
                    string castedValueStr = castedValue.ToString();

                    if (!isExistingRecord || (isExistingRecord && castedValueStr != null && !castedValueStr.IsNullOrEmpty()))
                    {
                        columnValues.Add(prop.Name, castedValueStr);
                    }
                }
            }

            string query = "";
            List<string> queryArgs = [];

            if (isExistingRecord)
            {
                string columns = string.Join(", ", columnValues.Keys);
                string[] values = columnValues.Values.ToArray();
                string templatedValues = string.Join(", ", columnValues.Select(v => $"{v.Key} = ?"));

                query =
                    $"UPDATE {modelTable}" + "\n" +
                    $"SET {templatedValues}" + "\n" +
                    $"WHERE {modelPrimaryKey}=?";

                // Add values
                queryArgs.AddRange(values);

                // Add primary key to query args for update look up
                queryArgs.Add(primaryKeyValueStr);
            }
            else
            {
                string columns = string.Join(", ", columnValues.Keys);
                string[] values = columnValues.Values.ToArray();
                string templatedValues = string.Join(", ", values.Select(v => "?"));

                query =
                    $"INSERT INTO {modelTable} ({columns})" + "\n" +
                    $"VALUES ({templatedValues})";

                // Add values
                queryArgs.AddRange(values);
            }

            Debug.WriteLine(query);
            for (int i = 0; i < queryArgs.Count; i++)
            {
                Debug.WriteLine(i + ": " + queryArgs[i]);
            }

            int? rowsAffected = AppDatabase.Update(
                query, 
                queryArgs.ToArray()
            );

            if (rowsAffected != null && rowsAffected > 0)
            {
                foreach (BaseModel childModel in delegatedChildModels)
                {
                    int? childRowsAffected = AppDatabase.WriteModelUpdate(childModel);

                    if (childRowsAffected == null || rowsAffected < 0)
                    {
                        return rowsAffected;
                    }
                }

                return rowsAffected;
            } else
            {
                return rowsAffected;
            }
        }
    }
}
