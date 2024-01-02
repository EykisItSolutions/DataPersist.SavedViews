using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Students;

// ** Action Model Design Pattern

public class List : PagedModel<Detail>
{
    #region Data

    public List() { Sort = "LastName"; }

    //  Advanced filter values

    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? BirthDayFrom { get; set; }
    public string? BirthDayThru { get; set; }
    public int? TotalEnrollments { get; set; }

    // Saved view values

    public int? ViewId { get; set; }
    public List<SelectListItem> ViewList { get; set; } = new();

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var query = BuildQuery();

        // ** Pagination pattern

        TotalRows = await query.CountAsync();
        var items = await query.Skip(Skip).Take(Take).ToListAsync();

        _mapper.Map(items, Items);

        return View(this);
    }

    #endregion

    #region Helpers

    protected IQueryable<Student> BuildQuery()
    {
        var query = _db.Students.AsNoTracking().AsQueryable();

        // Filters

        if (AdvancedFilter)
        {
            if (Name != null)
            {
                query = query.Where(s => s.FirstName.Contains(Name) || s.LastName.Contains(Name));
            }

            if (Email != null)
            {
                query = query.Where(s => s.Email.Contains(Email));
            }

            if (BirthDayFrom != null && DateTime.TryParse(BirthDayFrom, out DateTime birthDayFrom))
            {
                query = query.Where(s => s.DateOfBirth >= birthDayFrom);
            }

            if (BirthDayThru != null && DateTime.TryParse(BirthDayThru, out DateTime birthDayThru))
            {
                query = query.Where(s => s.DateOfBirth <= birthDayThru);
            }

            if (TotalEnrollments != null)
            {
                query = query.Where(s => s.TotalEnrollments == TotalEnrollments);
            }
        }
        else
        {
            if (Filter == 1)
                query = query.Where(s => _viewedService.GetIds("Student").Contains(s.Id));
            else if (Filter > 1)
                query = BuildViewQuery(); // Any of the saved views
        }

        // Sorting

        query = Sort switch
        {
            "LastName" => query.OrderBy(s => s.LastName),
            "-LastName" => query.OrderByDescending(s => s.LastName),
            "Alias" => query.OrderBy(s => s.Alias),
            "-Alias" => query.OrderByDescending(s => s.Alias),
            "Email" => query.OrderBy(s => s.Email),
            "-Email" => query.OrderByDescending(s => s.Email),
            "City" => query.OrderBy(s => s.City),
            "-City" => query.OrderByDescending(s => s.City),
            "Country" => query.OrderBy(s => s.Country),
            "-Country" => query.OrderByDescending(s => s.Country),
            "DateOfBirth" => query.OrderBy(s => s.DateOfBirth),
            "-DateOfBirth" => query.OrderByDescending(s => s.DateOfBirth),
            "TotalEnrollments" => query.OrderBy(s => s.TotalEnrollments),
            "-TotalEnrollments" => query.OrderByDescending(s => s.TotalEnrollments),
            _ => query.OrderBy(s => s.Id),
        };

        return query;
    }

    protected IQueryable<Student> BuildViewQuery()
    {
        try
        {
            ViewId = Filter = (ViewId ?? Filter); // This avoids the Post-Redirect-Get issue where prior route values are kept.

            var view = _db.Views.Single(v => v.Id == ViewId);

            var where = view.FilterClause;
            var orderBy = view.SortClause;

            var parms = view.Parms == null ?
                             Array.Empty<object>() :
                             view.Parms.Split('|').Cast<object>().ToArray();

            var sql = $"SELECT * FROM [Student] WHERE {where}"; // ORDER BY {orderBy}

            return _db.Students.FromSqlRaw(sql, parms).AsNoTracking().AsQueryable();
        }
        catch
        {
            Filter = 0;
            return _db.Students.AsNoTracking().AsQueryable();
        }
    }

    #endregion

    #region Mapping

    // Student mappings are defined in Detail.cs

    #endregion
}