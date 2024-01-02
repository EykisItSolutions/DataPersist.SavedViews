using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Home;

public class Error : BaseModel
{
    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        try
        {
            await Task.Yield();
            return View();
        }
        catch
        {
            return Redirect("/error.htm");
        }
    }

    #endregion
}

