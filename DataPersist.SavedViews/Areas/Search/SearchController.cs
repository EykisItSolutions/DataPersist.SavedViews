using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Search;

[Menu("Search")]
[Route("search")]
public class SearchController : Controller
{
    #region Pages

    [HttpGet]
    public async Task<IActionResult> List(List model) => await model.GetAsync();

    #endregion
}