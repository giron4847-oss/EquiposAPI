using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EquiposMVC.Models;

public class CategoriaItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class EquipoListItem
{
    public int Id { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
}

public class EquipoDetalle : EquipoListItem
{
    public string? Descripcion { get; set; }
    public string RowVersion { get; set; } = string.Empty;
}

public class EquipoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código de inventario es obligatorio")]
    [StringLength(50)]
    public string CodigoInventario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría")]
    public int CategoriaId { get; set; }

    public string? Estado { get; set; }
    public string? RowVersion { get; set; }
    public SelectList? Categorias { get; set; }
}

public class EquiposIndexViewModel
{
    public List<EquipoListItem> Items { get; set; } = new();
    public List<CategoriaItem> Categorias { get; set; } = new();
    public string? Texto { get; set; }
    public int? CategoriaId { get; set; }
    public string? Estado { get; set; }
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
    public string Rol { get; set; } = string.Empty;
}
