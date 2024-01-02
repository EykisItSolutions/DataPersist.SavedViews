using Microsoft.AspNetCore.Mvc;
namespace DataPersist.SavedViews.Areas.Admin;

public class Database : BaseModel
{
    #region Data

    public string? Operation { get; set; }

    #endregion

    #region Handlers
   
    public override async Task<IActionResult> PostAsync()
    {
        // **Rollup Column pattern
        // ** Search pattern

        if (Operation == "rollup")
        {
            await _rollup.AllAsync();

            await _activityService.SaveAsync("Ran database rollup operation");
        }
        else
        {
            _search.ReIndexAll();

            await _activityService.SaveAsync("Ran Lucene re-index operation");
        }

        Success = "Completed";

        return View(this);
    }

    #endregion
}

