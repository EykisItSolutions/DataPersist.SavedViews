using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Auth;

public class AuthController : Controller
{
    #region Pages

    [HttpGet("login")]
    public async Task<IActionResult> Login(Login model) => await model.GetAsync();

    [HttpGet("logout")]
    public async Task<IActionResult> Logout(Logout model) => await model.GetAsync();

    #endregion
}