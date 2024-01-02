using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Instructors;

[Menu("Instructors")]
[Route("instructors")]
public class InstructorController : Controller
{
    #region Pages

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

