using Microsoft.EntityFrameworkCore;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews;

#region Interface

public interface IViewedService
{
    Task SaveAsync(int whatId, string whatType, string whatName);
    int[] GetIds(string whatType);
}
#endregion

public class ViewedService : IViewedService
{
    #region Dependency Injection

    private readonly UniversityContext _db;
    private readonly ICurrentUser _currentUser;

    public ViewedService(UniversityContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    #endregion

    #region Handlers

    public async Task SaveAsync(int whatId, string whatType, string whatName)
    {
        // Logs a viewed record

        if (whatId != 0)
        {
            var viewed = await _db.Vieweds.SingleOrDefaultAsync(v => v.WhatId == whatId && v.WhatType == whatType && v.UserId == _currentUser.Id);
            if (viewed != null)
            {
                viewed.ViewDate = DateTime.Now;
                _db.Vieweds.Update(viewed);
                await _db.SaveChangesAsync();
            }
            else
            {
                viewed = new Viewed
                {
                    UserId = _currentUser.Id ?? 0,
                    WhatId = whatId,
                    WhatType = whatType,
                    WhatName = whatName,
                    ViewDate = DateTime.Now
                };

                await _db.Vieweds.AddAsync(viewed);
                await _db.SaveChangesAsync();
            }
        }
    }

    public int[] GetIds(string whatType)
    {
        // Get most recently viewed items of a given type for currentuser

        var whatIds = _db.Vieweds
            .FromSqlInterpolated($"SELECT WhatId FROM Viewed WHERE UserId = {_currentUser.Id} AND WhatType = {whatType}")
            .Select(v => v.WhatId).ToArray();

        return whatIds;
    }

    #endregion
}