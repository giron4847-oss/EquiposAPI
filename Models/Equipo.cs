using System.ComponentModel.DataAnnotations;

namespace EquiposAPI.Models;

public class Equipo
{
    public int Id { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public EstadoEquipo Estado { get; set; } = EstadoEquipo.Disponible;

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
