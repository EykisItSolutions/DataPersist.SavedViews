using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataPersist.SavedViews.Areas.Views;

// ** Action Model Design Pattern

public class Delete : BaseModel
{
    #region Data

    public int Id { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> PostAsync()
    {
        var view = await _db.Views.SingleAsync(c => c.Id == Id);
        var viewFilters = await _db.ViewFilters.Where(c => c.ViewId == Id).ToListAsync();
        var viewSorts = await _db.ViewSorts.Where(c => c.ViewId == Id).ToListAsync();

        // ** Unit for work pattern

        using var transaction = _db.Database.BeginTransaction();

        try
        {
            foreach (var viewSort in viewSorts)
                _db.ViewSorts.Remove(viewSort);

            foreach (var viewFilter in viewFilters)
                _db.ViewFilters.Remove(viewFilter);

            _db.Views.Remove(view);

            _db.SaveChanges();

            transaction.Commit();

            Success = "Deleted";

            return Json(true);

        }
        catch // (Exception ex)
        {
            transaction.Rollback();

            Success = "Delete Failed";
            
            return Json(false);
        }
    }

    #endregion
}
