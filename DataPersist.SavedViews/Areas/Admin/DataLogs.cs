using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace DataPersist.SavedViews.Areas.Admin;

public class DataLogs : PagedModel<_DataLog>
{
    #region Data

    public int Id { get; set; }

    #endregion

    #region Handlers

    public override async Task<IActionResult> GetAsync()
    {
        var query = _db.DataLogs.OrderByDescending(h => h.Id)
                                .AsNoTracking().AsQueryable();
            
        TotalRows = await query.CountAsync();
        var items = await query.Skip(Skip).Take(Take).ToListAsync();

        _mapper.Map(items, Items);

        return View(this);
    }

    public override async Task<IActionResult> PostAsync()
    {
        var datalog = await _db.DataLogs.SingleAsync(d => d.Id == Id);
        
        var sql = "UPDATE [{0}] SET {1} WHERE Id = @whatid";

        sql = string.Format(sql, datalog.What, "[" + datalog.Column + "] = " + datalog.OldValue);

        // ** Unit of Work pattern

        using var transaction = _db.Database.BeginTransaction();

        try
        {
            var parm = new SqlParameter("@whatid", datalog.WhatId);
            
            await _db.Database.ExecuteSqlRawAsync(sql, parm);
            
            _db.DataLogs.Remove(datalog);

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            Success = "Reversed";
        }
        catch // (Exception ex)
        {
            await transaction.RollbackAsync();
            
            Failure = "Reversal unsuccessful";
        }

        return LocalRedirect("/admin/datalogs");
    }
   
    #endregion

    #region Mapping

    private class MapperProfile : BaseProfile
    {
        public MapperProfile()
        {
            CreateMap<Domain.DataLog, _DataLog>()
              .ForMember(dest => dest.LogDate, opt => opt.MapFrom(src => src.LogDate.ToDate()))
              .ForMember(dest => dest.User, opt => opt.MapFrom(src => _cache.Users[(int)src.UserId!].FullName));
        }
    }

    #endregion
}

public class _DataLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string User { get; set; } = null!;
    public string LogDate { get; set; } = null!;
    public int WhatId { get; set; }
    public string What { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Column { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}