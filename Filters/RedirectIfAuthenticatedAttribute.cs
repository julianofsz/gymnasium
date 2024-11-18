using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace gymnasium_academia.Filters
{
    public class RedirectIfAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
            base.OnActionExecuting(context);
        }
    }
}
