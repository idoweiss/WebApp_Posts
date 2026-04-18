using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;

public static class DbHelper
{
    public static string ConnectionString { get; set; } =
        "Data Source=database.db";

    // --- מתודות עבור התלמידים ---
    // פרמטר יחיד
    // ללא הגנה מהזרקות 
    public static List<T> RunSelect<T>(string sql) where T : new()
    {
        return RunSelect<T>(sql, null);
    }

    public static int RunSqlChange(string sql)
    {
        return RunSqlChange(sql, null);
    }

    // --- מתודות מוגנות ---
    // תומכות בתבנית עם {}
    // מקבלות פרמטרים מופרדים

    public static List<T> RunSelect<T>(string sql, params object[] args) where T : new()
    {
        //Console.WriteLine($"The app is running in: {Directory.GetCurrentDirectory()}");

        var list = new List<T>();
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            connection.DefaultTimeout = 5; // seconds
            ConfigureSqlite(connection);

            using var command = CreateCommand(connection, sql, args);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                T obj = new T();
                MapRowToObject(reader, obj);
                list.Add(obj);
            }
        }
        return list;
    }

    public static int RunSqlChange(string sql, params object[] args)
    {
        using (var connection = new SqliteConnection(ConnectionString))
        {
            connection.Open();
            connection.DefaultTimeout = 5; // seconds
            ConfigureSqlite(connection);

            using var command = CreateCommand(connection, sql, args);
            return command.ExecuteNonQuery();
        }
    }

    // --- לוגיקה פנימית ---

    private static void ConfigureSqlite(SqliteConnection connection)
    {
        // משפר קריאה במקביל לכתיבה
        // מפחית נעילות במסד הנתונים
        using var pragma = connection.CreateCommand();
        pragma.CommandText = @"
        PRAGMA journal_mode=WAL;
        PRAGMA synchronous=NORMAL;";
        pragma.ExecuteNonQuery();
    }

    private static SqliteCommand CreateCommand(SqliteConnection connection, string sql, object[] args)
    {
        var command = connection.CreateCommand();

        // if no args run the query as is, no sql injection protection
        if (args == null || args.Length == 0)
        {
            command.CommandText = sql;
            return command;
        }

        // SQL injection pretected parameterization

        for (int i = 0; i < args.Length; i++)
        {
            var paramName = "@p" + i;
            int placeholderPos = sql.IndexOf("{}");

            if (placeholderPos >= 0)
            {
                sql = sql.Remove(placeholderPos, 2).Insert(placeholderPos, paramName);
            }

            command.Parameters.AddWithValue(paramName, args[i] ?? DBNull.Value);
        }

        command.CommandText = sql;
        return command;
    }

    private static void MapRowToObject<T>(SqliteDataReader reader, T obj)
    {
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string colName = reader.GetName(i);

            var field = fields.FirstOrDefault(f =>
                f.Name.Equals(colName, StringComparison.OrdinalIgnoreCase));

            if (field != null && !reader.IsDBNull(i))
            {
                try
                {
                    Type t = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
                    field.SetValue(obj, Convert.ChangeType(reader.GetValue(i), t));
                }
                catch
                {
                }
            }
        }
    }
}