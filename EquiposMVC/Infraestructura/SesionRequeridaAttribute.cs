using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EquiposMVC.Infrastructure;

public class SesionRequeridaAttribute : ActionFilterAttribute
{
    public string? Roles { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (string.IsNullOrEmpty(session.GetString("Token")))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", new { expirada = true });
            return;
        }

        if (!string.IsNullOrEmpty(Roles) && !Roles.Split(',').Contains(session.GetString("Rol")))
            context.Result = new RedirectToActionResult("AccesoDenegado", "Home", null);
    }
}
