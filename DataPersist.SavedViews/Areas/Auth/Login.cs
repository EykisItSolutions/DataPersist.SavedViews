using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Auth;

public class Login : BaseModel
{
    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        // Assume valid login. Log activity and redirect to site.

        await _activityService.SaveAsync("Login");

        return LocalRedirect("/patterns");
    }

    #endregion
}