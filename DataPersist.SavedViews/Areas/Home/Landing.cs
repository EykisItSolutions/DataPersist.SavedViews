using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataPersist.SavedViews.Areas.Home;

public class Landing : BaseModel
{
    #region Static

    private static bool _reindexed = false;
    private static bool _cacheloaded = false;

    #endregion

    #region Data

    public List<Item> Items { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        Reindex();
        LoadCache();

        // Get record counts for application tables

        Items.Add(new() { Icon = "icon-people", Name = "Students", Count = await _db.Students.CountAsync() });
        Items.Add(new() { Icon = "icon-doc", Name = "Enrollments", Count = await _db.Enrollments.CountAsync() });
        Items.Add(new() { Icon = "icon-calendar", Name = "Classes", Count = await _db.Classes.CountAsync() });
        Items.Add(new() { Icon = "icon-book-open", Name = "Courses", Count = await _db.Courses.CountAsync() });
        Items.Add(new() { Icon = "icon-briefcase", Name = "Instructors", Count = await _db.Instructors.CountAsync() });
        Items.Add(new() { Icon = "icon-user", Name = "Users", Count = await _db.Users.CountAsync() });

        return View(this);
    }

    #endregion

    #region Helpers

    private void Reindex()
    {
        // Reindexes Lucene text search only once during startup

        if (!_reindexed)
        {
            var search = ServiceLocator.Resolve<ISearch>();
            search.ReIndexAll();

            _reindexed = true;
        }
    }

    private void LoadCache()
    {
        // ** Eager Load pattern

        if (!_cacheloaded)
        {
            // Loading some frequently used caches

            _ = _cache.Countries;
            _ = _cache.Departments;
            _ = _cache.Users;
            _ = _cache.Schema;

            _cacheloaded = true;
        }
    }

    public class Item
    {
        public string Icon { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Count { get; set; }
    }

    #endregion
}