using System.Security.Claims;
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
public class PrestamosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PrestamosController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    private bool EsPersonal => User.IsInRole("Admin") || User.IsInRole("Docente");
    private int UsuarioIdDelToken => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET api/prestamos?soloVencidos=true
    // Admin y Docente ven todos. El Estudiante solo los suyos (el Id sale del token, nunca de un parámetro).
    [HttpGet]
    public async Task<ActionResult<List<PrestamoDto>>> GetAll([FromQuery] bool soloVencidos = false)
    {
        var query = _context.Prestamos.AsNoTracking()
            .Include(p => p.Equipo).Include(p => p.Usuario).AsQueryable();

        if (!EsPersonal)
        {
            var miId = UsuarioIdDelToken;
            query = query.Where(p => p.UsuarioId == miId);
        }

        if (soloVencidos)
        {
            var hoy = DateTime.Today;
            query = query.Where(p => p.FechaDevolucion == null && p.FechaVencimiento < hoy);
        }

        var lista = await query.OrderByDescending(p => p.FechaPrestamo).ToListAsync();
        return Ok(_mapper.Map<List<PrestamoDto>>(lista));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PrestamoDto>> GetById(int id)
    {
        var prestamo = await _context.Prestamos.AsNoTracking()
            .Include(p => p.Equipo).Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (prestamo == null) return NotFound();
        if (!EsPersonal && prestamo.UsuarioId != UsuarioIdDelToken) return NotFound();

        return Ok(_mapper.Map<PrestamoDto>(prestamo));
    }

    // POST api/prestamos
    [HttpPost]
    [Authorize(Roles = "Admin,Docente")]
    public async Task<ActionResult<PrestamoDto>> Create([FromBody] PrestamoCreateDto dto)
    {
        var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Id == dto.EquipoId);
        if (equipo == null) return NotFound("El equipo no existe.");

        if (!await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId))
            return NotFound("El usuario no existe.");

        // solo se presta un equipo Disponible
        if (equipo.Estado != EstadoEquipo.Disponible)
            return Conflict($"El equipo no está disponible (estado actual: {equipo.Estado}).");

        // la fecha de vencimiento no es anterior a hoy ni mayor a 7 días
        var ahora = DateTime.Now;
        if (dto.FechaVencimiento.Date < ahora.Date)
            return BadRequest("La fecha de vencimiento no puede ser anterior a hoy.");
        if (dto.FechaVencimiento.Date > ahora.Date.AddDays(7))
            return BadRequest("La fecha de vencimiento no puede ser mayor a 7 días desde la fecha del préstamo.");

        var prestamo = new Prestamo
        {
            EquipoId = dto.EquipoId,
            UsuarioId = dto.UsuarioId,
            FechaPrestamo = ahora,
            FechaVencimiento = dto.FechaVencimiento,
            Observaciones = dto.Observaciones
        };

        // el estado cambia a Prestado en el mismo SaveChanges (misma transacción)
        equipo.Estado = EstadoEquipo.Prestado;
        _context.Prestamos.Add(prestamo);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("El equipo fue modificado por otro usuario. Intenta de nuevo.");
        }

        var creado = await _context.Prestamos.AsNoTracking()
            .Include(p => p.Equipo).Include(p => p.Usuario)
            .FirstAsync(p => p.Id == prestamo.Id);

        return CreatedAtAction(nameof(GetById), new { id = prestamo.Id }, _mapper.Map<PrestamoDto>(creado));
    }

    // PUT api/prestamos/5/devolucion
    [HttpPut("{id}/devolucion")]
    [Authorize(Roles = "Admin,Docente")]
    public async Task<IActionResult> Devolver(int id)
    {
        var prestamo = await _context.Prestamos.Include(p => p.Equipo).FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        if (prestamo.FechaDevolucion != null)
            return Conflict("Este préstamo ya fue devuelto.");

        // se llena FechaDevolucion y el equipo vuelve a Disponible
        prestamo.FechaDevolucion = DateTime.Now;
        prestamo.Equipo!.Estado = EstadoEquipo.Disponible;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("El equipo fue modificado por otro usuario. Intenta de nuevo.");
        }

        return NoContent();
    }
}