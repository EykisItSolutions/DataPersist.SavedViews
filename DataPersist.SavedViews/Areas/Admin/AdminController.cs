using Microsoft.AspNetCore.Mvc;

namespace DataPersist.SavedViews.Areas.Admin;

[Menu("Admin")]
[Route("admin")]
public class AdminController : Controller
{
    #region Pages

    [HttpGet]
    public async Task<IActionResult> List(List model) => await model.GetAsync();

    [HttpGet("adhoc")]
    public async Task<IActionResult> Adhoc() => await new Adhoc().GetAsync();

    [HttpPost("adhoc")]
    public async Task<IActionResult> Adhoc(Adhoc model) => await model.PostAsync();

    [HttpGet("settings")]
    public async Task<IActionResult> Settings() => await new Settings().GetAsync();

    [HttpPost("settings")]
    public async Task<IActionResult> Settings(Settings model) => await model.PostAsync();

    [HttpGet("database")]
    public async Task<IActionResult> Database() => await new Database().GetAsync();

    [HttpPost("database/{operation}")]
    public async Task<IActionResult> Database(Database model) => await model.PostAsync();

    [HttpGet("histories")]
    public async Task<IActionResult> Histories(Histories model) => await model.GetAsync();

    [HttpPost("histories/reverse")]
    public async Task<IActionResult> Histories(int id) => await new Histories { Id = id }.PostAsync();

    [HttpGet("datalogs")]
    public async Task<IActionResult> DataLogs(DataLogs model) => await model.GetAsync();

    [HttpPost("datalogs/undo/{id}")]
    public async Task<IActionResult> DataLogs(int id) => await new DataLogs { Id = id }.PostAsync();

    [HttpGet("recyclebin")]
    public async Task<IActionResult> RecycleBin(RecycleBin model) => await model.GetAsync();

    [HttpPost("recyclebin/undo/{what}/{whatid}")]
    public async Task<IActionResult> RecycleBin(string what, int whatid) => await new RecycleBin { What = what, WhatId = whatid }.PostAsync();

    [HttpGet("activity")]
    public async Task<IActionResult> Activity(Activity model) => await model.GetAsync();

    [HttpPost("activity")]
    public async Task<IActionResult> Activity(string count) => await new Activity { Count = count }.PostAsync();

    [HttpGet("errors")]
    public async Task<IActionResult> Errors(Errors model) => await model.GetAsync();

    [HttpPost("errors")]
    public async Task<IActionResult> Errors(string count) => await new Errors { Count = count }.PostAsync();

    #endregion
}