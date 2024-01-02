using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DataPersist.SavedViews.Areas.Home;

public class CatchAll : BaseModel
{
    #region Data

    public string? Url { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        try 
        { 
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            
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

