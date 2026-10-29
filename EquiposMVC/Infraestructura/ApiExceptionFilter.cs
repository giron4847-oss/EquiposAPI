using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EquiposMVC.Infrastructure;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ApiException ex) return;

        switch (ex.StatusCode)
        {
            case 401:
                context.HttpContext.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Auth", new { expirada = true });
                break;
            case 403:
                context.Result = new RedirectToActionResult("AccesoDenegado", "Home", null);
                break;
            case >= 500:
                context.Result = new RedirectToActionResult("ErrorGenerico", "Home", null);
                break;
            default:
                return;
        }

        context.ExceptionHandled = true;
    }
}
