using Microsoft.AspNetCore.Mvc.Filters;

namespace DataPersist.SavedViews;

// Captures unhandled exceptions in the application

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger _logger;

    public GlobalExceptionFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GlobalExceptionFilter>();
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(0, context.Exception, "Global Exception");
    }
}
