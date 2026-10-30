using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EquiposReportes.Infrastructure;

// Solo entra quien hizo login y tiene rol Admin
public class SoloAdminAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var rol = context.HttpContext.Session.GetString("Rol");
        if (rol != "Admin")
            context.Result = new RedirectToActionResult("Login", "Account", null);
    }
}
