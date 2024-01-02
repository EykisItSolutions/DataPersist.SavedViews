using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataPersist.SavedViews.Areas.Admin;

public class Errors : PagedModel<_Error>
{
    #region Data

    public string Count { get; set; } = null!; // Number of records to delete

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var query = _db.Errors.OrderByDescending(a => a.Id)
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

            return LocalRedirect("/admin/errors");
        }

        if (Count == "All")
        {
            await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Error]");
        }
        else if (Count.IsNumeric())
        {
            var sql = $"DELETE FROM [Error] WHERE Id IN (SELECT TOP {Count} Id FROM [Error] ORDER BY Id DESC)";
            await _db.Database.ExecuteSqlRawAsync(sql);
        }

        Success = $"{Count} records deleted";

        return LocalRedirect("/admin/errors");
    }

    #endregion

    #region Mapping

    public class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Domain.Error, _Error>()
              .ForMember(dest => dest.ErrorDate, opt => opt.MapFrom(src => src.ErrorDate.ToDate()))
              .ForMember(dest => dest.User, opt => opt.MapFrom(src => (src.UserId == null || src.UserId == 0) ? "" : _cache.Users[(int)src.UserId!].FullName));
        }
    }

    #endregion
}

public class _Error
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string User { get; set; } = null!;
    public string ErrorDate { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Exception { get; set; }
    public string? IpAddress { get; set; }
    public string? Url { get; set; }
    public string? HttpReferer { get; set; }
    public string? UserAgent { get; set; }
}
