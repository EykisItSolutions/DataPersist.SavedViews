using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Enrollments;

// ** Action Model Design Pattern

public class List : PagedModel<Detail>
{
    #region Data

    public List() { Sort = "EnrollmentNumber"; }

    public string? EnrollmentNumber { get; set; }
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }
    public string? Status { get; set; }

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

    protected IQueryable<Enrollment> BuildQuery()
    {
        var query = _db.Enrollments.AsNoTracking().AsQueryable();

        // Filters

        if (AdvancedFilter)
        {
            if (EnrollmentNumber != null)
            {
                query = query.Where(e => e.EnrollmentNumber == EnrollmentNumber);
            }

            if (StudentId != null)
            {
                query = query.Where(e => e.StudentId == StudentId);
            }

            if (ClassId != null)
            {
                query = query.Where(e => e.ClassId == ClassId);
            }

            if (Status != null)
            {
                query = query.Where(e => e.Status == Status);
            }
        }
        else
        {
            query = Filter switch
            {
                1 => query.Where(s => _viewedService.GetIds("Enrollment").Contains(s.Id)),
                2 => query.Where(e => e.Status != "Paid"),
                3 => query.Where(e => e.Status == "Paid"),
                _ => query
            };
        }

        // Sorting

        query = Sort switch
        {
            "EnrollmentNumber" => query.OrderBy(e => e.EnrollmentNumber),
            "-EnrollmentNumber" => query.OrderByDescending(e => e.EnrollmentNumber),
            "Student" => query.OrderBy(e => e.Student),
            "-Student" => query.OrderByDescending(e => e.Student),
            "Course" => query.OrderBy(e => e.Course),
            "-Course" => query.OrderByDescending(e => e.Course),
            "EnrollDate" => query.OrderBy(e => e.EnrollDate),
            "-EnrollDate" => query.OrderByDescending(e => e.EnrollDate),
            "Fee" => query.OrderBy(e => e.Fee),
            "-Fee" => query.OrderByDescending(e => e.Fee),
            "AmountPaid" => query.OrderBy(e => e.AmountPaid),
            "-AmountPaid" => query.OrderByDescending(e => e.AmountPaid),
            "Status" => query.OrderBy(e => e.Status),
            "-Status" => query.OrderByDescending(e => e.Status),
            "AvgGrade" => query.OrderBy(e => e.AvgGrade),
            "-AvgGrade" => query.OrderByDescending(e => e.AvgGrade),
            "TotalQuizzes" => query.OrderBy(e => e.TotalQuizzes),
            "-TotalQuizzes" => query.OrderByDescending(e => e.TotalQuizzes),
            _ => query.OrderBy(e => e.Id),
        };

        return query;
    }

    #endregion

    #region Mapping

    // Enrollment mappings are defined in Detail.cs

    #endregion
}