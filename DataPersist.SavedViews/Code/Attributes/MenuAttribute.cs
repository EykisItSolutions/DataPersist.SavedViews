using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DataPersist.SavedViews;

// Specifies the currently active menu

public class MenuAttribute : ActionFilterAttribute
{
    private readonly string _menu = null!;

    public MenuAttribute(string menu)
    {
        _menu = menu;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (filterContext.Controller is Controller controller)
            controller.ViewBag.Menu = _menu;

        base.OnActionExecuting(filterContext);
    }
}
