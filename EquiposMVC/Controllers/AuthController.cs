using EquiposMVC.Infrastructure;
using EquiposMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

public class AuthController : Controller
{
    private readonly ApiClient _api;
    public AuthController(ApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(bool expirada = false)
    {
        if (expirada) ViewBag.Aviso = "Tu sesión expiró o no has iniciado sesión. Inicia sesión de nuevo.";
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var r = await _api.PostAsync<LoginResponse>("api/Auth/login", model);
            HttpContext.Session.SetString("Token", r!.Token);
            HttpContext.Session.SetString("Rol", r.Rol);
            HttpContext.Session.SetString("Username", r.Username);
            return RedirectToAction("Index", "Equipos");
        }
        catch (ApiException ex) when (ex.StatusCode == 401)
        {
            ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            return View(model);
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
