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
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CategoriasController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoriaDto>>> GetAll()
    {
        var categorias = await _context.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
        return Ok(_mapper.Map<List<CategoriaDto>>(categorias));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDto>> GetById(int id)
    {
        var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null) return NotFound();
        return Ok(_mapper.Map<CategoriaDto>(categoria));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CategoriaCreateDto dto)
    {
        if (await _context.Categorias.AnyAsync(c => c.Nombre == dto.Nombre))
            return Conflict("Ya existe una categoría con ese nombre.");

        var categoria = _mapper.Map<Categoria>(dto);
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, _mapper.Map<CategoriaDto>(categoria));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoriaUpdateDto dto)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null) return NotFound();

        if (await _context.Categorias.AnyAsync(c => c.Nombre == dto.Nombre && c.Id != id))
            return Conflict("Ya existe una categoría con ese nombre.");

        _mapper.Map(dto, categoria);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (categoria == null) return NotFound();

        if (await _context.Equipos.AnyAsync(e => e.CategoriaId == id))
            return Conflict("No se puede eliminar una categoría que tiene equipos.");

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
