using AutoMapper;
using DataPersist.SavedViews.Domain;

namespace DataPersist.SavedViews;

public abstract class BaseProfile : Profile
{
    // Base class to all AutoMapper Profiles

    #region Dependency Injection

    // ** Lazy Injection pattern

    private static HttpContext HttpContext => ServiceLocator.Resolve<IHttpContextAccessor>().HttpContext!;

    private ICurrentUser currentUser = null!;

    // Lifetime = Scoped
    protected ICache _cache => HttpContext.RequestServices.GetService<ICache>()!;
    protected UniversityContext _db => HttpContext.RequestServices.GetService<UniversityContext>()!;

    // Lifetime = Singleton
    protected ICurrentUser _currentUser => currentUser ??= HttpContext.RequestServices.GetService<ICurrentUser>()!;

    #endregion
}


