using EquiposMVC.Infrastructure;
using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquiposMVC.Controllers;

[SesionRequerida(Roles = "Admin")]
public class CategoriasController : Controller
{
    private readonly CategoriasService _categorias;

    public CategoriasController(CategoriasService categorias) => _categorias = categorias;

    public async Task<IActionResult> Index()
    {
        return View(await _categorias.ListarAsync());
    }

    [HttpGet]
    public IActionResult Create() => View(new CategoriaFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(CategoriaFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _categorias.CrearAsync(model.Nombre);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.StatusCode is 400 or 409)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var c = await _categorias.ObtenerAsync(id);
            if (c == null) return NotFound();
            return View(new CategoriaFormViewModel { Id = c.Id, Nombre = c.Nombre });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoriaFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            await _categorias.ActualizarAsync(model.Id, model.Nombre);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.StatusCode is 400 or 404 or 409)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var c = await _categorias.ObtenerAsync(id);
            if (c == null) return NotFound();
            return View(c);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _categorias.EliminarAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            ModelState.AddModelError("", ex.Message);
            var c = await _categorias.ObtenerAsync(id);
            return View("Delete", c);
        }
    }
}
