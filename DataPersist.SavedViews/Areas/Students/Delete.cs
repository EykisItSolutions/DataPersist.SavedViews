using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Students;

// ** Action Model Design Pattern

public class Delete : BaseModel
{
    #region Data

    public int Id { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> PostAsync()
    {
        var student = await _db.Students.SingleAsync(c => c.Id == Id);

        // ** History pattern

        await _historyService.SaveAsync(Id, student.GetType(), student.FullName, "DELETE");

        _db.Students.Remove(student);

        await _db.SaveChangesAsync();

        SettleDelete(student);

        Success = "Deleted";

        return Json(true);
    }

    #endregion

    #region Helpers

    private void SettleDelete(Student student)
    {
        // ** Cache pattern

        _cache.Students.Remove(student.Id);
        
        // ** Search pattern

        _search.DeleteStudent(student);
    }

    #endregion
}
