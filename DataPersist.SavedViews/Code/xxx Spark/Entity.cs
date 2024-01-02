//using System.Data;
//using System.Dynamic;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Text;

//namespace DataPersist.SavedViews;

//public partial class Entity<T> where T : Entity<T>, new()
//{
//    protected static string tableName { get; set; } = null!;
//    protected static string keyName { get; set; } = null!;
//    //protected static Db db { get; set; } = null!;
//    protected static bool audit { get; set; } = false;

//    private static Dictionary<string, PropertyInfo> props { get; set; } = new();
//    private static Dictionary<string, Func<Entity<T>, object>> propGetters { get; set; } = new();
//    private static Dictionary<string, SchemaMap> map { get; set; } = new();

//    private static string sqlSelect { get; set; } = null!;
//    private static string sqlInsert { get; set; } = null!;
//    private static string sqlUpdate { get; set; } = null!;
//    private static string sqlDelete { get; set; } = null!;
//    private static string sqlPaged { get; set; } = null!;

//    private static string allColumns { get; set; } = null!;
//    private static string allSets { get; set; } = null!;

//    protected string? columns { get; set; }

//    #region Static initialization

//    static Entity()
//    {
//        Init();
//    }

//    // this allows custom db, table, and key to be specified at startup

//    public static void Init(Db? db = null, string? table = null, string? key = null)
//    {
//        InitDb(db, table, key);
//        InitMetadata();

//        InitSelect();
//        InitInsert();
//        InitUpdate();
//        InitDelete();
//        InitPaged();
//    }

//    // sets db, table and primary key names

//    private static void InitDb(Db? _db, string? table, string? key)
//    {
//        db = _db ?? new Db();
//        tableName = table ?? typeof(T).Name;
//        keyName = key ?? "Id";
//    }

//    // initializes metadata for this type

//    private static void InitMetadata()
//    {
//        props = typeof(T).GetProperties().ToDictionary(p => p.Name);

//        propGetters = new Dictionary<string, Func<Entity<T>, object>>();
//        foreach (var prop in props)
//        {
//            propGetters.Add(prop.Key, ReflectionUtility.GetGetter(prop.Value) as Func<Entity<T>, object>);
//        }

//        map = new Dictionary<string, SchemaMap>();

//        foreach (dynamic? column in Columns)
//        {
//            if (props.ContainsKey(column!.COLUMN_NAME))
//            {
//                string columnName = column.COLUMN_NAME;
//                string columnDataType = column.DATA_TYPE;
//                string columnDefault = column.COLUMN_DEFAULT;

//                map.Add(columnName, new SchemaMap
//                {
//                    Prop = props[columnName],
//                    Default = Default(columnDataType, columnDefault),
//                    DataType = columnDataType
//                });
//            }
//        }
//    }

//    private static string[] auditColumns = new[] { "CreatedOn", "CreatedBy", "ChangedOn", "ChangedBy" };

//    // retrieves column schema data from database

//    private static IEnumerable<dynamic?> Columns
//    {
//        get
//        {
//            // get all columns for table, skip computed columns

//            string sql = @"SELECT COLUMN_NAME, COLUMN_DEFAULT, DATA_TYPE 
//                                 FROM INFORMATION_SCHEMA.COLUMNS 
//                                WHERE TABLE_NAME = @0
//                                  AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA +'.' + TABLE_NAME),COLUMN_NAME,'IsComputed') = 0";

//            var columns = db.Query(sql, tableName);
//            var noAuditColumns = columns.Where(c => !auditColumns.Contains((string)c!.COLUMN_NAME));

//            // with all audit columns present auditing will be supported

//            audit = columns.Count() - noAuditColumns.Count() == auditColumns.Count();
//            if (audit) return noAuditColumns;

//            return columns;
//        }
//    }

//    // returns typed column default value 

//    private static object? Default(string dataType, string value)
//    {
//        if (string.IsNullOrEmpty(value)) return null;
//        if (value.Contains("getdate()")) return dataType == "time" ? "gettime()" : "getdate()";
//        if (value.Contains("getutcdate()")) return dataType == "datetimeoffset" ? "getutcoffset()" : "getutcdate()";
//        if (value.Contains("newid()")) return "newid()";
//        if (value.Contains("newsequentialid()")) return "newsequentialid()";

//        string val = value.Replace("(N'", "").Replace("(", "").Replace(")", "").Replace("'", "");

//        // cast default values

//        if (dataType == "int")
//        {
//            if (int.TryParse(val, out int i)) return i;
//        }
//        else if (dataType == "bigint")
//        {
//            if (long.TryParse(val, out long l)) return l;
//        }
//        else if (dataType == "bit")
//        {
//            if (val == "0") return false;
//            if (val == "1") return true;
//        }
//        else if (dataType == "decimal" || dataType == "numeric" || dataType == "money" || dataType == "smallmoney")
//        {
//            if (decimal.TryParse(val, out decimal m)) return m;
//        }

//        // the following ones are used less often

//        else if (dataType == "smallint")
//        {
//            if (short.TryParse(val, out short s)) return s;
//        }
//        else if (dataType == "tinyint")
//        {
//            if (byte.TryParse(val, out byte b)) return b;
//        }
//        else if (dataType == "float")
//        {
//            if (double.TryParse(val, out double d)) return d;
//        }
//        else if (dataType == "real")
//        {
//            if (float.TryParse(val, out float f)) return f;
//        }
//        else if (dataType == "time")
//        {
//            if (TimeSpan.TryParse(val, out TimeSpan t)) return t;
//        }

//        return val;
//    }

//    #endregion

//    #region Constructors

//    // default contructor 

//    public Entity() { }

//    // creates new entity with optional default values

//    public Entity(bool defaults = false)
//    {
//        if (defaults)
//        {
//            foreach (var item in map.Values)
//                item.Prop.SetValue(this, item.Default.OrNow(), null);
//        }
//    }

//    #endregion

//    // retrieves a single object by id

//    public T? Single(int? id)
//    {
//        string sql = CreateSelect(where: keyName + " = @0 ");
//        OnSelectingAll(ref sql);
//        return db.Read(sql, Make, parms: id).FirstOrDefault();
//    }

//    // async: retrieves a single object by id

//    public async Task<T?> SingleAsync(int? id)
//    {
//        string sql = CreateSelect(where: keyName + " = @0 ");
//        OnSelectingAll(ref sql);
//        return (await db.ReadAsync(sql, Make, parms: id).ConfigureAwait(false)).FirstOrDefault();
//    }

//    // retrieves a single object with a where clause

//    public T? Single(string? columns = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateSelect(columns, where);
//        OnSelectingAll(ref sql);
//        return db.Read(sql, Make, columns, parms).FirstOrDefault();
//    }

//    // async: retrieves a single object with a where clause

//    public async Task<T?> SingleAsync(string? columns = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateSelect(columns, where);
//        OnSelectingAll(ref sql);
//        var result = await db.ReadAsync(sql, Make, columns, parms).ConfigureAwait(false);
//        return result.FirstOrDefault()!;
//    }

//    // retrieves a list of objects by their ids

//    public IEnumerable<T> All(string ids)
//    {
//        if (string.IsNullOrEmpty(ids)) return Enumerable.Empty<T>();
//        string sql = CreateSelect(where: keyName + " IN (" + ids + ") ");
//        OnSelectingAll(ref sql);
//        return db.Read(sql, Make);
//    }

//    // async: retrieves a list of objects by their ids

//    public async Task<IEnumerable<T>> AllAsync(string ids)
//    {
//        if (string.IsNullOrEmpty(ids)) return Enumerable.Empty<T>();
//        string sql = CreateSelect(where: keyName + " IN (" + ids + ") ");
//        OnSelectingAll(ref sql);
//        return await db.ReadAsync(sql, Make).ConfigureAwait(false);
//    }

//    // retrieves a list of objects given several criteria

//    public IEnumerable<T> All(string? columns = null, string? where = null, string? orderBy = null, int? top = null, params object?[] parms)
//    {
//        string sql = CreateSelect(columns, where, orderBy, top);
//        OnSelectingAll(ref sql);
//        return db.Read(sql, Make, columns, parms);
//    }

//    // async: retrieves a list of objects given several criteria

//    public async Task<IEnumerable<T>> AllAsync(string? columns = null, string? where = null, string? orderBy = null, int? top = null, params object?[] parms)
//    {
//        string sql = CreateSelect(columns, where, orderBy, top);
//        OnSelectingAll(ref sql);
//        return await db.ReadAsync(sql, Make, columns, parms).ConfigureAwait(false);
//    }

//    // retrieves a paged list without row count, given criteria

//    public IEnumerable<T> Paged(string? columns = null, string? where = null, string? orderBy = "Id", int page = 0, int pageSize = 20, params object?[] parms)
//    {
//        string sql = CreatePaged(columns, where, orderBy, page, pageSize);
//        OnSelectingAll(ref sql);
//        return db.Read(sql, Make, columns, parms);
//    }

//    // async: retrieves a paged list without row count, given criteria

//    public async Task<IEnumerable<T>> PagedAsync(string? columns = null, string? where = null, string? orderBy = "Id", int page = 0, int pageSize = 20, params object?[] parms)
//    {
//        string sql = CreatePaged(columns, where, orderBy, page, pageSize);
//        OnSelectingAll(ref sql);
//        return await db.ReadAsync(sql, Make, columns, parms).ConfigureAwait(false);
//    }

//    // retrieves a paged list of objects given several criteria

//    public IEnumerable<T> Paged(out int totalRows, string? columns = null, string? where = null, string? orderBy = "Id", int page = 0, int pageSize = 20, params object?[] parms)
//    {
//        totalRows = Count(where, parms);

//        string sql = CreatePaged(columns, where, orderBy, page, pageSize);
//        OnSelectingAll(ref sql);
//        return db.Read(sql, Make, columns, parms);
//    }

//    // async: retrieves a paged list of objects given several criteria. note: totalRows is of type object
//    //
//    // there is no async version because out and ref parameters are not supported in async methods.
//    // this is easily solved by two separate async calls: CountAsync and PagedAsync

//    // retrieves any scalar value by criteria

//    public virtual object? Scalar(string operation, string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar(operation, column, where);
//        OnSelectingAll(ref sql);
//        return db.Scalar(sql, parms);
//    }

//    // async: retrieves any scalar value by criteria

//    public virtual async Task<object?> ScalarAsync(string operation, string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar(operation, column, where);
//        OnSelectingAll(ref sql);
//        return await db.ScalarAsync(sql, parms).ConfigureAwait(false);
//    }

//    // retrieves a scalar count value by criteria

//    public virtual int Count(string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("COUNT", keyName, where);
//        OnSelectingAll(ref sql);

//        object? result = db.Scalar(sql, parms);
//        return result is null ? 0 : int.Parse(result.ToString()!);
//    }

//    // async: retrieves a scalar count value by criteria

//    public virtual async Task<int> CountAsync(string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("COUNT", keyName, where);
//        OnSelectingAll(ref sql);

//        object? result = await db.ScalarAsync(sql, parms).ConfigureAwait(false);
//        return result is null ? 0 : int.Parse(result.ToString()!);
//    }

//    // retrieves a scalar max value by criteria

//    public virtual object? Max(string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("MAX", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = db.Scalar(sql, parms);
//        return o is DBNull ? null : o;
//    }

//    // async: retrieves a scalar max value by criteria

//    public virtual async Task<object?> MaxAsync(string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("MAX", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = await db.ScalarAsync(sql, parms).ConfigureAwait(false);
//        return o is DBNull ? null : o;
//    }

//    // retrieves a scalar min value by criteria

//    public virtual object? Min(string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("MIN", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = db.Scalar(sql, parms);
//        return o is DBNull ? null : o;
//    }

//    // retrieves a scalar min value by criteria

//    public virtual async Task<object?> MinAsync(string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("MIN", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = await db.ScalarAsync(sql, parms).ConfigureAwait(false);
//        return o is DBNull ? null : o;
//    }

//    // retrieves a scalar sum value by criteria

//    public virtual object? Sum(string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("SUM", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = db.Scalar(sql, parms);
//        return o is DBNull ? null : o;
//    }

//    // async: retrieves a scalar sum value by criteria

//    public virtual async Task<object?> SumAsync(string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("SUM", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = await db.ScalarAsync(sql, parms).ConfigureAwait(false);
//        return o is DBNull ? null : o;
//    }

//    // retrieves a scalar avg value by criteria

//    public virtual object? Avg(string? column = null, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("AVG", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = db.Scalar(sql, parms);
//        return o is DBNull ? null : o;
//    }

//    // async: retrieves a scalar avg value by criteria

//    public virtual async Task<object?> AvgAsync(string column, string? where = null, params object?[] parms)
//    {
//        string sql = CreateScalar("AVG", column ?? keyName, where);
//        OnSelectingAll(ref sql);
//        object? o = await db.ScalarAsync(sql, parms).ConfigureAwait(false);
//        return o is DBNull ? null : o;
//    }

//    // iterates over data reader fields and populates an entity instance

//    private static readonly Func<IDataReader, string?, T> Make = (reader, columns) =>
//    {
//        T t = new() { columns = columns };

//        for (int i = 0; i < reader.FieldCount; i++)
//        {
//            string key = reader.GetName(i);
//            object? val = reader[i] == DBNull.Value ? null : reader[i];
//            map![key].Prop.SetValue(t, val, null);
//        }

//        t.OnSelected();

//        return t;
//    };

//    // iterates over object properties and prepares these as sql parameters

//    protected object?[] Take()
//    {
//        var objects = new List<object?>();

//        foreach (var item in map.Values)
//        {
//            objects.Add("@" + item.Prop.Name);
//            //objects.Add(item.Prop.GetValue(this, null));
//            objects.Add(propGetters[item.Prop.Name](this));
//        }

//        if (audit)
//        {
//            var dt = DateTime.Now;
//            var userId = Db.GetUserId();

//            objects.Add("@CreatedOn"); objects.Add(dt);
//            objects.Add("@CreatedBy"); objects.Add(userId);
//            objects.Add("@ChangedOn"); objects.Add(dt);
//            objects.Add("@ChangedBy"); objects.Add(userId);
//        }

//        return objects.ToArray();
//    }

//    // retrieves a list of dynamic objects using an arbitrary sql query

//    public IEnumerable<T> Query(string sql, params object?[] parms)
//    {
//        var items = db.Query(sql, parms);
//        foreach (var item in items)
//        {
//            yield return ToType(item);
//        }
//    }

//    // async: retrieves a list of dynamic objects using an arbitrary sql query

//    public async Task<IEnumerable<T>> QueryAsync(string sql, params object?[] parms)
//    {
//        var items = await db.QueryAsync(sql, parms).ConfigureAwait(false);
//        var list = new List<T>();

//        foreach (var item in items)
//        {
//            list.Add(ToType(item));
//        }
//        return list;
//    }

//    // maps an expando object to a typed entity instance

//    private static Func<ExpandoObject, T> ToType = expando =>
//    {
//        T t = new();

//        var dictionary = expando as IDictionary<string, object>;
//        foreach (var key in dictionary.Keys)
//        {
//            if (map!.ContainsKey(key))
//                map[key].Prop.SetValue(t, dictionary[key], null);
//        }
//        return t;
//    };

//    // executes any sql in database

//    public void Execute(string sql, params object?[] parms)
//    {
//        db.Execute(sql, parms);
//    }

//    // async: executes any sql in database

//    public async System.Threading.Tasks.Task ExecuteAsync(string sql, params object?[] parms)
//    {
//        await db.ExecuteAsync(sql, parms).ConfigureAwait(false);
//    }

//    // helper: creates a sql select statement

//    private string CreateSelect(string? columns = null, string? where = null, string? orderBy = null, int? top = null)
//    {
//        string? t = (top != null && top > -1) ? "TOP " + top : null;
//        string? c = PrepareSelectColumns(columns);
//        string? w = string.IsNullOrEmpty(where) ? null : "WHERE " + where;
//        string? o = "ORDER BY " + (string.IsNullOrEmpty(orderBy) ? keyName : orderBy);

//        return string.Format(sqlSelect, t, c, w, o);
//    }

//    // helper: creates a sql select statement for pagination

//    private string CreatePaged(string? columns, string? where, string? orderBy, int page, int pageSize)
//    {
//        string? c = PrepareSelectColumns(columns);
//        string? w = string.IsNullOrEmpty(where) ? null : "WHERE " + where;
//        string? o = string.IsNullOrEmpty(orderBy) ? keyName : orderBy;
//        string? r = "WHERE Row > " + ((page - 1) * pageSize) + " AND Row <= " + (page * pageSize);

//        return string.Format(sqlPaged, c, w, o, r);
//    }

//    // helper: creates a scalar sql select statement

//    private string CreateScalar(string operation, string? column = null, string? where = null)
//    {
//        string? op = operation.ToUpper();
//        string? c = column ?? keyName;
//        string? t = tableName.Bracket();
//        string? w = string.IsNullOrEmpty(where) ? null : "WHERE " + where;

//        return string.Format("SELECT {0}({1}) FROM {2} {3}", op, c, t, w);
//    }

//    private string PrepareSelectColumns(string? columns)
//    {
//        if (string.IsNullOrEmpty(columns) || columns.Trim() == "*")
//            return allColumns;

//        var cols = new StringBuilder("[Id], ");

//        foreach (var col in columns.Split(',').Select(c => c.Trim()))
//            if (!col.Equals("Id", StringComparison.CurrentCultureIgnoreCase))
//                cols.AppendFormat("[{0}],", col);

//        return cols.TrimEnd();
//    }

//    private string PrepareUpdateColumns()
//    {
//        if (string.IsNullOrEmpty(columns) || columns.Trim() == "*")
//            return allSets;

//        var sets = new StringBuilder();

//        var cols = columns.Split(',').Select(c => c.Trim()).ToList();
//        foreach (var col in cols)
//        {
//            // Don't include Id
//            if (col == keyName) continue;

//            //  Don't include 'create' audit fields
//            if (audit && (col == "CreatedOn" || col == "CreatedBy")) continue;

//            sets.AppendFormat("[{0}]=@{1},", col, col);
//        }

//        // Also include 'explicitly' assigned values that were not in original column list

//        foreach (var item in map.Values)
//        {
//            var col = item.Prop.Name;

//            if (col == keyName) continue;
//            if (cols.Contains(col)) continue;

//            // Only do this for columns without default values
//            var value = item.Prop.GetValue(this, null);
//            if (item.Default == null && value != null)
//                sets.AppendFormat("[{0}]=@{1},", col, col);
//        }

//        sets.AppendFormat("[{0}]=@{1},", "ChangedOn", "ChangedOn");
//        sets.AppendFormat("[{0}]=@{1},", "ChangedBy", "ChangedBy");

//        return sets.TrimEnd();
//    }

//    // indexer. this provides easy access to properties

//    private object? this[string name]
//    {
//        get { return map[name].Prop.GetValue(this, null); }
//        set { map[name].Prop.SetValue(this, value, null); }
//    }

//    #region Initialize Sql Statements

//    private static void InitSelect()
//    {
//        var cols = new StringBuilder();
//        foreach (var key in map.Keys)
//            cols.AppendFormat("[{0}], ", key);

//        allColumns = cols.TrimEnd();

//        // {{0}} is placeholders for TOP, {{1}} and {{2}} and {{3}} for COLUMNS, WHERE, ORDER BY.
//        string sql = "SELECT {{0}} {{1}} FROM {0} {{2}} {{3}}";
//        sqlSelect = string.Format(sql, tableName.Bracket());
//    }

//    // builds a sql select template string for pagination

//    private static void InitPaged()
//    {
//        var cols = new StringBuilder();
//        foreach (var key in map.Keys)
//            cols.AppendFormat("[{0}], ", key);

//        allColumns = cols.TrimEnd();

//        // {{0}}, {{1}} and {{2}} are for COLUMNS, WHERE, ORDER BY, {{3}} is for Row WHERE clause.
//        string sql = ";WITH cte AS (SELECT {{0}}, ROW_NUMBER() OVER (ORDER BY {{2}}) AS Row FROM {0} {{1}}) SELECT {{0}} FROM cte {{3}} ";

//        sqlPaged = string.Format(sql, tableName.Bracket());
//    }

//    // builds a sql insert template string

//    private static void InitInsert()
//    {
//        var cols = new StringBuilder();
//        var vals = new StringBuilder();

//        foreach (var key in PropsWithoutPrimaryKeyOrTimeStamps)
//        {
//            cols.AppendFormat("[{0}], ", key);
//            vals.AppendFormat("@{0}, ", key);
//        }

//        string sql = "INSERT INTO {0} ({1}) VALUES ({2})";
//        sqlInsert = string.Format(sql, tableName.Bracket(), cols.TrimEnd(), vals.TrimEnd());
//    }

//    // builds a sql update template string

//    private static void InitUpdate()
//    {
//        var sets = new StringBuilder();

//        foreach (var key in PropsWithoutPrimaryKeyOrTimeStamps)
//        {
//            // don't include 'create' audit fields
//            if (audit && (key == "CreatedOn" || key == "CreatedBy")) continue;

//            sets.AppendFormat("[{0}]=@{1}, ", key, key);
//        }

//        allSets = sets.TrimEnd();

//        // {{0}} are the SETs
//        string sql = "UPDATE {0} SET {{0}} WHERE {1} = @{2}";
//        sqlUpdate = string.Format(sql, tableName.Bracket(), keyName, keyName);
//    }

//    // builds a sql delete template string

//    private static void InitDelete()
//    {
//        string sql = "DELETE FROM {0} WHERE {1} = @{2}";
//        sqlDelete = string.Format(sql, tableName.Bracket(), keyName, keyName);
//    }

//    // returns all list of properties except primary key 

//    private static IEnumerable<string> PropsWithoutPrimaryKey
//    {
//        get
//        {
//            foreach (var key in map.Keys)
//                if (key != keyName)
//                    yield return key;

//            if (audit)
//                foreach (var key in auditColumns)
//                    yield return key;
//        }
//    }

//    private static IEnumerable<string> PropsWithoutPrimaryKeyOrTimeStamps
//    {
//        get
//        {
//            foreach (var key in map.Keys)
//                if (key != keyName &&
//                    map[key].DataType != "timestamp" &&
//                    map[key].DataType != "rowversion")
//                    yield return key;

//            if (audit)
//                foreach (var key in auditColumns)
//                    yield return key;
//        }
//    }

//    #endregion

//    // partial methods that allow custom coding (for all entities) before and after all db operations

//    partial void OnSelectingAll(ref string sql);
//    partial void OnInsertingAll(ref string sql);
//    partial void OnInsertedAll();
//    partial void OnUpdatingAll(ref string sql);
//    partial void OnUpdatedAll();
//    partial void OnDeletingAll(ref string sql);
//    partial void OnDeletedAll();

//    // virtual methods that allow custom coding (by entity type) before and after all db operations

//    protected virtual void OnSelected() { }
//    protected virtual void OnInserting(ref string sql) { }
//    protected virtual void OnInserted() { }
//    protected virtual void OnUpdating(ref string sql) { }
//    protected virtual void OnUpdated() { }
//    protected virtual void OnDeleting(ref string sql) { }
//    protected virtual void OnDeleted() { }

//    // event handlers by entity type

//    public static event EventHandler Inserted = null!;
//    public static event EventHandler Updated = null!;
//    public static event EventHandler Deleted = null!;

//    protected virtual void OnInserted(EventArgs e) { Inserted?.Invoke(this, e); }
//    protected virtual void OnUpdated(EventArgs e) { Updated?.Invoke(this, e); }
//    protected virtual void OnDeleted(EventArgs e) { Deleted?.Invoke(this, e); }

//    // inserts current entity instance

//    public virtual void Insert()
//    {
//        string sql = sqlInsert;

//        OnInsertingAll(ref sql);
//        OnInserting(ref sql);
//        this[keyName] = db.Insert(sql, Take());
//        OnInserted(EventArgs.Empty);
//        OnInserted();
//        OnInsertedAll();
//    }

//    // async: inserts current entity instance

//    public virtual async System.Threading.Tasks.Task InsertAsync()
//    {
//        string sql = sqlInsert;

//        OnInsertingAll(ref sql);
//        OnInserting(ref sql);
//        this[keyName] = await db.InsertAsync(sql, Take()).ConfigureAwait(false);
//        OnInserted(EventArgs.Empty);
//        OnInserted();
//        OnInsertedAll();
//    }

//    // updates current entity instance

//    public virtual void Update()
//    {
//        string sql = string.Format(sqlUpdate, PrepareUpdateColumns());

//        OnUpdatingAll(ref sql);
//        OnUpdating(ref sql);
//        db.Update(sql, Take());
//        OnUpdated(EventArgs.Empty);
//        OnUpdated();
//        OnUpdatedAll();
//    }

//    // async:  updates current entity instance

//    public virtual async System.Threading.Tasks.Task UpdateAsync()
//    {
//        string sql = string.Format(sqlUpdate, PrepareUpdateColumns());

//        OnUpdatingAll(ref sql);
//        OnUpdating(ref sql);
//        await db.UpdateAsync(sql, Take()).ConfigureAwait(false);
//        OnUpdated(EventArgs.Empty);
//        OnUpdated();
//        OnUpdatedAll();
//    }

//    // deletes current entity instance

//    public virtual void Delete()
//    {
//        string sql = sqlDelete;

//        OnDeletingAll(ref sql);
//        OnDeleting(ref sql);
//        db.Delete(sql, Take());
//        OnDeleted(EventArgs.Empty);
//        OnDeleted();
//        OnDeletedAll();
//    }

//    // async: deletes current entity instance

//    public virtual async System.Threading.Tasks.Task DeleteAsync()
//    {
//        string sql = sqlDelete;

//        OnDeletingAll(ref sql);
//        OnDeleting(ref sql);
//        await db.DeleteAsync(sql, Take()).ConfigureAwait(false);
//        OnDeleted(EventArgs.Empty);
//        OnDeleted();
//        OnDeletedAll();
//    }

//    #region Validation

//    protected virtual void Validate() { }

//    // executes validations and returns a boolean result

//    public bool IsValid
//    {
//        get
//        {
//            Errors.Clear();
//            Validate();
//            return Errors.Count == 0;
//        }
//    }

//    public Dictionary<string, string> Errors = new Dictionary<string, string>();

//    #endregion

//    #region Transacted actions

//    // inserts current entity instance as part of an ongoing transaction

//    public virtual void TransactedInsert(Db db)
//    {
//        string sql = sqlInsert;

//        OnInsertingAll(ref sql);
//        OnInserting(ref sql);
//        this[keyName] = db.TransactedInsert(sql, Take());
//        OnInserted();
//        OnInsertedAll();
//    }

//    // async: inserts current entity instance as part of an ongoing transaction

//    public virtual async System.Threading.Tasks.Task TransactedInsertAsync(Db db)
//    {
//        string sql = sqlInsert;

//        OnInsertingAll(ref sql);
//        OnInserting(ref sql);
//        this[keyName] = await db.TransactedInsertAsync(sql, Take()).ConfigureAwait(false);
//        OnInserted();
//        OnInsertedAll();
//    }

//    // updates current entity instance as part of an ongoing transaction

//    public virtual void TransactedUpdate(Db db)
//    {
//        string sql = string.Format(sqlUpdate, PrepareUpdateColumns());

//        OnUpdatingAll(ref sql);
//        OnUpdating(ref sql);
//        db.TransactedUpdate(sql, Take());
//        OnUpdated();
//        OnUpdatedAll();
//    }

//    // async: updates current entity instance as part of an ongoing transaction

//    public virtual async System.Threading.Tasks.Task TransactedUpdateAsync(Db db)
//    {
//        string sql = string.Format(sqlUpdate, PrepareUpdateColumns());

//        OnUpdatingAll(ref sql);
//        OnUpdating(ref sql);
//        await db.TransactedUpdateAsync(sql, Take()).ConfigureAwait(false);
//        OnUpdated();
//        OnUpdatedAll();
//    }

//    // deletes current entity instance as part of an ongoing transaction

//    public virtual void TransactedDelete(Db db)
//    {
//        string sql = sqlDelete;

//        OnDeletingAll(ref sql);
//        OnDeleting(ref sql);
//        db.TransactedDelete(sql, Take());
//        OnDeleted();
//        OnDeletedAll();
//    }

//    // async: deletes current entity instance as part of an ongoing transaction

//    public virtual async System.Threading.Tasks.Task TransactedDeleteAsync(Db db)
//    {
//        string sql = sqlDelete;

//        OnDeletingAll(ref sql);
//        OnDeleting(ref sql);
//        await db.TransactedDeleteAsync(sql, Take()).ConfigureAwait(false);
//        OnDeleted();
//        OnDeletedAll();
//    }

//    #endregion

//    // holds an object property and its default value

//    private class SchemaMap
//    {
//        public PropertyInfo Prop { get; set; } = null!;
//        public object? Default { get; set; }
//        public string? DataType { get; set; }
//    }
//}

//// entity extension

//static class EntityExtensions
//{
//    public static string TrimEnd(this StringBuilder sb)
//    {
//        return sb.ToString().TrimEnd(new char[] { ',', ' ', '|' });
//    }

//    public static string Bracket(this string item)
//    {
//        return "[" + item + "]";
//    }

//    public static bool IsNullOrEmpty(this string s)
//    {
//        return string.IsNullOrEmpty(s);
//    }

//    public static object? OrNow(this object? value)
//    {
//        if (value != null)
//        {
//            string val = value.ToString()!;

//            if (val == "getdate()") return DateTime.Now;
//            if (val == "gettime()") return DateTime.Now.TimeOfDay;

//            if (val == "getutcdate()") return DateTime.UtcNow;
//            if (val == "getutcoffset()") return DateTimeOffset.UtcNow;

//            if (val == "newid()") return Guid.NewGuid();
//            if (val == "newsequentialid()") return CreateSequentialGuid();
//        }

//        return value;
//    }

//    // creates newsequentialid() guids just like SQL Server

//    private class NativeMethods
//    {
//        [DllImport("rpcrt4.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        public static extern int UuidCreateSequential(ref Guid id);
//    }

//    public static Guid CreateSequentialGuid()
//    {
//        try
//        {
//            var guid = default(Guid);
//            NativeMethods.UuidCreateSequential(ref guid);

//            var bytes = guid.ToByteArray();

//            // reverse bits. Just like SQL Server does. 
//            Array.Reverse(bytes, 0, 4);
//            Array.Reverse(bytes, 4, 2);
//            Array.Reverse(bytes, 6, 2);

//            return new Guid(bytes);
//        }
//        catch
//        {
//            return Guid.NewGuid(); // alas
//        }
//    }
//}

//// reflection utility

//public class ReflectionUtility
//{
//    internal static Func<object, object> GetGetter(PropertyInfo property)
//    {
//        var method = property.GetGetMethod(true)!;
//        var genericHelper = typeof(ReflectionUtility).GetMethod("GetGetterHelper",
//            BindingFlags.Static | BindingFlags.NonPublic)!;

//        var constructedHelper = genericHelper.MakeGenericMethod(method.DeclaringType!, method.ReturnType);

//        var ret = constructedHelper.Invoke(null, new object[] { method });
//        return (Func<object, object>)ret!;
//    }

//    static Func<object, object> GetGetterHelper<TTarget, TResult>(MethodInfo method) where TTarget : class
//    {
//        // convert slow MethodInfo to a fast, strongly typed, open delegate
//        var func = (Func<TTarget, TResult>)Delegate.CreateDelegate(typeof(Func<TTarget, TResult>), method);
//        return (object target) => (TResult)func((TTarget)target)!;
//    }
//}