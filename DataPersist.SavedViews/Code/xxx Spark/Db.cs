//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using System.Data;
//using System.Data.Common;
//using System.Dynamic;

//namespace DataPersist.SavedViews;

//// Database Extensions to the Entity Framework Core

//public static class Db
//{
//    // use wrapper because yield cannot be in immediate try catch

//    //public static IEnumerable<T> Read<T>(string sql, Func<IDataReader, string?, T> make, string? columns = null, params object?[] parms)
//    //{
//    //    try { return ReadCore(sql, make, columns, parms); }
//    //    catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    //}

//    //// fast read and instantiate (i.e. make) a list of objects

//    //private static IEnumerable<T> ReadCore<T>(string sql, Func<IDataReader, string?, T> make, string? columns, params object?[] parms)
//    //{
//    //    using (var connection = CreateConnection())
//    //    {
//    //        using (var command = CreateCommand(sql, connection, parms))
//    //        {
//    //            using (var reader = command.ExecuteReader())
//    //            {
//    //                while (reader.Read())
//    //                {
//    //                    yield return make(reader, columns);
//    //                }
//    //            }
//    //        }
//    //    }
//    //}

//    //// async: wrapper (for consistency because yield cannot be used in this scenario)

//    //public async Task<IEnumerable<T>> ReadAsync<T>(string sql, Func<IDataReader, string?, T> make, string? columns = null, params object?[] parms)
//    //{
//    //    try { return await ReadCoreAsync(sql, make, columns, parms).ConfigureAwait(false); }
//    //    catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    //}

//    //// async: read and instantiate (i.e. make) a list of objects

//    //async Task<IEnumerable<T>> ReadCoreAsync<T>(string sql, Func<IDataReader, string?, T> make, string? columns, params object?[] parms)
//    //{
//    //    using (var connection = await CreateConnectionAsync().ConfigureAwait(false))
//    //    {
//    //        using (var command = CreateCommand(sql, connection, parms))
//    //        {
//    //            using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
//    //            {
//    //                var list = new List<T>();
//    //                while (await reader.ReadAsync().ConfigureAwait(false))
//    //                {
//    //                    list.Add(make(reader, columns));
//    //                }
//    //                return list;
//    //            }
//    //        }
//    //    }
//    //}



//    // Returns a list of dynamic objects using an arbitrary parameterized sql query

//    public static IEnumerable<dynamic?> Query(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try 
//        {
//            return database.QueryCore(sql, parms); 
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // fast read a list of dynamic types

//    private static IEnumerable<dynamic?> QueryCore(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        using (var connection = database.CreateConnection())
//        {
//            using (var command = connection.CreateCommand(sql, parms))
//            {
//                using (var reader = command.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        yield return reader.ToExpando();
//                    }
//                }
//            }
//        }
//    }

//    public static async Task<IEnumerable<dynamic?>> QueryAsync(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try 
//        { 
//            return await QueryCoreAsync(database, sql, parms).ConfigureAwait(false); 
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // async: fast read a list of dynamic types

//    private async static Task<IEnumerable<dynamic?>> QueryCoreAsync(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        using (var connection = database.CreateConnection())
//        {
//            using (var command = connection.CreateCommand(sql, parms))
//            {
//                using (var reader = await command.ExecuteReaderAsync())
//                {
//                    var list = new List<dynamic?>();
//                    while (await reader.ReadAsync().ConfigureAwait(false))
//                    {
//                        list.Add(reader.ToExpando());
//                    }
//                    return list;
//                }
//            }
//        }
//    }

//    // Insert a new record

//    public static int? Insert(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql + ";SELECT SCOPE_IDENTITY();", parms))
//                {
//                    object? result = command.ExecuteScalar();
//                    return result is null ? null : int.Parse(result.ToString()!);
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // async: Insert a new record

//    public static async Task<int?> InsertAsync(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql + ";SELECT SCOPE_IDENTITY();", parms))
//                {
//                    object? result = await command.ExecuteScalarAsync().ConfigureAwait(false);
//                    return result is null ? null : int.Parse(result.ToString()!);
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // Update an existing record

//    public static int Update(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql, parms))
//                {
//                    return command.ExecuteNonQuery();
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // async: Update an existing record

//    public static async Task<int> UpdateAsync(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql, parms))
//                {
//                    return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // Delete a record

//    public static int Delete(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        return database.Update(sql, parms);
//    }

//    // async: Delete a record

//    public static async Task<int> DeleteAsync(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        return await database.UpdateAsync(sql, parms).ConfigureAwait(false);
//    }

//    public static object? Scalar(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql, parms))
//                {
//                    return command.ExecuteScalar();
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // async: Return a scalar object

//    public static async Task<object?> ScalarAsync(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql, parms))
//                {
//                    return await command.ExecuteScalarAsync().ConfigureAwait(false);
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    #region DataSet data access

//    // Returns a DataSet given a query

//    public static DataSet GetDataSet(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql, parms))
//                {
//                    using (var adapter = command.CreateAdapter(database))
//                    {
//                        var ds = new DataSet();
//                        adapter.Fill(ds);

//                        return ds;
//                    }
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // Returns a DataTable given a query

//    public static DataTable GetDataTable(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        try
//        {
//            using (var connection = database.CreateConnection())
//            {
//                using (var command = connection.CreateCommand(sql, parms))
//                {
//                    using (var adapter = command.CreateAdapter(database))
//                    {
//                        var dt = new DataTable();
//                        adapter.Fill(dt);

//                        return dt;
//                    }
//                }
//            }
//        }
//        catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    }

//    // Returns a DataRow given a query

//    public static DataRow? GetDataRow(this DatabaseFacade database, string sql, params object?[] parms)
//    {
//        var dataTable = database.GetDataTable(sql, parms);
//        return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
//    }

//    #endregion

//    #region Helpers

//    // Create a connection object

//    private static DbConnection CreateConnection(this DatabaseFacade database)
//    {
//        var connection = database.GetDbConnection();
//        connection.Open();
//        return connection;
//    }

//    // Create a command object

//    private static DbCommand CreateCommand(this DbConnection connection, string sql, params object?[] parms)
//    {
//        var command = connection.CreateCommand();
//        command.Connection = connection;
//        command.CommandText = sql;
//        command.AddParameters(parms);
//        return command;
//    }

//    // Create an adapter object

//    private static DbDataAdapter CreateAdapter(this DbCommand command, DatabaseFacade database)
//    {
//        var factory = DbProviderFactories.GetFactory(database.ProviderName!);

//        var adapter =  factory.CreateDataAdapter()!;
//        adapter.SelectCommand = command;
//        return adapter;
//    }

//    // Add parameters to a command object

//    private static void AddParameters(this DbCommand command, object?[] parms)
//    {
//        if (parms != null && parms.Length > 0)
//        {
//            // named parameters. Used in INSERT, UPDATE, DELETE
//            string firstParam = parms[0]!.ToString()!;

//            if (!string.IsNullOrEmpty(firstParam) && firstParam.StartsWith("@"))
//            {
//                for (int i = 0; i < parms.Length; i += 2)
//                {
//                    var p = command.CreateParameter();

//                    p.ParameterName = parms[i]!.ToString()!;

//                    // No empty strings to the database
//                    if (parms[i + 1] is string && parms[i + 1]!.ToString()! == "")
//                        parms[i + 1] = null;

//                    p.Value = parms[i + 1] ?? DBNull.Value;

//                    command.Parameters.Add(p);
//                }
//            }
//            else  // ordinal parameters. Used in SELECT or possibly custom EXECUTE statements
//            {
//                for (int i = 0; i < parms.Length; i++)
//                {
//                    // Allow no empty strings going to the database
//                    if (parms[i] is string && parms[i]!.ToString()! == "")
//                        parms[i] = null;

//                    var p = command.CreateParameter();
//                    p.ParameterName = "@" + i;
//                    p.Value = parms[i] ?? DBNull.Value;

//                    command.Parameters.Add(p);
//                }
//            }
//        }
//    }

//    // Iterate over fields in datareader and returns an expando object

//    private static dynamic? ToExpando(this IDataReader reader)
//    {
//        var dictionary = new ExpandoObject() as IDictionary<string, object?>;
//        for (int i = 0; i < reader.FieldCount; i++)
//            dictionary.Add(reader.GetName(i), reader[i] == DBNull.Value ? null : reader[i]);

//        return dictionary as ExpandoObject;
//    }

//    #endregion
//}

//// Custom exception which holds Db execution context

//[Serializable]
//public class DbException : Exception
//{
//    public DbException() : base()
//    {
//    }
//    public DbException(string message)
//        : base("Micro ORM: " + message)
//    {
//    }

//    public DbException(string message, Exception innerException)
//        : base("Micro ORM: " + message, innerException)
//    {
//    }

//    public DbException(string sql, object?[] parms, Exception innerException)
//        : base("Micro ORM. " + (innerException.Message ?? "") + Environment.NewLine + Environment.NewLine +
//                 string.Format("Sql: {0}  ", (sql ?? "--")) + Environment.NewLine + Environment.NewLine +
//                 string.Format("Parms: {0}", (parms != null ? string.Join(",", Array.ConvertAll<object?, string>(parms, o => (o ?? "null").ToString()!)) : "--")),
//        innerException)
//    {
//    }

//    public DbException(string procedure, string message, Exception innerException)
//        : base("IMicro ORM. " + (innerException.Message ?? "") + Environment.NewLine + Environment.NewLine +
//                 string.Format("Procedure: {0}  ", (procedure ?? "--")) + Environment.NewLine + Environment.NewLine +
//                 string.Format("Message: {0}", (message ?? "--")) + Environment.NewLine + Environment.NewLine,
//        innerException)
//    {
//    }
//}

