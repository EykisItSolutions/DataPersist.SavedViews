using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Courses;

// ** Action Model Design Pattern

public class List : PagedModel<Detail>
{
    #region Data

    public List() { Sort = "CourseNumber"; }

    public string? Title { get; set; }
    public int? InstructorId { get; set; }
    public int? DepartmentId { get; set; }
    
    public string? FeeFrom { get; set; }
    public string? FeeThru { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // ** Soft delete pattern is automatically used for Courses

        var query = BuildQuery();

        // ** Pagination pattern

        TotalRows = await query.CountAsync();
        var items = await query.Skip(Skip).Take(Take).ToListAsync();

        _mapper.Map(items, Items);

        return View(this);
    }

    #endregion

    #region Helpers

    protected IQueryable<Course> BuildQuery()
    {
        var query = _db.Courses.AsNoTracking().AsQueryable();

        // Filters

        if (AdvancedFilter)
        {
            if (Title != null)
            {
                query = query.Where(c => c.Title.Contains(Title));
            }

            if (DepartmentId != null)
            {
                query = query.Where(c => c.DepartmentId == DepartmentId);
            }

            if (InstructorId != null)
            {
                query = query.Where(c => c.InstructorId == InstructorId);
            }

            if (FeeFrom != null && decimal.TryParse(FeeFrom, out decimal feeFrom))
            {
                query = query.Where(c => c.Fee >= feeFrom);
            }

            if (FeeThru != null && decimal.TryParse(FeeThru, out decimal feeThru))
            {
                query = query.Where(c => c.Fee <= feeThru);
            }
        }
        else
        {
            query = Filter switch
            {
                1 => query.Where(c => _viewedService.GetIds("Course").Contains(c.Id)),
                2 => query.Where(c => c.TotalClasses > 1),
                3 => query.Where(c => c.InstructorId == null),
                //3 => query.Where(c => c.OwnerId == _currentUser.Id),
                //4 => query.Where(c => c.OwnerId == _currentUser.Id && c.IsPrivate == true),
                //5 => query.Where(c => c.Stage == "Closed Won"),
                _ => query
            };
        }

        // Sorting

        query = Sort switch
        {
            "CourseNumber" => query.OrderBy(c => c.CourseNumber),
            "-CourseNumber" => query.OrderByDescending(c => c.CourseNumber),
            "Title" => query.OrderBy(c => c.Title),
            "-Title" => query.OrderByDescending(c => c.Title),
            "Department" => query.OrderBy(c => c.Department),
            "-Department" => query.OrderByDescending(c => c.Department),
            "Instructor" => query.OrderBy(c => c.Instructor),
            "-Instructor" => query.OrderByDescending(c => c.Instructor),
            "Fee" => query.OrderBy(c => c.Fee),
            "-Fee" => query.OrderByDescending(c => c.Fee),
            "TotalClasses" => query.OrderBy(c => c.TotalClasses),
            "-TotalClasses" => query.OrderByDescending(c => c.TotalClasses),
            _ => query.OrderBy(c => c.Id),
        };


        return query;
    }

    #endregion

    #region Mapping

    // Course mappings are defined in Detail.cs

    #endregion

}