using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Instructors;

// ** Action Model Design Pattern

public class List : PagedModel<Detail>
{
    #region Data

    public List() { Sort = "LastName"; }

    // Filter items

    public string? Name { get; set; }
    public string? IsFulltime { get; set; }
    public string? HireDateFrom { get; set; }
    public string? HireDateThru { get; set; }
    public int? TotalCoursesFrom { get; set; }
    public int? TotalCoursesThru { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // ** Soft delete pattern is automatically used for Instructors

        var query = BuildQuery();

        TotalRows = await query.CountAsync();
        var items = await query.Skip(Skip).Take(Take).ToListAsync();

        _mapper.Map(items, Items);

        return View(this);
    }

    #endregion

    #region Helpers

    protected IQueryable<Instructor> BuildQuery()
    {
        var query = _db.Instructors.AsNoTracking().AsQueryable();

        // Filters

        if (AdvancedFilter)
        {
            if (Name != null)
            {
                query = query.Where(s => s.FirstName.Contains(Name) || s.LastName.Contains(Name));
            }

            if (IsFulltime == "Yes")
            {
                query = query.Where(i => i.IsFulltime == true);
            }
            else if (IsFulltime == "No")
            {
                query = query.Where(i => i.IsFulltime == false);
            }

            if (HireDateFrom != null && DateTime.TryParse(HireDateFrom, out DateTime hireFrom))
            {
                query = query.Where(i => i.HireDate >= hireFrom);
            }

            if (HireDateThru != null && DateTime.TryParse(HireDateThru, out DateTime hireThru))
            {
                query = query.Where(i => i.HireDate <= hireThru);
            }

            if (TotalCoursesFrom != null)
            {
                query = query.Where(i => i.TotalCourses >= TotalCoursesFrom);
            }

            if (TotalCoursesThru != null)
            {
                query = query.Where(i => i.TotalCourses <= TotalCoursesThru);
            }
        }
        else
        {
            query = Filter switch
            {
                1 => query.Where(i => _viewedService.GetIds("Instructor").Contains(i.Id)),
                2 => query.Where(i => i.IsFulltime == true),
                3 => query.Where(i => i.HireDate > DateTime.Parse("2011/12/31")),
                4 => query.Where(i => i.TotalCourses > 2),
                _ => query
            };
        }

        // Sorting

        query = Sort switch
        {
            "LastName" => query.OrderBy(i => i.LastName),
            "-LastName" => query.OrderByDescending(i => i.LastName),
            "Email" => query.OrderBy(i => i.Email),
            "-Email" => query.OrderByDescending(i => i.Email),
            "Alias" => query.OrderBy(i => i.Alias),
            "-Alias" => query.OrderByDescending(i => i.Alias),
            "HireDate" => query.OrderBy(i => i.HireDate),
            "-HireDate" => query.OrderByDescending(i => i.HireDate),
            "IsFulltime" => query.OrderBy(i => i.IsFulltime),
            "-IsFulltime" => query.OrderByDescending(i => i.IsFulltime),
            "Salary" => query.OrderBy(i => i.Salary),
            "-Salary" => query.OrderByDescending(i => i.Salary),
            "TotalCourses" => query.OrderBy(i => i.TotalCourses),
            "-TotalCourses" => query.OrderByDescending(i => i.TotalCourses),
            _ => query.OrderBy(i => i.Id),
        };

        return query;
    }

    #endregion

    #region Mapping

    // Instructor mappings are defined in Detail.cs

    #endregion
}

