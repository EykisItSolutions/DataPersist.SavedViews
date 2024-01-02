using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Courses;

// ** Action Model Design Pattern

public class Delete : BaseModel
{
    #region Data

    public int Id { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> PostAsync()
    {
        var course = await _db.Courses.SingleAsync(c => c.Id == Id);

        // ** Soft delete pattern (handled by context -- records are not actually removed)

        _db.Courses.Remove(course);

        await _db.SaveChangesAsync();

        await SettleDeleteAsync(course);

        Success = "Deleted";

        return Json(true);
    }

    #endregion

    #region Helpers

    private async Task SettleDeleteAsync(Course course)
    {
        // ** Cache pattern

        _cache.Courses.Remove(course.Id);

        // ** Search pattern

        _search.DeleteCourse(course);

        // ** Rollup pattern

        await _rollup.CourseAsync(course);
    }

    #endregion
}

