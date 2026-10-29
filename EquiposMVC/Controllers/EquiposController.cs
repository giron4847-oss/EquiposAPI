using EquiposMVC.Infrastructure;
using EquiposMVC.Models;
using EquiposMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EquiposMVC.Controllers;

[SesionRequerida]
public class EquiposController : Controller
{
    private const int PorPagina = 10;
    private readonly EquiposService _equipos;
    private readonly CategoriasService _categorias;

    public EquiposController(EquiposService equipos, CategoriasService categorias)
    {
        _equipos = equipos;
        _categorias = categorias;
    }

    public async Task<IActionResult> Index(string? texto, int? categoriaId, string? estado, int pagina = 1)
    {
        var rol = HttpContext.Session.GetString("Rol") ?? "";
        if (rol == "Estudiante") estado = "Disponible";   // el estudiante solo ve equipos disponibles

        var todos = await _equipos.ListarAsync(texto, categoriaId, estado);

        var totalPaginas = (int)Math.Ceiling(todos.Count / (double)PorPagina);
        if (pagina < 1) pagina = 1;
        if (totalPaginas > 0 && pagina > totalPaginas) pagina = totalPaginas;

        var vm = new EquiposIndexViewModel
        {
            Items = todos.Skip((pagina - 1) * PorPagina).Take(PorPagina).ToList(),
            Categorias = await _categorias.ListarAsync(),
            Texto = texto,
            CategoriaId = categoriaId,
            Estado = estado,
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            Rol = rol
        };
        return View(vm);
    }

    [HttpGet, SesionRequerida(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var vm = new EquipoFormViewModel();
        await CargarCategorias(vm);
        return View(vm);
    }

    [HttpPost, SesionRequerida(Roles = "Admin")]
    public async Task<IActionResult> Create(EquipoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategorias(model);
            return View(model);
        }

        try
        {
            await _equipos.CrearAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.StatusCode is 400 or 409)
        {
            ModelState.AddModelError("", ex.Message);   // mensaje de la API dentro del formulario, sin perder lo escrito
            await CargarCategorias(model);
            return View(model);
        }
    }

    [HttpGet, SesionRequerida(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var e = await _equipos.ObtenerAsync(id);
            if (e == null) return NotFound();

            var vm = new EquipoFormViewModel
            {
                Id = e.Id,
                CodigoInventario = e.CodigoInventario,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion,
                CategoriaId = e.CategoriaId,
                Estado = e.Estado,
                RowVersion = e.RowVersion
            };
            await CargarCategorias(vm);
            return View(vm);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
    }

    [HttpPost, SesionRequerida(Roles = "Admin")]
    public async Task<IActionResult> Edit(EquipoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategorias(model);
            return View(model);
        }

        try
        {
            await _equipos.ActualizarAsync(model);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.StatusCode is 400 or 404 or 409)
        {
            ModelState.AddModelError("", ex.Message);   // aquí cae el 409 de concurrencia
            await CargarCategorias(model);
            return View(model);
        }
    }

    [HttpGet, SesionRequerida(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var e = await _equipos.ObtenerAsync(id);
            if (e == null) return NotFound();
            return View(e);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
    }

    [HttpPost, ActionName("Delete"), SesionRequerida(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _equipos.EliminarAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            ModelState.AddModelError("", ex.Message);
            var e = await _equipos.ObtenerAsync(id);
            return View("Delete", e);
        }
    }

    private async Task CargarCategorias(EquipoFormViewModel vm)
    {
        vm.Categorias = new SelectList(await _categorias.ListarAsync(), "Id", "Nombre", vm.CategoriaId);
    }
}
