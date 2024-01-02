using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataPersist.SavedViews.Areas.Search;

public class List : BaseModel
{
    #region Data

    public string? q { get; set; }
    public string SearchType { get; set; } = "All";


    public List<Students.Detail> Students { get; set; } = new();
    public List<Enrollments.Detail> Enrollments { get; set; } = new();
    public List<Courses.Detail> Courses { get; set; } = new();
    public List<Classes.Detail> Classes { get; set; } = new();
    public List<Instructors.Detail> Instructors { get; set; } = new();

    public int TotalStudents { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalCourses { get; set; }
    public int TotalClasses { get; set; }
    public int TotalInstructors { get; set; }


    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        if (!string.IsNullOrEmpty(q))
        {
            var results = _search.Get(q);

            // Students

            if (SearchType == "All" || SearchType == "Students")
            {
                var ids = results.Where(r => r.What == "Student").Select(r => int.Parse(r.Id)).Distinct().ToList();
                if (ids.Any())
                {
                    TotalStudents = ids.Count;

                    var query = _db.Students.AsNoTracking().AsQueryable().Where(s => ids.Contains(s.Id));
                    query = SearchType == "All" ? query.Take(5) : query;
                    
                    _mapper.Map(query.ToList(), Students);
                }
            }

            // Enrollments

            if (SearchType == "All" || SearchType == "Enrollments")
            {
                var ids = results.Where(r => r.What == "Enrollment").Select(r => int.Parse(r.Id)).Distinct().ToList();
                if (ids.Any())
                {
                    TotalEnrollments = ids.Count;

                    var query = _db.Enrollments.AsNoTracking().AsQueryable().Where(s => ids.Contains(s.Id));
                    query = SearchType == "All" ? query.Take(5) : query;

                    _mapper.Map(query.ToList(), Enrollments);
                }
            }

            // Courses

            if (SearchType == "All" || SearchType == "Courses")
            {
                var ids = results.Where(r => r.What == "Course").Select(r => int.Parse(r.Id)).Distinct().ToList();
                if (ids.Any())
                {
                    TotalCourses = ids.Count;

                    var query = _db.Courses.AsNoTracking().AsQueryable().Where(s => ids.Contains(s.Id));
                    query = SearchType == "All" ? query.Take(5) : query;

                    _mapper.Map(query.ToList(), Courses);
                }
            }

            // Classes

            if (SearchType == "All" || SearchType == "Classes")
            {
                var ids = results.Where(r => r.What == "Class").Select(r => int.Parse(r.Id)).Distinct().ToList();
                if (ids.Any())
                {
                    TotalClasses = ids.Count;

                    var query = _db.Classes.AsNoTracking().AsQueryable().Where(s => ids.Contains(s.Id));
                    query = SearchType == "All" ? query.Take(5) : query;

                    _mapper.Map(query.ToList(), Classes);
                }
            }

            // Instructors

            if (SearchType == "All" || SearchType == "Instructors")
            {
                var ids = results.Where(r => r.What == "Instructor").Select(r => int.Parse(r.Id)).Distinct().ToList();
                if (ids.Any())
                {
                    TotalInstructors = ids.Count;

                    var query = _db.Instructors.AsNoTracking().AsQueryable().Where(s => ids.Contains(s.Id));
                    query = SearchType == "All" ? query.Take(5) : query;

                    _mapper.Map(query.ToList(), Instructors);
                }
            }

            await _activityService.SaveAsync($"Searched: {SearchType} for '{q}'");

        }

        return View(this);
    }

    #endregion
}