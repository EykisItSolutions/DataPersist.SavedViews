using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataPersist.SavedViews.Areas.Admin;

public class Activity : PagedModel<_Activity>
{
    #region Data

    public string Count { get; set; } = null!; // # of rows to delete

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var query = _db.ActivityLogs.OrderByDescending(a => a.Id)
                       .AsNoTracking().AsQueryable();

        TotalRows = await query.CountAsync();
        var items = await query.Skip(Skip).Take(Take).ToListAsync();

        _mapper.Map(items, Items);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        if (Count == "0")
        {
            Failure = "Select # of records to delete";

            return LocalRedirect("/admin/activity");
        }

        if (Count == "All")
        {
            await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [ActivityLog]");
        }
        else if (Count.IsNumeric())
        {
            var sql = $"DELETE FROM [ActivityLog] WHERE Id IN (SELECT TOP {Count} Id FROM [ActivityLog] ORDER BY Id DESC)";
            await _db.Database.ExecuteSqlRawAsync(sql);
        }

        Success = $"{Count} records deleted";

        return LocalRedirect("/admin/activity");
    }

    #endregion

    #region Mapping

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Domain.ActivityLog, _Activity>()
              .ForMember(dest => dest.LogDate, opt => opt.MapFrom(src => src.LogDate.ToDate()))
              .ForMember(dest => dest.User, opt => opt.MapFrom(src => (src.UserId == null || src.UserId == 0) ? "" :_cache.Users[(int)src.UserId!].FullName));
        }
    }

    #endregion
}

public class _Activity
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? User { get; set; }
    public string LogDate { get; set; } = null!;
    public string Activity { get; set; } = null!;
    public string? IpAddress { get; set; }
    public string? Result { get; set; }
}
