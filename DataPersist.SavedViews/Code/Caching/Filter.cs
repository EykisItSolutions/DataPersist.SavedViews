using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews;

#region Interface

public interface IFilter
{
    List<SelectListItem> ViewItems(string what);
    List<SelectListItem> InstructorItems { get; }
    List<SelectListItem> CourseItems { get; }
    List<SelectListItem> ClassItems { get; }
    List<SelectListItem> EnrollmentItems { get; }
    List<SelectListItem> ActivityItems { get; }
    List<SelectListItem> ErrorItems { get; }
    List<SelectListItem> RecycleBinItems { get; }
}

#endregion

public class Filter : IFilter
{
    #region Dependency Injection

    private readonly UniversityContext _db;

    public Filter(UniversityContext db) { _db = db; }

    #endregion

    #region Filters

    public List<SelectListItem> ViewItems(string what)
    {
        var list = new List<SelectListItem>
        {
            new(value: "0", text: "All Students", selected: true),
            new(value: "1", text: "Recently Viewed")
        };

        var views = _db.Views.AsNoTracking()
                              .Where(v => v.What == what)
                              .OrderBy(v => v.Id)
                              .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                              .ToList();

        if (views.Any()) list.AddRange(views);

        return list;
    }

    public List<SelectListItem> InstructorItems =>
            new()
            {
                new(value: "0", text: "All Instructors", selected: true),
                new(value: "1", text: "Recently Viewed"),
                new(value: "2", text: "Full-time"),
                new(value: "3", text: "Hired after 2011"),
                new(value: "4", text: "More than 2 courses")
            };

    public List<SelectListItem> CourseItems =>
            new()
            {
                new(value: "0", text: "All Courses", selected: true),
                new(value: "1", text: "Recently Viewed"),
                new(value: "2", text: "More than 1 class"),
                new(value: "3", text: "Without Instructor")
            };


    public List<SelectListItem> ClassItems =>
            new()
            {
                new(value: "0", text: "All Classes", selected: true),
                new(value: "1", text: "Recently Viewed"),
                new(value: "2", text: "Max Students > 15"),
                new(value: "3", text: "Enrolled Students > 2")
            };


    public List<SelectListItem> EnrollmentItems =>
            new()
            {
                new(value: "0", text: "All Enrollments", selected: true),
                new(value: "1", text: "Recently Viewed"),
                new(value: "2", text: "Unpaid Enrollments"),
                new(value: "3", text: "Fully Paid Enrollments")
            };

    public List<SelectListItem> ActivityItems =>
           new()
           {
               new(value: "0", text: "-- None --", selected: true),
               new(value: "5", text: "Delete 5 activities"),
               new(value: "10", text: "Delete 10 activities"),
               new(value: "25", text: "Delete 25 activities"),
               new(value: "100", text: "Delete 100 activities"),
               new(value: "500", text: "Delete 500 activities"),
               new(value: "All", text: "Delete All activities")
           };

    public List<SelectListItem> ErrorItems =>
            new()
            {
                new(value: "0", text: "-- None --", selected: true),
                new(value: "5", text: "Delete 5 errors"),
                new(value: "10", text: "Delete 10 errors"),
                new(value: "25", text: "Delete 25 errors"),
                new(value: "100", text: "Delete 100 errors"),
                new(value: "500", text: "Delete 500 errors"),
                new(value: "All", text: "Delete All errors")
            };

    public List<SelectListItem> RecycleBinItems =>
           new()
           {
               new(value: "All", text: "All", selected: true),
               new(value: "Course", text: "Course"),
               new(value: "Instructor", text: "Instructor")
           };

    #endregion
}