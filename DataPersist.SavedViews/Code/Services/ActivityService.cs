using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews;

#region Interface

public interface IActivityService
{
    Task SaveAsync(string activity, bool success = true);
}
#endregion

public class ActivityService : IActivityService
{
    #region Dependency Injection

    private readonly UniversityContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActivityService(UniversityContext db, ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion

    #region Handlers

    public async Task SaveAsync(string activity, bool success = true)
    {
        var log = new ActivityLog()
        {
            UserId = _currentUser.Id,
            Activity = activity,
            IpAddress = _httpContextAccessor.IpAddress(),
            LogDate = DateTime.Now,
            Result = success ? "Success" : "Failed"
        };

        _db.ActivityLogs.Add(log);

        await _db.SaveChangesAsync();
    }

    #endregion
}