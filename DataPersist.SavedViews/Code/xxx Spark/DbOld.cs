//using Microsoft.Data.SqlClient;
//using System.Data;
//using System.Data.Common;
//using System.Dynamic;

//namespace DataPersist.SavedViews
//{
//    public partial class Db
//    {
//        private static readonly DbProviderFactory factory = SqlClientFactory.Instance;
//        public static string ConnectionString { get; set; } = null!;
//        public static Func<int?> GetUserId { get; set; } = () => null;
//        public Db() { }

//        public Db(string connectionString, Func<int?> getUserId)
//        {
//            ConnectionString = connectionString;
//            GetUserId = getUserId;
//        }

//        // use wrapper because yield cannot be in immediate try catch

//        public IEnumerable<T> Read<T>(string sql, Func<IDataReader, string?, T> make, string? columns = null, params object?[] parms)
//        {
//            try { return ReadCore(sql, make, columns, parms); }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // fast read and instantiate (i.e. make) a list of objects

//        IEnumerable<T> ReadCore<T>(string sql, Func<IDataReader, string?, T> make, string? columns, params object?[] parms)
//        {
//            using (var connection = CreateConnection())
//            {
//                using (var command = CreateCommand(sql, connection, parms))
//                {
//                    using (var reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            yield return make(reader, columns);
//                        }
//                    }
//                }
//            }
//        }

//        // async: wrapper (for consistency because yield cannot be used in this scenario)

//        public async Task<IEnumerable<T>> ReadAsync<T>(string sql, Func<IDataReader, string?, T> make, string? columns = null, params object?[] parms)
//        {
//            try { return await ReadCoreAsync(sql, make, columns, parms).ConfigureAwait(false); }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // async: read and instantiate (i.e. make) a list of objects

//        async Task<IEnumerable<T>> ReadCoreAsync<T>(string sql, Func<IDataReader, string?, T> make, string? columns, params object?[] parms)
//        {
//            using (var connection = await CreateConnectionAsync().ConfigureAwait(false))
//            {
//                using (var command = CreateCommand(sql, connection, parms))
//                {
//                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
//                    {
//                        var list = new List<T>();
//                        while (await reader.ReadAsync().ConfigureAwait(false))
//                        {
//                            list.Add(make(reader, columns));
//                        }
//                        return list;
//                    }
//                }
//            }
//        }

//        // use wrapper because yield cannot be in immediate try catch

//        public IEnumerable<dynamic?> Query(string sql, params object?[] parms)
//        {
//            try { return QueryCore(sql, parms); }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // fast read a list of dynamic types

//        IEnumerable<dynamic?> QueryCore(string sql, params object?[] parms)
//        {
//            using (var connection = CreateConnection())
//            {
//                using (var command = CreateCommand(sql, connection, parms))
//                {
//                    using (var reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            yield return reader.ToExpando();
//                        }
//                    }
//                }
//            }
//        }

//        // async: wrapper (for consistency, because yield return cannot be used)

//        public async Task<IEnumerable<dynamic>> QueryAsync(string sql, params object?[] parms)
//        {
//            try { return await QueryCoreAsync(sql, parms).ConfigureAwait(false); }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // async: fast read a list of dynamic types

//        async Task<IEnumerable<dynamic>> QueryCoreAsync(string sql, params object?[] parms)
//        {
//            using (var connection = await CreateConnectionAsync().ConfigureAwait(false))
//            {
//                using (var command = CreateCommand(sql, connection, parms))
//                {
//                    using (var reader = command.ExecuteReader())
//                    {
//                        var list = new List<dynamic>();
//                        while (await reader.ReadAsync().ConfigureAwait(false))
//                        {
//                            list.Add(reader.ToExpando());
//                        }
//                        return list;
//                    }
//                }
//            }
//        }

//        // executes any sql in database

//        public void Execute(string sql, params object?[] parms)
//        {
//            Update(sql, parms);
//        }

//        // async: executes any sql in database

//        public async System.Threading.Tasks.Task ExecuteAsync(string sql, params object?[] parms)
//        {
//            await UpdateAsync(sql, parms).ConfigureAwait(false);
//        }

//        // return a scalar object

//        public object? Scalar(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = CreateConnection())
//                {
//                    using (var command = CreateCommand(sql, connection, parms))
//                    {
//                        return command.ExecuteScalar();
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // async: return a scalar object

//        public async Task<object?> ScalarAsync(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = await CreateConnectionAsync().ConfigureAwait(false))
//                {
//                    using (var command = CreateCommand(sql, connection, parms))
//                    {
//                        return await command.ExecuteScalarAsync().ConfigureAwait(false);
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // insert a new record

//        public int? Insert(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = CreateConnection())
//                {
//                    using (var command = CreateCommand(sql + ";SELECT SCOPE_IDENTITY();", connection, parms))
//                    {
//                        object? result = command.ExecuteScalar();
//                        return result is null ? null : int.Parse(result.ToString()!);
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // async: insert a new record

//        public async Task<int?> InsertAsync(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = await CreateConnectionAsync().ConfigureAwait(false))
//                {
//                    using (var command = CreateCommand(sql + ";SELECT SCOPE_IDENTITY();", connection, parms))
//                    {
//                        object? result = await command.ExecuteScalarAsync().ConfigureAwait(false);
//                        return result is null ? null : int.Parse(result.ToString()!);
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // update an existing record

//        public int Update(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = CreateConnection())
//                {
//                    using (var command = CreateCommand(sql, connection, parms))
//                    {
//                        return command.ExecuteNonQuery();
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // async: update an existing record

//        public async Task<int> UpdateAsync(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = await CreateConnectionAsync().ConfigureAwait(false))
//                {
//                    using (var command = CreateCommand(sql, connection, parms))
//                    {
//                        return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // delete a record

//        public int Delete(string sql, params object?[] parms)
//        {
//            return Update(sql, parms);
//        }

//        // async: delete a record

//        public async Task<int> DeleteAsync(string sql, params object?[] parms)
//        {
//            return await UpdateAsync(sql, parms).ConfigureAwait(false);
//        }

//        #region Transaction support

//        DbConnection? _connection;
//        DbTransaction? _transaction;

//        // begins a new transaction

//        public void BeginTransaction()
//        {
//            _connection = CreateConnection();
//            _transaction = _connection.BeginTransaction();
//        }

//        // completes an ongoing transaction

//        public void CommitTransaction()
//        {
//            _transaction!.Commit();
//            _connection!.Close();

//            _transaction.Dispose();
//            _connection.Dispose();

//            _transaction = null;
//            _connection = null;
//        }

//        public void RollbackTransaction()
//        {
//            if (_transaction != null)
//            {
//                _transaction.Rollback();
//                _transaction.Dispose();
//                //_transaction = null;

//                if (_connection != null)
//                {
//                    _connection.Close();
//                    _connection.Dispose();
//                    //_connection = null;
//                }
//            }
//        }

//        // insert a new record as part of a transaction

//        public int? TransactedInsert(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var command = CreateCommand(sql + ";SELECT SCOPE_IDENTITY();", _connection!, parms))
//                {
//                    command.Transaction = _transaction;

//                    object? result = command.ExecuteScalar();
//                    return result == null ? null : int.Parse(result.ToString()!);
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // async: insert a new record as part of a transaction

//        public async Task<int?> TransactedInsertAsync(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var command = CreateCommand(sql + ";SELECT SCOPE_IDENTITY();", _connection!, parms))
//                {
//                    command.Transaction = _transaction;

//                    var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
//                    return result is null ? null : int.Parse(result.ToString()!);
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // update a record as part of a transaction

//        public int TransactedUpdate(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var command = CreateCommand(sql, _connection!, parms))
//                {
//                    command.Transaction = _transaction;
//                    return command.ExecuteNonQuery();
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // async: update a record as part of a transaction

//        public async Task<int> TransactedUpdateAsync(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var command = CreateCommand(sql, _connection!, parms))
//                {
//                    command.Transaction = _transaction;
//                    return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // delete a record as apart of a transaction

//        public int TransactedDelete(string sql, params object?[] parms)
//        {
//            return TransactedUpdate(sql, parms);
//        }

//        // async: delete a record as apart of a transaction

//        public async Task<int> TransactedDeleteAsync(string sql, params object?[] parms)
//        {
//            return await TransactedUpdateAsync(sql, parms).ConfigureAwait(false);
//        }

//        #endregion

//        #region DataSet data access

//        // returns a DataSet given a query

//        public DataSet GetDataSet(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = CreateConnection())
//                {
//                    using (var command = CreateCommand(sql, connection, parms))
//                    {
//                        using (var adapter = CreateAdapter(command))
//                        {
//                            var ds = new DataSet();
//                            adapter.Fill(ds);

//                            return ds;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // returns a DataTable given a query

//        public DataTable GetDataTable(string sql, params object?[] parms)
//        {
//            try
//            {
//                using (var connection = CreateConnection())
//                {
//                    using (var command = CreateCommand(sql, connection, parms))
//                    {
//                        using (var adapter = CreateAdapter(command))
//                        {
//                            var dt = new DataTable();
//                            adapter.Fill(dt);

//                            return dt;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex) { throw new DbException(sql, parms, ex); }
//        }

//        // returns a DataRow given a query

//        public DataRow? GetDataRow(string sql, params object?[] parms)
//        {
//            var dataTable = GetDataTable(sql, parms);
//            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
//        }

//        #endregion

//        // creates a connection object

//        DbConnection CreateConnection()
//        {
//            var connection = factory.CreateConnection()!;
//            connection.ConnectionString = ConnectionString;
//            connection.Open();
//            return connection;
//        }

//        // async: creates a connection object

//        async Task<DbConnection> CreateConnectionAsync()
//        {
//            var connection = factory.CreateConnection()!;
//            connection.ConnectionString = ConnectionString;
//            await connection.OpenAsync().ConfigureAwait(false);
//            return connection;
//        }

//        // creates a command object

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
//        DbCommand CreateCommand(string sql, DbConnection conn, params object?[] parms)
//        {
//            var command = factory.CreateCommand()!;
//            command.Connection = conn;
//            command.CommandText = sql;
//            command.AddParameters(parms);
//            return command;
//        }

//        // creates an adapter object

//        DbDataAdapter CreateAdapter(DbCommand command)
//        {
//            var adapter = factory.CreateDataAdapter()!;
//            adapter.SelectCommand = command;
//            return adapter;
//        }
//    }

//    // extension methods

//    public static class DbExtentions
//    {
//        // adds parameters to a command. either named or ordinal parameters.

//        public static void AddParameters(this DbCommand command, object?[] parms)
//        {
//            if (parms != null && parms.Length > 0)
//            {
//                // named parameters. Used in INSERT, UPDATE, DELETE
//                string firstParam = parms[0]!.ToString()!;

//                if (!string.IsNullOrEmpty(firstParam) && firstParam.StartsWith("@"))
//                {
//                    for (int i = 0; i < parms.Length; i += 2)
//                    {
//                        var p = command.CreateParameter();

//                        p.ParameterName = parms[i]!.ToString()!;

//                        // No empty strings to the database
//                        if (parms[i + 1] is string && parms[i + 1]!.ToString()! == "")
//                            parms[i + 1] = null;

//                        p.Value = parms[i + 1] ?? DBNull.Value;

//                        command.Parameters.Add(p);
//                    }
//                }
//                else  // ordinal parameters. Used in SELECT or possibly custom EXECUTE statements
//                {
//                    for (int i = 0; i < parms.Length; i++)
//                    {
//                        // Allow no empty strings going to the database
//                        if (parms[i] is string && parms[i]!.ToString()! == "")
//                            parms[i] = null;

//                        var p = command.CreateParameter();
//                        p.ParameterName = "@" + i;
//                        p.Value = parms[i] ?? DBNull.Value;

//                        command.Parameters.Add(p);
//                    }
//                }
//            }
//        }

//        // iterate over fields in datareader and returns an expando object

//        public static dynamic? ToExpando(this IDataReader reader)
//        {
//            var dictionary = new ExpandoObject() as IDictionary<string, object?>;
//            for (int i = 0; i < reader.FieldCount; i++)
//                dictionary.Add(reader.GetName(i), reader[i] == DBNull.Value ? null : reader[i]);

//            return dictionary as ExpandoObject;
//        }
//    }

//    // custom exception which holds Db execution context

//    //[Serializable]
//    //public class DbException : Exception
//    //{
//    //    public DbException() : base()
//    //    {
//    //    }
//    //    public DbException(string message)
//    //        : base("In Db: " + message)
//    //    {
//    //    }

//    //    public DbException(string message, Exception innerException)
//    //        : base("In Db: " + message, innerException)
//    //    {
//    //    }

//    //    public DbException(string sql, object?[] parms, Exception innerException)
//    //        : base("In Db. " + (innerException.Message ?? "") + Environment.NewLine + Environment.NewLine +
//    //                 string.Format("Sql: {0}  ", (sql ?? "--")) + Environment.NewLine + Environment.NewLine +
//    //                 string.Format("Parms: {0}", (parms != null ? string.Join(",", Array.ConvertAll<object?, string>(parms, o => (o ?? "null").ToString()!)) : "--")),
//    //        innerException)
//    //    {
//    //    }

//    //    public DbException(string procedure, string message, Exception innerException)
//    //        : base("In Db. " + (innerException.Message ?? "") + Environment.NewLine + Environment.NewLine +
//    //                 string.Format("Procedure: {0}  ", (procedure ?? "--")) + Environment.NewLine + Environment.NewLine +
//    //                 string.Format("Message: {0}", (message ?? "--")) + Environment.NewLine + Environment.NewLine,
//    //        innerException)
//    //    {
//    //    }
//    //}
//}
