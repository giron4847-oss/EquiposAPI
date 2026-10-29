using System.ComponentModel.DataAnnotations;

namespace EquiposMVC.Models;

public class CategoriaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
}
