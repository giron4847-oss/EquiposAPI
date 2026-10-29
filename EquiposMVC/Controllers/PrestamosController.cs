using EquiposMVC.Infrastructure;
using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EquiposMVC.Controllers;

[SesionRequerida]
public class PrestamosController : Controller
{
    private readonly PrestamosService _prestamos;
    private readonly EquiposService _equipos;

    public PrestamosController(PrestamosService prestamos, EquiposService equipos)
    {
        _prestamos = prestamos;
        _equipos = equipos;
    }

    public async Task<IActionResult> Index(bool soloVencidos = false)
    {
        var vm = new PrestamosIndexViewModel
        {
            Items = await _prestamos.ListarAsync(soloVencidos),
            SoloVencidos = soloVencidos,
            Rol = HttpContext.Session.GetString("Rol") ?? ""
        };
        return View(vm);
    }

    [HttpGet, SesionRequerida(Roles = "Admin,Docente")]
    public async Task<IActionResult> Create()
    {
        var vm = new PrestamoFormViewModel();
        await CargarEquipos(vm);
        return View(vm);
    }

    [HttpPost, SesionRequerida(Roles = "Admin,Docente")]
    public async Task<IActionResult> Create(PrestamoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarEquipos(model);
            return View(model);
        }

        try
        {
            await _prestamos.CrearAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.StatusCode is 400 or 404 or 409)
        {
            ModelState.AddModelError("", ex.Message);
            await CargarEquipos(model);
            return View(model);
        }
    }

    [HttpPost, SesionRequerida(Roles = "Admin,Docente")]
    public async Task<IActionResult> Devolver(int id)
    {
        try
        {
            await _prestamos.DevolverAsync(id);
        }
        catch (ApiException ex) when (ex.StatusCode is 404 or 409)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarEquipos(PrestamoFormViewModel vm)
    {
        var disponibles = await _equipos.ListarAsync(null, null, "Disponible");
        vm.Equipos = new SelectList(
            disponibles.Select(e => new { e.Id, Texto = $"{e.CodigoInventario} - {e.Nombre}" }),
            "Id", "Texto", vm.EquipoId);
    }
}