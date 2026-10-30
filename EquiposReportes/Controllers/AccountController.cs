using EquiposReportes.Models;
using EquiposReportes.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposReportes.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;
    public AccountController(AuthService auth) => _auth = auth;

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, rol, mensaje) = await _auth.LoginAsync(model.Username, model.Password);
        if (!ok) { ModelState.AddModelError("", mensaje); return View(model); }

        if (rol != "Admin")
        {
            ModelState.AddModelError("", "Acceso denegado: solo el rol Admin puede ver los reportes.");
            return View(model);
        }

        HttpContext.Session.SetString("Rol", rol);
        HttpContext.Session.SetString("Username", model.Username);
        return RedirectToAction("Index", "Reportes");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
