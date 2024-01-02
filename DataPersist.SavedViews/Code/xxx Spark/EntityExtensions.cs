//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Infrastructure;
//using System.Data;

//namespace DataPersist.SavedViews;

//public static class EntityExtensions
//{

//    //var properties = _db.Students.EntityType.GetProperties();
//    //foreach(var p in properties)
//    //{
//    //    p.FieldInfo.
//    //}

//    // Get DbContext from DbSet

//    //private static DbContext GetDbContext<T>(this DbSet<T> set) where T : class
//    //{
//    //    var infrastructure = set as IInfrastructure<IServiceProvider>;
//    //    var serviceProvider = infrastructure.Instance;
//    //    var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext)) as ICurrentDbContext;
//    //    return currentDbContext!.Context;
//    //}

//    public static T? Single<T>(this DbSet<T> set, int? id) where T : class, new()
//    {
//    //    var t = new T();
//    //    return t.Single(id);
//    //}

//        var context = set.GetService<ICurrentDbContext>().Context;

//        var entityType = set.EntityType;
//        var name = entityType.Name;
//        var x = set;
//        //set.EntityType.Local.
//        //var context = set.GetDbContext();

//        //var keyName = set.EntityType..GetKeys().First().GetName();
//        //string sql = CreateSelect(where: keyName + " = @0 ");

//        //var t = set.Single();
//        T t = new T();
//        return t;

//        //string sql = CreateSelect(where: keyName + " = @0 ");
//        //OnSelectingAll(ref sql);
//        //return Read(sql, Make, parms: id).FirstOrDefault();
//    }

    

//    //public static T? Single(DbSet<T> set, int? id)
//    //{
//    //    string sql = CreateSelect(where: keyName + " = @0 ");
//    //    //OnSelectingAll(ref sql);
//    //    return Read(sql, Make, parms: id).FirstOrDefault();
//    //}

//    // helper: creates a sql select statement

//    //private static string CreateSelect(string? columns = null, string? where = null, string? orderBy = null, int? top = null)
//    //{
//    //    string? t = (top != null && top > -1) ? "TOP " + top : null;
//    //    string? c = PrepareSelectColumns(columns);
//    //    string? w = string.IsNullOrEmpty(where) ? null : "WHERE " + where;
//    //    string? o = "ORDER BY " + (string.IsNullOrEmpty(orderBy) ? keyName : orderBy);

//    //    return string.Format(sqlSelect, t, c, w, o);
//    //}

//    //private static string PrepareSelectColumns(string? columns)
//    //{
//    //    if (string.IsNullOrEmpty(columns) || columns.Trim() == "*")
//    //        return allColumns;

//    //    var cols = new StringBuilder("[Id], ");

//    //    foreach (var col in columns.Split(',').Select(c => c.Trim()))
//    //        if (!col.Equals("Id", StringComparison.CurrentCultureIgnoreCase))
//    //            cols.AppendFormat("[{0}],", col);

//    //    return cols.TrimEnd();
//    //}

//    // Use wrapper because yield cannot be in immediate try catch

//    //public static IEnumerable<T> Read<T>(this DatabaseFacade database, string sql, Func<IDataReader, string?, T> make, string? columns = null, params object?[] parms)
//    //{
//    //    try { return database.ReadCore(sql, make, columns, parms); }
//    //    catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    //}

//    //// fast read and instantiate (i.e. make) a list of objects

//    //private static IEnumerable<T> ReadCore<T>(this DatabaseFacade database, string sql, Func<IDataReader, string?, T> make, string? columns, params object?[] parms)
//    //{
//    //    using (var connection = database.CreateConnection())
//    //    {
//    //        using (var command = connection.CreateCommand(sql, parms))
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

//    //public static async Task<IEnumerable<T>> ReadAsync<T>(this DatabaseFacade database, string sql, Func<IDataReader, string?, T> make, string? columns = null, params object?[] parms)
//    //{
//    //    try { return await database.ReadCoreAsync(sql, make, columns, parms).ConfigureAwait(false); }
//    //    catch (Exception ex) { throw new DbException(sql, parms, ex); }
//    //}

//    //// async: read and instantiate (i.e. make) a list of objects

//    //private static async Task<IEnumerable<T>> ReadCoreAsync<T>(this DatabaseFacade database, string sql, Func<IDataReader, string?, T> make, string? columns, params object?[] parms)
//    //{
//    //    using (var connection = database.CreateConnection())
//    //    {
//    //        using (var command = connection.CreateCommand(sql, parms))
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
//}