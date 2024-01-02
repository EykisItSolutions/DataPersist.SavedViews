using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Classes;

public class List : PagedModel<Detail>
{
    #region Data

    public List() { Sort = "ClassNumber"; }

    public string? ClassNumber { get; set; }
    public string? CourseId { get; set; }
   
    public string? StartDateFrom { get; set; }
    public string? StartDateThru { get; set; }

    public string? TotalEnrollmentsFrom { get; set; }
    public string? TotalEnrollmentsThru { get; set; }

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

    protected IQueryable<Class> BuildQuery()
    {
        var query = _db.Classes.AsNoTracking().AsQueryable();

        // Filters

        if (AdvancedFilter)
        {
            if (ClassNumber != null)
            {
                query = query.Where(e => e.ClassNumber == ClassNumber);
            }

            if (CourseId != null)
            {
                query = query.Where(c => c.CourseId == int.Parse(CourseId));
            }

            if (TotalEnrollmentsFrom != null && int.TryParse(TotalEnrollmentsFrom, out int totalFrom))
            {
                query = query.Where(c => c.TotalEnrollments >= totalFrom);
            }

            if (TotalEnrollmentsThru != null && int.TryParse(TotalEnrollmentsThru, out int totalThru))
            {
                query = query.Where(c => c.TotalEnrollments <= totalThru);
            }

            if (StartDateFrom != null && DateTime.TryParse(StartDateFrom, out DateTime dateFrom))
            {
                query = query.Where(c => c.StartDate >= dateFrom);
            }

            if (StartDateThru != null && DateTime.TryParse(StartDateThru, out DateTime dateThru))
            {
                query = query.Where(c => c.StartDate <= dateThru);
            }
        }
        else
        {
            query = Filter switch
            {
                1 => query.Where(c => _viewedService.GetIds("Class").Contains(c.Id)),
                2 => query.Where(c => c.MaxEnrollments > 15),
                3 => query.Where(c => c.TotalEnrollments > 2),
                _ => query
            };
        }

        // Sorting

        query = Sort switch
        {
            "ClassNumber" => query.OrderBy(c => c.ClassNumber),
            "-ClassNumber" => query.OrderByDescending(c => c.ClassNumber),
            "Course" => query.OrderBy(c => c.Course),
            "-Course" => query.OrderByDescending(c => c.Course),
            "StartDate" => query.OrderBy(c => c.StartDate),
            "-StartDate" => query.OrderByDescending(c => c.StartDate),
            "EndDate" => query.OrderBy(c => c.EndDate),
            "-EndDate" => query.OrderByDescending(c => c.EndDate),
            "Location" => query.OrderBy(c => c.Location),
            "-Location" => query.OrderByDescending(c => c.Location),
            "MaxEnrollments" => query.OrderBy(c => c.MaxEnrollments),
            "-MaxEnrollments" => query.OrderByDescending(c => c.MaxEnrollments),
            "TotalEnrollments" => query.OrderBy(c => c.TotalEnrollments),
            "-TotalEnrollments" => query.OrderByDescending(c => c.TotalEnrollments),
            _ => query.OrderBy(c => c.Id)
        };

        return query;
    }


    #endregion

    #region Mapping

    // Class mappings are defined in Detail.cs

    #endregion
}

