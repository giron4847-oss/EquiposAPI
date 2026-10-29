using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.Dtos;

public class PrestamoCreateDto
{
    [Range(1, int.MaxValue)] public int EquipoId { get; set; }
    [Range(1, int.MaxValue)] public int UsuarioId { get; set; }   // quién se lleva el equipo
    [Required] public DateTime FechaVencimiento { get; set; }
    public string? Observaciones { get; set; }
}

public class PrestamoDto
{
    public int Id { get; set; }
    public int EquipoId { get; set; }
    public string EquipoCodigoInventario { get; set; } = string.Empty;
    public string EquipoNombre { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioUsername { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string? Observaciones { get; set; }

    public bool EstaVencido => FechaDevolucion == null && FechaVencimiento.Date < DateTime.Today;
}