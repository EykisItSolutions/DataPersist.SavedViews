using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Courses;

[Menu("Courses")]
[Route("Courses")]
public class CourseController : Controller
{
    #region Pages

    [HttpGet]
    public async Task<IActionResult> List(List model) => await model.GetAsync();

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(Detail model) => await model.GetAsync();

    [HttpGet("edit/{id?}")]
    public async Task<IActionResult> Edit(int id, int instructorId) => 
        await new Edit { Id = id, InstructorId = instructorId }.GetAsync();

    [HttpPost("edit/{id?}")]
    public async Task<IActionResult> Edit(Edit model) => await model.PostAsync();

    [HttpPost("delete"), AjaxOnly]
    public async Task<IActionResult> Delete(Delete model) => await model.PostAsync();

    #endregion
}

