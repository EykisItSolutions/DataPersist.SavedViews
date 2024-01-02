using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SqlClient;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews;

// General purpose cache. 

#region Interface

public interface ICache
{
    Dictionary<int, User> Users { get; }
    Dictionary<int, Student> Students { get; }
    Dictionary<int, Department> Departments { get; }
    Dictionary<int, Country> Countries { get; }
    Dictionary<int, Course> Courses { get; }
    Dictionary<int, Class> Classes { get; }
    Dictionary<int, Instructor> Instructors { get; }
    
    Dictionary<string, Dictionary<string, Column>> Schema { get; }

    void ClearUsers();
    void ClearStudents();
    void ClearDepartments();
    void ClearCountries();
    void ClearCourses();
    void ClearClasses();
    void ClearInstructors();
    void ClearSchema();

    void Clear();
}

#endregion

public class Cache : ICache
{
    #region Dependency Injection 

    private readonly UniversityContext _db;
    private readonly IMemoryCache _memoryCache;

    public Cache(UniversityContext db, IMemoryCache memoryCache)
    {
        _db = db;
        _memoryCache = memoryCache;
    }

    #endregion

    #region Cache management

    private const string UsersKey = nameof(UsersKey);
    private const string StudentsKey = nameof(StudentsKey);
    private const string DepartmentsKey = nameof(DepartmentsKey);
    private const string CountriesKey = nameof(CountriesKey);
    private const string InstructorsKey = nameof(InstructorsKey);
    private const string CoursesKey = nameof(CoursesKey);
    private const string ClassesKey = nameof(ClassesKey);
    
    private const string SchemaKey = nameof(SchemaKey);

    // Keeps track of keys used
    private static readonly HashSet<string> UsedKeys = new();
    private static readonly object locker = new();

    #endregion

    #region Users

    public Dictionary<int, User> Users
    {
        get
        {
            // ** Lazy load pattern 

            if (_memoryCache.Get(UsersKey) is not Dictionary<int, User> dictionary)
            {
                lock (locker)
                {
                    dictionary = _db.Users.AsNoTracking().OrderBy(c => c.LastName).ToDictionary(c => c.Id);
                    Add(UsersKey, dictionary, DateTime.Now.AddHours(2));
                }
            }

            return dictionary;
        }
    }

    // Clear Users cache

    public void ClearUsers() => Clear(UsersKey);

    #endregion

    #region Students

    public Dictionary<int, Student> Students
    {
        get
        {
            // ** Lazy load pattern 

            if (_memoryCache.Get(StudentsKey) is not Dictionary<int, Student> dictionary)
            {
                lock (locker)
                {
                    dictionary = _db.Students.AsNoTracking().OrderBy(c => c.LastName).ToDictionary(c => c.Id);
                    Add(StudentsKey, dictionary, DateTime.Now.AddHours(2));
                }
            }

            return dictionary;
        }
    }

    // Clear Students cache

    public void ClearStudents() => Clear(StudentsKey);

    #endregion

    #region Departments

    public Dictionary<int, Department> Departments
    {
        get
        {
            // ** Lazy load pattern 

            if (_memoryCache.Get(DepartmentsKey) is not Dictionary<int, Department> dictionary)
            {
                lock (locker)
                {
                    dictionary = _db.Departments.AsNoTracking().OrderBy(c => c.Name).ToDictionary(c => c.Id);
                    Add(DepartmentsKey, dictionary, DateTime.Now.AddHours(2));
                }
            }

            return dictionary;
        }
    }

    // Clear Departments cache

    public void ClearDepartments() => Clear(DepartmentsKey);

    #endregion

    #region Countries

    public Dictionary<int, Country> Countries
    {
        get
        {
            // ** Lazy load pattern 

            if (_memoryCache.Get(CountriesKey) is not Dictionary<int, Country> dictionary)
            {
                lock (locker)
                {
                    dictionary = _db.Countries.AsNoTracking().OrderBy(c => c.Name).ToDictionary(c => c.Id);
                    Add(CountriesKey, dictionary, DateTime.Now.AddHours(2));
                }
            }

            return dictionary;
        }
    }

    // Clear countries cache

    public void ClearCountries() => Clear(CountriesKey);

    #endregion

    #region Courses

    public Dictionary<int, Course> Courses
    {
        get
        {
            // ** Lazy load pattern 

            if (_memoryCache.Get(CoursesKey) is not Dictionary<int, Course> dictionary)
            {
                lock (locker)
                {
                    dictionary = _db.Courses.AsNoTracking().OrderBy(i => i.Title).ToDictionary(i => i.Id);
                    Add(CoursesKey, dictionary, DateTime.Now.AddHours(2));
                }
            }

            return dictionary;
        }
    }

    // Clear courses cache

    public void ClearCourses() => Clear(CoursesKey);

    #endregion

    #region Classes

    public Dictionary<int, Class> Classes
    {
        get
        {
            // ** Lazy load pattern 

            if (_memoryCache.Get(ClassesKey) is not Dictionary<int, Class> dictionary)
            {
                lock (locker)
                {
                    dictionary = _db.Classes.AsNoTracking().OrderBy(i => i.ClassNumber).ToDictionary(i => i.Id);
                    Add(ClassesKey, dictionary, DateTime.Now.AddHours(2));
                }
            }

            return dictionary;
        }
    }

    // Clear classes cache

    public void ClearClasses() => Clear(ClassesKey);

    #endregion

    #region Instructors

    public Dictionary<int, Instructor> Instructors
    {
        get
        {
            // ** Lazy load pattern 

            if (_memoryCache.Get(InstructorsKey) is not Dictionary<int, Instructor> dictionary)
            {
                lock (locker)
                {
                    dictionary = _db.Instructors.AsNoTracking().OrderBy(i => i.LastName).ToDictionary(i => i.Id);
                    Add(InstructorsKey, dictionary, DateTime.Now.AddHours(2));
                }
            }

            return dictionary;
        }
    }

    // Clear instructors cache

    public void ClearInstructors() => Clear(InstructorsKey);

    #endregion

    #region Schema

    // Schema data with table and column names 

    public Dictionary<string, Dictionary<string, Column>> Schema
    {
        get
        {
            if (_memoryCache.Get(SchemaKey) is not Dictionary<string, Dictionary<string, Column>> dictionary)
            {
                // ** Lazy Load pattern

                lock (locker)
                {
                    dictionary = new();

                    var sql = @"SELECT TABLE_NAME 
                                      FROM INFORMATION_SCHEMA.TABLES
			                         WHERE TABLE_TYPE = 'BASE TABLE'
                                       AND TABLE_NAME NOT LIKE 'App%'
			                         ORDER BY TABLE_NAME";

                    var connectionString = _db.Database.GetConnectionString();
                    
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var tables = connection.Query<TableEntity>(sql).ToList();

                        foreach (var table in tables)
                        {
                            sql = @"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
			                              FROM INFORMATION_SCHEMA.COLUMNS 
			                             WHERE TABLE_NAME = '" + table!.TABLE_NAME + @"'";

                            var columns = connection.Query<ColumnEntity>(sql).ToList();

                            var cachedColumns = new Dictionary<string, Column>();

                            foreach (var column in columns)

                                cachedColumns.Add(column!.COLUMN_NAME,
                                    new Column
                                    {
                                        Name = column.COLUMN_NAME,
                                        DataType = column.DATA_TYPE,
                                        IsNullable = column.IS_NULLABLE,
                                        Default = column.COLUMN_DEFAULT
                                    });

                            dictionary.Add(table.TABLE_NAME, cachedColumns);
                        }
                    }

                    Add(SchemaKey, dictionary, DateTime.Now.AddHours(48));
                }
            }

            return dictionary;
        }
    }

    // Clear schema cache

    public void ClearSchema() => Clear(SchemaKey);

    #endregion

    #region Cache Helpers

    // clears single cache entry

    private void Clear(string key)
    {
        lock (locker)
        {
            _memoryCache.Remove(key);

            UsedKeys.Remove(key);
        }
    }

    // clears entire cache

    public void Clear()
    {
        lock (locker)
        {
            foreach (var usedKey in UsedKeys)
                _memoryCache.Remove(usedKey);

            UsedKeys.Clear();
        }
    }

    // add to cache 

    private void Add(string key, object value, DateTimeOffset expiration)
    {
        _memoryCache.Set(key, value,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiration));

        UsedKeys.Add(key);
    }

    #endregion
}

public class TableEntity
{
    public string TABLE_NAME { get; set; } = null!;
}

public class ColumnEntity
{
    public string COLUMN_NAME { get; set; } = null!;
    public string DATA_TYPE { get; set; } = null!;
    public string IS_NULLABLE { get; set; } = null!;
    public string? COLUMN_DEFAULT { get; set; }
}

public class Column
{
    public string Name { get; set; } = null!;
    public string DataType { get; set; } = null!;
    public string IsNullable { get; set; } = null!;
    public string? Default { get; set; }
}
