using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Enrollments;

// ** Action Model Design Pattern

public class Delete : BaseModel
{
    #region Data

    public int Id { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> PostAsync()
    {
        var enrollment = await _db.Enrollments.SingleAsync(c => c.Id == Id);

        // ** History pattern

        await _historyService.SaveAsync(Id, enrollment.GetType(), enrollment.EnrollmentNumber, "DELETE");

        _db.Enrollments.Remove(enrollment);

        await _db.SaveChangesAsync();

        await SettleDeleteAsync(enrollment);

        Success = "Deleted";

        return Json(true);
    }

    #endregion

    #region Helpers

    private async Task SettleDeleteAsync(Enrollment enrollment)
    {
        // ** Search pattern

        _search.DeleteEnrollment(enrollment);

        // ** Rollup pattern

        await _rollup.EnrollmentAsync(enrollment);
    }


    #endregion
}