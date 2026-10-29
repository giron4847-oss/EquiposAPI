using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EquiposMVC.Models;

public class PrestamoItem
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
    public bool EstaVencido { get; set; }
}

public class PrestamoFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un equipo")]
    public int EquipoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Escribe el Id del usuario")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaVencimiento { get; set; } = DateTime.Today.AddDays(7);

    public string? Observaciones { get; set; }
    public SelectList? Equipos { get; set; }
}

public class PrestamosIndexViewModel
{
    public List<PrestamoItem> Items { get; set; } = new();
    public bool SoloVencidos { get; set; }
    public string Rol { get; set; } = string.Empty;
}