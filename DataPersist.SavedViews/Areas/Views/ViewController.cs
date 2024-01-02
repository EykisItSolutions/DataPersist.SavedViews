using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Views;

[Route("views")]
public class ViewController : Controller
{
    #region Pages

    [HttpGet("edit/{id?}")]
    public async Task<IActionResult> Edit(int id) => await new Edit { Id = id }.GetAsync();

    [HttpPost("edit/{id?}")]
    public async Task<IActionResult> Edit(Edit model) => await model.PostAsync();

    [HttpPost("delete"), AjaxOnly]
    public async Task<IActionResult> Delete(Delete model) => await model.PostAsync();

    #endregion
}