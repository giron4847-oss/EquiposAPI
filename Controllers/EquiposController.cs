using AutoMapper;
using EquiposAPI.Data;
using EquiposAPI.Dtos;
using EquiposAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EquiposController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public EquiposController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET api/equipos?texto=&categoriaId=&estado=
    [HttpGet]
    public async Task<ActionResult<List<EquipoListDto>>> GetAll(
        [FromQuery] string? texto, [FromQuery] int? categoriaId, [FromQuery] EstadoEquipo? estado)
    {
        var query = _context.Equipos.AsNoTracking().Include(e => e.Categoria).AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(e => e.Nombre.Contains(texto) || e.CodigoInventario.Contains(texto));
        if (categoriaId.HasValue)
            query = query.Where(e => e.CategoriaId == categoriaId.Value);
        if (estado.HasValue)
            query = query.Where(e => e.Estado == estado.Value);

        var equipos = await query.OrderBy(e => e.Nombre).ToListAsync();
        return Ok(_mapper.Map<List<EquipoListDto>>(equipos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EquipoDetalleDto>> GetById(int id)
    {
        var equipo = await _context.Equipos.AsNoTracking()
            .Include(e => e.Categoria)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (equipo == null) return NotFound();
        return Ok(_mapper.Map<EquipoDetalleDto>(equipo));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EquipoDetalleDto>> Create([FromBody] EquipoCreateDto dto)
    {
        if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            return BadRequest("La categoría indicada no existe.");

        if (await _context.Equipos.AnyAsync(e => e.CodigoInventario == dto.CodigoInventario))
            return Conflict("Ya existe un equipo con ese código de inventario.");

        var equipo = _mapper.Map<Equipo>(dto);
        equipo.Estado = EstadoEquipo.Disponible;

        _context.Equipos.Add(equipo);
        await _context.SaveChangesAsync();

        var creado = await _context.Equipos.AsNoTracking().Include(e => e.Categoria).FirstAsync(e => e.Id == equipo.Id);
        return CreatedAtAction(nameof(GetById), new { id = equipo.Id }, _mapper.Map<EquipoDetalleDto>(creado));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] EquipoUpdateDto dto)
    {
        var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Id == id);
        if (equipo == null) return NotFound();

        if (!await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
            return BadRequest("La categoría indicada no existe.");

        if (await _context.Equipos.AnyAsync(e => e.CodigoInventario == dto.CodigoInventario && e.Id != id))
            return Conflict("Ya existe un equipo con ese código de inventario.");

        _mapper.Map(dto, equipo);

        // Concurrencia optimista: le decimos a EF con qué RowVersion editó el usuario
        _context.Entry(equipo).Property(e => e.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Otro usuario modificó este equipo mientras lo editabas. Recarga los datos e intenta de nuevo.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Id == id);
        if (equipo == null) return NotFound();

        if (await _context.Prestamos.AnyAsync(p => p.EquipoId == id))
            return Conflict("No se puede eliminar un equipo con historial de préstamos.");

        _context.Equipos.Remove(equipo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}