using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Instructors;

// ** Action Model Design Pattern

public class Delete : BaseModel
{
    #region Data

    public int Id { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> PostAsync()
    {
        var instructor = await _db.Instructors.SingleAsync(c => c.Id == Id);

        // ** Soft delete pattern (handled by UniversityContext -- records are not actually removed)

        _db.Instructors.Remove(instructor);
        
        await _db.SaveChangesAsync();

        await SettleDeleteAsync(instructor);

        Success = "Deleted";

        return Json(true);
    }

    #endregion

    #region Helpers

    private async Task SettleDeleteAsync(Instructor instructor)
    {
        // ** Cache pattern

        _cache.Instructors.Remove(instructor.Id);

        // ** Search pattern

        _search.DeleteInstructor(instructor);

        // ** Rollup pattern

        await _rollup.InstructorAsync(instructor);
    }

    #endregion
}