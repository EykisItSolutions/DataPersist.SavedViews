using Microsoft.AspNetCore.Http;

namespace DataPersist.SavedViews
{
    // ** Service locator pattern.
  
    public static class ServiceLocator
    {
        private static IHttpContextAccessor _httpContextAccessor = null!;

        public static void Register(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static T Resolve<T>()
        {
            var service = _httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(T));
            if (service != null)
                return (T)service;
            else
                throw new Exception($"Unable to resolve {typeof(T)} in ServiceLocator");
        }
    }


}
