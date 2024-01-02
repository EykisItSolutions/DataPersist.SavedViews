using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Home;

public class HomeController : Controller
{
    #region Pages

    [HttpGet("")]
    public async Task<IActionResult> Landing(Landing model) => await model.GetAsync();

    [Menu("Patterns")]
    [HttpGet("patterns")]
    public async Task<IActionResult> Patterns(Patterns model) => await model.GetAsync();

    [HttpGet("error")]
    public async Task<IActionResult> Error(Error model) => await model.GetAsync();

    [HttpGet("{*url}", Order = 99999)]
    public async Task<IActionResult> CatchAll(CatchAll model) => await model.GetAsync();

    #endregion
}