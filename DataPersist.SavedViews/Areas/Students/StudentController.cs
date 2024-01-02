using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Students;

[Menu("Students")]
[Route("students")]
public class StudentController : Controller
{
    #region Pages

    // ** Data Experience patterns -- list, edit, detail, delete

    [HttpGet]
    public async Task<IActionResult> List(List model) => await model.GetAsync();

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(Detail model) => await model.GetAsync();

    [HttpGet("edit/{id?}")]
    public async Task<IActionResult> Edit(int id) => await new Edit { Id = id }.GetAsync();

    [HttpPost("edit/{id?}")]
    public async Task<IActionResult> Edit(Edit model) => await model.PostAsync();

    [HttpPost("delete"), AjaxOnly]
    public async Task<IActionResult> Delete(Delete model) => await model.PostAsync();

    #endregion
}
