using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews.Areas.Classes;

// ** Action Model Design Pattern

public class Delete : BaseModel
{
    #region Data

    public int Id { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> PostAsync()
    {
        var clas = await _db.Classes.SingleAsync(c => c.Id == Id);

        // ** History pattern

        await _historyService.SaveAsync(Id, clas.GetType(), clas.ClassNumber, "DELETE");

        _db.Classes.Remove(clas);

        await _db.SaveChangesAsync();

        Success = "Deleted";

        SettleDelete(clas);

        return Json(true);
    }

    #endregion

    #region Helpers

    private void SettleDelete(Class clas)
    {
        // ** Search pattern

        _search.DeleteClass(clas);

        // ** Cache pattern

        _cache.Classes.Remove(clas.Id);
    }

    #endregion
}

