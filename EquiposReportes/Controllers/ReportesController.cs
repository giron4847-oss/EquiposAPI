using EquiposReportes.Data;
using EquiposReportes.Infrastructure;
using EquiposReportes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquiposReportes.Controllers;

[SoloAdmin]
public class ReportesController : Controller
{
    private readonly EquiposDbContext _db;
    public ReportesController(EquiposDbContext db) => _db = db;

    public IActionResult Index() => View();

    private Task<List<CategoriaOpcion>> CategoriasAsync() =>
        _db.Categorias.AsNoTracking().OrderBy(c => c.Nombre)
           .Select(c => new CategoriaOpcion { Id = c.Id, Nombre = c.Nombre }).ToListAsync();

    // 1) Inventario por categoria (conteo por estado). Filtro: categoria
    public async Task<IActionResult> Inventario(FiltroReporte filtro)
    {
        var datos = await _db.Equipos.AsNoTracking()
            .Where(e => filtro.CategoriaId == null || e.CategoriaId == filtro.CategoriaId)
            .GroupBy(e => e.Categoria.Nombre)
            .Select(g => new InventarioCategoriaVM
            {
                Categoria = g.Key,
                Total = g.Count(),
                Disponibles = g.Count(e => e.Estado == "Disponible"),
                Prestados = g.Count(e => e.Estado == "Prestado"),
                Mantenimiento = g.Count(e => e.Estado == "Mantenimiento")
            })
            .OrderBy(x => x.Categoria)
            .ToListAsync();

        return View(new ReporteViewModel<InventarioCategoriaVM> { Filtro = filtro, Categorias = await CategoriasAsync(), Datos = datos });
    }

    // 2) Prestamos vencidos: vencimiento < hoy y sin devolucion. Filtros: rango de vencimiento y categoria
    public async Task<IActionResult> Vencidos(FiltroReporte filtro)
    {
        var hoy = DateTime.Today;
        var q = _db.Prestamos.AsNoTracking()
            .Where(p => p.FechaDevolucion == null && p.FechaVencimiento < hoy);

        if (filtro.Desde.HasValue) q = q.Where(p => p.FechaVencimiento >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(p => p.FechaVencimiento < filtro.Hasta.Value.Date.AddDays(1));
        if (filtro.CategoriaId.HasValue) q = q.Where(p => p.Equipo.CategoriaId == filtro.CategoriaId);

        var datos = await q.OrderBy(p => p.FechaVencimiento)
            .Select(p => new PrestamoVencidoVM
            {
                Username = p.Usuario.Username,
                Codigo = p.Equipo.CodigoInventario,
                Equipo = p.Equipo.Nombre,
                FechaVencimiento = p.FechaVencimiento,
                DiasAtraso = EF.Functions.DateDiffDay(p.FechaVencimiento, hoy)
            }).ToListAsync();

        return View(new ReporteViewModel<PrestamoVencidoVM> { Filtro = filtro, Categorias = await CategoriasAsync(), Datos = datos });
    }

    // 3) Top 5 equipos mas prestados. Filtros: rango de fechas de prestamo y categoria
    public async Task<IActionResult> Top(FiltroReporte filtro)
    {
        var q = _db.Prestamos.AsNoTracking().AsQueryable();
        if (filtro.Desde.HasValue) q = q.Where(p => p.FechaPrestamo >= filtro.Desde.Value);
        if (filtro.Hasta.HasValue) q = q.Where(p => p.FechaPrestamo < filtro.Hasta.Value.Date.AddDays(1));
        if (filtro.CategoriaId.HasValue) q = q.Where(p => p.Equipo.CategoriaId == filtro.CategoriaId);

        var datos = await q
            .GroupBy(p => new { p.Equipo.CodigoInventario, p.Equipo.Nombre })
            .Select(g => new TopEquipoVM { Codigo = g.Key.CodigoInventario, Equipo = g.Key.Nombre, Prestamos = g.Count() })
            .OrderByDescending(x => x.Prestamos)
            .Take(5)
            .ToListAsync();

        return View(new ReporteViewModel<TopEquipoVM> { Filtro = filtro, Categorias = await CategoriasAsync(), Datos = datos });
    }

    // 4) Historial por usuario (busqueda por Username). Filtro extra: rango de fechas
    public async Task<IActionResult> Historial(FiltroReporte filtro)
    {
        var datos = new List<HistorialUsuarioVM>();
        if (!string.IsNullOrWhiteSpace(filtro.Username))
        {
            var q = _db.Prestamos.AsNoTracking()
                .Where(p => p.Usuario.Username.Contains(filtro.Username));
            if (filtro.Desde.HasValue) q = q.Where(p => p.FechaPrestamo >= filtro.Desde.Value);
            if (filtro.Hasta.HasValue) q = q.Where(p => p.FechaPrestamo < filtro.Hasta.Value.Date.AddDays(1));

            datos = await q.OrderByDescending(p => p.FechaPrestamo)
                .Select(p => new HistorialUsuarioVM
                {
                    Username = p.Usuario.Username,
                    Codigo = p.Equipo.CodigoInventario,
                    Equipo = p.Equipo.Nombre,
                    FechaPrestamo = p.FechaPrestamo,
                    FechaVencimiento = p.FechaVencimiento,
                    FechaDevolucion = p.FechaDevolucion
                }).ToListAsync();
        }
        return View(new ReporteViewModel<HistorialUsuarioVM> { Filtro = filtro, Categorias = new(), Datos = datos });
    }
}
