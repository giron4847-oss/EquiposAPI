using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EquiposAPI.Data;
using EquiposAPI.Dtos;
using EquiposAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EquiposAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly string[] RolesValidos = { "Admin", "Docente", "Estudiante" };

    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AuthController(AppDbContext context, IConfiguration config, IPasswordHasher<Usuario> hasher)
    {
        _context = context;
        _config = config;
        _hasher = hasher;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (usuario == null ||
            _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
            return Unauthorized("Usuario o contraseña incorrectos.");

        return Ok(new LoginResponseDto
        {
            Token = GenerarToken(usuario),
            Username = usuario.Username,
            Rol = usuario.Rol
        });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioDto>> Register([FromBody] RegisterDto dto)
    {
        if (!RolesValidos.Contains(dto.Rol))
            return BadRequest("El rol debe ser Admin, Docente o Estudiante.");

        if (await _context.Usuarios.AnyAsync(u => u.Username == dto.Username))
            return Conflict("El nombre de usuario ya existe.");

        var usuario = new Usuario { Username = dto.Username, Email = dto.Email, Rol = dto.Rol };
        usuario.PasswordHash = _hasher.HashPassword(usuario, dto.Password);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(new UsuarioDto { Id = usuario.Id, Username = usuario.Username, Email = usuario.Email, Rol = usuario.Rol });
    }

    private string GenerarToken(Usuario usuario)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Username),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
