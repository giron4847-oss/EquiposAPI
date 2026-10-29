using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

public class HomeController : Controller
{
    public IActionResult AccesoDenegado() => View();
    public IActionResult ErrorGenerico() => View();
    public IActionResult Error() => View("ErrorGenerico");
}