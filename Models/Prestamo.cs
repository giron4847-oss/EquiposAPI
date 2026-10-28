namespace EquiposAPI.Models;

public class Prestamo
{
    public int Id { get; set; }

    public int EquipoId { get; set; }
    public Equipo? Equipo { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaDevolucion { get; set; }   // null mientras no se devuelva
    public string? Observaciones { get; set; }
}
