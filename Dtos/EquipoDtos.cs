using System.ComponentModel.DataAnnotations;
using EquiposAPI.Models;

namespace EquiposAPI.Dtos;

// Para listar
public class EquipoListDto
{
    public int Id { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public EstadoEquipo Estado { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
}

// Para ver el detalle (incluye RowVersion, que el cliente devuelve al editar)
public class EquipoDetalleDto : EquipoListDto
{
    public string? Descripcion { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

// Para crear (el Estado siempre arranca en Disponible)
public class EquipoCreateDto
{
    [Required, StringLength(50)] public string CodigoInventario { get; set; } = string.Empty;
    [Required, StringLength(150)] public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    [Range(1, int.MaxValue)] public int CategoriaId { get; set; }
}

// Para actualizar (transporta RowVersion)
public class EquipoUpdateDto
{
    [Required, StringLength(50)] public string CodigoInventario { get; set; } = string.Empty;
    [Required, StringLength(150)] public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    [Range(1, int.MaxValue)] public int CategoriaId { get; set; }
    public EstadoEquipo Estado { get; set; }
    [Required] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
